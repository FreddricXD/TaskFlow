import { describe, expect, it } from 'vitest'
import { BOARD_COLUMNS, STATUS_LABELS } from '@/types'

describe('board constants', () => {
  it('defines four workflow columns', () => {
    expect(BOARD_COLUMNS).toHaveLength(4)
    expect(STATUS_LABELS.InProgress).toBe('In Progress')
  })
})
