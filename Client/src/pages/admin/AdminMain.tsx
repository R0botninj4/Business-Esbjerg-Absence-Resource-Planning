import { useState, type SubmitEvent } from "react";
import { presets } from "../../mockdata/testRules";
import type { Rule, RuleVersion } from "../../types/rule";
import { formatDate } from "../../utils/format";
import { currentVersion, upcomingVersions } from "../../utils/rules";

type Props = {
    rules: Rule[];
    onAddVersion: (ruleId: string, version: RuleVersion) => void;
};

const groupColors = {
    "Week 1–5": "bg-navy/10",
    "Week 6": "bg-slate/20",
    "General": "bg-line",
};

export function AdminMain({ rules, onAddVersion }: Props) {
    const [selectedPreset, setSelectedPreset] = useState("Ferieloven + 6. ferieuge");
    const [changed, setChanged] = useState(false); // true when the admin has changed a value after picking a preset
    const [openHistory, setOpenHistory] = useState<string | null>(null); // the rule whose history is shown

    // The "Schedule a change" form
    const [ruleId, setRuleId] = useState(rules[0]?.id ?? "");
    const [value, setValue] = useState("");
    const [effectiveFrom, setEffectiveFrom] = useState("");
    const [reason, setReason] = useState("");
    const [error, setError] = useState("");

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // "Edit" on a row fills in the form with that rule
    function editRule(rule: Rule) {
        setRuleId(rule.id);
        setValue(currentVersion(rule, today)?.value ?? "");
        setError("");
    }

    function handleSubmit(e: SubmitEvent) {
        e.preventDefault();

        if (value.trim() === "" || effectiveFrom === "") {
            setError("Fill in the new value and the date it starts.");
            return;
        }

        const date = new Date(effectiveFrom + "T00:00");
        if (date < today) {
            setError("A change can't start in the past – old periods must not be changed.");
            return;
        }

        onAddVersion(ruleId, { value: value, effectiveFrom: date, reason: reason });
        setChanged(true);
        setValue("");
        setEffectiveFrom("");
        setReason("");
        setError("");
    }

    return (
        <div>
            <h1 className="text-3xl">Rules & parameters</h1>
            <p className="text-slate mb-6">
                Every rule has a start date. A new value applies from that date and never changes history –
                old periods keep the values that applied then.
            </p>

            {/* Presets */}
            <h2 className="text-xl mb-3">Start from a preset</h2>
            <div className="grid grid-cols-3 gap-4 mb-8">
                {presets.map(preset => (
                    <button
                        key={preset.name}
                        onClick={() => setSelectedPreset(preset.name)}
                        className={"text-left bg-white rounded p-4 border-2 " +
                            (preset.name === selectedPreset ? "border-orange" : "border-line hover:border-orange/40")}
                    >
                        <p className="font-semibold">
                            {preset.name}
                            {preset.name === selectedPreset && (
                                <span className="ml-2 text-xs text-white bg-orange px-2 py-1 rounded-full">Selected</span>
                            )}
                        </p>
                        <p className="text-sm text-slate mt-1">{preset.description}</p>
                    </button>
                ))}
            </div>

            <div className="grid grid-cols-3 gap-6 items-start">
                {/* The rules */}
                <div className="col-span-2 bg-white border border-line rounded p-6">
                    <h2 className="text-2xl mb-4">
                        Active rules: {selectedPreset}
                        {changed && <span className="ml-2 text-sm bg-gold text-white px-2 py-1 rounded-full">Custom</span>}
                    </h2>

                    <div className="grid grid-cols-12 gap-2 text-sm font-semibold text-slate pb-2 border-b border-line">
                        <span className="col-span-4">Rule</span>
                        <span className="col-span-4">Value</span>
                        <span className="col-span-2">From</span>
                    </div>

                    {rules.map(rule => {
                        const current = currentVersion(rule, today);
                        const upcoming = upcomingVersions(rule, today);

                        return (
                            <div key={rule.id} className="border-b border-line last:border-0 py-3">
                                <div className="grid grid-cols-12 gap-2 items-center">
                                    <span className="col-span-4">
                                        <span className={"text-xs font-semibold px-2 py-1 rounded-full mr-2 " + groupColors[rule.group]}>
                                            {rule.group}
                                        </span>
                                        {rule.name}
                                    </span>
                                    <span className="col-span-4 font-semibold">{current?.value}</span>
                                    <span className="col-span-2">{current && formatDate(current.effectiveFrom)}</span>
                                    <span className="col-span-2 flex gap-3 justify-end">
                                        <button onClick={() => editRule(rule)} className="font-semibold hover:text-orange">Edit</button>
                                        <button
                                            onClick={() => setOpenHistory(openHistory === rule.id ? null : rule.id)}
                                            className="font-semibold hover:text-orange"
                                        >
                                            History
                                        </button>
                                    </span>
                                </div>

                                {/* Scheduled changes that haven't started yet */}
                                {upcoming.map(version => (
                                    <p key={version.effectiveFrom.getTime()} className="text-sm text-gold font-semibold mt-1">
                                        Scheduled: {version.value} from {formatDate(version.effectiveFrom)}
                                    </p>
                                ))}

                                {/* All versions, newest first */}
                                {openHistory === rule.id && (
                                    <div className="mt-2 bg-cream rounded p-3 text-sm">
                                        {[...rule.versions].reverse().map(version => (
                                            <p key={version.effectiveFrom.getTime()}>
                                                <strong>{formatDate(version.effectiveFrom)}:</strong> {version.value}
                                                {version.reason && <span className="text-slate"> · {version.reason}</span>}
                                            </p>
                                        ))}
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>

                {/* Schedule a change */}
                <form onSubmit={handleSubmit} className="bg-white border border-line rounded p-6 border-b-4 border-b-orange space-y-4">
                    <h2 className="text-2xl">Schedule a change</h2>

                    <label className="block font-semibold">
                        Rule
                        <select value={ruleId} onChange={e => setRuleId(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal bg-white">
                            {rules.map(rule => (
                                <option key={rule.id} value={rule.id}>{rule.group} – {rule.name}</option>
                            ))}
                        </select>
                    </label>

                    <label className="block font-semibold">
                        New value
                        <input value={value} onChange={e => setValue(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                    </label>

                    <label className="block font-semibold">
                        Effective from
                        <input type="date" value={effectiveFrom} onChange={e => setEffectiveFrom(e.target.value)} className="mt-1 w-full border border-line rounded p-2 font-normal" />
                    </label>

                    <label className="block font-semibold">
                        Reason / source
                        <input value={reason} onChange={e => setReason(e.target.value)} placeholder="e.g. new KL agreement" className="mt-1 w-full border border-line rounded p-2 font-normal" />
                    </label>

                    <p className="text-sm text-slate">Earlier periods are not recalculated.</p>
                    {error && <p className="text-red-600">{error}</p>}

                    <button type="submit" className="w-full bg-navy text-white font-semibold py-3 rounded hover:bg-navy/90">
                        Save change
                    </button>
                </form>
            </div>
        </div>
    );
}