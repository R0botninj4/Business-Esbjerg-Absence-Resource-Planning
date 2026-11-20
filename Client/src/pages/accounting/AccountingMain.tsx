import { useState } from "react";
import { WorkSchedule } from "../../components/workSchedule";
import { testEmployees } from "../../mockdata/testEmployees";
import { testEvents } from "../../mockdata/testEvents";
import type { HolidayRequest, RequestStatus } from "../../types/holidayRequest";
import { formatDate, holidayTypeName } from "../../utils/format";

type Props = {
    requests: HolidayRequest[];
    onDecide: (id: number, status: RequestStatus, comment: string) => void;
};

export function AccountingMain({ requests, onDecide }: Props) {
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [comment, setComment] = useState("");
    const [error, setError] = useState("");
    
    const pending = requests
        .filter(r => r.status === "pending")
        .sort((a, b) => a.createdAt.getTime() - b.createdAt.getTime());
    
    const selected = pending.find(r => r.id === selectedId) ?? pending[0];

    function selectRequest(id: number) {
        setSelectedId(id);
        setComment("");
        setError("");
    }

    function approve() {
        if (!selected) return;
        onDecide(selected.id, "approved", comment);
        setComment("");
        setError("");
    }

    function reject() {
        if (!selected) return;
        if (comment.trim() === "") {
            setError("Please write a comment to the employee before rejecting.");
            return;
        }
        onDecide(selected.id, "declined", comment);
        setComment("");
        setError("");
    }

    if (!selected) {
        return (
            <div>
                <h1 className="text-3xl mb-2">Approvals</h1>
                <p className="text-slate">No requests waiting. </p>
            </div>
        );
    }

    return (
        <div>
            <h1 className="text-3xl">Approvals</h1>
            <p className="text-slate mb-6">{pending.length} requests waiting · oldest first</p>

            <div className="grid grid-cols-4 gap-6 items-start">
                {/* Left: the list of requests */}
                <div className="space-y-3">
                    {pending.map(request => (
                        <button
                            key={request.id}
                            onClick={() => selectRequest(request.id)}
                            className={"w-full text-left bg-white rounded p-4 border-2 " +
                                (request.id === selected.id ? "border-orange" : "border-line hover:border-orange/40")}
                        >
                            <div className="flex justify-between items-start">
                                <span className="font-semibold text-lg">{request.employeeName}</span>
                                <span className="text-xs font-semibold bg-navy/10 px-2 py-1 rounded-full">
                                    {holidayTypeName(request.holidayType)}
                                </span>
                            </div>
                            <p>{formatDate(request.start)} – {formatDate(request.end)}</p>
                            <p className="text-sm text-slate mt-1">Requested {formatDate(request.createdAt)}</p>
                        </button>
                    ))}
                </div>

                {/* Right: the selected request and the work schedule */}
                <div className="col-span-3 space-y-6">
                    <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
                        <p className="text-sm font-semibold uppercase tracking-wide text-slate">Request from</p>
                        <h2 className="text-3xl">{selected.employeeName}</h2>

                        <div className="flex gap-10 mt-3">
                            <p><strong>Holiday type:</strong> {holidayTypeName(selected.holidayType)}</p>
                            <p><strong>From:</strong> {formatDate(selected.start)}</p>
                            <p><strong>To:</strong> {formatDate(selected.end)}</p>
                            <p className="text-slate">Requested {formatDate(selected.createdAt)}</p>
                        </div>

                        <label className="block mt-4 font-semibold">
                            Comment to the employee <span className="font-normal text-slate">(required if you reject)</span>
                            <textarea
                                value={comment}
                                onChange={e => setComment(e.target.value)}
                                rows={2}
                                className="mt-1 w-full border border-line rounded p-2 font-normal"
                            />
                        </label>

                        {error && <p className="text-red-600 mt-1">{error}</p>}

                        <div className="flex gap-3 mt-4">
                            <button onClick={approve} className="bg-navy text-white font-semibold px-8 py-2 rounded hover:bg-navy/90">
                                Approve
                            </button>
                            <button onClick={reject} className="border-2 border-red-600 text-red-600 font-semibold px-8 py-2 rounded bg-white hover:bg-red-50">
                                Reject
                            </button>
                        </div>
                    </div>

                    <WorkSchedule employees={testEmployees} requests={requests} events={testEvents} selected={selected} />
                </div>
            </div>
        </div>
    );
}