
CREATE TABLE users (
                       id uuid PRIMARY KEY,
                       email text NOT NULL,
                       password_hash text NOT NULL,
                       role text NOT NULL DEFAULT 'Employee',
                       is_active boolean NOT NULL DEFAULT true
);

CREATE UNIQUE INDEX ux_users_email ON users (email);

CREATE TABLE employments (
                             id uuid PRIMARY KEY,
                             user_id uuid NOT NULL,
                             type integer NOT NULL,
                             start_date date NOT NULL,
                             end_date date
);

CREATE INDEX ix_employments_user_id ON employments (user_id);

CREATE TABLE work_schedules (
                                id uuid PRIMARY KEY,
                                employment_id uuid NOT NULL,
                                start_date date NOT NULL,
                                end_date date
);

CREATE INDEX ix_work_schedules_employment_id ON work_schedules (employment_id);

CREATE TABLE work_schedule_days (
                                    id uuid PRIMARY KEY,
                                    work_schedule_id uuid NOT NULL,
                                    day integer NOT NULL,
                                    is_day_off boolean NOT NULL,
                                    start_time time,
                                    end_time time
);

CREATE INDEX ix_work_schedule_days_work_schedule_id ON work_schedule_days (work_schedule_id);