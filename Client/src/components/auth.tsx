import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { Navigate } from "react-router";
import { api } from "../api/client";
import type { UserInfo } from "../api/Api";

export type Role = "Employee" | "Accounting" | "Admin";

export function homePath(role: string) {
    if (role === "Admin") return "/admin";
    if (role === "Accounting") return "/accounting";
    return "/user";
}

type AuthValue = {
    user: UserInfo | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthValue | null>(null);

export function useAuth() {
    const value = useContext(AuthContext);
    if (!value) throw new Error("useAuth skal bruges inde i AuthProvider");
    return value;
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<UserInfo | null>(null);
    const [loading, setLoading] = useState(true);
    
    useEffect(() => {
        api.api.authMeList()
            .then(setUser)
            .catch(() => setUser(null))
            .finally(() => setLoading(false));
    }, []);

    async function login(email: string, password: string) {
        const loggedInUser = await api.api.authLoginCreate({ email, password });
        setUser(loggedInUser);
    }

    async function logout() {
        await api.api.authLogoutCreate();
        setUser(null);
    }

    return (
        <AuthContext.Provider value={{ user, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function RequireRole({ role, children }: { role: Role; children: ReactNode }) {
    const { user, loading } = useAuth();

    if (loading) return <p className="p-8">Loading...</p>;
    if (!user) return <Navigate to="/login" replace />;
    if (user.role !== role) return <Navigate to={homePath(user.role)} replace />;

    return children;
}

export function HomeRedirect() {
    const { user, loading } = useAuth();

    if (loading) return <p className="p-8">Loading...</p>;
    if (!user) return <Navigate to="/login" replace />;

    return <Navigate to={homePath(user.role)} replace />;
}