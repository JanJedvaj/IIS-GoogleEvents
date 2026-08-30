import type { IncomingMessage } from 'node:http';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
const target = 'https://localhost:7008';

function serveSpaOnNavigation(req: IncomingMessage): string | undefined {
  const isNavigation = req.headers.accept?.includes('text/html') ?? false;
  const isServiceEndpoint = req.url?.includes('.asmx') ?? false;

  return isNavigation && !isServiceEndpoint ? '/index.html' : undefined;
}

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target,
        changeOrigin: true,
        secure: false,
      },
      '/graphql': {
        target,
        changeOrigin: true,
        secure: false,
        bypass: serveSpaOnNavigation,
      },
      '/soap': {
        target,
        changeOrigin: true,
        secure: false,
        bypass: serveSpaOnNavigation,
      },
    },
  },
});
