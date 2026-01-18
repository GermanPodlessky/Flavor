# Deployment

## System Requirements

| Requirement | Details |
|-------------|---------|
| Browser | ~150MB (auto-downloaded on first run) |
| Memory | ~100-200MB per browser instance |
| Linux | Requires system libraries |

## Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    chromium \
    fonts-liberation \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcups2 \
    libdrm2 \
    libgbm1 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    && rm -rf /var/lib/apt/lists/*

ENV FLAVOR_BROWSER_PATH=/usr/bin/chromium

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

## Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pdf-service
spec:
  replicas: 2
  template:
    spec:
      containers:
        - name: pdf-service
          image: your-image:latest
          resources:
            requests:
              memory: "512Mi"
              cpu: "250m"
            limits:
              memory: "1Gi"
              cpu: "1000m"
          env:
            - name: Flavor__PoolSize
              value: "2"
```

**Tips:**
- Use `PoolSize: 1-2` per pod
- Scale horizontally (more pods) instead of larger pools
- Enable warmup to reduce cold start

## Azure

### App Service

```csharp
builder.Services.AddFlavor(options =>
{
    options.BrowserArgs = ["--no-sandbox", "--disable-dev-shm-usage"];
    options.PoolSize = 1;
});
```

### Container Apps / AKS

Use the Docker image. Container Apps work well:

```bash
az containerapp create \
  --name pdf-service \
  --resource-group mygroup \
  --image your-acr.azurecr.io/pdf-service:latest \
  --cpu 1 --memory 2Gi \
  --min-replicas 1 --max-replicas 5
```

### Azure Functions

250MB deployment limit. Options:

1. **Premium Plan** — no size limit (recommended)
2. **Container Functions** — run as Docker container
3. **Separate service** — call Container Apps from Function

```csharp
[Function("GeneratePdf")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
    [FromServices] IFlavorConverter converter)
{
    var html = await req.ReadAsStringAsync();
    var pdf = await converter.ConvertHtmlAsync(html);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "application/pdf");
    await response.Body.WriteAsync(pdf.ToBytes());
    return response;
}
```

## AWS

### Lambda with Chrome Layer

```yaml
# serverless.yml
functions:
  generatePdf:
    handler: PdfFunction::Handler
    memorySize: 1024
    timeout: 30
    layers:
      - arn:aws:lambda:us-east-1:764866452798:layer:chrome-aws-lambda:31
```

```csharp
builder.Services.AddFlavor(options =>
{
    options.BrowserExecutablePath = "/opt/chrome/chrome";
    options.BrowserArgs = [
        "--no-sandbox",
        "--disable-dev-shm-usage",
        "--disable-gpu",
        "--single-process"
    ];
});
```

### Fargate (Recommended)

Better for PDF generation — no size limits, consistent performance:

```json
{
  "containerDefinitions": [{
    "name": "pdf-service",
    "image": "your-ecr/pdf-service:latest",
    "memory": 1024,
    "cpu": 512
  }]
}
```

### Dedicated PDF Microservice

```
[Lambda] --HTTP--> [Fargate PDF Service] ---> PDF
```

Best of both worlds: Lambda for API, Fargate for heavy PDF work.

## Resource Recommendations

| Environment | Pool Size | Memory |
|-------------|-----------|--------|
| Development | 1 | 512MB |
| Production (low traffic) | 1-2 | 512MB-1GB |
| Production (high traffic) | 2-4 | 1-2GB |
| Memory-constrained | 1 | 256MB min |
