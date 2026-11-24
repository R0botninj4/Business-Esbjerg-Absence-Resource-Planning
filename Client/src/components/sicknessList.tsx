import type { SicknessReport } from "../types/sicknessReport";
import { formatDate, holidayTypeName } from "../utils/format";
import { totalHours } from "../utils/sickness";

const statusText = {
    pending: "Pending",
    approved: "Approved",
    declined: "Declined",
};

const badgeColors = {
    pending: "bg-gold",
    approved: "bg-navy",
    declined: "bg-orange",
};

// A short list of sickness reports, newest first
export function SicknessList({ reports }: { reports: SicknessReport[] }) {
    const sorted = [...reports].sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());

    if (sorted.length === 0) {
        return <p className="text-slate">No sickness reported.</p>;
    }

    return (
        <div>
            {sorted.map(report => (
                <div key={report.id} className="py-2 border-b border-line last:border-0">
                    <div className="flex justify-between items-center">
                        <span>{formatDate(report.start)} – {formatDate(report.end)}</span>
                        <span className={"text-xs font-semibold text-white px-2 py-1 rounded-full " + badgeColors[report.status]}>
                            {statusText[report.status]}
                        </span>
                    </div>
                    <p className="text-sm text-slate">
                        {holidayTypeName(report.holidayType)} · {totalHours(report.sickDays).toFixed(2)} h sick
                        {report.status === "approved" && " · " + (report.refundedHours ?? 0).toFixed(2) + " h given back"}
                    </p>
                    {report.comment && <p className="text-sm"><strong>Comment:</strong> {report.comment}</p>}
                </div>
            ))}
        </div>
    );
}