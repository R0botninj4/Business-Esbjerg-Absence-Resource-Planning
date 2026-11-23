
CREATE TABLE calendar_days (
                               id uuid PRIMARY KEY,
                               date date NOT NULL,
                               name text NOT NULL,
                               uses_holiday_hours boolean NOT NULL,
                               version integer NOT NULL,
                               created_at timestamptz NOT NULL,
                               created_by uuid NOT NULL
);

CREATE UNIQUE INDEX ux_calendar_days_date_version ON calendar_days (date, version);