import { api } from "./client";
import { ContentType } from "./Api";
import type { HolidayRequest, RequestStatus } from "../types/holidayRequest";

export type NewHolidayRequest = { account: number; startDate: string; endDate: string };
type SavedRequest = {
    id: string; userId: string; email: string; account: number;
    startDate: string; endDate: string; hours: number; status: RequestStatus; createdAt: string; comment?: string;
    cancellationStatus?: string; cancellationRequestedAt?: string; cancellationComment?: string;
};

function toRequest(saved: SavedRequest): HolidayRequest {
    return {
        id: saved.id, employeeId: saved.userId, employeeName: saved.email,
        holidayType: saved.account === 1 ? "weeks1to5" : "week6",
        start: new Date(saved.startDate + "T00:00"), end: new Date(saved.endDate + "T00:00"),
        createdAt: new Date(saved.createdAt), status: saved.status, hours: Number(saved.hours), comment: saved.comment,
        cancellationStatus: saved.cancellationStatus,
        cancellationRequestedAt: saved.cancellationRequestedAt ? new Date(saved.cancellationRequestedAt) : undefined,
        cancellationComment: saved.cancellationComment,
    };
}

export async function getHolidayRequests(accounting: boolean) {
    let path = "/api/holiday-requests";
    if (accounting) {
        path += "/review";
    }
    const saved = await api.request<SavedRequest[]>({ path, method: "GET", format: "json" });
    return saved.map(toRequest);
}

export async function createHolidayRequest(input: NewHolidayRequest) {
    const saved = await api.request<SavedRequest>({
        path: "/api/holiday-requests", method: "POST", type: ContentType.Json, format: "json", body: input,
    });
    return toRequest(saved);
}

export function previewHolidayRequest(input: NewHolidayRequest) {
    return api.request<{ hours: number }>({
        path: "/api/holiday-requests/preview", method: "POST", type: ContentType.Json, format: "json", body: input,
    });
}

export function withdrawHolidayRequest(id: string) {
    return api.request<void>({ path: `/api/holiday-requests/${id}`, method: "DELETE", format: "json" });
}

export async function decideHolidayRequest(id: string, approve: boolean, comment: string) {
    const saved = await api.request<SavedRequest>({
        path: `/api/holiday-requests/${id}/decision`,
        method: "POST",
        type: ContentType.Json,
        format: "json",
        body: { approve, comment },
    });
    return toRequest(saved);
}

export async function requestHolidayCancellation(id: string) {
    const saved = await api.request<SavedRequest>({
        path: `/api/holiday-requests/${id}/cancellation`, method: "POST", format: "json",
    });
    return toRequest(saved);
}

export async function decideHolidayCancellation(id: string, approve: boolean, comment: string) {
    const saved = await api.request<SavedRequest>({
        path: `/api/holiday-requests/${id}/cancellation/decision`,
        method: "POST", type: ContentType.Json, format: "json", body: { approve, comment },
    });
    return toRequest(saved);
}
