import { authHandlers } from './auth'
import { postHandlers } from './posts'
import { mediaHandlers } from './media'
import { categoryHandlers } from './categories'
import { userHandlers } from './users'
import { applicationHandlers } from './applications'
import { settingsHandlers } from './settings'
import { activityHandlers } from './activity'

export const handlers = [
  ...authHandlers,
  ...postHandlers,
  ...mediaHandlers,
  ...categoryHandlers,
  ...userHandlers,
  ...applicationHandlers,
  ...settingsHandlers,
  ...activityHandlers,
]
