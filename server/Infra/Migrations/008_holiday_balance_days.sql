-- Gem dagværdien sammen med forbruget, så senere planskift ikke omskriver historien.
ALTER TABLE holiday_usage ALTER COLUMN accrual_day_id DROP NOT NULL;
ALTER TABLE holiday_usage ADD COLUMN account integer;
ALTER TABLE holiday_usage ADD COLUMN earning_start date;
ALTER TABLE holiday_usage ADD COLUMN earning_end date;
ALTER TABLE holiday_usage ADD COLUMN usage_start date;
ALTER TABLE holiday_usage ADD COLUMN usage_end date;
ALTER TABLE holiday_usage ADD COLUMN day_length numeric(24,12);
ALTER TABLE holiday_usage ADD COLUMN days numeric(24,12);

UPDATE holiday_usage u SET account=a.account, earning_start=a.earning_start,
    earning_end=a.earning_end, usage_start=a.usage_start, usage_end=a.usage_end,
    day_length=(SELECT sum(extract(epoch FROM (d.end_time-d.start_time)))/18000
        FROM work_schedule_days d WHERE d.work_schedule_id=r.work_schedule_id AND NOT d.is_day_off)
FROM holiday_accrual_days a, holiday_request_days r
WHERE u.accrual_day_id=a.id AND u.request_day_id=r.id;
UPDATE holiday_usage SET days=hours/day_length;
ALTER TABLE holiday_usage ALTER COLUMN account SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN earning_start SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN earning_end SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN usage_start SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN usage_end SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN day_length SET NOT NULL;
ALTER TABLE holiday_usage ALTER COLUMN days SET NOT NULL;
ALTER TABLE holiday_usage ADD CHECK (day_length>0 AND days>0 AND account IN (1,2));

ALTER TABLE holiday_balance_adjustments ADD COLUMN day_length numeric(24,12);
UPDATE holiday_balance_adjustments a SET day_length=(
    SELECT sum(extract(epoch FROM (d.end_time-d.start_time)))/18000
    FROM work_schedule_days d WHERE NOT d.is_day_off AND d.work_schedule_id=coalesce(
        (SELECT s.id FROM work_schedules s JOIN employments e ON e.id=s.employment_id
         WHERE e.user_id=a.user_id AND s.start_date<=a.date AND (s.end_date IS NULL OR s.end_date>=a.date)
         ORDER BY s.start_date DESC LIMIT 1),
        (SELECT work_schedule_id FROM holiday_accrual_days WHERE id=a.accrual_day_id)));
ALTER TABLE holiday_balance_adjustments ALTER COLUMN day_length SET NOT NULL;
ALTER TABLE holiday_balance_adjustments ADD CHECK (day_length>0);

CREATE TABLE holiday_balance_conversions (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id),
    work_schedule_id uuid NOT NULL REFERENCES work_schedules(id),
    previous_schedule_id uuid NOT NULL REFERENCES work_schedules(id),
    account integer NOT NULL CHECK (account IN (1,2)),
    earning_start date NOT NULL,
    earning_end date NOT NULL,
    usage_start date NOT NULL,
    usage_end date NOT NULL,
    date date NOT NULL,
    remaining_days numeric(24,12) NOT NULL,
    before_hours numeric(12,2) NOT NULL,
    after_hours numeric(12,2) NOT NULL,
    hours numeric(12,2) NOT NULL,
    basis_key text NOT NULL,
    created_at timestamptz NOT NULL,
    UNIQUE(user_id,work_schedule_id,account,earning_start,earning_end,usage_start,usage_end,basis_key)
);
