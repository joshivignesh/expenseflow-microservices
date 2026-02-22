# ADR-002: Use RabbitMQ for Cross-Service Communication (Not HTTP)

**Date:** 2026-02-22  
**Status:** Accepted  
**Author:** Vignesh Joshi

---

## Context

When the Expense service approves an expense, the Notification service needs to send an email. There are two ways to trigger this:
1. **Synchronous HTTP** — Expense service calls Notification service directly via REST
2. **Asynchronous Messaging** — Expense service publishes an event to RabbitMQ; Notification service consumes it

## Decision

Use **RabbitMQ via MassTransit** for all cross-service communication that doesn't require an immediate response.

## Consequences

**Positive:**
- **Fault isolation** — if Notification service is down, Expense service still works. Message is queued and delivered when Notification recovers
- **Temporal decoupling** — services don't need to be available at the same time
- **No cascading failures** — a slow Notification service can't make Expense service slow
- **Retry built-in** — RabbitMQ retries failed deliveries automatically

**Negative:**
- **Eventual consistency** — email arrives slightly after approval (acceptable for this use case)
- **Debugging complexity** — need RabbitMQ management UI to inspect queues
- **Infrastructure dependency** — requires running RabbitMQ (handled via Docker Compose)

## When We Still Use HTTP

The API Gateway → Services communication uses HTTP because:
- Client needs an immediate response
- Gateway validates JWT synchronously
- Polly handles retry + circuit breaking on these calls

## References
- Microsoft Microservices Guide — Chapter 6: Asynchronous message-based communication
- Microsoft Cloud-Native Guide — Chapter 4: Event-driven patterns
