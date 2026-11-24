import type { CalendarEvent } from "../types/calendarEvent";
import type { Employee } from "../types/employee";
import type { HolidayType } from "../types/holidayRequest";
import type { SickDay, SicknessReport } from "../types/sicknessReport";
import { weekdayIndex } from "./dates";
import { hoursOfDay, scheduleOn } from "./schedule";

// The law: the first 5 sick days during week 1–5 holiday in a holiday year are not given back
const daysWithoutRefund = 5;

// The working days from start to end, and how many hours each day is worth.
// Weekends, days off and days the employee doesn't work are skipped.
export function sickDaysBetween(employee: Employee, start: Date, end: Date, events: CalendarEvent[]) {
    const days: SickDay[] = [];
    const day = new Date(start);

    while (day <= end) {
        const index = weekdayIndex(day);
        const isDayOff = events.some(e => e.type !== "info" && e.date.getTime() === day.getTime());

        if (index <= 4 && !isDayOff) {
            const hours = hoursOfDay(scheduleOn(employee, day)[index] ?? null);
            if (hours > 0) {
                days.push({ date: new Date(day), hours: hours });
            }
        }
        day.setDate(day.getDate() + 1);
    }
    return days;
}

export function totalHours(days: SickDay[]) {
    let total = 0;
    for (const day of days) {
        total += day.hours;
    }
    return total;
}

// The holiday year starts 1 September
function holidayYearStart(date: Date) {
    const year = date.getMonth() >= 8 ? date.getFullYear() : date.getFullYear() - 1;
    return new Date(year, 8, 1);
}

// A short text that explains the rule for the employee
export function sicknessRule(holidayType: HolidayType, beforeHoliday: boolean) {
    if (beforeHoliday) {
        return "Sick before the holiday started: all the sick days are given back.";
    }
    if (holidayType === "week6") {
        return "Sick during week 6 holiday: accounting decides if the days are given back.";
    }
    return "Sick during week 1–5 holiday: days are only given back after the first 5 sick days in the holiday year, and a doctor's note is needed.";
}

// How many hours we suggest accounting gives back. Accounting makes the final decision.
export function suggestedRefund(report: SicknessReport, allReports: SicknessReport[]) {
    const all = totalHours(report.sickDays);

    if (report.beforeHoliday) {
        return { hours: all, reason: "Sick before the holiday started, so all the sick days are given back." };
    }
    if (report.holidayType === "week6") {
        return { hours: all, reason: "Sick during week 6 holiday. The employer decides – you can lower the hours or set them to 0." };
    }

    // Sick during week 1–5 holiday: count the sick days already used in the same holiday year
    const yearStart = holidayYearStart(report.start).getTime();
    let usedDays = 0;
    for (const other of allReports) {
        const sameYear = holidayYearStart(other.start).getTime() === yearStart;
        const counts = other.status === "approved" && !other.beforeHoliday && other.holidayType === "weeks1to5";
        if (other.id !== report.id && other.employeeId === report.employeeId && sameYear && counts) {
            usedDays += other.sickDays.length;
        }
    }

    const daysLeft = Math.max(0, daysWithoutRefund - usedDays);
    const refundedDays = report.sickDays.slice(daysLeft); // skip the days that are not given back

    let reason = "Sick during week 1–5 holiday. The first 5 sick days in the holiday year are not given back (" + usedDays + " already used).";
    if (!report.doctorsNote) {
        reason += " There is no doctor's note, and the law requires documentation.";
    }
    return { hours: totalHours(refundedDays), reason: reason };
}