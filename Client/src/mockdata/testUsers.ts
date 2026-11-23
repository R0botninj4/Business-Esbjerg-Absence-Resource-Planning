import type { UserAccount } from "../types/userAccount";

// Temporary test data – will come from the backend later (/api/users).
// Employees are linked to their account by email.
export const testUsers: UserAccount[] = [
    { id: 1, email: "admin@test.dk", role: "Admin", isActive: true },
    { id: 2, email: "accounting@test.dk", role: "Accounting", isActive: true },
    { id: 3, email: "mette@businessesbjerg.com", role: "Employee", isActive: true },
    { id: 4, email: "jonas@businessesbjerg.com", role: "Employee", isActive: true },
    { id: 5, email: "sofie@businessesbjerg.com", role: "Employee", isActive: true },
    { id: 6, email: "anne@businessesbjerg.com", role: "Employee", isActive: true },
    { id: 7, email: "lars@businessesbjerg.com", role: "Employee", isActive: true },
];