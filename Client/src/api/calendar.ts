import { api } from "./client";
import { ContentType } from "./Api";
import type { CalendarEvent } from "../types/calendarEvent";

export type CalendarVersion = {
    id: string; date: string; name: string; usesHolidayHours: boolean;
    version: number; createdAt: string; createdBy: string;
};

export function getCalendar() {
    return api.request<CalendarVersion[]>({ path: "/api/calendar-days", method: "GET", format: "json" });
}

export function saveCalendarDay(date: string, name: string, usesHolidayHours: boolean) {
    return api.request<CalendarVersion>({
        path: "/api/calendar-days", method: "POST", type: ContentType.Json, format: "json",
        body: { date, name, usesHolidayHours },
    });
}

export function calendarEvents(history: CalendarVersion[]): CalendarEvent[] {
    const latest = new Map<string, CalendarVersion>();
    for (const day of history) {
        const previous = latest.get(day.date);
        if (!previous || day.version > previous.version) latest.set(day.date, day);
    }
    return [...latest.values()].map(day => ({
        date: new Date(day.date + "T00:00"), title: day.name,
        type: day.usesHolidayHours ? "info" : "companyDayOff",
    }));
}
