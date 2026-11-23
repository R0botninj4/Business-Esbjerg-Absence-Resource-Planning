import { useState, type SubmitEvent } from "react";
import { Calendar } from "../../components/calendar";
import type { CalendarEvent } from "../../types/calendarEvent";
import { formatDate } from "../../utils/format";

type Props = {
    events: CalendarEvent[];
    onAdd: (event: CalendarEvent) => void;
    onDelete: (event: CalendarEvent) => void;
};

const typeNames = {
    info: "Info / meeting",
    holiday: "Public holiday",
    companyDayOff: "Company day off",
};

const typeColors = {
    info: "bg-navy/10",
    holiday: "bg-orange/20",
    companyDayOff: "bg-gold/30",
};

export function AdminCalendar({ events, onAdd, onDelete }: Props) {
    const [selectedDay, setSelectedDay] = useState<Date | null>(null);
    const [type, setType] = useState<CalendarEvent["type"]>("info");
    const [title, setTitle] = useState("");
    const [error, setError] = useState("");

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Entries on the selected day
    const entriesOnDay = events.filter(e => selectedDay && e.date.getTime() === selectedDay.getTime());

    // The next 8 entries from today
    const upcoming = events
        .filter(e => e.date >= today)
        .sort((a, b) => a.date.getTime() - b.date.getTime())
        .slice(0, 8);

    function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (!selectedDay) return;

        if (title.trim() === "") {
            setError("Write a title, e.g. \"Personalemøde\" or \"Juledag\".");
            return;
        }

        onAdd({ date: selectedDay, title: title, type: type });
        setTitle("");
        setError("");
    }

    return (
        <div>
            <h1 className="text-3xl">Calendar</h1>
            <p className="text-slate mb-6">
                Click a day to add a meeting or other info, a public holiday or a company day off.
                Public holidays and company days off are never deducted from holiday.
            </p>

            <div className="grid grid-cols-3 gap-6 items-start">
                <div className="col-span-2 bg-white border border-line rounded p-6">
                    <Calendar start={selectedDay} end={null} onSelect={setSelectedDay} events={events} />
                </div>

                <div className="space-y-6">
                    {/* The selected day */}
                    <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
                        {!selectedDay ? (
                            <p className="text-slate">Click a day in the calendar to add something.</p>
                        ) : (
                            <>
                                <h2 className="text-2xl mb-3">
                                    {selectedDay.toLocaleDateString("en-GB", { weekday: "long" })} {formatDate(selectedDay)}
                                </h2>

                                {entriesOnDay.map(entry => (
                                    <div key={entry.title} className="flex justify-between items-center py-1">
                                        <span>
                                            <span className={"text-xs font-semibold px-2 py-1 rounded-full mr-2 " + typeColors[entry.type]}>
                                                {typeNames[entry.type]}
                                            </span>
                                            {entry.title}
                                        </span>
                                        <button onClick={() => onDelete(entry)} className="text-red-600 font-semibold hover:underline">
                                            Delete
                                        </button>
                                    </div>
                                ))}

                                <form onSubmit={handleSubmit} className="mt-4 space-y-3">
                                    <div className="space-y-1">
                                        {(["info", "holiday", "companyDayOff"] as const).map(t => (
                                            <label key={t} className="flex items-center gap-2">
                                                <input type="radio" checked={type === t} onChange={() => setType(t)} />
                                                {typeNames[t]}
                                            </label>
                                        ))}
                                    </div>

                                    <label className="block font-semibold">
                                        Title
                                        <input
                                            value={title}
                                            onChange={e => setTitle(e.target.value)}
                                            placeholder="e.g. Personalemøde kl. 10"
                                            className="mt-1 w-full border border-line rounded p-2 font-normal"
                                        />
                                    </label>

                                    {error && <p className="text-red-600">{error}</p>}

                                    <button type="submit" className="w-full bg-navy text-white font-semibold py-2 rounded hover:bg-navy/90">
                                        Add to calendar
                                    </button>
                                </form>
                            </>
                        )}
                    </div>

                    {/* Coming entries */}
                    <div className="bg-white border border-line rounded p-6">
                        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate mb-3">Coming entries</h2>
                        {upcoming.map(entry => (
                            <div key={entry.date.getTime() + entry.title} className="flex justify-between py-1 border-b border-line last:border-0">
                                <span>{formatDate(entry.date)}</span>
                                <span className="text-slate">{entry.title}</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}