# Essensys Backend - Node.js/Express

## Vue d'Ensemble

Backend moderne pour l'application domotique Essensys, développé en Node.js/Express avec TypeScript. Cette API remplace le système legacy ASP.NET MVC et fournit une architecture scalable et maintenable.

## 🚀 Démarrage Rapide

### Prérequis

- Node.js >= 18.0.0
- npm >= 9.0.0
- PostgreSQL >= 14
- Redis >= 6.0

### Installation

```bash
# Installer les dépendances
npm install

# Copier le fichier d'environnement
cp .env.example .env

# Configurer les variables d'environnement
nano .env

# Générer le client Prisma
npm run db:generate

# Exécuter les migrations
npm run db:migrate

# Démarrer en mode développement
npm run dev
```

### Scripts Disponibles

```bash
# Développement
npm run dev          # Démarrer avec hot-reload
npm run build        # Compiler TypeScript
npm run start        # Démarrer en production

# Tests
npm test             # Tous les tests
npm run test:pbt     # Tests Property-Based
npm run test:watch   # Tests en mode watch
npm run test:coverage # Couverture de code

# Qualité de code
npm run lint         # ESLint
npm run lint:fix     # Corriger automatiquement
npm run format       # Prettier
npm run typecheck    # Vérification TypeScript

# Base de données
npm run db:migrate   # Exécuter les migrations
npm run db:generate  # Générer le client Prisma
npm run db:studio    # Interface graphique Prisma
npm run db:seed      # Peupler la base de données
```

## 🏗️ Architecture

### Structure des Dossiers

```
src/
├── controllers/     # Contrôleurs Express
├── services/        # Logique métier
├── models/          # Modèles Prisma
├── middleware/      # Middlewares Express
├── routes/          # Définition des routes
├── config/          # Configuration (logger, DB, etc.)
├── utils/           # Utilitaires
├── types/           # Types TypeScript
└── index.ts         # Point d'entrée
```

### Technologies Utilisées

- **Runtime**: Node.js 18+
- **Framework**: Express.js
- **Langage**: TypeScript
- **ORM**: Prisma
- **Base de données**: PostgreSQL
- **Cache**: Redis
- **Authentification**: JWT
- **Validation**: Zod
- **Tests**: Jest + fast-check (PBT)
- **Logging**: Winston
- **Sécurité**: Helmet, CORS, Rate limiting

## 🔐 Authentification

### Utilisateurs Humains
- **JWT Access Token**: 15 minutes
- **JWT Refresh Token**: 30 jours
- **Hachage**: bcrypt (12 rounds)

### Boîtiers IoT
- **Machine Token**: 24 heures
- **Authentification**: Clé d'activation
- **Rate limiting**: 60 req/min par machine

## 📡 API Endpoints

### Authentification
```
POST /api/auth/login     # Connexion utilisateur
POST /api/auth/register  # Inscription utilisateur
POST /api/auth/refresh   # Rafraîchir le token
POST /api/auth/logout    # Déconnexion
```

### Utilisateurs
```
GET  /api/users/profile  # Profil utilisateur
PUT  /api/users/profile  # Modifier le profil
```

### Machines
```
GET  /api/machines       # Liste des machines
GET  /api/machines/:id   # Détails d'une machine
POST /api/machines       # Ajouter une machine
```

### Appareils
```
GET  /api/devices        # Liste des appareils
GET  /api/devices/:id    # Détails d'un appareil
PUT  /api/devices/:id    # Modifier un appareil
```

### Actions
```
POST /api/actions        # Créer une action
GET  /api/actions        # Liste des actions
GET  /api/actions/:id    # Détails d'une action
```

### Monitoring
```
GET  /api/health         # Health check simple
GET  /api/health/detailed # Health check détaillé
GET  /api/health/ready   # Readiness probe
GET  /api/health/live    # Liveness probe
```

## 🗄️ Base de Données

### Schéma Principal

- **users**: Utilisateurs de l'application
- **machines**: Boîtiers IoT
- **user_machines**: Relation utilisateurs-machines
- **device_types**: Types d'appareils
- **devices**: Appareils connectés
- **device_states**: États des appareils
- **actions**: Actions à exécuter
- **notifications**: Notifications envoyées

### Migrations

```bash
# Créer une nouvelle migration
npx prisma migrate dev --name nom_migration

# Appliquer les migrations
npm run db:migrate

# Reset de la base de données
npx prisma migrate reset
```

## 🧪 Tests

### Types de Tests

1. **Tests Unitaires**: Logique métier isolée
2. **Tests d'Intégration**: APIs et base de données
3. **Tests Property-Based**: Propriétés universelles
4. **Tests End-to-End**: Parcours utilisateur complets

### Exécution

```bash
# Tous les tests
npm test

# Tests avec couverture
npm run test:coverage

# Tests Property-Based uniquement
npm run test:pbt

# Tests en mode watch
npm run test:watch
```

## 📊 Monitoring et Logging

### Logs

- **error.log**: Erreurs uniquement
- **combined.log**: Tous les logs
- **http.log**: Requêtes HTTP
- **iot.log**: Actions IoT spécifiques

### Métriques

- Health checks pour Kubernetes
- Métriques de performance
- Monitoring des erreurs
- Suivi des connexions IoT

## 🔧 Configuration

### Variables d'Environnement

Voir `.env.example` pour la liste complète des variables.

### Sécurité

- Helmet.js pour les headers de sécurité
- CORS configuré pour le frontend
- Rate limiting par IP
- Validation stricte des entrées
- Hachage sécurisé des mots de passe

## 🚀 Déploiement

### Docker

```bash
# Build de l'image
docker build -t essensys-backend .

# Démarrer avec docker-compose
docker-compose up -d
```

### Production

```bash
# Build de production
npm run build

# Démarrer en production
NODE_ENV=production npm start
```

## 🤝 Contribution

### Standards de Code

- **ESLint**: Configuration stricte
- **Prettier**: Formatage automatique
- **TypeScript**: Types stricts
- **Commits**: Convention Conventional Commits

### Workflow

1. Créer une branche feature
2. Développer avec tests
3. Vérifier la qualité (`npm run lint`)
4. Créer une Pull Request
5. Review et merge

## 📚 Documentation

- **API**: Documentation OpenAPI générée
- **Code**: JSDoc pour les fonctions complexes
- **Architecture**: Diagrammes dans `/docs`

## 🐛 Dépannage

### Problèmes Courants

1. **Port déjà utilisé**: Changer `PORT` dans `.env`
2. **Base de données**: Vérifier `DATABASE_URL`
3. **Redis**: Vérifier `REDIS_URL`
4. **Permissions**: Vérifier les droits de fichiers

### Logs de Debug

```bash
# Activer les logs détaillés
LOG_LEVEL=debug npm run dev

# Voir les logs en temps réel
tail -f logs/combined.log
```

---

*Backend développé selon les spécifications du plan de migration Essensys*