# API Gateway — Routing Reference

All external traffic enters at `http://localhost:5000` (Gateway).  
The Gateway validates the JWT, applies rate limiting, then proxies to the correct service.

## Route Map

| External URL (via Gateway :5000)         | Proxied to                        | Auth         | Rate Limit |
|------------------------------------------|-----------------------------------|--------------|------------|
| `POST /identity/auth/register`           | Identity `:8080/api/auth/register`| Anonymous    | 10 req/min |
| `POST /identity/auth/login`              | Identity `:8080/api/auth/login`   | Anonymous    | 10 req/min |
| `GET  /identity/auth/profile/{id}`       | Identity `:8080/api/auth/profile/{id}` | JWT   | 100 req/min|
| `POST /expenses`                         | Expense  `:8080/api/expenses`     | JWT          | 100 req/min|
| `POST /expenses/{id}/submit`             | Expense  `:8080/api/expenses/{id}/submit` | JWT  | 100 req/min|
| `POST /expenses/{id}/approve`            | Expense  `:8080/api/expenses/{id}/approve`| JWT  | 100 req/min|
| `POST /expenses/{id}/reject`             | Expense  `:8080/api/expenses/{id}/reject` | JWT  | 100 req/min|
| `GET  /expenses/{id}`                    | Expense  `:8080/api/expenses/{id}`| JWT          | 100 req/min|
| `GET  /expenses/my`                      | Expense  `:8080/api/expenses/my`  | JWT          | 100 req/min|
| `GET  /expenses/pending`                 | Expense  `:8080/api/expenses/pending` | JWT+Role | 100 req/min|

## Correlation ID

Every request through the Gateway gets an `X-Correlation-ID` header.  
If the client sends one, it is preserved and forwarded.  
If not, the Gateway generates a new UUID.  
The same ID is returned in the response so client logs can be correlated with server logs.

## Health Checks

YARP actively polls `/health` on each downstream service every 10 seconds.  
If a service fails 3 consecutive checks, YARP stops routing to it automatically.
