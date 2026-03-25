---
name: Notifications Service Development
description: Guidelines for developing the Notifications microservice - an Express 5/TypeScript API with Prisma, Socket.IO, and AJV validation.
---

# Notifications Service Development

## Tech Stack

- **Runtime:** Node.js with TypeScript 5.9
- **Framework:** Express 5
- **ORM:** Prisma 7 (with @prisma/adapter-pg for PostgreSQL)
- **Database:** PostgreSQL via pg
- **Real-time:** Socket.IO 4 for WebSocket notifications
- **Validation:** AJV (ajv + ajv-formats + ajv-keywords) with JSON Schema
- **Auth:** JWT (jsonwebtoken) for internal service authentication
- **Security:** helmet, cors, express-rate-limit
- **Logging:** morgan
- **HTTP Status:** http-status-codes library
- **Dev:** nodemon with ts-node, tsx for scripts

## Project Structure

```
utils/src/
  server.ts               # Server entry (starts HTTP + Socket.IO)
  app.ts                  # Express app factory (createApp)
  config/
    config.ts             # Centralized config (env vars, cors, apiPrefix)
  controllers/
    index.ts              # Controller barrel export
    health.controller.ts  # Health check endpoint
    notifications.controller.ts  # Notification CRUD handlers
  services/
    jwt.service.ts        # JWT token verification
    pg-notify-listener.ts # PostgreSQL NOTIFY/LISTEN for real-time events
  routes/
    index.ts              # Route aggregator (mounts health + notifications)
    health.routes.ts
    notifications.routes.ts
  middleware/
    errorHandler.ts       # Global error handler
    notFoundHandler.ts    # 404 handler
    rateLimiter.ts        # Rate limiting (express-rate-limit)
    internalAuth.middleware.ts  # Internal service-to-service auth
    validateRequest.ts    # AJV schema validation middleware
  validation/
    index.ts              # Validation barrel export
    notification.validation.ts  # AJV JSON schemas + DTO interfaces
  sockets/
    index.ts              # Socket.IO setup
    notifications.socket.ts  # Notification WebSocket handlers
  lib/
    prisma.ts             # Prisma client singleton
  prisma/
    generated/            # Prisma generated client code
  constant/
    index.ts              # Constants barrel export
    database-lookups/     # Enum-like lookup constants
      roles.ts
      platforms.ts
      notificationRecipientsTypes.ts
      supplierProfileStates.ts
      memberProfileStates.ts
      ...
  types/                  # TypeScript type definitions
  utils/
    apiResponse.ts        # Standardized API response class
    appError.ts           # Custom error class
    asyncErrorHandler.ts  # Async wrapper for controllers
  scripts/
    generate-lookups.ts   # Script to generate lookup constants from DB
```

## Architecture Patterns

### App Factory
Express app created via `createApp()` in `app.ts`. Middleware order: helmet -> cors -> body parsing -> morgan -> health check -> API routes (with rate limiter) -> notFound -> errorHandler.

### Controller Pattern
Controllers use `asyncErrorHandler` wrapper to catch async errors:
```typescript
export const addNotificationRecipients = asyncErrorHandler(
  async (req: Request, res: Response) => {
    // ... business logic
    return ApiResponse.success(res, data);
  }
);
```

### Standardized Responses
All responses use `ApiResponse` class:
```typescript
ApiResponse.success(res, data, message?, statusCode?)  // { success: true, data, timestamp }
ApiResponse.error(res, message, statusCode?)            // { success: false, message, timestamp }
ApiResponse.created(res, data, message?)                // 201 response
ApiResponse.noContent(res)                              // 204 response
```

### Error Handling
Custom `AppError` class with status codes:
```typescript
throw new AppError('Notification not found', 404);
```

### Validation Middleware
AJV-based validation middleware with JSON Schema types:
```typescript
// In route:
router.post('/recipients', validateRequest(addNotificationRecipientsSchema), controller);

// Schema definition:
export const someSchema: JSONSchemaType<SomeDTO> = {
  type: 'object',
  properties: { ... },
  required: ['field1'],
};
```

### Database Access
Prisma client for all DB operations (not raw SQL):
```typescript
import { prisma } from '../lib/prisma';
const result = await prisma.notifications.findUnique({ where: { id } });
```

### Real-time
- Socket.IO for pushing notifications to connected clients
- PostgreSQL NOTIFY/LISTEN via `pg-notify-listener.ts` for DB-triggered events

### Internal Auth
Routes protected by `internalServiceAuth` middleware for service-to-service communication.

## Coding Conventions

- All source files are TypeScript (`.ts`)
- kebab-case for file names with dot-separated type suffix: `notifications.controller.ts`, `health.routes.ts`
- Barrel exports via `index.ts` in controllers, routes, constants, validation
- DTO interfaces co-located with their AJV schemas in `validation/`
- Database lookup constants in `constant/database-lookups/` (ALL_CAPS_SNAKE for values)
- Centralized config object in `config/config.ts` (not scattered `process.env`)
- Prisma for all database operations (no raw SQL)
- `asyncErrorHandler` wraps every controller function

## Naming Conventions

- Controllers: `{domain}.controller.ts`
- Routes: `{domain}.routes.ts`
- Validation: `{domain}.validation.ts`
- Services: `{domain}.service.ts`
- Sockets: `{domain}.socket.ts`
- Constants: camelCase files in `database-lookups/`

## Prisma Workflow

- **Pull schema from DB:** `npm run prisma:dbpull`
- **Generate client:** `npm run prisma:generate`
- **Build includes both:** `npm run build` runs dbpull -> generate -> tsc
- Generated code lives in `src/prisma/generated/`

## Testing

- **Lint:** `npm run lint` (ESLint 9 with typescript-eslint)
- **Lint fix:** `npm run lint:fix`
- No dedicated test framework configured yet; add Jest or Vitest as needed

## Build & Run

- **Dev:** `npm run dev` (nodemon + ts-node with watch)
- **Build:** `npm run build` (prisma:dbpull + prisma:generate + tsc)
- **Start:** `npm start` (node dist/server.js)
- **Docker:** `npm run docker:start` / `docker:stop` / `docker:restart`
- **Generate lookups:** `npm run generate-lookups` (tsx script)
- Health check at `GET /health`
