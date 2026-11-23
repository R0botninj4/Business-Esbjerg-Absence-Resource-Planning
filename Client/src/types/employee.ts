export type WorkDay = { from: string; to: string } | null;

export type SchedulePeriod = {
    validFrom: Date;
    days: WorkDay[]; 
};

export type Employee = {
    id: number;
    name: string;
    email: string;
    schedules: SchedulePeriod[];
    endDate?: Date;
};