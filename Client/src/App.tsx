import { useEffect, useState } from "react";
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
import { createHolidayRequest, getHolidayRequests, withdrawHolidayRequest, type NewHolidayRequest } from "./api/holidayRequests";
import { userError } from "./api/users";
import type { HolidayRequest } from "./types/holidayRequest";
import { WorkSchedulePage } from "./pages/accounting/WorkSchedulePage";

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
    return <BrowserRouter><AuthProvider><CalendarProvider><SessionRoutes /></CalendarProvider></AuthProvider></BrowserRouter>;
}

function SessionRoutes() {
    const { user } = useAuth();
    // A new login gets fresh state, so requests from the last account cannot stay on screen.
    return <AppRoutes key={user?.id || "logged-out"} />;
}

function AppRoutes() {
    const { events, error: calendarError, loading: calendarLoading } = useCalendar();
    const { user } = useAuth();
    const [requests, setRequests] = useState<HolidayRequest[]>([]);
    const [requestsLoading, setRequestsLoading] = useState(false);
    const [requestsError, setRequestsError] = useState("");

    useEffect(() => {
        let cancelled = false;
        if (!user || (user.role !== "Employee" && user.role !== "Accounting")) return;
        setRequestsLoading(true);
        getHolidayRequests(user.role === "Accounting")
            .then(saved => { if (!cancelled) setRequests(saved); })
            .catch(error => { if (!cancelled) setRequestsError(userError(error)); })
            .finally(() => { if (!cancelled) setRequestsLoading(false); });
        return () => { cancelled = true; };
    }, [user?.id]);

    async function addRequest(input: NewHolidayRequest) {
        const saved = await createHolidayRequest(input);
        setRequests(current => [saved, ...current]);
    }

    async function cancelRequest(id: string) {
        await withdrawHolidayRequest(id);
        setRequests(current => current.map(request => request.id === id ? { ...request, status: "cancelled" } : request));
    }
    return (
        <>
            {requestsLoading && <p role="status" className="p-4">Loading requests...</p>}
            {requestsError && <p role="alert" className="p-4 text-red-600">{requestsError} Reload the page to try again.</p>}
            {user && calendarError && <p role="alert" className="p-4 text-red-600">Calendar unavailable: {calendarError}</p>}
            {user && calendarLoading && <p role="status" className="p-4">Loading calendar...</p>}
                <Routes>
                    <Route path="/" element={<HomeRedirect />} />
                    <Route path="/login" element={<LoginPage />} />

                    <Route path="/user" element={<RequireRole role="Employee"><Layout links={userLinks} /></RequireRole>}>
                        <Route index element={<UserMain requests={requests} onCancel={cancelRequest} events={events} />} />
                        <Route path="request" element={<RequestPage requests={requests} onAdd={addRequest} onCancel={cancelRequest} events={events} />} />
                        <Route path="requests" element={<UserRequests requests={requests} onCancel={cancelRequest} />} />
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
                        <Route index element={<EmployeeProvider><AccountingMain requests={requests} events={events} /></EmployeeProvider>} />
                        <Route path="employees" element={<EmployeeProvider><EmployeeList /></EmployeeProvider>} />
                        <Route path="schedule" element={<EmployeeProvider><WorkSchedulePage events={events} /></EmployeeProvider>} />
                        <Route path="employees/:id" element={<EmployeeProvider><EmployeeInfo events={events} /></EmployeeProvider>} />
                    </Route>
                </Routes>
        </>
    );
}

export default App;