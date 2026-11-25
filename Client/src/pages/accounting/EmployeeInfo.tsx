import { useState } from "react";
import { Link, useParams } from "react-router";
import { Calendar } from "../../components/calendar";
import { ScheduleForm } from "../../components/scheduleForm";
import { FutureBalance } from "../../components/futureBalance";
import { OpeningBalanceForm } from "../../components/openingBalanceForm";
import { useAuth } from "../../components/auth";
import { useEmployees } from "../../components/employeeData";
import { getEmployments, userError } from "../../api/users";
import { saveSchedule } from "../../api/schedules";
import type { SchedulePeriod } from "../../types/employee";
import { formatDate, displayName } from "../../utils/format";
import { employmentText, hoursOfDay, scheduleOn, weekdayNames } from "../../utils/schedule";
import type { CalendarEvent } from "../../types/calendarEvent";
import type { HolidayRequest } from "../../types/holidayRequest";

export function EmployeeInfo({ events, requests = [] }: { events: CalendarEvent[]; requests?: HolidayRequest[] }) {
    const { id } = useParams();
    const { user } = useAuth();
    const { employees, reload } = useEmployees();
    const [editing, setEditing] = useState(false);
    const [success, setSuccess] = useState("");
    const [error, setError] = useState("");
    const employee = employees.find(employee => employee.id === id);
    if (!employee) return <p>Employee not found.</p>;
    const currentDays = scheduleOn(employee, new Date());
    const employeeRequests = requests.filter(request => request.employeeId === String(employee.id) &&
        (request.status === "approved" || request.status === "pending"));

    async function changeSchedule(period: SchedulePeriod) {
        if (!employee) return;
        const employments = await getEmployments(String(employee.id));
        const date = period.validFrom;
        const employment = employments.find(item => new Date(item.startDate + "T00:00") <= date && (!item.endDate || new Date(item.endDate + "T23:59:59.999") >= date));
        if (!employment) throw new Error("There is no employment on the selected date.");
        await saveSchedule(employment, employee.schedules, period);
        setEditing(false);
        setSuccess("The schedule has been saved. Earlier versions are kept.");
        try {
            await reload();
            setError("");
        } catch (error) {
            setError("The schedule is saved, but the list could not be refreshed. " + userError(error));
        }
    }

    function workingHours(date: Date) {
        if (!employee || date.getDay() === 0 || date.getDay() === 6) return "";
        if (events.some(event => event.type !== "info" && event.date.toDateString() === date.toDateString())) return "";
        const holiday = employeeRequests.find(request => date >= request.start && date <= request.end);
        if (holiday?.status === "approved") return "Holiday";
        if (holiday?.status === "pending") return "Pending holiday";
        const day = scheduleOn(employee, date)[date.getDay() - 1];
        return day ? day.from + "–" + day.to : "Not working";
    }

    return <div>
        <Link to="../employees" className="font-semibold">← All employees</Link>
        <h1 className="text-3xl mt-4 mb-4">{displayName(employee.email)}</h1>
        {success && <p role="status" className="mb-4">{success}</p>}
        {error && <p role="alert" className="mb-4 text-red-600">{error}</p>}
        <div className="bg-white border border-line rounded p-6 mb-6">
            <div className="flex justify-between mb-4"><h2 className="text-2xl">Work schedule today</h2>
                {!editing && <button onClick={() => { setEditing(true); setSuccess(""); }} className="font-semibold">New schedule version</button>}
            </div>
            <div className="grid grid-cols-5 gap-2">{currentDays.map((day, index) => <div key={index} className="border border-line rounded p-3">
                <p className="font-semibold">{weekdayNames[index]}</p><p>{day ? day.from + "–" + day.to : "Not working"}</p><p>{hoursOfDay(day).toFixed(2)} h</p>
            </div>)}</div>
            {editing && <ScheduleForm startDays={currentDays} onSave={changeSchedule} onClose={() => setEditing(false)} />}
            <h3 className="text-xl mt-6">Schedule history</h3>
            {employee.schedules.length === 0 && <p>No work schedules have been saved.</p>}
            {[...employee.schedules].reverse().map(period => <div key={period.id} className="border-b border-line py-3">
                <p>{formatDate(period.validFrom)} – {period.validTo ? formatDate(period.validTo) : "onwards"} · {employmentText(period.days, period.employmentType)}</p>
                <p className="text-sm text-slate">{period.days.map((day, index) => weekdayNames[index] + ": " + (day ? day.from + "–" + day.to : "off")).join(" · ")}</p>
            </div>)}
        </div>
        <Calendar start={null} end={null} onSelect={() => {}} events={events} requested={employeeRequests} dayText={workingHours} />
        <div className="mt-6 bg-white border border-line rounded p-6">
            <FutureBalance key={employee.id} userId={String(employee.id)} />
        </div>
        {user?.role === "Admin" && <OpeningBalanceForm key={employee.id} userId={String(employee.id)} />}
    </div>;
}
