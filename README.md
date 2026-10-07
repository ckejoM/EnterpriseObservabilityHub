# ObservabilityHub

A small playground for observability in .NET: two services call each other, and I can watch the traces, metrics, and logs that come out.

## What's in it
- **OrderApi:** `POST /api/orders` calls InventoryApi over HTTP and adds a small random delay.
- **InventoryApi:** `GET /api/inventory/{productId}` adds random latency and fails about 10% of the time, so there's something interesting to look at.
- Both services:
  - export **traces and metrics** over OTLP with OpenTelemetry (ASP.NET Core, HttpClient, and runtime instrumentation)
  - write **structured logs** with Serilog to the console and Elasticsearch
  - expose **health checks** at `/health/live` and `/health/ready`

## Observability stack (Docker Compose)
| Service | Port | Role |
| --- | --- | --- |
| OpenTelemetry Collector | 4317 / 4318, 8889 | Receives OTLP and exposes metrics for Prometheus |
| Prometheus | 9090 | Scrapes the collector every 5 s |
| Grafana | 3000 | Dashboards (anonymous admin, local only) |
| Elasticsearch | 9200 | Stores Serilog logs |
| Kibana | 5601 | Searches the logs |

## Stack
.NET 9 · ASP.NET Core Minimal APIs · OpenTelemetry · OTLP · OpenTelemetry Collector · Prometheus · Grafana · Serilog · Elasticsearch · Kibana · Docker Compose

## Run it locally
1. Start the stack: `docker compose up -d`
2. Run both APIs: `dotnet run --project InventoryApi` and `dotnet run --project OrderApi`. OrderApi expects InventoryApi at `http://localhost:5001`; you can override that with `InventoryApiUrl`.
3. Send a few requests (see `OrderApi/OrderApi.http`), then open:
   - Prometheus at http://localhost:9090
   - Grafana at http://localhost:3000 (add Prometheus `http://prometheus:9090` as a data source)
   - Kibana at http://localhost:5601 (index pattern `applogs-*`)

## Current limitations and what's next
- **Traces aren't visualized yet.** The collector only has a metrics pipeline, so exported traces are dropped. Next: add a traces pipeline to Jaeger or Tempo.
- **Grafana isn't provisioned.** The data source and dashboards are set up by hand. Next: provisioning files and a starter dashboard.
- **Blocking delays.** The simulated delays use `Thread.Sleep`. Next: `Task.Delay`.
- **Kubernetes.** Next: manifests to run the services and collector on a local cluster (kind or minikube) with readiness and liveness probes wired to the health endpoints.

---

Built by Jovan Madzic, Software Engineer in Belgrade · [LinkedIn](https://www.linkedin.com/in/jovan-madzic-12093b202/) · [GitHub](https://github.com/ckejoM)
