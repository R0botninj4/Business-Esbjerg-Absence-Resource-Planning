import type { Employee, WorkDay } from "../types/employee";
import { formatDate } from "./format";

export const weekdayNames = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];

function toMinutes(time: string) {
    const [hours, minutes] = time.split(":");
    return Number(hours) * 60 + Number(minutes);
}

export function hoursOfDay(day: WorkDay) {
    if (!day) return 0;
    return (toMinutes(day.to) - toMinutes(day.from)) / 60;
}

export function hoursPerWeek(days: WorkDay[]) {
    let total = 0;
    for (const day of days) {
        total += hoursOfDay(day);
    }
    return total;
}

export function employmentText(days: WorkDay[], employmentType?: number) {
    const hours = hoursPerWeek(days);
    let type = "Part time";
    if (hours >= 37) type = "Full time";
    if (employmentType === 1) type = "Full time";
    if (employmentType === 2) type = "Part time";
    if (employmentType === 3) type = "Hourly";
    return type + " - " + hours.toFixed(2) + " h/week";
}

export function scheduleOn(employee: Employee, date: Date): WorkDay[] {
    let days: WorkDay[] = [null, null, null, null, null];
    for (const period of employee.schedules) {
        if (period.validFrom <= date && (!period.validTo || date <= period.validTo)) {
            days = period.days;
        }
    }
    return days;
}

export const defaultFullTime: WorkDay[] = [
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:00" },
];

export const defaultPartTime: WorkDay[] = [
    { from: "09:00", to: "14:00" },
    { from: "09:00", to: "14:00" },
    { from: "09:00", to: "14:00" },
    null,
    null,
];

// What to show about an employee's employment today, e.g. "Full time - 37.00 h/week",
// "Starts 01.12.2026" if they haven't started yet, or "Ended 31.10.2026" if they have left
export function employmentToday(employee: Employee) {
    const today = new Date();
    const first = employee.schedules[0];

    if (first && first.validFrom > today) {
        return "Starts " + formatDate(first.validFrom);
    }
    if (employee.endDate && employee.endDate < today) {
        return "Ended " + formatDate(employee.endDate);
    }
    const current = employee.schedules.find(period => period.validFrom <= today && (!period.validTo || period.validTo >= today));
    if (!current) return "No current schedule";
    return employmentText(current.days, current.employmentType);
}
