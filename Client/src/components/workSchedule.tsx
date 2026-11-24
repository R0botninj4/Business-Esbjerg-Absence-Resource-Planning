import { useEffect, useState, type SubmitEvent } from "react";
import type { CalendarEvent } from "../types/calendarEvent";
import type { Employee } from "../types/employee";
import type { HolidayRequest } from "../types/holidayRequest";
import { mondayOfWeek, startOfWeek, weekdayIndex, weekNumber, weekYear } from "../utils/dates";
import { formatDate, displayName } from "../utils/format";
import { scheduleOn } from "../utils/schedule";

type Props = {
    employees: Employee[];
    requests: HolidayRequest[];
    events: CalendarEvent[];
    selected?: HolidayRequest;
};

type Cell = {
    text: string;
    color: string;
    working: boolean;
};

export function WorkSchedule({ employees, requests, events, selected }: Props) {
    const [monday, setMonday] = useState(startOfWeek(selected ? selected.start : new Date()));
    const [weekInput, setWeekInput] = useState("");
    const [yearInput, setYearInput] = useState("");
    
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

    function getCell(employee: Employee, day: Date): Cell {
        const time = day.getTime();
        
        const dayOff = events.find(e => e.type !== "info" && e.date.getTime() === time);
        if (dayOff) return { text: dayOff.title, color: "bg-line text-slate", working: false };
        
        if (day.getDay() === 0 || day.getDay() === 6) return { text: "", color: "bg-line", working: false };
        
        const holiday = requests.find(
            r =>
                r.employeeId === String(employee.id) &&
                (r.status === "pending" || r.status === "approved") &&
                time >= r.start.getTime() &&
                time <= r.end.getTime()
        );
        if (holiday && selected && holiday.id === selected.id) return { text: "This request", color: "bg-orange text-white font-semibold", working: false };
        if (holiday && holiday.status === "pending") return { text: "Pending", color: "bg-gold text-white font-semibold", working: false };
        if (holiday) return { text: "Holiday", color: "bg-navy text-white font-semibold", working: false };

        const workDay = scheduleOn(employee, day)[weekdayIndex(day)];
        if (!workDay) return { text: "Not working", color: "bg-line text-slate", working: false };
        
        return { text: workDay.from + "–" + workDay.to, color: "bg-white", working: true };
    }

    function presentCount(day: Date) {
        return employees.filter(employee => getCell(employee, day).working).length;
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
                {employees.map(employee => (
                    <tr key={employee.name}>
                            <td className={"pr-2 " + (String(employee.id) === selected?.employeeId ? "font-bold" : "")}>
                            {displayName(employee.name)}
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
                ))}

                <tr>
                    <td className="pt-2 font-semibold">Present</td>
                    {days.map(day => {
                        const count = presentCount(day);
                        return (
                            <td key={day.getTime()} className="pt-2 text-center font-semibold">
                                {count === 0 ? "–" : count + " / " + employees.length}
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
