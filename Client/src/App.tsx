import { useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router";
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
import { testCurrentEmployee, testRequests } from "./mockdata/testRequest";
import type { HolidayRequest, RequestStatus } from "./types/holidayRequest";
import { WorkSchedulePage } from "./pages/accounting/WorkSchedulePage";
import "./index.css";

const userLinks: NavItem[] = [
    { to: "/user", label: "Overview" },
    { to: "/user/request", label: "Request holiday" },
    { to: "/user/requests", label: "My requests" },
];

const accountingLinks: NavItem[] = [
    { to: "/accounting", label: "Overview" },
    { to: "/accounting/schedule", label: "Work schedule" },
    { to: "/accounting/employees", label: "Employees" },
];

const adminLinks: NavItem[] = [
    { to: "/admin", label: "Overview" },
    { to: "/admin/calendar", label: "Calendar" },
    { to: "/admin/users", label: "Users" },
];

export function App() {
    // TODO: load from the backend instead of test data
    // All requests from all employees. Accounting sees all of them,
    // the employee pages only get the logged-in employee's own requests.
    const [requests, setRequests] = useState<HolidayRequest[]>(testRequests);
    const myRequests = requests.filter(r => r.employeeName === testCurrentEmployee);

    function addRequest(request: HolidayRequest) {
        setRequests([...requests, request]);
    }

    function removeRequest(id: number) {
        setRequests(requests.filter(r => r.id !== id));
    }

    function cancelRequest(id: number) {
        setRequests(requests.map(r => (r.id === id ? { ...r, status: "cancelled" } : r)));
    }
    
    function decideRequest(id: number, status: RequestStatus, comment: string) {
        setRequests(requests.map(r => (r.id === id ? { ...r, status: status, comment: comment } : r)));
    }

    return (
        <BrowserRouter>
            <AuthProvider>
                <Routes>
                    <Route path="/" element={<HomeRedirect />} />
                    <Route path="/login" element={<LoginPage />} />

                    <Route path="/user" element={<Layout links={userLinks} />}>
                        <Route index element={<UserMain requests={myRequests} onCancel={cancelRequest} />} />
                        <Route path="request" element={<RequestPage requests={myRequests} onAdd={addRequest} onCancel={removeRequest} />} />
                        <Route path="requests" element={<UserRequests requests={myRequests} onCancel={cancelRequest} />} />
                    </Route>

                    <Route path="/admin" element={<RequireRole role="Admin"><Layout links={adminLinks} /></RequireRole>}>
                        <Route index element={<AdminMain />} />
                        <Route path="calendar" element={<AdminCalendar />} />
                        <Route path="users" element={<Users />} />
                    </Route>

                    <Route path="/accounting" element={<RequireRole role="Accounting"><Layout links={accountingLinks} /></RequireRole>}>
                        <Route index element={<AccountingMain requests={requests} onDecide={decideRequest} />} />
                        <Route path="employees" element={<EmployeeList />} />
                        <Route path="schedule" element={<WorkSchedulePage requests={requests} />} />
                        <Route path="employeesInfo" element={<EmployeeInfo />} />
                    </Route>
                </Routes>
            </AuthProvider>
        </BrowserRouter>
    );
}

export default App;