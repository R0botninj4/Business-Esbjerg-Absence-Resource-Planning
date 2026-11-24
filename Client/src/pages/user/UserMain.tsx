import { Link } from "react-router";

import { InfoBox } from "../../components/infoBox";
import { RequestCard } from "../../components/requestCard";
import { PostedBalance } from "../../components/postedBalance";
import { useAuth } from "../../components/auth";

import type { HolidayRequest } from "../../types/holidayRequest";
import { formatDate } from "../../utils/format";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    onCancel: (id: string) => Promise<void>;
    events: CalendarEvent[];
};

export function UserMain({ requests, onCancel, events }: Props) {
    const { user } = useAuth();
    const latest = [...requests]
        .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
        .slice(0, 5);
    
    const today = new Date();
    const comingDaysOff = events
        .filter(event => event.type !== "info" && event.date >= today)
        .sort((a, b) => a.date.getTime() - b.date.getTime())
        .slice(0, 5);
    

    return (
        <div>
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl">Overview</h1>
                <Link
                    to="/user/request"
                    className="bg-orange-light text-white font-semibold px-6 py-3 rounded hover:bg-orange"
                >
                    + Request holiday
                </Link>
            </div>

            <div className="mb-6 bg-white border border-line rounded p-6">
                {user && <PostedBalance userId={user.id} />}
            </div>
            {/* Info boxes */}
            <div className="grid grid-cols-3 gap-6 mb-8">
                <InfoBox title="Main holiday (hovedferie)">
                    <p className="text-slate">Main holiday status will be shown when approved holiday and the applicable rules are connected.</p>
                </InfoBox>

                <InfoBox title="Coming days off – never deducted">
                    {comingDaysOff.map(event => (
                        <div key={event.title} className="flex justify-between py-1">
                            <span>{event.date.toLocaleDateString("en-GB", { weekday: "short", day: "numeric", month: "short" })}</span>
                            <span className="text-slate">{event.title}</span>
                        </div>
                    ))}
                </InfoBox>

                <InfoBox title="Ill before or during holiday?">
                    {/* TODO: decide what goes here */}
                </InfoBox>
            </div>

            {/* Requests latest */}
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-2xl">My requests</h2>
                <Link to="/user/requests" className="font-semibold hover:text-orange">
                    See all →
                </Link>
            </div>

            {latest.length === 0 ? (
                <p className="text-slate">No requests yet</p>
            ) : (
                <div className="grid grid-cols-5 gap-4">
                    {latest.map(request => (
                        <RequestCard key={request.id} request={request} onCancel={onCancel} />
                    ))}
                </div>
            )}
        </div>
    );
}
