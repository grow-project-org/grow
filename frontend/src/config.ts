/**
 * App-wide configuration.
 *
 * The prototype is pinned to a fixed "today" so the seeded schedule always
 * demonstrates a realistic mix of due/overdue/upcoming plants. Swap this for
 * `new Date().toISOString().slice(0, 10)` to run against the real date.
 */
export const TODAY = '2026-07-25';

/** Upper bound for a single bulk-add operation. */
export const MAX_BULK_ADD = 200;
