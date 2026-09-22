import type { ActionType } from '../types';

export const ACTION_TYPES: readonly ActionType[] = ['water', 'fert'];

export const ACTION_API_TYPE: Record<ActionType, number> = {
  water: 0,
  fert: 1,
};

export const ACTION_ROUTE_NAME: Record<ActionType, string> = {
  water: 'Watering',
  fert: 'Fertilizing',
};

export const ACTION_DICTIONARY_KEY: Record<ActionType, 'Watering' | 'Fertilizing'> = {
  water: 'Watering',
  fert: 'Fertilizing',
};

export interface ActionMeta {
  emoji: string;
  verb: string;
  label: string;
  doneLabel: string;
}

export const ACTION_META: Record<ActionType, ActionMeta> = {
  water: { emoji: '💧', verb: 'Podlej', label: 'Podlewanie', doneLabel: 'Podlano' },
  fert: { emoji: '🌱', verb: 'Nawóź', label: 'Nawożenie', doneLabel: 'Nawieziono' },
};
