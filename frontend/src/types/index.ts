/**
 * Core domain types shared across the app.
 * A plant is a concrete instance (not a statistic): each has its own id,
 * species, code and history. Identity is species + code — there is no
 * separate display name; two visually different varieties of the same kind
 * of plant are two separate species, not two names under one species.
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
  | 'custom';

/**
 * Group behaviour is driven by its type:
 * - `work`   — similar rhythm/location, bulk actions apply at once
 * - `region` — a location tag; a plant can carry several at once
 * - `adhoc`  — a temporary list flagging plants for an upcoming one-off action
 */
export type GroupType = 'work' | 'region' | 'adhoc';

export interface Plant {
  readonly id: number;
  /** Physical pot label, e.g. `PJ03`. Unique across the garden, immutable after creation. */
  code: string;
  species: string | null;
  emoji: string;
  /** Pot volume in litres. */
  potL: number | null;
  /** Pot diameter in centimetres. */
  potCm: number | null;
  /** Names of every region / work group / temporary group this plant belongs to. */
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
  /** Repot only: new pot size at the time of the event, if provided. */
  readonly potL?: number | null;
  readonly potCm?: number | null;
  /** Harvest only: yield, if provided — never required. */
  readonly qty?: number | null;
  readonly weight?: number | null;
  /** Custom event only: free-text description. */
  readonly note?: string;
}

/**
 * A species is defined by its owner: private, flat (no hierarchy — two
 * varieties of the same plant are two species, not one species with a
 * variety name), carrying its own care-interval configuration.
 */
export interface Species {
  /** Unique per owner. */
  readonly name: string;
  emoji: string;
  /** Code prefix, fixed at creation (e.g. `PAP` for codes like `PAP-01`). */
  readonly prefix: string;
  /** Watering interval in days, or `null` if not tracked. */
  w: number | null;
  /** Fertilising interval in days, or `null` if not tracked. */
  f: number | null;
}
