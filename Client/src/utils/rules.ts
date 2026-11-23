import type { Rule } from "../types/rule";

export function currentVersion(rule: Rule, date: Date) {
    let current = rule.versions[0];
    for (const version of rule.versions) {
        if (version.effectiveFrom <= date) {
            current = version;
        }
    }
    return current;
}

export function upcomingVersions(rule: Rule, date: Date) {
    return rule.versions.filter(version => version.effectiveFrom > date);
}