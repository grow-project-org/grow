import { api, unwrap } from './client';
import type { components } from './schema';

export type SpecieDto = components['schemas']['SpecieDto'];
export type PlantDto = components['schemas']['PlantDto'];
export type PlantGroupDto = components['schemas']['PlantGroupDto'];
export type MeResponse = components['schemas']['MeResponse'];

const PAGE_SIZE = 100;

const fetchAllPages = async <T>(page: (from: number, limit: number) => Promise<T[]>): Promise<T[]> => {
  const all: T[] = [];

  for (let from = 0; ; from += PAGE_SIZE) {
    const batch = await page(from, PAGE_SIZE);
    all.push(...batch);
    if (batch.length < PAGE_SIZE) return all;
  }
};

export const usersApi = {
  register: (email: string, username: string) =>
    unwrap(api.POST('/api/users/register', { body: { email, username } })),
  login: (email: string, password: string) =>
    unwrap(api.POST('/api/users/login', { body: { email, password } })),
  me: () => unwrap(api.POST('/api/users/me')),
};

export const speciesApi = {
  list: (searchName?: string) =>
    fetchAllPages((from, limit) =>
      unwrap(api.GET('/api/species', { params: { query: { from, limit, searchName } } })),
    ),
  create: (name: string) => unwrap(api.POST('/api/species', { body: { name } })),
  updateInterval: (specieId: string, actionType: string, interval: string) =>
    unwrap(
      api.POST('/api/species/{specieId}/interval/{actionType}', {
        params: { path: { specieId, actionType } },
        body: { interval },
      }),
    ),
};

export const plantsApi = {
  list: (searchText?: string) =>
    fetchAllPages((from, limit) =>
      unwrap(api.GET('/api/plants', { params: { query: { from, limit, searchText } } })),
    ),
  create: (customId: string, specieId: string) =>
    unwrap(api.POST('/api/plants', { body: { customId, specieId } })),
  addEvent: (plantId: string, type: number, executedAt: string) =>
    unwrap(
      api.POST('/api/plants/{plantId}/events', {
        params: { path: { plantId } },
        body: { type, executedAt },
      }),
    ),
  addToGroup: (plantId: string, plantGroupId: string) =>
    unwrap(
      api.POST('/api/plants/{plantId}/groups/{plantGroupId}', {
        params: { path: { plantId, plantGroupId } },
      }),
    ),
  removeFromGroup: (plantId: string, plantGroupId: string) =>
    unwrap(
      api.DELETE('/api/plants/{plantId}/groups/{plantGroupId}', {
        params: { path: { plantId, plantGroupId } },
      }),
    ),
};

export const plantGroupsApi = {
  list: (searchName?: string) =>
    fetchAllPages((from, limit) =>
      unwrap(api.GET('/api/plant-groups', { params: { query: { from, limit, searchName } } })),
    ),
  create: (name: string, type: number) =>
    unwrap(api.POST('/api/plant-groups', { body: { name, type } })),
};
