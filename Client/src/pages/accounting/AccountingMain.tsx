import { useState } from "react";
import { WorkSchedule } from "../../components/workSchedule";
import { SicknessReview } from "../../components/sicknessReview";
import { useEmployees } from "../../components/employeeData";
import type { HolidayRequest } from "../../types/holidayRequest";
import type { SicknessReport, SicknessStatus } from "../../types/sicknessReport";
import { formatDate, holidayTypeName } from "../../utils/format";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    events: CalendarEvent[];
    sicknessReports: SicknessReport[];
    onDecideSickness: (id: number, status: SicknessStatus, refundedHours: number, comment: string) => void;
};

export function AccountingMain({ requests, events, sicknessReports, onDecideSickness }: Props) {
    const [selectedId, setSelectedId] = useState<string | null>(null);
    const [selectedSicknessId, setSelectedSicknessId] = useState<number | null>(null);
    const { employees } = useEmployees();

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

    function selectRequest(id: string) {
        setSelectedId(id);
        setSelectedSicknessId(null);
    }

    if (!selected && !selectedSickness) {
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
            <p className="text-slate mb-6">{pending.length + pendingSickness.length} requests waiting - oldest first</p>

            <div className="grid grid-cols-4 gap-6 items-start">
                {/* Left: the list of requests */}
                <div className="space-y-3">
                    {pending.map(request => (
                        <button
                            key={request.id}
                            onClick={() => selectRequest(request.id)}
                            className={"w-full text-left bg-white rounded p-4 border-2 " +
                                (request.id === selected?.id ? "border-orange" : "border-line hover:border-orange/40")}
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

                    {/* Sickness reports get a "Sickness" badge instead of the week */}
                    {pendingSickness.map(report => (
                        <button
                            key={report.id}
                            onClick={() => setSelectedSicknessId(report.id)}
                            className={"w-full text-left bg-white rounded p-4 border-2 " +
                                (report.id === selectedSickness?.id ? "border-orange" : "border-line hover:border-orange/40")}
                        >
                            <div className="flex justify-between items-start">
                                <span className="font-semibold text-lg">{report.employeeName}</span>
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
                            <h2 className="text-3xl">{selected.employeeName}</h2>

                            <div className="flex gap-10 mt-3">
                                <p><strong>Holiday type:</strong> {holidayTypeName(selected.holidayType)}</p>
                                <p><strong>From:</strong> {formatDate(selected.start)}</p>
                                <p><strong>To:</strong> {formatDate(selected.end)}</p>
                                <p className="text-slate">Requested {formatDate(selected.createdAt)}</p>
                            </div>

                            <p className="mt-4"><strong>Requested hours:</strong> {selected.hours.toFixed(2)} h</p>
                            <p className="mt-3 text-slate">Requests can be viewed here. Approval and rejection will be available later.</p>
                        </div>

                        <WorkSchedule employees={employees} requests={requests} events={events} selected={selected} />
                    </div>
                )}
            </div>
        </div>
    );
}