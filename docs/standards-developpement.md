# Standards de Développement - Migration Essensys

## Vue d'Ensemble

Ce document définit les standards de développement, conventions de code et bonnes pratiques à suivre pour la migration de l'application Essensys vers une architecture moderne React/Node.js. Ces standards garantissent la cohérence, la maintenabilité et la qualité du code produit par l'équipe de développement.

## Conventions de Code TypeScript/JavaScript

### Structure des Fichiers et Nommage

**Conventions de Nommage:**
```typescript
// Fichiers et dossiers: kebab-case
user-service.ts
device-controller.ts
auth-middleware.ts

// Classes: PascalCase
class UserService {}
class DeviceController {}
class AuthenticationMiddleware {}

// Interfaces: PascalCase avec préfixe I optionnel
interface User {}
interface IUserRepository {}

// Types: PascalCase
type ActionStatus = 'pending' | 'sent' | 'executed' | 'failed';

// Variables et fonctions: camelCase
const userName = 'john';
const getUserById = (id: string) => {};

// Constantes: SCREAMING_SNAKE_CASE
const API_BASE_URL = 'https://api.essensys.com';
const MAX_RETRY_ATTEMPTS = 3;

// Enums: PascalCase
enum DeviceType {
  HEATING = 'heating',
  SHUTTER = 'shutter',
  ALARM = 'alarm'
}
```

**Structure des Dossiers:**
```
src/
├── components/          # Composants React réutilisables
│   ├── common/         # Composants génériques
│   ├── forms/          # Composants de formulaires
│   └── layout/         # Composants de mise en page
├── pages/              # Pages principales de l'application
├── hooks/              # Custom React hooks
├── services/           # Services API et logique métier
├── store/              # State management (Redux Toolkit)
├── types/              # Définitions TypeScript
├── utils/              # Fonctions utilitaires
├── constants/          # Constantes de l'application
└── __tests__/          # Tests organisés par type
```

### Conventions TypeScript

**Configuration TypeScript Stricte:**
```json
// tsconfig.json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "noImplicitReturns": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "exactOptionalPropertyTypes": true
  }
}
```

**Définition des Types:**
```typescript
// Préférer les interfaces pour les objets
interface User {
  readonly id: string;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: Date;
}

// Utiliser les types union pour les énumérations
type UserRole = 'owner' | 'user' | 'guest';
type ActionStatus = 'pending' | 'sent' | 'executed' | 'failed';

// Typage strict des fonctions
const createUser = (userData: Omit<User, 'id' | 'createdAt'>): Promise<User> => {
  // Implementation
};

// Utiliser des génériques pour la réutilisabilité
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

// Typage des props React
interface DeviceCardProps {
  device: Device;
  onUpdate: (device: Device) => void;
  className?: string;
}
```

### Conventions ESLint et Prettier

**Configuration ESLint:**
```json
// .eslintrc.json
{
  "extends": [
    "@typescript-eslint/recommended",
    "react-hooks/recommended",
    "prettier"
  ],
  "rules": {
    "@typescript-eslint/no-unused-vars": "error",
    "@typescript-eslint/explicit-function-return-type": "warn",
    "@typescript-eslint/no-explicit-any": "error",
    "react-hooks/exhaustive-deps": "error",
    "prefer-const": "error",
    "no-var": "error"
  }
}
```

**Configuration Prettier:**
```json
// .prettierrc
{
  "semi": true,
  "trailingComma": "es5",
  "singleQuote": true,
  "printWidth": 80,
  "tabWidth": 2,
  "useTabs": false
}
```

## Conventions SQL et Base de Données

### Nommage des Tables et Colonnes

```sql
-- Tables: snake_case au pluriel
CREATE TABLE users (...);
CREATE TABLE device_states (...);
CREATE TABLE firmware_versions (...);

-- Colonnes: snake_case
CREATE TABLE users (
  id UUID PRIMARY KEY,
  first_name VARCHAR(255) NOT NULL,
  last_name VARCHAR(255) NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index: préfixe idx_ + table + colonnes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_device_states_device_timestamp ON device_states(device_id, timestamp DESC);

-- Contraintes: préfixe selon le type
-- fk_ pour foreign keys, uk_ pour unique, ck_ pour check
ALTER TABLE user_machines ADD CONSTRAINT fk_user_machines_user_id 
  FOREIGN KEY (user_id) REFERENCES users(id);
```

### Standards de Migration

```sql
-- Toujours utiliser des transactions
BEGIN;

-- Nommer les migrations avec timestamp
-- 20241220_001_create_users_table.sql
-- 20241220_002_add_device_types.sql

-- Inclure des rollback scripts
-- UP migration
CREATE TABLE users (...);

-- DOWN migration (dans un fichier séparé ou commenté)
-- DROP TABLE users;

-- Utiliser des migrations idempotentes
CREATE TABLE IF NOT EXISTS users (...);

COMMIT;
```

## Patterns Architecturaux

### Frontend React - Patterns Recommandés

**1. Composition over Inheritance:**
```typescript
// Préférer la composition
interface WithLoadingProps {
  isLoading: boolean;
  children: React.ReactNode;
}

const WithLoading: React.FC<WithLoadingProps> = ({ isLoading, children }) => {
  if (isLoading) return <LoadingSpinner />;
  return <>{children}</>;
};

// Utilisation
<WithLoading isLoading={loading}>
  <DeviceList devices={devices} />
</WithLoading>
```

**2. Custom Hooks pour la Logique Métier:**
```typescript
// Hook personnalisé pour la gestion des appareils
const useDevices = (machineId: string) => {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDevices = useCallback(async () => {
    try {
      setLoading(true);
      const response = await deviceService.getDevices(machineId);
      setDevices(response.data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [machineId]);

  useEffect(() => {
    fetchDevices();
  }, [fetchDevices]);

  return { devices, loading, error, refetch: fetchDevices };
};
```

**3. State Management avec Redux Toolkit:**
```typescript
// Slice Redux avec RTK
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export const fetchDevices = createAsyncThunk(
  'devices/fetchDevices',
  async (machineId: string, { rejectWithValue }) => {
    try {
      const response = await deviceService.getDevices(machineId);
      return response.data;
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

const devicesSlice = createSlice({
  name: 'devices',
  initialState: {
    items: [] as Device[],
    loading: false,
    error: null as string | null,
  },
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDevices.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDevices.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchDevices.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});
```

### Backend Node.js - Patterns Recommandés

**1. Architecture en Couches:**
```typescript
// Controller Layer
export class DeviceController {
  constructor(private deviceService: DeviceService) {}

  async getDevices(req: Request, res: Response): Promise<void> {
    try {
      const { machineId } = req.params;
      const devices = await this.deviceService.getDevicesByMachine(machineId);
      res.json({ success: true, data: devices });
    } catch (error) {
      res.status(500).json({ success: false, error: error.message });
    }
  }
}

// Service Layer
export class DeviceService {
  constructor(private deviceRepository: DeviceRepository) {}

  async getDevicesByMachine(machineId: string): Promise<Device[]> {
    const devices = await this.deviceRepository.findByMachineId(machineId);
    return devices.filter(device => device.isActive);
  }
}

// Repository Layer
export class DeviceRepository {
  async findByMachineId(machineId: string): Promise<Device[]> {
    return prisma.device.findMany({
      where: { machineId },
      include: { deviceType: true }
    });
  }
}
```

**2. Dependency Injection:**
```typescript
// Container de dépendances
export class Container {
  private static instance: Container;
  private services = new Map<string, any>();

  static getInstance(): Container {
    if (!Container.instance) {
      Container.instance = new Container();
    }
    return Container.instance;
  }

  register<T>(name: string, factory: () => T): void {
    this.services.set(name, factory);
  }

  resolve<T>(name: string): T {
    const factory = this.services.get(name);
    if (!factory) {
      throw new Error(`Service ${name} not found`);
    }
    return factory();
  }
}

// Configuration des services
const container = Container.getInstance();
container.register('deviceRepository', () => new DeviceRepository());
container.register('deviceService', () => 
  new DeviceService(container.resolve('deviceRepository'))
);
```

**3. Middleware Pattern:**
```typescript
// Middleware d'authentification
export const authenticateUser = async (
  req: Request, 
  res: Response, 
  next: NextFunction
): Promise<void> => {
  try {
    const token = req.headers.authorization?.replace('Bearer ', '');
    if (!token) {
      res.status(401).json({ error: 'Token manquant' });
      return;
    }

    const payload = jwt.verify(token, process.env.JWT_SECRET!) as UserPayload;
    req.user = payload;
    next();
  } catch (error) {
    res.status(401).json({ error: 'Token invalide' });
  }
};

// Middleware de validation
export const validateSchema = (schema: z.ZodSchema) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    try {
      schema.parse(req.body);
      next();
    } catch (error) {
      res.status(400).json({ error: 'Données invalides', details: error.errors });
    }
  };
};
```

## Bonnes Pratiques de Sécurité

### Authentification et Autorisation

```typescript
// Validation des entrées avec Zod
import { z } from 'zod';

const loginSchema = z.object({
  email: z.string().email('Email invalide'),
  password: z.string().min(8, 'Mot de passe trop court')
});

// Hashage sécurisé des mots de passe
import bcrypt from 'bcrypt';

const hashPassword = async (password: string): Promise<string> => {
  return bcrypt.hash(password, 12); // Cost factor élevé
};

// Génération de tokens JWT sécurisés
const generateTokens = (user: User) => {
  const accessToken = jwt.sign(
    { userId: user.id, email: user.email },
    process.env.JWT_SECRET!,
    { expiresIn: '15m', algorithm: 'HS256' }
  );
  
  const refreshToken = jwt.sign(
    { userId: user.id, type: 'refresh' },
    process.env.REFRESH_SECRET!,
    { expiresIn: '30d', algorithm: 'HS256' }
  );
  
  return { accessToken, refreshToken };
};
```

### Protection contre les Attaques

```typescript
// Protection CSRF
import csrf from 'csurf';
app.use(csrf({ cookie: true }));

// Rate limiting
import rateLimit from 'express-rate-limit';
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limite de 100 requêtes par IP
  message: 'Trop de requêtes, réessayez plus tard'
});
app.use('/api/', limiter);

// Validation et sanitisation
import DOMPurify from 'isomorphic-dompurify';
const sanitizeInput = (input: string): string => {
  return DOMPurify.sanitize(input);
};

// Headers de sécurité
import helmet from 'helmet';
app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      scriptSrc: ["'self'", "'unsafe-inline'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      imgSrc: ["'self'", "data:", "https:"]
    }
  }
}));
```

## Bonnes Pratiques de Performance

### Frontend React

```typescript
// Mémorisation des composants
const DeviceCard = React.memo<DeviceCardProps>(({ device, onUpdate }) => {
  return (
    <div className="device-card">
      <h3>{device.name}</h3>
      <button onClick={() => onUpdate(device)}>
        Mettre à jour
      </button>
    </div>
  );
});

// Lazy loading des composants
const DeviceSettings = React.lazy(() => import('./DeviceSettings'));

const App = () => (
  <Suspense fallback={<LoadingSpinner />}>
    <DeviceSettings />
  </Suspense>
);

// Optimisation des re-renders
const useOptimizedCallback = useCallback((deviceId: string) => {
  // Logique de callback
}, [/* dépendances minimales */]);

// Virtualisation pour les grandes listes
import { FixedSizeList as List } from 'react-window';

const DeviceList = ({ devices }: { devices: Device[] }) => (
  <List
    height={600}
    itemCount={devices.length}
    itemSize={80}
    itemData={devices}
  >
    {({ index, style, data }) => (
      <div style={style}>
        <DeviceCard device={data[index]} />
      </div>
    )}
  </List>
);
```

### Backend Node.js

```typescript
// Mise en cache avec Redis
import Redis from 'ioredis';
const redis = new Redis(process.env.REDIS_URL);

const getCachedDevices = async (machineId: string): Promise<Device[] | null> => {
  const cached = await redis.get(`devices:${machineId}`);
  return cached ? JSON.parse(cached) : null;
};

const setCachedDevices = async (machineId: string, devices: Device[]): Promise<void> => {
  await redis.setex(`devices:${machineId}`, 300, JSON.stringify(devices)); // 5 min TTL
};

// Pagination efficace
interface PaginationOptions {
  page: number;
  limit: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

const getPaginatedDevices = async (options: PaginationOptions) => {
  const offset = (options.page - 1) * options.limit;
  
  return prisma.device.findMany({
    skip: offset,
    take: options.limit,
    orderBy: options.sortBy ? {
      [options.sortBy]: options.sortOrder || 'asc'
    } : undefined
  });
};

// Requêtes optimisées avec includes
const getDeviceWithRelations = async (deviceId: string) => {
  return prisma.device.findUnique({
    where: { id: deviceId },
    include: {
      deviceType: true,
      machine: {
        select: { id: true, serialNumber: true }
      },
      states: {
        orderBy: { timestamp: 'desc' },
        take: 1
      }
    }
  });
};
```

## Guidelines pour les Revues de Code

### Checklist de Revue de Code

**Aspects Techniques:**
- [ ] Le code respecte les conventions de nommage établies
- [ ] Les types TypeScript sont correctement définis et utilisés
- [ ] Pas de `any` ou de types implicites
- [ ] Les fonctions ont une responsabilité unique (SRP)
- [ ] Les dépendances sont correctement injectées
- [ ] Gestion appropriée des erreurs avec try/catch
- [ ] Validation des entrées utilisateur
- [ ] Pas de secrets ou données sensibles en dur

**Aspects Sécurité:**
- [ ] Authentification et autorisation correctes
- [ ] Validation et sanitisation des entrées
- [ ] Protection contre les injections SQL
- [ ] Gestion sécurisée des tokens et sessions
- [ ] Headers de sécurité appropriés
- [ ] Pas de logs de données sensibles

**Aspects Performance:**
- [ ] Pas de requêtes N+1
- [ ] Utilisation appropriée de la mise en cache
- [ ] Optimisation des re-renders React
- [ ] Lazy loading quand approprié
- [ ] Pagination pour les grandes listes
- [ ] Index de base de données appropriés

**Aspects Maintenabilité:**
- [ ] Code lisible et bien documenté
- [ ] Tests unitaires et d'intégration présents
- [ ] Pas de duplication de code
- [ ] Séparation claire des responsabilités
- [ ] Documentation des APIs mise à jour
- [ ] Gestion appropriée des migrations

### Processus de Revue

1. **Revue Automatisée:**
   - ESLint et Prettier passent sans erreur
   - Tests unitaires passent
   - Couverture de code > 80%
   - Build réussit sans warnings

2. **Revue Manuelle:**
   - Au moins 2 reviewers pour les changements critiques
   - Focus sur la logique métier et l'architecture
   - Vérification de la sécurité et des performances
   - Validation de la documentation

3. **Critères d'Approbation:**
   - Tous les commentaires résolus
   - Tests passent en CI/CD
   - Documentation mise à jour
   - Approbation de 2 reviewers minimum

### Templates de Commentaires

```markdown
# Suggestion d'amélioration
**Problème:** Le code actuel fait X
**Suggestion:** Considérer Y pour améliorer Z
**Exemple:**
```typescript
// Au lieu de
const result = data.map(item => item.value).filter(v => v > 0);

// Préférer
const result = data
  .filter(item => item.value > 0)
  .map(item => item.value);
```

# Question de clarification
**Question:** Pourquoi utiliser cette approche plutôt que X ?
**Context:** Dans le contexte de Y, il pourrait être plus approprié de...

# Problème de sécurité
**Sévérité:** Critique/Majeure/Mineure
**Problème:** Description du problème de sécurité
**Solution:** Recommandation pour corriger
**Référence:** Lien vers la documentation de sécurité
```

## Outils et Configuration

### Configuration des Outils de Développement

**Package.json Scripts:**
```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "test": "vitest",
    "test:coverage": "vitest --coverage",
    "lint": "eslint src --ext .ts,.tsx",
    "lint:fix": "eslint src --ext .ts,.tsx --fix",
    "format": "prettier --write src/**/*.{ts,tsx}",
    "type-check": "tsc --noEmit"
  }
}
```

**Pre-commit Hooks (Husky):**
```json
// .husky/pre-commit
#!/usr/bin/env sh
. "$(dirname -- "$0")/_/husky.sh"

npm run lint
npm run type-check
npm run test:coverage
```

**Configuration VS Code:**
```json
// .vscode/settings.json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true
  },
  "typescript.preferences.importModuleSpecifier": "relative",
  "files.exclude": {
    "**/node_modules": true,
    "**/dist": true,
    "**/.git": true
  }
}
```

Ces standards garantissent un code de qualité, sécurisé et maintenable pour la migration Essensys. Ils doivent être appliqués de manière cohérente par toute l'équipe de développement.