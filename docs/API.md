# Lensmania — API Documentation

> Base URL : `/api`
> Authentification : Bearer JWT (HS256) via header `Authorization: Bearer <token>`
> Toutes les dates sont en ISO 8601 UTC.
> Document vérifié par rapport au code (`Controllers/`, `Services/`, `LensmaniaLibrary/DTOs/`) — 14/06/2026.

---

## Table des matières

1. [Auth](#1-auth)
2. [Posts](#2-posts)
3. [Uploads](#3-uploads)
4. [Events](#4-events)
5. [Themes](#5-themes)
6. [Users](#6-users)
7. [Health](#7-health)
8. [Modèles communs](#8-modèles-communs)
9. [Codes d'erreur](#9-codes-derreur)

---

## 1. Auth

Base route : `/api/auth`

---

### POST `/api/auth/register`

Crée un nouveau compte utilisateur (authentification locale).

**Request body**

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "motdepasse123"
}
```

| Champ      | Type     | Requis | Contraintes          |
|------------|----------|--------|----------------------|
| `username` | `string` | Oui    | 3..32 caractères     |
| `email`    | `string` | Oui    | Email valide, max 254|
| `password` | `string` | Oui    | 8..128 caractères    |

**Responses**

`200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiJ9...",
  "username": "johndoe",
  "isAdmin": false,
  "isPremium": false,
  "isActive": true
}
```

`409 Conflict` — Email ou username déjà utilisé
```json
{
  "code": "AUTH_DUPLICATE_IDENTITY",
  "message": "Cet e-mail ou ce nom d'utilisateur est deja utilise."
}
```

`400 Bad Request` — Validation du formulaire échouée
```json
{
  "code": "AUTH_VALIDATION_FAILED",
  "message": "Certaines donnees du formulaire sont invalides.",
  "errors": {
    "FieldName": ["validation message"]
  }
}
```

---

### POST `/api/auth/login`

Authentifie un utilisateur via email/mot de passe.

**Request body**

```json
{
  "email": "john@example.com",
  "password": "motdepasse123"
}
```

**Responses**

`200 OK` — Retourne `AuthResponse` (même format que `/register`)

`401 Unauthorized` — Identifiants incorrects
```json
{
  "code": "AUTH_INVALID_CREDENTIALS",
  "message": "E-mail ou mot de passe incorrect."
}
```

`403 Forbidden` — Compte bloqué
```json
{
  "code": "AUTH_USER_IS_BLOCKED",
  "message": "Votre compte a été bloqué"
}
```

---

### POST `/api/auth/google`

Authentifie ou inscrit un utilisateur via Google OAuth (ID token Google).

Si l'email Google correspond à un compte existant, il est lié. Sinon, un compte est créé automatiquement avec un username généré.

**Request body**

```json
{
  "idToken": "eyJhbGciOiJSUzI1NiJ9..."
}
```

**Responses**

`200 OK` — Retourne `AuthResponse`

`401 Unauthorized` — Token invalide ou email non vérifié
```json
{
  "code": "AUTH_GOOGLE_INVALID_TOKEN",
  "message": "Le jeton Google est invalide ou a expiré."
}
```
```json
{
  "code": "AUTH_GOOGLE_EMAIL_UNVERIFIED",
  "message": "Votre adresse e-mail Google n'est pas vérifiée."
}
```

`403 Forbidden` — Compte bloqué (même format que `/login`)

---

### POST `/api/auth/forgot-password`

Déclenche l'envoi d'un email de réinitialisation de mot de passe.

> Toujours `200 OK` même si l'email n'existe pas (sécurité anti-énumération).

**Request body**

```json
{
  "email": "john@example.com"
}
```

**Response — `200 OK`**
```json
{
  "message": "Si l'adresse correspond à un compte, un lien de réinitialisation a été envoyé."
}
```

---

### POST `/api/auth/reset-password`

Réinitialise le mot de passe via le token reçu par email. Le token est à usage unique et a une durée de vie limitée.

**Request body**

```json
{
  "token": "base64url_token",
  "newPassword": "nouveaumotdepasse123"
}
```

**Responses**

`200 OK`
```json
{
  "message": "Mot de passe modifié avec succès."
}
```

`400 Bad Request` — Token invalide, expiré ou déjà utilisé
```json
{
  "code": "AUTH_INVALID_OR_EXPIRED_RESET_TOKEN",
  "message": "Ce lien de réinitialisation est invalide ou a expiré."
}
```

---

### JWT Contract

| Claim              | Valeur                              |
|--------------------|-------------------------------------|
| `sub`              | ID utilisateur                      |
| `nameidentifier`   | ID utilisateur                      |
| `name`             | Username                            |
| `email`            | Email                               |
| `isAdmin`          | `"true"` / `"false"` (string)       |
| `isPremium`        | `"true"` / `"false"` (string)       |
| `isActive`         | `"true"` / `"false"` (string)       |

- Algorithme : `HS256`
- Expiration : `Jwt:ExpirationMinutes` (défaut : 60 min)
- Issuer : `Jwt:Issuer`
- Audience : `Jwt:Audience`

---

## 2. Posts

Base route : `/api/posts`

---

### GET `/api/posts`

Retourne une liste paginée de posts. Supporte le cursor-based (tri par date) et l'offset-based (tri par likes).

**Query parameters**

| Paramètre | Type     | Requis | Défaut      | Description                                          |
|-----------|----------|--------|-------------|------------------------------------------------------|
| `limit`   | `int`    | Non    | `10`        | Posts par page (1..50)                               |
| `cursor`  | `int`    | Non    | `null`      | ID du dernier post reçu (pagination par date)        |
| `offset`  | `int`    | Non    | `null`      | Offset (pagination par likes uniquement)             |
| `sort`    | `string` | Non    | `date_desc` | `date_desc`, `date_asc`, `likes_desc`, `likes_asc`   |
| `userId`  | `int`    | Non    | `null`      | Filtre par utilisateur (ID)                          |
| `eventId` | `int`    | Non    | `null`      | Filtre par événement                                 |

> Pour `likes_desc` / `likes_asc`, utiliser `offset` au lieu de `cursor`.
> Pour `date_desc` / `date_asc`, utiliser `cursor`.

**Response — `200 OK`**

```json
{
  "posts": [
    {
      "id": 42,
      "title": "Mon post",
      "photoUrl": "abc123.jpg",
      "username": "johndoe",
      "likesCount": 12,
      "isLikedByCurrentUser": false
    }
  ],
  "hasMore": true,
  "nextCursor": 41,
  "nextOffset": null
}
```

| Champ        | Type                      | Description                                          |
|--------------|---------------------------|------------------------------------------------------|
| `posts`      | `List<PostListItemResponse>` | Posts de la page courante                         |
| `hasMore`    | `bool`                    | `true` s'il reste des posts                          |
| `nextCursor` | `int?`                    | Curseur pour la prochaine page (tri par date)        |
| `nextOffset` | `int?`                    | Offset pour la prochaine page (tri par likes)        |

---

### GET `/api/posts/{id}`

Retourne le détail d'un post par son ID.

**Response — `200 OK`**

```json
{
  "id": 42,
  "title": "Mon post",
  "photoUrl": "abc123.jpg",
  "description": "Une photo test",
  "createdAt": "2026-03-29T12:30:00Z",
  "username": "johndoe",
  "userId": 7
}
```

`404 Not Found` — Post introuvable

---

### GET `/api/posts/{username}`

Retourne les posts d'un utilisateur identifié par son username.

**Query parameters** — mêmes que `GET /api/posts` sauf `userId` et `eventId`

**Responses**

`200 OK` — Retourne `PaginatedPosts` (même format que `GET /api/posts`)
`404 Not Found` — Utilisateur introuvable

---

### POST `/api/posts`

🔒 *Authentification requise*

Crée un nouveau post. La photo doit être uploadée au préalable via `POST /api/uploads/photo`, qui retourne un nom de fichier à passer ici.

**Request body**

```json
{
  "title": "Mon post",
  "description": "Une photo test",
  "photoUrl": "abc123.jpg",
  "eventId": 1
}
```

| Champ         | Type     | Requis | Description                                         |
|---------------|----------|--------|-----------------------------------------------------|
| `title`       | `string` | Non    | Titre du post                                       |
| `description` | `string` | Non    | Description                                         |
| `photoUrl`    | `string` | Oui    | Nom de fichier retourné par `/api/uploads/photo`    |
| `eventId`     | `int`    | Non    | ID de l'événement associé                           |

**Validations métier**
- `photoUrl` doit être un nom de fichier simple (sans `/` ni `\`)
- Extension autorisée : `.jpg`, `.jpeg`, `.png`
- Le fichier doit exister sur le serveur (avoir été uploadé)
- Si `eventId` est fourni, l'événement doit exister

**Responses**

`200 OK` — Retourne `PostResponse`

`400 Bad Request`
```json
{ "message": "PhotoUrl is required" }
```

`401 Unauthorized`

---

### DELETE `/api/posts/{id}`

🔒 *Authentification requise — propriétaire du post uniquement*

Supprime un post et son fichier photo (best-effort).

| Code | Description                               |
|------|-------------------------------------------|
| 204  | Supprimé avec succès                      |
| 401  | Non authentifié                           |
| 403  | Authentifié mais pas propriétaire du post |
| 404  | Post introuvable                          |

---

### POST `/api/posts/{id}/likes`

🔒 *Authentification requise*

Toggle like/unlike sur un post (idempotent : si déjà liké, unlike ; sinon like).

| Code | Description      |
|------|------------------|
| 204  | Succès           |
| 401  | Non authentifié  |
| 404  | Post introuvable |

---

## 3. Uploads

Base route : `/api/uploads`

🔒 *Authentification requise sur tous les endpoints*

---

### POST `/api/uploads/photo`

Upload une photo de post. Retourne le nom de fichier à utiliser dans `CreatePostRequest.photoUrl`.

**Request** : `multipart/form-data`

| Champ   | Type       | Description      |
|---------|------------|------------------|
| `photo` | `IFormFile`| Fichier image    |

**Contraintes**
- Formats acceptés : `.jpg`, `.jpeg`, `.png`
- Taille max : **5 Mo**
- Vérification des magic bytes (protection contre le renommage malicieux)

**Response — `200 OK`**
```json
"abc123.jpg"
```
> La réponse est une string brute (nom de fichier), pas un objet JSON.

`400 Bad Request`
```json
{ "message": "Aucun fichier fourni." }
```
```json
{ "message": "Format non autorisé" }
```
```json
{ "message": "Fichier trop volumineux (max 5 Mo)" }
```

---

### POST `/api/uploads/badge`

Upload une image de badge. Retourne le chemin relatif à utiliser dans `CreateBadgeRequest.imageUrl`.

**Request** : `multipart/form-data`

| Champ   | Type       | Description      |
|---------|------------|------------------|
| `badge` | `IFormFile`| Fichier image    |

**Contraintes**
- Formats acceptés : `.jpg`, `.jpeg`, `.png`, `.webp`, `.svg`
- Taille max : **2 Mo**
- SVG : sanitisé automatiquement (XSS stripping)

**Response — `200 OK`**
```json
"uploads/badges/abc123.svg"
```
> La réponse est une string brute (chemin relatif).

`400 Bad Request` — Même format que `/uploads/photo`

---

## 4. Events

Base route : `/api/events`

---

### GET `/api/events`

Retourne une liste paginée d'événements (offset-based).

**Query parameters**

| Paramètre | Type     | Requis | Défaut | Description                                              |
|-----------|----------|--------|--------|----------------------------------------------------------|
| `offset`  | `int`    | Non    | `0`    | Nombre d'éléments à sauter (≥ 0)                        |
| `limit`   | `int`    | Non    | `10`   | Éléments par page (1..100)                               |
| `status`  | `string` | Non    | `null` | `active` (en cours) ou `past` (terminés). Omis = tous.   |

> Tri : `past` → par `endDate` desc ; sinon par `startDate` desc.

**Response — `200 OK`**

```json
{
  "events": [
    {
      "id": 1,
      "name": "Photo de printemps",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2026-04-30T23:59:59Z",
      "isPremium": false,
      "themeName": "Nature",
      "themeIcon": "🌿",
      "badgeCount": 1,
      "coverPhotoUrl": "abc123.jpg"
    }
  ],
  "total": 42,
  "offset": 0,
  "limit": 10
}
```

`400 Bad Request`
```json
{ "message": "status doit être 'active' ou 'past'." }
```

---

### GET `/api/events/{id}`

Retourne le détail complet d'un événement.

**Response — `200 OK`**

```json
{
  "id": 1,
  "name": "Photo de printemps",
  "description": "Capturez la beauté du printemps.",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-04-30T23:59:59Z",
  "isPremium": false,
  "theme": {
    "id": 2,
    "name": "Nature",
    "icon": "🌿"
  },
  "badges": [
    {
      "id": 10,
      "name": "Or",
      "imageUrl": "uploads/badges/abc123.svg"
    }
  ],
  "coverPhotoUrl": "abc123.jpg"
}
```

`404 Not Found`

---

### POST `/api/events`

🔒 *Admin requis*

Crée un nouvel événement. **Exactement un badge** est requis.

**Request body**

```json
{
  "name": "Photo de printemps",
  "description": "Capturez la beauté du printemps.",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-04-30T23:59:59Z",
  "isPremium": false,
  "themeId": 2,
  "badges": [
    {
      "name": "Or",
      "imageUrl": "uploads/badges/abc123.svg"
    }
  ]
}
```

**Validations**

| Champ         | Règle                                                          |
|---------------|----------------------------------------------------------------|
| `name`        | Requis, max 200 chars, pas de `< > " ' % ; ( ) & +`           |
| `description` | Max 600 chars, pas de `< > " ' % ; ( ) & +`                   |
| `startDate`   | Requis, **doit être dans le futur**                            |
| `endDate`     | Requis, **doit être après `startDate`**                        |
| `themeId`     | Entier positif, thème existant en base                         |
| `badges`      | **Exactement 1 badge** requis (pas 0, pas 2+)                  |

**Responses**

`200 OK` — Retourne `EventDetailedResponse`

`400 Bad Request`
```json
{ "message": "La date de début doit être dans le futur." }
```
```json
{ "message": "La date de fin doit être postérieure à la date de début." }
```
```json
{ "message": "Un événement doit avoir exactement un badge." }
```

`401 Unauthorized`
`403 Forbidden`

---

### PUT `/api/events/{id}`

🔒 *Admin requis*

Met à jour un événement. Remplace intégralement les badges existants. **Exactement un badge** requis.

**Request body**

```json
{
  "name": "string",
  "description": "string",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-04-30T23:59:59Z",
  "isPremium": false,
  "themeId": 2,
  "badges": [
    {
      "name": "Or",
      "imageUrl": "uploads/badges/abc123.svg"
    }
  ]
}
```

**Validations** — mêmes règles métier que POST, sauf que `startDate` n'a pas à être dans le futur.

**Responses**

`200 OK` — Retourne `EventDetailedResponse`
`400 Bad Request`
`403 Forbidden`
`404 Not Found`

---

### DELETE `/api/events/{id}`

🔒 *Admin requis*

Supprime un événement.

| Code | Description     |
|------|-----------------|
| 204  | Supprimé        |
| 403  | Non admin       |
| 404  | Introuvable     |

---

### PUT `/api/events/{id}/cover`

🔒 *Admin requis*

Définit la photo de couverture d'un événement. Le post doit appartenir à cet événement.

**Request body**

```json
{
  "postId": 42
}
```

| Champ    | Règle                         |
|----------|-------------------------------|
| `postId` | Entier ≥ 1, post de l'événement|

**Responses**

`200 OK` — Retourne `EventDetailedResponse`

`404 Not Found`
```json
{ "message": "Événement introuvable ou le post n'appartient pas à cet événement." }
```

`403 Forbidden`

---

## 5. Themes

Base route : `/api/themes`

---

### GET `/api/themes`

Retourne une liste paginée de thèmes (offset-based). Pas d'authentification requise.

**Query parameters**

| Paramètre | Type  | Requis | Défaut | Description                      |
|-----------|-------|--------|--------|----------------------------------|
| `offset`  | `int` | Non    | `0`    | Nombre d'éléments à sauter (≥ 0) |
| `limit`   | `int` | Non    | `10`   | Éléments par page (1..100)       |

**Response — `200 OK`**

```json
{
  "themes": [
    {
      "id": 2,
      "name": "Nature",
      "icon": "🌿"
    }
  ],
  "total": 15,
  "offset": 0,
  "limit": 10
}
```

`400 Bad Request`
```json
{ "message": "offset doit être supérieur ou égal à 0." }
```

---

### GET `/api/themes/{id}`

Retourne un thème par son ID.

**Response — `200 OK`**

```json
{
  "id": 2,
  "name": "Nature",
  "icon": "🌿"
}
```

`404 Not Found`

---

## 6. Users

Base route : `/api/user`

🔒 *Tous les endpoints nécessitent une authentification*

---

### GET `/api/user/me`

Retourne les claims JWT de l'utilisateur connecté (léger, sans requête DB).

**Response — `200 OK`**

```json
{
  "email": "john@example.com",
  "isAdmin": "false",
  "isPremium": "false"
}
```

> Les valeurs `isAdmin` et `isPremium` sont des **strings** (valeurs brutes du claim JWT).

---

### GET `/api/user/{username}`

Retourne le profil public d'un utilisateur par son username.

**Response — `200 OK`**

```json
{
  "id": 7,
  "username": "johndoe",
  "badges": [
    {
      "badgeId": 10,
      "name": "Or",
      "imageUrl": "uploads/badges/abc123.svg",
      "eventName": "Photo de printemps",
      "awardedAt": "2026-04-30T23:59:59Z",
      "winningPhotoUrl": "abc123.jpg"
    }
  ]
}
```

`404 Not Found`

---

### PUT `/api/user/me`

Met à jour le profil de l'utilisateur connecté. Tous les champs sont optionnels.

**Request body**

```json
{
  "username": "nouveaupseudo",
  "email": "nouveau@example.com"
}
```

| Champ      | Règle                                                   |
|------------|---------------------------------------------------------|
| `username` | Optionnel, 3..30 chars, regex `^[a-zA-Z0-9_.-]+$`      |
| `email`    | Optionnel, email valide, max 254 chars                  |

**Responses**

`200 OK` — Retourne `UserResponse`
```json
{ "id": 7, "username": "nouveaupseudo", "email": "nouveau@example.com" }
```

`400 Bad Request` — Username ou email déjà pris
```json
{ "message": "Ce nom d'utilisateur est déjà pris." }
```

`401 Unauthorized`
`404 Not Found` — Utilisateur introuvable ou inactif

---

### DELETE `/api/user/me`

Supprime (hard delete) le compte de l'utilisateur connecté.

| Code | Description              |
|------|--------------------------|
| 204  | Compte supprimé          |
| 401  | Non authentifié          |
| 404  | Utilisateur introuvable  |

---

### GET `/api/user`

🔒 *Admin requis*

Retourne la liste paginée de tous les utilisateurs.

**Query parameters**

| Paramètre | Type  | Requis | Défaut | Description                      |
|-----------|-------|--------|--------|----------------------------------|
| `offset`  | `int` | Non    | `0`    | Nombre d'éléments à sauter (≥ 0) |
| `limit`   | `int` | Non    | `20`   | Éléments par page (1..100)       |

**Response — `200 OK`**

```json
{
  "users": [
    {
      "id": 1,
      "username": "johndoe",
      "email": "john@example.com",
      "isActive": true,
      "isAdmin": false,
      "isPremium": false,
      "createdAt": "2025-01-15T10:00:00Z"
    }
  ],
  "total": 100,
  "offset": 0,
  "limit": 20
}
```

---

### PATCH `/api/user/{id}/active`

🔒 *Admin requis*

Active ou désactive un compte utilisateur (soft block/unblock).

**Request body** : booléen brut

```json
true
```

| Code | Description              |
|------|--------------------------|
| 204  | Mis à jour               |
| 403  | Non admin                |
| 404  | Utilisateur introuvable  |

---

### DELETE `/api/user/{id}`

🔒 *Admin requis*

Supprime (hard delete) un utilisateur. Impossible de supprimer un administrateur.

| Code | Description                           |
|------|---------------------------------------|
| 204  | Supprimé                              |
| 403  | Non admin                             |
| 404  | Utilisateur introuvable               |
| 409  | Conflit — l'utilisateur est admin     |

`409 Conflict`
```json
{
  "code": "business_rule_violation",
  "message": "Vous n'êtes pas autorisé à supprimer un administrateur."
}
```

---

## 7. Health

### GET `/api/health`

Endpoint de healthcheck. Pas d'authentification requise.

**Response — `200 OK`**

```json
{
  "status": "healthy",
  "timestamp": "2026-06-11T10:00:00Z"
}
```

---

## 8. Modèles communs

### `AuthResponse`

```json
{
  "token": "eyJhbGciOiJIUzI1NiJ9...",
  "username": "johndoe",
  "isAdmin": false,
  "isPremium": false,
  "isActive": true
}
```

### `PostListItemResponse` *(dans les listes paginées)*

```json
{
  "id": 42,
  "title": "Mon post",
  "photoUrl": "abc123.jpg",
  "username": "johndoe",
  "likesCount": 12,
  "isLikedByCurrentUser": false
}
```

### `PostResponse` *(détail)*

```json
{
  "id": 42,
  "title": "Mon post",
  "photoUrl": "abc123.jpg",
  "description": "Une description",
  "createdAt": "2026-03-29T12:30:00Z",
  "username": "johndoe",
  "userId": 7
}
```

### `PaginatedPosts`

```json
{
  "posts": [],
  "hasMore": true,
  "nextCursor": 41,
  "nextOffset": null
}
```

### `EventResponse` *(dans les listes)*

```json
{
  "id": 1,
  "name": "Photo de printemps",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-04-30T23:59:59Z",
  "isPremium": false,
  "themeName": "Nature",
  "themeIcon": "🌿",
  "badgeCount": 1,
  "coverPhotoUrl": "abc123.jpg"
}
```

### `EventDetailedResponse` *(détail)*

```json
{
  "id": 1,
  "name": "Photo de printemps",
  "description": "Description...",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-04-30T23:59:59Z",
  "isPremium": false,
  "theme": { "id": 2, "name": "Nature", "icon": "🌿" },
  "badges": [{ "id": 10, "name": "Or", "imageUrl": "uploads/badges/abc123.svg" }],
  "coverPhotoUrl": "abc123.jpg"
}
```

### `ThemeResponse`

```json
{ "id": 2, "name": "Nature", "icon": "🌿" }
```

### `UserResponse`

```json
{ "id": 7, "username": "johndoe", "email": "john@example.com" }
```

### `UserAdminResponse`

```json
{
  "id": 7,
  "username": "johndoe",
  "email": "john@example.com",
  "isActive": true,
  "isAdmin": false,
  "isPremium": false,
  "createdAt": "2025-01-15T10:00:00Z"
}
```

### `PublicUserProfileResponse`

```json
{
  "id": 7,
  "username": "johndoe",
  "badges": [
    {
      "badgeId": 10,
      "name": "Or",
      "imageUrl": "uploads/badges/abc123.svg",
      "eventName": "Photo de printemps",
      "awardedAt": "2026-04-30T23:59:59Z",
      "winningPhotoUrl": "abc123.jpg"
    }
  ]
}
```

### `BadgeResponse`

```json
{ "id": 10, "name": "Or", "imageUrl": "uploads/badges/abc123.svg" }
```

### `ApiErrorResponse`

```json
{
  "code": "string",
  "message": "string"
}
```

---

## 9. Codes d'erreur

### Codes métier (`code` field)

| Code                                  | HTTP | Description                                        |
|---------------------------------------|------|----------------------------------------------------|
| `AUTH_DUPLICATE_IDENTITY`             | 409  | Email ou username déjà utilisé à l'inscription     |
| `AUTH_INVALID_CREDENTIALS`            | 401  | Email ou mot de passe incorrect                    |
| `AUTH_USER_IS_BLOCKED`                | 403  | Compte désactivé par un admin                      |
| `AUTH_GOOGLE_INVALID_TOKEN`           | 401  | ID token Google invalide ou expiré                 |
| `AUTH_GOOGLE_EMAIL_UNVERIFIED`        | 401  | Email Google non vérifié                           |
| `AUTH_INVALID_OR_EXPIRED_RESET_TOKEN` | 400  | Token de réinitialisation invalide/expiré/consommé |
| `AUTH_VALIDATION_FAILED`              | 400  | Données du formulaire invalides (register/login)   |
| `business_rule_violation`             | 409  | Violation d'une règle métier (ex: suppr. admin)    |

### Codes HTTP standards

| Code | Description                                                |
|------|------------------------------------------------------------|
| 200  | Succès                                                     |
| 204  | Succès sans contenu (DELETE, toggle like, PATCH)           |
| 400  | Requête invalide (validation ou règle métier)              |
| 401  | Non authentifié (token manquant, invalide ou expiré)       |
| 403  | Accès interdit (authentifié mais droits insuffisants)      |
| 404  | Ressource introuvable                                      |
| 409  | Conflit (doublon, violation règle métier)                  |
| 500  | Erreur interne serveur                                     |

---

## Notes sur les URLs de fichiers

Les `photoUrl` et `imageUrl` retournés par l'API sont des **chemins relatifs ou noms de fichiers** (ex: `abc123.jpg`, `uploads/badges/abc123.svg`). Le client est responsable de les préfixer avec l'adresse de base du serveur de fichiers statiques pour former l'URL complète.

Exemple :
```
photoUrl: "abc123.jpg"
→ URL complète: https://server.example.com/uploads/photos/abc123.jpg

imageUrl: "uploads/badges/abc123.svg"
→ URL complète: https://server.example.com/uploads/badges/abc123.svg
```
