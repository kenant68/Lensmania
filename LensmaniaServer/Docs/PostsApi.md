# Posts API

## Base Route

- `/api/posts`

## Endpoints

### GET `/api/posts`

Returns a paginated list of posts, sorted from latest `createdAt` to oldest, using cursor-based pagination.

---

## Query Parameters

| Parameter | Type  | Required | Default | Description                                           |
|-----------|-------|----------|---------|-------------------------------------------------------|
| `limit`   | `int` | No       | `10`    | Number of posts to return per page                    |
| `cursor`  | `int` | No       | `null`  | Id of the last post received, used to load more posts |

---

## Examples

### First page (without cursor)

```http
GET /api/posts?limit=10
```

### Next page (with cursor)

```http
GET /api/posts?limit=10&cursor=32
```

---

## Response

### Success — `200 OK`

```json
{
  "posts": [
    {
      "title": "Mon post",
      "photoUrl": "https://example.com/photo.jpg",
      "description": "Une photo test",
      "createdAt": "2026-03-29T12:30:00Z"
    }
  ],
  "hasMore": true,
  "nextCursor": 32
}
```

| Field        | Type                 | Description                                 |
|--------------|----------------------|---------------------------------------------|
| `posts`      | `List<PostResponse>` | List of current page's posts                |
| `hasMore`    | `bool`               | `true` if there are more posts to load      |
| `nextCursor` | `int \| null`        | Id to pass as `cursor` for the next request |

---

## HTTP response codes

| Code | Description           |
|------|-----------------------|
| 200  | Success               |
| 500  | Internal server error |

---

## How pagination works

This endpoint uses a **cursor-based pagination** rather than an offset pagination, better suited for infinite scrolling.

The client uses the `nextCursor` field from the previous response to load the next batch. This prevents duplicate posts or skipped posts if new posts are added between requests.

```http
Request 1 : GET /api/posts?limit=10
→ gets posts 100..91, nextCursor = 91

Request 2 : GET /api/posts?limit=10&cursor=91
→ gets posts 90..81, nextCursor = 81

... until hasMore = false
```

---
