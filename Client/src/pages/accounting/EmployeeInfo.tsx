import { useState } from "react";
import { Link, useParams } from "react-router";
import { BalanceCard } from "../../components/balanceCard";
import { Calendar } from "../../components/calendar";
import { InfoBox } from "../../components/infoBox";
import { RequestRow } from "../../components/requestRow";
import { ScheduleForm } from "../../components/scheduleForm";
import { testBalances } from "../../mockdata/testBalances";
import { testEvents } from "../../mockdata/testEvents";
import type { Employee, SchedulePeriod } from "../../types/employee";
import type { HolidayRequest } from "../../types/holidayRequest";
import { formatDate } from "../../utils/format";
import { weekdayIndex } from "../../utils/dates";
import { employmentText, hoursOfDay, scheduleOn, weekdayNames } from "../../utils/schedule";

type Props = {
    employees: Employee[];
    requests: HolidayRequest[];
    onAddSchedule: (employeeId: number, period: SchedulePeriod) => void;
};

export function EmployeeInfo({ employees, requests, onAddSchedule }: Props) {
    const { id } = useParams(); // the number in the URL, e.g. /accounting/employees/3
    const [editing, setEditing] = useState(false);

    const employee = employees.find(e => e.id === Number(id));

    if (!employee) {
        return (
            <div>
                <Link to="/accounting/employees" className="font-semibold hover:text-orange">← All employees</Link>
                <p className="mt-4">Employee not found.</p>
            </div>
        );
    }

    const currentDays = scheduleOn(employee, new Date());

    // This employee's requests, newest first, split into the same three rows as "My requests"
    const theirs = requests
        .filter(r => r.employeeName === employee.name)
        .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
    const pending = theirs.filter(r => r.status === "pending");
    const approved = theirs.filter(r => r.status === "approved");
    const closed = theirs.filter(r => r.status === "declined" || r.status === "cancelled");

    // The holidays shown in the calendar: pending and approved
    const booked = theirs.filter(r => r.status === "pending" || r.status === "approved");

    // Working hours shown in the calendar. Empty on weekends, days off and holidays.
    function workingHours(day: Date) {
        const time = day.getTime();
        const index = weekdayIndex(day);
        const isDayOff = testEvents.some(e => e.type !== "info" && e.date.getTime() === time);
        const isHoliday = booked.some(r => time >= r.start.getTime() && time <= r.end.getTime());
        if (index > 4 || isDayOff || isHoliday) return "";

        const workDay = scheduleOn(employee!, day)[index];
        return workDay ? workDay.from + "–" + workDay.to : "Not working";
    }

    // The day before the next period starts, or null for the newest period
    function periodEnd(index: number) {
        const next = employee!.schedules[index + 1];
        if (!next) return null;
        return new Date(next.validFrom.getFullYear(), next.validFrom.getMonth(), next.validFrom.getDate() - 1);
    }

    return (
        <div>
            <Link to="/accounting/employees" className="font-semibold hover:text-orange">← All employees</Link>

            <h1 className="text-3xl mt-4">{employee.name}</h1>
            <p className="text-slate mb-6">{employee.email} - {employmentText(currentDays)}</p>

            {/* Balances (mock data – the same for everyone until the backend calculates them) */}
            <div className="grid grid-cols-2 gap-6 mb-8">
                <BalanceCard title="Week 1–5" hours={testBalances.weeks1to5.hours} useBy={testBalances.weeks1to5.useBy} lineColor="border-b-navy" />
                <BalanceCard title="Week 6" hours={testBalances.week6.hours} useBy={testBalances.week6.useBy} lineColor="border-b-slate" />
            </div>

            {/* Work schedule (full width, history at the bottom) */}
            <div className="bg-white border border-line rounded p-6 mb-8">
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-2xl">Work schedule</h2>
                    {!editing && (
                        <button onClick={() => setEditing(true)} className="border border-navy font-semibold px-4 py-2 rounded hover:bg-cream">
                            Edit schedule
                        </button>
                    )}
                </div>

                <div className="grid grid-cols-5 gap-2">
                    {currentDays.map((day, index) => (
                        <div key={index} className={"rounded border border-line p-3 text-center " + (day ? "bg-white" : "bg-line text-slate")}>
                            <p className="font-semibold">{weekdayNames[index]}</p>
                            <p>{day ? day.from + "–" + day.to : "Not working"}</p>
                            <p className="text-sm text-slate">{hoursOfDay(day).toFixed(2)} h</p>
                        </div>
                    ))}
                </div>

                {editing && (
                    <ScheduleForm
                        startDays={currentDays}
                        onSave={period => onAddSchedule(employee.id, period)}
                        onClose={() => setEditing(false)}
                    />
                )}

                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate mt-6 mb-2">Schedule history</h3>
                {employee.schedules.map((period, index) => {
                    const endDate = periodEnd(index);
                    return (
                        <div key={period.validFrom.getTime()} className="flex gap-8 py-1 border-b border-line last:border-0">
                            <span className="w-64">{formatDate(period.validFrom)} – {endDate ? formatDate(endDate) : "onwards"}</span>
                            <span className="text-slate">{employmentText(period.days)}</span>
                        </div>
                    );
                }).reverse()}
            </div>

            {/* Calendar (read only) beside the requests */}
            <div className="grid grid-cols-5 gap-8 mb-8 items-start">
                <div className="col-span-3 bg-white border border-line rounded p-6">
                    <h2 className="text-2xl mb-4">Calendar</h2>
                    <Calendar start={null} end={null} onSelect={() => {}} events={testEvents} requested={booked} dayText={workingHours} />
                    <div className="flex gap-5 mt-4 text-sm">
                        <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-gold"></span>Pending</span>
                        <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-navy"></span>Approved</span>
                    </div>
                </div>

                {/* Requests (no buttons – accounting decides on the Approvals page) */}
                <div className="col-span-2 min-w-0">
                    <RequestRow title="Pending" requests={pending} />
                    <RequestRow title="Approved" requests={approved} />
                    <RequestRow title="Declined & cancelled" requests={closed} />
                </div>
            </div>

            {/* Room for later features */}
            <div className="grid grid-cols-2 gap-6">
                <InfoBox title="Sickness & holiday obstacles">
                    <p className="text-slate">Coming later (#10).</p>
                </InfoBox>
                <InfoBox title="Transfer agreements">
                    <p className="text-slate">Coming later (#6). Transfer is currently turned off by company policy.</p>
                </InfoBox>
            </div>
        </div>
    );
}