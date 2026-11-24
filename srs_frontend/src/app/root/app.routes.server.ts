import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'properties/:id',
    renderMode: RenderMode.Client
  },
  {
    path: 'properties/:id/edit',
    renderMode: RenderMode.Client
  },
  {
    path: 'users/:id/edit',
    renderMode: RenderMode.Client
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
