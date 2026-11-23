import { api } from "./client";
import { ContentType } from "./Api";
import type { WorkDay } from "../types/employee";

export type SavedUser = {
    id: string;
    email: string;
    role: "Employee" | "Accounting" | "Admin";
    isActive: boolean;
};

export type SavedEmployment = {
    id: string;
    userId: string;
    type: number;
    startDate: string;
    endDate: string | null;
};

export type NewEmployee = {
    email: string;
    password: string;
    type: number;
    startDate: string;
    endDate: string | null;
    days: WorkDay[];
};

export function getUsers() {
    return api.request<SavedUser[]>({ path: "/api/users", method: "GET", format: "json" });
}

export function getEmployments(userId: string) {
    return api.request<SavedEmployment[]>({
        path: `/api/users/${userId}/employments`, method: "GET", format: "json",
    });
}

export function createEmployee(employee: NewEmployee) {
    // Frontendens liste starter med mandag på plads 0. API'et bruger mandag = 1.
    const days = employee.days.map((day, index) => ({
        day: index + 1,
        isDayOff: day === null,
        startTime: day ? day.from + ":00" : null,
        endTime: day ? day.to + ":00" : null,
    }));
    return api.request<SavedUser>({
        path: "/api/employees", method: "POST", type: ContentType.Json, format: "json",
        body: { ...employee, days },
    });
}

export function changeUserEmail(user: SavedUser, email: string) {
    return api.request<void>({
        path: `/api/users/${user.id}`, method: "PUT", type: ContentType.Json, format: "json",
        body: { email, role: user.role },
    });
}

export function deactivateUser(id: string) {
    return api.request<void>({ path: `/api/users/${id}`, method: "DELETE", format: "json" });
}

export function reactivateUser(id: string) {
    return api.request<void>({ path: `/api/users/${id}/reactivate`, method: "POST", format: "json" });
}

export function userError(error: unknown): string {
    // Den fælles API-klient lægger serverens JSON-fejl i feltet error.
    const response = error as { status?: number; error?: { message?: string; errors?: Record<string, string[]> } };
    if (response?.status === 401) return "Your session has expired. Please log in again.";
    if (response?.status === 403) return "You do not have permission to perform this action.";
    if (response?.error?.message) return response.error.message;
    if (response?.error?.errors) return Object.values(response.error.errors).flat().join(" ");
    return "The change could not be completed. Please check the connection and try again.";
}
