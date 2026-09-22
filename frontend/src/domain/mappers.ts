import type { PlantDto, PlantGroupDto, SpecieDto } from '../api/resources';
import type { Group, Plant, Species } from '../types';
import { dateOf } from '../utils/date';
import { ACTION_DICTIONARY_KEY } from './actions';
import { GROUP_TYPE_FROM_API } from './groups';
import { parseIntervalDays } from './interval';

export const toSpecies = (dto: SpecieDto): Species => ({
  id: dto.id,
  name: dto.name,
  w: parseIntervalDays(dto.intervals[ACTION_DICTIONARY_KEY.water]),
  f: parseIntervalDays(dto.intervals[ACTION_DICTIONARY_KEY.fert]),
});

export const toPlant = (dto: PlantDto): Plant => ({
  id: dto.id,
  code: dto.customId,
  specieId: dto.specieId,
  groupIds: dto.plantGroupIds,
  lastWater: mapDate(dto.lastExecutions[ACTION_DICTIONARY_KEY.water]),
  lastFert: mapDate(dto.lastExecutions[ACTION_DICTIONARY_KEY.fert]),
  nextWater: dto.nextDates[ACTION_DICTIONARY_KEY.water] ?? null,
  nextFert: dto.nextDates[ACTION_DICTIONARY_KEY.fert] ?? null,
});

export const toGroup = (dto: PlantGroupDto): Group => ({
  id: dto.id,
  name: dto.name,
  type: GROUP_TYPE_FROM_API(dto.type),
  plantIds: dto.plantIds,
});

const mapDate = (timestamp: string | undefined): string | null =>
  timestamp ? dateOf(timestamp) : null;
