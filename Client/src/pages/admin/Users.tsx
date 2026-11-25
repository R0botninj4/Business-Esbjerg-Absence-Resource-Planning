import { useEffect, useState } from "react";
import { useAuth } from "../../components/auth";
import { CreateEmployeeForm } from "../../components/createEmployeeForm";
import {
    getUsers, getEmployments, createEmployee, changeUserEmail, deactivateUser,
    reactivateUser, userError, type SavedUser, type SavedEmployment, type NewEmployee,
} from "../../api/users";

export function Users() {
    const { user: me } = useAuth();
    const [users, setUsers] = useState<SavedUser[]>([]);
    const [employments, setEmployments] = useState<SavedEmployment[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [creating, setCreating] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editEmail, setEditEmail] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    async function loadUsers() {
        setLoading(true);
        setError("");
        try {
            const savedUsers = await getUsers();
            const periods: SavedEmployment[] = [];
            for (const user of savedUsers) {
                if (user.role === "Employee") {
                    const savedPeriods = await getEmployments(user.id);
                    periods.push(...savedPeriods);
                }
            }
            setUsers(savedUsers);
            setEmployments(periods);
        } catch (error) {
            setUsers([]);
            setEmployments([]);
            setError(userError(error));
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => { void loadUsers(); }, []);

    async function addEmployee(employee: NewEmployee) {
        // Formularen lukkes først, når API'et har gemt medarbejder og ugeplan.
        const saved = await createEmployee(employee);
        setUsers(current => [...current, saved]);
        setSuccess("Employee created.");
        await loadUsers();
    }

    async function saveEmail(user: SavedUser) {
        if (busy) return;
        setBusy(true);
        setError("");
        setSuccess("");
        try {
            await changeUserEmail(user, editEmail.trim());
            setEditingId(null);
            setSuccess("Email saved.");
            await loadUsers();
        } catch (error) {
            setError(userError(error));
        } finally {
            setBusy(false);
        }
    }

    async function setActive(user: SavedUser) {
        if (busy) return;
        if (user.isActive && !window.confirm("Deactivate " + user.email + "? Their history will be kept.")) return;
        setBusy(true);
        setError("");
        setSuccess("");
        try {
            if (user.isActive) await deactivateUser(user.id);
            else await reactivateUser(user.id);
            setSuccess(user.isActive ? "User deactivated." : "User reactivated.");
            await loadUsers();
        } catch (error) {
            setError(userError(error));
        } finally {
            setBusy(false);
        }
    }

    function employmentText(userId: string) {
        // Lokale kalenderdatoer undgår, at UTC flytter datoen omkring midnat.
        const now = new Date();
        const today = now.getFullYear() + "-" + String(now.getMonth() + 1).padStart(2, "0") + "-" + String(now.getDate()).padStart(2, "0");
        const periods = employments.filter(period => period.userId === userId);
        const active = periods.find(period => period.startDate <= today && (!period.endDate || period.endDate >= today));
        if (active) {
            if (active.type === 1) return "Full time";
            if (active.type === 2) return "Part time";
            return "Hourly";
        }
        const next = periods.filter(period => period.startDate > today).sort((a, b) => a.startDate.localeCompare(b.startDate))[0];
        if (next) return "Starts " + next.startDate;
        if (periods.length > 0) return "Employment ended";
        return "–";
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl">Users</h1>
                {!creating && <button disabled={loading || busy} onClick={() => { setCreating(true); setSuccess(""); }} className="bg-orange-light text-white font-semibold px-6 py-3 rounded disabled:opacity-50">+ Add employee</button>}
            </div>
            {creating && <CreateEmployeeForm onCreate={addEmployee} onClose={() => setCreating(false)} />}
            {error && <p role="alert" className="text-red-600 mb-3">{error} <button onClick={() => void loadUsers()} disabled={loading || busy} className="underline">Reload users</button></p>}
            {success && <p role="status" className="mb-3 text-navy">{success}</p>}
            {loading ? <p role="status">Loading users...</p> : (
                <div className="bg-white border border-line rounded overflow-x-auto">
                    <table className="w-full text-left">
                        <thead className="text-sm text-slate border-b border-line"><tr>
                            <th className="p-4">Email</th><th className="p-4">Role</th><th className="p-4">Employment</th><th className="p-4">Status</th><th className="p-4">Actions</th>
                        </tr></thead>
                        <tbody>{users.map(user => {
                            const isMe = user.id === me?.id;
                            const isProtected = user.role !== "Employee" || isMe;
                            return <tr key={user.id} className={"border-b border-line " + (user.isActive ? "" : "text-slate bg-line/40")}>
                                <td className="p-4">{editingId === user.id ? <input aria-label="New email" type="email" value={editEmail} onChange={e => setEditEmail(e.target.value)} disabled={busy} className="border border-line rounded p-1" /> : user.email}{isMe && " (you)"}</td>
                                <td className="p-4">{user.role}</td><td className="p-4">{employmentText(user.id)}</td><td className="p-4">{user.isActive ? "Active" : "Inactive"}</td>
                                <td className="p-4"><div className="flex gap-3">
                                    {editingId === user.id ? <>
                                        <button disabled={busy} onClick={() => void saveEmail(user)} className="font-semibold">{busy ? "Saving..." : "Save"}</button>
                                        <button disabled={busy} onClick={() => setEditingId(null)}>Cancel</button>
                                    </> : <>
                                        <button disabled={busy || creating} onClick={() => { setEditingId(user.id); setEditEmail(user.email); setError(""); setSuccess(""); }} className="font-semibold">Edit</button>
                                        {!isProtected && <button disabled={busy || creating} onClick={() => void setActive(user)} className="font-semibold text-red-600">{user.isActive ? "Deactivate" : "Reactivate"}</button>}
                                    </>}
                                </div></td>
                            </tr>;
                        })}</tbody>
                    </table>
                    {users.length === 0 && <p className="p-4">No users to display.</p>}
                </div>
            )}
            <p className="text-sm text-slate mt-3">Admin and Accounting accounts cannot be deactivated. Deactivation keeps the employee's data and history.</p>
        </div>
    );
}
