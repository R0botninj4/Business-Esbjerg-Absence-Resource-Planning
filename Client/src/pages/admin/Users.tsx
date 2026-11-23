import { useState } from "react";
import { useAuth } from "../../components/auth";
import { CreateEmployeeForm } from "../../components/createEmployeeForm";
import type { Employee } from "../../types/employee";
import type { UserAccount } from "../../types/userAccount";
import { employmentToday } from "../../utils/schedule";

type Props = {
    users: UserAccount[];
    employees: Employee[];
    onCreateEmployee: (employee: Omit<Employee, "id">) => void;
    onChangeEmail: (id: number, email: string) => void;
    onSetActive: (id: number, isActive: boolean) => void;
};

const roleColors = {
    Employee: "bg-navy/10",
    Accounting: "bg-gold/30",
    Admin: "bg-orange/20",
};

export function Users({ users, employees, onCreateEmployee, onChangeEmail, onSetActive }: Props) {
    const { user: me } = useAuth(); // the admin who is logged in
    const [creating, setCreating] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [editEmail, setEditEmail] = useState("");
    const [error, setError] = useState("");

    function startEdit(user: UserAccount) {
        setEditingId(user.id);
        setEditEmail(user.email);
        setError("");
    }

    function saveEdit(user: UserAccount) {
        if (users.some(u => u.email === editEmail && u.id !== user.id)) {
            setError("The email is already in use.");
            return;
        }
        onChangeEmail(user.id, editEmail);
        setEditingId(null);
        setError("");
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl">Users</h1>
                {!creating && (
                    <button onClick={() => setCreating(true)} className="bg-orange-light text-white font-semibold px-6 py-3 rounded hover:bg-orange">
                        + Add employee
                    </button>
                )}
            </div>

            {creating && <CreateEmployeeForm users={users} onCreate={onCreateEmployee} onClose={() => setCreating(false)} />}

            {error && <p className="text-red-600 mb-3">{error}</p>}

            <div className="bg-white border border-line rounded">
                <div className="grid grid-cols-12 gap-4 px-6 py-3 text-sm font-semibold text-slate border-b border-line">
                    <span className="col-span-2">Name</span>
                    <span className="col-span-3">Email</span>
                    <span className="col-span-2">Role</span>
                    <span className="col-span-2">Employment</span>
                    <span className="col-span-1">Status</span>
                    <span className="col-span-2"></span>
                </div>

                {users.map(user => {
                    const employee = employees.find(e => e.email === user.email);
                    const isMe = user.email === me?.email;
                    // The Admin and Accounting accounts must always exist, and you can't deactivate yourself
                    const isProtected = user.role !== "Employee" || isMe;

                    return (
                        <div
                            key={user.id}
                            className={"grid grid-cols-12 gap-4 px-6 py-3 items-center border-b border-line last:border-0 " +
                                (user.isActive ? "" : "text-slate bg-line/40")}
                        >
                            <span className="col-span-2 font-semibold">{employee ? employee.name : "–"}</span>

                            <span className="col-span-3">
                                {editingId === user.id ? (
                                    <input value={editEmail} onChange={e => setEditEmail(e.target.value)} className="w-full border border-line rounded p-1" />
                                ) : (
                                    <>
                                        {user.email}
                                        {isMe && <span className="text-slate"> (you)</span>}
                                    </>
                                )}
                            </span>

                            <span className="col-span-2">
                                <span className={"text-sm font-semibold px-3 py-1 rounded-full " + roleColors[user.role]}>{user.role}</span>
                            </span>

                            <span className="col-span-2 text-slate">{employee ? employmentToday(employee) : "–"}</span>

                            <span className="col-span-1">{user.isActive ? "Active" : "Inactive"}</span>

                            <span className="col-span-2 flex gap-3 justify-end">
                                {editingId === user.id ? (
                                    <>
                                        <button onClick={() => saveEdit(user)} className="font-semibold hover:text-orange">Save</button>
                                        <button onClick={() => setEditingId(null)} className="font-semibold text-slate">Cancel</button>
                                    </>
                                ) : (
                                    <>
                                        <button onClick={() => startEdit(user)} className="font-semibold hover:text-orange">Edit</button>
                                        {!isProtected && (
                                            user.isActive ? (
                                                <button onClick={() => onSetActive(user.id, false)} className="font-semibold text-red-600 hover:underline">
                                                    Deactivate
                                                </button>
                                            ) : (
                                                <button onClick={() => onSetActive(user.id, true)} className="font-semibold text-navy hover:underline">
                                                    Reactivate
                                                </button>
                                            )
                                        )}
                                    </>
                                )}
                            </span>
                        </div>
                    );
                })}
            </div>

            <p className="text-sm text-slate mt-3">
                The Admin and Accounting accounts can't be deactivated – there must always be exactly one of each.
                A deactivated user can't log in, but their data and history are kept.
                Work schedules are changed by accounting on the employee's page.
            </p>
        </div>
    );
}