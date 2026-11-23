import { useEffect, useState } from "react";
import { useAuth } from "../../components/auth";
import { loadEmployee } from "../../api/schedules";
import { userError } from "../../api/users";
import type { Employee } from "../../types/employee";
import { formatDate } from "../../utils/format";
import { weekdayNames, scheduleOn } from "../../utils/schedule";

export function MySchedule() {
    const { user } = useAuth();
    const [employee, setEmployee] = useState<Employee | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    useEffect(() => {
        let cancelled = false;
        if (!user?.id || !user.email) return;
        setLoading(true);
        loadEmployee({ id: user.id, email: user.email, role: "Employee", isActive: true })
            .then(data => { if (!cancelled) setEmployee(data); })
            .catch(error => { if (!cancelled) setError(userError(error)); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [user?.id]);
    if (loading) return <p>Loading your schedule...</p>;
    if (error) return <p role="alert">{error}</p>;
    if (!employee || employee.schedules.length === 0) return <p>No work schedule has been saved for your account.</p>;
    return <div>
        <h1 className="text-3xl mb-6">My work schedule</h1>
        <div className="grid grid-cols-5 gap-3">{scheduleOn(employee, new Date()).map((day, index) => <div key={index} className="bg-white border border-line p-4">
            <strong>{weekdayNames[index]}</strong><p>{day ? day.from + "–" + day.to : "Not working"}</p>
        </div>)}</div>
        <h2 className="text-2xl mt-6">History and upcoming schedules</h2>
        {[...employee.schedules].reverse().map(period => <div key={period.id} className="bg-white border border-line p-4 mt-3">
            <strong>{formatDate(period.validFrom)} – {period.validTo ? formatDate(period.validTo) : "onwards"}</strong>
            {period.days.map((day, index) => <p key={index}>{weekdayNames[index]}: {day ? day.from + "–" + day.to : "off"}</p>)}
        </div>)}
    </div>;
}
