import { RequestRow } from "../../components/requestRow";
import type { HolidayRequest } from "../../types/holidayRequest";

type Props = {
    requests: HolidayRequest[];
    onCancel: (id: number) => void;
};

export function UserRequests({ requests, onCancel }: Props) {
    // Newest first: sort by when the request was created
    const sorted = [...requests].sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());

    const pending = sorted.filter(r => r.status === "pending");
    const approved = sorted.filter(r => r.status === "approved");
    const closed = sorted.filter(r => r.status === "declined" || r.status === "cancelled");

    return (
        <div>
            <h1 className="text-3xl mb-6">My requests</h1>

            <RequestRow title="Pending" requests={pending} onCancel={onCancel} />
            <RequestRow title="Approved" requests={approved} onCancel={onCancel} />
            <RequestRow title="Declined & cancelled" requests={closed} onCancel={onCancel} />
        </div>
    );
}