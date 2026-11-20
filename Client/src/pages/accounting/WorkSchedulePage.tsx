import { WorkSchedule } from "../../components/workSchedule";
import { testEmployees } from "../../mockdata/testEmployees";
import { testEvents } from "../../mockdata/testEvents";
import type { HolidayRequest } from "../../types/holidayRequest";

type Props = {
    requests: HolidayRequest[];
};

export function WorkSchedulePage({ requests }: Props) {
    return <WorkSchedule employees={testEmployees} requests={requests} events={testEvents} />;
}