import { api } from "./client";
import { ContentType } from "./Api";

export type RuleSettings = {
    sicknessWaitingDays: number;
    name: string;
    holidayDaysPerMonth: number;
    holidayDaysPerYear: number;
    sixthWeekHoursPerCalendarDay: number;
    sixthWeekEnabled: boolean;
    sixthWeekHoursPerMonth: number;
    fullTimeHoursPerWeek: number;
    maximumDebtDays: number;
    allowTransfer: boolean;
    maximumTransferDays: number;
    holidayEarningStartMonth: number;
    holidayEarningStartDay: number;
    holidayUsageStartMonth: number;
    holidayUsageStartDay: number;
    holidayUsageStartYearOffset: number;
    holidayUsageEndMonth: number;
    holidayUsageEndDay: number;
    holidayUsageEndYearOffset: number;
    sixthWeekEarningStartMonth: number;
    sixthWeekEarningStartDay: number;
    sixthWeekUsageStartMonth: number;
    sixthWeekUsageStartDay: number;
    sixthWeekUsageStartYearOffset: number;
    sixthWeekUsageEndMonth: number;
    sixthWeekUsageEndDay: number;
    sixthWeekUsageEndYearOffset: number;
    mainHolidayDays: number;
    mainHolidayStartMonth: number;
    mainHolidayStartDay: number;
    mainHolidayEndMonth: number;
    mainHolidayEndDay: number;
    mainHolidayNoticeMonths: number;
    otherHolidayNoticeMonths: number;
    debtDeadlineMonth: number;
    debtDeadlineDay: number;
    holidayTransferDeadlineMonth: number;
    holidayTransferDeadlineDay: number;
    sixthWeekTransferDeadlineMonth: number;
    sixthWeekTransferDeadlineDay: number;
};

export type RuleVersion = {
    sicknessWaitingDays: number;
    id: string;
    effectiveFrom: string;
    version: number;
    createdAt: string;
    createdBy: string;
    name: string;
    holidayDaysPerMonth: number;
    holidayDaysPerYear: number | null;
    sixthWeekHoursPerCalendarDay: number | null;
    sixthWeekEnabled: boolean;
    sixthWeekHoursPerMonth: number;
    fullTimeHoursPerWeek: number;
    maximumDebtDays: number;
    allowTransfer: boolean;
    maximumTransferDays: number;
    periods: Omit<RuleSettings, "sicknessWaitingDays" | "name" | "holidayDaysPerMonth" | "holidayDaysPerYear" | "sixthWeekHoursPerCalendarDay" | "sixthWeekEnabled" | "sixthWeekHoursPerMonth" | "fullTimeHoursPerWeek" | "maximumDebtDays" | "allowTransfer" | "maximumTransferDays"> | null;
};

export function getRuleHistory() {
    return api.request<RuleVersion[]>({ path: "/api/holiday-rules", method: "GET", format: "json" });
}

export function getRulePresets() {
    return api.request<RuleSettings[]>({ path: "/api/holiday-rules/presets", method: "GET", format: "json" });
}

export function createRule(effectiveFrom: string, settings: RuleSettings) {
    return api.request<RuleVersion>({
        path: "/api/holiday-rules", method: "POST", type: ContentType.Json, format: "json",
        body: { effectiveFrom, settings },
    });
}

// Ingen standardtal indsaettes i en gammel, ufuldstaendig version.
export function settingsFromVersion(version: RuleVersion): RuleSettings | null {
    if (!version.periods || version.holidayDaysPerYear === null || version.sixthWeekHoursPerCalendarDay === null) return null;
    return {
        ...version.periods,
        sicknessWaitingDays: version.sicknessWaitingDays,
        name: version.name, holidayDaysPerMonth: version.holidayDaysPerMonth,
        holidayDaysPerYear: version.holidayDaysPerYear,
        sixthWeekHoursPerCalendarDay: version.sixthWeekHoursPerCalendarDay,
        sixthWeekEnabled: version.sixthWeekEnabled, sixthWeekHoursPerMonth: version.sixthWeekHoursPerMonth,
        fullTimeHoursPerWeek: version.fullTimeHoursPerWeek, maximumDebtDays: version.maximumDebtDays,
        allowTransfer: version.allowTransfer, maximumTransferDays: version.maximumTransferDays,
    };
}
