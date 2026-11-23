import { useState, type SubmitEvent } from "react";
import { Calendar } from "../../components/calendar";
import { useCalendar } from "../../components/calendarData";
import { saveCalendarDay } from "../../api/calendar";
import { dateText } from "../../api/schedules";
import { userError } from "../../api/users";

export function AdminCalendar() {
    const { history, events, reload, loading, error: loadError } = useCalendar();
    const [selected, setSelected] = useState<Date | null>(null);
    const [name, setName] = useState("");
    const [usesHolidayHours, setUsesHolidayHours] = useState(false);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const date = selected ? dateText(selected) : "";
    const versions = history.filter(day => day.date === date).sort((a, b) => b.version - a.version);

    function select(date: Date) {
        setSelected(date);
        const latest = history.filter(day => day.date === dateText(date)).sort((a, b) => b.version - a.version)[0];
        setName(latest?.name || "");
        setUsesHolidayHours(latest?.usesHolidayHours ?? false);
        setError("");
        setSuccess("");
    }

    async function save(event: SubmitEvent) {
        event.preventDefault();
        if (!selected || saving) return;
        setSaving(true);
        setError("");
        setSuccess("");
        try {
            await saveCalendarDay(date, name, usesHolidayHours);
            setSuccess("New calendar version saved. Previous versions are kept.");
            await reload();
        } catch (error) { setError(userError(error)); }
        finally { setSaving(false); }
    }

    async function retry() {
        try { await reload(); setError(""); }
        catch (error) { setError(userError(error)); }
    }

    return <div>
        <h1 className="text-3xl mb-3">Calendar</h1>
        <p className="mb-6">Select a future date. Save a day off or restore a normal working day by adding a new version. Earlier versions are kept.</p>
        {(error || loadError) && <p role="alert" className="text-red-600 mb-3">{error || loadError} <button disabled={saving} onClick={() => void retry()} className="underline">Reload</button></p>}
        {loading && <p role="status">Loading calendar...</p>}
        {success && <p role="status" className="mb-3">{success}</p>}
        <div className="grid grid-cols-3 gap-6">
            <div className="col-span-2"><Calendar start={selected} end={null} onSelect={date => { if (!saving) select(date); }} events={events} /></div>
            <div className="bg-white border border-line rounded p-6">
                {!selected ? <p>Select a date to view its history or save a new version.</p> : <>
                    <h2 className="text-2xl mb-4">{date}</h2>
                    <form onSubmit={save}><fieldset disabled={saving || loading || !!loadError}>
                        <label className="block">Title<input required value={name} onChange={event => setName(event.target.value)} className="w-full border border-line rounded p-2 mt-1" /></label>
                        <label className="block mt-4">Day type<select value={String(usesHolidayHours)} onChange={event => setUsesHolidayHours(event.target.value === "true")} className="w-full border border-line rounded p-2 mt-1">
                            <option value="false">Day off — no holiday hours charged</option><option value="true">Normal day — use the work schedule</option>
                        </select></label>
                        <button type="submit" disabled={date <= dateText(new Date())} className="mt-4 bg-navy text-white rounded p-3 disabled:opacity-40">{saving ? "Saving..." : "Save new version"}</button>
                        {date <= dateText(new Date()) && <p className="mt-2 text-sm">Past dates and today can only be viewed.</p>}
                    </fieldset></form>
                    <h3 className="text-xl mt-6 mb-2">Version history</h3>
                    {versions.length === 0 && <p>No calendar override. The employee's work schedule applies.</p>}
                    {versions.map(version => <div key={version.id} className="border-b border-line py-3">
                        <strong>v{version.version}: {version.name}</strong><p>{version.usesHolidayHours ? "Normal work schedule" : "Day off"}</p>
                        <p className="text-sm text-slate">{new Date(version.createdAt).toLocaleString()} · {version.createdBy}</p>
                    </div>)}
                </>}
            </div>
        </div>
    </div>;
}
