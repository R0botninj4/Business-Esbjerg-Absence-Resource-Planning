import { useEffect, useState, type SubmitEvent } from "react";
import { api } from "../api/client";
import { ContentType } from "../api/Api";
import { userError } from "../api/users";
import { PostedBalance } from "./postedBalance";
import type { HolidayBalanceAdjustment } from "../api/Api";

export function OpeningBalanceForm({ userId }: { userId: string }) {
    const [account, setAccount] = useState("2");
    const [hours, setHours] = useState("");
    const [earningStart, setEarningStart] = useState("");
    const [earningEnd, setEarningEnd] = useState("");
    const [usageStart, setUsageStart] = useState("");
    const [usageEnd, setUsageEnd] = useState("");
    const [reason, setReason] = useState("");
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState("");
    const [saved, setSaved] = useState(false);
    const [history, setHistory] = useState<HolidayBalanceAdjustment[]>([]);
    // Samme indhold får samme ID ved et nyt forsøg efter en forbindelsesfejl.
    const [operationId, setOperationId] = useState(crypto.randomUUID());

    useEffect(() => {
        async function loadHistory() {
            try {
                const items = await api.request<HolidayBalanceAdjustment[]>({
                    path: `/api/users/${userId}/holiday-balances/adjustments`, method: "GET", format: "json",
                });
                const openingBalances = items.filter(item => !item.accrualDayId);
                setHistory(openingBalances);
            } catch (error) {
                setError(userError(error));
            }
        }
        void loadHistory();
    }, [userId]);

    function changed() {
        setOperationId(crypto.randomUUID());
        setSaved(false);
    }

    async function save(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault();
        if (busy || saved) return;
        setBusy(true);
        setError("");
        try {
            const record = await api.request<HolidayBalanceAdjustment>({
                path: `/api/users/${userId}/holiday-balances/opening-balances`,
                method: "POST", type: ContentType.Json, format: "json",
                body: { operationId, account: Number(account), hours: Number(hours), earningStart, earningEnd, usageStart, usageEnd, reason },
            });
            setHistory(items => [...items.filter(item => item.id !== record.id), record]);
            setSaved(true);
        } catch (error) {
            setError(userError(error));
        } finally {
            setBusy(false);
        }
    }

    return <section className="bg-white border border-line rounded p-6 mt-6">
        <h2 className="text-2xl mb-3">Opening holiday balance</h2>
        <p className="mb-4 text-slate">Enter documented hours remaining at the system's start. Include only earnings from before that date. Later earnings are added automatically. Use the original earning and usage periods from the old records.</p>
        <form onSubmit={save} onChange={changed}>
            <fieldset disabled={busy} className="grid grid-cols-2 gap-4">
                <label>Account<select value={account} onChange={event => setAccount(event.target.value)} className="block border border-line rounded p-2 w-full"><option value="1">Week 1–5</option><option value="2">Week 6</option></select></label>
                <label>Opening hours<input required type="number" min="0.01" step="0.01" value={hours} onChange={event => setHours(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
                <label>Earning period starts<input required type="date" value={earningStart} onChange={event => setEarningStart(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
                <label>Earning period ends<input required type="date" value={earningEnd} onChange={event => setEarningEnd(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
                <label>Can be used from<input required type="date" value={usageStart} onChange={event => setUsageStart(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
                <label>Can be used until<input required type="date" value={usageEnd} onChange={event => setUsageEnd(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
                <label className="col-span-2">Reason / source<input required maxLength={500} value={reason} onChange={event => setReason(event.target.value)} className="block border border-line rounded p-2 w-full" /></label>
            </fieldset>
            {error && <p role="alert" className="text-red-600 mt-3">{error}</p>}
            {saved && <p role="status" className="mt-3">Opening balance saved. The record is kept in the balance history.</p>}
            <button disabled={busy || saved} className="bg-navy text-white rounded px-5 py-3 mt-4 disabled:opacity-50">{busy ? "Saving..." : "Save opening balance"}</button>
        </form>
        {saved && <div className="mt-4"><PostedBalance key={operationId} userId={userId} /></div>}
        {history.length > 0 && <div className="mt-4">
            <h3 className="text-xl">Saved opening balances</h3>
            {history.map(item => <div key={item.id} className="border-b border-line py-3">
                <p>Week {item.account === 1 ? "1–5" : "6"}: {Number(item.hours).toFixed(2)} h · As of {item.date}</p>
                <p>Earning: {item.earningStart} – {item.earningEnd} · Usage: {item.usageStart} – {item.usageEnd}</p>
                <p>{item.reason}</p>
            </div>)}
        </div>}
    </section>;
}
