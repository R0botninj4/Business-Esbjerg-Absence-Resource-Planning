import { BrowserRouter, Routes, Route, Navigate } from "react-router";
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

export function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Navigate to="/user" replace />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/user" element={<UserMain />} />
                <Route path="/user/request" element={<RequestPage />} />
                <Route path="/user/requests" element={<UserRequests />} />
                <Route path="/admin" element={<AdminMain />} />
                <Route path="/admin/calendar" element={<AdminCalendar />} />
                <Route path="/admin/users" element={<Users />} />
                <Route path="/accounting" element={<AccountingMain />} />
                <Route path="/accounting/employees" element={<EmployeeList />} />
                <Route path="/accounting/employeesInfo" element={<EmployeeInfo />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;