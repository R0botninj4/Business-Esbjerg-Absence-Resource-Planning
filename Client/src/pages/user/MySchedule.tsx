import { useEffect, useState } from "react";
import { useAuth } from "../../components/auth";
import { WorkSchedule } from "../../components/workSchedule";
import { loadEmployee } from "../../api/schedules";
import { userError } from "../../api/users";
import type { CalendarEvent } from "../../types/calendarEvent";
import type { Employee } from "../../types/employee";
import type { HolidayRequest } from "../../types/holidayRequest";
import { formatDate } from "../../utils/format";
import { employmentText, weekdayNames } from "../../utils/schedule";

type Props = {
    events: CalendarEvent[];
    requests: HolidayRequest[]; // the logged-in employee's own requests
};

// The employee's "Work schedule" page: the shared week overview and their own schedule history
export function MySchedule({ events, requests }: Props) {
    const { user } = useAuth();
    const [employee, setEmployee] = useState<Employee | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // TODO (#94): also load the colleagues and their approved holiday, when the backend allows it
    useEffect(() => {
        if (!user) return;
        loadEmployee({ id: user.id, email: user.email, role: "Employee", isActive: true })
            .then(setEmployee)
            .catch(error => setError(userError(error)))
            .finally(() => setLoading(false));
    }, [user?.id]);

    if (loading) return <p>Loading your schedule...</p>;
    if (error) return <p role="alert" className="text-red-600">{error}</p>;
    if (!user || !employee || employee.schedules.length === 0) {
        return <p>No work schedule has been saved for your account.</p>;
    }

    const today = new Date();
    const history = [...employee.schedules].reverse(); // newest first

    return (
        <div>
            <p className="mb-4 text-slate">Who is at work and who is on holiday. Your own row is highlighted.</p>

            <WorkSchedule employees={[employee]} requests={requests} events={events} viewerId={user.id} />

            <h2 className="text-2xl mt-8 mb-4">My schedule history</h2>
            <div className="grid grid-cols-3 gap-4">
                {history.map(period => {
                    const isCurrent = period.validFrom <= today && (!period.validTo || today <= period.validTo);
                    return (
                        <div key={period.id} className={"bg-white border border-line rounded p-5 " + (isCurrent ? "border-b-4 border-b-navy" : "")}>
                            <div className="flex justify-between items-start mb-2">
                                <strong>{formatDate(period.validFrom)} – {period.validTo ? formatDate(period.validTo) : "onwards"}</strong>
                                {isCurrent && <span className="text-xs font-semibold text-white bg-navy px-2 py-1 rounded-full">Current</span>}
                                {period.validFrom > today && <span className="text-xs font-semibold text-white bg-gold px-2 py-1 rounded-full">Upcoming</span>}
                            </div>
                            <p className="text-sm text-slate mb-2">{employmentText(period.days, period.employmentType)}</p>
                            {period.days.map((day, index) => (
                                <div key={index} className="flex justify-between py-0.5">
                                    <span>{weekdayNames[index]}</span>
                                    <span className={day ? "" : "text-slate"}>{day ? day.from + "–" + day.to : "Not working"}</span>
                                </div>
                            ))}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}