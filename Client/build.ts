import tailwind from "bun-plugin-tailwind";

const result = await Bun.build({
    entrypoints: ["./src/index.html"],
    outdir: "./dist",
    target: "browser",
    minify: true,
    sourcemap: "linked",
    define: { "process.env.NODE_ENV": JSON.stringify("production") },
    env: "BUN_PUBLIC_*",
    plugins: [tailwind],
});

if (!result.success) {
    console.error(result.logs);
    process.exit(1);
}

console.log("Frontend built with Tailwind CSS.");
