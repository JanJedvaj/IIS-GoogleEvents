# IISFe

React frontend for the `IISGoogleEvents` backend. Vite + React 19 + TypeScript (strict) + React Router + TanStack Query + MUI + React Hook Form/Zod + Axios.

A full README (prerequisites, every surface URL, the `DataSource` switch) is Phase 8 scope per the project's implementation plan - this is a placeholder until then.

## Running locally

1. Backend must already be running (see `IISGoogleEvents/`) with Postgres up via `docker compose up -d` at the monorepo root.
2. `npm install`
3. `npm run dev` - serves at `http://localhost:5173`.

API calls go through the Vite dev proxy (`vite.config.ts`) to `https://localhost:7008`, so the browser only ever talks to its own origin. This matters: the refresh-token cookie is `HttpOnly`/`Secure`/`SameSite=Strict`, and a genuinely cross-origin request (different scheme counts) gets it silently dropped by current Chrome regardless of CORS settings. `dotnet dev-certs https --trust` must already be done on this machine (it is, from the backend's Phase 1 setup) for the proxy's upstream TLS handshake to succeed.

Seeded accounts (Local mode): `admin`/`admin` (full access), `user`/`user` (read-only).
