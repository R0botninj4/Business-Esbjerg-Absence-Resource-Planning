import type { HolidayType } from "../types/holidayRequest";
export function formatDate(date: Date | null) {
    if (!date) return "-";
    return date.toLocaleDateString("da-DK", { day: "2-digit", month: "2-digit", year: "numeric" });
}
export function holidayTypeName(holidayType: HolidayType) {
    return holidayType === "week6" ? "Week 6" : "Week 1–5";
}