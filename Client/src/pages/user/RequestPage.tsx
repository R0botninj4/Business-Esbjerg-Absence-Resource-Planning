import { useState } from "react";
import { Calendar } from "../../components/calendar";
import { RequestCard } from "../../components/requestCard";
import { testEvents } from "../../mockdata/testEvents";
import type { HolidayRequest, HolidayType } from "../../types/holidayRequest";
import { formatDate, holidayTypeName } from "../../utils/format";

type Props = {
    requests: HolidayRequest[];
    onAdd: (request: HolidayRequest) => void;
    onCancel: (id: number) => void;
};

function toInputValue(date: Date | null) {
    if (!date) return "";
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return date.getFullYear() + "-" + month + "-" + day;
}

function fromInputValue(value: string) {
    return value ? new Date(value + "T00:00") : null;
}

export function RequestPage({ requests, onAdd, onCancel }: Props) {
    const [start, setStart] = useState<Date | null>(null);
    const [end, setEnd] = useState<Date | null>(null);
    const [holidayType, setHolidayType] = useState<HolidayType>("weeks1to5");
    
    const pendingRequests = requests.filter(r => r.status === "pending");
    const bookedRequests = requests.filter(r => r.status === "pending" || r.status === "approved");

    function handleSelect(day: Date) {
        if (!start || end) {
            setStart(day);
            setEnd(null);
        } else if (day < start) {
            setStart(day);
        } else {
            setEnd(day);
        }
    }

    function handleStartInput(value: string) {
        const date = fromInputValue(value);
        setStart(date);
        if (date && end && end < date) setEnd(null);
    }

    function handleConfirm() {
        if (!start || !end) return;

        const request: HolidayRequest = {
            id: Date.now(), //TUDO temporary id – the database will make the real one
            holidayType: holidayType,
            start: start,
            end: end,
            createdAt: new Date(),
            status: "pending",
        };

        // TODO: send to the backend when the endpoint exists
        console.log("Holiday request:", request);

        onAdd(request);
        setStart(null);
        setEnd(null);
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
                        requested={bookedRequests}
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

                {/* Request Card */}
                <div className="ml-12 pl-12 border-l-2 border-line space-y-4">
                    {pendingRequests.map(request => (
                        <RequestCard key={request.id} request={request} onCancel={onCancel} />
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