ALTER TABLE holiday_rules ADD COLUMN sickness_waiting_days integer NOT NULL DEFAULT 5 CHECK (sickness_waiting_days >= 0);

CREATE TABLE holiday_obstacles (
    id uuid PRIMARY KEY,
    operation_id uuid NOT NULL UNIQUE,
    user_id uuid NOT NULL REFERENCES users(id),
    request_id uuid REFERENCES holiday_requests(id),
    kind text NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL CHECK (end_date >= start_date),
    documentation_reference text NOT NULL,
    status text NOT NULL DEFAULT 'pending',
    created_at timestamptz NOT NULL,
    decided_at timestamptz,
    decided_by uuid REFERENCES users(id),
    comment text NOT NULL DEFAULT ''
);

CREATE TABLE holiday_obstacle_days (
    id uuid PRIMARY KEY,
    obstacle_id uuid NOT NULL REFERENCES holiday_obstacles(id),
    usage_id uuid NOT NULL REFERENCES holiday_usage(id),
    date date NOT NULL,
    hours numeric NOT NULL,
    holiday_year_start date NOT NULL,
    waiting_days integer NOT NULL,
    is_waiting_day boolean NOT NULL DEFAULT false,
    refunded_hours numeric NOT NULL DEFAULT 0,
    UNIQUE (obstacle_id, usage_id)
);
