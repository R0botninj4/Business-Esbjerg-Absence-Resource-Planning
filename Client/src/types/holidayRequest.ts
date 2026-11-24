export type HolidayType = "weeks1to5" | "week6";

export type RequestStatus = "pending" | "approved" | "declined" | "cancelled";

export type HolidayRequest = {
    id: string;
    employeeId: string;
    employeeName: string;
    holidayType: HolidayType;
    start: Date;
    end: Date;
    createdAt: Date;
    status: RequestStatus;
    hours: number;
    comment?: string;
};
