import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { getEmployees } from "../api/schedules";
import { userError } from "../api/users";
import type { Employee } from "../types/employee";

type EmployeeData = { employees: Employee[]; reload: () => Promise<void> };
const EmployeeContext = createContext<EmployeeData | null>(null);

export function useEmployees() {
    const data = useContext(EmployeeContext);
    if (!data) throw new Error("EmployeeProvider is missing.");
    return data;
}

export function EmployeeProvider({ children }: { children: ReactNode }) {
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    async function reload() {
        // Behold den aabne formular ved genindlaesning efter en gemning.
        const saved = await getEmployees();
        setEmployees(saved);
    }

    async function load() {
        setLoading(true);
        setError("");
        try { await reload(); }
        catch (error) { setError(userError(error)); }
        finally { setLoading(false); }
    }

    useEffect(() => { void load(); }, []);
    if (loading) return <p role="status" className="p-6">Loading employees and schedules...</p>;
    if (error) return <div role="alert" className="p-6">{error} <button onClick={() => void load()} className="underline">Try again</button></div>;
    return <EmployeeContext.Provider value={{ employees, reload }}>{children}</EmployeeContext.Provider>;
}
