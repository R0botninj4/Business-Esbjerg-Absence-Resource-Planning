ALTER TABLE holiday_balance_adjustments ALTER COLUMN accrual_day_id DROP NOT NULL;
ALTER TABLE holiday_balance_adjustments
    ADD COLUMN account integer,
    ADD COLUMN earning_start date,
    ADD COLUMN earning_end date,
    ADD COLUMN usage_start date,
    ADD COLUMN usage_end date;

UPDATE holiday_balance_adjustments AS adjustment
SET account = day.account, earning_start = day.earning_start, earning_end = day.earning_end,
    usage_start = day.usage_start, usage_end = day.usage_end
FROM holiday_accrual_days AS day WHERE day.id = adjustment.accrual_day_id;

ALTER TABLE holiday_balance_adjustments
    ALTER COLUMN account SET NOT NULL,
    ALTER COLUMN earning_start SET NOT NULL,
    ALTER COLUMN earning_end SET NOT NULL,
    ALTER COLUMN usage_start SET NOT NULL,
    ALTER COLUMN usage_end SET NOT NULL;

-- En startsaldo pr. medarbejder, konto og optjeningsperiode.
CREATE UNIQUE INDEX ux_holiday_opening_balance
    ON holiday_balance_adjustments (user_id, account, earning_start, earning_end)
    WHERE accrual_day_id IS NULL;
