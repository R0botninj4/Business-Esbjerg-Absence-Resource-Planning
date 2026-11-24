ALTER TABLE holiday_requests ADD COLUMN decided_at timestamptz;
ALTER TABLE holiday_requests ADD COLUMN decided_by uuid REFERENCES users(id);
ALTER TABLE holiday_requests ADD COLUMN decision_comment text NOT NULL DEFAULT '';

CREATE TABLE holiday_usage (
    id uuid PRIMARY KEY,
    request_day_id uuid NOT NULL REFERENCES holiday_request_days(id),
    user_id uuid NOT NULL REFERENCES users(id),
    accrual_day_id uuid NOT NULL REFERENCES holiday_accrual_days(id),
    date date NOT NULL,
    hours numeric(12, 2) NOT NULL CHECK (hours > 0),
    UNIQUE (request_day_id, accrual_day_id)
);
CREATE INDEX ix_holiday_usage_user ON holiday_usage(user_id);
