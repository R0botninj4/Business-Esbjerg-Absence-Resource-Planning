ALTER TABLE holiday_requests
    ADD COLUMN cancellation_status text NOT NULL DEFAULT '',
    ADD COLUMN cancellation_requested_at timestamptz,
    ADD COLUMN cancellation_decided_at timestamptz,
    ADD COLUMN cancellation_decided_by uuid REFERENCES users(id),
    ADD COLUMN cancellation_comment text NOT NULL DEFAULT '';

-- Forbruget beholdes til historikken. Annullering frigiver kun reservationen.
ALTER TABLE holiday_usage ADD COLUMN cancelled_at timestamptz;
