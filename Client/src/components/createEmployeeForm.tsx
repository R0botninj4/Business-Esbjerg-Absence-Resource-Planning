import { useState, type SubmitEvent } from "react";
import type { WorkDay } from "../types/employee";
import { userError, type NewEmployee } from "../api/users";
import { defaultFullTime, defaultPartTime, employmentText } from "../utils/schedule";
import { WeekScheduleInput } from "./weekScheduleInput";

type Props = {
    onCreate: (employee: NewEmployee) => Promise<void>;
    onClose: () => void;
};

// Every employee must use their organisation email
const emailDomain = "@businessesbjerg.com";

// Form for creating an employee: login, employment period and the first weekly schedule
export function CreateEmployeeForm({ onCreate, onClose }: Props) {
    const [saving, setSaving] = useState(false);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [fullTime, setFullTime] = useState(true);
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [days, setDays] = useState<WorkDay[]>(defaultFullTime);
    const [error, setError] = useState("");

    // Full time / part time gives a starting schedule that the admin can adjust
    function chooseEmployment(isFullTime: boolean) {
        setFullTime(isFullTime);
        setDays(isFullTime ? defaultFullTime : defaultPartTime);
    }

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();

        if (!email.trim().toLowerCase().endsWith(emailDomain)) return setError("The email must end with " + emailDomain + ".");

        if (password.length < 12) return setError("The password must be at least 12 characters.");
        if (startDate === "") return setError("Choose the start date.");
        if (endDate !== "" && endDate < startDate) return setError("The end date must be on or after the start date.");
        if (!days.some(day => day !== null)) return setError("The employee must work at least one day.");
        for (const day of days) {
            if (day && day.from >= day.to) return setError("A working day must end after it starts.");
        }

        if (saving) return;
        setSaving(true);
        setError("");
        try {
            await onCreate({
                email: email.trim(), password, type: fullTime ? 1 : 2,
                startDate, endDate: endDate || null, days,
            });
            onClose();
        } catch (error) {
            setError(userError(error));
        } finally {
            setSaving(false);
        }
    }

    return (
        <form onSubmit={handleSubmit} className="bg-white border border-line rounded p-6 border-b-4 border-b-orange mb-8">
            <h2 className="text-2xl mb-4">New employee</h2>

            <fieldset disabled={saving} className="grid grid-cols-3 gap-4">
                <label className="font-semibold">
                    Email
                    <input type="email" required maxLength={254} value={email} onChange={e => setEmail(e.target.value)} placeholder={"name" + emailDomain} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                </label>
                <label className="font-semibold">
                    Password <span className="font-normal text-slate">(min. 12 characters)</span>
                    <input type="password" required minLength={12} maxLength={128} autoComplete="new-password" value={password} onChange={e => setPassword(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                </label>

                <div className="font-semibold">
                    Employment
                    <div className="flex gap-4 mt-3 font-normal">
                        <label className="flex items-center gap-2">
                            <input type="radio" checked={fullTime} onChange={() => chooseEmployment(true)} /> Full time
                        </label>
                        <label className="flex items-center gap-2">
                            <input type="radio" checked={!fullTime} onChange={() => chooseEmployment(false)} /> Part time
                        </label>
                    </div>
                </div>
                <label className="font-semibold">
                    Start date
                    <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                </label>
                <label className="font-semibold">
                    End date <span className="font-normal text-slate">(optional)</span>
                    <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                </label>
            </fieldset>

            <div className="mt-6 border-t border-line pt-4">
                <p className="font-semibold mb-2">First weekly schedule</p>
                <fieldset disabled={saving}><WeekScheduleInput days={days} onChange={setDays} /></fieldset>
                <p className="mt-3 text-slate">{employmentText(days)} · accounting can change it later on the employee's page</p>
            </div>

            {error && <p className="text-red-600 mt-3">{error}</p>}

            <div className="flex gap-3 mt-4">
                <button disabled={saving} type="submit" className="bg-navy text-white font-semibold px-6 py-2 rounded hover:bg-navy/90">{saving ? "Saving..." : "Create employee"}</button>
                <button disabled={saving} type="button" onClick={onClose} className="border border-navy font-semibold px-6 py-2 rounded bg-white hover:bg-cream">Cancel</button>
            </div>
        </form>
    );
}
