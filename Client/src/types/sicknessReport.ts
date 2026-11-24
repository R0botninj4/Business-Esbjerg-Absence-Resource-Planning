import type { HolidayType } from "./holidayRequest";

export type SicknessStatus = "pending" | "approved" | "declined";

// One working day the employee was sick, and how many hours that day is worth
export type SickDay = {
    date: Date;
    hours: number;
};

export type SicknessReport = {
    id: number;
    requestId: string; // the holiday the sickness belongs to
    employeeId: string;
    employeeName: string;
    holidayType: HolidayType;
    start: Date;
    end: Date;
    sickDays: SickDay[];
    beforeHoliday: boolean; // true = sick before the holiday started, false = sick during it
    doctorsNote?: { name: string; url: string };
    createdAt: Date;
    status: SicknessStatus;
    refundedHours?: number; // set by accounting when they approve
    comment?: string;
};