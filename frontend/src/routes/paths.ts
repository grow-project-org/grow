export const ROUTES = {
  today: '/',
  plants: '/plants',
  plant: '/plants/:id',
  add: '/add',
  calendar: '/calendar',
  groups: '/groups',
} as const;

export const plantPath = (id: string): string => `/plants/${id}`;
