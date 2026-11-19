import { BrowserRouter, Routes, Route } from "react-router";
import { Layout, type NavItem } from "./components/layout";
import { AuthProvider, HomeRedirect, RequireRole } from "./components/auth";
import { LoginPage } from "./pages/login/LoginPage";
import { UserMain } from "./pages/user/UserMain";
import { RequestPage } from "./pages/user/RequestPage";
import { UserRequests } from "./pages/user/UserRequests";
import { AdminMain } from "./pages/admin/AdminMain";
import { AdminCalendar } from "./pages/admin/AdminCalendar";
import { Users } from "./pages/admin/Users";
import { AccountingMain } from "./pages/accounting/AccountingMain";
import { EmployeeList } from "./pages/accounting/EmployeeList";
import { EmployeeInfo } from "./pages/accounting/EmployeeInfo";
import "./index.css";

const userLinks: NavItem[] = [
    { to: "/user", label: "Overview" },
    { to: "/user/request", label: "Request holiday" },
    { to: "/user/requests", label: "My requests" },
];

const accountingLinks: NavItem[] = [
    { to: "/accounting", label: "Overview" },
    { to: "/accounting/employees", label: "Employees" },
];

const adminLinks: NavItem[] = [
    { to: "/admin", label: "Overview" },
    { to: "/admin/calendar", label: "Calendar" },
    { to: "/admin/users", label: "Users" },
];
export function App() {
    return (
        <BrowserRouter>
            <AuthProvider>
                <Routes>
                    <Route path="/" element={<HomeRedirect />} />
                    <Route path="/login" element={<LoginPage />} />

                    <Route path="/user" element={<RequireRole role="Employee"><Layout links={userLinks} /></RequireRole>}>
                        <Route index element={<UserMain />} />
                        <Route path="request" element={<RequestPage />} />
                        <Route path="requests" element={<UserRequests />} />
                    </Route>

                    <Route path="/admin" element={<RequireRole role="Admin"><Layout links={adminLinks} /></RequireRole>}>
                        <Route index element={<AdminMain />} />
                        <Route path="calendar" element={<AdminCalendar />} />
                        <Route path="users" element={<Users />} />
                    </Route>

                    <Route path="/accounting" element={<RequireRole role="Accounting"><Layout links={accountingLinks} /></RequireRole>}>
                        <Route index element={<AccountingMain />} />
                        <Route path="employees" element={<EmployeeList />} />
                        <Route path="employeesInfo" element={<EmployeeInfo />} />
                    </Route>
                </Routes>
            </AuthProvider>
        </BrowserRouter>
    );
}

export default App;