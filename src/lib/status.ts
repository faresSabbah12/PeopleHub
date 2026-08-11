/**
 * Shared HR status vocabulary. Extend here so every module (attendance,
 * requests, payroll) speaks the same language and reuses the same labels.
 */
export type Status =
  | 'present'
  | 'remote'
  | 'leave'
  | 'absent'
  | 'pending'
  | 'approved'
  | 'rejected';

/** Labels live in the common namespace so any page can translate a status. */
export const statusLabelKeys = {
  present: 'common:STATUS_PRESENT',
  remote: 'common:STATUS_REMOTE',
  leave: 'common:STATUS_LEAVE',
  absent: 'common:STATUS_ABSENT',
  pending: 'common:STATUS_PENDING',
  approved: 'common:STATUS_APPROVED',
  rejected: 'common:STATUS_REJECTED',
} as const satisfies Record<Status, string>;
