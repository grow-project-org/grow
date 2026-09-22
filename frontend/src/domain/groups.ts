import type { GroupType } from '../types';

export const GROUP_TYPE_TO_API: Record<GroupType, number> = {
  region: 1,
  work: 2,
  adhoc: 3,
};

export const GROUP_TYPE_FROM_API = (value: number): GroupType => {
  if (value === 1) return 'region';
  if (value === 3) return 'adhoc';
  return 'work';
};

export interface GroupTypeMeta {
  emoji: string;
  label: string;
  bg: string;
  ink: string;
}

export const GROUP_TYPE_META: Record<GroupType, GroupTypeMeta> = {
  work: { emoji: '⚡', label: 'Grupa robocza', bg: '#e7f0ff', ink: '#2f5fa8' },
  region: { emoji: '📍', label: 'Region', bg: '#f3ecff', ink: '#6b4bb0' },
  adhoc: { emoji: '📌', label: 'Grupa tymczasowa', bg: '#fff0e0', ink: '#b5701a' },
};
