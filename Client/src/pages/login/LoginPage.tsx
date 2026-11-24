import { useState, type SubmitEvent } from "react";
import { Navigate } from "react-router";
import { homePath, useAuth } from "../../components/auth";
import logo from "../../assets/logo.svg";

export function LoginPage() {
    const { user, loading, login } = useAuth();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loggingIn, setLoggingIn] = useState(false);
    
    if (!loading && user) return <Navigate to={homePath(user.role)} replace />;

    async function handleLogin(e: SubmitEvent) {
        e.preventDefault();
        setError("");
        setLoggingIn(true);

        try {
            await login(email, password);
        } catch (err) {
            if ((err as { status?: number }).status === 401) {
                setError("Wrong email or password.");
            } else {
                setError("Could not reach the server. Please try again.");
            }
        } finally {
            setLoggingIn(false);
        }
    }

    return (
        <div className="min-h-screen flex items-center justify-center">
            <form
                onSubmit={handleLogin}
                className="w-full max-w-md bg-white border border-line rounded p-10 border-b-4 border-b-orange"
            >
                <img src={logo} alt="Business Esbjerg" className="h-14 mx-auto mb-8" />
                <h1 className="text-3xl mb-1">Log in</h1>
                <p className="text-slate mb-6">Holiday and absence planning</p>

                <label className="block font-semibold">
                    Email
                    <input
                        type="email"
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                        autoComplete="email"
                        required
                        className="mt-1 w-full border border-line rounded p-2 font-normal"
                    />
                </label>

                <label className="block font-semibold mt-4">
                    Password
                    <input
                        type="password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                        autoComplete="current-password"
                        required
                        className="mt-1 w-full border border-line rounded p-2 font-normal"
                    />
                </label>

                {error && <p role="alert" className="text-red-600 mt-4">{error}</p>}

                <button
                    type="submit"
                    disabled={loggingIn}
                    className="mt-6 w-full bg-navy text-white font-semibold py-3 rounded hover:bg-navy/90 disabled:opacity-60"
                >
                    {loggingIn ? "Logging in..." : "Log in"}
                </button>

                <p className="text-sm text-slate text-center mt-6">
                    Forgot your password? Contact the system owner.
                </p>
            </form>
        </div>
    );
}