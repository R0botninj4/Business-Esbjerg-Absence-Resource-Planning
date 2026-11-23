CREATE TABLE holiday_balance_adjustments (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users (id),
    operation_id uuid NOT NULL,
    accrual_day_id uuid NOT NULL REFERENCES holiday_accrual_days (id),
    date date NOT NULL,
    hours numeric NOT NULL,
    reason text NOT NULL,
    created_at timestamptz NOT NULL,
    created_by uuid NOT NULL REFERENCES users (id)
);

CREATE UNIQUE INDEX ux_holiday_balance_adjustments_operation
    ON holiday_balance_adjustments (operation_id);
