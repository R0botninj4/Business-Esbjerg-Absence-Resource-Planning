export type WorkDay = { from: string; to: string } | null;

export type Employee = {
    name: string;
    schedule: WorkDay[]; 
};