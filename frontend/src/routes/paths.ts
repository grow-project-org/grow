/** Centralised route paths — the single source of truth for navigation. */
export const ROUTES = {
  today: '/',
  plants: '/plants',
  plant: '/plants/:id',
  repot: '/plants/:id/repot',
  add: '/add',
  calendar: '/calendar',
  groups: '/groups',
} as const;

export const plantPath = (id: number): string => `/plants/${id}`;
export const repotPath = (id: number): string => `/plants/${id}/repot`;
