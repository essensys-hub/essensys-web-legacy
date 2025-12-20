# Infrastructure et Déploiement - Essensys Migration

## Vue d'Ensemble

Cette documentation détaille l'architecture d'infrastructure et de déploiement pour la migration d'Essensys, incluant la containerisation Docker, la configuration des environnements, les stratégies de monitoring/logging et les pipelines CI/CD.

## Architecture de Déploiement avec Docker

### Structure Docker Multi-Services

```
essensys-infrastructure/
├── docker/
│   ├── frontend/
│   │   ├── Dockerfile
│   │   ├── nginx.conf
│   │   └── .dockerignore
│   ├── backend/
│   │   ├── Dockerfile
│   │   ├── Dockerfile.dev
│   │   └── .dockerignore
│   ├── database/
│   │   ├── Dockerfile
│   │   ├── init/
│   │   │   ├── 01-init-db.sql
│   │   │   ├── 02-create-extensions.sql
│   │   │   └── 03-seed-data.sql
│   │   └── config/
│   │       └── postgresql.conf
│   ├── redis/
│   │   ├── Dockerfile
│   │   └── redis.conf
│   └── nginx/
│       ├── Dockerfile
│       ├── nginx.conf
│       ├── ssl/
│       └── templates/
├── docker-compose.yml
├── docker-compose.dev.yml
├── docker-compose.prod.yml
├── docker-compose.test.yml
└── .env.example
```

### Dockerfile Frontend (React)

```dockerfile
# docker/frontend/Dockerfile
# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Copier les fichiers de dépendances
COPY package*.json ./
COPY tsconfig.json ./
COPY vite.config.ts ./

# Installer les dépendances
RUN npm ci --only=production

# Copier le code source
COPY src/ ./src/
COPY public/ ./public/
COPY index.html ./

# Arguments de build
ARG VITE_API_URL
ARG VITE_WS_URL
ARG VITE_APP_VERSION
ARG NODE_ENV=production

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_WS_URL=$VITE_WS_URL
ENV VITE_APP_VERSION=$VITE_APP_VERSION
ENV NODE_ENV=$NODE_ENV

# Build de l'application
RUN npm run build

# Production stage
FROM nginx:1.25-alpine

# Installer des outils de debugging (optionnel)
RUN apk add --no-cache curl

# Copier la configuration Nginx
COPY docker/frontend/nginx.conf /etc/nginx/nginx.conf

# Copier les fichiers buildés
COPY --from=builder /app/dist /usr/share/nginx/html

# Copier le script d'entrée pour la configuration dynamique
COPY docker/frontend/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Exposer le port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
```

### Configuration Nginx Frontend

```nginx
# docker/frontend/nginx.conf
events {
    worker_connections 1024;
}

http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;

    # Logging
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                   '$status $body_bytes_sent "$http_referer" '
                   '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;
    error_log /var/log/nginx/error.log warn;

    # Performance
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/atom+xml
        image/svg+xml;

    # Security headers
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";
    add_header Referrer-Policy "strict-origin-when-cross-origin";

    server {
        listen 80;
        server_name _;
        root /usr/share/nginx/html;
        index index.html;

        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # API proxy
        location /api/ {
            proxy_pass http://backend:8000/api/;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;
            
            # Timeouts
            proxy_connect_timeout 60s;
            proxy_send_timeout 60s;
            proxy_read_timeout 60s;
        }

        # WebSocket proxy
        location /ws/ {
            proxy_pass http://backend:8000/ws/;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Health check endpoint
        location /health {
            access_log off;
            return 200 "healthy\n";
            add_header Content-Type text/plain;
        }

        # SPA fallback
        location / {
            try_files $uri $uri/ /index.html;
        }

        # Security
        location ~ /\. {
            deny all;
        }
    }
}
```

### Dockerfile Backend (Node.js)

```dockerfile
# docker/backend/Dockerfile
FROM node:18-alpine AS base

# Installer les dépendances système
RUN apk add --no-cache \
    dumb-init \
    curl \
    && rm -rf /var/cache/apk/*

# Créer un utilisateur non-root
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nodejs -u 1001

WORKDIR /app

# Copier les fichiers de dépendances
COPY package*.json ./
COPY tsconfig.json ./

# Development stage
FROM base AS development

ENV NODE_ENV=development

# Installer toutes les dépendances (dev + prod)
RUN npm ci

# Copier le code source
COPY . .

# Changer le propriétaire
RUN chown -R nodejs:nodejs /app
USER nodejs

EXPOSE 8000

CMD ["npm", "run", "dev"]

# Build stage
FROM base AS builder

ENV NODE_ENV=production

# Installer toutes les dépendances pour le build
RUN npm ci

# Copier le code source
COPY . .

# Build de l'application
RUN npm run build

# Production stage
FROM base AS production

ENV NODE_ENV=production

# Installer seulement les dépendances de production
RUN npm ci --only=production && npm cache clean --force

# Copier les fichiers buildés
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/package*.json ./

# Copier les fichiers de configuration
COPY prisma/ ./prisma/
COPY docker/backend/docker-entrypoint.sh ./docker-entrypoint.sh

# Permissions
RUN chown -R nodejs:nodejs /app
RUN chmod +x ./docker-entrypoint.sh

USER nodejs

EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8000/api/health || exit 1

ENTRYPOINT ["dumb-init", "--"]
CMD ["./docker-entrypoint.sh"]
```

### Script d'Entrée Backend

```bash
#!/bin/sh
# docker/backend/docker-entrypoint.sh

set -e

echo "🚀 Starting Essensys Backend..."

# Attendre que la base de données soit prête
echo "⏳ Waiting for database..."
until npx prisma db push --accept-data-loss 2>/dev/null; do
  echo "Database is unavailable - sleeping"
  sleep 2
done

echo "✅ Database is ready!"

# Exécuter les migrations Prisma
echo "🔄 Running database migrations..."
npx prisma migrate deploy

# Générer le client Prisma
echo "🔧 Generating Prisma client..."
npx prisma generate

# Seed de la base de données si nécessaire
if [ "$SEED_DATABASE" = "true" ]; then
    echo "🌱 Seeding database..."
    npx prisma db seed
fi

echo "🎯 Starting application..."
exec node dist/server.js
```

### Docker Compose - Développement

```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  # Base de données PostgreSQL
  database:
    image: postgres:15-alpine
    container_name: essensys-db-dev
    environment:
      POSTGRES_DB: essensys_dev
      POSTGRES_USER: essensys
      POSTGRES_PASSWORD: essensys_dev_password
      POSTGRES_INITDB_ARGS: "--encoding=UTF-8 --lc-collate=fr_FR.UTF-8 --lc-ctype=fr_FR.UTF-8"
    volumes:
      - postgres_data_dev:/var/lib/postgresql/data
      - ./docker/database/init:/docker-entrypoint-initdb.d
      - ./docker/database/config/postgresql.conf:/etc/postgresql/postgresql.conf
    ports:
      - "5432:5432"
    networks:
      - essensys-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U essensys -d essensys_dev"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis pour cache et sessions
  redis:
    image: redis:7-alpine
    container_name: essensys-redis-dev
    command: redis-server /usr/local/etc/redis/redis.conf
    volumes:
      - redis_data_dev:/data
      - ./docker/redis/redis.conf:/usr/local/etc/redis/redis.conf
    ports:
      - "6379:6379"
    networks:
      - essensys-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 3

  # Backend Node.js
  backend:
    build:
      context: .
      dockerfile: docker/backend/Dockerfile
      target: development
    container_name: essensys-backend-dev
    environment:
      NODE_ENV: development
      DATABASE_URL: postgresql://essensys:essensys_dev_password@database:5432/essensys_dev
      REDIS_URL: redis://redis:6379
      JWT_SECRET: dev_jwt_secret_change_in_production
      MACHINE_JWT_SECRET: dev_machine_jwt_secret_change_in_production
      SEED_DATABASE: "true"
    volumes:
      - .:/app
      - /app/node_modules
      - backend_logs_dev:/app/logs
    ports:
      - "8000:8000"
      - "9229:9229" # Debug port
    depends_on:
      database:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - essensys-network
    command: npm run dev:debug

  # Frontend React
  frontend:
    build:
      context: .
      dockerfile: docker/frontend/Dockerfile
      target: development
    container_name: essensys-frontend-dev
    environment:
      VITE_API_URL: http://localhost:8000/api
      VITE_WS_URL: ws://localhost:8000/ws
      VITE_APP_VERSION: dev
    volumes:
      - ./frontend:/app
      - /app/node_modules
    ports:
      - "3000:3000"
    networks:
      - essensys-network
    command: npm run dev

  # Nginx reverse proxy
  nginx:
    image: nginx:1.25-alpine
    container_name: essensys-nginx-dev
    volumes:
      - ./docker/nginx/dev.conf:/etc/nginx/nginx.conf
    ports:
      - "80:80"
    depends_on:
      - backend
      - frontend
    networks:
      - essensys-network

volumes:
  postgres_data_dev:
  redis_data_dev:
  backend_logs_dev:

networks:
  essensys-network:
    driver: bridge
```

### Docker Compose - Production

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  # Base de données PostgreSQL
  database:
    image: postgres:15-alpine
    container_name: essensys-db-prod
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./docker/database/init:/docker-entrypoint-initdb.d
      - ./docker/database/config/postgresql.prod.conf:/etc/postgresql/postgresql.conf
    networks:
      - essensys-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 30s
      timeout: 10s
      retries: 3
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  # Redis
  redis:
    image: redis:7-alpine
    container_name: essensys-redis-prod
    command: redis-server /usr/local/etc/redis/redis.conf --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
      - ./docker/redis/redis.prod.conf:/usr/local/etc/redis/redis.conf
    networks:
      - essensys-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Backend
  backend:
    build:
      context: .
      dockerfile: docker/backend/Dockerfile
      target: production
      args:
        NODE_ENV: production
    container_name: essensys-backend-prod
    environment:
      NODE_ENV: production
      DATABASE_URL: ${DATABASE_URL}
      REDIS_URL: redis://:${REDIS_PASSWORD}@redis:6379
      JWT_SECRET: ${JWT_SECRET}
      MACHINE_JWT_SECRET: ${MACHINE_JWT_SECRET}
      SMTP_HOST: ${SMTP_HOST}
      SMTP_PORT: ${SMTP_PORT}
      SMTP_USER: ${SMTP_USER}
      SMTP_PASSWORD: ${SMTP_PASSWORD}
      SMS_API_KEY: ${SMS_API_KEY}
    volumes:
      - backend_logs:/app/logs
      - backend_uploads:/app/uploads
    depends_on:
      database:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - essensys-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M

  # Frontend
  frontend:
    build:
      context: .
      dockerfile: docker/frontend/Dockerfile
      args:
        VITE_API_URL: ${VITE_API_URL}
        VITE_WS_URL: ${VITE_WS_URL}
        VITE_APP_VERSION: ${APP_VERSION}
    container_name: essensys-frontend-prod
    networks:
      - essensys-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 128M
        reservations:
          memory: 64M

  # Nginx Load Balancer
  nginx:
    build:
      context: ./docker/nginx
      dockerfile: Dockerfile
    container_name: essensys-nginx-prod
    volumes:
      - ./docker/nginx/prod.conf:/etc/nginx/nginx.conf
      - ./docker/nginx/ssl:/etc/nginx/ssl
      - nginx_logs:/var/log/nginx
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - backend
      - frontend
    networks:
      - essensys-network
    restart: unless-stopped

  # Monitoring - Prometheus
  prometheus:
    image: prom/prometheus:latest
    container_name: essensys-prometheus
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    ports:
      - "9090:9090"
    networks:
      - essensys-network
    restart: unless-stopped

  # Monitoring - Grafana
  grafana:
    image: grafana/grafana:latest
    container_name: essensys-grafana
    environment:
      GF_SECURITY_ADMIN_PASSWORD: ${GRAFANA_PASSWORD}
    volumes:
      - grafana_data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
    ports:
      - "3001:3000"
    networks:
      - essensys-network
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  backend_logs:
  backend_uploads:
  nginx_logs:
  prometheus_data:
  grafana_data:

networks:
  essensys-network:
    driver: bridge
```

## Configuration des Environnements

### Variables d'Environnement

```bash
# .env.example
# Application
APP_NAME=Essensys
APP_VERSION=1.0.0
NODE_ENV=production

# URLs
FRONTEND_URL=https://app.essensys.com
BACKEND_URL=https://api.essensys.com
VITE_API_URL=https://api.essensys.com/api
VITE_WS_URL=wss://api.essensys.com/ws

# Base de données
DATABASE_URL=postgresql://essensys:secure_password@database:5432/essensys_prod
POSTGRES_DB=essensys_prod
POSTGRES_USER=essensys
POSTGRES_PASSWORD=secure_password

# Redis
REDIS_URL=redis://:redis_password@redis:6379
REDIS_PASSWORD=redis_password

# JWT
JWT_SECRET=your_super_secure_jwt_secret_here_minimum_32_chars
MACHINE_JWT_SECRET=your_machine_jwt_secret_here_minimum_32_chars
JWT_EXPIRES_IN=15m
REFRESH_TOKEN_EXPIRES_IN=30d

# Email (SMTP)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_SECURE=false
SMTP_USER=noreply@essensys.com
SMTP_PASSWORD=smtp_password
EMAIL_FROM=Essensys <noreply@essensys.com>

# SMS
SMS_PROVIDER=twilio
SMS_API_KEY=your_sms_api_key
SMS_API_SECRET=your_sms_api_secret
SMS_FROM=+33123456789

# Monitoring
GRAFANA_PASSWORD=grafana_admin_password

# Sécurité
CORS_ORIGIN=https://app.essensys.com
RATE_LIMIT_WINDOW_MS=900000
RATE_LIMIT_MAX_REQUESTS=100

# Logging
LOG_LEVEL=info
LOG_FORMAT=json

# Uploads
MAX_FILE_SIZE=10485760
UPLOAD_PATH=/app/uploads
```

### Configuration par Environnement

```typescript
// src/config/environments.ts
interface EnvironmentConfig {
  database: {
    url: string;
    ssl: boolean;
    poolSize: number;
  };
  redis: {
    url: string;
    keyPrefix: string;
  };
  jwt: {
    secret: string;
    expiresIn: string;
    refreshExpiresIn: string;
  };
  cors: {
    origin: string | string[];
    credentials: boolean;
  };
  rateLimit: {
    windowMs: number;
    max: number;
  };
  logging: {
    level: string;
    format: string;
  };
}

const environments: Record<string, EnvironmentConfig> = {
  development: {
    database: {
      url: process.env.DATABASE_URL!,
      ssl: false,
      poolSize: 5
    },
    redis: {
      url: process.env.REDIS_URL!,
      keyPrefix: 'essensys:dev:'
    },
    jwt: {
      secret: process.env.JWT_SECRET!,
      expiresIn: '1h',
      refreshExpiresIn: '7d'
    },
    cors: {
      origin: ['http://localhost:3000', 'http://localhost:5173'],
      credentials: true
    },
    rateLimit: {
      windowMs: 15 * 60 * 1000, // 15 minutes
      max: 1000 // requests per windowMs
    },
    logging: {
      level: 'debug',
      format: 'pretty'
    }
  },

  staging: {
    database: {
      url: process.env.DATABASE_URL!,
      ssl: true,
      poolSize: 10
    },
    redis: {
      url: process.env.REDIS_URL!,
      keyPrefix: 'essensys:staging:'
    },
    jwt: {
      secret: process.env.JWT_SECRET!,
      expiresIn: '15m',
      refreshExpiresIn: '30d'
    },
    cors: {
      origin: process.env.CORS_ORIGIN!,
      credentials: true
    },
    rateLimit: {
      windowMs: 15 * 60 * 1000,
      max: 500
    },
    logging: {
      level: 'info',
      format: 'json'
    }
  },

  production: {
    database: {
      url: process.env.DATABASE_URL!,
      ssl: true,
      poolSize: 20
    },
    redis: {
      url: process.env.REDIS_URL!,
      keyPrefix: 'essensys:prod:'
    },
    jwt: {
      secret: process.env.JWT_SECRET!,
      expiresIn: '15m',
      refreshExpiresIn: '30d'
    },
    cors: {
      origin: process.env.CORS_ORIGIN!,
      credentials: true
    },
    rateLimit: {
      windowMs: 15 * 60 * 1000,
      max: 100
    },
    logging: {
      level: 'warn',
      format: 'json'
    }
  }
};

export const config = environments[process.env.NODE_ENV || 'development'];
```

## Stratégie de Monitoring et Logging

### Configuration Winston (Logging)

```typescript
// src/utils/logger.ts
import winston from 'winston';
import { config } from '../config/environments';

const logFormat = winston.format.combine(
  winston.format.timestamp(),
  winston.format.errors({ stack: true }),
  config.logging.format === 'json' 
    ? winston.format.json()
    : winston.format.combine(
        winston.format.colorize(),
        winston.format.simple()
      )
);

export const logger = winston.createLogger({
  level: config.logging.level,
  format: logFormat,
  defaultMeta: { 
    service: 'essensys-backend',
    version: process.env.APP_VERSION 
  },
  transports: [
    // Console output
    new winston.transports.Console({
      format: config.logging.format === 'json' 
        ? winston.format.json()
        : winston.format.combine(
            winston.format.colorize(),
            winston.format.simple()
          )
    }),

    // File output for production
    ...(process.env.NODE_ENV === 'production' ? [
      new winston.transports.File({
        filename: '/app/logs/error.log',
        level: 'error',
        maxsize: 10485760, // 10MB
        maxFiles: 5
      }),
      new winston.transports.File({
        filename: '/app/logs/combined.log',
        maxsize: 10485760,
        maxFiles: 10
      })
    ] : [])
  ],

  // Handle uncaught exceptions
  exceptionHandlers: [
    new winston.transports.File({ 
      filename: '/app/logs/exceptions.log' 
    })
  ],

  // Handle unhandled promise rejections
  rejectionHandlers: [
    new winston.transports.File({ 
      filename: '/app/logs/rejections.log' 
    })
  ]
});

// Structured logging helpers
export const loggers = {
  request: (req: any, res: any, duration: number) => {
    logger.info('HTTP Request', {
      method: req.method,
      url: req.url,
      statusCode: res.statusCode,
      duration,
      ip: req.ip,
      userAgent: req.get('User-Agent'),
      userId: req.user?.id,
      machineId: req.machine?.id
    });
  },

  error: (error: Error, context?: any) => {
    logger.error('Application Error', {
      message: error.message,
      stack: error.stack,
      ...context
    });
  },

  security: (event: string, details: any) => {
    logger.warn('Security Event', {
      event,
      ...details,
      timestamp: new Date().toISOString()
    });
  },

  performance: (operation: string, duration: number, metadata?: any) => {
    logger.info('Performance Metric', {
      operation,
      duration,
      ...metadata
    });
  }
};
```

### Configuration Prometheus (Métriques)

```typescript
// src/middleware/metrics.ts
import promClient from 'prom-client';
import { Request, Response, NextFunction } from 'express';

// Créer un registre pour les métriques
const register = new promClient.Registry();

// Métriques par défaut (CPU, mémoire, etc.)
promClient.collectDefaultMetrics({ register });

// Métriques personnalisées
const httpRequestDuration = new promClient.Histogram({
  name: 'http_request_duration_seconds',
  help: 'Duration of HTTP requests in seconds',
  labelNames: ['method', 'route', 'status_code'],
  buckets: [0.1, 0.5, 1, 2, 5, 10]
});

const httpRequestTotal = new promClient.Counter({
  name: 'http_requests_total',
  help: 'Total number of HTTP requests',
  labelNames: ['method', 'route', 'status_code']
});

const activeConnections = new promClient.Gauge({
  name: 'websocket_connections_active',
  help: 'Number of active WebSocket connections'
});

const deviceActions = new promClient.Counter({
  name: 'device_actions_total',
  help: 'Total number of device actions',
  labelNames: ['action_type', 'status', 'device_type']
});

const databaseQueries = new promClient.Histogram({
  name: 'database_query_duration_seconds',
  help: 'Duration of database queries in seconds',
  labelNames: ['operation', 'table']
});

// Enregistrer les métriques
register.registerMetric(httpRequestDuration);
register.registerMetric(httpRequestTotal);
register.registerMetric(activeConnections);
register.registerMetric(deviceActions);
register.registerMetric(databaseQueries);

// Middleware pour collecter les métriques HTTP
export const metricsMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const start = Date.now();
  
  res.on('finish', () => {
    const duration = (Date.now() - start) / 1000;
    const route = req.route?.path || req.path;
    
    httpRequestDuration
      .labels(req.method, route, res.statusCode.toString())
      .observe(duration);
    
    httpRequestTotal
      .labels(req.method, route, res.statusCode.toString())
      .inc();
  });
  
  next();
};

// Endpoint pour exposer les métriques
export const metricsHandler = async (req: Request, res: Response) => {
  res.set('Content-Type', register.contentType);
  res.end(await register.metrics());
};

// Helpers pour les métriques métier
export const metrics = {
  incrementDeviceAction: (actionType: string, status: string, deviceType: string) => {
    deviceActions.labels(actionType, status, deviceType).inc();
  },

  observeDatabaseQuery: (operation: string, table: string, duration: number) => {
    databaseQueries.labels(operation, table).observe(duration / 1000);
  },

  setActiveConnections: (count: number) => {
    activeConnections.set(count);
  }
};
```

### Configuration Grafana

```yaml
# monitoring/grafana/datasources/prometheus.yml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
```

```json
// monitoring/grafana/dashboards/essensys-overview.json
{
  "dashboard": {
    "id": null,
    "title": "Essensys - Vue d'ensemble",
    "tags": ["essensys"],
    "timezone": "Europe/Paris",
    "panels": [
      {
        "title": "Requêtes HTTP par minute",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[1m])",
            "legendFormat": "{{method}} {{route}}"
          }
        ]
      },
      {
        "title": "Temps de réponse moyen",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])",
            "legendFormat": "Temps moyen"
          }
        ]
      },
      {
        "title": "Connexions WebSocket actives",
        "type": "singlestat",
        "targets": [
          {
            "expr": "websocket_connections_active",
            "legendFormat": "Connexions"
          }
        ]
      },
      {
        "title": "Actions d'appareils par type",
        "type": "piechart",
        "targets": [
          {
            "expr": "sum by (action_type) (rate(device_actions_total[1h]))",
            "legendFormat": "{{action_type}}"
          }
        ]
      }
    ]
  }
}
```

## Outils CI/CD et Pipelines de Déploiement

### GitHub Actions Workflow

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # Tests et qualité du code
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: essensys_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
      
      redis:
        image: redis:7
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18'
          cache: 'npm'

      - name: Install dependencies
        run: |
          npm ci
          cd frontend && npm ci

      - name: Lint code
        run: |
          npm run lint
          cd frontend && npm run lint

      - name: Type check
        run: |
          npm run type-check
          cd frontend && npm run type-check

      - name: Run backend tests
        env:
          DATABASE_URL: postgresql://postgres:postgres@localhost:5432/essensys_test
          REDIS_URL: redis://localhost:6379
          JWT_SECRET: test_jwt_secret
          MACHINE_JWT_SECRET: test_machine_jwt_secret
        run: |
          npm run test:coverage

      - name: Run frontend tests
        run: |
          cd frontend && npm run test:coverage

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/lcov.info,./frontend/coverage/lcov.info

  # Build et push des images Docker
  build:
    needs: test
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    
    permissions:
      contents: read
      packages: write

    strategy:
      matrix:
        component: [backend, frontend]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}-${{ matrix.component }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=sha,prefix={{branch}}-
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./docker/${{ matrix.component }}/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  # Déploiement en staging
  deploy-staging:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    
    environment:
      name: staging
      url: https://staging.essensys.com

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to staging
        uses: appleboy/ssh-action@v1.0.0
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: ${{ secrets.STAGING_USER }}
          key: ${{ secrets.STAGING_SSH_KEY }}
          script: |
            cd /opt/essensys
            git pull origin develop
            docker-compose -f docker-compose.staging.yml pull
            docker-compose -f docker-compose.staging.yml up -d
            docker system prune -f

      - name: Run health checks
        run: |
          sleep 30
          curl -f https://staging-api.essensys.com/api/health || exit 1

  # Déploiement en production
  deploy-production:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    environment:
      name: production
      url: https://app.essensys.com

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to production
        uses: appleboy/ssh-action@v1.0.0
        with:
          host: ${{ secrets.PRODUCTION_HOST }}
          username: ${{ secrets.PRODUCTION_USER }}
          key: ${{ secrets.PRODUCTION_SSH_KEY }}
          script: |
            cd /opt/essensys
            git pull origin main
            docker-compose -f docker-compose.prod.yml pull
            docker-compose -f docker-compose.prod.yml up -d --no-deps backend frontend
            docker system prune -f

      - name: Run smoke tests
        run: |
          sleep 60
          curl -f https://api.essensys.com/api/health || exit 1
          curl -f https://app.essensys.com/health || exit 1

      - name: Notify deployment
        uses: 8398a7/action-slack@v3
        with:
          status: ${{ job.status }}
          channel: '#deployments'
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}
        if: always()
```

### Scripts de Déploiement

```bash
#!/bin/bash
# scripts/deploy.sh

set -e

ENVIRONMENT=${1:-staging}
VERSION=${2:-latest}

echo "🚀 Deploying Essensys to $ENVIRONMENT (version: $VERSION)"

# Validation des paramètres
if [[ ! "$ENVIRONMENT" =~ ^(staging|production)$ ]]; then
    echo "❌ Environment must be 'staging' or 'production'"
    exit 1
fi

# Configuration par environnement
case $ENVIRONMENT in
    staging)
        COMPOSE_FILE="docker-compose.staging.yml"
        HOST="staging.essensys.com"
        ;;
    production)
        COMPOSE_FILE="docker-compose.prod.yml"
        HOST="app.essensys.com"
        ;;
esac

echo "📋 Using compose file: $COMPOSE_FILE"

# Backup de la base de données (production uniquement)
if [[ "$ENVIRONMENT" == "production" ]]; then
    echo "💾 Creating database backup..."
    docker-compose -f $COMPOSE_FILE exec -T database pg_dump -U essensys essensys_prod > "backup_$(date +%Y%m%d_%H%M%S).sql"
fi

# Pull des nouvelles images
echo "📥 Pulling latest images..."
docker-compose -f $COMPOSE_FILE pull

# Arrêt gracieux des services
echo "⏹️ Stopping services..."
docker-compose -f $COMPOSE_FILE down --timeout 30

# Démarrage des services
echo "▶️ Starting services..."
docker-compose -f $COMPOSE_FILE up -d

# Attendre que les services soient prêts
echo "⏳ Waiting for services to be ready..."
sleep 30

# Health checks
echo "🔍 Running health checks..."
for i in {1..10}; do
    if curl -f "https://$HOST/api/health" > /dev/null 2>&1; then
        echo "✅ Backend is healthy"
        break
    fi
    echo "⏳ Waiting for backend... ($i/10)"
    sleep 10
done

for i in {1..10}; do
    if curl -f "https://$HOST/health" > /dev/null 2>&1; then
        echo "✅ Frontend is healthy"
        break
    fi
    echo "⏳ Waiting for frontend... ($i/10)"
    sleep 10
done

# Nettoyage
echo "🧹 Cleaning up..."
docker system prune -f

echo "🎉 Deployment completed successfully!"
```

Cette architecture d'infrastructure et de déploiement fournit une base robuste et scalable pour la migration d'Essensys, avec une containerisation complète, des environnements bien séparés, un monitoring avancé et des pipelines CI/CD automatisés.