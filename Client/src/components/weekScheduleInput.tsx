import type { WorkDay } from "../types/employee";
import { weekdayNames } from "../utils/schedule";
import { TimeSelect } from "./timeSelect";

type Props = {
    days: WorkDay[]; // Monday to Friday
    onChange: (days: WorkDay[]) => void;
};

// Monday to Friday, each with a "Working" checkbox and from/to times.
// Used when creating an employee (admin) and when changing a schedule (accounting).
export function WeekScheduleInput({ days, onChange }: Props) {
    // Change one day in the list (React needs a new list, so we copy it first)
    function updateDay(index: number, day: WorkDay) {
        const copy = [...days];
        copy[index] = day;
        onChange(copy);
    }

    return (
        <div className="space-y-2">
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
    );
}