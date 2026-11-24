import { Link } from "react-router";
import { useEmployees } from "../../components/employeeData";
import { employmentToday } from "../../utils/schedule";
import { displayName } from "../../utils/format";

export function EmployeeList() {
    const { employees } = useEmployees();
    return <div>
        <h1 className="text-3xl mb-6">Employees</h1>
        <div className="bg-white border border-line rounded">
            {employees.length === 0 && <p className="p-6">No employee accounts have been created.</p>}
            {employees.map(employee => <Link key={employee.id} to={String(employee.id)} className="flex justify-between p-6 border-b border-line hover:bg-cream">
                <span className="font-semibold">{displayName(employee.email)}</span><span>{employmentToday(employee)} →</span>
            </Link>)}
        </div>
    </div>;
}
