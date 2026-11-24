import type { HolidayType } from "../types/holidayRequest";
export function displayName(name: string) {
    const domain = "@businessesbjerg.com";
    if (name.toLowerCase().endsWith(domain)) {
        return name.slice(0, -domain.length);
    }
    return name;
}

export function formatDate(date: Date | null) {
    if (!date) return "-";
    return date.toLocaleDateString("da-DK", { day: "2-digit", month: "2-digit", year: "numeric" });
}
export function holidayTypeName(holidayType: HolidayType) {
    return holidayType === "week6" ? "Week 6" : "Week 1–5";
}
