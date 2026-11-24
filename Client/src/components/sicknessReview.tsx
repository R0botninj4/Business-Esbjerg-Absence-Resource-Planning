import { useState } from "react";
import type { SicknessReport, SicknessStatus } from "../types/sicknessReport";
import { formatDate, holidayTypeName, displayName } from "../utils/format";
import { suggestedRefund, totalHours } from "../utils/sickness";

type Props = {
    report: SicknessReport;
    allReports: SicknessReport[]; // used to count sick days already given this holiday year
    onDecide: (id: number, status: SicknessStatus, refundedHours: number, comment: string) => void;
};

// Accounting's view of one sickness report on the Approvals page
export function SicknessReview({ report, allReports, onDecide }: Props) {
    const suggestion = suggestedRefund(report, allReports);
    const sickHours = totalHours(report.sickDays);

    const [hours, setHours] = useState(suggestion.hours.toFixed(2));
    const [comment, setComment] = useState("");
    const [error, setError] = useState("");

    function approve() {
        const refund = Number(hours);
        if (hours === "" || refund < 0 || refund > sickHours) {
            setError("Hours to give back must be between 0 and " + sickHours.toFixed(2) + ".");
            return;
        }
        onDecide(report.id, "approved", refund, comment);
    }

    function decline() {
        if (comment.trim() === "") {
            setError("Please write a comment to the employee before declining.");
            return;
        }
        onDecide(report.id, "declined", 0, comment);
    }

    return (
        <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
            <p className="text-sm font-semibold uppercase tracking-wide text-slate">Sickness report from</p>
            <h2 className="text-3xl">{displayName(report.employeeName)}</h2>

            <div className="flex gap-10 mt-3">
                <p><strong>Holiday type:</strong> {holidayTypeName(report.holidayType)}</p>
                <p><strong>Sick from:</strong> {formatDate(report.start)}</p>
                <p><strong>To:</strong> {formatDate(report.end)}</p>
                <p className="text-slate">Reported {formatDate(report.createdAt)}</p>
            </div>
            <p className="mt-2"><strong>Got sick:</strong> {report.beforeHoliday ? "Before the holiday started" : "During the holiday"}</p>

            {/* The sick days and their hours */}
            <div className="mt-4 grid grid-cols-2 gap-8">
                <div>
                    <p className="font-semibold">Sick days</p>
                    {report.sickDays.map(day => (
                        <div key={day.date.getTime()} className="flex justify-between border-b border-line py-1">
                            <span>{day.date.toLocaleDateString("en-GB", { weekday: "short", day: "2-digit", month: "2-digit" })}</span>
                            <span>{day.hours.toFixed(2)} h</span>
                        </div>
                    ))}
                    <div className="flex justify-between py-1 font-semibold">
                        <span>Total</span>
                        <span>{sickHours.toFixed(2)} h</span>
                    </div>
                </div>

                <div>
                    <p className="font-semibold">Doctor's note</p>
                    {report.doctorsNote ? (
                        <a href={report.doctorsNote.url} target="_blank" className="underline hover:text-orange">
                            {report.doctorsNote.name}
                        </a>
                    ) : (
                        <p className="text-slate">No doctor's note</p>
                    )}
                </div>
            </div>

            {/* Our suggestion – accounting decides */}
            <div className="mt-4 bg-cream border border-line rounded p-4">
                <p><strong>Suggested:</strong> give back {suggestion.hours.toFixed(2)} h</p>
                <p className="text-sm text-slate">{suggestion.reason}</p>
            </div>

            <label className="block mt-4 font-semibold">
                Hours to give back
                <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={hours}
                    onChange={e => setHours(e.target.value)}
                    className="block mt-1 w-40 border border-line rounded p-2 font-normal"
                />
            </label>

            <label className="block mt-4 font-semibold">
                Comment to the employee <span className="font-normal text-slate">(required if you decline)</span>
                <textarea
                    value={comment}
                    onChange={e => setComment(e.target.value)}
                    rows={2}
                    className="mt-1 w-full border border-line rounded p-2 font-normal"
                />
            </label>

            {error && <p role="alert" className="text-red-600 mt-1">{error}</p>}

            <div className="flex gap-3 mt-4">
                <button onClick={approve} className="bg-navy text-white font-semibold px-8 py-2 rounded hover:bg-navy/90">
                    Approve
                </button>
                <button onClick={decline} className="border-2 border-red-600 text-red-600 font-semibold px-8 py-2 rounded bg-white hover:bg-red-50">
                    Decline
                </button>
            </div>
        </div>
    );
}
