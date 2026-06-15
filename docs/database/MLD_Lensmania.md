# Schéma de base de données — Lensmania

> Schéma relationnel **reflétant l'implémentation réelle** (entités EF Core + `AppDbContext` + migrations).
> Source de vérité : `LensmaniaServer/Models/`, `LensmaniaServer/Database/AppDbContext.cs`, `LensmaniaServer/Migrations/`.
>


```mermaid
erDiagram
    User ||--o{ Post : "creates"
    User ||--o{ Event : "organizes"
    User ||--o{ Earn : "is awarded"
    User ||--o{ PostLike : "likes"
    User ||--o{ PasswordResetToken : "requests"

    Theme ||--o{ Event : "themes"
    Event ||--o{ Post : "contains (optional)"
    Event ||--o{ Badge : "rewards"

    Badge ||--o{ Earn : "granted via"
    Post  ||--o{ PostLike : "liked in"

    Post |o--o{ Event : "is cover photo of"
    Post |o--o{ Event : "is winner of"

    User {
        int      Id PK
        string   Username "required, unique, max 50"
        string   Email "required, unique"
        string   PasswordHash "nullable (null for Google accounts)"
        string   GoogleId "nullable, unique (filtered)"
        enum     AuthProvider "Local | Google"
        bool     IsAdmin "replaces the old Role table"
        bool     IsPremium
        bool     IsActive "default true"
        datetime CreatedAt
    }

    Post {
        int      Id PK
        string   Title "nullable, max 150"
        string   PhotoUrl "required, max 400"
        string   Description "nullable, max 300"
        datetime CreatedAt
        int      LikesCount
        int      UserId FK "-> User, Restrict, indexed"
        int      EventId FK "nullable, -> Event, SetNull, indexed"
    }

    Event {
        int      Id PK
        string   Name "required, max 200"
        string   Description "max 600"
        datetime StartDate
        datetime EndDate "CK_Events_DateRange: EndDate >= StartDate"
        bool     IsPremium
        int      ThemeId FK "-> Theme, Restrict"
        int      UserId FK "-> User (organizer), Restrict"
        int      CoverPhotoPostId FK "nullable, -> Post, SetNull"
        datetime ClosedAt "nullable (set when the event is auto-closed)"
        int      WinnerPostId FK "nullable, -> Post, SetNull"
    }

    Theme {
        int      Id PK
        string   Name "required, max 100"
        string   Icon "nullable, max 400"
    }

    Badge {
        int      Id PK
        string   Name "required, max 100"
        string   ImageUrl "max 400"
        int      EventId FK "-> Event, Cascade (mandatory)"
    }

    Earn {
        int      UserId PK, FK "-> User, Cascade"
        int      BadgeId PK, FK "-> Badge, Cascade"
        datetime AwardedAt
    }

    PostLike {
        int      UserId PK, FK "-> User, Cascade"
        int      PostId PK, FK "-> Post, Cascade"
    }

    PasswordResetToken {
        int      Id PK
        int      UserId FK "-> User, Cascade"
        string   TokenHash "required, unique, max 64"
        datetime ExpiresAt
        datetime ConsumedAt "nullable"
        datetime CreatedAt
        bytes    RowVersion "nullable, concurrency token"
    }
```

## Notes de modélisation

- **Authentification** : `User.PasswordHash` est nullable car un compte créé via Google n'a pas de mot de passe local. `AuthProvider` distingue `Local` / `Google` ; `GoogleId` porte un index unique filtré (`WHERE "GoogleId" IS NOT NULL`).
- **Rôles** : il n'y a pas de table `Role`. Le statut administrateur est porté par le booléen `User.IsAdmin` (policy `AdminOnly` côté API).
- **Événements (concours)** :
  - `UserId` = organisateur de l'événement.
  - `CoverPhotoPostId` = photo de couverture (un `Post`, optionnel).
  - `ClosedAt` + `WinnerPostId` sont renseignés automatiquement par `EventClosingBackgroundService` à la clôture.
  - Contrainte CHECK `CK_Events_DateRange` : `EndDate >= StartDate`.
- **Likes** : table d'association `PostLike` à clé composite `(UserId, PostId)`. `Post.LikesCount` est un compteur dénormalisé.
- **Badges / récompenses** : un `Badge` appartient obligatoirement à un `Event` (suppression en cascade). `Earn` associe un `User` à un `Badge` avec la date d'attribution `(UserId, BadgeId)`.
- **Réinitialisation de mot de passe** : `PasswordResetToken` stocke le hash du jeton (jamais le jeton en clair), avec expiration, consommation à usage unique et jeton de concurrence `RowVersion`. Index `(UserId, ConsumedAt)`.

## Écarts avec les anciens diagrammes (`MCD_Lensmania.jpg` / `MLD_Lensmania.jpg`)

Les anciennes images contenaient des entités **prévisionnelles non implémentées**, retirées ici : `Role`, `Country`, `Subscription`, `Payment` et la relation d'abonnement `follow`. Principales corrections par rapport à elles :

- `User` : suppression de `avatar`, `role_id`, `country_code`, `event_id` ; ajout de `GoogleId`, `AuthProvider`, `IsAdmin` ; `password` → `PasswordHash` (nullable).
- `Post` : `photo` → `PhotoUrl` ; suppression de `country_code`.
- `Event` : ajout de `Description`, `UserId`, `CoverPhotoPostId`, `ClosedAt`, `WinnerPostId` + contrainte de dates.
- `Badge` : ajout de `Name` ; `image` → `ImageUrl` ; lien à `Event` rendu obligatoire.
- `PostLike` : pas de colonne `liked_at` (contrairement à l'ancien diagramme).
- Ajout de la table `PasswordResetToken`, absente des anciens schémas.
