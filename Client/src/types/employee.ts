export type WorkDay = { from: string; to: string } | null;

export type SchedulePeriod = {
    id?: string;
    employmentId?: string;
    employmentType?: number;
    validFrom: Date;
    validTo?: Date;
    days: WorkDay[]; 
};

export type Employee = {
    id: string | number;
    name: string;
    email: string;
    schedules: SchedulePeriod[];
    endDate?: Date;
};
