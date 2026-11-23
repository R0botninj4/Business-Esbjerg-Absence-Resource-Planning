import { Link } from "react-router";
import { BalanceCard } from "../../components/balanceCard";
import { InfoBox } from "../../components/infoBox";
import { RequestCard } from "../../components/requestCard";
import { testBalances } from "../../mockdata/testBalances";
import { testMainHoliday } from "../../mockdata/testMainHoliday";
import type { HolidayRequest } from "../../types/holidayRequest";
import { formatDate } from "../../utils/format";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    onCancel: (id: number) => void;
    events: CalendarEvent[];
};

export function UserMain({ requests, onCancel, events }: Props) {
    const latest = [...requests]
        .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
        .slice(0, 5);
    
    const today = new Date();
    const comingDaysOff = events
        .filter(event => event.type !== "info" && event.date >= today)
        .sort((a, b) => a.date.getTime() - b.date.getTime())
        .slice(0, 5);
    
    const nextYear = today.getFullYear() + 1;

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

            {}
            <div className="grid grid-cols-2 gap-6 mb-8">
                <BalanceCard
                    title="Week 1–5"
                    hours={testBalances.weeks1to5.hours}
                    useBy={testBalances.weeks1to5.useBy}
                    lineColor="border-b-navy"
                />
                <BalanceCard
                    title="Week 6"
                    hours={testBalances.week6.hours}
                    useBy={testBalances.week6.useBy}
                    lineColor="border-b-slate"
                />
            </div>

            {/* Info boxes */}
            <div className="grid grid-cols-3 gap-6 mb-8">
                <InfoBox title="Main holiday (hovedferie)">
                    {testMainHoliday ? (
                        <p>
                            <span className="text-sm font-semibold bg-navy text-white px-3 py-1 rounded-full mr-2">
                                ✓ {testMainHoliday.start.getFullYear()} done
                            </span>
                            {formatDate(testMainHoliday.start)} – {formatDate(testMainHoliday.end)}
                        </p>
                    ) : (
                        <p className="font-semibold">Not planned yet</p>
                    )}
                    <p className="text-slate mt-3">
                        Next: 3 weeks between 1 May and 30 Sep {nextYear}.
                        Request at least 3 months ahead – for a start on 1 May, by {formatDate(new Date(nextYear, 1, 1))}.
                    </p>
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