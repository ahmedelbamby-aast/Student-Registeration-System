# Telemetry Contract

This document defines the SPEC-018 governed telemetry schemas, not domain or EF entities. The contracts keep operational signals stable, bounded, and PII-minimized while leaving implementation choices outside the shared Contracts project. All timestamps are generated from server UTC.

Exporter selection and endpoint configuration are deployment concerns. When an exporter is unavailable, the application may use a bounded fallback that preserves service availability without allowing an unbounded local queue.

Telemetry MUST NOT contain credentials, full student profiles, raw request or response bodies, query strings, personal identifiers, database connection details, or infrastructure topology. Attribute names are allow-listed by each schema; unexpected attributes are discarded before export. Demo telemetry is retained only as long as needed for diagnosis and is purged within seven days.

## Runtime metric series

The application accepts only these aggregate series: request latency and
throughput, unexpected errors, business rejection codes, optimizer duration,
SQL duration and lock waits, deadlocks, capacity conflicts, and counter
reconciliation mismatches. Runtime constants in
`OperationalMetricNames` are the executable source of their stable names.

Metric dimensions are limited to `code`, `method`, `module`, `operation`,
`outcome`, `route`, and `statusClass`. Values are controlled short codes, not
request values: every value also belongs to a closed server-owned allow-list.
Unknown names, dimensions, or values are rejected before emission and the
in-process fallback retains at most 256 unique series. Reaching that limit
drops the new operational signal without failing the business request. Client
correlation headers are never reused; the server creates a new opaque
correlation identifier for each request.

Readiness starts fail-closed. SQL availability must be reported positively
before health can become ready; a missing optional telemetry exporter yields a
degraded summary after SQL is healthy. Neither dependency name or state is
returned in the public response.

## Structured log schema

<!-- STRUCTURED_LOG_SCHEMA_START -->
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://student-registration.demo/schemas/structured-log/1.0",
  "title": "StructuredLog",
  "x-owner-spec": "SPEC-018",
  "x-schema-version": "1.0",
  "x-immutable-after-signoff": true,
  "type": "object",
  "additionalProperties": false,
  "required": [
    "timestampUtc",
    "level",
    "eventCode",
    "messageTemplate",
    "correlationId",
    "attributes"
  ],
  "properties": {
    "timestampUtc": {
      "type": "string",
      "format": "date-time"
    },
    "level": {
      "type": "string",
      "enum": ["trace", "debug", "information", "warning", "error", "critical"]
    },
    "eventCode": {
      "type": "string",
      "minLength": 1,
      "maxLength": 100,
      "pattern": "^[A-Z][A-Z0-9_]*$"
    },
    "messageTemplate": {
      "type": "string",
      "minLength": 1,
      "maxLength": 512
    },
    "correlationId": {
      "type": "string",
      "minLength": 1,
      "maxLength": 128
    },
    "traceId": {
      "type": "string",
      "pattern": "^[0-9a-f]{32}$"
    },
    "attributes": {
      "type": "object",
      "additionalProperties": false,
      "maxProperties": 8,
      "properties": {
        "module": { "type": "string", "maxLength": 64 },
        "operation": { "type": "string", "maxLength": 128 },
        "routeTemplate": { "type": "string", "maxLength": 160 },
        "httpMethod": { "type": "string", "maxLength": 12 },
        "statusCode": { "type": "integer", "minimum": 100, "maximum": 599 },
        "durationMs": { "type": "number", "minimum": 0 },
        "outcomeCode": { "type": "string", "maxLength": 64 },
        "attempt": { "type": "integer", "minimum": 1, "maximum": 20 }
      }
    }
  },
  "examples": [
    {
      "timestampUtc": "2026-07-14T01:30:00Z",
      "level": "warning",
      "eventCode": "SPEC018_HEALTH_DEGRADED",
      "messageTemplate": "Health status changed to {HealthStatus}",
      "correlationId": "01J2Y4PX9V7W6M5N4K3H2G1F0E",
      "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
      "attributes": {
        "module": "operations",
        "operation": "health-summary",
        "outcomeCode": "degraded"
      }
    }
  ]
}
```
<!-- STRUCTURED_LOG_SCHEMA_END -->

## Trace schema

<!-- TRACE_SCHEMA_START -->
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://student-registration.demo/schemas/trace/1.0",
  "title": "Trace",
  "x-owner-spec": "SPEC-018",
  "x-schema-version": "1.0",
  "x-immutable-after-signoff": true,
  "type": "object",
  "additionalProperties": false,
  "required": [
    "traceId",
    "spanId",
    "correlationId",
    "operationName",
    "startedAtUtc",
    "durationMs",
    "status",
    "attributes"
  ],
  "properties": {
    "traceId": {
      "type": "string",
      "pattern": "^[0-9a-f]{32}$"
    },
    "spanId": {
      "type": "string",
      "pattern": "^[0-9a-f]{16}$"
    },
    "parentSpanId": {
      "type": "string",
      "pattern": "^[0-9a-f]{16}$"
    },
    "correlationId": {
      "type": "string",
      "minLength": 1,
      "maxLength": 128
    },
    "operationName": {
      "type": "string",
      "minLength": 1,
      "maxLength": 128,
      "pattern": "^[a-z][a-z0-9._-]*$"
    },
    "startedAtUtc": {
      "type": "string",
      "format": "date-time"
    },
    "durationMs": {
      "type": "number",
      "minimum": 0
    },
    "status": {
      "type": "string",
      "enum": ["unset", "ok", "error"]
    },
    "attributes": {
      "type": "object",
      "additionalProperties": false,
      "maxProperties": 8,
      "properties": {
        "module": { "type": "string", "maxLength": 64 },
        "routeTemplate": { "type": "string", "maxLength": 160 },
        "httpMethod": { "type": "string", "maxLength": 12 },
        "statusCode": { "type": "integer", "minimum": 100, "maximum": 599 },
        "outcomeCode": { "type": "string", "maxLength": 64 },
        "databaseOperation": { "type": "string", "maxLength": 64 },
        "retryCount": { "type": "integer", "minimum": 0, "maximum": 20 },
        "replicaRole": { "type": "string", "enum": ["primary", "secondary", "unknown"] }
      }
    }
  },
  "examples": [
    {
      "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
      "spanId": "00f067aa0ba902b7",
      "correlationId": "01J2Y4PX9V7W6M5N4K3H2G1F0E",
      "operationName": "registration.submit",
      "startedAtUtc": "2026-07-14T01:45:00Z",
      "durationMs": 142.75,
      "status": "ok",
      "attributes": {
        "module": "registration",
        "routeTemplate": "/api/registrations/submit",
        "httpMethod": "POST",
        "statusCode": 200,
        "outcomeCode": "accepted"
      }
    }
  ]
}
```
<!-- TRACE_SCHEMA_END -->
