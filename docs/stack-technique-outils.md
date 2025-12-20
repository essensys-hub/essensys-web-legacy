# Stack Technique et Outils - Migration Essensys

## Vue d'Ensemble

Ce document spécifie la stack technique complète, les versions des technologies et la configuration des outils pour la migration Essensys. Il inclut également les templates de projet et les instructions d'installation des environnements de développement.

## Stack Technique Complète

### Frontend React

**Technologies Principales:**
```json
{
  "react": "^18.2.0",
  "react-dom": "^18.2.0",
  "typescript": "^5.3.0",
  "@types/react": "^18.2.0",
  "@types/react-dom": "^18.2.0"
}
```

**State Management:**
```json
{
  "@reduxjs/toolkit": "^2.0.1",
  "react-redux": "^9.0.4",
  "@types/react-redux": "^7.1.33"
}
```

**Routing et Navigation:**
```json
{
  "react-router-dom": "^6.20.1",
  "@types/react-router-dom": "^5.3.3"
}
```

**UI et Styling:**
```json
{
  "@mui/material": "^5.15.0",
  "@mui/icons-material": "^5.15.0",
  "@emotion/react": "^11.11.1",
  "@emotion/styled": "^11.11.0",
  "styled-components": "^6.1.6",
  "@types/styled-components": "^5.1.34"
}
```

**Formulaires et Validation:**
```json
{
  "react-hook-form": "^7.48.2",
  "zod": "^3.22.4",
  "@hookform/resolvers": "^3.3.2"
}
```

**Utilitaires Frontend:**
```json
{
  "axios": "^1.6.2",
  "socket.io-client": "^4.7.4",
  "react-window": "^1.8.8",
  "@types/react-window": "^1.8.8",
  "date-fns": "^2.30.0",
  "lodash": "^4.17.21",
  "@types/lodash": "^4.14.202"
}
```

### Backend Node.js

**Runtime et Framework:**
```json
{
  "node": ">=18.18.0",
  "express": "^4.18.2",
  "@types/express": "^4.17.21",
  "typescript": "^5.3.0",
  "ts-node": "^10.9.1"
}
```

**Base de Données et ORM:**
```json
{
  "prisma": "^5.7.1",
  "@prisma/client": "^5.7.1",
  "pg": "^8.11.3",
  "@types/pg": "^8.10.9"
}
```

**Authentification et Sécurité:**
```json
{
  "jsonwebtoken": "^9.0.2",
  "@types/jsonwebtoken": "^9.0.5",
  "bcrypt": "^5.1.1",
  "@types/bcrypt": "^5.0.2",
  "helmet": "^7.1.0",
  "cors": "^2.8.5",
  "@types/cors": "^2.8.17"
}
```

**Validation et Middleware:**
```json
{
  "zod": "^3.22.4",
  "express-rate-limit": "^7.1.5",
  "express-validator": "^7.0.1",
  "multer": "^1.4.5-lts.1",
  "@types/multer": "^1.4.11"
}
```

**Cache et Session:**
```json
{
  "redis": "^4.6.11",
  "ioredis": "^5.3.2",
  "@types/ioredis": "^5.0.0"
}
```

**WebSocket et Temps Réel:**
```json
{
  "socket.io": "^4.7.4",
  "@types/socket.io": "^3.0.2"
}
```

**Utilitaires Backend:**
```json
{
  "winston": "^3.11.0",
  "dotenv": "^16.3.1",
  "nodemailer": "^6.9.7",
  "@types/nodemailer": "^6.4.14",
  "cron": "^3.1.6",
  "@types/cron": "^2.4.0"
}
```

### Base de Données

**SGBD Principal:**
- **PostgreSQL**: 15.5+
- **Extensions**: uuid-ossp, pgcrypto, pg_stat_statements

**Cache:**
- **Redis**: 7.2+

### Infrastructure et Déploiement

**Conteneurisation:**
```yaml
# Docker versions
docker: ">=24.0.0"
docker-compose: ">=2.20.0"
```

**Orchestration:**
```yaml
# Kubernetes (optionnel pour production)
kubernetes: ">=1.28.0"
helm: ">=3.13.0"
```

**CI/CD:**
```yaml
# GitHub Actions ou GitLab CI
node: "18.18.0"
docker: "24.0.0"
```

## Configuration des Outils de Build

### Frontend - Configuration Vite

**vite.config.ts:**
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@components': resolve(__dirname, 'src/components'),
      '@pages': resolve(__dirname, 'src/pages'),
      '@hooks': resolve(__dirname, 'src/hooks'),
      '@services': resolve(__dirname, 'src/services'),
      '@store': resolve(__dirname, 'src/store'),
      '@types': resolve(__dirname, 'src/types'),
      '@utils': resolve(__dirname, 'src/utils')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:3001',
        changeOrigin: true
      },
      '/socket.io': {
        target: 'http://localhost:3001',
        ws: true
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          redux: ['@reduxjs/toolkit', 'react-redux'],
          ui: ['@mui/material', '@mui/icons-material'],
          utils: ['axios', 'date-fns', 'lodash']
        }
      }
    }
  },
  define: {
    __APP_VERSION__: JSON.stringify(process.env.npm_package_version)
  }
});
```

**tsconfig.json (Frontend):**
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@components/*": ["src/components/*"],
      "@pages/*": ["src/pages/*"],
      "@hooks/*": ["src/hooks/*"],
      "@services/*": ["src/services/*"],
      "@store/*": ["src/store/*"],
      "@types/*": ["src/types/*"],
      "@utils/*": ["src/utils/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

### Backend - Configuration TypeScript

**tsconfig.json (Backend):**
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "commonjs",
    "lib": ["ES2020"],
    "outDir": "./dist",
    "rootDir": "./src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true,
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "experimentalDecorators": true,
    "emitDecoratorMetadata": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@controllers/*": ["src/controllers/*"],
      "@services/*": ["src/services/*"],
      "@models/*": ["src/models/*"],
      "@middleware/*": ["src/middleware/*"],
      "@utils/*": ["src/utils/*"],
      "@types/*": ["src/types/*"]
    }
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist", "**/*.test.ts", "**/*.spec.ts"]
}
```

**nodemon.json:**
```json
{
  "watch": ["src"],
  "ext": "ts,json",
  "ignore": ["src/**/*.test.ts", "src/**/*.spec.ts"],
  "exec": "ts-node -r tsconfig-paths/register src/index.ts",
  "env": {
    "NODE_ENV": "development"
  }
}
```

## Outils de Test

### Configuration Jest

**jest.config.js (Frontend):**
```javascript
export default {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/src/setupTests.ts'],
  moduleNameMapping: {
    '^@/(.*)$': '<rootDir>/src/$1',
    '^@components/(.*)$': '<rootDir>/src/components/$1',
    '^@pages/(.*)$': '<rootDir>/src/pages/$1',
    '^@hooks/(.*)$': '<rootDir>/src/hooks/$1',
    '^@services/(.*)$': '<rootDir>/src/services/$1',
    '^@store/(.*)$': '<rootDir>/src/store/$1',
    '^@types/(.*)$': '<rootDir>/src/types/$1',
    '^@utils/(.*)$': '<rootDir>/src/utils/$1'
  },
  collectCoverageFrom: [
    'src/**/*.{ts,tsx}',
    '!src/**/*.d.ts',
    '!src/main.tsx',
    '!src/vite-env.d.ts'
  ],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80
    }
  },
  testMatch: [
    '<rootDir>/src/**/__tests__/**/*.{ts,tsx}',
    '<rootDir>/src/**/*.{test,spec}.{ts,tsx}'
  ]
};
```

**vitest.config.ts (Alternative moderne):**
```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/setupTests.ts',
        'src/main.tsx',
        'src/vite-env.d.ts'
      ],
      thresholds: {
        global: {
          branches: 80,
          functions: 80,
          lines: 80,
          statements: 80
        }
      }
    }
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@components': resolve(__dirname, 'src/components'),
      '@pages': resolve(__dirname, 'src/pages'),
      '@hooks': resolve(__dirname, 'src/hooks'),
      '@services': resolve(__dirname, 'src/services'),
      '@store': resolve(__dirname, 'src/store'),
      '@types': resolve(__dirname, 'src/types'),
      '@utils': resolve(__dirname, 'src/utils')
    }
  }
});
```

### Configuration Cypress

**cypress.config.ts:**
```typescript
import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:3000',
    supportFile: 'cypress/support/e2e.ts',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    video: true,
    screenshotOnRunFailure: true,
    viewportWidth: 1280,
    viewportHeight: 720,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    env: {
      apiUrl: 'http://localhost:3001/api'
    }
  },
  component: {
    devServer: {
      framework: 'react',
      bundler: 'vite'
    },
    supportFile: 'cypress/support/component.ts',
    specPattern: 'src/**/*.cy.{js,jsx,ts,tsx}'
  }
});
```

## Templates de Projet

### Structure Frontend React

```
essensys-frontend/
├── public/
│   ├── index.html
│   ├── favicon.ico
│   └── manifest.json
├── src/
│   ├── components/
│   │   ├── common/
│   │   │   ├── Button/
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Button.test.tsx
│   │   │   │   └── index.ts
│   │   │   ├── LoadingSpinner/
│   │   │   └── index.ts
│   │   ├── device/
│   │   │   ├── DeviceCard/
│   │   │   ├── DeviceList/
│   │   │   ├── HeatingControl/
│   │   │   └── index.ts
│   │   ├── forms/
│   │   └── layout/
│   ├── pages/
│   │   ├── Dashboard/
│   │   ├── DeviceControl/
│   │   ├── Settings/
│   │   └── Login/
│   ├── hooks/
│   │   ├── useDevices.ts
│   │   ├── useAuth.ts
│   │   └── index.ts
│   ├── services/
│   │   ├── api-client.ts
│   │   ├── device-service.ts
│   │   ├── auth-service.ts
│   │   └── websocket-service.ts
│   ├── store/
│   │   ├── slices/
│   │   │   ├── auth-slice.ts
│   │   │   ├── devices-slice.ts
│   │   │   └── notifications-slice.ts
│   │   ├── index.ts
│   │   └── middleware.ts
│   ├── types/
│   │   ├── api.ts
│   │   ├── device.ts
│   │   ├── user.ts
│   │   └── index.ts
│   ├── utils/
│   │   ├── constants.ts
│   │   ├── helpers.ts
│   │   └── validators.ts
│   ├── styles/
│   │   ├── globals.css
│   │   ├── variables.css
│   │   └── components.css
│   ├── __tests__/
│   │   ├── setup.ts
│   │   └── utils/
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── cypress/
│   ├── e2e/
│   ├── fixtures/
│   └── support/
├── package.json
├── vite.config.ts
├── tsconfig.json
├── vitest.config.ts
├── cypress.config.ts
├── .eslintrc.json
├── .prettierrc
└── README.md
```

### Structure Backend Node.js

```
essensys-backend/
├── src/
│   ├── controllers/
│   │   ├── auth-controller.ts
│   │   ├── device-controller.ts
│   │   ├── machine-controller.ts
│   │   └── index.ts
│   ├── services/
│   │   ├── auth-service.ts
│   │   ├── device-service.ts
│   │   ├── cache-service.ts
│   │   ├── notification-service.ts
│   │   └── index.ts
│   ├── models/
│   │   ├── user.ts
│   │   ├── device.ts
│   │   ├── machine.ts
│   │   └── index.ts
│   ├── middleware/
│   │   ├── auth-middleware.ts
│   │   ├── validation-middleware.ts
│   │   ├── error-middleware.ts
│   │   └── index.ts
│   ├── routes/
│   │   ├── auth-routes.ts
│   │   ├── device-routes.ts
│   │   ├── machine-routes.ts
│   │   └── index.ts
│   ├── config/
│   │   ├── database.ts
│   │   ├── redis.ts
│   │   ├── jwt.ts
│   │   └── index.ts
│   ├── utils/
│   │   ├── logger.ts
│   │   ├── crypto.ts
│   │   ├── validators.ts
│   │   └── helpers.ts
│   ├── types/
│   │   ├── api.ts
│   │   ├── auth.ts
│   │   ├── device.ts
│   │   └── index.ts
│   ├── __tests__/
│   │   ├── integration/
│   │   ├── unit/
│   │   └── setup.ts
│   ├── websocket/
│   │   ├── handlers/
│   │   ├── middleware/
│   │   └── index.ts
│   ├── jobs/
│   │   ├── cleanup-job.ts
│   │   ├── notification-job.ts
│   │   └── index.ts
│   └── index.ts
├── prisma/
│   ├── schema.prisma
│   ├── migrations/
│   └── seed.ts
├── docker/
│   ├── Dockerfile
│   ├── docker-compose.yml
│   └── docker-compose.prod.yml
├── scripts/
│   ├── build.sh
│   ├── deploy.sh
│   └── migrate.sh
├── package.json
├── tsconfig.json
├── jest.config.js
├── nodemon.json
├── .eslintrc.json
├── .prettierrc
└── README.md
```

## Configuration des Environnements de Développement

### Variables d'Environnement

**.env.example (Frontend):**
```bash
# API Configuration
REACT_APP_API_URL=http://localhost:3001/api
REACT_APP_WS_URL=http://localhost:3001

# Application Configuration
REACT_APP_NAME=Essensys
REACT_APP_VERSION=1.0.0
REACT_APP_ENVIRONMENT=development

# Feature Flags
REACT_APP_ENABLE_WEBSOCKET=true
REACT_APP_ENABLE_NOTIFICATIONS=true
REACT_APP_DEBUG_MODE=true

# External Services
REACT_APP_SENTRY_DSN=
REACT_APP_GOOGLE_ANALYTICS_ID=
```

**.env.example (Backend):**
```bash
# Server Configuration
NODE_ENV=development
PORT=3001
HOST=localhost

# Database Configuration
DATABASE_URL=postgresql://essensys:password@localhost:5432/essensys_dev
DATABASE_POOL_SIZE=10

# Redis Configuration
REDIS_URL=redis://localhost:6379
REDIS_PASSWORD=

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-change-in-production
REFRESH_SECRET=your-super-secret-refresh-key-change-in-production
JWT_EXPIRES_IN=15m
REFRESH_EXPIRES_IN=30d

# Machine JWT (for IoT devices)
MACHINE_JWT_SECRET=your-machine-jwt-secret-change-in-production
MACHINE_JWT_EXPIRES_IN=24h

# Email Configuration
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASS=your-app-password
EMAIL_FROM=noreply@essensys.com

# SMS Configuration (Twilio example)
TWILIO_ACCOUNT_SID=
TWILIO_AUTH_TOKEN=
TWILIO_PHONE_NUMBER=

# File Upload
MAX_FILE_SIZE=10485760
UPLOAD_PATH=./uploads

# Logging
LOG_LEVEL=debug
LOG_FILE=./logs/app.log

# Security
BCRYPT_ROUNDS=12
RATE_LIMIT_WINDOW_MS=900000
RATE_LIMIT_MAX_REQUESTS=100

# External Services
SENTRY_DSN=
```

### Docker Configuration

**docker-compose.yml (Développement):**
```yaml
version: '3.8'

services:
  # Base de données PostgreSQL
  postgres:
    image: postgres:15.5-alpine
    container_name: essensys-postgres
    environment:
      POSTGRES_DB: essensys_dev
      POSTGRES_USER: essensys
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./docker/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - essensys-network

  # Cache Redis
  redis:
    image: redis:7.2-alpine
    container_name: essensys-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - essensys-network

  # Backend API
  backend:
    build:
      context: .
      dockerfile: docker/Dockerfile.backend
    container_name: essensys-backend
    environment:
      NODE_ENV: development
      DATABASE_URL: postgresql://essensys:password@postgres:5432/essensys_dev
      REDIS_URL: redis://redis:6379
    ports:
      - "3001:3001"
    volumes:
      - ./backend:/app
      - /app/node_modules
    depends_on:
      - postgres
      - redis
    networks:
      - essensys-network
    command: npm run dev

  # Frontend React
  frontend:
    build:
      context: .
      dockerfile: docker/Dockerfile.frontend
    container_name: essensys-frontend
    environment:
      REACT_APP_API_URL: http://localhost:3001/api
      REACT_APP_WS_URL: http://localhost:3001
    ports:
      - "3000:3000"
    volumes:
      - ./frontend:/app
      - /app/node_modules
    depends_on:
      - backend
    networks:
      - essensys-network
    command: npm run dev

  # Adminer pour la gestion de base de données
  adminer:
    image: adminer:4.8.1
    container_name: essensys-adminer
    ports:
      - "8080:8080"
    depends_on:
      - postgres
    networks:
      - essensys-network

volumes:
  postgres_data:
  redis_data:

networks:
  essensys-network:
    driver: bridge
```

**Dockerfile.backend:**
```dockerfile
FROM node:18.18.0-alpine

# Installer les dépendances système
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    postgresql-client

# Créer le répertoire de l'application
WORKDIR /app

# Copier les fichiers de dépendances
COPY package*.json ./
COPY prisma ./prisma/

# Installer les dépendances
RUN npm ci --only=production && npm cache clean --force

# Copier le code source
COPY . .

# Générer le client Prisma
RUN npx prisma generate

# Construire l'application
RUN npm run build

# Créer un utilisateur non-root
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nodejs -u 1001

# Changer la propriété des fichiers
RUN chown -R nodejs:nodejs /app
USER nodejs

# Exposer le port
EXPOSE 3001

# Commande de démarrage
CMD ["npm", "start"]
```

**Dockerfile.frontend:**
```dockerfile
FROM node:18.18.0-alpine as builder

# Créer le répertoire de l'application
WORKDIR /app

# Copier les fichiers de dépendances
COPY package*.json ./

# Installer les dépendances
RUN npm ci --only=production && npm cache clean --force

# Copier le code source
COPY . .

# Construire l'application
RUN npm run build

# Stage de production avec Nginx
FROM nginx:1.25-alpine

# Copier la configuration Nginx
COPY docker/nginx.conf /etc/nginx/nginx.conf

# Copier les fichiers construits
COPY --from=builder /app/dist /usr/share/nginx/html

# Exposer le port
EXPOSE 80

# Commande de démarrage
CMD ["nginx", "-g", "daemon off;"]
```

### Scripts d'Installation

**setup-dev.sh:**
```bash
#!/bin/bash

# Script d'installation de l'environnement de développement Essensys

set -e

echo "🚀 Installation de l'environnement de développement Essensys"

# Vérifier les prérequis
check_prerequisites() {
    echo "📋 Vérification des prérequis..."
    
    # Node.js
    if ! command -v node &> /dev/null; then
        echo "❌ Node.js n'est pas installé. Version requise: 18.18.0+"
        exit 1
    fi
    
    NODE_VERSION=$(node -v | cut -d'v' -f2)
    if ! npx semver -r ">=18.18.0" "$NODE_VERSION" &> /dev/null; then
        echo "❌ Version Node.js insuffisante. Actuelle: $NODE_VERSION, Requise: 18.18.0+"
        exit 1
    fi
    
    # Docker
    if ! command -v docker &> /dev/null; then
        echo "❌ Docker n'est pas installé"
        exit 1
    fi
    
    # Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        echo "❌ Docker Compose n'est pas installé"
        exit 1
    fi
    
    echo "✅ Tous les prérequis sont satisfaits"
}

# Installer les dépendances
install_dependencies() {
    echo "📦 Installation des dépendances..."
    
    # Backend
    if [ -d "backend" ]; then
        echo "📦 Installation des dépendances backend..."
        cd backend
        npm install
        cd ..
    fi
    
    # Frontend
    if [ -d "frontend" ]; then
        echo "📦 Installation des dépendances frontend..."
        cd frontend
        npm install
        cd ..
    fi
}

# Configurer les variables d'environnement
setup_environment() {
    echo "⚙️ Configuration des variables d'environnement..."
    
    # Backend
    if [ -d "backend" ] && [ ! -f "backend/.env" ]; then
        cp backend/.env.example backend/.env
        echo "✅ Fichier .env backend créé"
    fi
    
    # Frontend
    if [ -d "frontend" ] && [ ! -f "frontend/.env" ]; then
        cp frontend/.env.example frontend/.env
        echo "✅ Fichier .env frontend créé"
    fi
}

# Démarrer les services Docker
start_services() {
    echo "🐳 Démarrage des services Docker..."
    docker-compose up -d postgres redis
    
    # Attendre que PostgreSQL soit prêt
    echo "⏳ Attente de PostgreSQL..."
    until docker-compose exec postgres pg_isready -U essensys; do
        sleep 1
    done
    
    echo "✅ Services Docker démarrés"
}

# Initialiser la base de données
setup_database() {
    echo "🗄️ Initialisation de la base de données..."
    
    if [ -d "backend" ]; then
        cd backend
        
        # Générer le client Prisma
        npx prisma generate
        
        # Exécuter les migrations
        npx prisma migrate dev --name init
        
        # Seed la base de données
        npx prisma db seed
        
        cd ..
        echo "✅ Base de données initialisée"
    fi
}

# Fonction principale
main() {
    check_prerequisites
    install_dependencies
    setup_environment
    start_services
    setup_database
    
    echo ""
    echo "🎉 Installation terminée avec succès!"
    echo ""
    echo "📝 Prochaines étapes:"
    echo "   1. Démarrer le backend: cd backend && npm run dev"
    echo "   2. Démarrer le frontend: cd frontend && npm run dev"
    echo "   3. Ouvrir http://localhost:3000 dans votre navigateur"
    echo ""
    echo "🔧 Outils disponibles:"
    echo "   - Adminer (DB): http://localhost:8080"
    echo "   - API Backend: http://localhost:3001"
    echo ""
}

# Exécuter le script
main "$@"
```

**package.json (Root):**
```json
{
  "name": "essensys-migration",
  "version": "1.0.0",
  "description": "Migration Essensys vers architecture moderne",
  "private": true,
  "workspaces": [
    "frontend",
    "backend"
  ],
  "scripts": {
    "install:all": "npm install && npm run install:frontend && npm run install:backend",
    "install:frontend": "cd frontend && npm install",
    "install:backend": "cd backend && npm install",
    "dev": "concurrently \"npm run dev:backend\" \"npm run dev:frontend\"",
    "dev:frontend": "cd frontend && npm run dev",
    "dev:backend": "cd backend && npm run dev",
    "build": "npm run build:frontend && npm run build:backend",
    "build:frontend": "cd frontend && npm run build",
    "build:backend": "cd backend && npm run build",
    "test": "npm run test:frontend && npm run test:backend",
    "test:frontend": "cd frontend && npm run test",
    "test:backend": "cd backend && npm run test",
    "lint": "npm run lint:frontend && npm run lint:backend",
    "lint:frontend": "cd frontend && npm run lint",
    "lint:backend": "cd backend && npm run lint",
    "docker:up": "docker-compose up -d",
    "docker:down": "docker-compose down",
    "docker:logs": "docker-compose logs -f",
    "db:migrate": "cd backend && npx prisma migrate dev",
    "db:seed": "cd backend && npx prisma db seed",
    "db:reset": "cd backend && npx prisma migrate reset"
  },
  "devDependencies": {
    "concurrently": "^8.2.2"
  },
  "engines": {
    "node": ">=18.18.0",
    "npm": ">=9.0.0"
  }
}
```

Cette configuration complète fournit tous les outils et templates nécessaires pour démarrer le développement de l'application Essensys moderne avec une stack technique robuste et des environnements de développement bien configurés.