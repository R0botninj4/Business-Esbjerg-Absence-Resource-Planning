import type { HolidayRequest } from "../types/holidayRequest";

// The employee we pretend is logged in, until the backend gives us the real user
export const testCurrentEmployee = "Mette Jensen";

// Temporary test data – will be replaced by data from the API
export const testRequests: HolidayRequest[] = [
    {
        id: 1,
        employeeName: "Mette Jensen",
        holidayType: "weeks1to5",
        start: new Date(2026, 6, 6),
        end: new Date(2026, 6, 24),
        createdAt: new Date(2026, 2, 10),
        status: "approved",
    },
    {
        id: 2,
        employeeName: "Mette Jensen",
        holidayType: "weeks1to5",
        start: new Date(2026, 11, 21),
        end: new Date(2026, 11, 23),
        createdAt: new Date(2026, 9, 1),
        status: "approved",
    },
    {
        id: 3,
        employeeName: "Mette Jensen",
        holidayType: "week6",
        start: new Date(2026, 9, 19),
        end: new Date(2026, 9, 20),
        createdAt: new Date(2026, 8, 15),
        status: "declined",
    },
    {
        id: 4,
        employeeName: "Mette Jensen",
        holidayType: "week6",
        start: new Date(2026, 10, 2),
        end: new Date(2026, 10, 3),
        createdAt: new Date(2026, 9, 5),
        status: "cancelled",
    },

    // Requests from colleagues (so accounting has something to approve)
    {
        id: 5,
        employeeName: "Sofie Madsen",
        holidayType: "week6",
        start: new Date(2026, 10, 30),
        end: new Date(2026, 11, 2),
        createdAt: new Date(2026, 10, 18),
        status: "pending",
    },
    {
        id: 6,
        employeeName: "Jonas Berg",
        holidayType: "weeks1to5",
        start: new Date(2026, 11, 21),
        end: new Date(2026, 11, 23),
        createdAt: new Date(2026, 10, 20),
        status: "pending",
    },
    {
        id: 7,
        employeeName: "Anne Kirk",
        holidayType: "weeks1to5",
        start: new Date(2026, 11, 28),
        end: new Date(2026, 11, 30),
        createdAt: new Date(2026, 9, 2),
        status: "approved",
    },
    {
        id: 8,
        employeeName: "Lars Holm",
        holidayType: "week6",
        start: new Date(2026, 11, 22),
        end: new Date(2026, 11, 22),
        createdAt: new Date(2026, 9, 5),
        status: "approved",
    },
];