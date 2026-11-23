import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { useAuth } from "./auth";
import { calendarEvents, getCalendar, type CalendarVersion } from "../api/calendar";
import { userError } from "../api/users";
import type { CalendarEvent } from "../types/calendarEvent";

type CalendarData = { history: CalendarVersion[]; events: CalendarEvent[]; reload: () => Promise<void>; error: string; loading: boolean };
const CalendarContext = createContext<CalendarData | null>(null);
export function useCalendar() {
    const data = useContext(CalendarContext);
    if (!data) throw new Error("CalendarProvider is missing.");
    return data;
}

export function CalendarProvider({ children }: { children: ReactNode }) {
    const { user } = useAuth();
    const [history, setHistory] = useState<CalendarVersion[]>([]);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    async function reload() {
        const saved = await getCalendar();
        setHistory(saved);
        setError("");
    }
    useEffect(() => {
        let cancelled = false;
        setHistory([]);
        setError("");
        if (!user?.id) { setLoading(false); return; }
        setLoading(true);
        getCalendar().then(data => { if (!cancelled) setHistory(data); })
            .catch(error => { if (!cancelled) setError(userError(error)); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [user?.id]);
    return <CalendarContext.Provider value={{ history, events: calendarEvents(history), reload, error, loading }}>{children}</CalendarContext.Provider>;
}
