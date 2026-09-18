import type { ActionType, GroupType } from '../types';

/**
 * Wire contracts for Grow.WebApi, mirrored from the actual C# request/response
 * records (`Grow.WebApi/Endpoints/*.cs`, `Grow.WebApi/Dtos/SpecieDto.cs`).
 * There is no shared schema/OpenAPI client yet, so these are hand-kept in
 * sync — see `TODO(backend)` markers throughout the app for the capabilities
 * that don't have a backend counterpart at all yet.
 */

export interface CreateUserRequest {
  email: string;
  username: string;
}

/** `Microsoft.AspNetCore.Identity.Data.LoginRequest` — `password` is required
 *  by the DTO but not actually checked by the backend yet (see its `//todo
 *  add real confirmation` comment); we still have to send a non-empty value. */
export interface LoginRequest {
  email: string;
  password: string;
}

export interface MeResponse {
  username: string;
}

/** `PlantActionType` — only these two exist server-side; there is no generic
 *  "log an event" action (repot/prune/harvest/custom stay local-only). */
export type ApiActionType = 0 | 1;

/** JSON body enum values are plain integers (no `JsonStringEnumConverter`
 *  registered backend-side) — `Watering = 0`, `Fertilizing = 1`. */
export const ACTION_TYPE_TO_API: Record<ActionType, ApiActionType> = {
  water: 0,
  fert: 1,
};

/** The interval endpoint's route segment is instead the enum *name* string
 *  (`Enum.TryParse<PlantActionType>` in `SpeciesEndpoints.UpdateInterval`) —
 *  an asymmetry with the integer encoding used everywhere else. */
export const ACTION_TYPE_ROUTE_NAME: Record<ActionType, 'Watering' | 'Fertilizing'> = {
  water: 'Watering',
  fert: 'Fertilizing',
};

/** `GroupType` — note the numbering does not start at 0: `Region = 1`,
 *  `WorkGroup = 2`, `TemporaryGroup = 3`. */
export const GROUP_TYPE_TO_API: Record<GroupType, 1 | 2 | 3> = {
  region: 1,
  work: 2,
  adhoc: 3,
};

/** `Dictionary<PlantActionType, TimeSpan>` — dictionary keys serialise as the
 *  enum *name* (`"Watering"`/`"Fertilizing"`), PascalCase, unlike every other
 *  camelCase property in the response. Values are TimeSpan's constant
 *  `[-][d.]hh:mm:ss[.fffffff]` format. */
export interface SpecieIntervalsDto {
  Watering?: string;
  Fertilizing?: string;
}

export interface SpecieDto {
  id: string;
  name: string;
  intervals: SpecieIntervalsDto;
}

/** The frontend only ever tracks whole-day intervals — collapse a TimeSpan
 *  string down to a day count, rounding to the nearest day. */
export const parseIntervalDays = (timeSpan: string): number => {
  const match = /^(-)?(?:(\d+)\.)?(\d{2}):(\d{2}):(\d{2})/.exec(timeSpan);
  if (!match) return 0;
  const [, negative, days, hours, minutes, seconds] = match;
  const totalSeconds =
    Number(days ?? 0) * 86400 + Number(hours) * 3600 + Number(minutes) * 60 + Number(seconds);
  const value = Math.round(totalSeconds / 86400);
  return negative ? -value : value;
};

export const formatIntervalDays = (days: number): string => `${Math.max(0, Math.round(days))}.00:00:00`;

export interface CreateSpecieRequest {
  name: string;
}

export interface CreateSpecieResponse {
  specieId: string;
}

export interface UpdateIntervalRequest {
  interval: string;
}

export interface CreatePlantRequest {
  customId: string;
  specieId: string;
}

export interface CreatePlantResponse {
  createdPlantId: string;
}

export interface AddEventRequest {
  type: ApiActionType;
  executedAt: string;
}

export interface AddEventResponse {
  plantEventId: string;
}

export interface CreatePlantGroupRequest {
  name: string;
  type: 1 | 2 | 3;
}

export interface CreatePlantGroupResponse {
  createdPlantGroupId: string;
}
