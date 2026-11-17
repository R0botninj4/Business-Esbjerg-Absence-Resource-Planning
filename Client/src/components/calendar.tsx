import { useEffect, useState } from "react";
import type { CalendarEvent } from "../types/calendarEvent";

type Props = {
    start: Date | null;
    end: Date | null;
    onSelect: (day: Date) => void;
    events?: CalendarEvent[];
};

const weekdays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

const eventColors = {
    holiday: "text-orange",
    companyDayOff: "text-gold",
    info: "text-navy",
};

export function Calendar({ start, end, onSelect, events = [] }: Props) {
    const today = new Date();
    const [month, setMonth] = useState(new Date(today.getFullYear(), today.getMonth(), 1));

    // Jump to the start date's month (e.g. when it is typed in the text field)
    useEffect(() => {
        if (start) setMonth(new Date(start.getFullYear(), start.getMonth(), 1));
    }, [start]);

    const year = month.getFullYear();
    const monthIndex = month.getMonth();
    const daysInMonth = new Date(year, monthIndex + 1, 0).getDate();
    const emptyCells = (month.getDay() + 6) % 7; // Monday = 0, Sunday = 6

    const days: (Date | null)[] = [
        ...Array(emptyCells).fill(null),
        ...Array.from({ length: daysInMonth }, (_, i) => new Date(year, monthIndex, i + 1)),
    ];

    function changeMonth(step: number) {
        setMonth(new Date(year, monthIndex + step, 1));
    }

    function eventsOn(day: Date) {
        return events.filter(event => event.date.getTime() === day.getTime());
    }

    function dayClass(day: Date) {
        const time = day.getTime();
        const isEdge = time === start?.getTime() || time === end?.getTime();
        const isBetween = start && end && time > start.getTime() && time < end.getTime();
        const isWeekend = day.getDay() === 0 || day.getDay() === 6;
        const isDayOff = eventsOn(day).some(event => event.type !== "info");

        if (isEdge) return "bg-orange text-white";
        if (isBetween) return "bg-orange/20";
        if (isWeekend || isDayOff) return "bg-line text-slate";
        return "bg-white hover:bg-orange/10";
    }

    return (
        <div className="max-w-2xl">
            <div className="flex items-center justify-between mb-4">
                <button onClick={() => changeMonth(-1)} className="px-3 py-1 hover:text-orange">←</button>
                <h2 className="text-2xl">
                    {month.toLocaleDateString("en-GB", { month: "long", year: "numeric" })}
                </h2>
                <button onClick={() => changeMonth(1)} className="px-3 py-1 hover:text-orange">→</button>
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
