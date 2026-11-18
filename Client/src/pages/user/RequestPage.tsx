import { useState } from "react";
import { Calendar } from "../../components/calendar";
import { testEvents } from "../../mockdata/testEvents";

type HolidayType = "weeks1to5" | "week6";

type SentRequest = {
    holidayType: HolidayType;
    start: Date;
    end: Date;
};

// Date -> "YYYY-MM-DD" (the format <input type="date"> uses)
function toInputValue(date: Date | null) {
    if (!date) return "";
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return date.getFullYear() + "-" + month + "-" + day;
}

// Date -> "23.11.2026" (Danish format, for showing dates to the user)
function formatDate(date: Date | null) {
    return date ? date.toLocaleDateString("da-DK") : "-";
}

// "YYYY-MM-DD" -> Date (local time, midnight)
function fromInputValue(value: string) {
    return value ? new Date(value + "T00:00") : null;
}

function holidayTypeName(holidayType: HolidayType) {
    return holidayType === "week6" ? "Week 6" : "Week 1–5";
}

export function RequestPage() {
    const [start, setStart] = useState<Date | null>(null);
    const [end, setEnd] = useState<Date | null>(null);
    const [holidayType, setHolidayType] = useState<HolidayType>("weeks1to5");
    const [sentRequests, setSentRequests] = useState<SentRequest[]>([]);

    function handleSelect(day: Date) {
        if (!start || end) {
            // First click, or starting a new selection
            setStart(day);
            setEnd(null);
        } else if (day < start) {
            // Clicked before the start date: move the start
            setStart(day);
        } else {
            // Second click: set the end date
            setEnd(day);
        }
    }

    function handleStartInput(value: string) {
        const date = fromInputValue(value);
        setStart(date);
        if (date && end && end < date) setEnd(null); // end can't be before start
    }

    function handleConfirm() {
        if (!start || !end) return;

        const request = { holidayType: holidayType, start: start, end: end };

        // TODO: send to the backend when the endpoint exists
        console.log("Holiday request:", request);

        setSentRequests([...sentRequests, request]);
        setStart(null);
        setEnd(null);
    }

    function handleCancel(index: number) {
        // TODO: tell the backend the request is cancelled
        setSentRequests(sentRequests.filter((_, i) => i !== index));
    }

    return (
        <div>
            <h1 className="text-3xl mb-6">Request holiday</h1>

            <div className="grid grid-cols-3 items-start">
                {/* Left: choose holiday type and dates */}
                <div className="col-span-2">
                    <div className="flex gap-2 mb-6">
                        <button
                            onClick={() => setHolidayType("weeks1to5")}
                            className={"px-4 py-2 rounded border border-navy font-semibold " +
                                (holidayType === "weeks1to5" ? "bg-navy text-white" : "bg-white")}
                        >
                            Week 1–5
                        </button>
                        <button
                            onClick={() => setHolidayType("week6")}
                            className={"px-4 py-2 rounded border border-navy font-semibold " +
                                (holidayType === "week6" ? "bg-navy text-white" : "bg-white")}
                        >
                            Week 6
                        </button>
                    </div>

                    <Calendar
                        start={start}
                        end={end}
                        onSelect={handleSelect}
                        events={testEvents}
                        requested={sentRequests}
                    />

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

                {/* Right: sent requests and the new request */}
                <div className="ml-12 pl-12 border-l-2 border-line space-y-4">
                    {sentRequests.map((request, index) => (
                        <div key={index} className="bg-line text-slate border border-line rounded p-6 border-b-4 border-b-slate">
                            <h2 className="text-xl mb-2">{holidayTypeName(request.holidayType)}</h2>
                            <p><strong>From:</strong> {formatDate(request.start)}</p>
                            <p><strong>To:</strong> {formatDate(request.end)}</p>
                            <p className="mt-2 font-semibold">Pending</p>

                            <button
                                onClick={() => handleCancel(index)}
                                className="mt-4 w-full border-2 border-red-600 text-red-600 font-semibold py-2 rounded bg-white hover:bg-red-50"
                            >
                                Cancel request
                            </button>
                        </div>
                    ))}

                    <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
                        <h2 className="text-2xl mb-4">New request</h2>
                        <p><strong>Holiday type:</strong> {holidayTypeName(holidayType)}</p>
                        <p><strong>From:</strong> {formatDate(start)}</p>
                        <p><strong>To:</strong> {formatDate(end)}</p>

                        <button
                            onClick={handleConfirm}
                            disabled={!start || !end}
                            className="mt-6 w-full bg-orange-light text-white font-semibold px-6 py-3 rounded hover:bg-orange disabled:opacity-40 disabled:cursor-not-allowed"
                        >
                            Confirm request
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}