import { api } from "./client";
import { ContentType } from "./Api";
import { getEmployments, type SavedUser, type SavedEmployment } from "./users";
import type { Employee, SchedulePeriod, WorkDay } from "../types/employee";

export type SavedSchedule = { id: string; employmentId: string; startDate: string; endDate: string | null };
type SavedDay = { day: number; isDayOff: boolean; startTime: string | null; endTime: string | null };

export function dateText(date: Date) {
    return date.getFullYear() + "-" + String(date.getMonth() + 1).padStart(2, "0") + "-" + String(date.getDate()).padStart(2, "0");
}

export async function loadEmployee(user: SavedUser): Promise<Employee> {
    const employments = await getEmployments(user.id);
    const periods: SchedulePeriod[] = [];
    for (const employment of employments) {
        const plans = await api.request<SavedSchedule[]>({
            path: `/api/employments/${employment.id}/work-schedules`, method: "GET", format: "json",
        });
        for (const plan of plans) {
            const savedDays = await api.request<SavedDay[]>({
                path: `/api/employments/${employment.id}/work-schedules/${plan.id}/days`, method: "GET", format: "json",
            });
            const days: WorkDay[] = [null, null, null, null, null];
            for (const day of savedDays) {
                if (!day.isDayOff && day.startTime && day.endTime) {
                    days[day.day - 1] = { from: day.startTime.slice(0, 5), to: day.endTime.slice(0, 5) };
                }
            }
            periods.push({
                id: plan.id, employmentId: employment.id, employmentType: employment.type,
                validFrom: new Date(plan.startDate + "T00:00"),
                validTo: plan.endDate ? new Date(plan.endDate + "T23:59:59.999") : undefined,
                days,
            });
        }
    }
    periods.sort((a, b) => a.validFrom.getTime() - b.validFrom.getTime());
    return { id: user.id, name: user.email, email: user.email, schedules: periods };
}

export async function getEmployees() {
    const users = await api.request<SavedUser[]>({ path: "/api/employees", method: "GET", format: "json" });
    const employees: Employee[] = [];
    for (const user of users) employees.push(await loadEmployee(user));
    return employees;
}

export function saveSchedule(employment: SavedEmployment, periods: SchedulePeriod[], period: SchedulePeriod) {
    const date = period.validFrom;
    const previous = periods.find(plan => plan.employmentId === employment.id && plan.validFrom <= date && (!plan.validTo || plan.validTo >= date));
    const days = period.days.map((day, index) => ({
        day: index + 1, isDayOff: day === null,
        startTime: day ? day.from + ":00" : null, endTime: day ? day.to + ":00" : null,
    }));
    const path = `/api/employments/${employment.id}/work-schedules`;
    if (previous) {
        return api.request<SavedSchedule>({
            path: path + "/" + previous.id, method: "PUT", type: ContentType.Json, format: "json",
            body: { startDate: dateText(date), days },
        });
    }
    return api.request<SavedSchedule>({
        path, method: "POST", type: ContentType.Json, format: "json",
        body: { startDate: dateText(date), endDate: employment.endDate, days },
    });
}
