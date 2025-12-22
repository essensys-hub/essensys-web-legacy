// Types de base pour l'application Essensys

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  address: {
    line1: string;
    line2?: string;
    postalCode: string;
    city: string;
  };
  phone?: string;
  securityQuestion: string;
  isActive: boolean;
  emailVerified: boolean;
  createdAt: Date;
  updatedAt: Date;
  lastLogin?: Date;
  machines: UserMachine[];
}

export interface Machine {
  id: string;
  serialNumber: string;
  activationKey: string;
  firmwareVersion: string;
  isActive: boolean;
  alarmEnabled: boolean;
  timezone: string;
  createdAt: Date;
  updatedAt: Date;
  lastConnection?: Date;
  connectionCount: number;
  lastIpAddress?: string;
  hardwareRevision?: string;
  devices: Device[];
}

export interface UserMachine {
  id: string;
  userId: string;
  machineId: string;
  role: UserRole;
  permissions: Record<string, any>;
  createdAt: Date;
}

export interface Device {
  id: string;
  machineId: string;
  deviceType: DeviceType;
  name: string;
  zone?: string;
  config: Record<string, any>;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  currentState?: DeviceState;
}

export interface DeviceType {
  id: string;
  name: string;
  displayName: string;
  category: DeviceCategory;
  icon?: string;
  configSchema: Record<string, any>;
  createdAt: Date;
}

export interface DeviceState {
  id: string;
  deviceId: string;
  stateData: Record<string, any>;
  timestamp: Date;
}

export interface Action {
  id: string;
  machineId: string;
  deviceId?: string;
  actionType: string;
  payload: Record<string, any>;
  status: ActionStatus;
  priority: number;
  createdAt: Date;
  sentAt?: Date;
  executedAt?: Date;
  retryCount: number;
  maxRetries: number;
  errorMessage?: string;
}

export interface NotificationContact {
  id: string;
  userId: string;
  type: NotificationType;
  contactValue: string;
  displayName?: string;
  isVerified: boolean;
  isActive: boolean;
  preferences: Record<string, any>;
  createdAt: Date;
}

export interface Notification {
  id: string;
  contactId: string;
  type: NotificationType;
  subject?: string;
  message: string;
  status: NotificationStatus;
  sentAt?: Date;
  deliveredAt?: Date;
  errorMessage?: string;
}

// Enums et types union
export type UserRole = 'owner' | 'user' | 'guest';
export type DeviceCategory = 'climate' | 'security' | 'comfort';
export type ActionStatus = 'pending' | 'sent' | 'executed' | 'failed';
export type NotificationType = 'sms' | 'email' | 'push';
export type NotificationStatus = 'pending' | 'sent' | 'delivered' | 'failed';

// Types pour l'authentification
export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface UserPayload {
  userId: string;
  email: string;
  roles: string[];
  machineIds: string[];
  permissions: Permission[];
}

export interface MachinePayload {
  machineId: string;
  serialNumber: string;
  firmwareVersion: string;
  lastConnection: Date;
  allowedEndpoints: string[];
}

export interface Permission {
  resource: string;
  actions: string[];
}

// Types pour les requêtes API
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  address: {
    line1: string;
    line2?: string;
    postalCode: string;
    city: string;
  };
  phone?: string;
  securityQuestion: string;
  securityAnswer: string;
  activationKey: string;
}

export interface CreateActionRequest {
  machineId: string;
  deviceId?: string;
  actionType: string;
  payload: Record<string, any>;
  priority?: number;
}

export interface UpdateDeviceStateRequest {
  deviceId: string;
  stateData: Record<string, any>;
}

// Types pour les réponses API
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
}

// Types pour la configuration
export interface DatabaseConfig {
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  ssl?: boolean;
}

export interface RedisConfig {
  host: string;
  port: number;
  password?: string;
  db?: number;
}

export interface JwtConfig {
  accessTokenSecret: string;
  refreshTokenSecret: string;
  accessTokenExpiry: string;
  refreshTokenExpiry: string;
}

// Types pour les erreurs
export interface AppError extends Error {
  statusCode: number;
  isOperational: boolean;
}

export interface ValidationError {
  field: string;
  message: string;
  value?: any;
}