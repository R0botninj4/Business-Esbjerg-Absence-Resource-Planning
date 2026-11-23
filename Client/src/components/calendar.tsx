import { useEffect, useState } from "react";
import type { CalendarEvent } from "../types/calendarEvent";

type Props = {
    start: Date | null;
    end: Date | null;
    onSelect: (day: Date) => void;
    events?: CalendarEvent[];
    requested?: { start: Date; end: Date; status: string }[];
    dayText?: (day: Date) => string;
};

const weekdays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

const eventColors = {
    holiday: "text-orange",
    companyDayOff: "text-gold",
    info: "text-navy",
};

export function Calendar({ start, end, onSelect, events = [], requested = [], dayText }: Props) {
    const today = new Date();
    const [month, setMonth] = useState(new Date(today.getFullYear(), today.getMonth(), 1));
    
    useEffect(() => {
        if (start) setMonth(new Date(start.getFullYear(), start.getMonth(), 1));
    }, [start]);

    const year = month.getFullYear();
    const monthIndex = month.getMonth();
    const daysInMonth = new Date(year, monthIndex + 1, 0).getDate();
    
    let emptyCells = month.getDay() - 1;
    if (emptyCells === -1) emptyCells = 6;
    
    const days: (Date | null)[] = [];
    for (let i = 0; i < emptyCells; i++) {
        days.push(null);
    }
    for (let d = 1; d <= daysInMonth; d++) {
        days.push(new Date(year, monthIndex, d));
    }

    function changeMonth(step: number) {
        setMonth(new Date(year, monthIndex + step, 1));
    }

    function eventsOn(day: Date) {
        return events.filter(event => event.date.getTime() === day.getTime());
    }

    function dayClass(day: Date) {
        const time = day.getTime();
        const isEdge = (start && time === start.getTime()) || (end && time === end.getTime());
        const isBetween = start && end && time > start.getTime() && time < end.getTime();
        const isWeekend = day.getDay() === 0 || day.getDay() === 6;
        const isDayOff = eventsOn(day).some(event => event.type !== "info");
        const isRequested = requested.some(r => time >= r.start.getTime() && time <= r.end.getTime());
        const holiday = requested.find(r => time >= r.start.getTime() && time <= r.end.getTime());
        
        
        if (isEdge) return "bg-orange text-white";
        if (isBetween) return "bg-orange/20";
        if (isRequested) return "bg-slate/40 text-white";
        if (isWeekend || isDayOff) return "bg-line text-slate";
        if (holiday && holiday.status === "approved") return "bg-navy text-white";
        if (holiday) return "bg-gold text-white";
        return "bg-white hover:bg-orange/10";
    }

    return (
        <div className="w-full">
            <div className="flex items-center justify-between mb-4">
                <button onClick={() => changeMonth(-1)} className="text-3xl px-4 py-1 rounded hover:bg-orange/10 hover:text-orange">←</button>
                <h2 className="text-2xl">
                    {month.toLocaleDateString("en-GB", { month: "long", year: "numeric" })}
                </h2>
                <button onClick={() => changeMonth(1)} className="text-3xl px-4 py-1 rounded hover:bg-orange/10 hover:text-orange">→</button>
            </div>

            <div className="grid grid-cols-7 gap-1">
                {weekdays.map(name => (
                    <div key={name} className="text-center font-semibold pb-2">{name}</div>
                ))}

                {days.map((day, i) =>
                    day ? (
                        <button
                            key={i}
                            onClick={() => onSelect(day)}
                            className={"h-20 border border-line rounded p-2 text-left align-top flex flex-col " + dayClass(day)}
                        >
                            <span>{day.getDate()}</span>
                            {dayText && <span className="text-xs opacity-70">{dayText(day)}</span>}
                            
                            <span>{day.getDate()}</span>
                            {eventsOn(day).map(event => (
                                <span key={event.title} title={event.title} className={"text-xs font-semibold truncate " + eventColors[event.type]}>
                                    {event.title}
                                </span>
                            ))}
                        </button>
                    ) : (
                        <div key={i} />
                    )
                )}
            </div>
        </div>
    );
}
