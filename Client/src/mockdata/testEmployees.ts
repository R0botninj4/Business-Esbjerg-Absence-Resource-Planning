import type { Employee, WorkDay } from "../types/employee";

// Two schedules that many employees use (Mon–Thu 7.5 h, Fri 7 h = 37 h a week)
const fullTime: WorkDay[] = [
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:30" },
    { from: "08:00", to: "15:00" },
];

const partTime: WorkDay[] = [
    { from: "09:00", to: "14:00" },
    { from: "09:00", to: "14:00" },
    { from: "09:00", to: "14:00" },
    null,
    null,
];

// Temporary test data – will come from the backend later (#7)
export const testEmployees: Employee[] = [
    {
        id: 1,
        name: "Mette Jensen",
        email: "mette@businessesbjerg.com",
        schedules: [{ validFrom: new Date(2022, 0, 1), days: fullTime }],
    },
    {
        id: 2,
        name: "Jonas Berg",
        email: "jonas@businessesbjerg.com",
        schedules: [{ validFrom: new Date(2023, 7, 1), days: fullTime }],
    },
    {
        id: 3,
        name: "Sofie Madsen",
        email: "sofie@businessesbjerg.com",
        // Went from full time to part time in March 2026
        schedules: [
            { validFrom: new Date(2023, 1, 1), days: fullTime },
            { validFrom: new Date(2026, 2, 1), days: partTime },
        ],
    },
    {
        id: 4,
        name: "Anne Kirk",
        email: "anne@businessesbjerg.com",
        schedules: [
            {
                validFrom: new Date(2021, 8, 1),
                days: [
                    { from: "07:30", to: "15:00" },
                    { from: "07:30", to: "15:00" },
                    { from: "07:30", to: "15:00" },
                    { from: "07:30", to: "15:00" },
                    { from: "07:30", to: "14:30" },
                ],
            },
        ],
    },
    {
        id: 5,
        name: "Lars Holm",
        email: "lars@businessesbjerg.com",
        schedules: [
            {
                validFrom: new Date(2024, 3, 1),
                days: [
                    { from: "08:30", to: "16:00" },
                    { from: "08:30", to: "16:00" },
                    { from: "08:30", to: "16:00" },
                    { from: "08:30", to: "16:00" },
                    { from: "08:30", to: "15:30" },
                ],
            },
        ],
    },
];