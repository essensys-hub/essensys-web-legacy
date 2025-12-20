# Architecture Frontend React - Essensys Migration

## Vue d'Ensemble

Cette documentation détaille l'architecture frontend React/TypeScript pour la migration d'Essensys, définissant la structure des composants, les patterns de state management, la configuration TypeScript et les outils de build.

## Structure des Composants et Hiérarchie des Dossiers

### Structure de Projet Recommandée

```
essensys-frontend/
├── public/
│   ├── index.html
│   ├── favicon.ico
│   └── manifest.json
├── src/
│   ├── components/           # Composants réutilisables
│   │   ├── common/          # Composants génériques
│   │   │   ├── Button/
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Button.test.tsx
│   │   │   │   ├── Button.stories.tsx
│   │   │   │   └── index.ts
│   │   │   ├── Input/
│   │   │   ├── Modal/
│   │   │   ├── Loading/
│   │   │   ├── ErrorBoundary/
│   │   │   └── Layout/
│   │   ├── device/          # Composants spécifiques aux appareils
│   │   │   ├── DeviceCard/
│   │   │   ├── DeviceControl/
│   │   │   ├── HeatingControl/
│   │   │   ├── ShutterControl/
│   │   │   ├── AlarmControl/
│   │   │   └── DeviceStatus/
│   │   ├── auth/            # Composants d'authentification
│   │   │   ├── LoginForm/
│   │   │   ├── RegisterForm/
│   │   │   ├── PasswordReset/
│   │   │   └── UserProfile/
│   │   └── charts/          # Composants de visualisation
│   │       ├── TemperatureChart/
│   │       ├── EnergyChart/
│   │       └── StatusChart/
│   ├── pages/               # Pages principales
│   │   ├── Dashboard/
│   │   │   ├── Dashboard.tsx
│   │   │   ├── Dashboard.test.tsx
│   │   │   └── index.ts
│   │   ├── DeviceControl/
│   │   ├── Settings/
│   │   ├── Login/
│   │   ├── Register/
│   │   └── NotFound/
│   ├── hooks/               # Custom React hooks
│   │   ├── useAuth.ts
│   │   ├── useDevices.ts
│   │   ├── useWebSocket.ts
│   │   ├── useLocalStorage.ts
│   │   └── useNotifications.ts
│   ├── services/            # Services API
│   │   ├── api/
│   │   │   ├── auth.ts
│   │   │   ├── devices.ts
│   │   │   ├── users.ts
│   │   │   └── notifications.ts
│   │   ├── websocket/
│   │   │   ├── websocket.ts
│   │   │   └── messageHandlers.ts
│   │   └── storage/
│   │       ├── localStorage.ts
│   │       └── sessionStorage.ts
│   ├── store/               # State management (Redux Toolkit)
│   │   ├── index.ts
│   │   ├── slices/
│   │   │   ├── authSlice.ts
│   │   │   ├── devicesSlice.ts
│   │   │   ├── uiSlice.ts
│   │   │   └── notificationsSlice.ts
│   │   ├── middleware/
│   │   │   ├── authMiddleware.ts
│   │   │   └── websocketMiddleware.ts
│   │   └── selectors/
│   │       ├── authSelectors.ts
│   │       └── deviceSelectors.ts
│   ├── types/               # Définitions TypeScript
│   │   ├── api.ts
│   │   ├── auth.ts
│   │   ├── devices.ts
│   │   ├── user.ts
│   │   └── common.ts
│   ├── utils/               # Utilitaires
│   │   ├── constants.ts
│   │   ├── formatters.ts
│   │   ├── validators.ts
│   │   ├── dateUtils.ts
│   │   └── errorHandling.ts
│   ├── styles/              # Styles globaux
│   │   ├── globals.css
│   │   ├── variables.css
│   │   └── themes/
│   │       ├── light.ts
│   │       └── dark.ts
│   ├── assets/              # Assets statiques
│   │   ├── images/
│   │   ├── icons/
│   │   └── fonts/
│   ├── config/              # Configuration
│   │   ├── env.ts
│   │   ├── api.ts
│   │   └── routes.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── tests/                   # Tests globaux
│   ├── setup.ts
│   ├── mocks/
│   └── utils/
├── docs/                    # Documentation
│   ├── components.md
│   ├── state-management.md
│   └── deployment.md
├── .env.example
├── .env.local
├── .gitignore
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
├── eslint.config.js
└── README.md
```

### Conventions de Nommage

**Composants:**
- PascalCase pour les noms de composants (`DeviceCard`, `LoginForm`)
- Dossier par composant avec fichier index.ts pour les exports
- Suffixes descriptifs (`Form`, `Modal`, `Card`, `Control`)

**Hooks:**
- Préfixe `use` suivi du nom descriptif (`useAuth`, `useDevices`)
- camelCase pour les noms de hooks

**Types:**
- PascalCase pour les interfaces et types (`User`, `Device`, `ApiResponse`)
- Suffixe `Props` pour les props de composants (`ButtonProps`)
- Suffixe `State` pour les états Redux (`AuthState`)

## Patterns de State Management (Redux Toolkit)

### Configuration du Store

```typescript
// src/store/index.ts
import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import authSlice from './slices/authSlice';
import devicesSlice from './slices/devicesSlice';
import uiSlice from './slices/uiSlice';
import notificationsSlice from './slices/notificationsSlice';
import { authMiddleware } from './middleware/authMiddleware';
import { websocketMiddleware } from './middleware/websocketMiddleware';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    devices: devicesSlice,
    ui: uiSlice,
    notifications: notificationsSlice,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['websocket/connect', 'websocket/disconnect'],
      },
    })
      .concat(authMiddleware)
      .concat(websocketMiddleware),
  devTools: process.env.NODE_ENV !== 'production',
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
```

### Slice d'Authentification

```typescript
// src/store/slices/authSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { authApi } from '../../services/api/auth';
import { User, LoginCredentials, AuthTokens } from '../../types/auth';

interface AuthState {
  user: User | null;
  tokens: AuthTokens | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  tokens: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
};

// Async thunks
export const loginUser = createAsyncThunk(
  'auth/login',
  async (credentials: LoginCredentials, { rejectWithValue }) => {
    try {
      const response = await authApi.login(credentials);
      // Stocker les tokens de manière sécurisée
      localStorage.setItem('refreshToken', response.refreshToken);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Erreur de connexion');
    }
  }
);

export const refreshToken = createAsyncThunk(
  'auth/refresh',
  async (_, { getState, rejectWithValue }) => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        throw new Error('Aucun refresh token disponible');
      }
      const response = await authApi.refreshToken(refreshToken);
      return response;
    } catch (error: any) {
      localStorage.removeItem('refreshToken');
      return rejectWithValue(error.response?.data?.message || 'Erreur de rafraîchissement');
    }
  }
);

export const logoutUser = createAsyncThunk(
  'auth/logout',
  async (_, { getState }) => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken);
      } catch (error) {
        // Ignorer les erreurs de logout côté serveur
      }
    }
    localStorage.removeItem('refreshToken');
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    updateUser: (state, action: PayloadAction<Partial<User>>) => {
      if (state.user) {
        state.user = { ...state.user, ...action.payload };
      }
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.tokens = {
          accessToken: action.payload.accessToken,
          refreshToken: action.payload.refreshToken,
        };
        state.isAuthenticated = true;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
        state.isAuthenticated = false;
      })
      // Refresh token
      .addCase(refreshToken.fulfilled, (state, action) => {
        state.tokens = {
          accessToken: action.payload.accessToken,
          refreshToken: state.tokens?.refreshToken || '',
        };
      })
      .addCase(refreshToken.rejected, (state) => {
        state.user = null;
        state.tokens = null;
        state.isAuthenticated = false;
      })
      // Logout
      .addCase(logoutUser.fulfilled, (state) => {
        state.user = null;
        state.tokens = null;
        state.isAuthenticated = false;
      });
  },
});

export const { clearError, updateUser } = authSlice.actions;
export default authSlice.reducer;
```

### Slice des Appareils

```typescript
// src/store/slices/devicesSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { devicesApi } from '../../services/api/devices';
import { Device, DeviceAction, DeviceState } from '../../types/devices';

interface DevicesState {
  devices: Device[];
  selectedDevice: Device | null;
  isLoading: boolean;
  error: string | null;
  lastUpdate: string | null;
}

const initialState: DevicesState = {
  devices: [],
  selectedDevice: null,
  isLoading: false,
  error: null,
  lastUpdate: null,
};

export const fetchDevices = createAsyncThunk(
  'devices/fetchAll',
  async (machineId: string, { rejectWithValue }) => {
    try {
      const devices = await devicesApi.getDevices(machineId);
      return devices;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Erreur de chargement');
    }
  }
);

export const sendDeviceAction = createAsyncThunk(
  'devices/sendAction',
  async ({ deviceId, action }: { deviceId: string; action: DeviceAction }, { rejectWithValue }) => {
    try {
      const result = await devicesApi.sendAction(deviceId, action);
      return { deviceId, action: result };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Erreur d\'envoi de commande');
    }
  }
);

const devicesSlice = createSlice({
  name: 'devices',
  initialState,
  reducers: {
    selectDevice: (state, action: PayloadAction<string>) => {
      state.selectedDevice = state.devices.find(d => d.id === action.payload) || null;
    },
    updateDeviceState: (state, action: PayloadAction<{ deviceId: string; state: DeviceState }>) => {
      const device = state.devices.find(d => d.id === action.payload.deviceId);
      if (device) {
        device.currentState = action.payload.state;
        device.lastUpdate = new Date().toISOString();
      }
      state.lastUpdate = new Date().toISOString();
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDevices.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchDevices.fulfilled, (state, action) => {
        state.isLoading = false;
        state.devices = action.payload;
        state.lastUpdate = new Date().toISOString();
      })
      .addCase(fetchDevices.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(sendDeviceAction.fulfilled, (state, action) => {
        // Marquer l'action comme en cours
        const device = state.devices.find(d => d.id === action.payload.deviceId);
        if (device) {
          device.pendingActions = device.pendingActions || [];
          device.pendingActions.push(action.payload.action);
        }
      });
  },
});

export const { selectDevice, updateDeviceState, clearError } = devicesSlice.actions;
export default devicesSlice.reducer;
```

### Sélecteurs Réutilisables

```typescript
// src/store/selectors/deviceSelectors.ts
import { createSelector } from '@reduxjs/toolkit';
import { RootState } from '../index';
import { DeviceCategory } from '../../types/devices';

export const selectAllDevices = (state: RootState) => state.devices.devices;
export const selectSelectedDevice = (state: RootState) => state.devices.selectedDevice;
export const selectDevicesLoading = (state: RootState) => state.devices.isLoading;

export const selectDevicesByCategory = createSelector(
  [selectAllDevices, (_, category: DeviceCategory) => category],
  (devices, category) => devices.filter(device => device.deviceType.category === category)
);

export const selectOnlineDevices = createSelector(
  [selectAllDevices],
  (devices) => devices.filter(device => device.status === 'online')
);

export const selectDevicesWithPendingActions = createSelector(
  [selectAllDevices],
  (devices) => devices.filter(device => 
    device.pendingActions && device.pendingActions.length > 0
  )
);
```

## Configuration TypeScript avec Types Stricts

### Configuration tsconfig.json

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "allowJs": false,
    
    /* Bundler mode */
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    
    /* Linting strict */
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "noImplicitReturns": true,
    "noImplicitOverride": true,
    "noPropertyAccessFromIndexSignature": true,
    "noUncheckedIndexedAccess": true,
    "exactOptionalPropertyTypes": true,
    
    /* Path mapping */
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "@/components/*": ["./src/components/*"],
      "@/pages/*": ["./src/pages/*"],
      "@/hooks/*": ["./src/hooks/*"],
      "@/services/*": ["./src/services/*"],
      "@/store/*": ["./src/store/*"],
      "@/types/*": ["./src/types/*"],
      "@/utils/*": ["./src/utils/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

### Définitions de Types Strictes

```typescript
// src/types/common.ts
export type Nullable<T> = T | null;
export type Optional<T> = T | undefined;
export type ID = string;
export type Timestamp = string; // ISO 8601

export interface ApiResponse<T> {
  readonly data: T;
  readonly success: boolean;
  readonly message?: string;
}

export interface ApiError {
  readonly code: string;
  readonly message: string;
  readonly details?: Record<string, unknown>;
}

export interface PaginatedResponse<T> {
  readonly data: readonly T[];
  readonly pagination: {
    readonly page: number;
    readonly limit: number;
    readonly total: number;
    readonly totalPages: number;
  };
}

// src/types/devices.ts
export type DeviceStatus = 'online' | 'offline' | 'error' | 'maintenance';
export type DeviceCategory = 'climate' | 'security' | 'comfort';
export type ActionStatus = 'pending' | 'sent' | 'executed' | 'failed';

export interface DeviceType {
  readonly id: ID;
  readonly name: string;
  readonly displayName: string;
  readonly category: DeviceCategory;
  readonly icon?: string;
  readonly configSchema: Record<string, unknown>;
}

export interface Device {
  readonly id: ID;
  readonly machineId: ID;
  readonly deviceType: DeviceType;
  readonly name: string;
  readonly zone?: string;
  readonly config: Record<string, unknown>;
  readonly isActive: boolean;
  readonly status: DeviceStatus;
  readonly currentState?: DeviceState;
  readonly lastUpdate?: Timestamp;
  readonly pendingActions?: readonly DeviceAction[];
  readonly createdAt: Timestamp;
  readonly updatedAt: Timestamp;
}

export interface DeviceState {
  readonly [key: string]: unknown;
  readonly timestamp: Timestamp;
}

export interface DeviceAction {
  readonly id: ID;
  readonly actionType: string;
  readonly payload: Record<string, unknown>;
  readonly status: ActionStatus;
  readonly priority: number;
  readonly createdAt: Timestamp;
  readonly executedAt?: Timestamp;
  readonly errorMessage?: string;
}

// Types spécifiques par appareil
export interface HeatingState extends DeviceState {
  readonly temperature: number;
  readonly targetTemperature: number;
  readonly mode: 'off' | 'eco' | 'comfort' | 'auto';
  readonly humidity?: number;
}

export interface ShutterState extends DeviceState {
  readonly position: number; // 0-100
  readonly isMoving: boolean;
  readonly direction?: 'up' | 'down';
}

export interface AlarmState extends DeviceState {
  readonly armed: boolean;
  readonly zones: readonly string[];
  readonly lastTriggered?: Timestamp;
}
```

### Utilitaires de Type

```typescript
// src/types/utils.ts
export type DeepReadonly<T> = {
  readonly [P in keyof T]: T[P] extends object ? DeepReadonly<T[P]> : T[P];
};

export type NonEmptyArray<T> = [T, ...T[]];

export type RequiredKeys<T, K extends keyof T> = T & Required<Pick<T, K>>;

export type PartialExcept<T, K extends keyof T> = Partial<T> & Pick<T, K>;

// Type guards
export const isNotNull = <T>(value: T | null): value is T => value !== null;
export const isNotUndefined = <T>(value: T | undefined): value is T => value !== undefined;
export const isDefined = <T>(value: T | null | undefined): value is T => 
  value !== null && value !== undefined;

// Validation helpers
export const assertNever = (value: never): never => {
  throw new Error(`Unexpected value: ${value}`);
};

export const exhaustiveCheck = (value: never): never => {
  throw new Error(`Exhaustive check failed: ${value}`);
};
```

## Bibliothèques UI et Outils de Build (Vite)

### Configuration Vite

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': resolve(__dirname, './src'),
      '@/components': resolve(__dirname, './src/components'),
      '@/pages': resolve(__dirname, './src/pages'),
      '@/hooks': resolve(__dirname, './src/hooks'),
      '@/services': resolve(__dirname, './src/services'),
      '@/store': resolve(__dirname, './src/store'),
      '@/types': resolve(__dirname, './src/types'),
      '@/utils': resolve(__dirname, './src/utils'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:8000',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          redux: ['@reduxjs/toolkit', 'react-redux'],
          ui: ['@headlessui/react', '@heroicons/react'],
        },
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './tests/setup.ts',
  },
});
```

### Stack UI Recommandée

**Bibliothèques UI Principales:**
- **Headless UI** + **Tailwind CSS** : Composants accessibles et styling utilitaire
- **Heroicons** : Icônes cohérentes et optimisées
- **React Hook Form** : Gestion des formulaires performante
- **Zod** : Validation de schémas TypeScript-first

**Bibliothèques de Visualisation:**
- **Recharts** : Graphiques React natifs
- **React Query (TanStack Query)** : Gestion du cache et synchronisation serveur

**Utilitaires:**
- **date-fns** : Manipulation des dates
- **clsx** : Gestion conditionnelle des classes CSS
- **React Hot Toast** : Notifications toast

### Configuration des Dépendances

```json
{
  "name": "essensys-frontend",
  "private": true,
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest --coverage",
    "lint": "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0",
    "lint:fix": "eslint . --ext ts,tsx --fix",
    "type-check": "tsc --noEmit"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "@reduxjs/toolkit": "^2.0.1",
    "react-redux": "^9.0.4",
    "@tanstack/react-query": "^5.17.0",
    "react-router-dom": "^6.20.1",
    "react-hook-form": "^7.48.2",
    "@hookform/resolvers": "^3.3.2",
    "zod": "^3.22.4",
    "@headlessui/react": "^1.7.17",
    "@heroicons/react": "^2.0.18",
    "recharts": "^2.8.0",
    "date-fns": "^3.0.6",
    "clsx": "^2.0.0",
    "react-hot-toast": "^2.4.1",
    "axios": "^1.6.2"
  },
  "devDependencies": {
    "@types/react": "^18.2.43",
    "@types/react-dom": "^18.2.17",
    "@vitejs/plugin-react": "^4.2.1",
    "vite": "^5.0.8",
    "typescript": "^5.2.2",
    "@typescript-eslint/eslint-plugin": "^6.14.0",
    "@typescript-eslint/parser": "^6.14.0",
    "eslint": "^8.55.0",
    "eslint-plugin-react-hooks": "^4.6.0",
    "eslint-plugin-react-refresh": "^0.4.5",
    "tailwindcss": "^3.3.6",
    "autoprefixer": "^10.4.16",
    "postcss": "^8.4.32",
    "vitest": "^1.0.4",
    "@vitest/ui": "^1.0.4",
    "@testing-library/react": "^14.1.2",
    "@testing-library/jest-dom": "^6.1.5",
    "@testing-library/user-event": "^14.5.1",
    "jsdom": "^23.0.1"
  }
}
```

### Configuration Tailwind CSS

```javascript
// tailwind.config.js
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eff6ff',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
        },
        gray: {
          50: '#f9fafb',
          100: '#f3f4f6',
          500: '#6b7280',
          900: '#111827',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}
```

Cette architecture frontend React fournit une base solide et scalable pour la migration d'Essensys, avec des patterns modernes, une configuration TypeScript stricte et des outils de développement optimisés.