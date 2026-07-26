import { request } from './http';
import type { GardenSnapshotDTO } from './dto';

/**
 * Preliminary REST surface for the garden. A snapshot model keeps the client
 * simple and sync robust: read the whole garden, write the whole garden.
 * Split into finer-grained resources (`/plants`, `/groups`, `/log`) once the
 * backend contract firms up.
 */
export const gardenApi = {
  /** GET /garden — load the current snapshot. */
  fetch: (): Promise<GardenSnapshotDTO> => request<GardenSnapshotDTO>('/garden'),
  /** PUT /garden — persist the whole snapshot. */
  push: (snapshot: GardenSnapshotDTO): Promise<GardenSnapshotDTO> =>
    request<GardenSnapshotDTO>('/garden', { method: 'PUT', body: snapshot }),
};
