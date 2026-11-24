import { useEffect, useState } from "react";
import { Calendar } from "../../components/calendar";
import { RequestCard } from "../../components/requestCard";
import type { HolidayRequest, HolidayType } from "../../types/holidayRequest";
import { formatDate, holidayTypeName } from "../../utils/format";
import { previewHolidayRequest, type NewHolidayRequest } from "../../api/holidayRequests";
import { userError } from "../../api/users";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    onAdd: (request: NewHolidayRequest) => Promise<void>;
    onCancel: (id: string) => Promise<void>;
    events: CalendarEvent[];
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

export function RequestPage({ requests, onAdd, onCancel, events }: Props) {
    const [start, setStart] = useState<Date | null>(null);
    const [end, setEnd] = useState<Date | null>(null);
    const [holidayType, setHolidayType] = useState<HolidayType>("weeks1to5");
    const [hours, setHours] = useState<number | null>(null);
    const [calculating, setCalculating] = useState(false);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    useEffect(() => {
        let cancelled = false;
        setHours(null);
        setError("");
        if (!start || !end) { setCalculating(false); return; }
        setCalculating(true);
        previewHolidayRequest({ account: holidayType === "weeks1to5" ? 1 : 2, startDate: toInputValue(start), endDate: toInputValue(end) })
            .then(preview => { if (!cancelled) setHours(Number(preview.hours)); })
            .catch(error => { if (!cancelled) setError(userError(error)); })
            .finally(() => { if (!cancelled) setCalculating(false); });
        return () => { cancelled = true; };
    }, [start, end, holidayType, requests]);
    
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

    async function handleConfirm() {
        if (!start || !end || hours === null || calculating || saving) {
            return;
        }
        setSaving(true);
        setError("");
        setSuccess("");
        try {
            await onAdd({ account: holidayType === "weeks1to5" ? 1 : 2, startDate: toInputValue(start), endDate: toInputValue(end) });
            setStart(null);
            setEnd(null);
            setSuccess("Your request has been saved and is waiting for approval.");
        } catch (error) {
            setError(userError(error));
        } finally {
            setSaving(false);
        }
    }
    return (
        <div>
            <h1 className="text-3xl mb-6">Request holiday</h1>
            {success && <p role="status" className="mb-4">{success}</p>}

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
                        events={events}
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

                        <p className="mt-3"><strong>Requested hours:</strong> {calculating ? "Calculating..." : hours === null ? "–" : hours.toFixed(2) + " h"}</p>
                        <p className="text-sm text-slate mt-2">Submitting a request does not approve the holiday or deduct hours from your balance.</p>
                        {error && <p role="alert" className="text-red-600 mt-3">{error}</p>}
                        <button
                            onClick={() => void handleConfirm()}
                            disabled={!start || !end || hours === null || calculating || saving}
                            className="mt-6 w-full bg-orange-light text-white font-semibold px-6 py-3 rounded hover:bg-orange disabled:opacity-40 disabled:cursor-not-allowed"
                        >
                            {saving ? "Saving..." : "Confirm request"}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}