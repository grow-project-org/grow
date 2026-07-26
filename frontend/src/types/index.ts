/**
 * Core domain types shared across the app.
 * A plant is a concrete instance (not a statistic): each has its own id,
 * container, dates and history.
 */

/** Recurring care actions that have a per-species interval. */
export type ActionType = 'water' | 'fert';

/** Every kind of event that can land in a plant's history log. */
export type LogType =
  | 'water'
  | 'fert'
  | 'repot'
  | 'add'
  | 'prune'
  | 'harvest'
  | 'move';

/**
 * Group behaviour is driven by its type:
 * - `work`   — same rhythm, bulk actions apply at once
 * - `region` — an area of the garden, used for stats/overview
 * - `adhoc`  — a temporary, one-off task grouping
 */
export type GroupType = 'work' | 'region' | 'adhoc';

export interface Plant {
  readonly id: number;
  /** Human-facing identifier, e.g. `PAP-05`. User-renameable. */
  code: string;
  name: string;
  species: string | null;
  emoji: string;
  loc: string | null;
  /** Pot volume in litres. */
  potL: number | null;
  /** Pot diameter in centimetres. */
  potCm: number | null;
  groups: string[];
  /** ISO date (yyyy-mm-dd) of the last watering. */
  lastWater: string;
  /** ISO date of the last fertilising, or null if never / not applicable. */
  lastFert: string | null;
}

export interface Group {
  name: string;
  emoji: string;
  type: GroupType;
}

export interface LogEntry {
  readonly uid: number;
  /** The plant instance this entry belongs to. */
  readonly id: number;
  readonly type: LogType;
  /** ISO date (yyyy-mm-dd). */
  readonly date: string;
}

/** Care intervals (in days) for a species. `null` means "not tracked". */
export interface SpeciesInfo {
  /** Watering interval in days. */
  readonly w: number | null;
  /** Fertilising interval in days. */
  readonly f: number | null;
}
