export function weekdayIndex(date: Date) {
    let index = date.getDay() - 1;
    if (index === -1) index = 6;
    return index;
}

export function startOfWeek(date: Date) {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate() - weekdayIndex(date));
}

export function weekNumber(date: Date) {
    const thursday = new Date(date.getFullYear(), date.getMonth(), date.getDate() - weekdayIndex(date) + 3);
    const firstOfJanuary = new Date(thursday.getFullYear(), 0, 1);
    const daysSince = Math.round((thursday.getTime() - firstOfJanuary.getTime()) / (24 * 60 * 60 * 1000));
    return Math.floor(daysSince / 7) + 1;
}

export function mondayOfWeek(year: number, week: number) {
    const week1Monday = startOfWeek(new Date(year, 0, 4));
    return new Date(week1Monday.getFullYear(), week1Monday.getMonth(), week1Monday.getDate() + (week - 1) * 7);
}

export function weekYear(date: Date) {
    const thursday = new Date(date.getFullYear(), date.getMonth(), date.getDate() - weekdayIndex(date) + 3);
    return thursday.getFullYear();
}