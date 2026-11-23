import { WorkSchedule } from "../../components/workSchedule";
import type { Employee } from "../../types/employee";
import type { HolidayRequest } from "../../types/holidayRequest";
import type { CalendarEvent } from "../../types/calendarEvent";

type Props = {
    requests: HolidayRequest[];
    employees: Employee[];
    events: CalendarEvent[];
};

export function WorkSchedulePage({ requests, employees, events }: Props) {
    return <WorkSchedule employees={employees} requests={requests} events={events} />;
}