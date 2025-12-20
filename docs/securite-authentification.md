# Sécurité et Authentification - Essensys Migration

## Vue d'Ensemble

Cette documentation détaille la conception complète de la sécurité et de l'authentification pour la migration d'Essensys, incluant l'authentification JWT pour utilisateurs et boîtiers, la gestion des rôles et permissions, la migration des mots de passe et la protection contre les attaques communes.

## Authentification JWT pour Utilisateurs et Boîtiers

### Architecture d'Authentification Dual

Le système Essensys nécessite deux types d'authentification distincts :
1. **Authentification Utilisateurs** : Pour les interfaces web et mobiles
2. **Authentification Boîtiers IoT** : Pour les communications machine-to-machine

### Authentification Utilisateurs (JWT)

#### Structure des Tokens JWT

```typescript
// Types pour l'authentification utilisateur
interface UserJWTPayload {
  sub: string;           // User ID (subject)
  email: string;         // Email utilisateur
  roles: string[];       // Rôles globaux
  machines: {            // Machines accessibles
    id: string;
    role: 'owner' | 'admin' | 'user' | 'guest';
    permissions: string[];
  }[];
  iat: number;          // Issued at
  exp: number;          // Expiration
  jti: string;          // JWT ID (pour révocation)
}

interface RefreshTokenPayload {
  sub: string;          // User ID
  type: 'refresh';      // Type de token
  sessionId: string;    // ID de session
  iat: number;
  exp: number;
  jti: string;
}
```

#### Service d'Authentification Utilisateur

```typescript
// src/services/auth/UserAuthService.ts
import jwt from 'jsonwebtoken';
import bcrypt from 'bcrypt';
import { randomBytes } from 'crypto';
import { PrismaClient } from '@prisma/client';
import { RedisClient } from '../cache/RedisClient';
import { logger } from '../../utils/logger';
import { AppError } from '../../utils/errors/AppError';

export class UserAuthService {
  constructor(
    private prisma: PrismaClient,
    private redis: RedisClient
  ) {}

  /**
   * Authentification par email/mot de passe
   */
  async login(email: string, password: string, deviceInfo?: any): Promise<{
    accessToken: string;
    refreshToken: string;
    user: any;
    expiresIn: number;
  }> {
    try {
      // Récupérer l'utilisateur avec ses machines
      const user = await this.prisma.user.findUnique({
        where: { email: email.toLowerCase() },
        include: {
          userMachines: {
            where: {
              machine: { isActive: true }
            },
            include: {
              machine: {
                select: {
                  id: true,
                  serialNumber: true,
                  isActive: true
                }
              }
            }
          }
        }
      });

      if (!user || !user.isActive) {
        await this.logSecurityEvent('login_failed', {
          email,
          reason: 'user_not_found_or_inactive',
          ip: deviceInfo?.ip
        });
        throw new AppError('Identifiants invalides', 401);
      }

      // Vérifier le mot de passe
      const isPasswordValid = await this.verifyPassword(password, user.passwordHash);
      if (!isPasswordValid) {
        await this.logSecurityEvent('login_failed', {
          userId: user.id,
          email,
          reason: 'invalid_password',
          ip: deviceInfo?.ip
        });
        throw new AppError('Identifiants invalides', 401);
      }

      // Vérifier si l'email est vérifié
      if (!user.emailVerified) {
        throw new AppError('Email non vérifié. Veuillez vérifier votre email.', 403);
      }

      // Créer une session
      const sessionId = randomBytes(32).toString('hex');
      const session = await this.createUserSession(user.id, sessionId, deviceInfo);

      // Générer les tokens
      const accessToken = await this.generateAccessToken(user);
      const refreshToken = await this.generateRefreshToken(user.id, sessionId);

      // Mettre à jour les statistiques de connexion
      await this.prisma.user.update({
        where: { id: user.id },
        data: {
          lastLogin: new Date(),
          loginCount: { increment: 1 }
        }
      });

      await this.logSecurityEvent('login_success', {
        userId: user.id,
        email,
        sessionId,
        ip: deviceInfo?.ip
      });

      return {
        accessToken,
        refreshToken,
        user: this.sanitizeUser(user),
        expiresIn: 15 * 60 // 15 minutes
      };

    } catch (error) {
      if (error instanceof AppError) throw error;
      
      logger.error('Erreur lors de la connexion', {
        email,
        error: error.message
      });
      throw new AppError('Erreur interne lors de la connexion', 500);
    }
  }

  /**
   * Rafraîchissement du token d'accès
   */
  async refreshAccessToken(refreshToken: string): Promise<{
    accessToken: string;
    expiresIn: number;
  }> {
    try {
      // Vérifier et décoder le refresh token
      const payload = jwt.verify(
        refreshToken, 
        process.env.JWT_REFRESH_SECRET!
      ) as RefreshTokenPayload;

      // Vérifier que la session existe et est active
      const session = await this.prisma.userSession.findUnique({
        where: { 
          id: payload.sessionId,
          isActive: true,
          expiresAt: { gt: new Date() }
        },
        include: {
          user: {
            include: {
              userMachines: {
                where: {
                  machine: { isActive: true }
                },
                include: {
                  machine: {
                    select: {
                      id: true,
                      serialNumber: true,
                      isActive: true
                    }
                  }
                }
              }
            }
          }
        }
      });

      if (!session || !session.user.isActive) {
        throw new AppError('Session invalide ou expirée', 401);
      }

      // Mettre à jour la dernière utilisation de la session
      await this.prisma.userSession.update({
        where: { id: session.id },
        data: { lastUsedAt: new Date() }
      });

      // Générer un nouveau token d'accès
      const accessToken = await this.generateAccessToken(session.user);

      return {
        accessToken,
        expiresIn: 15 * 60
      };

    } catch (error) {
      if (error instanceof jwt.JsonWebTokenError) {
        throw new AppError('Refresh token invalide', 401);
      }
      throw error;
    }
  }

  /**
   * Déconnexion
   */
  async logout(refreshToken: string): Promise<void> {
    try {
      const payload = jwt.verify(
        refreshToken, 
        process.env.JWT_REFRESH_SECRET!
      ) as RefreshTokenPayload;

      // Désactiver la session
      await this.prisma.userSession.update({
        where: { id: payload.sessionId },
        data: { isActive: false }
      });

      // Ajouter le token à la blacklist (Redis)
      await this.redis.setex(
        `blacklist:${payload.jti}`,
        30 * 24 * 60 * 60, // 30 jours
        'revoked'
      );

      await this.logSecurityEvent('logout', {
        userId: payload.sub,
        sessionId: payload.sessionId
      });

    } catch (error) {
      // Ignorer les erreurs de déconnexion
      logger.warn('Erreur lors de la déconnexion', { error: error.message });
    }
  }

  /**
   * Génération du token d'accès
   */
  private async generateAccessToken(user: any): Promise<string> {
    const payload: UserJWTPayload = {
      sub: user.id,
      email: user.email,
      roles: ['user'], // Rôles globaux si nécessaire
      machines: user.userMachines.map((um: any) => ({
        id: um.machine.id,
        role: um.role,
        permissions: um.permissions || []
      })),
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + (15 * 60), // 15 minutes
      jti: randomBytes(16).toString('hex')
    };

    return jwt.sign(payload, process.env.JWT_SECRET!, {
      algorithm: 'HS256'
    });
  }

  /**
   * Génération du refresh token
   */
  private async generateRefreshToken(userId: string, sessionId: string): Promise<string> {
    const payload: RefreshTokenPayload = {
      sub: userId,
      type: 'refresh',
      sessionId,
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + (30 * 24 * 60 * 60), // 30 jours
      jti: randomBytes(16).toString('hex')
    };

    return jwt.sign(payload, process.env.JWT_REFRESH_SECRET!, {
      algorithm: 'HS256'
    });
  }

  /**
   * Vérification du mot de passe
   */
  private async verifyPassword(plainPassword: string, hashedPassword: string): Promise<boolean> {
    // Support de la migration depuis SHA1
    if (hashedPassword.length === 40) { // SHA1 hash
      const crypto = require('crypto');
      const sha1Hash = crypto.createHash('sha1').update(plainPassword).digest('hex');
      
      if (sha1Hash === hashedPassword) {
        // Migrer vers bcrypt
        const newHash = await bcrypt.hash(plainPassword, 12);
        // Note: La mise à jour sera faite lors de la prochaine connexion
        return true;
      }
      return false;
    }

    // Vérification bcrypt normale
    return bcrypt.compare(plainPassword, hashedPassword);
  }

  /**
   * Création d'une session utilisateur
   */
  private async createUserSession(userId: string, sessionId: string, deviceInfo?: any): Promise<any> {
    const expiresAt = new Date();
    expiresAt.setDate(expiresAt.getDate() + 30); // 30 jours

    return this.prisma.userSession.create({
      data: {
        id: sessionId,
        userId,
        refreshTokenHash: '', // Sera mis à jour après génération
        deviceInfo: deviceInfo || {},
        expiresAt,
        isActive: true
      }
    });
  }

  /**
   * Nettoyage des données utilisateur sensibles
   */
  private sanitizeUser(user: any): any {
    const { passwordHash, securityAnswerHash, ...sanitized } = user;
    return sanitized;
  }

  /**
   * Logging des événements de sécurité
   */
  private async logSecurityEvent(event: string, details: any): Promise<void> {
    logger.warn('Security Event', {
      event,
      ...details,
      timestamp: new Date().toISOString()
    });

    // Optionnel : Stocker en base pour audit
    await this.prisma.auditLog.create({
      data: {
        userId: details.userId || null,
        action: event,
        resourceType: 'authentication',
        newValues: details,
        ipAddress: details.ip,
        timestamp: new Date()
      }
    }).catch(() => {}); // Ignorer les erreurs d'audit
  }
}
```

### Authentification Boîtiers IoT

#### Structure des Tokens Machine

```typescript
// Types pour l'authentification machine
interface MachineJWTPayload {
  sub: string;              // Machine ID
  type: 'machine';          // Type de token
  serialNumber: string;     // Numéro de série
  firmwareVersion: string;  // Version firmware
  capabilities: string[];   // Capacités du boîtier
  iat: number;
  exp: number;
  jti: string;
}
```

#### Service d'Authentification Machine

```typescript
// src/services/auth/MachineAuthService.ts
export class MachineAuthService {
  constructor(
    private prisma: PrismaClient,
    private redis: RedisClient
  ) {}

  /**
   * Authentification par clé d'activation
   */
  async authenticateMachine(activationKey: string, deviceInfo?: any): Promise<{
    machineToken: string;
    machineId: string;
    config: any;
  }> {
    try {
      // Valider le format de la clé
      if (!this.validateActivationKeyFormat(activationKey)) {
        throw new AppError('Format de clé d\'activation invalide', 400);
      }

      // Hasher la clé pour la recherche
      const keyHash = this.hashActivationKey(activationKey);

      // Rechercher la machine
      const machine = await this.prisma.machine.findUnique({
        where: { 
          activationKeyHash: keyHash,
          isActive: true 
        }
      });

      if (!machine) {
        await this.logSecurityEvent('machine_auth_failed', {
          activationKey: activationKey.substring(0, 8) + '***',
          reason: 'invalid_key',
          ip: deviceInfo?.ip
        });
        throw new AppError('Clé d\'activation invalide', 401);
      }

      // Générer le token machine
      const machineToken = await this.generateMachineToken(machine);

      // Mettre à jour les informations de connexion
      await this.prisma.machine.update({
        where: { id: machine.id },
        data: {
          lastConnection: new Date(),
          lastIpAddress: deviceInfo?.ip,
          connectionCount: { increment: 1 }
        }
      });

      // Configuration pour le boîtier
      const config = {
        pollInterval: 30, // secondes
        maxRetries: 3,
        timeout: 10000, // ms
        endpoints: {
          actions: '/api/myactions',
          status: '/api/mystatus',
          serverInfo: '/api/serverinfos'
        }
      };

      await this.logSecurityEvent('machine_auth_success', {
        machineId: machine.id,
        serialNumber: machine.serialNumber,
        ip: deviceInfo?.ip
      });

      return {
        machineToken,
        machineId: machine.id,
        config
      };

    } catch (error) {
      if (error instanceof AppError) throw error;
      
      logger.error('Erreur lors de l\'authentification machine', {
        error: error.message
      });
      throw new AppError('Erreur interne lors de l\'authentification', 500);
    }
  }

  /**
   * Validation du token machine
   */
  async validateMachineToken(token: string): Promise<MachineJWTPayload> {
    try {
      // Vérifier le token
      const payload = jwt.verify(
        token, 
        process.env.MACHINE_JWT_SECRET!
      ) as MachineJWTPayload;

      // Vérifier que la machine existe toujours
      const machine = await this.prisma.machine.findUnique({
        where: { 
          id: payload.sub,
          isActive: true 
        }
      });

      if (!machine) {
        throw new AppError('Machine non trouvée ou inactive', 401);
      }

      return payload;

    } catch (error) {
      if (error instanceof jwt.JsonWebTokenError) {
        throw new AppError('Token machine invalide', 401);
      }
      throw error;
    }
  }

  /**
   * Génération du token machine
   */
  private async generateMachineToken(machine: any): Promise<string> {
    const payload: MachineJWTPayload = {
      sub: machine.id,
      type: 'machine',
      serialNumber: machine.serialNumber,
      firmwareVersion: machine.firmwareVersion,
      capabilities: ['actions', 'status', 'firmware_update'], // Basé sur le firmware
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + (24 * 60 * 60), // 24 heures
      jti: randomBytes(16).toString('hex')
    };

    return jwt.sign(payload, process.env.MACHINE_JWT_SECRET!, {
      algorithm: 'HS256'
    });
  }

  /**
   * Validation du format de clé d'activation
   */
  private validateActivationKeyFormat(key: string): boolean {
    const pattern = /^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/;
    return pattern.test(key);
  }

  /**
   * Hashage de la clé d'activation
   */
  private hashActivationKey(key: string): string {
    const crypto = require('crypto');
    const salt = process.env.ACTIVATION_KEY_SALT || 'essensys_salt';
    return crypto.createHash('sha256').update(key + salt).digest('hex');
  }

  /**
   * Logging des événements de sécurité machine
   */
  private async logSecurityEvent(event: string, details: any): Promise<void> {
    logger.warn('Machine Security Event', {
      event,
      ...details,
      timestamp: new Date().toISOString()
    });
  }
}
```

## Rôles, Permissions et Politiques d'Autorisation

### Système de Rôles Hiérarchique

```typescript
// Types pour les rôles et permissions
type UserRole = 'owner' | 'admin' | 'user' | 'guest';

interface Permission {
  resource: string;     // 'device', 'machine', 'user', etc.
  action: string;       // 'read', 'write', 'delete', 'control'
  conditions?: any;     // Conditions supplémentaires
}

interface RoleDefinition {
  name: UserRole;
  description: string;
  permissions: Permission[];
  inherits?: UserRole[];
}
```

### Définition des Rôles

```typescript
// src/services/auth/RoleService.ts
export class RoleService {
  private static readonly ROLE_DEFINITIONS: Record<UserRole, RoleDefinition> = {
    owner: {
      name: 'owner',
      description: 'Propriétaire de la machine - accès complet',
      permissions: [
        { resource: 'machine', action: '*' },
        { resource: 'device', action: '*' },
        { resource: 'user', action: 'invite' },
        { resource: 'user', action: 'remove' },
        { resource: 'settings', action: '*' },
        { resource: 'firmware', action: 'update' },
        { resource: 'logs', action: 'read' }
      ]
    },

    admin: {
      name: 'admin',
      description: 'Administrateur - gestion des appareils et utilisateurs',
      permissions: [
        { resource: 'machine', action: 'read' },
        { resource: 'device', action: '*' },
        { resource: 'user', action: 'invite' },
        { resource: 'settings', action: 'write' },
        { resource: 'logs', action: 'read' }
      ]
    },

    user: {
      name: 'user',
      description: 'Utilisateur standard - contrôle des appareils',
      permissions: [
        { resource: 'machine', action: 'read' },
        { resource: 'device', action: 'read' },
        { resource: 'device', action: 'control' },
        { resource: 'settings', action: 'read' }
      ]
    },

    guest: {
      name: 'guest',
      description: 'Invité - lecture seule',
      permissions: [
        { resource: 'machine', action: 'read' },
        { resource: 'device', action: 'read' }
      ]
    }
  };

  /**
   * Vérifier si un utilisateur a une permission spécifique
   */
  static hasPermission(
    userRole: UserRole, 
    resource: string, 
    action: string
  ): boolean {
    const roleDefinition = this.ROLE_DEFINITIONS[userRole];
    if (!roleDefinition) return false;

    return roleDefinition.permissions.some(permission => {
      const resourceMatch = permission.resource === resource || permission.resource === '*';
      const actionMatch = permission.action === action || permission.action === '*';
      return resourceMatch && actionMatch;
    });
  }

  /**
   * Obtenir toutes les permissions d'un rôle
   */
  static getRolePermissions(role: UserRole): Permission[] {
    const roleDefinition = this.ROLE_DEFINITIONS[role];
    return roleDefinition ? roleDefinition.permissions : [];
  }

  /**
   * Vérifier si un rôle peut effectuer une action sur une ressource
   */
  static canAccess(
    userRole: UserRole,
    resource: string,
    action: string,
    context?: any
  ): boolean {
    // Vérification de base des permissions
    if (!this.hasPermission(userRole, resource, action)) {
      return false;
    }

    // Logique contextuelle supplémentaire
    if (context) {
      // Par exemple, un utilisateur ne peut contrôler que ses propres appareils
      if (resource === 'device' && action === 'control') {
        return context.deviceOwnedByUser === true;
      }
    }

    return true;
  }
}
```

### Middleware d'Autorisation

```typescript
// src/middleware/auth/authorization.ts
import { Request, Response, NextFunction } from 'express';
import { RoleService } from '../../services/auth/RoleService';
import { AppError } from '../../utils/errors/AppError';

/**
 * Middleware d'autorisation basé sur les ressources
 */
export const authorize = (resource: string, action: string) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      if (!req.user) {
        throw new AppError('Authentification requise', 401);
      }

      // Déterminer le rôle de l'utilisateur pour la ressource demandée
      let userRole: UserRole = 'guest';
      
      if (req.params.machineId) {
        const machineAccess = req.user.machines.find(
          m => m.id === req.params.machineId
        );
        userRole = machineAccess?.role || 'guest';
      }

      // Vérifier les permissions
      const hasPermission = RoleService.canAccess(userRole, resource, action, {
        userId: req.user.id,
        machineId: req.params.machineId,
        deviceId: req.params.deviceId
      });

      if (!hasPermission) {
        logger.warn('Accès refusé', {
          userId: req.user.id,
          userRole,
          resource,
          action,
          path: req.path
        });
        throw new AppError('Permissions insuffisantes', 403);
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

/**
 * Middleware pour vérifier l'accès à une machine spécifique
 */
export const requireMachineAccess = (minimumRole: UserRole = 'guest') => {
  return (req: Request, res: Response, next: NextFunction): void => {
    if (!req.user) {
      return next(new AppError('Authentification requise', 401));
    }

    const machineId = req.params.machineId;
    if (!machineId) {
      return next(new AppError('ID de machine requis', 400));
    }

    const machineAccess = req.user.machines.find(m => m.id === machineId);
    if (!machineAccess) {
      return next(new AppError('Accès non autorisé à cette machine', 403));
    }

    // Vérifier le niveau de rôle minimum
    const roleHierarchy: Record<UserRole, number> = {
      guest: 1,
      user: 2,
      admin: 3,
      owner: 4
    };

    if (roleHierarchy[machineAccess.role] < roleHierarchy[minimumRole]) {
      return next(new AppError('Niveau d\'autorisation insuffisant', 403));
    }

    // Ajouter les informations d'accès à la requête
    (req as any).machineAccess = machineAccess;
    next();
  };
};
```

## Migration des Mots de Passe et Clés d'Activation

### Stratégie de Migration des Mots de Passe

```typescript
// src/services/migration/PasswordMigrationService.ts
export class PasswordMigrationService {
  constructor(private prisma: PrismaClient) {}

  /**
   * Migration progressive des mots de passe SHA1 vers bcrypt
   */
  async migrateUserPassword(userId: string, plainPassword: string): Promise<void> {
    try {
      const user = await this.prisma.user.findUnique({
        where: { id: userId }
      });

      if (!user) {
        throw new AppError('Utilisateur non trouvé', 404);
      }

      // Vérifier si le mot de passe est encore en SHA1
      if (user.passwordHash.length === 40) { // SHA1 = 40 caractères hex
        const crypto = require('crypto');
        const sha1Hash = crypto.createHash('sha1').update(plainPassword).digest('hex');
        
        if (sha1Hash === user.passwordHash) {
          // Migrer vers bcrypt
          const newHash = await bcrypt.hash(plainPassword, 12);
          
          await this.prisma.user.update({
            where: { id: userId },
            data: { 
              passwordHash: newHash,
              updatedAt: new Date()
            }
          });

          logger.info('Mot de passe migré vers bcrypt', { userId });
        }
      }
    } catch (error) {
      logger.error('Erreur lors de la migration du mot de passe', {
        userId,
        error: error.message
      });
    }
  }

  /**
   * Migration des clés d'activation
   */
  async migrateActivationKeys(): Promise<void> {
    try {
      // Récupérer toutes les machines avec des clés non hashées
      const machines = await this.prisma.machine.findMany({
        where: {
          activationKeyHash: null // Ou critère pour identifier les anciennes clés
        }
      });

      for (const machine of machines) {
        if (machine.activationKey) {
          const hashedKey = this.hashActivationKey(machine.activationKey);
          
          await this.prisma.machine.update({
            where: { id: machine.id },
            data: {
              activationKeyHash: hashedKey,
              updatedAt: new Date()
            }
          });
        }
      }

      logger.info('Migration des clés d\'activation terminée', {
        migratedCount: machines.length
      });

    } catch (error) {
      logger.error('Erreur lors de la migration des clés', {
        error: error.message
      });
    }
  }

  /**
   * Hashage sécurisé des clés d'activation
   */
  private hashActivationKey(key: string): string {
    const crypto = require('crypto');
    const salt = process.env.ACTIVATION_KEY_SALT || 'essensys_default_salt';
    return crypto.createHash('sha256').update(key + salt).digest('hex');
  }
}
```

### Script de Migration

```sql
-- Script SQL pour la migration des données d'authentification

-- 1. Ajouter une colonne pour marquer les mots de passe à migrer
ALTER TABLE users ADD COLUMN IF NOT EXISTS password_needs_migration BOOLEAN DEFAULT true;

-- 2. Marquer les mots de passe bcrypt existants comme migrés
UPDATE users 
SET password_needs_migration = false 
WHERE length(password_hash) > 40; -- bcrypt hashes sont plus longs que SHA1

-- 3. Fonction pour migrer un mot de passe lors de la connexion
CREATE OR REPLACE FUNCTION migrate_user_password(
    user_email VARCHAR(255),
    plain_password VARCHAR(255)
) RETURNS BOOLEAN AS $$
DECLARE
    user_record RECORD;
    sha1_hash VARCHAR(40);
    bcrypt_hash VARCHAR(255);
BEGIN
    -- Récupérer l'utilisateur
    SELECT * INTO user_record FROM users WHERE email = user_email;
    
    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;
    
    -- Si le mot de passe n'a pas besoin de migration, vérifier normalement
    IF NOT user_record.password_needs_migration THEN
        RETURN crypt(plain_password, user_record.password_hash) = user_record.password_hash;
    END IF;
    
    -- Calculer le hash SHA1 pour comparaison
    sha1_hash := encode(digest(plain_password, 'sha1'), 'hex');
    
    -- Vérifier si le mot de passe SHA1 correspond
    IF sha1_hash = user_record.password_hash THEN
        -- Générer un nouveau hash bcrypt
        bcrypt_hash := crypt(plain_password, gen_salt('bf', 12));
        
        -- Mettre à jour avec le nouveau hash
        UPDATE users 
        SET password_hash = bcrypt_hash,
            password_needs_migration = false,
            updated_at = NOW()
        WHERE id = user_record.id;
        
        RETURN TRUE;
    END IF;
    
    RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- 4. Fonction pour hasher les clés d'activation
CREATE OR REPLACE FUNCTION hash_activation_key(key VARCHAR(39)) 
RETURNS VARCHAR(64) AS $$
BEGIN
    RETURN encode(digest(key || 'essensys_salt', 'sha256'), 'hex');
END;
$$ LANGUAGE plpgsql;

-- 5. Migrer les clés d'activation existantes
UPDATE machines 
SET activation_key_hash = hash_activation_key(activation_key)
WHERE activation_key_hash IS NULL AND activation_key IS NOT NULL;
```

## Protection contre les Attaques Communes

### Protection CSRF (Cross-Site Request Forgery)

```typescript
// src/middleware/security/csrf.ts
import csrf from 'csurf';
import { Request, Response, NextFunction } from 'express';

// Configuration CSRF pour les sessions web
export const csrfProtection = csrf({
  cookie: {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict'
  },
  ignoreMethods: ['GET', 'HEAD', 'OPTIONS'],
  skip: (req) => {
    // Ignorer CSRF pour les APIs machine (authentifiées par JWT)
    return req.headers['x-machine-token'] !== undefined;
  }
});

// Middleware pour fournir le token CSRF au frontend
export const provideCsrfToken = (req: Request, res: Response, next: NextFunction) => {
  res.locals.csrfToken = req.csrfToken();
  next();
};
```

### Protection XSS (Cross-Site Scripting)

```typescript
// src/middleware/security/xss.ts
import helmet from 'helmet';
import { Request, Response, NextFunction } from 'express';

// Configuration Helmet pour la sécurité des headers
export const xssProtection = helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
      fontSrc: ["'self'", "https://fonts.gstatic.com"],
      imgSrc: ["'self'", "data:", "https:"],
      scriptSrc: ["'self'"],
      connectSrc: ["'self'", "wss:", "ws:"],
      frameSrc: ["'none'"],
      objectSrc: ["'none'"],
      baseUri: ["'self'"],
      formAction: ["'self'"]
    }
  },
  crossOriginEmbedderPolicy: false, // Pour compatibilité
  hsts: {
    maxAge: 31536000,
    includeSubDomains: true,
    preload: true
  }
});

// Sanitisation des entrées utilisateur
export const sanitizeInput = (req: Request, res: Response, next: NextFunction) => {
  const sanitizeValue = (value: any): any => {
    if (typeof value === 'string') {
      // Échapper les caractères HTML dangereux
      return value
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;')
        .replace(/\//g, '&#x2F;');
    }
    
    if (typeof value === 'object' && value !== null) {
      const sanitized: any = Array.isArray(value) ? [] : {};
      for (const key in value) {
        sanitized[key] = sanitizeValue(value[key]);
      }
      return sanitized;
    }
    
    return value;
  };

  // Sanitiser les données d'entrée
  if (req.body) {
    req.body = sanitizeValue(req.body);
  }
  
  if (req.query) {
    req.query = sanitizeValue(req.query);
  }

  next();
};
```

### Rate Limiting et Protection DDoS

```typescript
// src/middleware/security/rateLimiting.ts
import rateLimit from 'express-rate-limit';
import RedisStore from 'rate-limit-redis';
import { RedisClient } from '../../services/cache/RedisClient';

const redis = new RedisClient();

// Rate limiting général
export const generalRateLimit = rateLimit({
  store: new RedisStore({
    sendCommand: (...args: string[]) => redis.call(...args),
  }),
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // 100 requêtes par IP
  message: {
    error: 'Trop de requêtes, veuillez réessayer plus tard',
    retryAfter: '15 minutes'
  },
  standardHeaders: true,
  legacyHeaders: false,
});

// Rate limiting strict pour l'authentification
export const authRateLimit = rateLimit({
  store: new RedisStore({
    sendCommand: (...args: string[]) => redis.call(...args),
  }),
  windowMs: 15 * 60 * 1000,
  max: 5, // 5 tentatives de connexion par IP
  skipSuccessfulRequests: true,
  message: {
    error: 'Trop de tentatives de connexion, veuillez réessayer plus tard',
    retryAfter: '15 minutes'
  }
});

// Rate limiting pour les actions d'appareils
export const deviceActionRateLimit = rateLimit({
  store: new RedisStore({
    sendCommand: (...args: string[]) => redis.call(...args),
  }),
  windowMs: 60 * 1000, // 1 minute
  max: 30, // 30 actions par minute par utilisateur
  keyGenerator: (req) => {
    return req.user?.id || req.ip;
  },
  message: {
    error: 'Trop d\'actions sur les appareils, veuillez ralentir',
    retryAfter: '1 minute'
  }
});

// Rate limiting spécifique aux machines IoT
export const machineRateLimit = rateLimit({
  store: new RedisStore({
    sendCommand: (...args: string[]) => redis.call(...args),
  }),
  windowMs: 60 * 1000, // 1 minute
  max: 60, // 60 requêtes par minute par machine
  keyGenerator: (req) => {
    return (req as any).machine?.id || req.ip;
  },
  message: {
    error: 'Trop de requêtes de cette machine',
    retryAfter: '1 minute'
  }
});
```

### Détection et Prévention des Intrusions

```typescript
// src/services/security/IntrusionDetectionService.ts
export class IntrusionDetectionService {
  constructor(
    private redis: RedisClient,
    private prisma: PrismaClient
  ) {}

  /**
   * Détecter les tentatives de force brute
   */
  async detectBruteForce(ip: string, userId?: string): Promise<boolean> {
    const key = `brute_force:${ip}:${userId || 'anonymous'}`;
    const attempts = await this.redis.incr(key);
    
    if (attempts === 1) {
      await this.redis.expire(key, 3600); // 1 heure
    }

    // Seuil d'alerte
    if (attempts >= 10) {
      await this.triggerSecurityAlert('brute_force_detected', {
        ip,
        userId,
        attempts
      });
      return true;
    }

    return false;
  }

  /**
   * Détecter les anomalies de connexion
   */
  async detectAnomalousLogin(userId: string, loginInfo: any): Promise<void> {
    try {
      // Récupérer l'historique des connexions
      const recentLogins = await this.prisma.auditLog.findMany({
        where: {
          userId,
          action: 'login_success',
          timestamp: {
            gte: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) // 30 jours
          }
        },
        orderBy: { timestamp: 'desc' },
        take: 10
      });

      // Analyser les patterns
      const suspiciousFactors = [];

      // Nouvelle géolocalisation
      const knownIPs = recentLogins.map(log => log.ipAddress).filter(Boolean);
      if (loginInfo.ip && !knownIPs.includes(loginInfo.ip)) {
        suspiciousFactors.push('new_ip');
      }

      // Heure inhabituelle
      const loginHour = new Date().getHours();
      const usualHours = recentLogins.map(log => new Date(log.timestamp).getHours());
      const isUnusualHour = usualHours.length > 0 && 
        !usualHours.some(hour => Math.abs(hour - loginHour) <= 2);
      
      if (isUnusualHour) {
        suspiciousFactors.push('unusual_time');
      }

      // Déclencher une alerte si nécessaire
      if (suspiciousFactors.length >= 2) {
        await this.triggerSecurityAlert('anomalous_login', {
          userId,
          factors: suspiciousFactors,
          loginInfo
        });
      }

    } catch (error) {
      logger.error('Erreur lors de la détection d\'anomalies', {
        userId,
        error: error.message
      });
    }
  }

  /**
   * Détecter les tentatives d'accès non autorisé
   */
  async detectUnauthorizedAccess(req: any): Promise<void> {
    const suspiciousPatterns = [
      /\.\./,                    // Path traversal
      /<script/i,                // XSS attempts
      /union.*select/i,          // SQL injection
      /exec\s*\(/i,             // Command injection
      /eval\s*\(/i              // Code injection
    ];

    const url = req.url;
    const body = JSON.stringify(req.body || {});
    const query = JSON.stringify(req.query || {});

    for (const pattern of suspiciousPatterns) {
      if (pattern.test(url) || pattern.test(body) || pattern.test(query)) {
        await this.triggerSecurityAlert('malicious_request', {
          ip: req.ip,
          url,
          method: req.method,
          userAgent: req.get('User-Agent'),
          pattern: pattern.toString()
        });
        break;
      }
    }
  }

  /**
   * Déclencher une alerte de sécurité
   */
  private async triggerSecurityAlert(type: string, details: any): Promise<void> {
    logger.error('Alerte de sécurité', {
      type,
      details,
      timestamp: new Date().toISOString()
    });

    // Stocker l'alerte en base
    await this.prisma.auditLog.create({
      data: {
        action: `security_alert_${type}`,
        resourceType: 'security',
        newValues: details,
        ipAddress: details.ip,
        severity: 'critical',
        timestamp: new Date()
      }
    }).catch(() => {}); // Ignorer les erreurs

    // Notifier les administrateurs (implémentation selon les besoins)
    // await this.notificationService.sendSecurityAlert(type, details);
  }
}
```

### Configuration de Sécurité Globale

```typescript
// src/config/security.ts
export const securityConfig = {
  // JWT
  jwt: {
    secret: process.env.JWT_SECRET!,
    refreshSecret: process.env.JWT_REFRESH_SECRET!,
    accessTokenExpiry: '15m',
    refreshTokenExpiry: '30d',
    algorithm: 'HS256' as const
  },

  // Mots de passe
  password: {
    minLength: 8,
    requireUppercase: true,
    requireLowercase: true,
    requireNumbers: true,
    requireSpecialChars: false,
    bcryptRounds: 12
  },

  // Sessions
  session: {
    maxConcurrentSessions: 5,
    inactivityTimeout: 30 * 60 * 1000, // 30 minutes
    absoluteTimeout: 8 * 60 * 60 * 1000 // 8 heures
  },

  // Rate limiting
  rateLimit: {
    general: { windowMs: 15 * 60 * 1000, max: 100 },
    auth: { windowMs: 15 * 60 * 1000, max: 5 },
    deviceActions: { windowMs: 60 * 1000, max: 30 },
    machines: { windowMs: 60 * 1000, max: 60 }
  },

  // CORS
  cors: {
    origin: process.env.CORS_ORIGIN?.split(',') || ['http://localhost:3000'],
    credentials: true,
    optionsSuccessStatus: 200
  },

  // Headers de sécurité
  security: {
    hsts: {
      maxAge: 31536000,
      includeSubDomains: true,
      preload: true
    },
    csp: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'"],
      imgSrc: ["'self'", "data:", "https:"],
      connectSrc: ["'self'", "wss:", "ws:"]
    }
  }
};
```

Cette architecture de sécurité et d'authentification fournit une protection complète et moderne pour la migration d'Essensys, avec une authentification robuste pour les utilisateurs et les boîtiers IoT, un système de permissions granulaire et des protections avancées contre les attaques communes.