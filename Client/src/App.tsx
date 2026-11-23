import { useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router";
import { Layout, type NavItem } from "./components/layout";
import { AuthProvider, HomeRedirect, RequireRole, useAuth } from "./components/auth";
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

import { testEmployees } from "./mockdata/testEmployees";
import { CalendarProvider, useCalendar } from "./components/calendarData";
import { EmployeeProvider } from "./components/employeeData";
import { MySchedule } from "./pages/user/MySchedule";

import "./index.css";

const userLinks: NavItem[] = [
    { to: "/user", label: "Overview" },
    { to: "/user/request", label: "Request holiday" },
    { to: "/user/requests", label: "My requests" },
    { to: "/user/schedule", label: "My work schedule" },
];

const accountingLinks: NavItem[] = [
    { to: "/accounting", label: "Overview" },
    { to: "/accounting/schedule", label: "Work schedule" },
    { to: "/accounting/employees", label: "Employees" },
];

const adminLinks: NavItem[] = [
    { to: "/admin", label: "Rules" },
    { to: "/admin/calendar", label: "Calendar" },
    { to: "/admin/users", label: "Users" },
    { to: "/admin/employees", label: "Work schedules" },
];

export function App() {
    return <BrowserRouter><AuthProvider><CalendarProvider><AppRoutes /></CalendarProvider></AuthProvider></BrowserRouter>;
}

function AppRoutes() {
    const { events, error: calendarError, loading: calendarLoading } = useCalendar();
    const { user } = useAuth();
    // TODO: load from the backend instead of test data
    // All requests from all employees. Accounting sees all of them,
    // the employee pages only get the logged-in employee's own requests.
    const [requests, setRequests] = useState<HolidayRequest[]>(testRequests);
    const myRequests = requests.filter(r => r.employeeName === testCurrentEmployee);
    const employees = testEmployees; // Approval screen remains a separate integration task.

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
        <>
            {user && calendarError && <p role="alert" className="p-4 text-red-600">Calendar unavailable: {calendarError}</p>}
            {user && calendarLoading && <p role="status" className="p-4">Loading calendar...</p>}
                <Routes>
                    <Route path="/" element={<HomeRedirect />} />
                    <Route path="/login" element={<LoginPage />} />

                    <Route path="/user" element={<RequireRole role="Employee"><Layout links={userLinks} /></RequireRole>}>
                        <Route index element={<UserMain requests={myRequests} onCancel={cancelRequest} events={events} />} />
                        <Route path="request" element={<RequestPage requests={myRequests} onAdd={addRequest} onCancel={removeRequest} events={events} />} />
                        <Route path="requests" element={<UserRequests requests={myRequests} onCancel={cancelRequest} />} />
                        <Route path="schedule" element={<MySchedule key={user?.id} />} />
                    </Route>

                    <Route path="/admin" element={<RequireRole role="Admin"><Layout links={adminLinks} /></RequireRole>}>
                        <Route index element={<AdminMain />} />
                        <Route path="calendar" element={<AdminCalendar />} />
                        <Route path="users" element={<Users />} />
                        <Route path="employees" element={<EmployeeProvider><EmployeeList /></EmployeeProvider>} />
                        <Route path="employees/:id" element={<EmployeeProvider><EmployeeInfo events={events} /></EmployeeProvider>} />
                    </Route>

                    <Route path="/accounting" element={<RequireRole role="Accounting"><Layout links={accountingLinks} /></RequireRole>}>
                        <Route index element={<AccountingMain requests={requests} employees={employees} onDecide={decideRequest} events={events} />} />
                        <Route path="employees" element={<EmployeeProvider><EmployeeList /></EmployeeProvider>} />
                        <Route path="schedule" element={<EmployeeProvider><WorkSchedulePage events={events} /></EmployeeProvider>} />
                        <Route path="employees/:id" element={<EmployeeProvider><EmployeeInfo events={events} /></EmployeeProvider>} />
                    </Route>
                </Routes>
        </>
    );
}

export default App;