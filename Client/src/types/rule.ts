export type RuleVersion = {
    value: string;
    effectiveFrom: Date;
    reason: string;
};

export type Rule = {
    id: string;
    group: "Week 1–5" | "Week 6" | "General";
    name: string;
    versions: RuleVersion[]; 
};