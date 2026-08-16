/**
 * Signed-in user. Static until authentication lands; the shape mirrors what the
 * API is expected to return so only this file changes.
 */
export const currentUser = {
  name: 'Fares Sabbah',
  roleKey: 'common:HR_MANAGER',
} as const;
