import type { CalendarEvent } from "../types/calendarEvent";

// Temporary test data – will be replaced by data from the API
export const testEvents: CalendarEvent[] = [
    { date: new Date(2026, 10, 20), title: "Personalemøde", type: "info" },
    { date: new Date(2026, 11, 24), title: "Juleaftensdag", type: "companyDayOff" },
    { date: new Date(2026, 11, 25), title: "Juledag", type: "holiday" },
    { date: new Date(2026, 11, 26), title: "2. juledag", type: "holiday" },
    { date: new Date(2026, 11, 31), title: "Nytårsaftensdag", type: "companyDayOff" },
    { date: new Date(2027, 0, 1), title: "Nytårsdag", type: "holiday" },
];