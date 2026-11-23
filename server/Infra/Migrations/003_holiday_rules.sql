
CREATE TABLE holiday_rules (
                               id uuid PRIMARY KEY,
                               name text NOT NULL,
                               effective_from date NOT NULL,
                               version integer NOT NULL,
                               holiday_days_per_month numeric NOT NULL,
                               holiday_days_per_year numeric,
                               sixth_week_hours_per_calendar_day numeric,
                               sixth_week_enabled boolean NOT NULL,
                               sixth_week_hours_per_month numeric NOT NULL,
                               full_time_hours_per_week numeric NOT NULL,
                               maximum_debt_days numeric NOT NULL,
                               allow_transfer boolean NOT NULL,
                               maximum_transfer_days numeric NOT NULL,
                               created_at timestamptz NOT NULL,
                               created_by uuid NOT NULL
);

CREATE UNIQUE INDEX ux_holiday_rules_effective_from_version ON holiday_rules (effective_from, version);

CREATE TABLE holiday_rule_periods (
                                      holiday_rule_id uuid PRIMARY KEY,
                                      holiday_earning_start_month integer NOT NULL,
                                      holiday_earning_start_day integer NOT NULL,
                                      holiday_usage_start_month integer NOT NULL,
                                      holiday_usage_start_day integer NOT NULL,
                                      holiday_usage_start_year_offset integer NOT NULL,
                                      holiday_usage_end_month integer NOT NULL,
                                      holiday_usage_end_day integer NOT NULL,
                                      holiday_usage_end_year_offset integer NOT NULL,
                                      sixth_week_earning_start_month integer NOT NULL,
                                      sixth_week_earning_start_day integer NOT NULL,
                                      sixth_week_usage_start_month integer NOT NULL,
                                      sixth_week_usage_start_day integer NOT NULL,
                                      sixth_week_usage_start_year_offset integer NOT NULL,
                                      sixth_week_usage_end_month integer NOT NULL,
                                      sixth_week_usage_end_day integer NOT NULL,
                                      sixth_week_usage_end_year_offset integer NOT NULL,
                                      main_holiday_days integer NOT NULL,
                                      main_holiday_start_month integer NOT NULL,
                                      main_holiday_start_day integer NOT NULL,
                                      main_holiday_end_month integer NOT NULL,
                                      main_holiday_end_day integer NOT NULL,
                                      main_holiday_notice_months integer NOT NULL,
                                      other_holiday_notice_months integer NOT NULL,
                                      debt_deadline_month integer NOT NULL,
                                      debt_deadline_day integer NOT NULL,
                                      holiday_transfer_deadline_month integer NOT NULL,
                                      holiday_transfer_deadline_day integer NOT NULL,
                                      sixth_week_transfer_deadline_month integer NOT NULL,
                                      sixth_week_transfer_deadline_day integer NOT NULL
);