// Temporary test data – this year's main holiday (hovedferie).
// Set it to null to see how the box looks when it isn't planned yet.
// Will be worked out by the backend later (#29)
export const testMainHoliday: { start: Date; end: Date } | null = {
    start: new Date(2026, 6, 6),
    end: new Date(2026, 6, 24),
};