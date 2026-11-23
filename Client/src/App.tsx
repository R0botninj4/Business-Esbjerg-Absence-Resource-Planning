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
import type { Employee, SchedulePeriod } from "./types/employee";
import { testEmployees } from "./mockdata/testEmployees";
import { testEvents } from "./mockdata/testEvents";
import { testRules } from "./mockdata/testRules";
import type { CalendarEvent } from "./types/calendarEvent";
import type { Rule, RuleVersion } from "./types/rule";
import { testUsers } from "./mockdata/testUsers";
import type { UserAccount } from "./types/userAccount";
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
    { to: "/admin", label: "Rules" },
    { to: "/admin/calendar", label: "Calendar" },
    { to: "/admin/users", label: "Users" },
];

export function App() {
    // TODO: load from the backend instead of test data
    // All requests from all employees. Accounting sees all of them,
    // the employee pages only get the logged-in employee's own requests.
    const [requests, setRequests] = useState<HolidayRequest[]>(testRequests);
    const myRequests = requests.filter(r => r.employeeName === testCurrentEmployee);
    const [employees, setEmployees] = useState<Employee[]>(testEmployees);
    const [events, setEvents] = useState<CalendarEvent[]>(testEvents);
    const [rules, setRules] = useState<Rule[]>(testRules);
    const [users, setUsers] = useState<UserAccount[]>(testUsers);

    function addRequest(request: HolidayRequest) {
        setRequests([...requests, request]);
    }

    function removeRequest(id: number) {
        setRequests(requests.filter(r => r.id !== id));
    }

    function cancelRequest(id: number) {
        setRequests(requests.map(r => (r.id === id ? { ...r, status: "cancelled" } : r)));
    }

    function addSchedulePeriod(employeeId: number, period: SchedulePeriod) {
        setEmployees(employees.map(e => {
            if (e.id !== employeeId) return e;

            // Replace a period that starts on the same day, then keep the list sorted (oldest first)
            const others = e.schedules.filter(p => p.validFrom.getTime() !== period.validFrom.getTime());
            const schedules = [...others, period].sort((a, b) => a.validFrom.getTime() - b.validFrom.getTime());
            return { ...e, schedules: schedules };
        }));
    }
    
    function addEvent(event: CalendarEvent) {
        setEvents([...events, event]);
    }

    function deleteEvent(event: CalendarEvent) {
        setEvents(events.filter(e => e !== event));
    }
    
    function addRuleVersion(ruleId: string, version: RuleVersion) {
        setRules(rules.map(rule => {
            if (rule.id !== ruleId) return rule;
            const versions = [...rule.versions, version].sort((a, b) => a.effectiveFrom.getTime() - b.effectiveFrom.getTime());
            return { ...rule, versions: versions };
        }));
    }
    
    function decideRequest(id: number, status: RequestStatus, comment: string) {
        setRequests(requests.map(r => (r.id === id ? { ...r, status: status, comment: comment } : r)));
    }
    
    function createEmployee(employee: Omit<Employee, "id">) {
        const newUserId = Math.max(0, ...users.map(u => u.id)) + 1;
        const newEmployeeId = Math.max(0, ...employees.map(e => e.id)) + 1;
        setUsers([...users, { id: newUserId, email: employee.email, role: "Employee", isActive: true }]);
        setEmployees([...employees, { ...employee, id: newEmployeeId }]);
    }
    
    function changeEmail(id: number, email: string) {
        const oldEmail = users.find(u => u.id === id)?.email;
        setUsers(users.map(u => (u.id === id ? { ...u, email: email } : u)));
        setEmployees(employees.map(e => (e.email === oldEmail ? { ...e, email: email } : e)));
    }
    
    function setUserActive(id: number, isActive: boolean) {
        setUsers(users.map(u => (u.id === id ? { ...u, isActive: isActive } : u)));
    }

    return (
        <BrowserRouter>
            <AuthProvider>
                <Routes>
                    <Route path="/" element={<HomeRedirect />} />
                    <Route path="/login" element={<LoginPage />} />

                    <Route path="/user" element={<Layout links={userLinks} />}>
                        <Route index element={<UserMain requests={myRequests} onCancel={cancelRequest} events={events} />} />
                        <Route path="request" element={<RequestPage requests={myRequests} onAdd={addRequest} onCancel={removeRequest} events={events} />} />
                        <Route path="requests" element={<UserRequests requests={myRequests} onCancel={cancelRequest} />} />
                    </Route>

                    <Route path="/admin" element={<RequireRole role="Admin"><Layout links={adminLinks} /></RequireRole>}>
                        <Route index element={<AdminMain rules={rules} onAddVersion={addRuleVersion} />} />
                        <Route path="calendar" element={<AdminCalendar events={events} onAdd={addEvent} onDelete={deleteEvent} />} />
                        <Route path="users" element={<Users users={users} employees={employees} onCreateEmployee={createEmployee} onChangeEmail={changeEmail} onSetActive={setUserActive} />} />
                    </Route>

                    <Route path="/accounting" element={<RequireRole role="Accounting"><Layout links={accountingLinks} /></RequireRole>}>
                        <Route index element={<AccountingMain requests={requests} employees={employees} onDecide={decideRequest} events={events} />} />
                        <Route path="employees" element={<EmployeeList employees={employees} requests={requests} />} />
                        <Route path="schedule" element={<WorkSchedulePage requests={requests} employees={employees} events={events} />} />
                        <Route path="employees/:id" element={<EmployeeInfo employees={employees} requests={requests} onAddSchedule={addSchedulePeriod} events={events} />} />
                    </Route>
                </Routes>
            </AuthProvider>
        </BrowserRouter>
    );
}

export default App;