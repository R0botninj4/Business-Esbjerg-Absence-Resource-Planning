import { useEffect, useState, type SubmitEvent } from "react";
import { createRule, getRuleHistory, getRulePresets, settingsFromVersion, type RuleSettings, type RuleVersion } from "../../api/rules";
import { userError } from "../../api/users";
import { dateText } from "../../api/schedules";

// Labels and groups only. The values always come from the API.
const groups: { title: string; fields: [keyof RuleSettings, string][] }[] = [
    { title: "Accrual and accounts", fields: [
        ["sicknessWaitingDays", "Sick days without replacement during week 1–5"],
        ["nonTransferableHolidayDays", "Week 1–5: protected days that cannot be transferred"],
        ["holidayDaysPerMonth", "Week 1–5: days earned per month"], ["holidayDaysPerYear", "Week 1–5: days per full year"],
        ["sixthWeekHoursPerMonth", "Week 6: full-time hours per month"], ["sixthWeekHoursPerCalendarDay", "Week 6: hours per calendar day in a partial month"],
        ["fullTimeHoursPerWeek", "Full-time working hours per week"], ["maximumDebtDays", "Maximum holiday debt (days)"], ["maximumTransferDays", "Maximum transfer (days)"],
    ] },
    { title: "Week 1–5 periods", fields: [
        ["holidayEarningStartMonth", "Earning year starts: month"], ["holidayEarningStartDay", "Earning year starts: day"],
        ["holidayUsageStartMonth", "Usage starts: month"], ["holidayUsageStartDay", "Usage starts: day"], ["holidayUsageStartYearOffset", "Usage starts: year offset"],
        ["holidayUsageEndMonth", "Usage ends: month"], ["holidayUsageEndDay", "Usage ends: day"], ["holidayUsageEndYearOffset", "Usage ends: year offset"],
    ] },
    { title: "Week 6 periods", fields: [
        ["sixthWeekEarningStartMonth", "Earning year starts: month"], ["sixthWeekEarningStartDay", "Earning year starts: day"],
        ["sixthWeekUsageStartMonth", "Usage starts: month"], ["sixthWeekUsageStartDay", "Usage starts: day"], ["sixthWeekUsageStartYearOffset", "Usage starts: year offset"],
        ["sixthWeekUsageEndMonth", "Usage ends: month"], ["sixthWeekUsageEndDay", "Usage ends: day"], ["sixthWeekUsageEndYearOffset", "Usage ends: year offset"],
    ] },
    { title: "Main holiday and notice", fields: [
        ["mainHolidayDays", "Main holiday length (days)"], ["mainHolidayStartMonth", "Main holiday window starts: month"], ["mainHolidayStartDay", "Main holiday window starts: day"],
        ["mainHolidayEndMonth", "Main holiday window ends: month"], ["mainHolidayEndDay", "Main holiday window ends: day"],
        ["mainHolidayNoticeMonths", "Main holiday notice (months)"], ["otherHolidayNoticeMonths", "Other holiday notice (months)"],
    ] },
    { title: "Debt and transfer deadlines", fields: [
        ["debtDeadlineMonth", "Debt deadline: month"], ["debtDeadlineDay", "Debt deadline: day"],
        ["holidayTransferDeadlineMonth", "Week 1–5 transfer deadline: month"], ["holidayTransferDeadlineDay", "Week 1–5 transfer deadline: day"],
        ["sixthWeekTransferDeadlineMonth", "Week 6 transfer deadline: month"], ["sixthWeekTransferDeadlineDay", "Week 6 transfer deadline: day"],
    ] },
];

export function AdminMain() {
    const [history, setHistory] = useState<RuleVersion[]>([]);
    const [presets, setPresets] = useState<RuleSettings[]>([]);
    const [settings, setSettings] = useState<RuleSettings | null>(null);
    const [effectiveFrom, setEffectiveFrom] = useState("");
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [selectedId, setSelectedId] = useState("");

    async function load() {
        setLoading(true);
        setError("");
        try {
            const saved = await getRuleHistory();
            const templates = await getRulePresets();
            setHistory(saved);
            setPresets(templates);
            setSelectedId(saved[0]?.id || "");
            const latest = saved[0];
            // An incomplete historical version must not silently receive preset values.
            if (latest) setSettings(settingsFromVersion(latest));
            else setSettings(templates[0] ? { ...templates[0] } : null);
        } catch (error) { setError(userError(error)); }
        finally { setLoading(false); }
    }
    useEffect(() => { void load(); }, []);

    function changeNumber(field: keyof RuleSettings, value: string) {
        if (!settings) return;
        setSettings({ ...settings, [field]: value === "" ? NaN : Number(value) });
    }

    async function save(event: SubmitEvent) {
        event.preventDefault();
        if (!settings || saving) return;
        setSaving(true);
        setError("");
        setSuccess("");
        try {
            const saved = await createRule(effectiveFrom, settings);
            setHistory(current => [saved, ...current]);
            setSelectedId(saved.id);
            setEffectiveFrom("");
            setSuccess("New rule version saved. Previous versions have not been changed.");
        } catch (error) { setError(userError(error)); }
        finally { setSaving(false); }
    }

    const today = dateText(new Date());
    const active = history.filter(version => version.effectiveFrom <= today)
        .sort((a, b) => b.effectiveFrom.localeCompare(a.effectiveFrom) || b.version - a.version)[0];
    const selected = history.find(version => version.id === selectedId);
    const selectedSettings = selected ? settingsFromVersion(selected) : null;
    const selectedValues = selected ? { ...selected, ...selected.periods } : null;
    if (loading) return <p role="status">Loading rules...</p>;

    return <div>
        <h1 className="text-3xl mb-4">Rules & parameters</h1>
        <p className="mb-4">{active ? "Active: " + active.name + " from " + active.effectiveFrom : "No active rule version yet."}</p>
        {error && <p role="alert" className="text-red-600 mb-4">{error} <button disabled={saving} onClick={() => void load()} className="underline">Reload</button></p>}
        {success && <p role="status" className="mb-4">{success}</p>}
        <div className="bg-white border border-line rounded p-6 mb-6">
            <h2 className="text-2xl mb-3">Saved history and upcoming changes</h2>
            {history.length === 0 && <p>No saved rule versions.</p>}
            <div className="flex flex-wrap gap-3">{history.map(version => <button key={version.id} onClick={() => setSelectedId(version.id)} className={"border rounded p-3 " + (selectedId === version.id ? "border-orange" : "border-line")}>
                {version.name} · {version.effectiveFrom} · v{version.version}{version.effectiveFrom > today ? " (scheduled)" : ""}
            </button>)}</div>
            {selected && <div className="mt-4">
                <p>Saved {new Date(selected.createdAt).toLocaleString()} · by {selected.createdBy}</p>
                {!selectedSettings && <p className="mt-2 text-red-600">This historical version is incomplete. Missing fields have not been filled in from a preset.</p>}
                {groups.map(group => <details key={group.title} className="mt-3"><summary className="font-semibold cursor-pointer">{group.title}</summary>
                    {group.fields.map(([field, label]) => {
                        const value = selectedValues?.[field];
                        return <p key={field}>{label}: <strong>{value === null || value === undefined ? "Not recorded" : String(value)}</strong></p>;
                    })}
                </details>)}
                <p className="mt-2">Week 6: {selected.sixthWeekEnabled ? "Enabled" : "Disabled"} · Transfers: {selected.allowTransfer ? "Allowed" : "Disabled"}</p>
                <button disabled={!selectedSettings || saving} onClick={() => { setSettings(selectedSettings); setSuccess(""); }} className="mt-3 underline disabled:opacity-40">Use this version as a starting point</button>
            </div>}
        </div>
        <form onSubmit={save} className="bg-white border border-line rounded p-6">
            <fieldset disabled={saving}>
                <h2 className="text-2xl mb-3">Create a new rule version</h2>
                <p className="mb-3 text-slate">All settings are saved together. Old versions are kept. Year offset 0 means the earning year's start year; 1 means the following year.</p>
                <label className="block mb-4">Start from a preset
                    <select defaultValue="" onChange={event => { const preset = presets[Number(event.target.value)]; if (preset) setSettings({ ...preset }); }} className="border border-line rounded p-2 ml-3">
                        <option value="" disabled>Choose preset</option>{presets.map((preset, index) => <option key={index} value={index}>{preset.name}</option>)}
                    </select>
                </label>
                {!settings ? <p>Choose a complete saved version or a preset to create new settings.</p> : <>
                    <div className="grid grid-cols-2 gap-4">
                        <label>Version name / source<input required maxLength={100} value={settings.name} onChange={event => setSettings({ ...settings, name: event.target.value })} className="block w-full border border-line rounded p-2" /></label>
                        <label>Effective from<input required type="date" value={effectiveFrom} onChange={event => setEffectiveFrom(event.target.value)} className="block w-full border border-line rounded p-2" /></label>
                    </div>
                    <p className="text-sm text-slate mt-2">The first version can describe existing periods. Later changes must start after today.</p>
                    <div className="flex gap-6 my-4">
                        <label><input type="checkbox" checked={settings.sixthWeekEnabled} onChange={event => setSettings({ ...settings, sixthWeekEnabled: event.target.checked })} /> Enable week 6</label>
                        <label><input type="checkbox" checked={settings.allowTransfer} onChange={event => setSettings({ ...settings, allowTransfer: event.target.checked })} /> Allow transfers</label>
                    </div>
                    {groups.map(group => <div key={group.title} className="border-t border-line mt-4 pt-4">
                        <h3 className="text-xl mb-3">{group.title}</h3>
                        <div className="grid grid-cols-3 gap-4">{group.fields.map(([field, label]) => <label key={field}>{label}
                            <input required type="number" step="any" min="0" value={Number.isNaN(settings[field]) ? "" : String(settings[field])} onChange={event => changeNumber(field, event.target.value)} className="block border border-line rounded w-full p-2" />
                        </label>)}</div>
                    </div>)}
                    <button type="submit" className="bg-navy text-white rounded px-6 py-3 mt-6">{saving ? "Saving..." : "Save new version"}</button>
                </>}
            </fieldset>
        </form>
    </div>;
}
