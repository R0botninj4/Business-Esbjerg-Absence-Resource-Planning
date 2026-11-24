import type { HolidayRequest } from "../types/holidayRequest";
import { RequestCard } from "./requestCard";

type Props = {
    title: string;
    requests: HolidayRequest[];
    onCancel?: (id: string) => Promise<void>; // optional: without it, the cards have no buttons
};

// One row of request cards that scrolls sideways when there are many
export function RequestRow({ title, requests, onCancel }: Props) {
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