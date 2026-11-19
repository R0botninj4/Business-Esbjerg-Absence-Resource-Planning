import { useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router";
import { Layout, type NavItem } from "./components/layout";
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
import { testRequests } from "./mockdata/testRequest";
import type { HolidayRequest } from "./types/holidayRequest";
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
    // TODO: load from the backend instead of test data
    const [requests, setRequests] = useState<HolidayRequest[]>(testRequests);

    function addRequest(request: HolidayRequest) {
        setRequests([...requests, request]);
    }

    function removeRequest(id: number) {
        setRequests(requests.filter(r => r.id !== id));
    }

    function cancelRequest(id: number) {
        setRequests(requests.map(r => (r.id === id ? { ...r, status: "cancelled" } : r)));
    }

    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Navigate to="/user" replace />} />
                <Route path="/login" element={<LoginPage />} />

                <Route path="/user" element={<Layout links={userLinks} />}>
                    <Route index element={<UserMain />} />
                    <Route path="request" element={<RequestPage requests={requests} onAdd={addRequest} onCancel={removeRequest} />} />
                    <Route path="requests" element={<UserRequests requests={requests} onCancel={cancelRequest} />} />
                </Route>

                <Route path="/admin" element={<Layout links={adminLinks} />}>
                    <Route index element={<AdminMain />} />
                    <Route path="calendar" element={<AdminCalendar />} />
                    <Route path="users" element={<Users />} />
                </Route>

                <Route path="/accounting" element={<Layout links={accountingLinks} />}>
                    <Route index element={<AccountingMain />} />
                    <Route path="employees" element={<EmployeeList />} />
                    <Route path="employeesInfo" element={<EmployeeInfo />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}

export default App;