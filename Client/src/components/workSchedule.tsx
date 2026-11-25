import { useEffect, useState, type SubmitEvent } from "react";
import type { CalendarEvent } from "../types/calendarEvent";
import type { Employee } from "../types/employee";
import type { HolidayRequest } from "../types/holidayRequest";
import { mondayOfWeek, startOfWeek, weekdayIndex, weekNumber, weekYear } from "../utils/dates";
import { formatDate, displayName } from "../utils/format";
import { scheduleOn } from "../utils/schedule";
import { api } from "../api/client";
import type { ColleagueCalendarResult } from "../api/Api";
import { dateText } from "../api/schedules";
import { userError } from "../api/users";

type Props = {
    employees: Employee[];
    requests: HolidayRequest[];
    events: CalendarEvent[];
    selected?: HolidayRequest;
    viewerId?: string; // set on the employee's page: the logged-in employee (gets highlighted)
};

type Cell = {
    text: string;
    color: string;
    working: boolean;
};
type ScheduleEmployee = {
    id: string | number;
    name: string;
};

export function WorkSchedule({ employees, requests, events, selected, viewerId }: Props) {
    const [monday, setMonday] = useState(startOfWeek(selected ? selected.start : new Date()));
    const [weekInput, setWeekInput] = useState("");
    const [yearInput, setYearInput] = useState("");
    const [calendar, setCalendar] = useState<ColleagueCalendarResult | null>(null);
    const [calendarLoading, setCalendarLoading] = useState(false);
    const [calendarError, setCalendarError] = useState("");

    useEffect(() => {
        // Medarbejdervisningen bruger det fælles API. Regnskabets visning har sine egne data.
        if (!viewerId) return;
        let cancelled = false;
        async function loadCalendar() {
            setCalendarLoading(true);
            setCalendarError("");
            setCalendar(null);
            const end = new Date(monday.getFullYear(), monday.getMonth(), monday.getDate() + 6);
            try {
                const saved = await api.request<ColleagueCalendarResult>({
                    path: "/api/colleague-calendar",
                    method: "GET", format: "json",
                    query: { startDate: dateText(monday), endDate: dateText(end) },
                });
                if (!cancelled) setCalendar(saved);
            } catch (error) {
                if (!cancelled) setCalendarError(userError(error));
            } finally {
                if (!cancelled) setCalendarLoading(false);
            }
        }
        void loadCalendar();
        return () => { cancelled = true; };
    }, [monday, viewerId]);

    useEffect(() => {
        if (selected) setMonday(startOfWeek(selected.start));
    }, [selected]);

    useEffect(() => {
        setWeekInput(String(weekNumber(monday)));
        setYearInput(String(weekYear(monday)));
    }, [monday]);

    const days: Date[] = [];
    for (let i = 0; i < 7; i++) {
        days.push(new Date(monday.getFullYear(), monday.getMonth(), monday.getDate() + i));
    }
    const sunday = days[6]!;

    function changeWeek(step: number) {
        setMonday(new Date(monday.getFullYear(), monday.getMonth(), monday.getDate() + step * 7));
    }

    function goToWeek(e: SubmitEvent) {
        e.preventDefault();

        const week = Number(weekInput);
        const year = Number(yearInput);
        const newMonday = mondayOfWeek(year, week);

        if (!week || !year || weekNumber(newMonday) !== week) {
            setWeekInput(String(weekNumber(monday)));
            setYearInput(String(weekYear(monday)));
            return;
        }

        setMonday(newMonday);
    }

    function getSharedCell(employee: ScheduleEmployee, day: Date): Cell {
        const savedDay = calendar?.days?.find(item => item.date === dateText(day));
        const savedCell = savedDay?.employees?.find(item => item.employeeId === String(employee.id));
        if (!savedCell) return { text: "–", color: "bg-line text-slate", working: false };
        if (savedCell.status === "holiday") return { text: "Holiday", color: "bg-navy text-white font-semibold", working: false };
        if (savedCell.status === "unavailable") return { text: "Unavailable", color: "bg-line text-slate", working: false };
        if (savedCell.status === "missingSchedule") return { text: "Missing schedule", color: "bg-gold text-white", working: false };
        if (savedCell.status !== "working") return { text: "Not working", color: "bg-line text-slate", working: false };
        if (String(employee.id) !== viewerId) return { text: "At work", color: "bg-white", working: true };
        const pending = requests.some(request => request.status === "pending" &&
            request.employeeId === viewerId && day >= request.start && day <= request.end);
        if (pending) return { text: "Pending", color: "bg-gold text-white font-semibold", working: true };
        const from = savedCell.startTime?.slice(0, 5) ?? "";
        const to = savedCell.endTime?.slice(0, 5) ?? "";
        return { text: from + "–" + to, color: "bg-white", working: true };
    }

    function getCell(employee: ScheduleEmployee, day: Date): Cell {
        if (viewerId) return getSharedCell(employee, day);
        const time = day.getTime();

        const dayOff = events.find(e => e.type !== "info" && e.date.getTime() === time);
        if (dayOff) return { text: dayOff.title, color: "bg-line text-slate", working: false };

        if (day.getDay() === 0 || day.getDay() === 6) return { text: "", color: "bg-line", working: false };

        const holiday = requests.find(
            r =>
                r.employeeId === String(employee.id) &&
                (r.status === "approved" || r.status === "pending") &&
                time >= r.start.getTime() &&
                time <= r.end.getTime()
        );
        if (holiday && selected && holiday.id === selected.id) return { text: "This request", color: "bg-orange text-white font-semibold", working: false };
        if (holiday && holiday.status === "pending") return { text: "Pending", color: "bg-gold text-white font-semibold", working: false };
        if (holiday) return { text: "Holiday", color: "bg-navy text-white font-semibold", working: false };

        const fullEmployee = employees.find(item => item.id === employee.id);
        if (!fullEmployee) return { text: "Not working", color: "bg-line text-slate", working: false };
        const workDay = scheduleOn(fullEmployee, day)[weekdayIndex(day)];
        if (!workDay) return { text: "Not working", color: "bg-line text-slate", working: false };

        return { text: workDay.from + "–" + workDay.to, color: "bg-white", working: true };
    }

    // The logged-in employee is shown first, so they don't have to look for themselves
    let visibleEmployees: ScheduleEmployee[] = employees;
    if (viewerId) {
        visibleEmployees = [];
        for (const colleague of calendar?.employees ?? []) {
            if (colleague.id && colleague.name) visibleEmployees.push({ id: colleague.id, name: colleague.name });
        }
    }
    const me = visibleEmployees.filter(e => String(e.id) === viewerId);
    const others = visibleEmployees.filter(e => String(e.id) !== viewerId);
    const rows = [...me, ...others];

    function presentLabel(day: Date) {
        if (viewerId) {
            const savedDay = calendar?.days?.find(item => item.date === dateText(day));
            if (!savedDay || Number(savedDay.scheduledCount) === 0) return "–";
            return savedDay.presentCount + " / " + savedDay.scheduledCount;
        }
        const count = employees.filter(employee => getCell(employee, day).working).length;
        if (count === 0) return "–";
        return count + " / " + employees.length;
    }

    return (
        <div className="bg-white border border-line rounded p-6">
            {/* Header: title (left), previous/next week (middle), week picker (right) */}
            <div className="grid grid-cols-3 items-center mb-4">
                <h2 className="text-2xl">Work schedule</h2>

                <div className="flex items-center justify-center gap-2">
                    <button onClick={() => changeWeek(-1)} className="text-3xl px-4 py-1 rounded hover:bg-orange/10 hover:text-orange">←</button>
                    <span className="font-semibold text-lg text-center">
                        Week {weekNumber(monday)} - {formatDate(monday)} - {formatDate(sunday)}
                    </span>
                    <button onClick={() => changeWeek(1)} className="text-3xl px-4 py-1 rounded hover:bg-orange/10 hover:text-orange">→</button>
                </div>

                <form onSubmit={goToWeek} className="flex items-center justify-end gap-2">
                    <label className="font-semibold">
                        Week{" "}
                        <input
                            type="number"
                            value={weekInput}
                            onChange={e => setWeekInput(e.target.value)}
                            className="w-16 border border-line rounded p-2 font-normal"
                        />
                    </label>
                    <label className="font-semibold">
                        Year{" "}
                        <input
                            type="number"
                            value={yearInput}
                            onChange={e => setYearInput(e.target.value)}
                            className="w-20 border border-line rounded p-2 font-normal"
                        />
                    </label>
                    <button type="submit" className="bg-navy text-white font-semibold px-4 py-2 rounded hover:bg-navy/90">Go</button>
                </form>
            </div>

            {calendarLoading && <p role="status">Loading colleagues and approved holiday...</p>}
            {calendarError && <p role="alert" className="text-red-600">{calendarError}</p>}
            {!calendarLoading && !calendarError && viewerId && rows.length === 0 && <p>No employees scheduled in this period.</p>}
            <table className="w-full table-fixed text-sm">
                <thead>
                <tr>
                    <th className="w-36"></th>
                    {days.map(day => (
                        <th key={day.getTime()} className="pb-2">
                            {day.toLocaleDateString("en-GB", { weekday: "short" })} {day.getDate()}.{day.getMonth() + 1}
                        </th>
                    ))}
                </tr>
                </thead>

                <tbody>
                {rows.map(employee => {
                    const isViewer = String(employee.id) === viewerId;
                    return (
                        <tr key={employee.name} className={isViewer ? "bg-orange/10" : ""}>
                            <td className={"pr-2 pl-2 " + (isViewer || String(employee.id) === selected?.employeeId ? "font-bold" : "")}>
                                {displayName(employee.name)}
                                {isViewer && <span className="text-orange"> (you)</span>}
                            </td>
                            {days.map(day => {
                                const cell = getCell(employee, day);
                                return (
                                    <td key={day.getTime()} className="p-0.5">
                                        <div className={"h-11 rounded border border-line flex items-center justify-center text-center px-1 " + cell.color}>
                                            {cell.text}
                                        </div>
                                    </td>
                                );
                            })}
                        </tr>
                    );
                })}

                <tr>
                    <td className="pt-2 font-semibold">Present</td>
                    {days.map(day => {
                        return (
                            <td key={day.getTime()} className="pt-2 text-center font-semibold">
                                {presentLabel(day)}
                            </td>
                        );
                    })}
                </tr>
                </tbody>
            </table>

            <div className="flex gap-5 mt-4 text-sm">
                {selected && (
                    <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-orange"></span>This request</span>
                )}
                <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-gold"></span>{selected ? "Other pending" : "Pending"}</span>
                <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-navy"></span>Approved holiday</span>
                <span className="flex items-center gap-2"><span className="w-4 h-4 rounded bg-line"></span>Not working / weekend / day off</span>
            </div>
        </div>
    );
}
