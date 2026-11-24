import { useEffect, useState } from "react";
import { api } from "../api/client";
import { userError } from "../api/users";

type Balance = {
    weeksOneToFiveHours: number;
    sixthWeekHours: number;
};

export function PostedBalance({ userId }: { userId: string }) {
    const [balance, setBalance] = useState<Balance | null>(null);
    const [error, setError] = useState("");

    useEffect(() => {
        let cancelled = false;
        setBalance(null);
        setError("");
        const today = new Date();
        const month = String(today.getMonth() + 1).padStart(2, "0");
        const day = String(today.getDate()).padStart(2, "0");
        const date = today.getFullYear() + "-" + month + "-" + day;

        async function load() {
            try {
                const saved = await api.request<Balance>({
                    path: `/api/users/${userId}/holiday-balances`,
                    query: { date },
                    method: "GET",
                    format: "json",
                });
                if (!cancelled) {
                    setBalance(saved);
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
        <p className="text-sm text-slate mt-2">Posted earnings and adjustments, minus approved holiday. Pending requests are not reserved. Forecast earnings, holiday debt and conversion after changes in working hours are not included.</p>
    </div>;
}
