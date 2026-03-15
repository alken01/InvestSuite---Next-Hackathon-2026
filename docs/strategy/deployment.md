# Deployment Guide (FREE Tier Only)

Priority: **$0 cost**.

## Architecture

```
[Vercel - FREE]              [Render - FREE]
Next.js Frontend    →→→→     ASP.NET Core 9 API  →→→  Claude API
(Mantine + CSS Modules)      (JSON data, no DB)
```

---

## 1. Frontend — Vercel (FREE)

### Setup
```bash
npm i -g vercel
vercel login
vercel          # preview
vercel --prod   # production
```

Or connect GitHub repo at https://vercel.com/new — auto-deploys on push.

### next.config.js rewrites (proxy to backend)
```js
/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/backend/:path*',
        destination: `${process.env.BACKEND_URL || 'http://localhost:5144'}/:path*`,
      },
    ];
  },
};
module.exports = nextConfig;
```

Using rewrites means no CORS issues (browser thinks it's same-origin).

### Environment Variables (Vercel dashboard)
```
BACKEND_URL=https://your-dotnet-app.onrender.com
```

### Free Tier Limits
| Resource | Limit |
|----------|-------|
| Deployments/day | 100 |
| Bandwidth | 100 GB/month |
| Serverless function timeout | 60 seconds |

---

## 2. Backend — Render (FREE)

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln ./
COPY src/Api/*.csproj src/Api/
COPY src/Core/*.csproj src/Core/
COPY src/Infrastructure/*.csproj src/Infrastructure/
RUN dotnet restore
COPY . .
RUN dotnet publish src/Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
EXPOSE 10000
ENTRYPOINT ["dotnet", "Api.dll"]
```

### render.yaml
```yaml
services:
  - type: web
    runtime: docker
    name: investsuite-api
    plan: free
    dockerfilePath: ./Dockerfile
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: CLAUDE_API_KEY
        sync: false
    healthCheckPath: /health
```

### Free Tier Limits
| Resource | Limit |
|----------|-------|
| Compute hours | 750/month |
| Spin-down after inactivity | 15 minutes |
| Cold start time | ~60 seconds |

**Demo tip:** Hit the endpoint 2 minutes before your pitch to wake it up.

---

## Local Development

```bash
# Terminal 1: Frontend
cd frontend && npm run dev

# Terminal 2: Backend
cd backend && dotnet run --project src/Api
```

## Cost Summary

| Service | Platform | Cost |
|---------|----------|------|
| Frontend (Next.js) | Vercel | FREE |
| Backend (ASP.NET Core) | Render | FREE |
| **Total** | | **$0** |
