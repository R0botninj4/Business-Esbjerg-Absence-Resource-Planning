import { useState, type SubmitEvent } from "react";
import type { SchedulePeriod, WorkDay } from "../types/employee";
import { employmentText } from "../utils/schedule";
import { userError } from "../api/users";
import { WeekScheduleInput } from "./weekScheduleInput";

type Props = {
    startDays: WorkDay[]; // the current schedule, used as a starting point
    onSave: (period: SchedulePeriod) => void | Promise<void>;
    onClose: () => void;
};

// A form for giving an employee a new weekly schedule from a certain date
export function ScheduleForm({ startDays, onSave, onClose }: Props) {
    const [days, setDays] = useState<WorkDay[]>(startDays);
    const [validFrom, setValidFrom] = useState("");
    const [error, setError] = useState("");
    const [saving, setSaving] = useState(false);

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();

        if (validFrom === "") {
            setError("Choose the date the new schedule starts.");
            return;
        }
        for (const day of days) {
            if (day && day.from >= day.to) {
                setError("A working day must end after it starts.");
                return;
            }
        }

        if (saving) return;
        setSaving(true);
        setError("");
        try {
            await onSave({ validFrom: new Date(validFrom + "T00:00"), days });
            onClose();
        } catch (error) {
            setError(error instanceof Error ? error.message : userError(error));
        } finally {
            setSaving(false);
        }
    }

    return (
        <form onSubmit={handleSubmit} className="mt-4 border-t border-line pt-4">
            <fieldset disabled={saving}><h3 className="text-xl mb-3">New schedule</h3>

            <label className="font-semibold">
                Valid from{" "}
                <input
                    type="date"
                    value={validFrom}
                    onChange={e => setValidFrom(e.target.value)}
                    className="border border-line rounded p-2 font-normal"
                />
            </label>

            <div className="mt-4">
                <WeekScheduleInput days={days} onChange={setDays} />
            </div>

            <p className="mt-3 text-slate">New schedule: {employmentText(days)}</p>
            {error && <p className="text-red-600 mt-2">{error}</p>}

            <div className="flex gap-3 mt-4">
                <button type="submit" className="bg-navy text-white font-semibold px-6 py-2 rounded hover:bg-navy/90">
                    Save schedule
                </button>
                <button type="button" onClick={onClose} className="border border-navy font-semibold px-6 py-2 rounded bg-white hover:bg-cream">
                    Cancel
                </button>
            </div>
            </fieldset>
        </form>
    );
}
