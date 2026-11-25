import { WorkSchedule } from "../../components/workSchedule";
import { useEmployees } from "../../components/employeeData";
import type { CalendarEvent } from "../../types/calendarEvent";
import type { HolidayRequest } from "../../types/holidayRequest";

export function WorkSchedulePage({ events, requests }: { events: CalendarEvent[]; requests: HolidayRequest[] }) {
    const { employees } = useEmployees();
    return <div>
        <p className="mb-4 text-slate">Saved work schedules with pending and approved holiday.</p>
        {employees.length === 0 ? <p>No employees to display.</p> : <WorkSchedule employees={employees} requests={requests} events={events} />}
    </div>;
}
