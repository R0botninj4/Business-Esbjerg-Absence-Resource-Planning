import type { Rule } from "../types/rule";

// Temporary test data – the rules will come from the backend later (#16)
export const testRules: Rule[] = [
    {
        id: "weeks1to5-earned",
        group: "Week 1–5",
        name: "Days earned per month",
        versions: [{ value: "2.08 days", effectiveFrom: new Date(2020, 8, 1), reason: "Ferieloven" }],
    },
    {
        id: "weeks1to5-periods",
        group: "Week 1–5",
        name: "Earning / usage period",
        versions: [{ value: "1 Sep – 31 Aug / until 31 Dec the following year", effectiveFrom: new Date(2020, 8, 1), reason: "Ferieloven" }],
    },
    {
        id: "weeks1to5-transfer",
        group: "Week 1–5",
        name: "Transfer of 5th week",
        versions: [{ value: "Off (max 5 days, deadline 31 Dec)", effectiveFrom: new Date(2020, 8, 1), reason: "Company policy" }],
    },
    {
        id: "week6-enabled",
        group: "Week 6",
        name: "6th holiday week",
        versions: [{ value: "On", effectiveFrom: new Date(2020, 0, 1), reason: "KL 05.11" }],
    },
    {
        id: "week6-earned",
        group: "Week 6",
        name: "Hours earned per month (full time)",
        versions: [{ value: "3.083 h", effectiveFrom: new Date(2020, 0, 1), reason: "KL 05.11" }],
    },
    {
        id: "week6-periods",
        group: "Week 6",
        name: "Earning / usage period",
        versions: [{ value: "1 Jan – 31 Dec / 1 May – 30 Apr the following year", effectiveFrom: new Date(2020, 0, 1), reason: "KL 05.11" }],
    },
    {
        id: "week6-transfer",
        group: "Week 6",
        name: "Transfer deadline",
        versions: [{ value: "30 Sep", effectiveFrom: new Date(2020, 0, 1), reason: "KL 05.11" }],
    },
    {
        id: "main-holiday",
        group: "General",
        name: "Main holiday",
        versions: [{ value: "15 days in a row, 1 May – 30 Sep", effectiveFrom: new Date(2020, 8, 1), reason: "Ferieloven" }],
    },
    {
        id: "notice",
        group: "General",
        name: "Notice",
        versions: [{ value: "3 months (main holiday) / 1 month (other)", effectiveFrom: new Date(2020, 8, 1), reason: "Ferieloven" }],
    },
    {
        id: "request-unit",
        group: "General",
        name: "Request unit",
        versions: [{ value: "Whole days (deducted in hours)", effectiveFrom: new Date(2020, 8, 1), reason: "Company policy" }],
    },
    {
        id: "holiday-debt",
        group: "General",
        name: "Max holiday debt",
        versions: [
            { value: "Not allowed", effectiveFrom: new Date(2020, 8, 1), reason: "Company policy" },
            { value: "1 week (5 days)", effectiveFrom: new Date(2024, 0, 1), reason: "New company policy" },
        ],
    },
    {
        id: "balance-31-august",
        group: "General",
        name: "Balance on 31 August",
        versions: [{ value: "Must be 0 or more", effectiveFrom: new Date(2024, 0, 1), reason: "Company policy" }],
    },
];

// The presets an admin can start from (hourly-paid staff are not part of the system)
export const presets = [
    { name: "Ferieloven (minimum)", description: "5 weeks, nothing else." },
    { name: "Ferieloven + 6. ferieuge", description: "KL 05.11 model. What Business Esbjerg uses." },
    { name: "Ferieloven + feriefridage", description: "E.g. Industriens Overenskomst with 5 extra days." },
];