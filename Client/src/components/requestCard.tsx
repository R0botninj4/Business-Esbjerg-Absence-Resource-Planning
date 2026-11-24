import { useState } from "react";
import { userError } from "../api/users";
import type { HolidayRequest } from "../types/holidayRequest";
import { formatDate, holidayTypeName } from "../utils/format";

type Props = {
    request: HolidayRequest;
    onCancel?: (id: string) => Promise<void>;
};

const statusText = {
    pending: "Pending",
    approved: "Approved",
    declined: "Declined",
    cancelled: "Cancelled",
};

const badgeColors = {
    pending: "bg-gold",
    approved: "bg-navy",
    declined: "bg-orange",
    cancelled: "bg-slate",
};

const lineColors = {
    pending: "border-b-gold",
    approved: "border-b-navy",
    declined: "border-b-orange",
    cancelled: "border-b-slate",
};
export function RequestCard({ request, onCancel }: Props) {
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    async function withdraw() {
        if (!onCancel || saving) {
            return;
        }
        setSaving(true);
        setError("");
        try {
            await onCancel(request.id);
        }
        catch (error) {
            setError(userError(error));
        }
        finally {
            setSaving(false);
        }
    }
    return (
        <div className={"bg-white border border-line rounded p-6 border-b-4 " + lineColors[request.status]}>
            <div className="flex justify-between items-start">
                <h2 className="text-xl">{holidayTypeName(request.holidayType)}</h2>
                <span className={"text-sm font-semibold text-white px-3 py-1 rounded-full " + badgeColors[request.status]}>
                    {statusText[request.status]}
                </span>
            </div>

            <p className="mt-2"><strong>From:</strong> {formatDate(request.start)}</p>
            <p><strong>To:</strong> {formatDate(request.end)}</p>
            <p className="text-sm text-slate mt-2">Requested: {formatDate(request.createdAt)}</p>

            <p className="mt-2"><strong>Hours:</strong> {request.hours.toFixed(2)} h</p>
            {request.comment && <p className="mt-2"><strong>Comment:</strong> {request.comment}</p>}
            {error && <p role="alert" className="text-red-600 mt-2">{error}</p>}
            {onCancel && request.status === "pending" && (
                <button
                    disabled={saving}
                    onClick={() => void withdraw()}
                    className="mt-4 w-full border-2 border-red-600 text-red-600 font-semibold py-2 rounded bg-white hover:bg-red-50"
                >
                    {saving ? "Withdrawing..." : "Withdraw request"}
                </button>
            )}

            {onCancel && request.status === "approved" && (
                <button
                    disabled title="Cancellation of approved holiday will be available later"
                    className="mt-4 w-full border-2 border-navy text-navy font-semibold py-2 rounded bg-white hover:bg-cream"
                >
                    Request cancellation
                </button>
            )}
        </div>
    );
}