import { request } from './http';
import type {
  AddEventRequest,
  AddEventResponse,
  CreatePlantGroupRequest,
  CreatePlantGroupResponse,
  CreatePlantRequest,
  CreatePlantResponse,
  CreateSpecieRequest,
  CreateSpecieResponse,
  CreateUserRequest,
  LoginRequest,
  MeResponse,
  SpecieDto,
  UpdateIntervalRequest,
} from './dto';

/** `POST /api/users/*` — session auth. There is no mapped `/logout` route
 *  yet (`UsersEndpoints.Logout` exists but isn't registered in
 *  `MapUsersEndpoints`) — TODO(backend): expose a logout route. */
export const usersApi = {
  register: (body: CreateUserRequest): Promise<void> =>
    request<void>('/api/users/register', { method: 'POST', body }),
  login: (body: LoginRequest): Promise<void> =>
    request<void>('/api/users/login', { method: 'POST', body }),
  /** Doubles as the "am I signed in" probe — note this is POST, not GET. */
  me: (): Promise<MeResponse> => request<MeResponse>('/api/users/me', { method: 'POST' }),
};

/** `/api/species` — the only resource with a real read endpoint today. */
export const speciesApi = {
  list: (): Promise<SpecieDto[]> => request<SpecieDto[]>('/api/species'),
  create: (body: CreateSpecieRequest): Promise<CreateSpecieResponse> =>
    request<CreateSpecieResponse>('/api/species', { method: 'POST', body }),
  updateInterval: (
    specieId: string,
    actionTypeRouteName: 'Watering' | 'Fertilizing',
    body: UpdateIntervalRequest,
  ): Promise<void> =>
    request<void>(`/api/species/${specieId}/interval/${actionTypeRouteName}`, { method: 'POST', body }),
};

/** `/api/plants` — create + water/fert events + group membership.
 *  TODO(backend): no GET to list/read plants, no repot/prune/harvest/custom
 *  event types, no way to undo/delete a logged event. */
export const plantsApi = {
  create: (body: CreatePlantRequest): Promise<CreatePlantResponse> =>
    request<CreatePlantResponse>('/api/plants', { method: 'POST', body }),
  addEvent: (plantId: string, body: AddEventRequest): Promise<AddEventResponse> =>
    request<AddEventResponse>(`/api/plants/${plantId}/events`, { method: 'POST', body }),
  addToGroup: (plantId: string, groupId: string): Promise<void> =>
    request<void>(`/api/plants/${plantId}/groups/${groupId}`, { method: 'POST' }),
  removeFromGroup: (plantId: string, groupId: string): Promise<void> =>
    request<void>(`/api/plants/${plantId}/groups/${groupId}`, { method: 'DELETE' }),
};

/** `/api/plant-groups` — create only. TODO(backend): no GET to list groups. */
export const plantGroupsApi = {
  create: (body: CreatePlantGroupRequest): Promise<CreatePlantGroupResponse> =>
    request<CreatePlantGroupResponse>('/api/plant-groups', { method: 'POST', body }),
};
