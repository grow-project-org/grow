import type { Group, LogEntry, Plant } from '../types';
import type { GardenState } from '../state/gardenReducer';

/**
 * Wire format exchanged with the backend — the full garden snapshot plus a
 * server-set timestamp for last-write-wins reconciliation.
 */
export interface GardenSnapshotDTO {
  garden: Plant[];
  groups: Group[];
  log: LogEntry[];
  done: Record<string, boolean>;
  dismissed: Record<string, boolean>;
  updatedAt: string;
}

export const toDTO = (state: GardenState): GardenSnapshotDTO => ({
  garden: state.garden,
  groups: state.groups,
  log: state.log,
  done: state.done,
  dismissed: state.dismissed,
  updatedAt: new Date().toISOString(),
});

export const fromDTO = (dto: GardenSnapshotDTO): GardenState => ({
  garden: dto.garden,
  groups: dto.groups,
  log: dto.log,
  done: dto.done,
  dismissed: dto.dismissed,
});
