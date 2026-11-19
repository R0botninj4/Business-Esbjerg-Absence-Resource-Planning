import { useState } from "react";
import { Navigate, useNavigate } from "react-router";
import { homePath, useAuth } from "../../components/auth";

export function LoginPage() {
    const navigate = useNavigate();
    const { user, loading, login } = useAuth();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    
    if (!loading && user) return <Navigate to={homePath(user.role)} replace />;

    async function handleLogin(e: React.FormEvent) {
        e.preventDefault();
        setError("");

        try {
            await login(email, password);
        } catch (err) {
            if ((err as { status?: number }).status === 401) {
                setError("Forkert e-mail eller adgangskode.");
            } else {
                setError("Kunne ikke kontakte serveren. Prøv igen.");
            }
        }
    }

    return (
        <form onSubmit={handleLogin}>
            <input placeholder="E-mail" value={email} onChange={(e) => setEmail(e.target.value)} />
            <input type="password" placeholder="Adgangskode" value={password} onChange={(e) => setPassword(e.target.value)} />
            <button type="submit">Login</button>
            {error && <p>{error}</p>}
        </form>
    );
}