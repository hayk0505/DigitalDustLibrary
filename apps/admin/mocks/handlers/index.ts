import { authHandlers } from './auth'
import { postHandlers } from './posts'
import { mediaHandlers } from './media'

export const handlers = [...authHandlers, ...postHandlers, ...mediaHandlers]
