
CREATE TABLE holiday_accrual_months (
                                        id uuid PRIMARY KEY,
                                        user_id uuid NOT NULL,
                                        month date NOT NULL,
                                        posted_at timestamptz NOT NULL,
                                        posted_by uuid NOT NULL
);

CREATE UNIQUE INDEX ux_holiday_accrual_months_user_month ON holiday_accrual_months (user_id, month);

CREATE TABLE holiday_accrual_days (
                                      id uuid PRIMARY KEY,
                                      accrual_month_id uuid NOT NULL,
                                      employment_id uuid NOT NULL,
                                      date date NOT NULL,
                                      account integer NOT NULL,
                                      hours numeric NOT NULL,
                                      raw_hours numeric NOT NULL,
                                      raw_days numeric NOT NULL,
                                      holiday_rule_id uuid NOT NULL,
                                      work_schedule_id uuid NOT NULL,
                                      earning_start date NOT NULL,
                                      earning_end date NOT NULL,
                                      usage_start date NOT NULL,
                                      usage_end date NOT NULL
);

CREATE UNIQUE INDEX ux_holiday_accrual_days_month_date_account ON holiday_accrual_days (accrual_month_id, date, account);