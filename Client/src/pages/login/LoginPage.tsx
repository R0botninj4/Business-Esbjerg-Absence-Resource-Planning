import { useState } from "react";
import { useNavigate } from "react-router";
import { api } from "../../api/client";

export function LoginPage() {
    const navigate = useNavigate();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    async function login() {
        try {
            await api.api.authLoginCreate({ email, password });
            navigate("/user");
        } catch {
            setError("Forkert e-mail eller adgangskode.");
        }
    }

    return (
        <div>
            <input placeholder="E-mail" value={email} onChange={(e) => setEmail(e.target.value)} />
            <input type="password" placeholder="Adgangskode" value={password} onChange={(e) => setPassword(e.target.value)} />
            <button onClick={login}>Login</button>
            {error && <p>{error}</p>}
        </div>
    );
}
