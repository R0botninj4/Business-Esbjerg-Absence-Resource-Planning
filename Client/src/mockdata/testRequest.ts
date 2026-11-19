import type { HolidayRequest } from "../types/holidayRequest";

// Temporary test data – will be replaced by data from the API
export const testRequests: HolidayRequest[] = [
    {
        id: 1,
        holidayType: "weeks1to5",
        start: new Date(2026, 6, 6),
        end: new Date(2026, 6, 24),
        createdAt: new Date(2026, 2, 10),
        status: "approved",
    },
    {
        id: 2,
        holidayType: "weeks1to5",
        start: new Date(2026, 11, 21),
        end: new Date(2026, 11, 23),
        createdAt: new Date(2026, 9, 1),
        status: "approved",
    },
    {
        id: 3,
        holidayType: "week6",
        start: new Date(2026, 9, 19),
        end: new Date(2026, 9, 20),
        createdAt: new Date(2026, 8, 15),
        status: "declined",
    },
    {
        id: 4,
        holidayType: "week6",
        start: new Date(2026, 10, 2),
        end: new Date(2026, 10, 3),
        createdAt: new Date(2026, 9, 5),
        status: "cancelled",
    },
];