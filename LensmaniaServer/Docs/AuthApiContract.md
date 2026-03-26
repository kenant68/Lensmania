# Auth API Contract

## Base Route

- `/api/auth`

## Endpoints

### POST `/api/auth/register`

Request body:

```json
{
  "username": "string (required, 3..32)",
  "email": "valid email (required, max 254)",
  "password": "string (required, 8..128)"
}
```

Success response (`200 OK`):

```json
{
  "token": "jwt",
  "username": "string",
  "isAdmin": false,
  "isPremium": false
}
```

Error response (`409 Conflict`):

```json
{
  "code": "AUTH_DUPLICATE_IDENTITY",
  "message": "Email or username is already in use."
}
```

Validation error response (`400 Bad Request`):

```json
{
  "code": "AUTH_VALIDATION_FAILED",
  "message": "Validation failed for authentication request.",
  "errors": {
    "FieldName": [
      "validation message"
    ]
  }
}
```

### POST `/api/auth/login`

Request body:

```json
{
  "email": "valid email (required, max 254)",
  "password": "string (required, 8..128)"
}
```

Success response (`200 OK`):

```json
{
  "token": "jwt",
  "username": "string",
  "isAdmin": false,
  "isPremium": false
}
```

Error response (`401 Unauthorized`):

```json
{
  "code": "AUTH_INVALID_CREDENTIALS",
  "message": "Invalid email or password."
}
```

Validation error response (`400 Bad Request`):

```json
{
  "code": "AUTH_VALIDATION_FAILED",
  "message": "Validation failed for authentication request.",
  "errors": {
    "FieldName": [
      "validation message"
    ]
  }
}
```

## JWT Contract

- Signing algorithm: `HS256`
- Expiration: `Jwt:ExpirationMinutes` (default `60`)
- Issuer: `Jwt:Issuer`
- Audience: `Jwt:Audience`

Claims:

- `sub`: user id
- `nameidentifier`: user id
- `name`: username
- `email`: user email
- `isAdmin`: boolean serialized as string (`True`/`False`)
- `isPremium`: boolean serialized as string (`True`/`False`)
