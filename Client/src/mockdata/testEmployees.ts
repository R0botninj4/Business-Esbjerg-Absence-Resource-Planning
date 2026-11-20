import type { Employee } from "../types/employee";

// Temporary test data – will come from the backend later (#7)
export const testEmployees: Employee[] = [
    {
        name: "Mette Jensen",
        schedule: [
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:00" },
        ],
    },
    {
        name: "Jonas Berg",
        schedule: [
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:30" },
            { from: "08:00", to: "15:00" },
        ],
    },
    {
        name: "Sofie Madsen", // part-time: Monday to Wednesday
        schedule: [
            { from: "09:00", to: "14:00" },
            { from: "09:00", to: "14:00" },
            { from: "09:00", to: "14:00" },
            null,
            null,
        ],
    },
    {
        name: "Anne Kirk",
        schedule: [
            { from: "07:30", to: "15:00" },
            { from: "07:30", to: "15:00" },
            { from: "07:30", to: "15:00" },
            { from: "07:30", to: "15:00" },
            { from: "07:30", to: "14:30" },
        ],
    },
    {
        name: "Lars Holm",
        schedule: [
            { from: "08:30", to: "16:00" },
            { from: "08:30", to: "16:00" },
            { from: "08:30", to: "16:00" },
            { from: "08:30", to: "16:00" },
            { from: "08:30", to: "15:30" },
        ],
    },
];