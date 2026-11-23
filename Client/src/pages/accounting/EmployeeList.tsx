import { Link } from "react-router";
import type { Employee } from "../../types/employee";
import type { HolidayRequest } from "../../types/holidayRequest";
import { formatDate } from "../../utils/format";
import { employmentText, scheduleOn } from "../../utils/schedule";

type Props = {
    employees: Employee[];
    requests: HolidayRequest[];
};

export function EmployeeList({ employees, requests }: Props) {
    const today = new Date();

    // How many requests are waiting for an answer from this employee
    function pendingCount(employee: Employee) {
        return requests.filter(r => r.employeeName === employee.name && r.status === "pending").length;
    }

    // The next approved holiday that hasn't ended yet
    function nextHoliday(employee: Employee) {
        const upcoming = requests
            .filter(r => r.employeeName === employee.name && r.status === "approved" && r.end >= today)
            .sort((a, b) => a.start.getTime() - b.start.getTime());
        return upcoming[0];
    }

    return (
        <div>
            <h1 className="text-3xl mb-6">Employees</h1>

            <div className="bg-white border border-line rounded">
                {/* Headings */}
                <div className="grid grid-cols-5 gap-4 px-6 py-3 text-sm font-semibold text-slate border-b border-line">
                    <span className="col-span-2">Employee</span>
                    <span>Employment</span>
                    <span>Next holiday</span>
                    <span>Pending requests</span>
                </div>

                {/* One row per employee */}
                {employees.map(employee => {
                    const pending = pendingCount(employee);
                    const holiday = nextHoliday(employee);

                    return (
                        <Link
                            key={employee.id}
                            to={"/accounting/employees/" + employee.id}
                            className="grid grid-cols-5 gap-4 px-6 py-4 items-center border-b border-line last:border-0 hover:bg-cream"
                        >
                            <span className="col-span-2">
                                <span className="font-semibold text-lg">{employee.name}</span>
                                <span className="block text-sm text-slate">{employee.email}</span>
                            </span>

                            <span>{employmentText(scheduleOn(employee, today))}</span>

                            <span>{holiday ? formatDate(holiday.start) + " – " + formatDate(holiday.end) : "–"}</span>

                            <span className="flex justify-between items-center">
                                {pending > 0 ? (
                                    <span className="text-sm font-semibold text-white bg-gold px-3 py-1 rounded-full">
                                        {pending} pending
                                    </span>
                                ) : (
                                    <span className="text-slate">None</span>
                                )}
                                <span className="text-2xl">→</span>
                            </span>
                        </Link>
                    );
                })}
            </div>
        </div>
    );
}