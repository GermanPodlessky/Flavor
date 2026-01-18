# Benchmarks

Environment: .NET 8.0, Windows 11, Intel Core i7, 16GB RAM

## Single Document Generation

| Scenario | Mean | Allocated |
|----------|-----:|----------:|
| Simple HTML | 120 ms | 554 KB |
| HTML with CSS (Grid, Table) | 181 ms | 1,091 KB |
| HTML with Images | 123 ms | 631 KB |
| Large Document (100 rows) | 183 ms | 2,111 KB |
| With Header/Footer | 200 ms | 1,101 KB |

## Concurrent Generation

| Pool Size | Requests | Mean | Throughput |
|----------:|---------:|-----:|-----------:|
| 1 | 4 | 524 ms | 7.6 PDF/sec |
| 1 | 8 | 1,053 ms | 7.6 PDF/sec |
| 2 | 4 | 319 ms | 12.5 PDF/sec |
| 2 | 8 | 636 ms | 12.6 PDF/sec |
| 4 | 4 | 222 ms | 18.0 PDF/sec |
| 4 | 8 | 445 ms | 18.0 PDF/sec |

## Cold Start vs Warm

| Scenario | Time |
|----------|-----:|
| Cold start (first PDF) | ~2.5 sec |
| Warm (subsequent) | ~120-200 ms |

Use `AddFlavorWarmup()` to reduce first-request latency.

## Memory Usage

| Pool Size | Memory |
|-----------|--------|
| 1 | ~150-200 MB |
| 2 | ~250-350 MB |
| 4 | ~450-600 MB |

## Run Benchmarks

```bash
dotnet run -c Release --project tests/Flavor.Benchmarks
```
