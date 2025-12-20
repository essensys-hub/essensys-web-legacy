# Spécifications API - Migration Essensys

## Vue d'Ensemble

Ce document fournit la documentation complète des APIs REST pour l'application Essensys moderne. Il inclut les spécifications OpenAPI, les exemples d'utilisation, les codes d'erreur et les collections Postman pour faciliter le développement et les tests.

## Spécification OpenAPI 3.0

### Configuration de Base

```yaml
openapi: 3.0.3
info:
  title: Essensys API
  description: API REST pour l'application de domotique Essensys
  version: 1.0.0
  contact:
    name: Équipe Essensys
    email: dev@essensys.com
  license:
    name: MIT
    url: https://opensource.org/licenses/MIT

servers:
  - url: http://localhost:3001/api
    description: Serveur de développement
  - url: https://staging-api.essensys.com/api
    description: Serveur de staging
  - url: https://api.essensys.com/api
    description: Serveur de production

security:
  - BearerAuth: []
  - MachineAuth: []

components:
  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      description: Token JWT pour l'authentification utilisateur
    
    MachineAuth:
      type: apiKey
      in: header
      name: X-Machine-Token
      description: Token JWT pour l'authentification des boîtiers IoT
```
### Schémas de Données

```yaml
components:
  schemas:
    # Réponses API standardisées
    ApiResponse:
      type: object
      properties:
        success:
          type: boolean
          description: Indique si la requête a réussi
        data:
          description: Données de la réponse (varie selon l'endpoint)
        message:
          type: string
          description: Message descriptif optionnel
        pagination:
          $ref: '#/components/schemas/Pagination'
      required:
        - success

    ApiError:
      type: object
      properties:
        success:
          type: boolean
          example: false
        error:
          type: string
          description: Message d'erreur principal
        details:
          type: array
          items:
            type: object
          description: Détails supplémentaires sur l'erreur
        code:
          type: string
          description: Code d'erreur spécifique
      required:
        - success
        - error

    Pagination:
      type: object
      properties:
        page:
          type: integer
          minimum: 1
          description: Numéro de page actuelle
        limit:
          type: integer
          minimum: 1
          maximum: 100
          description: Nombre d'éléments par page
        total:
          type: integer
          minimum: 0
          description: Nombre total d'éléments
        totalPages:
          type: integer
          minimum: 0
          description: Nombre total de pages
      required:
        - page
        - limit
        - total
        - totalPages

    # Modèles métier
    User:
      type: object
      properties:
        id:
          type: string
          format: uuid
          description: Identifiant unique de l'utilisateur
        email:
          type: string
          format: email
          description: Adresse email de l'utilisateur
        firstName:
          type: string
          maxLength: 255
          description: Prénom de l'utilisateur
        lastName:
          type: string
          maxLength: 255
          description: Nom de famille de l'utilisateur
        address:
          $ref: '#/components/schemas/Address'
        phone:
          type: string
          pattern: '^(\+33|0)[1-9](\d{8})$'
          description: Numéro de téléphone français
        isActive:
          type: boolean
          description: Indique si le compte utilisateur est actif
        emailVerified:
          type: boolean
          description: Indique si l'email a été vérifié
        createdAt:
          type: string
          format: date-time
          description: Date de création du compte
        updatedAt:
          type: string
          format: date-time
          description: Date de dernière modification
        lastLogin:
          type: string
          format: date-time
          description: Date de dernière connexion
        machines:
          type: array
          items:
            $ref: '#/components/schemas/UserMachine'
          description: Machines associées à l'utilisateur
      required:
        - id
        - email
        - firstName
        - lastName
        - isActive
        - emailVerified
        - createdAt
        - updatedAt

    Address:
      type: object
      properties:
        line1:
          type: string
          maxLength: 255
          description: Première ligne d'adresse
        line2:
          type: string
          maxLength: 255
          description: Deuxième ligne d'adresse (optionnelle)
        postalCode:
          type: string
          pattern: '^[0-9]{5}$'
          description: Code postal français
        city:
          type: string
          maxLength: 255
          description: Ville
      required:
        - line1
        - postalCode
        - city

    Machine:
      type: object
      properties:
        id:
          type: string
          format: uuid
          description: Identifiant unique de la machine
        serialNumber:
          type: string
          maxLength: 255
          description: Numéro de série du boîtier
        activationKey:
          type: string
          pattern: '^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$'
          description: Clé d'activation (32 caractères)
        firmwareVersion:
          type: string
          maxLength: 50
          description: Version du firmware installé
        isActive:
          type: boolean
          description: Indique si la machine est active
        alarmEnabled:
          type: boolean
          description: Indique si l'alarme est activée
        timezone:
          type: string
          default: Europe/Paris
          description: Fuseau horaire de la machine
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
        lastConnection:
          type: string
          format: date-time
          description: Date de dernière connexion
        connectionCount:
          type: integer
          minimum: 0
          description: Nombre total de connexions
        lastIpAddress:
          type: string
          format: ipv4
          description: Dernière adresse IP de connexion
        devices:
          type: array
          items:
            $ref: '#/components/schemas/Device'
      required:
        - id
        - serialNumber
        - activationKey
        - firmwareVersion
        - isActive
        - alarmEnabled
        - timezone
        - createdAt
        - updatedAt

    Device:
      type: object
      properties:
        id:
          type: string
          format: uuid
        machineId:
          type: string
          format: uuid
        deviceType:
          $ref: '#/components/schemas/DeviceType'
        name:
          type: string
          maxLength: 255
          description: Nom personnalisé de l'appareil
        zone:
          type: string
          maxLength: 100
          description: Zone où se trouve l'appareil
        config:
          type: object
          description: Configuration spécifique à l'appareil
        isActive:
          type: boolean
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
        currentState:
          type: object
          description: État actuel de l'appareil
      required:
        - id
        - machineId
        - deviceType
        - name
        - config
        - isActive
        - createdAt
        - updatedAt

    DeviceType:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
          enum: [heating_zone, shutter, alarm_system, water_heater]
        displayName:
          type: string
          description: Nom d'affichage localisé
        category:
          type: string
          enum: [climate, security, comfort, energy]
        icon:
          type: string
          description: Nom de l'icône à afficher
        configSchema:
          type: object
          description: Schéma JSON de configuration
      required:
        - id
        - name
        - displayName
        - category

    Action:
      type: object
      properties:
        id:
          type: string
          format: uuid
        machineId:
          type: string
          format: uuid
        deviceId:
          type: string
          format: uuid
        actionType:
          type: string
          description: Type d'action à exécuter
        payload:
          type: object
          description: Paramètres de l'action
        status:
          type: string
          enum: [pending, sent, executed, failed]
        priority:
          type: integer
          minimum: 1
          maximum: 10
          default: 5
        createdAt:
          type: string
          format: date-time
        sentAt:
          type: string
          format: date-time
        executedAt:
          type: string
          format: date-time
        retryCount:
          type: integer
          minimum: 0
        maxRetries:
          type: integer
          minimum: 0
          default: 3
        errorMessage:
          type: string
      required:
        - id
        - machineId
        - actionType
        - payload
        - status
        - priority
        - createdAt
        - retryCount
        - maxRetries
```
## Endpoints API

### Authentification

#### POST /auth/login
Authentifie un utilisateur et retourne des tokens JWT.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "firstName": "Jean",
      "lastName": "Dupont"
    },
    "expiresIn": 900
  }
}
```

**Errors:**
- 400: Données invalides
- 401: Email ou mot de passe incorrect
- 429: Trop de tentatives de connexion

#### POST /auth/refresh
Rafraîchit le token d'accès avec un refresh token.

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 900
  }
}
```

#### POST /auth/logout
Déconnecte l'utilisateur et invalide le refresh token.

**Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Déconnexion réussie"
}
```

### Gestion des Machines

#### GET /machines
Liste toutes les machines accessibles par l'utilisateur.

**Query Parameters:**
- `page` (integer, default: 1): Numéro de page
- `limit` (integer, default: 20): Éléments par page
- `isActive` (boolean): Filtrer par statut actif

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "serialNumber": "ESS-2024-001",
      "firmwareVersion": "V2.1.0",
      "isActive": true,
      "alarmEnabled": true,
      "lastConnection": "2024-12-20T10:30:00Z",
      "devices": []
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 1,
    "totalPages": 1
  }
}
```

#### GET /machines/{machineId}
Récupère les détails d'une machine spécifique.

**Path Parameters:**
- `machineId` (uuid, required): ID de la machine

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "serialNumber": "ESS-2024-001",
    "activationKey": "ABCD-1234-EFGH-5678-IJKL-9012-MNOP-3456",
    "firmwareVersion": "V2.1.0",
    "isActive": true,
    "alarmEnabled": true,
    "timezone": "Europe/Paris",
    "createdAt": "2024-01-15T08:00:00Z",
    "updatedAt": "2024-12-20T10:30:00Z",
    "lastConnection": "2024-12-20T10:30:00Z",
    "connectionCount": 1523,
    "lastIpAddress": "192.168.1.100",
    "devices": [
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "name": "Chauffage Salon",
        "zone": "salon",
        "deviceType": {
          "name": "heating_zone",
          "displayName": "Zone de Chauffage",
          "category": "climate"
        },
        "isActive": true
      }
    ]
  }
}
```

**Errors:**
- 404: Machine non trouvée
- 403: Accès non autorisé à cette machine

### Gestion des Appareils

#### GET /machines/{machineId}/devices
Liste tous les appareils d'une machine.

**Path Parameters:**
- `machineId` (uuid, required): ID de la machine

**Query Parameters:**
- `page` (integer, default: 1)
- `limit` (integer, default: 20)
- `category` (string): Filtrer par catégorie (climate, security, comfort, energy)
- `zone` (string): Filtrer par zone
- `isActive` (boolean): Filtrer par statut actif

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "machineId": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Chauffage Salon",
      "zone": "salon",
      "deviceType": {
        "id": "770e8400-e29b-41d4-a716-446655440002",
        "name": "heating_zone",
        "displayName": "Zone de Chauffage",
        "category": "climate",
        "icon": "heating"
      },
      "config": {
        "targetTemperature": 22,
        "mode": "comfort"
      },
      "isActive": true,
      "createdAt": "2024-01-15T08:00:00Z",
      "updatedAt": "2024-12-20T10:00:00Z",
      "currentState": {
        "currentTemperature": 21.5,
        "targetTemperature": 22,
        "mode": "comfort",
        "isHeating": true
      }
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 1,
    "totalPages": 1
  }
}
```

#### GET /devices/{deviceId}
Récupère les détails d'un appareil spécifique.

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "machineId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Chauffage Salon",
    "zone": "salon",
    "deviceType": {
      "id": "770e8400-e29b-41d4-a716-446655440002",
      "name": "heating_zone",
      "displayName": "Zone de Chauffage",
      "category": "climate",
      "icon": "heating",
      "configSchema": {
        "type": "object",
        "properties": {
          "targetTemperature": {"type": "number", "minimum": 5, "maximum": 30},
          "mode": {"type": "string", "enum": ["off", "eco", "comfort", "auto"]}
        }
      }
    },
    "config": {
      "targetTemperature": 22,
      "mode": "comfort"
    },
    "isActive": true,
    "createdAt": "2024-01-15T08:00:00Z",
    "updatedAt": "2024-12-20T10:00:00Z"
  }
}
```

#### POST /machines/{machineId}/devices
Crée un nouvel appareil pour une machine.

**Path Parameters:**
- `machineId` (uuid, required): ID de la machine

**Request Body:**
```json
{
  "deviceTypeId": "770e8400-e29b-41d4-a716-446655440002",
  "name": "Chauffage Chambre",
  "zone": "chambre",
  "config": {
    "targetTemperature": 20,
    "mode": "eco"
  }
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "880e8400-e29b-41d4-a716-446655440003",
    "machineId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Chauffage Chambre",
    "zone": "chambre",
    "deviceType": {
      "name": "heating_zone",
      "displayName": "Zone de Chauffage",
      "category": "climate"
    },
    "config": {
      "targetTemperature": 20,
      "mode": "eco"
    },
    "isActive": true,
    "createdAt": "2024-12-20T11:00:00Z",
    "updatedAt": "2024-12-20T11:00:00Z"
  },
  "message": "Appareil créé avec succès"
}
```

**Errors:**
- 400: Données invalides
- 403: Accès non autorisé
- 404: Machine ou type d'appareil non trouvé

#### PUT /devices/{deviceId}
Met à jour un appareil existant.

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Request Body:**
```json
{
  "name": "Chauffage Principal",
  "zone": "salon",
  "config": {
    "targetTemperature": 23,
    "mode": "comfort"
  },
  "isActive": true
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "name": "Chauffage Principal",
    "zone": "salon",
    "config": {
      "targetTemperature": 23,
      "mode": "comfort"
    },
    "isActive": true,
    "updatedAt": "2024-12-20T11:15:00Z"
  },
  "message": "Appareil mis à jour avec succès"
}
```

#### DELETE /devices/{deviceId}
Désactive un appareil (soft delete).

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Response 200:**
```json
{
  "success": true,
  "message": "Appareil désactivé avec succès"
}
```

### Actions et Contrôle

#### POST /devices/{deviceId}/actions
Crée une nouvelle action pour un appareil.

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Request Body:**
```json
{
  "type": "set_temperature",
  "payload": {
    "targetTemperature": 24
  }
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "990e8400-e29b-41d4-a716-446655440004",
    "deviceId": "660e8400-e29b-41d4-a716-446655440001",
    "machineId": "550e8400-e29b-41d4-a716-446655440000",
    "actionType": "set_temperature",
    "payload": {
      "targetTemperature": 24
    },
    "status": "pending",
    "priority": 5,
    "createdAt": "2024-12-20T11:20:00Z",
    "retryCount": 0,
    "maxRetries": 3
  },
  "message": "Action programmée avec succès"
}
```

**Errors:**
- 400: Action invalide
- 403: Accès non autorisé
- 404: Appareil non trouvé

#### GET /devices/{deviceId}/actions
Liste les actions d'un appareil.

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Query Parameters:**
- `status` (string): Filtrer par statut (pending, sent, executed, failed)
- `page` (integer, default: 1)
- `limit` (integer, default: 20)

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440004",
      "actionType": "set_temperature",
      "payload": {
        "targetTemperature": 24
      },
      "status": "executed",
      "createdAt": "2024-12-20T11:20:00Z",
      "sentAt": "2024-12-20T11:20:05Z",
      "executedAt": "2024-12-20T11:20:10Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 1,
    "totalPages": 1
  }
}
```

#### GET /devices/{deviceId}/state
Récupère l'état actuel d'un appareil.

**Path Parameters:**
- `deviceId` (uuid, required): ID de l'appareil

**Response 200:**
```json
{
  "success": true,
  "data": {
    "currentTemperature": 21.5,
    "targetTemperature": 22,
    "mode": "comfort",
    "isHeating": true,
    "lastUpdate": "2024-12-20T11:25:00Z"
  }
}
```

### APIs Boîtiers IoT

#### POST /api/machine/auth
Authentifie un boîtier IoT avec sa clé d'activation.

**Security:** Aucune (endpoint public)

**Request Body:**
```json
{
  "activationKey": "ABCD-1234-EFGH-5678-IJKL-9012-MNOP-3456"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "machineToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "machineId": "550e8400-e29b-41d4-a716-446655440000",
    "serialNumber": "ESS-2024-001",
    "config": {
      "pollInterval": 30,
      "timezone": "Europe/Paris"
    }
  }
}
```

**Errors:**
- 401: Clé d'activation invalide
- 403: Machine désactivée

#### GET /api/myactions
Récupère les actions en attente pour un boîtier.

**Security:** MachineAuth

**Headers:**
- `X-Machine-Token`: Token JWT du boîtier

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440004",
      "deviceId": "660e8400-e29b-41d4-a716-446655440001",
      "actionType": "set_temperature",
      "payload": {
        "targetTemperature": 24
      },
      "priority": 5,
      "createdAt": "2024-12-20T11:20:00Z"
    }
  ]
}
```

#### POST /api/mystatus
Envoie l'état actuel des appareils du boîtier.

**Security:** MachineAuth

**Headers:**
- `X-Machine-Token`: Token JWT du boîtier

**Request Body:**
```json
{
  "states": [
    {
      "deviceId": "660e8400-e29b-41d4-a716-446655440001",
      "stateData": {
        "currentTemperature": 21.5,
        "targetTemperature": 22,
        "mode": "comfort",
        "isHeating": true
      },
      "timestamp": "2024-12-20T11:25:00Z"
    }
  ]
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "États enregistrés avec succès"
}
```

#### POST /api/myactions/{actionId}/complete
Marque une action comme complétée.

**Security:** MachineAuth

**Path Parameters:**
- `actionId` (uuid, required): ID de l'action

**Request Body:**
```json
{
  "success": true,
  "executedAt": "2024-12-20T11:20:10Z",
  "errorMessage": null
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Action marquée comme complétée"
}
```

## Codes d'Erreur

### Codes HTTP Standards

| Code | Signification | Description |
|------|---------------|-------------|
| 200 | OK | Requête réussie |
| 201 | Created | Ressource créée avec succès |
| 204 | No Content | Requête réussie sans contenu de réponse |
| 400 | Bad Request | Données de requête invalides |
| 401 | Unauthorized | Authentification requise ou invalide |
| 403 | Forbidden | Accès interdit à la ressource |
| 404 | Not Found | Ressource non trouvée |
| 409 | Conflict | Conflit avec l'état actuel de la ressource |
| 422 | Unprocessable Entity | Validation des données échouée |
| 429 | Too Many Requests | Limite de taux dépassée |
| 500 | Internal Server Error | Erreur serveur interne |
| 503 | Service Unavailable | Service temporairement indisponible |

### Codes d'Erreur Personnalisés

```json
{
  "success": false,
  "error": "Message d'erreur principal",
  "code": "ERROR_CODE",
  "details": [
    {
      "field": "email",
      "message": "Format d'email invalide"
    }
  ]
}
```

**Codes d'erreur métier:**

| Code | Description |
|------|-------------|
| AUTH_INVALID_CREDENTIALS | Email ou mot de passe incorrect |
| AUTH_TOKEN_EXPIRED | Token JWT expiré |
| AUTH_TOKEN_INVALID | Token JWT invalide |
| AUTH_REFRESH_TOKEN_INVALID | Refresh token invalide |
| MACHINE_NOT_FOUND | Machine non trouvée |
| MACHINE_INACTIVE | Machine désactivée |
| MACHINE_UNAUTHORIZED | Accès non autorisé à cette machine |
| DEVICE_NOT_FOUND | Appareil non trouvé |
| DEVICE_INACTIVE | Appareil désactivé |
| DEVICE_UNAUTHORIZED | Accès non autorisé à cet appareil |
| ACTION_INVALID | Action invalide pour ce type d'appareil |
| ACTION_FAILED | Échec de l'exécution de l'action |
| VALIDATION_ERROR | Erreur de validation des données |
| RATE_LIMIT_EXCEEDED | Limite de taux dépassée |
| INTERNAL_ERROR | Erreur interne du serveur |

## Collection Postman

### Configuration de l'Environnement

```json
{
  "name": "Essensys Development",
  "values": [
    {
      "key": "baseUrl",
      "value": "http://localhost:3001/api",
      "enabled": true
    },
    {
      "key": "accessToken",
      "value": "",
      "enabled": true
    },
    {
      "key": "refreshToken",
      "value": "",
      "enabled": true
    },
    {
      "key": "machineToken",
      "value": "",
      "enabled": true
    },
    {
      "key": "userId",
      "value": "",
      "enabled": true
    },
    {
      "key": "machineId",
      "value": "",
      "enabled": true
    },
    {
      "key": "deviceId",
      "value": "",
      "enabled": true
    }
  ]
}
```

### Scripts de Tests Postman

**Script de pré-requête global:**
```javascript
// Ajouter automatiquement le token d'authentification
if (pm.environment.get("accessToken")) {
    pm.request.headers.add({
        key: "Authorization",
        value: "Bearer " + pm.environment.get("accessToken")
    });
}

// Ajouter le token machine si nécessaire
if (pm.request.url.path.includes("myactions") || pm.request.url.path.includes("mystatus")) {
    pm.request.headers.add({
        key: "X-Machine-Token",
        value: pm.environment.get("machineToken")
    });
}
```

**Script de test pour l'authentification:**
```javascript
// Test de connexion réussie
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has success property", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('success');
    pm.expect(jsonData.success).to.be.true;
});

pm.test("Response contains tokens", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.data).to.have.property('accessToken');
    pm.expect(jsonData.data).to.have.property('refreshToken');
    
    // Sauvegarder les tokens dans l'environnement
    pm.environment.set("accessToken", jsonData.data.accessToken);
    pm.environment.set("refreshToken", jsonData.data.refreshToken);
    pm.environment.set("userId", jsonData.data.user.id);
});
```

**Script de test pour les endpoints de ressources:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response is valid", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('success');
    pm.expect(jsonData).to.have.property('data');
});

pm.test("Pagination is present", function () {
    var jsonData = pm.response.json();
    if (Array.isArray(jsonData.data)) {
        pm.expect(jsonData).to.have.property('pagination');
        pm.expect(jsonData.pagination).to.have.property('page');
        pm.expect(jsonData.pagination).to.have.property('limit');
        pm.expect(jsonData.pagination).to.have.property('total');
        pm.expect(jsonData.pagination).to.have.property('totalPages');
    }
});

// Sauvegarder l'ID de la première ressource pour les tests suivants
pm.test("Save first resource ID", function () {
    var jsonData = pm.response.json();
    if (Array.isArray(jsonData.data) && jsonData.data.length > 0) {
        pm.environment.set("resourceId", jsonData.data[0].id);
    }
});
```

Cette documentation API complète fournit toutes les informations nécessaires pour développer et tester les intégrations avec l'application Essensys moderne.