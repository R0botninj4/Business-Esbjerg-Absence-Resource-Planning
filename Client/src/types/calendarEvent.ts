export type CalendarEvent = {
    date: Date;
    title: string;
    type: "holiday" | "companyDayOff" | "info";
};