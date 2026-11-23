type Props = {
    value: string; // "HH:MM", e.g. "08:30"
    onChange: (value: string) => void;
};

// 00, 01 ... 23
const hours: string[] = [];
for (let h = 0; h < 24; h++) {
    hours.push(String(h).padStart(2, "0"));
}

const minutes = ["00", "15", "30", "45"];

// A 24-hour time picker made of two dropdowns, so it never shows AM/PM
export function TimeSelect({ value, onChange }: Props) {
    const [hour, minute] = value.split(":");

    return (
        <span className="inline-flex items-center gap-1">
            <select value={hour} onChange={e => onChange(e.target.value + ":" + minute)} className="border border-line rounded p-1 bg-white">
                {hours.map(h => <option key={h} value={h}>{h}</option>)}
            </select>
            :
            <select value={minute} onChange={e => onChange(hour + ":" + e.target.value)} className="border border-line rounded p-1 bg-white">
                {minutes.map(m => <option key={m} value={m}>{m}</option>)}
            </select>
        </span>
    );
}