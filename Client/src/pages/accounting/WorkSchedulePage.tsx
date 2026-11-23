import { WorkSchedule } from "../../components/workSchedule";
import { useEmployees } from "../../components/employeeData";
import type { CalendarEvent } from "../../types/calendarEvent";

export function WorkSchedulePage({ events }: { events: CalendarEvent[] }) {
    const { employees } = useEmployees();
    return <div>
        <p className="mb-4 text-slate">Working hours from saved schedules. Holiday requests are not connected yet.</p>
        {employees.length === 0 ? <p>No employees to display.</p> : <WorkSchedule employees={employees} requests={[]} events={events} />}
    </div>;
}
