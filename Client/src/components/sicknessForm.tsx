import { useEffect, useState } from "react";
import { useAuth } from "./auth";
import { loadEmployee } from "../api/schedules";
import type { CalendarEvent } from "../types/calendarEvent";
import type { Employee } from "../types/employee";
import type { HolidayRequest } from "../types/holidayRequest";
import type { SickDay, SicknessReport } from "../types/sicknessReport";
import { formatDate, holidayTypeName } from "../utils/format";
import { sickDaysBetween, sicknessRule, totalHours } from "../utils/sickness";

type Props = {
    start: Date | null;
    end: Date | null;
    holidays: HolidayRequest[]; // the employee's pending and approved holidays
    reports: SicknessReport[]; // the employee's earlier sickness reports
    events: CalendarEvent[];
    onReport: (report: SicknessReport) => void;
};

// The form on "Request holiday" when the employee chooses "Sickness"
export function SicknessForm({ start, end, holidays, reports, events, onReport }: Props) {
    const { user } = useAuth();
    const [employee, setEmployee] = useState<Employee | null>(null);
    const [beforeHoliday, setBeforeHoliday] = useState(false);
    const [note, setNote] = useState<File | null>(null);
    const [fileKey, setFileKey] = useState(0); // changing the key empties the file field
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    // Load the work schedule, so we know how many hours each sick day is worth
    useEffect(() => {
        if (!user) return;
        loadEmployee({ id: user.id, email: user.email, role: "Employee", isActive: true })
            .then(setEmployee)
            .catch(() => setError("Could not load your work schedule."));
    }, [user?.id]);

    // Remove old messages when the employee picks new days
    useEffect(() => {
        setError("");
        if (start) setSuccess("");
    }, [start, end]);

    // The booked holiday that all the selected days are inside
    let holiday: HolidayRequest | undefined = undefined;
    if (start && end) {
        holiday = holidays.find(h => start >= h.start && end <= h.end);
    }

    let sickDays: SickDay[] = [];
    if (holiday && employee && start && end) {
        sickDays = sickDaysBetween(employee, start, end, events);
    }

    // True if some of the days are already in a report that isn't declined
    function alreadyReported() {
        return reports.some(r =>
            r.status !== "declined" && start && end && start <= r.end && end >= r.start
        );
    }

    function handleSend() {
        setError("");
        setSuccess("");

        if (!user || !start || !end) {
            setError("Choose the days you were sick in the calendar.");
            return;
        }
        if (!holiday) {
            setError("Sickness can only be reported on days you have booked holiday. Pick days from one holiday at a time.");
            return;
        }
        if (sickDays.length === 0) {
            setError("There are no working days in the days you picked.");
            return;
        }
        if (alreadyReported()) {
            setError("Some of these days are already reported as sickness.");
            return;
        }

        // TODO: send to the backend (#10) when the endpoint exists. Then the file is uploaded too.
        onReport({
            id: Date.now(), // temporary id – the database will make the real one
            requestId: holiday.id,
            employeeId: user.id,
            employeeName: user.email,
            holidayType: holiday.holidayType,
            start: start,
            end: end,
            sickDays: sickDays,
            beforeHoliday: beforeHoliday,
            doctorsNote: note ? { name: note.name, url: URL.createObjectURL(note) } : undefined,
            createdAt: new Date(),
            status: "pending",
        });

        setBeforeHoliday(false);
        setNote(null);
        setFileKey(fileKey + 1);
        setSuccess("Your sickness report has been sent to accounting.");
    }

    return (
        <div className="bg-white border border-line rounded p-6 border-b-4 border-b-orange">
            <h2 className="text-2xl mb-4">Report sickness</h2>

            <p><strong>From:</strong> {formatDate(start)}</p>
            <p><strong>To:</strong> {formatDate(end)}</p>
            <p>
                <strong>Holiday:</strong>{" "}
                {holiday ? holidayTypeName(holiday.holidayType) + ", " + formatDate(holiday.start) + " – " + formatDate(holiday.end) : "–"}
            </p>
            <p><strong>Sick days:</strong> {sickDays.length} working days · {totalHours(sickDays).toFixed(2)} h</p>

            <p className="font-semibold mt-4">When did you get sick?</p>
            <label className="flex items-center gap-2">
                <input type="radio" checked={beforeHoliday} onChange={() => setBeforeHoliday(true)} />
                Before the holiday started
            </label>
            <label className="flex items-center gap-2">
                <input type="radio" checked={!beforeHoliday} onChange={() => setBeforeHoliday(false)} />
                During the holiday
            </label>

            <label className="block font-semibold mt-4">
                Doctor's note <span className="font-normal text-slate">(optional)</span>
                <input
                    key={fileKey}
                    type="file"
                    accept=".pdf,image/*"
                    onChange={e => setNote(e.target.files?.[0] ?? null)}
                    className="block mt-1 font-normal"
                />
            </label>

            {holiday && (
                <p className="text-sm text-slate mt-4">{sicknessRule(holiday.holidayType, beforeHoliday)}</p>
            )}

            {error && <p role="alert" className="text-red-600 mt-3">{error}</p>}
            {success && <p role="status" className="mt-3">{success}</p>}

            <button
                onClick={handleSend}
                disabled={!start || !end}
                className="mt-6 w-full bg-orange-light text-white font-semibold px-6 py-3 rounded hover:bg-orange disabled:opacity-40 disabled:cursor-not-allowed"
            >
                Send to accounting
            </button>
        </div>
    );
}