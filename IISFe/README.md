# IISFe

React frontend for the `IISGoogleEvents` backend. Vite + React 19 + TypeScript (strict) + React Router + TanStack Query + MUI + React Hook Form/Zod + Axios.

Backend surfaces this client talks to, all proxied same-origin through `vite.config.ts`:

| Surface | URL |
|---|---|
| REST + Swagger | `https://localhost:7008/swagger` |
| GraphQL (Banana Cake Pop) | `https://localhost:7008/graphql` |
| SOAP | `https://localhost:7008/soap/EventSoapService.asmx` |
| gRPC-Web | `POST /weather.WeatherService/GetWeatherByCity` |

The `AppOptions:DataSource` switch (`Local` or `External`, in `appsettings.json` or as
`AppOptions__DataSource`) decides whether events come from local Postgres or live Google Calendar;
the client reads it via `GET /api/events/capabilities` and labels pages accordingly.

The Weather page calls the backend's gRPC service directly over gRPC-Web using a client generated
from `IISGoogleEvents.API/Protos/weather.proto`. The generated output is committed at
`src/gen/weather_pb.ts` - after editing the proto, re-run `npm run generate:proto`.

## Running locally

1. Backend must already be running (see `IISGoogleEvents/`) with Postgres up via `docker compose up -d` at the monorepo root.
2. `npm install`
3. `npm run dev` - serves at `http://localhost:5173`.

API calls go through the Vite dev proxy (`vite.config.ts`) to `https://localhost:7008`, so the browser only ever talks to its own origin. This matters: the refresh-token cookie is `HttpOnly`/`Secure`/`SameSite=Strict`, and a genuinely cross-origin request (different scheme counts) gets it silently dropped by current Chrome regardless of CORS settings. `dotnet dev-certs https --trust` must already be done on this machine (it is, from the backend's Phase 1 setup) for the proxy's upstream TLS handshake to succeed.

Seeded accounts (Local mode): `admin`/`admin` (full access), `user`/`user` (read-only).
