import { useEffect, useState } from "react";
import { api } from "../api/client";
import { userError } from "../api/users";

type Balance = {
    weeksOneToFiveHours: number;
    sixthWeekHours: number;
};

type Conversion = {
    id: string;
    date: string;
    account: number;
    beforeHours: number;
    afterHours: number;
    hours: number;
};

export function PostedBalance({ userId }: { userId: string }) {
    const [balance, setBalance] = useState<Balance | null>(null);
    const [error, setError] = useState("");
    const [conversions, setConversions] = useState<Conversion[]>([]);

    useEffect(() => {
        let cancelled = false;
        setBalance(null);
        setError("");
        setConversions([]);

        async function load() {
            try {
                const saved = await api.request<Balance>({
                    path: `/api/users/${userId}/holiday-balances/reconcile`,
                    method: "POST",
                    format: "json",
                });
                if (!cancelled) {
                    setBalance(saved);
                }
                const history = await api.request<Conversion[]>({
                    path: `/api/users/${userId}/holiday-balances/conversions`,
                    method: "GET",
                    format: "json",
                });
                if (!cancelled) {
                    setConversions(history);
                }
            } catch (error) {
                if (!cancelled) {
                    setError(userError(error));
                }
            }
        }
        void load();
        return () => { cancelled = true; };
    }, [userId]);

    return <div>
        <h2 className="text-xl mb-2">Posted holiday balance</h2>
        {error && <p role="alert" className="text-red-600">{error}</p>}
        {!balance && !error && <p role="status">Loading balance...</p>}
        {balance && <div>
            <p>Week 1–5: <strong>{Number(balance.weeksOneToFiveHours).toFixed(2)} h</strong></p>
            <p>Week 6: <strong>{Number(balance.sixthWeekHours).toFixed(2)} h</strong></p>
        </div>}
        <p className="text-sm text-slate mt-2">Posted earnings and adjustments, minus approved holiday, converted to the current working hours. This can be negative when future holiday is reserved against expected earnings. Use the forecast below to see expected earnings and actual holiday debt on a date. Pending requests are not reserved.</p>
        {conversions.length > 0 && <details className="mt-3">
            <summary className="cursor-pointer font-semibold">Working-time conversion history</summary>
            <p className="text-sm text-slate">Later postings can add a correction. Earlier entries are kept.</p>
            {conversions.map(item => <p key={item.id} className="py-2 border-b border-line">
                {item.date} · {item.account === 1 ? "Week 1–5" : "Week 6"} · {Number(item.beforeHours).toFixed(2)} h → {Number(item.afterHours).toFixed(2)} h · Recorded change: {Number(item.hours).toFixed(2)} h
            </p>)}
        </details>}
    </div>;
}
