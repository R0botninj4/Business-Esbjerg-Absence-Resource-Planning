import type { Employee, WorkDay } from "../types/employee";

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

export function employmentText(days: WorkDay[]) {
    const hours = hoursPerWeek(days);
    const type = hours >= 37 ? "Full time" : "Part time";
    return type + " - " + hours.toFixed(2) + " h/week";
}


export function scheduleOn(employee: Employee, date: Date): WorkDay[] {
    let days: WorkDay[] = [null, null, null, null, null];
    for (const period of employee.schedules) {
        if (period.validFrom <= date) {
            days = period.days;
        }
    }
    return days;
}