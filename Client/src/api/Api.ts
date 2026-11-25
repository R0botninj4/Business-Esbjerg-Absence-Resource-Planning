/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface CalendarDay {
  /** @format uuid */
  id?: string;
  /** @format date */
  date?: string;
  name?: string;
  usesHolidayHours?: boolean;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  version?: number | string;
  /** @format date-time */
  createdAt?: string;
  /** @format uuid */
  createdBy?: string;
}

export interface ChangeWorkScheduleRequest {
  /** @format date */
  startDate?: string;
  days?: WorkScheduleDayRequest[];
}

export interface CreateCalendarDayRequest {
  /** @format date */
  date?: string;
  name: string;
  usesHolidayHours: null | boolean;
}

export interface CreateEmployeeRequest {
  /**
   * @minLength 0
   * @maxLength 254
   */
  email: string;
  /**
   * @minLength 12
   * @maxLength 128
   */
  password: string;
  type?: EmploymentType;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: null | string;
  days?: null | WorkScheduleDayRequest[];
}

export interface CreateEmploymentRequest {
  type?: EmploymentType;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: null | string;
  days?: null | WorkScheduleDayRequest[];
}

export interface CreateHolidayRuleRequest {
  /** @format date */
  effectiveFrom?: string;
  settings: null | HolidayRuleSettings;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  role: string;
}

export interface CreateWorkScheduleRequest {
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: null | string;
  days?: WorkScheduleDayRequest[];
}

export type DayOfWeek = number;

export interface Employment {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  userId?: string;
  type?: EmploymentType;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: null | string;
}

export type EmploymentType = number;

export interface EndEmploymentRequest {
  /** @format date */
  endDate?: string;
}

export interface EndWorkScheduleRequest {
  /** @format date */
  endDate?: string;
}

export type HolidayAccount = number;

export interface HolidayAccountPeriod {
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
  /** @format date */
  transferDeadline?: string;
}

export interface HolidayAccrualDay {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  accrualMonthId?: string;
  /** @format uuid */
  employmentId?: string;
  /** @format date */
  date?: string;
  account?: HolidayAccount;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  rawHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  rawDays?: number | string;
  /** @format uuid */
  holidayRuleId?: string;
  /** @format uuid */
  workScheduleId?: string;
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
}

export interface HolidayAccrualResult {
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  weeksOneToFiveHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHours?: number | string;
  isPosted?: boolean;
  days?: HolidayAccrualDay[];
}

export interface HolidayAdjustmentRequest {
  /** @format uuid */
  operationId?: string;
  /** @format uuid */
  accrualDayId?: string;
  /** @format date */
  date?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  reason?: string;
}

export interface HolidayBalanceAdjustment {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  userId?: string;
  /** @format uuid */
  operationId?: string;
  /** @format uuid */
  accrualDayId?: null | string;
  account?: HolidayAccount;
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
  /** @format date */
  date?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  dayLength?: number | string;
  reason?: string;
  /** @format date-time */
  createdAt?: string;
  /** @format uuid */
  createdBy?: string;
}

export interface HolidayBalanceConversion {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  userId?: string;
  /** @format uuid */
  workScheduleId?: string;
  /** @format uuid */
  previousScheduleId?: string;
  account?: HolidayAccount;
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
  /** @format date */
  date?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  remainingDays?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  beforeHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  afterHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  basisKey?: string;
  /** @format date-time */
  createdAt?: string;
}

export interface HolidayBalancePeriod {
  account?: HolidayAccount;
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  postedHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  expectedHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  adjustmentHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  usedHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  reservedHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  conversionHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  debtHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  dayLength?: number | string;
}

export interface HolidayBalanceResult {
  /** @format date */
  date?: string;
  includesExpectedAccrual?: boolean;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  weeksOneToFiveHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  weeksOneToFiveDebtHours?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekDebtHours?: number | string;
  includesHolidayUsage?: boolean;
  includesScheduleConversions?: boolean;
  periods?: HolidayBalancePeriod[];
}

export interface HolidayDecisionInput {
  approve?: boolean;
  comment?: string;
}

export interface HolidayHoursDay {
  /** @format date */
  date?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  description?: string;
  /** @format uuid */
  workScheduleId?: null | string;
  /** @format uuid */
  calendarDayId?: null | string;
}

export interface HolidayHoursResult {
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  days?: HolidayHoursDay[];
}

export interface HolidayOpeningBalanceRequest {
  /** @format uuid */
  operationId?: string;
  account?: HolidayAccount;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  /** @format date */
  earningStart?: string;
  /** @format date */
  earningEnd?: string;
  /** @format date */
  usageStart?: string;
  /** @format date */
  usageEnd?: string;
  reason?: string;
}

export interface HolidayRequestDay {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  requestId?: string;
  /** @format date */
  date?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  /** @format uuid */
  employmentId?: string;
  /** @format uuid */
  holidayRuleId?: string;
  /** @format uuid */
  workScheduleId?: null | string;
  /** @format uuid */
  calendarDayId?: null | string;
  description?: string;
  basis?: string;
}

export interface HolidayRequestInput {
  account?: HolidayAccount;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
}

export interface HolidayRequestPreview {
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  days?: HolidayRequestDay[];
}

export interface HolidayRequestResponse {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  userId?: string;
  email?: string;
  account?: HolidayAccount;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  hours?: number | string;
  status?: string;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  withdrawnAt?: null | string;
  /** @format date-time */
  decidedAt?: null | string;
  /** @format uuid */
  decidedBy?: null | string;
  comment?: string;
  cancellationStatus?: string;
  /** @format date-time */
  cancellationRequestedAt?: null | string;
  /** @format date-time */
  cancellationDecidedAt?: null | string;
  /** @format uuid */
  cancellationDecidedBy?: null | string;
  cancellationComment?: string;
}

export interface HolidayRule {
  /** @format uuid */
  id?: string;
  name?: string;
  /** @format date */
  effectiveFrom?: string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  version?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  holidayDaysPerMonth?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  holidayDaysPerYear?: null | number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHoursPerCalendarDay?: null | number | string;
  sixthWeekEnabled?: boolean;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHoursPerMonth?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  fullTimeHoursPerWeek?: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  maximumDebtDays?: number | string;
  allowTransfer?: boolean;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  maximumTransferDays?: number | string;
  periods?: null | HolidayRulePeriodSettings;
  /** @format date-time */
  createdAt?: string;
  /** @format uuid */
  createdBy?: string;
}

export interface HolidayRulePeriodSettings {
  /** @format uuid */
  holidayRuleId?: string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayEarningStartMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayEarningStartDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartYearOffset?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndYearOffset?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekEarningStartMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekEarningStartDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartYearOffset?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndYearOffset?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayDays?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayStartMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayStartDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayEndMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayEndDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayNoticeMonths?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  otherHolidayNoticeMonths?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  debtDeadlineMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  debtDeadlineDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayTransferDeadlineMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayTransferDeadlineDay?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekTransferDeadlineMonth?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekTransferDeadlineDay?: number | string;
}

export interface HolidayRulePeriodsResult {
  weeksOneToFive?: HolidayAccountPeriod;
  sixthWeek?: HolidayAccountPeriod;
  sixthWeekEnabled?: boolean;
  /** @format date */
  mainHolidayStart?: string;
  /** @format date */
  mainHolidayEnd?: string;
  /** @format date */
  debtDeadline?: string;
}

export interface HolidayRuleSettings {
  /**
   * @minLength 0
   * @maxLength 100
   */
  name: string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  holidayDaysPerMonth: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  holidayDaysPerYear: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHoursPerCalendarDay: number | string;
  sixthWeekEnabled: boolean;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  sixthWeekHoursPerMonth: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  fullTimeHoursPerWeek: number | string;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  maximumDebtDays: number | string;
  allowTransfer: boolean;
  /**
   * @format double
   * @pattern ^-?(?:0|[1-9]\d*)(?:\.\d+)?$
   */
  maximumTransferDays: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayEarningStartMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayEarningStartDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageStartYearOffset: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayUsageEndYearOffset: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekEarningStartMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekEarningStartDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageStartYearOffset: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekUsageEndYearOffset: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayDays: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayStartMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayStartDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayEndMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayEndDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  mainHolidayNoticeMonths: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  otherHolidayNoticeMonths: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  debtDeadlineMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  debtDeadlineDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayTransferDeadlineMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  holidayTransferDeadlineDay: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekTransferDeadlineMonth: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  sixthWeekTransferDeadlineDay: number | string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ProblemDetails {
  type?: null | string;
  title?: null | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  status?: null | number | string;
  detail?: null | string;
  instance?: null | string;
}

export interface UpdateUserRequest {
  email: string;
  role: string;
}

export interface UserInfo {
  /** @format uuid */
  id: string;
  email: string;
  role: string;
}

export interface UserResponse {
  /** @format uuid */
  id: string;
  email: string;
  role: string;
  isActive: boolean;
}

export interface WeatherForecast {
  /** @format date */
  date?: string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  temperatureC?: number | string;
  /**
   * @format int32
   * @pattern ^-?(?:0|[1-9]\d*)$
   */
  temperatureF?: number | string;
  summary?: null | string;
}

export interface WorkSchedule {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  employmentId?: string;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: null | string;
}

export interface WorkScheduleDay {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  workScheduleId?: string;
  day?: DayOfWeek;
  isDayOff?: boolean;
  /** @format time */
  startTime?: null | string;
  /** @format time */
  endTime?: null | string;
}

export interface WorkScheduleDayRequest {
  day?: DayOfWeek;
  isDayOff?: boolean;
  /** @format time */
  startTime?: null | string;
  /** @format time */
  endTime?: null | string;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5159/";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title Api | v1
 * @version 1.0.0
 * @baseUrl http://localhost:5159/
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthLoginCreate
     * @request POST:/api/auth/login
     */
    authLoginCreate: (data: LoginRequest, params: RequestParams = {}) =>
      this.request<UserInfo, ProblemDetails>({
        path: `/api/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogoutCreate
     * @request POST:/api/auth/logout
     */
    authLogoutCreate: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/auth/logout`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthMeList
     * @request GET:/api/auth/me
     */
    authMeList: (params: RequestParams = {}) =>
      this.request<UserInfo, ProblemDetails>({
        path: `/api/auth/me`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CalendarDays
     * @name CalendarDaysList
     * @request GET:/api/calendar-days
     */
    calendarDaysList: (params: RequestParams = {}) =>
      this.request<CalendarDay[], any>({
        path: `/api/calendar-days`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CalendarDays
     * @name CalendarDaysCreate
     * @request POST:/api/calendar-days
     */
    calendarDaysCreate: (
      data: CreateCalendarDayRequest,
      params: RequestParams = {},
    ) =>
      this.request<CalendarDay, ProblemDetails>({
        path: `/api/calendar-days`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Employees
     * @name EmployeesList
     * @request GET:/api/employees
     */
    employeesList: (params: RequestParams = {}) =>
      this.request<UserResponse[], any>({
        path: `/api/employees`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Employees
     * @name EmployeesCreate
     * @request POST:/api/employees
     */
    employeesCreate: (
      data: CreateEmployeeRequest,
      params: RequestParams = {},
    ) =>
      this.request<UserResponse, any>({
        path: `/api/employees`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Employments
     * @name UsersEmploymentsList
     * @request GET:/api/users/{userId}/employments
     */
    usersEmploymentsList: (userId: string, params: RequestParams = {}) =>
      this.request<Employment[], any>({
        path: `/api/users/${userId}/employments`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Employments
     * @name UsersEmploymentsCreate
     * @request POST:/api/users/{userId}/employments
     */
    usersEmploymentsCreate: (
      userId: string,
      data: CreateEmploymentRequest,
      params: RequestParams = {},
    ) =>
      this.request<Employment, ProblemDetails>({
        path: `/api/users/${userId}/employments`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Employments
     * @name UsersEmploymentsEndPartialUpdate
     * @request PATCH:/api/users/{userId}/employments/{employmentId}/end
     */
    usersEmploymentsEndPartialUpdate: (
      userId: string,
      employmentId: string,
      data: EndEmploymentRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/api/users/${userId}/employments/${employmentId}/end`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayAccrual
     * @name UsersHolidayAccrualList
     * @request GET:/api/users/{userId}/holiday-accrual
     */
    usersHolidayAccrualList: (
      userId: string,
      query?: {
        /**
         * @format int32
         * @pattern ^-?(?:0|[1-9]\d*)$
         */
        year?: number | string;
        /**
         * @format int32
         * @pattern ^-?(?:0|[1-9]\d*)$
         */
        month?: number | string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccrualResult, any>({
        path: `/api/users/${userId}/holiday-accrual`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayAccrual
     * @name UsersHolidayAccrualCreate
     * @request POST:/api/users/{userId}/holiday-accrual
     */
    usersHolidayAccrualCreate: (
      userId: string,
      query?: {
        /**
         * @format int32
         * @pattern ^-?(?:0|[1-9]\d*)$
         */
        year?: number | string;
        /**
         * @format int32
         * @pattern ^-?(?:0|[1-9]\d*)$
         */
        month?: number | string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccrualResult, any>({
        path: `/api/users/${userId}/holiday-accrual`,
        method: "POST",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayAccrual
     * @name UsersHolidayAccrualForecastList
     * @request GET:/api/users/{userId}/holiday-accrual/forecast
     */
    usersHolidayAccrualForecastList: (
      userId: string,
      query?: {
        /** @format date */
        date?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayAccrualResult, any>({
        path: `/api/users/${userId}/holiday-accrual/forecast`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesList
     * @request GET:/api/users/{userId}/holiday-balances
     */
    usersHolidayBalancesList: (
      userId: string,
      query?: {
        /** @format date */
        date?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceResult, any>({
        path: `/api/users/${userId}/holiday-balances`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesForecastList
     * @request GET:/api/users/{userId}/holiday-balances/forecast
     */
    usersHolidayBalancesForecastList: (
      userId: string,
      query?: {
        /** @format date */
        date?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceResult, any>({
        path: `/api/users/${userId}/holiday-balances/forecast`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesReconcileCreate
     * @request POST:/api/users/{userId}/holiday-balances/reconcile
     */
    usersHolidayBalancesReconcileCreate: (
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceResult, any>({
        path: `/api/users/${userId}/holiday-balances/reconcile`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesConversionsList
     * @request GET:/api/users/{userId}/holiday-balances/conversions
     */
    usersHolidayBalancesConversionsList: (
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceConversion[], any>({
        path: `/api/users/${userId}/holiday-balances/conversions`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesAdjustmentsList
     * @request GET:/api/users/{userId}/holiday-balances/adjustments
     */
    usersHolidayBalancesAdjustmentsList: (
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceAdjustment[], any>({
        path: `/api/users/${userId}/holiday-balances/adjustments`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesAdjustmentsCreate
     * @request POST:/api/users/{userId}/holiday-balances/adjustments
     */
    usersHolidayBalancesAdjustmentsCreate: (
      userId: string,
      data: HolidayAdjustmentRequest,
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceAdjustment, any>({
        path: `/api/users/${userId}/holiday-balances/adjustments`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayBalances
     * @name UsersHolidayBalancesOpeningBalancesCreate
     * @request POST:/api/users/{userId}/holiday-balances/opening-balances
     */
    usersHolidayBalancesOpeningBalancesCreate: (
      userId: string,
      data: HolidayOpeningBalanceRequest,
      params: RequestParams = {},
    ) =>
      this.request<HolidayBalanceAdjustment, any>({
        path: `/api/users/${userId}/holiday-balances/opening-balances`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayHours
     * @name EmploymentsHolidayHoursList
     * @request GET:/api/employments/{employmentId}/holiday-hours
     */
    employmentsHolidayHoursList: (
      employmentId: string,
      query?: {
        /** @format date */
        startDate?: string;
        /** @format date */
        endDate?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayHoursResult, ProblemDetails>({
        path: `/api/employments/${employmentId}/holiday-hours`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsList
     * @request GET:/api/holiday-requests
     */
    holidayRequestsList: (params: RequestParams = {}) =>
      this.request<HolidayRequestResponse[], any>({
        path: `/api/holiday-requests`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsCreate
     * @request POST:/api/holiday-requests
     */
    holidayRequestsCreate: (
      data: HolidayRequestInput,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRequestResponse, any>({
        path: `/api/holiday-requests`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsPendingList
     * @request GET:/api/holiday-requests/pending
     */
    holidayRequestsPendingList: (params: RequestParams = {}) =>
      this.request<HolidayRequestResponse[], any>({
        path: `/api/holiday-requests/pending`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsReviewList
     * @request GET:/api/holiday-requests/review
     */
    holidayRequestsReviewList: (params: RequestParams = {}) =>
      this.request<HolidayRequestResponse[], any>({
        path: `/api/holiday-requests/review`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsDetail
     * @request GET:/api/holiday-requests/{id}
     */
    holidayRequestsDetail: (id: string, params: RequestParams = {}) =>
      this.request<HolidayRequestResponse, any>({
        path: `/api/holiday-requests/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsDelete
     * @request DELETE:/api/holiday-requests/{id}
     */
    holidayRequestsDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/holiday-requests/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsDaysList
     * @request GET:/api/holiday-requests/{id}/days
     */
    holidayRequestsDaysList: (id: string, params: RequestParams = {}) =>
      this.request<HolidayRequestDay[], any>({
        path: `/api/holiday-requests/${id}/days`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsDecisionCreate
     * @request POST:/api/holiday-requests/{id}/decision
     */
    holidayRequestsDecisionCreate: (
      id: string,
      data: HolidayDecisionInput,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRequestResponse, any>({
        path: `/api/holiday-requests/${id}/decision`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsPreviewCreate
     * @request POST:/api/holiday-requests/preview
     */
    holidayRequestsPreviewCreate: (
      data: HolidayRequestInput,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRequestPreview, any>({
        path: `/api/holiday-requests/preview`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsCancellationCreate
     * @request POST:/api/holiday-requests/{id}/cancellation
     */
    holidayRequestsCancellationCreate: (
      id: string,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRequestResponse, any>({
        path: `/api/holiday-requests/${id}/cancellation`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRequests
     * @name HolidayRequestsCancellationDecisionCreate
     * @request POST:/api/holiday-requests/{id}/cancellation/decision
     */
    holidayRequestsCancellationDecisionCreate: (
      id: string,
      data: HolidayDecisionInput,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRequestResponse, any>({
        path: `/api/holiday-requests/${id}/cancellation/decision`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesList
     * @request GET:/api/holiday-rules
     */
    holidayRulesList: (params: RequestParams = {}) =>
      this.request<HolidayRule[], any>({
        path: `/api/holiday-rules`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesCreate
     * @request POST:/api/holiday-rules
     */
    holidayRulesCreate: (
      data: CreateHolidayRuleRequest,
      params: RequestParams = {},
    ) =>
      this.request<HolidayRule, ProblemDetails>({
        path: `/api/holiday-rules`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesDetail
     * @request GET:/api/holiday-rules/{id}
     */
    holidayRulesDetail: (id: string, params: RequestParams = {}) =>
      this.request<HolidayRule, any>({
        path: `/api/holiday-rules/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesPresetsList
     * @request GET:/api/holiday-rules/presets
     */
    holidayRulesPresetsList: (params: RequestParams = {}) =>
      this.request<HolidayRuleSettings[], any>({
        path: `/api/holiday-rules/presets`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesPeriodsList
     * @request GET:/api/holiday-rules/{id}/periods
     */
    holidayRulesPeriodsList: (
      id: string,
      query?: {
        /**
         * @format int32
         * @pattern ^-?(?:0|[1-9]\d*)$
         */
        earningYear?: number | string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayRulePeriodsResult, any>({
        path: `/api/holiday-rules/${id}/periods`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags HolidayRules
     * @name HolidayRulesForDateList
     * @request GET:/api/holiday-rules/for-date
     */
    holidayRulesForDateList: (
      query?: {
        /** @format date */
        date?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<HolidayRule, any>({
        path: `/api/holiday-rules/for-date`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersList
     * @request GET:/api/users
     */
    usersList: (params: RequestParams = {}) =>
      this.request<UserResponse[], any>({
        path: `/api/users`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersCreate
     * @request POST:/api/users
     */
    usersCreate: (data: CreateUserRequest, params: RequestParams = {}) =>
      this.request<UserResponse, any>({
        path: `/api/users`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersDetail
     * @request GET:/api/users/{id}
     */
    usersDetail: (id: string, params: RequestParams = {}) =>
      this.request<UserResponse, any>({
        path: `/api/users/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersUpdate
     * @request PUT:/api/users/{id}
     */
    usersUpdate: (
      id: string,
      data: UpdateUserRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/users/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersDelete
     * @request DELETE:/api/users/{id}
     */
    usersDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/users/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Users
     * @name UsersReactivateCreate
     * @request POST:/api/users/{id}/reactivate
     */
    usersReactivateCreate: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/users/${id}/reactivate`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesList
     * @request GET:/api/employments/{employmentId}/work-schedules
     */
    employmentsWorkSchedulesList: (
      employmentId: string,
      params: RequestParams = {},
    ) =>
      this.request<WorkSchedule[], any>({
        path: `/api/employments/${employmentId}/work-schedules`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesCreate
     * @request POST:/api/employments/{employmentId}/work-schedules
     */
    employmentsWorkSchedulesCreate: (
      employmentId: string,
      data: CreateWorkScheduleRequest,
      params: RequestParams = {},
    ) =>
      this.request<WorkSchedule, ProblemDetails>({
        path: `/api/employments/${employmentId}/work-schedules`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesForDateList
     * @request GET:/api/employments/{employmentId}/work-schedules/for-date
     */
    employmentsWorkSchedulesForDateList: (
      employmentId: string,
      query?: {
        /** @format date */
        date?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<WorkSchedule, ProblemDetails>({
        path: `/api/employments/${employmentId}/work-schedules/for-date`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesDaysList
     * @request GET:/api/employments/{employmentId}/work-schedules/{workScheduleId}/days
     */
    employmentsWorkSchedulesDaysList: (
      employmentId: string,
      workScheduleId: string,
      params: RequestParams = {},
    ) =>
      this.request<WorkScheduleDay[], any>({
        path: `/api/employments/${employmentId}/work-schedules/${workScheduleId}/days`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesEndPartialUpdate
     * @request PATCH:/api/employments/{employmentId}/work-schedules/{workScheduleId}/end
     */
    employmentsWorkSchedulesEndPartialUpdate: (
      employmentId: string,
      workScheduleId: string,
      data: EndWorkScheduleRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/api/employments/${employmentId}/work-schedules/${workScheduleId}/end`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name EmploymentsWorkSchedulesUpdate
     * @request PUT:/api/employments/{employmentId}/work-schedules/{workScheduleId}
     */
    employmentsWorkSchedulesUpdate: (
      employmentId: string,
      workScheduleId: string,
      data: ChangeWorkScheduleRequest,
      params: RequestParams = {},
    ) =>
      this.request<WorkSchedule, ProblemDetails>({
        path: `/api/employments/${employmentId}/work-schedules/${workScheduleId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  weatherForecast = {
    /**
     * No description
     *
     * @tags WeatherForecast
     * @name GetWeatherForecast
     * @request GET:/WeatherForecast
     */
    getWeatherForecast: (params: RequestParams = {}) =>
      this.request<WeatherForecast[], any>({
        path: `/WeatherForecast`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
}
