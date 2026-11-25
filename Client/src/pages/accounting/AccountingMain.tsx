import { useEffect, useState } from "react";
import { userError } from "../../api/users";
import { PostedBalance } from "../../components/postedBalance";
import { WorkSchedule } from "../../components/workSchedule";
import { SicknessReview } from "../../components/sicknessReview";
import { useEmployees } from "../../components/employeeData";
import type { HolidayRequest } from "../../types/holidayRequest";
import type { SicknessReport, SicknessStatus } from "../../types/sicknessReport";
import { formatDate, holidayTypeName, displayName } from "../../utils/format";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    events: CalendarEvent[];
    onDecide: (id: string, approve: boolean, comment: string) => Promise<void>;
    sicknessReports: SicknessReport[];
    onDecideSickness: (id: number, status: SicknessStatus, refundedHours: number, comment: string) => void;
};

export function AccountingMain({ requests, events, onDecide, sicknessReports, onDecideSickness }: Props) {
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [selectedSicknessId, setSelectedSicknessId] = useState<number | null>(null);
    const { employees } = useEmployees();
    const [comment, setComment] = useState("");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    const pending = requests
        .filter(r => r.status === "pending")
        .sort((a, b) => a.createdAt.getTime() - b.createdAt.getTime());

    const pendingSickness = sicknessReports
        .filter(r => r.status === "pending")
        .sort((a, b) => a.createdAt.getTime() - b.createdAt.getTime());

    // Only one thing is selected at a time: a holiday request or a sickness report.
    // If nothing is chosen, we show the oldest holiday request (or the oldest sickness report).
    let selectedSickness = pendingSickness.find(r => r.id === selectedSicknessId);
    let selected = selectedSickness ? undefined : pending.find(r => r.id === selectedId) ?? pending[0];
    if (!selected && !selectedSickness) {
        selectedSickness = pendingSickness[0];
    }

    useEffect(() => {
        setComment("");
        setError("");
    }, [selected?.id]);

    async function decide(approve: boolean) {
        if (!selected || saving) {
            return;
        }
        setSaving(true);
        setError("");
        setSuccess("");
        try {
            await onDecide(selected.id, approve, comment);
            if (approve) {
                setSuccess("The request has been approved and the hours reserved.");
            } else {
                setSuccess("The request has been declined.");
            }
        } catch (error) {
            setError(userError(error));
        } finally {
            setSaving(false);
        }
    }

    function selectRequest(id: string) {
        setSelectedId(id);
        setSelectedSicknessId(null);
    }

    if (!selected && !selectedSickness) {
        return (
            <div>
                <h1 className="text-3xl mb-2">Approvals</h1>
                {success && <p role="status" className="mb-3">{success}</p>}
                <p className="text-slate">No requests waiting. </p>
            </div>
        );
    }

    return (
        <div>
            <h1 className="text-3xl">Approvals</h1>
            {success && <p role="status" className="mt-3">{success}</p>}
            <p className="text-slate mb-6">{pending.length + pendingSickness.length} requests waiting - oldest first</p>

            <div className="grid grid-cols-4 gap-6 items-start">
                {/* Left: the list of requests */}
                <div className="space-y-3">
                    {pending.map(request => (
                        <button
                            key={request.id}
                            disabled={saving}
                            onClick={() => selectRequest(request.id)}
                            className={"w-full text-left bg-white rounded p-4 border-2 " +
                                (request.id === selected?.id ? "border-orange" : "border-line hover:border-orange/40")}
                        >
                            <div className="flex justify-between items-start">
                                <span className="font-semibold text-lg">{displayName(request.employeeName)}</span>
                                <span className="text-xs font-semibold bg-navy/10 px-2 py-1 rounded-full">
                                    {holidayTypeName(request.holidayType)}
                                </span>
                            </div>
                            <p>{formatDate(request.start)} – {formatDate(request.end)}</p>
                            <p className="text-sm text-slate mt-1">Requested {formatDate(request.createdAt)}</p>
                        </button>
                    ))}

                    {/* Sickness reports get a "Sickness" badge instead of the week */}
                    {pendingSickness.map(report => (
                        <button
                            key={report.id}
                            disabled={saving}
                            onClick={() => setSelectedSicknessId(report.id)}
                            className={"w-full text-left bg-white rounded p-4 border-2 " +
                                (report.id === selectedSickness?.id ? "border-orange" : "border-line hover:border-orange/40")}
                        >
                            <div className="flex justify-between items-start">
                                <span className="font-semibold text-lg">{displayName(report.employeeName)}</span>
                                <span className="text-xs font-semibold text-white bg-orange px-2 py-1 rounded-full">
                                    Sickness
                                </span>
                            </div>
                            <p>{formatDate(report.start)} – {formatDate(report.end)}</p>
                            <p className="text-sm text-slate mt-1">Reported {formatDate(report.createdAt)}</p>
                        </button>
                    ))}
                </div>

                {/* Right: the selected sickness report */}
                {selectedSickness && (
                    <div className="col-span-3">
                        <SicknessReview
                            key={selectedSickness.id}
                            report={selectedSickness}
                            allReports={sicknessReports}
                            onDecide={onDecideSickness}
                        />
                    </div>
                )}

                {/* Right: the selected request and the work schedule */}
                {selected && (
                    <div className="col-span-3 space-y-6">
                        <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
                            <p className="text-sm font-semibold uppercase tracking-wide text-slate">Request from</p>
                            <h2 className="text-3xl">{displayName(selected.employeeName)}</h2>

                            <div className="flex gap-10 mt-3">
                                <p><strong>Holiday type:</strong> {holidayTypeName(selected.holidayType)}</p>
                                <p><strong>From:</strong> {formatDate(selected.start)}</p>
                                <p><strong>To:</strong> {formatDate(selected.end)}</p>
                                <p className="text-slate">Requested {formatDate(selected.createdAt)}</p>
                            </div>

                            <p className="mt-4"><strong>Requested hours:</strong> {selected.hours.toFixed(2)} h</p>
                            <div className="mt-4"><PostedBalance key={selected.id} userId={selected.employeeId} /></div>
                            <p className="mt-3 text-slate">Approval checks expected earnings, the saved holiday debt limit and the repayment deadline. Earlier approved holiday is included.</p>
                            <label className="block mt-4">
                                Comment (required when declining)
                                <textarea value={comment} maxLength={500} disabled={saving} onChange={event => setComment(event.target.value)} className="block w-full mt-1 border border-line rounded p-2" />
                            </label>
                            {error && <div role="alert" className="mt-3 text-red-600">
                                <p>{error}</p>
                                <button onClick={() => window.location.reload()} className="underline">Reload requests and balance</button>
                            </div>}
                            <div className="flex gap-3 mt-4">
                                <button disabled={saving} onClick={() => void decide(true)} className="bg-navy text-white rounded px-5 py-2 disabled:opacity-40">{saving ? "Saving..." : "Approve"}</button>
                                <button disabled={saving || !comment.trim()} onClick={() => void decide(false)} className="border border-orange text-orange rounded px-5 py-2 disabled:opacity-40">Decline</button>
                            </div>
                        </div>

                        <WorkSchedule employees={employees} requests={requests} events={events} selected={selected} />
                    </div>
                )}
            </div>
        </div>
    );
}
