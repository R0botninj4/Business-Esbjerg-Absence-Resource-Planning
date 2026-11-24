-- Local demo accounts only. Each hash is for the password 1234567891011.
INSERT INTO users (id, email, password_hash, role, is_active) VALUES
    (gen_random_uuid(), 'admin@businessesbjerg.com', 'AQAAAAIAAYagAAAAEBda5qCWmRgO1IJkqLHIcJZFn5gAR9OSvL6SdrZBLz9jscUSoPqL6svs0WtQ5bEdQA==', 'Admin', true),
    (gen_random_uuid(), 'accounting@businessesbjerg.com', 'AQAAAAIAAYagAAAAEBGubNRzWnVtswwX+wJznGD2TAAWi27xtq61hWK66cF7jLp11Fu9eE3kFDk2dOt11Q==', 'Accounting', true),
    (gen_random_uuid(), 'employee@businessesbjerg.com', 'AQAAAAIAAYagAAAAEOrTc2IDb09S10no8jBLaMMqStz/SbRDMyZlLOnySyFohka1piV04N4kJHMyQYDeEg==', 'Employee', true);

-- The demo employee starts today and has a 37-hour working week.
INSERT INTO employments (id, user_id, type, start_date)
SELECT gen_random_uuid(), id, 1, CURRENT_DATE FROM users WHERE role = 'Employee';

INSERT INTO work_schedules (id, employment_id, start_date)
SELECT gen_random_uuid(), id, start_date FROM employments;

INSERT INTO work_schedule_days (id, work_schedule_id, day, is_day_off, start_time, end_time)
SELECT gen_random_uuid(), schedule.id, weekday.day, false, TIME '08:00', weekday.end_time
FROM work_schedules schedule
CROSS JOIN (VALUES
    (1, TIME '15:30'), (2, TIME '15:30'), (3, TIME '15:30'),
    (4, TIME '15:30'), (5, TIME '15:00')
) AS weekday(day, end_time);
