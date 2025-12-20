# Stratégie de Tests - Migration Essensys

## Vue d'Ensemble

Ce document définit la stratégie complète de tests pour la migration Essensys, incluant les types de tests, les frameworks utilisés, les objectifs de couverture et les templates d'implémentation. L'objectif est d'assurer la qualité, la fiabilité et la parité fonctionnelle entre l'ancien et le nouveau système.

## Types de Tests et Frameworks

### Tests Unitaires

**Objectif:** Tester les unités de code individuelles (fonctions, classes, composants) de manière isolée.

**Frameworks:**
- **Frontend:** Vitest + React Testing Library + Jest DOM
- **Backend:** Jest + Supertest pour les tests d'API

**Couverture Cible:** 90% minimum

**Scope:**
- Fonctions utilitaires
- Hooks React personnalisés
- Services métier
- Composants React isolés
- Middleware Express
- Fonctions de validation

### Tests d'Intégration

**Objectif:** Tester l'interaction entre plusieurs composants ou services.

**Frameworks:**
- **Frontend:** Vitest + MSW (Mock Service Worker)
- **Backend:** Jest + Supertest + Base de données de test

**Couverture Cible:** 80% des flux critiques

**Scope:**
- Intégration API Frontend-Backend
- Flux d'authentification complets
- Interactions avec la base de données
- Communication WebSocket
- Intégration avec services externes (mocks)

### Tests End-to-End (E2E)

**Objectif:** Tester les parcours utilisateur complets dans un environnement proche de la production.

**Frameworks:**
- **Cypress** pour les tests E2E principaux
- **Playwright** pour les tests cross-browser (optionnel)

**Couverture Cible:** 100% des parcours critiques utilisateur

**Scope:**
- Parcours d'authentification
- Contrôle des appareils IoT
- Gestion des paramètres utilisateur
- Notifications et alertes
- Compatibilité mobile

### Tests de Performance

**Objectif:** Valider les performances et la scalabilité du système.

**Frameworks:**
- **Artillery** pour les tests de charge API
- **Lighthouse CI** pour les performances frontend
- **k6** pour les tests de stress (optionnel)

**Métriques Cibles:**
- Temps de réponse API < 200ms (95e percentile)
- First Contentful Paint < 1.5s
- Time to Interactive < 3s
- Throughput > 1000 req/s

### Tests de Compatibilité Hardware

**Objectif:** Valider la communication avec les boîtiers IoT existants.

**Frameworks:**
- **Jest** avec simulateurs de boîtiers
- **Docker** pour environnements de test isolés

**Scope:**
- Protocoles de communication
- Formats de données
- Gestion des timeouts
- Compatibilité firmware

## Configuration des Frameworks de Test

### Frontend - Vitest + React Testing Library

**vitest.config.ts:**
```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    globals: true,
    css: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      exclude: [
        'node_modules/',
        'src/setupTests.ts',
        'src/main.tsx',
        'src/vite-env.d.ts',
        '**/*.d.ts',
        '**/*.config.*',
        '**/coverage/**'
      ],
      thresholds: {
        global: {
          branches: 85,
          functions: 90,
          lines: 90,
          statements: 90
        },
        // Seuils spécifiques par dossier
        'src/components/': {
          branches: 90,
          functions: 95,
          lines: 95,
          statements: 95
        },
        'src/hooks/': {
          branches: 95,
          functions: 95,
          lines: 95,
          statements: 95
        }
      }
    },
    // Configuration pour les tests parallèles
    pool: 'threads',
    poolOptions: {
      threads: {
        singleThread: false,
        maxThreads: 4
      }
    }
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@components': resolve(__dirname, 'src/components'),
      '@hooks': resolve(__dirname, 'src/hooks'),
      '@services': resolve(__dirname, 'src/services'),
      '@store': resolve(__dirname, 'src/store'),
      '@types': resolve(__dirname, 'src/types'),
      '@utils': resolve(__dirname, 'src/utils')
    }
  }
});
```

**setupTests.ts:**
```typescript
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, beforeAll, afterAll, vi } from 'vitest';
import { server } from './mocks/server';

// Configuration globale des mocks
beforeAll(() => {
  // Démarrer le serveur MSW
  server.listen({ onUnhandledRequest: 'error' });
  
  // Mock des APIs du navigateur
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  });

  // Mock de localStorage
  const localStorageMock = {
    getItem: vi.fn(),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  };
  Object.defineProperty(window, 'localStorage', {
    value: localStorageMock
  });

  // Mock de WebSocket
  global.WebSocket = vi.fn().mockImplementation(() => ({
    close: vi.fn(),
    send: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
  }));
});

afterEach(() => {
  // Nettoyer après chaque test
  cleanup();
  server.resetHandlers();
  vi.clearAllMocks();
});

afterAll(() => {
  // Arrêter le serveur MSW
  server.close();
});
```

### Backend - Jest + Supertest

**jest.config.js:**
```javascript
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  roots: ['<rootDir>/src'],
  testMatch: [
    '**/__tests__/**/*.test.ts',
    '**/?(*.)+(spec|test).ts'
  ],
  transform: {
    '^.+\\.ts$': 'ts-jest',
  },
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/**/*.d.ts',
    '!src/index.ts',
    '!src/**/*.test.ts',
    '!src/**/*.spec.ts'
  ],
  coverageDirectory: 'coverage',
  coverageReporters: ['text', 'lcov', 'html'],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 85,
      lines: 85,
      statements: 85
    },
    // Seuils spécifiques
    'src/services/': {
      branches: 90,
      functions: 95,
      lines: 95,
      statements: 95
    },
    'src/controllers/': {
      branches: 85,
      functions: 90,
      lines: 90,
      statements: 90
    }
  },
  setupFilesAfterEnv: ['<rootDir>/src/__tests__/setup.ts'],
  testTimeout: 10000,
  // Configuration pour les tests parallèles
  maxWorkers: '50%',
  // Variables d'environnement pour les tests
  testEnvironmentOptions: {
    NODE_ENV: 'test'
  }
};
```

**setup.ts (Backend):**
```typescript
import { PrismaClient } from '@prisma/client';
import Redis from 'ioredis';

// Configuration de la base de données de test
const prisma = new PrismaClient({
  datasources: {
    db: {
      url: process.env.TEST_DATABASE_URL || 'postgresql://test:test@localhost:5433/essensys_test'
    }
  }
});

// Configuration Redis de test
const redis = new Redis({
  host: process.env.TEST_REDIS_HOST || 'localhost',
  port: parseInt(process.env.TEST_REDIS_PORT || '6380'),
  db: 1 // Base de données séparée pour les tests
});

// Nettoyage avant chaque test
beforeEach(async () => {
  // Nettoyer la base de données
  await prisma.$transaction([
    prisma.deviceState.deleteMany(),
    prisma.action.deleteMany(),
    prisma.device.deleteMany(),
    prisma.userMachine.deleteMany(),
    prisma.machine.deleteMany(),
    prisma.user.deleteMany()
  ]);
  
  // Nettoyer Redis
  await redis.flushdb();
});

// Nettoyage après tous les tests
afterAll(async () => {
  await prisma.$disconnect();
  await redis.quit();
});

// Export pour utilisation dans les tests
export { prisma, redis };
```

## Templates et Exemples de Tests

### Tests Unitaires Frontend

**Composant React:**
```typescript
// src/components/device/HeatingControl.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { HeatingControl } from './HeatingControl';
import { devicesSlice } from '@store/slices/devices-slice';
import { Device, HeatingState } from '@types/device';

// Mock du hook useDeviceActions
vi.mock('@hooks/useDeviceActions', () => ({
  useDeviceActions: vi.fn(() => ({
    executeAction: vi.fn(),
    isLoading: false,
    error: null
  }))
}));

describe('HeatingControl', () => {
  const mockDevice: Device = {
    id: 'device-1',
    name: 'Chauffage Salon',
    zone: 'salon',
    machineId: 'machine-1',
    deviceType: {
      id: 'type-1',
      name: 'heating_zone',
      category: 'climate'
    },
    isActive: true,
    config: {},
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockState: HeatingState = {
    currentTemperature: 20.5,
    targetTemperature: 22,
    mode: 'comfort',
    isHeating: true
  };

  const mockOnStateChange = vi.fn();

  const renderWithStore = (initialState = {}) => {
    const store = configureStore({
      reducer: {
        devices: devicesSlice.reducer
      },
      preloadedState: initialState
    });

    return render(
      <Provider store={store}>
        <HeatingControl
          device={mockDevice}
          currentState={mockState}
          onStateChange={mockOnStateChange}
        />
      </Provider>
    );
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render device name and zone', () => {
    renderWithStore();
    
    expect(screen.getByText('Chauffage Salon')).toBeInTheDocument();
    expect(screen.getByText('salon')).toBeInTheDocument();
  });

  it('should display current temperature', () => {
    renderWithStore();
    
    expect(screen.getByText('20.5°C')).toBeInTheDocument();
    expect(screen.getByText('Température actuelle')).toBeInTheDocument();
  });

  it('should allow temperature adjustment', async () => {
    const mockExecuteAction = vi.fn().mockResolvedValue({});
    vi.mocked(useDeviceActions).mockReturnValue({
      executeAction: mockExecuteAction,
      isLoading: false,
      error: null,
      clearError: vi.fn()
    });

    renderWithStore();
    
    const slider = screen.getByRole('slider');
    fireEvent.change(slider, { target: { value: '24' } });
    
    // Simuler la fin du glissement
    fireEvent.mouseUp(slider);
    
    await waitFor(() => {
      expect(mockExecuteAction).toHaveBeenCalledWith({
        type: 'set_temperature',
        payload: { targetTemperature: 24 }
      });
    });
  });

  it('should handle mode changes', async () => {
    const mockExecuteAction = vi.fn().mockResolvedValue({});
    vi.mocked(useDeviceActions).mockReturnValue({
      executeAction: mockExecuteAction,
      isLoading: false,
      error: null,
      clearError: vi.fn()
    });

    renderWithStore();
    
    const ecoButton = screen.getByText('Eco');
    fireEvent.click(ecoButton);
    
    await waitFor(() => {
      expect(mockExecuteAction).toHaveBeenCalledWith({
        type: 'set_mode',
        payload: { mode: 'eco' }
      });
    });
  });

  it('should disable controls when loading', () => {
    vi.mocked(useDeviceActions).mockReturnValue({
      executeAction: vi.fn(),
      isLoading: true,
      error: null,
      clearError: vi.fn()
    });

    renderWithStore();
    
    const slider = screen.getByRole('slider');
    const buttons = screen.getAllByRole('button');
    
    expect(slider).toBeDisabled();
    buttons.forEach(button => {
      expect(button).toBeDisabled();
    });
  });

  it('should handle errors gracefully', async () => {
    const mockExecuteAction = vi.fn().mockRejectedValue(new Error('Network error'));
    vi.mocked(useDeviceActions).mockReturnValue({
      executeAction: mockExecuteAction,
      isLoading: false,
      error: 'Network error',
      clearError: vi.fn()
    });

    renderWithStore();
    
    const slider = screen.getByRole('slider');
    fireEvent.change(slider, { target: { value: '25' } });
    fireEvent.mouseUp(slider);
    
    await waitFor(() => {
      // Vérifier que la température est revenue à la valeur précédente
      expect(slider).toHaveValue('22');
    });
  });
});
```

**Hook personnalisé:**
```typescript
// src/hooks/useDeviceActions.test.ts
import { renderHook, act } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { useDeviceActions } from './useDeviceActions';
import { deviceService } from '@services/device-service';

// Mock du service
vi.mock('@services/device-service');

describe('useDeviceActions', () => {
  const mockStore = configureStore({
    reducer: {
      actions: (state = { items: [] }) => state,
      notifications: (state = { items: [] }) => state
    }
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <Provider store={mockStore}>{children}</Provider>
  );

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should execute action successfully', async () => {
    const mockResult = { actionId: 'action-1' };
    vi.mocked(deviceService.executeAction).mockResolvedValue(mockResult);

    const { result } = renderHook(() => useDeviceActions('device-1'), { wrapper });

    await act(async () => {
      const response = await result.current.executeAction({
        type: 'set_temperature',
        payload: { targetTemperature: 22 }
      });
      expect(response).toEqual(mockResult);
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it('should handle errors', async () => {
    const mockError = new Error('API Error');
    vi.mocked(deviceService.executeAction).mockRejectedValue(mockError);

    const { result } = renderHook(() => useDeviceActions('device-1'), { wrapper });

    await act(async () => {
      try {
        await result.current.executeAction({
          type: 'set_temperature',
          payload: { targetTemperature: 22 }
        });
      } catch (error) {
        expect(error).toBe(mockError);
      }
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe('API Error');
  });

  it('should clear error', () => {
    const { result } = renderHook(() => useDeviceActions('device-1'), { wrapper });

    act(() => {
      result.current.clearError();
    });

    expect(result.current.error).toBeNull();
  });
});
```

### Tests Unitaires Backend

**Service métier:**
```typescript
// src/services/device-service.test.ts
import { DeviceService } from './device-service';
import { PrismaClient } from '@prisma/client';
import { CacheService } from './cache-service';
import { AuditService } from './audit-service';
import { prisma } from '../__tests__/setup';

// Mocks
const mockCacheService = {
  get: vi.fn(),
  set: vi.fn(),
  delete: vi.fn(),
  deletePattern: vi.fn()
} as jest.Mocked<CacheService>;

const mockAuditService = {
  log: vi.fn()
} as jest.Mocked<AuditService>;

describe('DeviceService', () => {
  let deviceService: DeviceService;

  beforeEach(() => {
    deviceService = new DeviceService(prisma, mockCacheService, mockAuditService);
    vi.clearAllMocks();
  });

  describe('getDevicesByMachine', () => {
    it('should return cached devices if available', async () => {
      const cachedDevices = {
        devices: [{ id: 'device-1', name: 'Test Device' }],
        total: 1,
        page: 1,
        limit: 20
      };
      
      mockCacheService.get.mockResolvedValue(cachedDevices);

      const result = await deviceService.getDevicesByMachine('machine-1', {
        page: 1,
        limit: 20
      });

      expect(result).toEqual(cachedDevices);
      expect(mockCacheService.get).toHaveBeenCalledWith(
        expect.stringContaining('devices:machine-1:')
      );
    });

    it('should fetch from database and cache if not cached', async () => {
      mockCacheService.get.mockResolvedValue(null);

      // Créer des données de test
      const machine = await prisma.machine.create({
        data: {
          id: 'machine-1',
          serialNumber: 'SN001',
          activationKey: 'KEY001',
          activationKeyHash: 'hash001'
        }
      });

      const deviceType = await prisma.deviceType.create({
        data: {
          name: 'heating_zone',
          displayName: 'Zone de Chauffage',
          category: 'climate'
        }
      });

      const device = await prisma.device.create({
        data: {
          machineId: machine.id,
          deviceTypeId: deviceType.id,
          name: 'Test Device',
          zone: 'salon'
        }
      });

      const result = await deviceService.getDevicesByMachine('machine-1', {
        page: 1,
        limit: 20
      });

      expect(result.devices).toHaveLength(1);
      expect(result.devices[0].name).toBe('Test Device');
      expect(mockCacheService.set).toHaveBeenCalled();
    });

    it('should filter by category', async () => {
      mockCacheService.get.mockResolvedValue(null);

      // Créer des données de test avec différentes catégories
      const machine = await prisma.machine.create({
        data: {
          id: 'machine-1',
          serialNumber: 'SN001',
          activationKey: 'KEY001',
          activationKeyHash: 'hash001'
        }
      });

      const heatingType = await prisma.deviceType.create({
        data: {
          name: 'heating_zone',
          displayName: 'Zone de Chauffage',
          category: 'climate'
        }
      });

      const securityType = await prisma.deviceType.create({
        data: {
          name: 'alarm_system',
          displayName: 'Système d\'Alarme',
          category: 'security'
        }
      });

      await prisma.device.createMany({
        data: [
          {
            machineId: machine.id,
            deviceTypeId: heatingType.id,
            name: 'Chauffage',
            zone: 'salon'
          },
          {
            machineId: machine.id,
            deviceTypeId: securityType.id,
            name: 'Alarme',
            zone: 'entree'
          }
        ]
      });

      const result = await deviceService.getDevicesByMachine('machine-1', {
        page: 1,
        limit: 20,
        category: 'climate'
      });

      expect(result.devices).toHaveLength(1);
      expect(result.devices[0].name).toBe('Chauffage');
    });
  });

  describe('createDevice', () => {
    it('should create device and invalidate cache', async () => {
      const machine = await prisma.machine.create({
        data: {
          id: 'machine-1',
          serialNumber: 'SN001',
          activationKey: 'KEY001',
          activationKeyHash: 'hash001'
        }
      });

      const deviceType = await prisma.deviceType.create({
        data: {
          name: 'heating_zone',
          displayName: 'Zone de Chauffage',
          category: 'climate'
        }
      });

      const deviceData = {
        machineId: machine.id,
        deviceTypeId: deviceType.id,
        name: 'Nouveau Chauffage',
        zone: 'chambre',
        config: { targetTemperature: 20 },
        userId: 'user-1'
      };

      const result = await deviceService.createDevice(deviceData);

      expect(result.name).toBe('Nouveau Chauffage');
      expect(result.zone).toBe('chambre');
      expect(mockCacheService.deletePattern).toHaveBeenCalledWith(
        `devices:${machine.id}:*`
      );
      expect(mockAuditService.log).toHaveBeenCalledWith({
        action: 'device_created',
        resourceType: 'device',
        resourceId: result.id,
        newValues: result,
        userId: 'user-1'
      });
    });
  });
});
```

**Contrôleur API:**
```typescript
// src/controllers/device-controller.test.ts
import request from 'supertest';
import { app } from '../app';
import { prisma } from '../__tests__/setup';
import jwt from 'jsonwebtoken';

describe('DeviceController', () => {
  let authToken: string;
  let machineId: string;
  let deviceId: string;

  beforeEach(async () => {
    // Créer un utilisateur et une machine de test
    const user = await prisma.user.create({
      data: {
        email: 'test@example.com',
        passwordHash: 'hashedpassword',
        firstName: 'Test',
        lastName: 'User'
      }
    });

    const machine = await prisma.machine.create({
      data: {
        serialNumber: 'SN001',
        activationKey: 'KEY001',
        activationKeyHash: 'hash001'
      }
    });

    machineId = machine.id;

    await prisma.userMachine.create({
      data: {
        userId: user.id,
        machineId: machine.id,
        role: 'owner'
      }
    });

    // Générer un token JWT
    authToken = jwt.sign(
      { userId: user.id, email: user.email, machineIds: [machine.id] },
      process.env.JWT_SECRET || 'test-secret'
    );

    const deviceType = await prisma.deviceType.create({
      data: {
        name: 'heating_zone',
        displayName: 'Zone de Chauffage',
        category: 'climate'
      }
    });

    const device = await prisma.device.create({
      data: {
        machineId: machine.id,
        deviceTypeId: deviceType.id,
        name: 'Test Device',
        zone: 'salon'
      }
    });

    deviceId = device.id;
  });

  describe('GET /api/machines/:machineId/devices', () => {
    it('should return devices for authorized user', async () => {
      const response = await request(app)
        .get(`/api/machines/${machineId}/devices`)
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(response.body.data).toHaveLength(1);
      expect(response.body.data[0].name).toBe('Test Device');
    });

    it('should return 401 without token', async () => {
      await request(app)
        .get(`/api/machines/${machineId}/devices`)
        .expect(401);
    });

    it('should return 403 for unauthorized machine', async () => {
      const otherMachine = await prisma.machine.create({
        data: {
          serialNumber: 'SN002',
          activationKey: 'KEY002',
          activationKeyHash: 'hash002'
        }
      });

      await request(app)
        .get(`/api/machines/${otherMachine.id}/devices`)
        .set('Authorization', `Bearer ${authToken}`)
        .expect(403);
    });

    it('should support pagination', async () => {
      const response = await request(app)
        .get(`/api/machines/${machineId}/devices?page=1&limit=10`)
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body.pagination).toEqual({
        page: 1,
        limit: 10,
        total: 1,
        totalPages: 1
      });
    });
  });

  describe('POST /api/machines/:machineId/devices', () => {
    it('should create device with valid data', async () => {
      const deviceType = await prisma.deviceType.findFirst({
        where: { name: 'heating_zone' }
      });

      const deviceData = {
        deviceTypeId: deviceType!.id,
        name: 'Nouveau Chauffage',
        zone: 'chambre',
        config: { targetTemperature: 20 }
      };

      const response = await request(app)
        .post(`/api/machines/${machineId}/devices`)
        .set('Authorization', `Bearer ${authToken}`)
        .send(deviceData)
        .expect(201);

      expect(response.body.success).toBe(true);
      expect(response.body.data.name).toBe('Nouveau Chauffage');
    });

    it('should return 400 with invalid data', async () => {
      const response = await request(app)
        .post(`/api/machines/${machineId}/devices`)
        .set('Authorization', `Bearer ${authToken}`)
        .send({ name: '' }) // Nom vide invalide
        .expect(400);

      expect(response.body.success).toBe(false);
      expect(response.body.error).toBe('Données invalides');
    });
  });

  describe('POST /api/devices/:deviceId/actions', () => {
    it('should execute action with valid data', async () => {
      const actionData = {
        type: 'set_temperature',
        payload: { targetTemperature: 22 }
      };

      const response = await request(app)
        .post(`/api/devices/${deviceId}/actions`)
        .set('Authorization', `Bearer ${authToken}`)
        .send(actionData)
        .expect(201);

      expect(response.body.success).toBe(true);
      expect(response.body.data.actionType).toBe('set_temperature');
    });
  });
});
```

### Tests d'Intégration

**Test d'intégration Frontend-Backend:**
```typescript
// src/__tests__/integration/device-management.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { setupServer } from 'msw/node';
import { rest } from 'msw';
import { store } from '@store';
import { DeviceManagementPage } from '@pages/DeviceManagement';

// Configuration MSW pour simuler l'API
const server = setupServer(
  rest.get('/api/machines/:machineId/devices', (req, res, ctx) => {
    return res(ctx.json({
      success: true,
      data: [
        {
          id: 'device-1',
          name: 'Chauffage Salon',
          zone: 'salon',
          deviceType: { name: 'heating_zone', category: 'climate' },
          isActive: true
        }
      ],
      pagination: { page: 1, limit: 20, total: 1, totalPages: 1 }
    }));
  }),

  rest.post('/api/devices/:deviceId/actions', (req, res, ctx) => {
    return res(ctx.json({
      success: true,
      data: {
        id: 'action-1',
        actionType: 'set_temperature',
        status: 'pending'
      }
    }));
  }),

  rest.get('/api/devices/:deviceId/state', (req, res, ctx) => {
    return res(ctx.json({
      success: true,
      data: {
        currentTemperature: 20.5,
        targetTemperature: 22,
        mode: 'comfort',
        isHeating: true
      }
    }));
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Device Management Integration', () => {
  const renderWithProviders = (component: React.ReactElement) => {
    return render(
      <Provider store={store}>
        <BrowserRouter>
          {component}
        </BrowserRouter>
      </Provider>
    );
  };

  it('should load and display devices', async () => {
    renderWithProviders(<DeviceManagementPage />);

    // Attendre que les appareils se chargent
    await waitFor(() => {
      expect(screen.getByText('Chauffage Salon')).toBeInTheDocument();
    });

    expect(screen.getByText('salon')).toBeInTheDocument();
  });

  it('should execute device action and update state', async () => {
    renderWithProviders(<DeviceManagementPage />);

    // Attendre le chargement
    await waitFor(() => {
      expect(screen.getByText('Chauffage Salon')).toBeInTheDocument();
    });

    // Cliquer sur le bouton de contrôle
    const controlButton = screen.getByText('Contrôler');
    fireEvent.click(controlButton);

    // Changer la température
    const temperatureSlider = screen.getByRole('slider');
    fireEvent.change(temperatureSlider, { target: { value: '24' } });
    fireEvent.mouseUp(temperatureSlider);

    // Vérifier que l'action a été envoyée
    await waitFor(() => {
      expect(screen.getByText('Action envoyée avec succès')).toBeInTheDocument();
    });
  });

  it('should handle API errors gracefully', async () => {
    // Simuler une erreur API
    server.use(
      rest.get('/api/machines/:machineId/devices', (req, res, ctx) => {
        return res(ctx.status(500), ctx.json({
          success: false,
          error: 'Erreur serveur'
        }));
      })
    );

    renderWithProviders(<DeviceManagementPage />);

    await waitFor(() => {
      expect(screen.getByText('Erreur de connexion au serveur')).toBeInTheDocument();
    });
  });
});
```

### Tests End-to-End avec Cypress

**cypress/e2e/device-control.cy.ts:**
```typescript
describe('Device Control E2E', () => {
  beforeEach(() => {
    // Connexion utilisateur
    cy.login('test@example.com', 'password');
    cy.visit('/dashboard');
  });

  it('should control heating device', () => {
    // Naviguer vers le contrôle des appareils
    cy.get('[data-testid="devices-menu"]').click();
    cy.get('[data-testid="heating-devices"]').click();

    // Sélectionner un appareil de chauffage
    cy.get('[data-testid="device-card"]').first().click();

    // Vérifier l'affichage de l'état actuel
    cy.get('[data-testid="current-temperature"]').should('contain', '°C');
    cy.get('[data-testid="target-temperature"]').should('be.visible');

    // Changer la température cible
    cy.get('[data-testid="temperature-slider"]')
      .invoke('val', 24)
      .trigger('change')
      .trigger('mouseup');

    // Vérifier la confirmation
    cy.get('[data-testid="success-notification"]')
      .should('contain', 'Température mise à jour');

    // Changer le mode
    cy.get('[data-testid="mode-eco"]').click();
    
    cy.get('[data-testid="success-notification"]')
      .should('contain', 'Mode mis à jour');
  });

  it('should handle device offline state', () => {
    // Simuler un appareil hors ligne
    cy.intercept('GET', '/api/devices/*/state', {
      statusCode: 503,
      body: { success: false, error: 'Appareil hors ligne' }
    });

    cy.get('[data-testid="devices-menu"]').click();
    cy.get('[data-testid="device-card"]').first().click();

    // Vérifier l'affichage de l'état hors ligne
    cy.get('[data-testid="device-offline"]').should('be.visible');
    cy.get('[data-testid="temperature-slider"]').should('be.disabled');
  });

  it('should work on mobile devices', () => {
    cy.viewport('iphone-x');
    
    cy.get('[data-testid="mobile-menu"]').click();
    cy.get('[data-testid="devices-menu"]').click();
    
    // Vérifier l'affichage mobile
    cy.get('[data-testid="device-list"]').should('be.visible');
    cy.get('[data-testid="device-card"]').should('have.class', 'mobile-layout');
  });
});
```

**Commandes Cypress personnalisées:**
```typescript
// cypress/support/commands.ts
declare global {
  namespace Cypress {
    interface Chainable {
      login(email: string, password: string): Chainable<void>;
      mockDeviceState(deviceId: string, state: any): Chainable<void>;
    }
  }
}

Cypress.Commands.add('login', (email: string, password: string) => {
  cy.request({
    method: 'POST',
    url: '/api/auth/login',
    body: { email, password }
  }).then((response) => {
    window.localStorage.setItem('accessToken', response.body.data.accessToken);
    window.localStorage.setItem('refreshToken', response.body.data.refreshToken);
  });
});

Cypress.Commands.add('mockDeviceState', (deviceId: string, state: any) => {
  cy.intercept('GET', `/api/devices/${deviceId}/state`, {
    statusCode: 200,
    body: { success: true, data: state }
  });
});
```

## Métriques de Qualité et Objectifs de Couverture

### Objectifs de Couverture par Type

**Tests Unitaires:**
- **Frontend Components:** 95% (branches, fonctions, lignes)
- **Frontend Hooks:** 95% (branches, fonctions, lignes)
- **Frontend Utils:** 90% (branches, fonctions, lignes)
- **Backend Services:** 95% (branches, fonctions, lignes)
- **Backend Controllers:** 90% (branches, fonctions, lignes)
- **Backend Utils:** 90% (branches, fonctions, lignes)

**Tests d'Intégration:**
- **API Endpoints:** 100% des endpoints critiques
- **Database Operations:** 90% des opérations CRUD
- **Authentication Flows:** 100% des flux d'authentification
- **WebSocket Events:** 80% des événements temps réel

**Tests E2E:**
- **User Journeys:** 100% des parcours critiques
- **Device Control:** 100% des fonctionnalités de contrôle
- **Error Scenarios:** 80% des scénarios d'erreur
- **Mobile Compatibility:** 100% des fonctionnalités principales

### Métriques de Performance

**Frontend:**
- First Contentful Paint < 1.5s
- Largest Contentful Paint < 2.5s
- Time to Interactive < 3s
- Cumulative Layout Shift < 0.1

**Backend:**
- Response Time (95e percentile) < 200ms
- Throughput > 1000 req/s
- Error Rate < 0.1%
- Database Query Time < 50ms

### Pipeline de Tests CI/CD

**GitHub Actions Workflow:**
```yaml
# .github/workflows/test.yml
name: Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '18.18.0'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json
      
      - name: Install dependencies
        run: cd frontend && npm ci
      
      - name: Run unit tests
        run: cd frontend && npm run test:coverage
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: frontend/coverage/lcov.info
          flags: frontend

  test-backend:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15.5
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_DB: essensys_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      redis:
        image: redis:7.2
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '18.18.0'
          cache: 'npm'
          cache-dependency-path: backend/package-lock.json
      
      - name: Install dependencies
        run: cd backend && npm ci
      
      - name: Run migrations
        run: cd backend && npx prisma migrate deploy
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_test
      
      - name: Run unit tests
        run: cd backend && npm run test:coverage
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_test
          REDIS_URL: redis://localhost:6379
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: backend/coverage/lcov.info
          flags: backend

  test-e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '18.18.0'
      
      - name: Start services
        run: docker-compose -f docker-compose.test.yml up -d
      
      - name: Wait for services
        run: |
          timeout 60 bash -c 'until curl -f http://localhost:3000; do sleep 2; done'
          timeout 60 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'
      
      - name: Run Cypress tests
        uses: cypress-io/github-action@v6
        with:
          working-directory: frontend
          browser: chrome
          record: true
        env:
          CYPRESS_RECORD_KEY: ${{ secrets.CYPRESS_RECORD_KEY }}
      
      - name: Stop services
        run: docker-compose -f docker-compose.test.yml down
```

Cette stratégie de tests complète garantit la qualité, la fiabilité et la parité fonctionnelle du nouveau système Essensys avec l'ancien système legacy.