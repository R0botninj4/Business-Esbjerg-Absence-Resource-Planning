ALTER TABLE holiday_rules ADD COLUMN non_transferable_holiday_days numeric NOT NULL DEFAULT 20
    CHECK (non_transferable_holiday_days >= 0);

CREATE TABLE holiday_transfers (
    id uuid PRIMARY KEY,
    operation_id uuid NOT NULL UNIQUE,
    user_id uuid NOT NULL REFERENCES users(id),
    accrual_day_id uuid NOT NULL REFERENCES holiday_accrual_days(id),
    rule_id uuid NOT NULL REFERENCES holiday_rules(id),
    account integer NOT NULL CHECK (account IN (1,2)),
    earning_start date NOT NULL,
    earning_end date NOT NULL,
    source_usage_start date NOT NULL,
    source_usage_end date NOT NULL,
    target_usage_start date NOT NULL,
    target_usage_end date NOT NULL,
    deadline date NOT NULL,
    hours numeric NOT NULL CHECK (hours > 0),
    days numeric(24,12) NOT NULL CHECK (days > 0),
    day_length numeric(24,12) NOT NULL CHECK (day_length > 0),
    debit_date date,
    effective_date date,
    status text NOT NULL DEFAULT 'pending',
    agreement_reference text NOT NULL DEFAULT '',
    comment text NOT NULL DEFAULT '',
    created_at timestamptz NOT NULL,
    decided_at timestamptz,
    decided_by uuid REFERENCES users(id),
    CHECK (target_usage_start > source_usage_end AND target_usage_end >= target_usage_start)
);
