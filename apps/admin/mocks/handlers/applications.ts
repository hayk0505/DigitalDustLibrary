import { http, HttpResponse } from 'msw'
import { applications } from '../fixtures/applications'

export const applicationHandlers = [
  http.get('/api/applications', () => HttpResponse.json(applications)),

  http.post('/api/applications/:id/approve', ({ params }) => {
    const application = applications.find((a) => a.id === params.id)
    if (!application) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    if (application.status !== 'pending') {
      return HttpResponse.json({ message: 'This application has already been reviewed.' }, { status: 409 })
    }
    application.status = 'approved'
    application.reviewedAt = new Date().toISOString()
    return HttpResponse.json(application)
  }),

  http.post('/api/applications/:id/reject', ({ params }) => {
    const application = applications.find((a) => a.id === params.id)
    if (!application) return HttpResponse.json({ message: 'Not found' }, { status: 404 })
    if (application.status !== 'pending') {
      return HttpResponse.json({ message: 'This application has already been reviewed.' }, { status: 409 })
    }
    application.status = 'rejected'
    application.reviewedAt = new Date().toISOString()
    return HttpResponse.json(application)
  }),
]
