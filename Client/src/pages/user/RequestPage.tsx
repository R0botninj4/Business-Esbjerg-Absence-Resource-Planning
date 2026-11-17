import { useState } from "react";
import { Calendar } from "../../components/calendar";
import { testEvents } from "../../mockdata/testEvents";

// Date -> "YYYY-MM-DD" (the format <input type="date"> uses)
function toInputValue(date: Date | null) {
    if (!date) return "";
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${date.getFullYear()}-${month}-${day}`;
}

// "YYYY-MM-DD" -> Date (local time, midnight)
function fromInputValue(value: string) {
    return value ? new Date(value + "T00:00") : null;
}

export function RequestPage() {
    const [start, setStart] = useState<Date | null>(null);
    const [end, setEnd] = useState<Date | null>(null);

    function handleSelect(day: Date) {
        if (!start || end || day < start) {
            setStart(day);
            setEnd(null);
        } else {
            setEnd(day);
        }
    }

    function handleStartInput(value: string) {
        const date = fromInputValue(value);
        setStart(date);
        if (date && end && end < date) setEnd(null); // end can't be before start
    }

    return (
        <div>
            <h1 className="text-3xl mb-6">Request holiday</h1>

            <Calendar start={start} end={end} onSelect={handleSelect} events={testEvents} />

            <div className="mt-6 flex gap-6">
                <label className="flex flex-col font-semibold">
                    Start date
                    <input
                        type="date"
                        value={toInputValue(start)}
                        onChange={e => handleStartInput(e.target.value)}
                        className="mt-1 border border-line rounded p-2 bg-white font-normal"
                    />
                </label>

                <label className="flex flex-col font-semibold">
                    End date
                    <input
                        type="date"
                        value={toInputValue(end)}
                        min={toInputValue(start)}
                        onChange={e => setEnd(fromInputValue(e.target.value))}
                        className="mt-1 border border-line rounded p-2 bg-white font-normal"
                    />
                </label>
            </div>
        </div>
    );
}