import { RequestCard } from "../../components/requestCard";
import type { HolidayRequest } from "../../types/holidayRequest";

type Props = {
    requests: HolidayRequest[];
    onCancel: (id: number) => void;
};

export function UserRequests({ requests, onCancel }: Props) {
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

type RowProps = {
    title: string;
    requests: HolidayRequest[];
    onCancel: (id: number) => void;
};

function RequestRow({ title, requests, onCancel }: RowProps) {
    return (
        <section className="mb-8">
            <h2 className="text-2xl mb-4">{title} ({requests.length})</h2>

            {requests.length === 0 ? (
                <p className="text-slate">No requests</p>
            ) : (
                <div className="flex gap-4 overflow-x-auto pb-4">
                    {requests.map(request => (
                        <div key={request.id} className="w-80 shrink-0">
                            <RequestCard request={request} onCancel={onCancel} />
                        </div>
                    ))}
                </div>
            )}
        </section>
    );
}