CREATE TABLE holiday_requests (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id),
    account integer NOT NULL CHECK (account IN (1, 2)),
    start_date date NOT NULL,
    end_date date NOT NULL CHECK (end_date >= start_date),
    hours numeric(12, 2) NOT NULL CHECK (hours > 0),
    status text NOT NULL CHECK (status IN ('pending', 'approved', 'declined', 'cancelled')),
    created_at timestamptz NOT NULL,
    withdrawn_at timestamptz
);

CREATE INDEX ix_holiday_requests_user_dates ON holiday_requests(user_id, start_date, end_date);

CREATE TABLE holiday_request_days (
    id uuid PRIMARY KEY,
    request_id uuid NOT NULL REFERENCES holiday_requests(id),
    date date NOT NULL,
    hours numeric(12, 2) NOT NULL CHECK (hours >= 0),
    employment_id uuid NOT NULL REFERENCES employments(id),
    holiday_rule_id uuid NOT NULL REFERENCES holiday_rules(id),
    work_schedule_id uuid REFERENCES work_schedules(id),
    calendar_day_id uuid REFERENCES calendar_days(id),
    description text NOT NULL,
    basis text NOT NULL,
    UNIQUE (request_id, basis, date)
);
