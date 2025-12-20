# Architecture Backend Node.js - Essensys Migration

## Vue d'Ensemble

Cette documentation détaille l'architecture backend Node.js/Express pour la migration d'Essensys, définissant la structure des services et contrôleurs, les patterns API RESTful, la gestion des middlewares, la validation des données et l'organisation du code.

## Structure des Services et Contrôleurs Express

### Structure de Projet Recommandée

```
essensys-backend/
├── src/
│   ├── controllers/          # Contrôleurs REST
│   │   ├── auth/
│   │   │   ├── AuthController.ts
│   │   │   ├── UserController.ts
│   │   │   └── index.ts
│   │   ├── devices/
│   │   │   ├── DeviceController.ts
│   │   │   ├── MachineController.ts
│   │   │   └── index.ts
│   │   ├── legacy/
│   │   │   ├── LegacyApiController.ts  # Compatibilité boîtiers
│   │   │   └── index.ts
│   │   └── notifications/
│   │       ├── NotificationController.ts
│   │       └── index.ts
│   ├── services/             # Logique métier
│   │   ├── auth/
│   │   │   ├── AuthService.ts
│   │   │   ├── TokenService.ts
│   │   │   ├── PasswordService.ts
│   │   │   └── index.ts
│   │   ├── devices/
│   │   │   ├── DeviceService.ts
│   │   │   ├── ActionService.ts
│   │   │   ├── StateService.ts
│   │   │   └── index.ts
│   │   ├── communication/
│   │   │   ├── MachineCommService.ts
│   │   │   ├── WebSocketService.ts
│   │   │   └── index.ts
│   │   ├── notifications/
│   │   │   ├── NotificationService.ts
│   │   │   ├── SmsService.ts
│   │   │   ├── EmailService.ts
│   │   │   └── index.ts
│   │   └── firmware/
│   │       ├── FirmwareService.ts
│   │       ├── UpdateService.ts
│   │       └── index.ts
│   ├── models/               # Modèles de données (Prisma)
│   │   ├── User.ts
│   │   ├── Machine.ts
│   │   ├── Device.ts
│   │   ├── Action.ts
│   │   └── index.ts
│   ├── middleware/           # Middlewares Express
│   │   ├── auth/
│   │   │   ├── authenticate.ts
│   │   │   ├── authorize.ts
│   │   │   └── machineAuth.ts
│   │   ├── validation/
│   │   │   ├── validateRequest.ts
│   │   │   ├── schemas/
│   │   │   │   ├── authSchemas.ts
│   │   │   │   ├── deviceSchemas.ts
│   │   │   │   └── userSchemas.ts
│   │   │   └── index.ts
│   │   ├── security/
│   │   │   ├── cors.ts
│   │   │   ├── helmet.ts
│   │   │   ├── rateLimiting.ts
│   │   │   └── index.ts
│   │   ├── logging/
│   │   │   ├── requestLogger.ts
│   │   │   ├── errorLogger.ts
│   │   │   └── index.ts
│   │   └── error/
│   │       ├── errorHandler.ts
│   │       ├── notFound.ts
│   │       └── index.ts
│   ├── routes/               # Définition des routes
│   │   ├── auth.ts
│   │   ├── users.ts
│   │   ├── devices.ts
│   │   ├── machines.ts
│   │   ├── legacy.ts         # Routes compatibilité legacy
│   │   ├── notifications.ts
│   │   ├── firmware.ts
│   │   └── index.ts
│   ├── config/               # Configuration
│   │   ├── database.ts
│   │   ├── redis.ts
│   │   ├── jwt.ts
│   │   ├── email.ts
│   │   ├── sms.ts
│   │   └── index.ts
│   ├── utils/                # Utilitaires
│   │   ├── logger.ts
│   │   ├── crypto.ts
│   │   ├── validators.ts
│   │   ├── formatters.ts
│   │   ├── constants.ts
│   │   └── errors/
│   │       ├── AppError.ts
│   │       ├── ValidationError.ts
│   │       └── index.ts
│   ├── types/                # Types TypeScript
│   │   ├── express.d.ts      # Extensions Express
│   │   ├── auth.ts
│   │   ├── devices.ts
│   │   ├── api.ts
│   │   └── index.ts
│   ├── websocket/            # WebSocket handlers
│   │   ├── handlers/
│   │   │   ├── deviceUpdates.ts
│   │   │   ├── notifications.ts
│   │   │   └── index.ts
│   │   ├── middleware/
│   │   │   ├── wsAuth.ts
│   │   │   └── index.ts
│   │   └── server.ts
│   ├── jobs/                 # Tâches en arrière-plan
│   │   ├── schedulers/
│   │   │   ├── deviceSync.ts
│   │   │   ├── cleanup.ts
│   │   │   └── index.ts
│   │   ├── workers/
│   │   │   ├── notificationWorker.ts
│   │   │   ├── firmwareWorker.ts
│   │   │   └── index.ts
│   │   └── queue.ts
│   ├── database/             # Base de données
│   │   ├── migrations/
│   │   ├── seeds/
│   │   ├── schema.prisma
│   │   └── client.ts
│   ├── app.ts                # Configuration Express
│   └── server.ts             # Point d'entrée
├── tests/                    # Tests
│   ├── unit/
│   ├── integration/
│   ├── e2e/
│   ├── fixtures/
│   └── setup.ts
├── docs/                     # Documentation
│   ├── api/
│   ├── deployment/
│   └── development/
├── scripts/                  # Scripts utilitaires
│   ├── build.sh
│   ├── deploy.sh
│   └── migrate.sh
├── .env.example
├── .env.local
├── .gitignore
├── package.json
├── tsconfig.json
├── jest.config.js
├── docker-compose.yml
├── Dockerfile
└── README.md
```

### Architecture en Couches

**Couche Contrôleur (Controllers):**
- Gestion des requêtes HTTP
- Validation des paramètres d'entrée
- Orchestration des services
- Formatage des réponses

**Couche Service (Services):**
- Logique métier pure
- Orchestration des opérations complexes
- Gestion des transactions
- Indépendante du transport (HTTP, WebSocket)

**Couche Modèle (Models):**
- Définition des entités métier
- Validation des données
- Relations entre entités
- Accès aux données via Prisma

**Couche Middleware:**
- Authentification et autorisation
- Validation des requêtes
- Logging et monitoring
- Gestion des erreurs

## Patterns API RESTful et Gestion des Middlewares

### Structure des Contrôleurs

```typescript
// src/controllers/devices/DeviceController.ts
import { Request, Response, NextFunction } from 'express';
import { DeviceService } from '../../services/devices/DeviceService';
import { ActionService } from '../../services/devices/ActionService';
import { validateRequest } from '../../middleware/validation/validateRequest';
import { deviceSchemas } from '../../middleware/validation/schemas/deviceSchemas';
import { AppError } from '../../utils/errors/AppError';
import { logger } from '../../utils/logger';

export class DeviceController {
  constructor(
    private deviceService: DeviceService,
    private actionService: ActionService
  ) {}

  /**
   * GET /api/machines/:machineId/devices
   * Récupère tous les appareils d'une machine
   */
  public getDevices = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const { machineId } = req.params;
      const userId = req.user!.id;

      // Vérifier l'accès à la machine
      const hasAccess = await this.deviceService.checkMachineAccess(userId, machineId);
      if (!hasAccess) {
        throw new AppError('Accès non autorisé à cette machine', 403);
      }

      const devices = await this.deviceService.getDevicesByMachine(machineId);
      
      res.json({
        success: true,
        data: devices,
        meta: {
          count: devices.length,
          machineId
        }
      });
    } catch (error) {
      next(error);
    }
  };

  /**
   * GET /api/devices/:deviceId/status
   * Récupère le statut d'un appareil
   */
  public getDeviceStatus = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const { deviceId } = req.params;
      const userId = req.user!.id;

      const device = await this.deviceService.getDeviceById(deviceId);
      if (!device) {
        throw new AppError('Appareil non trouvé', 404);
      }

      // Vérifier l'accès
      const hasAccess = await this.deviceService.checkDeviceAccess(userId, deviceId);
      if (!hasAccess) {
        throw new AppError('Accès non autorisé à cet appareil', 403);
      }

      const status = await this.deviceService.getDeviceStatus(deviceId);
      const pendingActions = await this.actionService.getPendingActions(deviceId);

      res.json({
        success: true,
        data: {
          deviceId,
          status: status.status,
          currentState: status.currentState,
          lastUpdate: status.lastUpdate,
          pendingActions
        }
      });
    } catch (error) {
      next(error);
    }
  };

  /**
   * POST /api/devices/:deviceId/actions
   * Envoie une commande à un appareil
   */
  public sendDeviceAction = [
    validateRequest(deviceSchemas.sendAction),
    async (req: Request, res: Response, next: NextFunction): Promise<void> => {
      try {
        const { deviceId } = req.params;
        const { actionType, payload } = req.body;
        const userId = req.user!.id;

        // Vérifier l'accès et les permissions
        const hasAccess = await this.deviceService.checkDeviceAccess(userId, deviceId);
        if (!hasAccess) {
          throw new AppError('Accès non autorisé à cet appareil', 403);
        }

        const canControl = await this.deviceService.checkControlPermission(userId, deviceId);
        if (!canControl) {
          throw new AppError('Permission de contrôle requise', 403);
        }

        // Créer et envoyer l'action
        const action = await this.actionService.createAction({
          deviceId,
          actionType,
          payload,
          userId,
          priority: req.body.priority || 5
        });

        logger.info('Action créée', {
          actionId: action.id,
          deviceId,
          actionType,
          userId
        });

        res.status(201).json({
          success: true,
          data: {
            actionId: action.id,
            status: action.status,
            estimatedExecutionTime: action.estimatedExecutionTime
          }
        });
      } catch (error) {
        next(error);
      }
    }
  ];

  /**
   * PUT /api/devices/:deviceId/config
   * Met à jour la configuration d'un appareil
   */
  public updateDeviceConfig = [
    validateRequest(deviceSchemas.updateConfig),
    async (req: Request, res: Response, next: NextFunction): Promise<void> => {
      try {
        const { deviceId } = req.params;
        const { name, zone, config } = req.body;
        const userId = req.user!.id;

        // Vérifier les permissions d'administration
        const isAdmin = await this.deviceService.checkAdminPermission(userId, deviceId);
        if (!isAdmin) {
          throw new AppError('Permissions d\'administration requises', 403);
        }

        const updatedDevice = await this.deviceService.updateDeviceConfig(deviceId, {
          name,
          zone,
          config
        });

        logger.info('Configuration appareil mise à jour', {
          deviceId,
          userId,
          changes: { name, zone, config }
        });

        res.json({
          success: true,
          data: updatedDevice
        });
      } catch (error) {
        next(error);
      }
    }
  ];
}
```

### Services Métier

```typescript
// src/services/devices/DeviceService.ts
import { PrismaClient } from '@prisma/client';
import { Device, DeviceStatus, DeviceConfig } from '../../types/devices';
import { AppError } from '../../utils/errors/AppError';
import { logger } from '../../utils/logger';
import { WebSocketService } from '../communication/WebSocketService';
import { MachineCommService } from '../communication/MachineCommService';

export class DeviceService {
  constructor(
    private prisma: PrismaClient,
    private wsService: WebSocketService,
    private machineComm: MachineCommService
  ) {}

  /**
   * Récupère tous les appareils d'une machine
   */
  public async getDevicesByMachine(machineId: string): Promise<Device[]> {
    try {
      const devices = await this.prisma.device.findMany({
        where: {
          machineId,
          isActive: true
        },
        include: {
          deviceType: true,
          machine: {
            select: {
              id: true,
              serialNumber: true,
              isActive: true,
              lastConnection: true
            }
          }
        },
        orderBy: [
          { zone: 'asc' },
          { name: 'asc' }
        ]
      });

      // Enrichir avec l'état actuel
      const devicesWithState = await Promise.all(
        devices.map(async (device) => {
          const currentState = await this.getCurrentState(device.id);
          return {
            ...device,
            currentState,
            status: this.determineDeviceStatus(device, currentState)
          };
        })
      );

      return devicesWithState;
    } catch (error) {
      logger.error('Erreur lors de la récupération des appareils', {
        machineId,
        error: error.message
      });
      throw new AppError('Erreur lors de la récupération des appareils', 500);
    }
  }

  /**
   * Récupère l'état actuel d'un appareil
   */
  public async getCurrentState(deviceId: string): Promise<any> {
    const latestState = await this.prisma.deviceState.findFirst({
      where: { deviceId },
      orderBy: { timestamp: 'desc' }
    });

    return latestState?.stateData || {};
  }

  /**
   * Détermine le statut d'un appareil
   */
  private determineDeviceStatus(device: any, currentState: any): DeviceStatus {
    // Si la machine n'est pas connectée
    if (!device.machine.isActive) {
      return 'offline';
    }

    // Vérifier la dernière connexion (plus de 5 minutes = offline)
    const lastConnection = new Date(device.machine.lastConnection);
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    
    if (lastConnection < fiveMinutesAgo) {
      return 'offline';
    }

    // Vérifier s'il y a des erreurs dans l'état
    if (currentState.error || currentState.fault) {
      return 'error';
    }

    // Vérifier le mode maintenance
    if (currentState.maintenance) {
      return 'maintenance';
    }

    return 'online';
  }

  /**
   * Vérifie l'accès d'un utilisateur à une machine
   */
  public async checkMachineAccess(userId: string, machineId: string): Promise<boolean> {
    const userMachine = await this.prisma.userMachine.findFirst({
      where: {
        userId,
        machineId,
        user: { isActive: true },
        machine: { isActive: true }
      }
    });

    return !!userMachine;
  }

  /**
   * Vérifie l'accès d'un utilisateur à un appareil
   */
  public async checkDeviceAccess(userId: string, deviceId: string): Promise<boolean> {
    const device = await this.prisma.device.findFirst({
      where: {
        id: deviceId,
        isActive: true,
        machine: {
          isActive: true,
          userMachines: {
            some: {
              userId,
              user: { isActive: true }
            }
          }
        }
      }
    });

    return !!device;
  }

  /**
   * Vérifie les permissions de contrôle
   */
  public async checkControlPermission(userId: string, deviceId: string): Promise<boolean> {
    const userMachine = await this.prisma.userMachine.findFirst({
      where: {
        userId,
        machine: {
          devices: {
            some: { id: deviceId }
          }
        }
      }
    });

    // Les propriétaires et utilisateurs peuvent contrôler, pas les invités
    return userMachine?.role !== 'guest';
  }

  /**
   * Vérifie les permissions d'administration
   */
  public async checkAdminPermission(userId: string, deviceId: string): Promise<boolean> {
    const userMachine = await this.prisma.userMachine.findFirst({
      where: {
        userId,
        machine: {
          devices: {
            some: { id: deviceId }
          }
        }
      }
    });

    // Seuls les propriétaires peuvent administrer
    return userMachine?.role === 'owner';
  }

  /**
   * Met à jour la configuration d'un appareil
   */
  public async updateDeviceConfig(
    deviceId: string, 
    config: Partial<DeviceConfig>
  ): Promise<Device> {
    try {
      const updatedDevice = await this.prisma.device.update({
        where: { id: deviceId },
        data: {
          name: config.name,
          zone: config.zone,
          config: config.config,
          updatedAt: new Date()
        },
        include: {
          deviceType: true
        }
      });

      // Notifier les clients connectés
      await this.wsService.notifyDeviceUpdate(deviceId, updatedDevice);

      return updatedDevice;
    } catch (error) {
      logger.error('Erreur lors de la mise à jour de la configuration', {
        deviceId,
        config,
        error: error.message
      });
      throw new AppError('Erreur lors de la mise à jour de la configuration', 500);
    }
  }

  /**
   * Récupère le statut détaillé d'un appareil
   */
  public async getDeviceStatus(deviceId: string): Promise<{
    status: DeviceStatus;
    currentState: any;
    lastUpdate: Date;
  }> {
    const device = await this.prisma.device.findUnique({
      where: { id: deviceId },
      include: {
        machine: true,
        deviceStates: {
          orderBy: { timestamp: 'desc' },
          take: 1
        }
      }
    });

    if (!device) {
      throw new AppError('Appareil non trouvé', 404);
    }

    const currentState = device.deviceStates[0]?.stateData || {};
    const status = this.determineDeviceStatus(device, currentState);
    const lastUpdate = device.deviceStates[0]?.timestamp || device.updatedAt;

    return {
      status,
      currentState,
      lastUpdate
    };
  }
}
```

### Middleware d'Authentification

```typescript
// src/middleware/auth/authenticate.ts
import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';
import { PrismaClient } from '@prisma/client';
import { AppError } from '../../utils/errors/AppError';
import { logger } from '../../utils/logger';

const prisma = new PrismaClient();

interface JwtPayload {
  userId: string;
  email: string;
  roles: string[];
  machineIds: string[];
  iat: number;
  exp: number;
}

declare global {
  namespace Express {
    interface Request {
      user?: {
        id: string;
        email: string;
        roles: string[];
        machineIds: string[];
      };
    }
  }
}

/**
 * Middleware d'authentification JWT pour les utilisateurs
 */
export const authenticate = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
  try {
    const authHeader = req.headers.authorization;
    
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      throw new AppError('Token d\'authentification manquant', 401);
    }

    const token = authHeader.substring(7); // Enlever "Bearer "
    
    if (!token) {
      throw new AppError('Token d\'authentification manquant', 401);
    }

    // Vérifier et décoder le JWT
    const decoded = jwt.verify(token, process.env.JWT_SECRET!) as JwtPayload;
    
    // Vérifier que l'utilisateur existe toujours et est actif
    const user = await prisma.user.findUnique({
      where: { id: decoded.userId },
      select: {
        id: true,
        email: true,
        isActive: true,
        userMachines: {
          where: {
            machine: { isActive: true }
          },
          select: {
            machineId: true,
            role: true
          }
        }
      }
    });

    if (!user || !user.isActive) {
      throw new AppError('Utilisateur invalide ou inactif', 401);
    }

    // Enrichir les informations utilisateur
    req.user = {
      id: user.id,
      email: user.email,
      roles: user.userMachines.map(um => um.role),
      machineIds: user.userMachines.map(um => um.machineId)
    };

    next();
  } catch (error) {
    if (error instanceof jwt.JsonWebTokenError) {
      logger.warn('Token JWT invalide', {
        error: error.message,
        ip: req.ip,
        userAgent: req.get('User-Agent')
      });
      return next(new AppError('Token d\'authentification invalide', 401));
    }

    if (error instanceof jwt.TokenExpiredError) {
      logger.info('Token JWT expiré', {
        ip: req.ip,
        userAgent: req.get('User-Agent')
      });
      return next(new AppError('Token d\'authentification expiré', 401));
    }

    next(error);
  }
};

/**
 * Middleware d'authentification pour les boîtiers IoT
 */
export const authenticateMachine = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
  try {
    const machineToken = req.headers['x-machine-token'] as string;
    
    if (!machineToken) {
      throw new AppError('Token machine manquant', 401);
    }

    // Vérifier le token machine
    const decoded = jwt.verify(machineToken, process.env.MACHINE_JWT_SECRET!) as {
      machineId: string;
      serialNumber: string;
      iat: number;
      exp: number;
    };

    // Vérifier que la machine existe et est active
    const machine = await prisma.machine.findUnique({
      where: { id: decoded.machineId },
      select: {
        id: true,
        serialNumber: true,
        isActive: true,
        firmwareVersion: true
      }
    });

    if (!machine || !machine.isActive) {
      throw new AppError('Machine invalide ou inactive', 401);
    }

    // Mettre à jour la dernière connexion
    await prisma.machine.update({
      where: { id: machine.id },
      data: {
        lastConnection: new Date(),
        connectionCount: { increment: 1 },
        lastIpAddress: req.ip
      }
    });

    // Ajouter les informations machine à la requête
    (req as any).machine = {
      id: machine.id,
      serialNumber: machine.serialNumber,
      firmwareVersion: machine.firmwareVersion
    };

    next();
  } catch (error) {
    if (error instanceof jwt.JsonWebTokenError) {
      logger.warn('Token machine invalide', {
        error: error.message,
        ip: req.ip,
        userAgent: req.get('User-Agent')
      });
      return next(new AppError('Token machine invalide', 401));
    }

    next(error);
  }
};

/**
 * Middleware d'autorisation par rôle
 */
export const authorize = (allowedRoles: string[]) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    if (!req.user) {
      return next(new AppError('Authentification requise', 401));
    }

    const hasRole = req.user.roles.some(role => allowedRoles.includes(role));
    
    if (!hasRole) {
      logger.warn('Accès refusé - rôle insuffisant', {
        userId: req.user.id,
        userRoles: req.user.roles,
        requiredRoles: allowedRoles,
        path: req.path
      });
      return next(new AppError('Permissions insuffisantes', 403));
    }

    next();
  };
};

/**
 * Middleware d'autorisation pour l'accès à une machine spécifique
 */
export const authorizeMachine = (machineIdParam: string = 'machineId') => {
  return (req: Request, res: Response, next: NextFunction): void => {
    if (!req.user) {
      return next(new AppError('Authentification requise', 401));
    }

    const machineId = req.params[machineIdParam];
    
    if (!machineId) {
      return next(new AppError('ID de machine manquant', 400));
    }

    if (!req.user.machineIds.includes(machineId)) {
      logger.warn('Accès refusé - machine non autorisée', {
        userId: req.user.id,
        requestedMachineId: machineId,
        userMachineIds: req.user.machineIds,
        path: req.path
      });
      return next(new AppError('Accès non autorisé à cette machine', 403));
    }

    next();
  };
};
```

## Configuration de la Validation des Données et Gestion d'Erreurs

### Schémas de Validation Zod

```typescript
// src/middleware/validation/schemas/deviceSchemas.ts
import { z } from 'zod';

export const deviceSchemas = {
  // Validation pour l'envoi d'une action
  sendAction: z.object({
    body: z.object({
      actionType: z.string().min(1, 'Type d\'action requis'),
      payload: z.record(z.any()).refine(
        (data) => Object.keys(data).length > 0,
        'Payload ne peut pas être vide'
      ),
      priority: z.number().int().min(1).max(10).optional().default(5)
    }),
    params: z.object({
      deviceId: z.string().uuid('ID d\'appareil invalide')
    })
  }),

  // Validation pour la mise à jour de configuration
  updateConfig: z.object({
    body: z.object({
      name: z.string().min(1).max(255).optional(),
      zone: z.string().min(1).max(100).optional(),
      config: z.record(z.any()).optional()
    }).refine(
      (data) => Object.keys(data).length > 0,
      'Au moins un champ doit être fourni'
    ),
    params: z.object({
      deviceId: z.string().uuid('ID d\'appareil invalide')
    })
  }),

  // Validation pour les paramètres de requête
  getDevices: z.object({
    params: z.object({
      machineId: z.string().uuid('ID de machine invalide')
    }),
    query: z.object({
      category: z.enum(['climate', 'security', 'comfort']).optional(),
      status: z.enum(['online', 'offline', 'error', 'maintenance']).optional(),
      zone: z.string().optional()
    })
  })
};

// Schémas pour l'authentification
export const authSchemas = {
  login: z.object({
    body: z.object({
      email: z.string().email('Email invalide'),
      password: z.string().min(8, 'Mot de passe trop court'),
      rememberMe: z.boolean().optional().default(false)
    })
  }),

  register: z.object({
    body: z.object({
      email: z.string().email('Email invalide'),
      password: z.string()
        .min(8, 'Mot de passe trop court')
        .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/, 'Mot de passe trop faible'),
      firstName: z.string().min(1).max(100),
      lastName: z.string().min(1).max(100),
      activationKey: z.string().regex(
        /^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/,
        'Clé d\'activation invalide'
      ),
      address: z.object({
        line1: z.string().min(1).max(255),
        line2: z.string().max(255).optional(),
        postalCode: z.string().regex(/^\d{5}$/, 'Code postal invalide'),
        city: z.string().min(1).max(100)
      }),
      phone: z.string().regex(/^\+33[1-9]\d{8}$/, 'Numéro de téléphone invalide').optional(),
      securityQuestion: z.string().min(1).max(255),
      securityAnswer: z.string().min(1).max(255)
    })
  }),

  refreshToken: z.object({
    body: z.object({
      refreshToken: z.string().min(1, 'Refresh token requis')
    })
  })
};
```

### Middleware de Validation

```typescript
// src/middleware/validation/validateRequest.ts
import { Request, Response, NextFunction } from 'express';
import { ZodSchema, ZodError } from 'zod';
import { AppError } from '../../utils/errors/AppError';
import { logger } from '../../utils/logger';

/**
 * Middleware de validation des requêtes avec Zod
 */
export const validateRequest = (schema: ZodSchema) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    try {
      // Valider la requête complète (body, params, query)
      const validatedData = schema.parse({
        body: req.body,
        params: req.params,
        query: req.query
      });

      // Remplacer les données de la requête par les données validées
      req.body = validatedData.body || req.body;
      req.params = validatedData.params || req.params;
      req.query = validatedData.query || req.query;

      next();
    } catch (error) {
      if (error instanceof ZodError) {
        const validationErrors = error.errors.map(err => ({
          field: err.path.join('.'),
          message: err.message,
          code: err.code
        }));

        logger.warn('Erreur de validation de requête', {
          path: req.path,
          method: req.method,
          errors: validationErrors,
          body: req.body,
          params: req.params,
          query: req.query
        });

        const errorMessage = validationErrors
          .map(err => `${err.field}: ${err.message}`)
          .join(', ');

        return next(new AppError(`Données invalides: ${errorMessage}`, 400, validationErrors));
      }

      next(error);
    }
  };
};

/**
 * Middleware de validation spécifique pour les paramètres d'URL
 */
export const validateParams = (schema: ZodSchema) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    try {
      req.params = schema.parse(req.params);
      next();
    } catch (error) {
      if (error instanceof ZodError) {
        const errorMessage = error.errors
          .map(err => `${err.path.join('.')}: ${err.message}`)
          .join(', ');
        
        return next(new AppError(`Paramètres invalides: ${errorMessage}`, 400));
      }
      next(error);
    }
  };
};

/**
 * Middleware de validation pour le body uniquement
 */
export const validateBody = (schema: ZodSchema) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    try {
      req.body = schema.parse(req.body);
      next();
    } catch (error) {
      if (error instanceof ZodError) {
        const errorMessage = error.errors
          .map(err => `${err.path.join('.')}: ${err.message}`)
          .join(', ');
        
        return next(new AppError(`Corps de requête invalide: ${errorMessage}`, 400));
      }
      next(error);
    }
  };
};
```

### Gestion Centralisée des Erreurs

```typescript
// src/middleware/error/errorHandler.ts
import { Request, Response, NextFunction } from 'express';
import { AppError } from '../../utils/errors/AppError';
import { logger } from '../../utils/logger';

interface ErrorResponse {
  success: false;
  error: {
    code: string;
    message: string;
    details?: any;
  };
  timestamp: string;
  path: string;
  method: string;
}

/**
 * Middleware global de gestion des erreurs
 */
export const errorHandler = (
  error: Error,
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  // Si les headers ont déjà été envoyés, déléguer à Express
  if (res.headersSent) {
    return next(error);
  }

  let statusCode = 500;
  let errorCode = 'INTERNAL_SERVER_ERROR';
  let message = 'Une erreur interne s\'est produite';
  let details: any = undefined;

  // Gestion des erreurs personnalisées
  if (error instanceof AppError) {
    statusCode = error.statusCode;
    errorCode = error.code;
    message = error.message;
    details = error.details;
  }
  // Gestion des erreurs Prisma
  else if (error.name === 'PrismaClientKnownRequestError') {
    const prismaError = error as any;
    
    switch (prismaError.code) {
      case 'P2002':
        statusCode = 409;
        errorCode = 'DUPLICATE_ENTRY';
        message = 'Cette ressource existe déjà';
        break;
      case 'P2025':
        statusCode = 404;
        errorCode = 'NOT_FOUND';
        message = 'Ressource non trouvée';
        break;
      default:
        statusCode = 400;
        errorCode = 'DATABASE_ERROR';
        message = 'Erreur de base de données';
    }
  }
  // Gestion des erreurs JWT
  else if (error.name === 'JsonWebTokenError') {
    statusCode = 401;
    errorCode = 'INVALID_TOKEN';
    message = 'Token d\'authentification invalide';
  }
  else if (error.name === 'TokenExpiredError') {
    statusCode = 401;
    errorCode = 'TOKEN_EXPIRED';
    message = 'Token d\'authentification expiré';
  }

  // Logger l'erreur
  const logLevel = statusCode >= 500 ? 'error' : 'warn';
  logger[logLevel]('Erreur HTTP', {
    error: {
      name: error.name,
      message: error.message,
      stack: error.stack
    },
    request: {
      method: req.method,
      path: req.path,
      query: req.query,
      params: req.params,
      ip: req.ip,
      userAgent: req.get('User-Agent')
    },
    user: req.user?.id,
    machine: (req as any).machine?.id,
    statusCode,
    errorCode
  });

  // Construire la réponse d'erreur
  const errorResponse: ErrorResponse = {
    success: false,
    error: {
      code: errorCode,
      message,
      ...(details && { details })
    },
    timestamp: new Date().toISOString(),
    path: req.path,
    method: req.method
  };

  // En développement, inclure la stack trace
  if (process.env.NODE_ENV === 'development' && statusCode >= 500) {
    (errorResponse.error as any).stack = error.stack;
  }

  res.status(statusCode).json(errorResponse);
};

/**
 * Middleware pour les routes non trouvées
 */
export const notFoundHandler = (req: Request, res: Response): void => {
  const errorResponse: ErrorResponse = {
    success: false,
    error: {
      code: 'NOT_FOUND',
      message: `Route ${req.method} ${req.path} non trouvée`
    },
    timestamp: new Date().toISOString(),
    path: req.path,
    method: req.method
  };

  logger.warn('Route non trouvée', {
    method: req.method,
    path: req.path,
    query: req.query,
    ip: req.ip,
    userAgent: req.get('User-Agent')
  });

  res.status(404).json(errorResponse);
};

/**
 * Wrapper pour les fonctions async des contrôleurs
 */
export const asyncHandler = (fn: Function) => {
  return (req: Request, res: Response, next: NextFunction) => {
    Promise.resolve(fn(req, res, next)).catch(next);
  };
};
```

## Structure des Routes et Organisation du Code

### Configuration des Routes Principales

```typescript
// src/routes/index.ts
import { Router } from 'express';
import authRoutes from './auth';
import userRoutes from './users';
import deviceRoutes from './devices';
import machineRoutes from './machines';
import legacyRoutes from './legacy';
import notificationRoutes from './notifications';
import firmwareRoutes from './firmware';

const router = Router();

// Routes d'authentification (publiques)
router.use('/auth', authRoutes);

// Routes legacy pour compatibilité boîtiers
router.use('/api', legacyRoutes);

// Routes API modernes (authentifiées)
router.use('/users', userRoutes);
router.use('/devices', deviceRoutes);
router.use('/machines', machineRoutes);
router.use('/notifications', notificationRoutes);
router.use('/firmware', firmwareRoutes);

// Route de santé
router.get('/health', (req, res) => {
  res.json({
    success: true,
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    environment: process.env.NODE_ENV
  });
});

export default router;
```

### Routes des Appareils

```typescript
// src/routes/devices.ts
import { Router } from 'express';
import { DeviceController } from '../controllers/devices/DeviceController';
import { DeviceService } from '../services/devices/DeviceService';
import { ActionService } from '../services/devices/ActionService';
import { authenticate, authorizeMachine } from '../middleware/auth/authenticate';
import { rateLimiter } from '../middleware/security/rateLimiting';
import { validateRequest } from '../middleware/validation/validateRequest';
import { deviceSchemas } from '../middleware/validation/schemas/deviceSchemas';
import { prisma } from '../database/client';
import { wsService } from '../websocket/server';
import { machineComm } from '../services/communication/MachineCommService';

const router = Router();

// Initialiser les services et contrôleurs
const deviceService = new DeviceService(prisma, wsService, machineComm);
const actionService = new ActionService(prisma, machineComm);
const deviceController = new DeviceController(deviceService, actionService);

// Appliquer l'authentification à toutes les routes
router.use(authenticate);

// Routes des appareils
router.get(
  '/machines/:machineId/devices',
  authorizeMachine('machineId'),
  validateRequest(deviceSchemas.getDevices),
  deviceController.getDevices
);

router.get(
  '/:deviceId/status',
  deviceController.getDeviceStatus
);

router.post(
  '/:deviceId/actions',
  rateLimiter.deviceActions, // Limite les actions par appareil
  deviceController.sendDeviceAction
);

router.put(
  '/:deviceId/config',
  deviceController.updateDeviceConfig
);

// Routes d'historique
router.get(
  '/:deviceId/history',
  validateRequest(deviceSchemas.getHistory),
  deviceController.getDeviceHistory
);

router.get(
  '/:deviceId/actions/history',
  validateRequest(deviceSchemas.getActionHistory),
  deviceController.getActionHistory
);

export default router;
```

Cette architecture backend Node.js fournit une base robuste et scalable pour la migration d'Essensys, avec des patterns modernes, une validation stricte des données, une gestion d'erreur centralisée et une organisation claire du code.