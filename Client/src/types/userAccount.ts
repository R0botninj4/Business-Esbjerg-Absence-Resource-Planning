export type UserRole = "Employee" | "Accounting" | "Admin";

export type UserAccount = {
    id: number;
    email: string;
    role: UserRole;
    isActive: boolean;
};