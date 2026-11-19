export type HolidayType = "weeks1to5" | "week6";

export type RequestStatus = "pending" | "approved" | "declined" | "cancelled";

export type HolidayRequest = {
    id: number;
    holidayType: HolidayType;
    start: Date;
    end: Date;
    createdAt: Date;
    status: RequestStatus;
};