import { useState, type SubmitEvent } from "react";
import type { SchedulePeriod, WorkDay } from "../types/employee";
import { employmentText, weekdayNames } from "../utils/schedule";
import { TimeSelect } from "./timeSelect";

type Props = {
    startDays: WorkDay[]; // the current schedule, used as a starting point
    onSave: (period: SchedulePeriod) => void;
    onClose: () => void;
};

// A form for giving an employee a new weekly schedule from a certain date
export function ScheduleForm({ startDays, onSave, onClose }: Props) {
    const [days, setDays] = useState<WorkDay[]>(startDays);
    const [validFrom, setValidFrom] = useState("");
    const [error, setError] = useState("");

    // Change one day in the list (React needs a new list, so we copy it first)
    function updateDay(index: number, day: WorkDay) {
        const copy = [...days];
        copy[index] = day;
        setDays(copy);
    }

    function handleSubmit(e: SubmitEvent) {
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

        onSave({ validFrom: new Date(validFrom + "T00:00"), days: days });
        onClose();
    }

    return (
        <form onSubmit={handleSubmit} className="mt-4 border-t border-line pt-4">
            <h3 className="text-xl mb-3">New schedule</h3>

            <label className="font-semibold">
                Valid from{" "}
                <input
                    type="date"
                    value={validFrom}
                    onChange={e => setValidFrom(e.target.value)}
                    className="border border-line rounded p-2 font-normal"
                />
            </label>

            <div className="mt-4 space-y-2">
                {days.map((day, index) => (
                    <div key={index} className="flex items-center gap-4">
                        <span className="w-28 font-semibold">{weekdayNames[index]}</span>

                        <label className="flex items-center gap-2">
                            <input
                                type="checkbox"
                                checked={day !== null}
                                onChange={e => updateDay(index, e.target.checked ? { from: "08:00", to: "15:30" } : null)}
                            />
                            Working
                        </label>

                        {day && (
                            <>
                                <TimeSelect value={day.from} onChange={value => updateDay(index, { from: value, to: day.to })} />
                                –
                                <TimeSelect value={day.to} onChange={value => updateDay(index, { from: day.from, to: value })} />
                            </>
                        )}
                    </div>
                ))}
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
        </form>
    );
}