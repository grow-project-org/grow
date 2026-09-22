export type ActionType = 'water' | 'fert';

export type GroupType = 'work' | 'region' | 'adhoc';

export interface Species {
  readonly id: string;
  readonly name: string;
  readonly w: number | null;
  readonly f: number | null;
}

export interface Plant {
  readonly id: string;
  readonly code: string;
  readonly specieId: string;
  readonly groupIds: readonly string[];
  readonly lastWater: string | null;
  readonly lastFert: string | null;
  readonly nextWater: string | null;
  readonly nextFert: string | null;
}

export interface Group {
  readonly id: string;
  readonly name: string;
  readonly type: GroupType;
  readonly plantIds: readonly string[];
}
