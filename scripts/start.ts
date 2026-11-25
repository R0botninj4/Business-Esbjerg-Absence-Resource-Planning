import { join, resolve } from "node:path";

const root = resolve(import.meta.dir, "..");
const client = join(root, "Client");
const children: Bun.Subprocess[] = [];
let stopping = false;

function run(command: string[], cwd = root) {
    const result = Bun.spawnSync(command, { cwd, stdout: "inherit", stderr: "inherit" });
    if (result.exitCode !== 0) {
        throw new Error("Kommandoen fejlede: " + command.join(" "));
    }
}

function succeeds(command: string[]) {
    const result = Bun.spawnSync(command, { cwd: root, stdout: "ignore", stderr: "ignore" });
    return result.exitCode === 0;
}

async function responds(url: string) {
    try {
        const response = await fetch(url, { signal: AbortSignal.timeout(1000) });
        return response.ok;
    } catch {
        return false;
    }
}

async function waitFor(url: string, child: Bun.Subprocess) {
    for (let attempt = 0; attempt < 90; attempt++) {
        if (child.exitCode !== null) {
            throw new Error("Serveren stoppede under opstart. Se fejlen ovenfor.");
        }
        if (await responds(url)) {
            return;
        }
        await Bun.sleep(1000);
    }
    throw new Error("Serveren blev ikke klar: " + url);
}

function start(command: string[], cwd: string) {
    const child = Bun.spawn(command, { cwd, stdin: "inherit", stdout: "inherit", stderr: "inherit" });
    children.push(child);
    return child;
}

function stop() {
    if (stopping) {
        return;
    }
    stopping = true;
    for (const child of children) {
        if (child.exitCode !== null) {
            continue;
        }
        if (process.platform === "win32") {
            // Stop også API-processen, som dotnet run har startet.
            Bun.spawnSync(["taskkill", "/PID", String(child.pid), "/T", "/F"], { stdout: "ignore", stderr: "ignore" });
        } else {
            child.kill();
        }
    }
}

process.on("SIGINT", () => { stop(); process.exit(0); });
process.on("SIGTERM", () => { stop(); process.exit(0); });

try {
    console.log("Starter Docker og databasen...");
    if (!succeeds(["docker", "info"])) {
        run(["docker", "desktop", "start"]);
    }
    let dockerReady = false;
    for (let attempt = 0; attempt < 30; attempt++) {
        if (succeeds(["docker", "info"])) {
            dockerReady = true;
            break;
        }
        await Bun.sleep(1000);
    }
    if (!dockerReady) {
        throw new Error("Docker er ikke klar. Åbn Docker Desktop og prøv igen.");
    }
    run(["docker", "compose", "up", "-d", "db"]);
    let databaseReady = false;
    for (let attempt = 0; attempt < 30; attempt++) {
        if (succeeds(["docker", "compose", "exec", "-T", "db", "pg_isready", "-h", "127.0.0.1", "-U", "postgres", "-d", "business_esbjerg"])) {
            databaseReady = true;
            break;
        }
        await Bun.sleep(1000);
    }
    if (!databaseReady) {
        throw new Error("Databasen blev ikke klar.");
    }

    run(["bun", "install", "--frozen-lockfile"], client);

    if (!await responds("http://localhost:5159/openapi/v1.json")) {
        console.log("Starter API...");
        const api = start(["dotnet", "run", "--project", "server/Api/Api.csproj", "--launch-profile", "http"], root);
        await waitFor("http://localhost:5159/openapi/v1.json", api);
    } else {
        console.log("API kører allerede.");
    }
    if (!await responds("http://localhost:3000")) {
        console.log("Starter frontend...");
        const frontend = start(["bun", "run", "dev"], client);
        await waitFor("http://localhost:3000", frontend);
    } else {
        console.log("Frontend kører allerede.");
    }

    console.log("\nKlar: http://localhost:3000");
    console.log("Swagger: http://localhost:5159/swagger");
    console.log("Ctrl+C stopper serverne startet her. Databasen og dens data beholdes.");
    if (children.length > 0) {
        const exitCode = await Promise.race(children.map(child => child.exited));
        process.exitCode = exitCode;
    }
} catch (error) {
    console.error(error instanceof Error ? error.message : error);
    process.exitCode = 1;
} finally {
    stop();
}
