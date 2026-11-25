import { useState } from "react";
import { api } from "../api/client";
import { userError } from "../api/users";
import { dateText } from "../api/schedules";

type Period = {
    account: number;
    usageStart: string;
    usageEnd: string;
    postedHours: number;
    expectedHours: number;
    adjustmentHours: number;
    usedHours: number;
    reservedHours: number;
    hours: number;
};

type Balance = {
    date: string;
    weeksOneToFiveHours: number;
    sixthWeekHours: number;
    periods: Period[];
};

export function FutureBalance({ userId }: { userId: string }) {
    const today = dateText(new Date());
    const [date, setDate] = useState(today);
    const [balance, setBalance] = useState<Balance | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    async function calculate() {
        setLoading(true);
        setBalance(null);
        setError("");
        try {
            const saved = await api.request<Balance>({
                path: `/api/users/${userId}/holiday-balances/forecast`,
                method: "GET",
                query: { date },
                format: "json",
            });
            setBalance(saved);
        } catch (error) {
            setError(userError(error));
        } finally {
            setLoading(false);
        }
    }

    function periodStatus(period: Period, date: string) {
        if (date < period.usageStart) {
            return "Not available yet";
        }
        if (date > period.usageEnd) {
            return "Usage period ended";
        }
        return "Available in this period";
    }

    return <div>
        <h2 className="text-xl mb-3">Holiday balance on a future date</h2>
        <form onSubmit={event => { event.preventDefault(); void calculate(); }} className="flex items-end gap-3">
            <label>
                Date
                <input type="date" required min={today} value={date} disabled={loading}
                    onChange={event => { setDate(event.target.value); setBalance(null); setError(""); }}
                    className="block mt-1 border border-line rounded p-2" />
            </label>
            <button disabled={loading} className="bg-navy text-white rounded px-4 py-2 disabled:opacity-40">
                {loading ? "Calculating..." : "Calculate balance"}
            </button>
        </form>
        <p className="text-sm text-slate mt-3">Includes expected earnings under saved rules and schedules. All approved holiday is deducted, including reservations after the selected date. Pending requests are not deducted. This calculation saves no postings and does not approve holiday or grant holiday debt.</p>
        <p className="text-sm text-slate mt-2">A change in weekly working hours requires balance conversion, which is not connected yet. The calculation will report an error if conversion is needed.</p>
        {error && <p role="alert" className="text-red-600 mt-3">{error}</p>}
        {balance && <div className="mt-4" role="status">
            <p>Week 1–5 on {balance.date}: <strong>{Number(balance.weeksOneToFiveHours).toFixed(2)} h</strong></p>
            <p>Week 6 on {balance.date}: <strong>{Number(balance.sixthWeekHours).toFixed(2)} h</strong></p>
            <div className="mt-4 space-y-3">
                {balance.periods.map((period, index) => <div key={index} className="border border-line rounded p-3">
                    <p className="font-semibold">{period.account === 1 ? "Week 1–5" : "Week 6"}: {periodStatus(period, balance.date)}</p>
                    <p>Usage: {period.usageStart} – {period.usageEnd}</p>
                    <p>Posted: {Number(period.postedHours).toFixed(2)} h · Expected: {Number(period.expectedHours).toFixed(2)} h · Adjustments: {Number(period.adjustmentHours).toFixed(2)} h</p>
                    <p>Used: {Number(period.usedHours).toFixed(2)} h · Reserved: {Number(period.reservedHours).toFixed(2)} h · Remaining: {Number(period.hours).toFixed(2)} h</p>
                </div>)}
            </div>
        </div>}
    </div>;
}
