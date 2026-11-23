import { WorkSchedule } from "../../components/workSchedule";
import type { Employee } from "../../types/employee";
import { testEvents } from "../../mockdata/testEvents";
import type { HolidayRequest } from "../../types/holidayRequest";

type Props = {
    requests: HolidayRequest[];
    employees: Employee[];
};

export function WorkSchedulePage({ requests, employees }: Props) {
    return <WorkSchedule employees={employees} requests={requests} events={testEvents} />;
}