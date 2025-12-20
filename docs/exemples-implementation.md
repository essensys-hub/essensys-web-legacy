# Exemples d'Implémentation par Couche - Migration Essensys

## Vue d'Ensemble

Ce document fournit des exemples concrets d'implémentation pour chaque couche de l'architecture moderne Essensys. Ces templates servent de référence pour l'équipe de développement et garantissent la cohérence des patterns utilisés.

## Couche Frontend React

### Composants React - Exemples Concrets

**1. Composant de Contrôle de Chauffage:**

```typescript
// src/components/device/HeatingControl.tsx
import React, { useState, useCallback } from 'react';
import { Device, HeatingState } from '../../types/device';
import { useDeviceActions } from '../../hooks/useDeviceActions';
import { Card, Slider, Switch, Button } from '../common';

interface HeatingControlProps {
  device: Device;
  currentState: HeatingState;
  onStateChange: (newState: HeatingState) => void;
  disabled?: boolean;
}

export const HeatingControl: React.FC<HeatingControlProps> = ({
  device,
  currentState,
  onStateChange,
  disabled = false
}) => {
  const [localTemperature, setLocalTemperature] = useState(currentState.targetTemperature);
  const { executeAction, isLoading } = useDeviceActions(device.id);

  const handleTemperatureChange = useCallback((temperature: number) => {
    setLocalTemperature(temperature);
  }, []);

  const handleTemperatureCommit = useCallback(async () => {
    try {
      await executeAction({
        type: 'set_temperature',
        payload: { targetTemperature: localTemperature }
      });
      onStateChange({
        ...currentState,
        targetTemperature: localTemperature
      });
    } catch (error) {
      console.error('Erreur lors du changement de température:', error);
      // Reset to previous value on error
      setLocalTemperature(currentState.targetTemperature);
    }
  }, [localTemperature, currentState, executeAction, onStateChange]);

  const handleModeChange = useCallback(async (mode: HeatingState['mode']) => {
    try {
      await executeAction({
        type: 'set_mode',
        payload: { mode }
      });
      onStateChange({
        ...currentState,
        mode
      });
    } catch (error) {
      console.error('Erreur lors du changement de mode:', error);
    }
  }, [currentState, executeAction, onStateChange]);

  return (
    <Card className="heating-control">
      <div className="heating-control__header">
        <h3>{device.name}</h3>
        <span className="heating-control__zone">{device.zone}</span>
      </div>

      <div className="heating-control__current-temp">
        <span className="temperature-display">
          {currentState.currentTemperature}°C
        </span>
        <span className="temperature-label">Température actuelle</span>
      </div>

      <div className="heating-control__target">
        <label htmlFor={`temp-slider-${device.id}`}>
          Température cible: {localTemperature}°C
        </label>
        <Slider
          id={`temp-slider-${device.id}`}
          min={5}
          max={30}
          step={0.5}
          value={localTemperature}
          onChange={handleTemperatureChange}
          onChangeCommitted={handleTemperatureCommit}
          disabled={disabled || isLoading}
        />
      </div>

      <div className="heating-control__modes">
        <div className="mode-buttons">
          {(['off', 'eco', 'comfort', 'auto'] as const).map((mode) => (
            <Button
              key={mode}
              variant={currentState.mode === mode ? 'primary' : 'secondary'}
              onClick={() => handleModeChange(mode)}
              disabled={disabled || isLoading}
              className={`mode-button mode-button--${mode}`}
            >
              {mode.charAt(0).toUpperCase() + mode.slice(1)}
            </Button>
          ))}
        </div>
      </div>

      <div className="heating-control__status">
        <Switch
          checked={currentState.isHeating}
          disabled={true}
          label="Chauffage en cours"
        />
      </div>
    </Card>
  );
};

// Types associés
export interface HeatingState {
  currentTemperature: number;
  targetTemperature: number;
  mode: 'off' | 'eco' | 'comfort' | 'auto';
  isHeating: boolean;
  schedule?: HeatingSchedule;
}

interface HeatingSchedule {
  enabled: boolean;
  periods: Array<{
    start: string; // HH:mm
    end: string;   // HH:mm
    temperature: number;
    days: number[]; // 0-6 (dimanche-samedi)
  }>;
}
```

**2. Hook Personnalisé pour Actions d'Appareils:**

```typescript
// src/hooks/useDeviceActions.ts
import { useState, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { deviceService } from '../services/device-service';
import { addAction } from '../store/slices/actions-slice';
import { showNotification } from '../store/slices/notifications-slice';

interface DeviceAction {
  type: string;
  payload: Record<string, any>;
}

export const useDeviceActions = (deviceId: string) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dispatch = useDispatch();

  const executeAction = useCallback(async (action: DeviceAction) => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await deviceService.executeAction(deviceId, action);
      
      // Ajouter l'action au store pour suivi
      dispatch(addAction({
        id: result.actionId,
        deviceId,
        type: action.type,
        payload: action.payload,
        status: 'pending',
        createdAt: new Date().toISOString()
      }));

      dispatch(showNotification({
        type: 'success',
        message: 'Action envoyée avec succès',
        duration: 3000
      }));

      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Erreur inconnue';
      setError(errorMessage);
      
      dispatch(showNotification({
        type: 'error',
        message: `Erreur: ${errorMessage}`,
        duration: 5000
      }));
      
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, [deviceId, dispatch]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    executeAction,
    isLoading,
    error,
    clearError
  };
};
```

**3. Composant de Liste d'Appareils avec Virtualisation:**

```typescript
// src/components/device/DeviceList.tsx
import React, { useMemo } from 'react';
import { FixedSizeList as List } from 'react-window';
import { Device } from '../../types/device';
import { DeviceCard } from './DeviceCard';
import { LoadingSpinner } from '../common/LoadingSpinner';
import { EmptyState } from '../common/EmptyState';

interface DeviceListProps {
  devices: Device[];
  loading?: boolean;
  onDeviceSelect: (device: Device) => void;
  selectedDeviceId?: string;
  filter?: {
    category?: string;
    zone?: string;
    status?: 'active' | 'inactive';
  };
}

export const DeviceList: React.FC<DeviceListProps> = ({
  devices,
  loading = false,
  onDeviceSelect,
  selectedDeviceId,
  filter
}) => {
  const filteredDevices = useMemo(() => {
    if (!filter) return devices;

    return devices.filter(device => {
      if (filter.category && device.deviceType.category !== filter.category) {
        return false;
      }
      if (filter.zone && device.zone !== filter.zone) {
        return false;
      }
      if (filter.status && device.isActive !== (filter.status === 'active')) {
        return false;
      }
      return true;
    });
  }, [devices, filter]);

  if (loading) {
    return <LoadingSpinner message="Chargement des appareils..." />;
  }

  if (filteredDevices.length === 0) {
    return (
      <EmptyState
        title="Aucun appareil trouvé"
        description="Aucun appareil ne correspond aux critères de filtrage."
        action={{
          label: "Actualiser",
          onClick: () => window.location.reload()
        }}
      />
    );
  }

  const ItemRenderer = ({ index, style }: { index: number; style: React.CSSProperties }) => {
    const device = filteredDevices[index];
    return (
      <div style={style}>
        <DeviceCard
          device={device}
          isSelected={device.id === selectedDeviceId}
          onClick={() => onDeviceSelect(device)}
        />
      </div>
    );
  };

  return (
    <div className="device-list">
      <div className="device-list__header">
        <h2>Appareils ({filteredDevices.length})</h2>
      </div>
      
      <List
        height={600}
        itemCount={filteredDevices.length}
        itemSize={120}
        className="device-list__container"
      >
        {ItemRenderer}
      </List>
    </div>
  );
};
```

### State Management Redux Toolkit

**1. Slice pour la Gestion des Appareils:**

```typescript
// src/store/slices/devices-slice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Device, DeviceState } from '../../types/device';
import { deviceService } from '../../services/device-service';

interface DevicesState {
  items: Device[];
  currentStates: Record<string, DeviceState>;
  loading: boolean;
  error: string | null;
  selectedDeviceId: string | null;
}

const initialState: DevicesState = {
  items: [],
  currentStates: {},
  loading: false,
  error: null,
  selectedDeviceId: null
};

// Async Thunks
export const fetchDevices = createAsyncThunk(
  'devices/fetchDevices',
  async (machineId: string, { rejectWithValue }) => {
    try {
      const response = await deviceService.getDevices(machineId);
      return response.data;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Erreur inconnue');
    }
  }
);

export const fetchDeviceState = createAsyncThunk(
  'devices/fetchDeviceState',
  async (deviceId: string, { rejectWithValue }) => {
    try {
      const response = await deviceService.getDeviceState(deviceId);
      return { deviceId, state: response.data };
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Erreur inconnue');
    }
  }
);

export const updateDeviceConfig = createAsyncThunk(
  'devices/updateDeviceConfig',
  async ({ deviceId, config }: { deviceId: string; config: Record<string, any> }, { rejectWithValue }) => {
    try {
      const response = await deviceService.updateDevice(deviceId, { config });
      return response.data;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Erreur inconnue');
    }
  }
);

// Slice
const devicesSlice = createSlice({
  name: 'devices',
  initialState,
  reducers: {
    selectDevice: (state, action: PayloadAction<string>) => {
      state.selectedDeviceId = action.payload;
    },
    clearSelection: (state) => {
      state.selectedDeviceId = null;
    },
    updateDeviceState: (state, action: PayloadAction<{ deviceId: string; state: DeviceState }>) => {
      const { deviceId, state: newState } = action.payload;
      state.currentStates[deviceId] = newState;
    },
    clearError: (state) => {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      // Fetch devices
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
      })
      
      // Fetch device state
      .addCase(fetchDeviceState.fulfilled, (state, action) => {
        const { deviceId, state: deviceState } = action.payload;
        state.currentStates[deviceId] = deviceState;
      })
      
      // Update device config
      .addCase(updateDeviceConfig.fulfilled, (state, action) => {
        const updatedDevice = action.payload;
        const index = state.items.findIndex(device => device.id === updatedDevice.id);
        if (index !== -1) {
          state.items[index] = updatedDevice;
        }
      });
  }
});

export const { selectDevice, clearSelection, updateDeviceState, clearError } = devicesSlice.actions;
export default devicesSlice.reducer;

// Selectors
export const selectAllDevices = (state: { devices: DevicesState }) => state.devices.items;
export const selectDevicesLoading = (state: { devices: DevicesState }) => state.devices.loading;
export const selectDevicesError = (state: { devices: DevicesState }) => state.devices.error;
export const selectSelectedDevice = (state: { devices: DevicesState }) => {
  const { items, selectedDeviceId } = state.devices;
  return selectedDeviceId ? items.find(device => device.id === selectedDeviceId) : null;
};
export const selectDeviceState = (deviceId: string) => (state: { devices: DevicesState }) => 
  state.devices.currentStates[deviceId];
```

## Couche Backend Node.js

### Contrôleurs Express - Templates

**1. Contrôleur d'Appareils:**

```typescript
// src/controllers/device-controller.ts
import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { DeviceService } from '../services/device-service';
import { ActionService } from '../services/action-service';
import { ApiResponse, PaginationQuery } from '../types/api';
import { Device, DeviceAction } from '../types/device';

// Schémas de validation
const createDeviceSchema = z.object({
  machineId: z.string().uuid(),
  deviceTypeId: z.string().uuid(),
  name: z.string().min(1).max(255),
  zone: z.string().optional(),
  config: z.record(z.any()).default({})
});

const updateDeviceSchema = z.object({
  name: z.string().min(1).max(255).optional(),
  zone: z.string().optional(),
  config: z.record(z.any()).optional(),
  isActive: z.boolean().optional()
});

const executeActionSchema = z.object({
  type: z.string().min(1),
  payload: z.record(z.any())
});

export class DeviceController {
  constructor(
    private deviceService: DeviceService,
    private actionService: ActionService
  ) {}

  // GET /api/machines/:machineId/devices
  async getDevices(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { machineId } = req.params;
      const query = req.query as PaginationQuery;

      // Vérifier l'autorisation d'accès à la machine
      if (!req.user?.machineIds.includes(machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cette machine'
        });
        return;
      }

      const result = await this.deviceService.getDevicesByMachine(machineId, {
        page: parseInt(query.page || '1'),
        limit: parseInt(query.limit || '20'),
        category: query.category,
        zone: query.zone,
        isActive: query.isActive === 'true' ? true : query.isActive === 'false' ? false : undefined
      });

      const response: ApiResponse<Device[]> = {
        success: true,
        data: result.devices,
        pagination: {
          page: result.page,
          limit: result.limit,
          total: result.total,
          totalPages: Math.ceil(result.total / result.limit)
        }
      };

      res.json(response);
    } catch (error) {
      next(error);
    }
  }

  // GET /api/devices/:deviceId
  async getDevice(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { deviceId } = req.params;
      
      const device = await this.deviceService.getDeviceById(deviceId);
      if (!device) {
        res.status(404).json({
          success: false,
          error: 'Appareil non trouvé'
        });
        return;
      }

      // Vérifier l'autorisation
      if (!req.user?.machineIds.includes(device.machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cet appareil'
        });
        return;
      }

      const response: ApiResponse<Device> = {
        success: true,
        data: device
      };

      res.json(response);
    } catch (error) {
      next(error);
    }
  }

  // POST /api/machines/:machineId/devices
  async createDevice(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { machineId } = req.params;
      
      // Validation
      const validatedData = createDeviceSchema.parse({
        ...req.body,
        machineId
      });

      // Vérifier l'autorisation
      if (!req.user?.machineIds.includes(machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cette machine'
        });
        return;
      }

      const device = await this.deviceService.createDevice(validatedData);

      const response: ApiResponse<Device> = {
        success: true,
        data: device,
        message: 'Appareil créé avec succès'
      };

      res.status(201).json(response);
    } catch (error) {
      if (error instanceof z.ZodError) {
        res.status(400).json({
          success: false,
          error: 'Données invalides',
          details: error.errors
        });
        return;
      }
      next(error);
    }
  }

  // PUT /api/devices/:deviceId
  async updateDevice(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { deviceId } = req.params;
      
      // Validation
      const validatedData = updateDeviceSchema.parse(req.body);

      // Vérifier que l'appareil existe et l'autorisation
      const existingDevice = await this.deviceService.getDeviceById(deviceId);
      if (!existingDevice) {
        res.status(404).json({
          success: false,
          error: 'Appareil non trouvé'
        });
        return;
      }

      if (!req.user?.machineIds.includes(existingDevice.machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cet appareil'
        });
        return;
      }

      const updatedDevice = await this.deviceService.updateDevice(deviceId, validatedData);

      const response: ApiResponse<Device> = {
        success: true,
        data: updatedDevice,
        message: 'Appareil mis à jour avec succès'
      };

      res.json(response);
    } catch (error) {
      if (error instanceof z.ZodError) {
        res.status(400).json({
          success: false,
          error: 'Données invalides',
          details: error.errors
        });
        return;
      }
      next(error);
    }
  }

  // POST /api/devices/:deviceId/actions
  async executeAction(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { deviceId } = req.params;
      
      // Validation
      const validatedAction = executeActionSchema.parse(req.body);

      // Vérifier que l'appareil existe et l'autorisation
      const device = await this.deviceService.getDeviceById(deviceId);
      if (!device) {
        res.status(404).json({
          success: false,
          error: 'Appareil non trouvé'
        });
        return;
      }

      if (!req.user?.machineIds.includes(device.machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cet appareil'
        });
        return;
      }

      const action = await this.actionService.createAction({
        deviceId,
        machineId: device.machineId,
        actionType: validatedAction.type,
        payload: validatedAction.payload,
        userId: req.user.userId
      });

      const response: ApiResponse<DeviceAction> = {
        success: true,
        data: action,
        message: 'Action programmée avec succès'
      };

      res.status(201).json(response);
    } catch (error) {
      if (error instanceof z.ZodError) {
        res.status(400).json({
          success: false,
          error: 'Action invalide',
          details: error.errors
        });
        return;
      }
      next(error);
    }
  }

  // GET /api/devices/:deviceId/state
  async getDeviceState(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { deviceId } = req.params;
      
      // Vérifier que l'appareil existe et l'autorisation
      const device = await this.deviceService.getDeviceById(deviceId);
      if (!device) {
        res.status(404).json({
          success: false,
          error: 'Appareil non trouvé'
        });
        return;
      }

      if (!req.user?.machineIds.includes(device.machineId)) {
        res.status(403).json({
          success: false,
          error: 'Accès non autorisé à cet appareil'
        });
        return;
      }

      const state = await this.deviceService.getLatestDeviceState(deviceId);

      const response: ApiResponse<any> = {
        success: true,
        data: state || {},
        message: state ? undefined : 'Aucun état disponible pour cet appareil'
      };

      res.json(response);
    } catch (error) {
      next(error);
    }
  }
}
```

**2. Service Métier pour Appareils:**

```typescript
// src/services/device-service.ts
import { PrismaClient } from '@prisma/client';
import { Device, DeviceState, CreateDeviceData, UpdateDeviceData } from '../types/device';
import { CacheService } from './cache-service';
import { AuditService } from './audit-service';

interface DeviceFilters {
  page: number;
  limit: number;
  category?: string;
  zone?: string;
  isActive?: boolean;
}

interface PaginatedResult<T> {
  devices: T[];
  total: number;
  page: number;
  limit: number;
}

export class DeviceService {
  constructor(
    private prisma: PrismaClient,
    private cacheService: CacheService,
    private auditService: AuditService
  ) {}

  async getDevicesByMachine(
    machineId: string, 
    filters: DeviceFilters
  ): Promise<PaginatedResult<Device>> {
    const cacheKey = `devices:${machineId}:${JSON.stringify(filters)}`;
    
    // Vérifier le cache
    const cached = await this.cacheService.get<PaginatedResult<Device>>(cacheKey);
    if (cached) {
      return cached;
    }

    const where: any = {
      machineId,
      ...(filters.isActive !== undefined && { isActive: filters.isActive })
    };

    if (filters.category) {
      where.deviceType = {
        category: filters.category
      };
    }

    if (filters.zone) {
      where.zone = filters.zone;
    }

    const [devices, total] = await Promise.all([
      this.prisma.device.findMany({
        where,
        include: {
          deviceType: true,
          machine: {
            select: { id: true, serialNumber: true }
          }
        },
        skip: (filters.page - 1) * filters.limit,
        take: filters.limit,
        orderBy: [
          { zone: 'asc' },
          { name: 'asc' }
        ]
      }),
      this.prisma.device.count({ where })
    ]);

    const result = {
      devices: devices as Device[],
      total,
      page: filters.page,
      limit: filters.limit
    };

    // Mettre en cache pour 5 minutes
    await this.cacheService.set(cacheKey, result, 300);

    return result;
  }

  async getDeviceById(deviceId: string): Promise<Device | null> {
    const cacheKey = `device:${deviceId}`;
    
    const cached = await this.cacheService.get<Device>(cacheKey);
    if (cached) {
      return cached;
    }

    const device = await this.prisma.device.findUnique({
      where: { id: deviceId },
      include: {
        deviceType: true,
        machine: {
          select: { id: true, serialNumber: true }
        }
      }
    });

    if (device) {
      await this.cacheService.set(cacheKey, device as Device, 300);
    }

    return device as Device | null;
  }

  async createDevice(data: CreateDeviceData): Promise<Device> {
    const device = await this.prisma.device.create({
      data: {
        machineId: data.machineId,
        deviceTypeId: data.deviceTypeId,
        name: data.name,
        zone: data.zone,
        config: data.config
      },
      include: {
        deviceType: true,
        machine: {
          select: { id: true, serialNumber: true }
        }
      }
    });

    // Invalider le cache des appareils de cette machine
    await this.cacheService.deletePattern(`devices:${data.machineId}:*`);

    // Audit log
    await this.auditService.log({
      action: 'device_created',
      resourceType: 'device',
      resourceId: device.id,
      newValues: device,
      userId: data.userId
    });

    return device as Device;
  }

  async updateDevice(deviceId: string, data: UpdateDeviceData): Promise<Device> {
    // Récupérer l'ancien état pour l'audit
    const oldDevice = await this.getDeviceById(deviceId);
    if (!oldDevice) {
      throw new Error('Appareil non trouvé');
    }

    const updatedDevice = await this.prisma.device.update({
      where: { id: deviceId },
      data: {
        ...(data.name && { name: data.name }),
        ...(data.zone !== undefined && { zone: data.zone }),
        ...(data.config && { config: data.config }),
        ...(data.isActive !== undefined && { isActive: data.isActive }),
        updatedAt: new Date()
      },
      include: {
        deviceType: true,
        machine: {
          select: { id: true, serialNumber: true }
        }
      }
    });

    // Invalider les caches
    await Promise.all([
      this.cacheService.delete(`device:${deviceId}`),
      this.cacheService.deletePattern(`devices:${oldDevice.machineId}:*`)
    ]);

    // Audit log
    await this.auditService.log({
      action: 'device_updated',
      resourceType: 'device',
      resourceId: deviceId,
      oldValues: oldDevice,
      newValues: updatedDevice,
      userId: data.userId
    });

    return updatedDevice as Device;
  }

  async getLatestDeviceState(deviceId: string): Promise<DeviceState | null> {
    const cacheKey = `device_state:${deviceId}`;
    
    const cached = await this.cacheService.get<DeviceState>(cacheKey);
    if (cached) {
      return cached;
    }

    const latestState = await this.prisma.deviceState.findFirst({
      where: { deviceId },
      orderBy: { timestamp: 'desc' }
    });

    if (latestState) {
      const state = latestState.stateData as DeviceState;
      await this.cacheService.set(cacheKey, state, 60); // Cache 1 minute
      return state;
    }

    return null;
  }

  async updateDeviceState(deviceId: string, stateData: DeviceState): Promise<void> {
    await this.prisma.deviceState.create({
      data: {
        deviceId,
        stateData: stateData as any,
        timestamp: new Date()
      }
    });

    // Mettre à jour le cache
    await this.cacheService.set(`device_state:${deviceId}`, stateData, 60);

    // Nettoyer les anciens états (garder seulement les 1000 derniers)
    await this.prisma.deviceState.deleteMany({
      where: {
        deviceId,
        id: {
          notIn: await this.prisma.deviceState.findMany({
            where: { deviceId },
            select: { id: true },
            orderBy: { timestamp: 'desc' },
            take: 1000
          }).then(states => states.map(s => s.id))
        }
      }
    });
  }

  async deleteDevice(deviceId: string, userId: string): Promise<void> {
    const device = await this.getDeviceById(deviceId);
    if (!device) {
      throw new Error('Appareil non trouvé');
    }

    await this.prisma.device.update({
      where: { id: deviceId },
      data: { isActive: false }
    });

    // Invalider les caches
    await Promise.all([
      this.cacheService.delete(`device:${deviceId}`),
      this.cacheService.deletePattern(`devices:${device.machineId}:*`)
    ]);

    // Audit log
    await this.auditService.log({
      action: 'device_deleted',
      resourceType: 'device',
      resourceId: deviceId,
      oldValues: device,
      userId
    });
  }
}
```

## Couche Base de Données

### Exemples de Requêtes et Migrations

**1. Migration de Création de Table:**

```sql
-- migrations/20241220_001_create_devices_table.sql
-- UP Migration
BEGIN;

-- Créer la table des types d'appareils
CREATE TABLE device_types (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name VARCHAR(100) UNIQUE NOT NULL,
  display_name VARCHAR(255) NOT NULL,
  category VARCHAR(50) NOT NULL,
  icon VARCHAR(100),
  config_schema JSONB,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  CONSTRAINT device_types_category_check CHECK (category IN ('climate', 'security', 'comfort', 'energy'))
);

-- Créer la table des appareils
CREATE TABLE devices (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  machine_id UUID NOT NULL REFERENCES machines(id) ON DELETE CASCADE,
  device_type_id UUID NOT NULL REFERENCES device_types(id),
  name VARCHAR(255) NOT NULL,
  zone VARCHAR(100),
  config JSONB DEFAULT '{}',
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  
  CONSTRAINT devices_name_machine_unique UNIQUE(machine_id, name)
);

-- Créer la table des états d'appareils
CREATE TABLE device_states (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  device_id UUID NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
  state_data JSONB NOT NULL,
  timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les performances
CREATE INDEX idx_devices_machine_id ON devices(machine_id);
CREATE INDEX idx_devices_type_category ON devices(device_type_id);
CREATE INDEX idx_devices_zone ON devices(zone) WHERE zone IS NOT NULL;
CREATE INDEX idx_devices_active ON devices(is_active) WHERE is_active = true;

CREATE INDEX idx_device_states_device_timestamp ON device_states(device_id, timestamp DESC);
CREATE INDEX idx_device_states_timestamp ON device_states(timestamp);

-- Trigger pour updated_at automatique
CREATE OR REPLACE FUNCTION update_devices_updated_at()
RETURNS TRIGGER AS $
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$ language 'plpgsql';

CREATE TRIGGER trigger_devices_updated_at 
  BEFORE UPDATE ON devices
  FOR EACH ROW 
  EXECUTE FUNCTION update_devices_updated_at();

-- Insérer les types d'appareils de base
INSERT INTO device_types (name, display_name, category, config_schema) VALUES
('heating_zone', 'Zone de Chauffage', 'climate', '{
  "type": "object",
  "properties": {
    "targetTemperature": {"type": "number", "minimum": 5, "maximum": 30},
    "mode": {"type": "string", "enum": ["off", "eco", "comfort", "auto"]},
    "schedule": {
      "type": "object",
      "properties": {
        "enabled": {"type": "boolean"},
        "periods": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "start": {"type": "string", "pattern": "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"},
              "end": {"type": "string", "pattern": "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"},
              "temperature": {"type": "number", "minimum": 5, "maximum": 30},
              "days": {"type": "array", "items": {"type": "integer", "minimum": 0, "maximum": 6}}
            },
            "required": ["start", "end", "temperature", "days"]
          }
        }
      }
    }
  },
  "required": ["targetTemperature", "mode"]
}'),

('shutter', 'Volet Roulant', 'comfort', '{
  "type": "object",
  "properties": {
    "position": {"type": "number", "minimum": 0, "maximum": 100},
    "autoMode": {"type": "boolean"},
    "schedule": {
      "type": "object",
      "properties": {
        "enabled": {"type": "boolean"},
        "openTime": {"type": "string", "pattern": "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"},
        "closeTime": {"type": "string", "pattern": "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"}
      }
    }
  },
  "required": ["position"]
}'),

('alarm_system', 'Système d''Alarme', 'security', '{
  "type": "object",
  "properties": {
    "armed": {"type": "boolean"},
    "mode": {"type": "string", "enum": ["disarmed", "home", "away", "night"]},
    "zones": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": {"type": "string"},
          "name": {"type": "string"},
          "enabled": {"type": "boolean"},
          "type": {"type": "string", "enum": ["door", "window", "motion", "smoke", "flood"]}
        }
      }
    }
  },
  "required": ["armed", "mode"]
}');

COMMIT;
```

**2. Requêtes Complexes Optimisées:**

```sql
-- Requête pour récupérer les appareils avec leur dernier état
WITH latest_states AS (
  SELECT DISTINCT ON (device_id) 
    device_id,
    state_data,
    timestamp
  FROM device_states
  ORDER BY device_id, timestamp DESC
)
SELECT 
  d.id,
  d.name,
  d.zone,
  d.config,
  d.is_active,
  dt.name as device_type_name,
  dt.display_name as device_type_display,
  dt.category,
  ls.state_data as current_state,
  ls.timestamp as last_update
FROM devices d
JOIN device_types dt ON d.device_type_id = dt.id
LEFT JOIN latest_states ls ON d.id = ls.device_id
WHERE d.machine_id = $1
  AND d.is_active = true
ORDER BY d.zone ASC, d.name ASC;

-- Requête pour les statistiques d'utilisation par zone
SELECT 
  d.zone,
  dt.category,
  COUNT(*) as device_count,
  COUNT(CASE WHEN d.is_active THEN 1 END) as active_count,
  AVG(CASE 
    WHEN dt.category = 'climate' AND ls.state_data->>'currentTemperature' IS NOT NULL 
    THEN (ls.state_data->>'currentTemperature')::numeric 
  END) as avg_temperature
FROM devices d
JOIN device_types dt ON d.device_type_id = dt.id
LEFT JOIN (
  SELECT DISTINCT ON (device_id) 
    device_id,
    state_data
  FROM device_states
  ORDER BY device_id, timestamp DESC
) ls ON d.id = ls.device_id
WHERE d.machine_id = $1
GROUP BY d.zone, dt.category
ORDER BY d.zone, dt.category;

-- Requête pour l'historique des états avec pagination
SELECT 
  ds.id,
  ds.state_data,
  ds.timestamp,
  d.name as device_name,
  d.zone
FROM device_states ds
JOIN devices d ON ds.device_id = d.id
WHERE d.machine_id = $1
  AND ds.timestamp >= $2
  AND ds.timestamp <= $3
ORDER BY ds.timestamp DESC
LIMIT $4 OFFSET $5;
```

**3. Procédures Stockées pour la Maintenance:**

```sql
-- Procédure de nettoyage des anciens états
CREATE OR REPLACE FUNCTION cleanup_old_device_states(
  retention_days INTEGER DEFAULT 90,
  batch_size INTEGER DEFAULT 1000
) RETURNS INTEGER AS $
DECLARE
  deleted_count INTEGER := 0;
  total_deleted INTEGER := 0;
  cutoff_date TIMESTAMP WITH TIME ZONE;
BEGIN
  cutoff_date := NOW() - (retention_days || ' days')::INTERVAL;
  
  LOOP
    DELETE FROM device_states
    WHERE id IN (
      SELECT id 
      FROM device_states 
      WHERE timestamp < cutoff_date
      LIMIT batch_size
    );
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    total_deleted := total_deleted + deleted_count;
    
    EXIT WHEN deleted_count = 0;
    
    -- Pause pour éviter de bloquer la base
    PERFORM pg_sleep(0.1);
  END LOOP;
  
  RETURN total_deleted;
END;
$ LANGUAGE plpgsql;

-- Fonction d'agrégation des données historiques
CREATE OR REPLACE FUNCTION aggregate_device_states_hourly(
  target_date DATE DEFAULT CURRENT_DATE - 1
) RETURNS VOID AS $
DECLARE
  start_time TIMESTAMP WITH TIME ZONE;
  end_time TIMESTAMP WITH TIME ZONE;
BEGIN
  start_time := target_date::TIMESTAMP WITH TIME ZONE;
  end_time := start_time + INTERVAL '1 day';
  
  -- Créer la table d'agrégation si elle n'existe pas
  CREATE TABLE IF NOT EXISTS device_states_hourly (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    device_id UUID NOT NULL,
    hour_timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    avg_state JSONB,
    min_state JSONB,
    max_state JSONB,
    sample_count INTEGER,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    UNIQUE(device_id, hour_timestamp)
  );
  
  -- Agréger les données par heure
  INSERT INTO device_states_hourly (
    device_id,
    hour_timestamp,
    avg_state,
    min_state,
    max_state,
    sample_count
  )
  SELECT 
    device_id,
    date_trunc('hour', timestamp) as hour_timestamp,
    jsonb_build_object(
      'avgTemperature', 
      AVG((state_data->>'currentTemperature')::numeric)
    ) as avg_state,
    (array_agg(state_data ORDER BY timestamp))[1] as min_state,
    (array_agg(state_data ORDER BY timestamp DESC))[1] as max_state,
    COUNT(*) as sample_count
  FROM device_states
  WHERE timestamp >= start_time 
    AND timestamp < end_time
    AND state_data->>'currentTemperature' IS NOT NULL
  GROUP BY device_id, date_trunc('hour', timestamp)
  ON CONFLICT (device_id, hour_timestamp) DO NOTHING;
  
END;
$ LANGUAGE plpgsql;
```

## Patterns d'Intégration entre Couches

### Communication Frontend-Backend

**1. Service API avec Gestion d'Erreurs:**

```typescript
// src/services/api-client.ts
import axios, { AxiosInstance, AxiosError, AxiosResponse } from 'axios';
import { store } from '../store';
import { logout } from '../store/slices/auth-slice';
import { showNotification } from '../store/slices/notifications-slice';

class ApiClient {
  private client: AxiosInstance;
  private refreshPromise: Promise<string> | null = null;

  constructor() {
    this.client = axios.create({
      baseURL: process.env.REACT_APP_API_URL || 'http://localhost:3001/api',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    // Request interceptor pour ajouter le token
    this.client.interceptors.request.use(
      (config) => {
        const state = store.getState();
        const token = state.auth.accessToken;
        
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor pour gérer les erreurs et le refresh token
    this.client.interceptors.response.use(
      (response: AxiosResponse) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as any;

        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          try {
            const newToken = await this.refreshAccessToken();
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return this.client(originalRequest);
          } catch (refreshError) {
            store.dispatch(logout());
            store.dispatch(showNotification({
              type: 'error',
              message: 'Session expirée, veuillez vous reconnecter',
              duration: 5000
            }));
            return Promise.reject(refreshError);
          }
        }

        // Gestion des autres erreurs
        this.handleApiError(error);
        return Promise.reject(error);
      }
    );
  }

  private async refreshAccessToken(): Promise<string> {
    if (this.refreshPromise) {
      return this.refreshPromise;
    }

    this.refreshPromise = (async () => {
      try {
        const state = store.getState();
        const refreshToken = state.auth.refreshToken;

        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        const response = await axios.post(`${this.client.defaults.baseURL}/auth/refresh`, {
          refreshToken
        });

        const { accessToken } = response.data.data;
        
        // Mettre à jour le store avec le nouveau token
        // (ceci devrait être fait via une action Redux)
        
        return accessToken;
      } finally {
        this.refreshPromise = null;
      }
    })();

    return this.refreshPromise;
  }

  private handleApiError(error: AxiosError): void {
    let message = 'Une erreur est survenue';

    if (error.response) {
      const data = error.response.data as any;
      message = data.error || data.message || message;
    } else if (error.request) {
      message = 'Erreur de connexion au serveur';
    }

    store.dispatch(showNotification({
      type: 'error',
      message,
      duration: 5000
    }));
  }

  // Méthodes publiques
  async get<T>(url: string, params?: any): Promise<T> {
    const response = await this.client.get(url, { params });
    return response.data;
  }

  async post<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.post(url, data);
    return response.data;
  }

  async put<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.put(url, data);
    return response.data;
  }

  async delete<T>(url: string): Promise<T> {
    const response = await this.client.delete(url);
    return response.data;
  }
}

export const apiClient = new ApiClient();
```

**2. WebSocket pour Temps Réel:**

```typescript
// src/services/websocket-service.ts
import { io, Socket } from 'socket.io-client';
import { store } from '../store';
import { updateDeviceState } from '../store/slices/devices-slice';
import { addAction, updateActionStatus } from '../store/slices/actions-slice';

class WebSocketService {
  private socket: Socket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  connect(token: string): void {
    if (this.socket?.connected) {
      return;
    }

    this.socket = io(process.env.REACT_APP_WS_URL || 'http://localhost:3001', {
      auth: {
        token
      },
      transports: ['websocket'],
      upgrade: false
    });

    this.setupEventListeners();
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.disconnect();
      this.socket = null;
    }
  }

  private setupEventListeners(): void {
    if (!this.socket) return;

    this.socket.on('connect', () => {
      console.log('WebSocket connecté');
      this.reconnectAttempts = 0;
      
      // S'abonner aux mises à jour des machines de l'utilisateur
      const state = store.getState();
      const machineIds = state.auth.user?.machineIds || [];
      
      machineIds.forEach(machineId => {
        this.socket?.emit('subscribe', { machineId });
      });
    });

    this.socket.on('disconnect', (reason) => {
      console.log('WebSocket déconnecté:', reason);
      
      if (reason === 'io server disconnect') {
        // Reconnexion manuelle nécessaire
        this.handleReconnection();
      }
    });

    this.socket.on('connect_error', (error) => {
      console.error('Erreur de connexion WebSocket:', error);
      this.handleReconnection();
    });

    // Événements métier
    this.socket.on('device_state_updated', (data) => {
      store.dispatch(updateDeviceState({
        deviceId: data.deviceId,
        state: data.state
      }));
    });

    this.socket.on('action_status_updated', (data) => {
      store.dispatch(updateActionStatus({
        actionId: data.actionId,
        status: data.status,
        executedAt: data.executedAt,
        errorMessage: data.errorMessage
      }));
    });

    this.socket.on('new_action', (data) => {
      store.dispatch(addAction(data.action));
    });
  }

  private handleReconnection(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Nombre maximum de tentatives de reconnexion atteint');
      return;
    }

    this.reconnectAttempts++;
    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
    
    setTimeout(() => {
      console.log(`Tentative de reconnexion ${this.reconnectAttempts}/${this.maxReconnectAttempts}`);
      this.socket?.connect();
    }, delay);
  }

  subscribeToMachine(machineId: string): void {
    this.socket?.emit('subscribe', { machineId });
  }

  unsubscribeFromMachine(machineId: string): void {
    this.socket?.emit('unsubscribe', { machineId });
  }
}

export const webSocketService = new WebSocketService();
```

Ces exemples d'implémentation fournissent une base solide pour le développement de l'application Essensys moderne, en respectant les bonnes pratiques et patterns établis dans les standards de développement.