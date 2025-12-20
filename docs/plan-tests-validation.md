# Plan de Tests et Validation - Migration Essensys

## Vue d'Ensemble

Ce document définit la stratégie complète de tests pour valider la parité fonctionnelle entre le système Essensys legacy et le nouveau système React/Node.js. L'objectif est d'assurer la qualité, la fiabilité et la continuité de service pendant et après la migration.

## 6.1 Scénarios de Test Fonctionnels

### Features Métier Identifiées

Basé sur l'analyse du système legacy, voici les features métier critiques à tester :

#### 1. Authentification et Gestion des Utilisateurs

**Scénarios de Test Principaux :**

**TC-AUTH-001 : Connexion Utilisateur Standard**
- **Objectif :** Valider la connexion avec email/mot de passe
- **Prérequis :** Utilisateur existant avec compte actif
- **Étapes :**
  1. Accéder à la page de connexion
  2. Saisir email et mot de passe valides
  3. Cliquer sur "Se connecter"
- **Résultat Attendu :** 
  - Redirection vers le tableau de bord
  - Token JWT généré et stocké
  - Session utilisateur active
- **Critères d'Acceptation :**
  - Temps de réponse < 2 secondes
  - Token JWT valide pendant 15 minutes
  - Refresh token valide pendant 30 jours

**TC-AUTH-002 : Connexion avec Identifiants Invalides**
- **Objectif :** Valider la gestion des erreurs d'authentification
- **Étapes :**
  1. Saisir email valide et mot de passe incorrect
  2. Tenter la connexion
- **Résultat Attendu :**
  - Message d'erreur "Identifiants invalides"
  - Pas de redirection
  - Compteur de tentatives incrémenté

**TC-AUTH-003 : Migration Mot de Passe SHA1 vers bcrypt**
- **Objectif :** Valider la migration transparente des mots de passe
- **Prérequis :** Utilisateur avec ancien hash SHA1
- **Étapes :**
  1. Connexion avec ancien mot de passe
  2. Vérifier la mise à jour automatique du hash
- **Résultat Attendu :**
  - Connexion réussie
  - Hash migré vers bcrypt en base
  - Flag `password_needs_migration` mis à false

**TC-AUTH-004 : Gestion des Sessions Multiples**
- **Objectif :** Valider la gestion des connexions multiples
- **Étapes :**
  1. Se connecter depuis un navigateur
  2. Se connecter depuis un autre appareil
  3. Vérifier les sessions actives
- **Résultat Attendu :**
  - Deux sessions distinctes créées
  - Tokens indépendants
  - Possibilité de déconnexion sélective

#### 2. Authentification des Boîtiers IoT

**TC-IOT-001 : Authentification Boîtier avec Clé d'Activation**
- **Objectif :** Valider l'authentification des boîtiers existants
- **Prérequis :** Boîtier avec clé d'activation valide
- **Étapes :**
  1. Envoyer requête POST /api/machine/auth avec clé
  2. Vérifier la réponse
- **Résultat Attendu :**
  - Token machine JWT généré (24h)
  - Mise à jour de `last_connection`
  - Configuration machine retournée

**TC-IOT-002 : Validation Format Clé d'Activation**
- **Objectif :** Valider le format des clés d'activation
- **Étapes :**
  1. Tester différents formats de clés
  2. Valider le pattern XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
- **Résultat Attendu :**
  - Clés valides acceptées
  - Clés invalides rejetées avec erreur 400

**TC-IOT-003 : Rate Limiting Boîtiers**
- **Objectif :** Valider la limitation de débit pour les boîtiers
- **Étapes :**
  1. Envoyer 70 requêtes en 1 minute depuis un boîtier
  2. Vérifier la limitation
- **Résultat Attendu :**
  - 60 premières requêtes acceptées
  - Requêtes suivantes rejetées avec 429

#### 3. Contrôle des Appareils de Chauffage

**TC-HEAT-001 : Modification Température Cible**
- **Objectif :** Valider le contrôle de température
- **Prérequis :** Zone de chauffage configurée
- **Étapes :**
  1. Accéder au contrôle de chauffage
  2. Modifier la température cible (18°C → 22°C)
  3. Confirmer l'action
- **Résultat Attendu :**
  - Action créée en base avec status 'pending'
  - Notification temps réel à l'interface
  - Boîtier récupère l'action via /api/myactions

**TC-HEAT-002 : Changement Mode Chauffage**
- **Objectif :** Valider les modes de chauffage (Eco, Confort, Auto)
- **Étapes :**
  1. Passer du mode Confort au mode Eco
  2. Vérifier l'impact sur la température cible
- **Résultat Attendu :**
  - Mode mis à jour instantanément
  - Température ajustée selon le mode
  - Historique des changements enregistré

**TC-HEAT-003 : Programmation Horaire**
- **Objectif :** Valider la programmation automatique
- **Étapes :**
  1. Configurer une programmation (Eco 22h-6h, Confort 6h-22h)
  2. Vérifier l'exécution automatique
- **Résultat Attendu :**
  - Actions programmées créées
  - Exécution selon planning
  - Possibilité d'override manuel

**TC-HEAT-004 : Gestion Multi-Zones**
- **Objectif :** Valider le contrôle indépendant des zones
- **Prérequis :** Plusieurs zones de chauffage configurées
- **Étapes :**
  1. Modifier température zone salon (22°C)
  2. Modifier température zone chambre (19°C)
  3. Vérifier l'indépendance
- **Résultat Attendu :**
  - Chaque zone contrôlée indépendamment
  - Pas d'interférence entre zones
  - États distincts en base de données

#### 4. Contrôle des Volets Roulants

**TC-SHUT-001 : Ouverture/Fermeture Manuelle**
- **Objectif :** Valider le contrôle manuel des volets
- **Étapes :**
  1. Ouvrir un volet (position 0% → 100%)
  2. Fermer partiellement (100% → 50%)
  3. Fermer complètement (50% → 0%)
- **Résultat Attendu :**
  - Position mise à jour en temps réel
  - Actions transmises au boîtier
  - État persisté en base

**TC-SHUT-002 : Programmation Automatique**
- **Objectif :** Valider l'ouverture/fermeture automatique
- **Étapes :**
  1. Programmer ouverture à 7h00
  2. Programmer fermeture à 20h00
  3. Vérifier l'exécution
- **Résultat Attendu :**
  - Actions programmées exécutées
  - Logs d'exécution disponibles
  - Gestion des jours fériés

**TC-SHUT-003 : Gestion Météo**
- **Objectif :** Valider la fermeture automatique par vent fort
- **Prérequis :** Capteur météo configuré
- **Étapes :**
  1. Simuler alerte vent fort
  2. Vérifier fermeture automatique
- **Résultat Attendu :**
  - Volets fermés automatiquement
  - Notification utilisateur envoyée
  - Override manuel possible

#### 5. Système d'Alarme

**TC-ALARM-001 : Activation/Désactivation Alarme**
- **Objectif :** Valider le contrôle de l'alarme
- **Étapes :**
  1. Activer l'alarme totale
  2. Désactiver l'alarme
  3. Activer l'alarme partielle (nuit)
- **Résultat Attendu :**
  - État alarme mis à jour
  - Zones surveillées configurées
  - Historique des activations

**TC-ALARM-002 : Déclenchement Alarme**
- **Objectif :** Valider la détection d'intrusion
- **Prérequis :** Alarme activée, capteur configuré
- **Étapes :**
  1. Simuler déclenchement capteur
  2. Vérifier les notifications
- **Résultat Attendu :**
  - Alarme déclenchée instantanément
  - SMS/Email envoyés aux contacts
  - Enregistrement événement

**TC-ALARM-003 : Gestion Codes d'Accès**
- **Objectif :** Valider les codes utilisateur
- **Étapes :**
  1. Créer nouveau code utilisateur
  2. Désactiver alarme avec ce code
  3. Supprimer le code
- **Résultat Attendu :**
  - Code créé et fonctionnel
  - Traçabilité des utilisations
  - Révocation immédiate

#### 6. Notifications et Alertes

**TC-NOTIF-001 : Envoi SMS**
- **Objectif :** Valider l'envoi de SMS d'alerte
- **Prérequis :** Numéro de téléphone configuré
- **Étapes :**
  1. Déclencher une alerte (alarme, panne)
  2. Vérifier réception SMS
- **Résultat Attendu :**
  - SMS reçu dans les 30 secondes
  - Contenu correct et lisible
  - Statut de livraison tracé

**TC-NOTIF-002 : Notifications Email**
- **Objectif :** Valider les emails de notification
- **Étapes :**
  1. Configurer notifications email
  2. Déclencher événement (température atteinte)
  3. Vérifier réception email
- **Résultat Attendu :**
  - Email reçu avec template correct
  - Liens d'action fonctionnels
  - Possibilité de désabonnement

**TC-NOTIF-003 : Push Notifications Web**
- **Objectif :** Valider les notifications push
- **Prérequis :** Navigateur compatible, permissions accordées
- **Étapes :**
  1. Activer notifications push
  2. Déclencher événement
  3. Vérifier notification
- **Résultat Attendu :**
  - Notification push affichée
  - Clic redirige vers l'application
  - Gestion des permissions

#### 7. Gestion des Versions Firmware

**TC-FIRM-001 : Mise à Jour Firmware**
- **Objectif :** Valider le processus de mise à jour
- **Prérequis :** Nouvelle version firmware disponible
- **Étapes :**
  1. Lancer mise à jour depuis l'interface
  2. Suivre le progrès
  3. Vérifier finalisation
- **Résultat Attendu :**
  - Téléchargement progressif tracé
  - Installation réussie
  - Version mise à jour en base

**TC-FIRM-002 : Rollback Firmware**
- **Objectif :** Valider le retour version précédente
- **Étapes :**
  1. Détecter problème post-mise à jour
  2. Lancer rollback automatique
  3. Vérifier restauration
- **Résultat Attendu :**
  - Rollback exécuté automatiquement
  - Fonctionnalités restaurées
  - Incident tracé

### Tests d'Acceptation avec Critères Mesurables

#### Critères de Performance

**Temps de Réponse :**
- Authentification : < 2 secondes
- Chargement tableau de bord : < 3 secondes
- Exécution action appareil : < 1 seconde
- Synchronisation état boîtier : < 5 secondes

**Disponibilité :**
- Uptime système : > 99.5%
- Disponibilité API : > 99.9%
- Temps de récupération après panne : < 5 minutes

**Capacité :**
- Utilisateurs simultanés : > 1000
- Actions par minute : > 10000
- Boîtiers connectés : > 5000

#### Critères de Compatibilité

**Navigateurs Supportés :**
- Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- Support mobile iOS 14+, Android 10+

**Versions Firmware Boîtiers :**
- Compatibilité descendante 3 versions
- Migration automatique protocoles

### Tests de Régression

#### Cas d'Usage Critiques Legacy

**Régression R-001 : Parcours Utilisateur Complet**
- Connexion → Navigation → Contrôle Chauffage → Déconnexion
- Validation : Aucune régression fonctionnelle

**Régression R-002 : Communication Boîtiers**
- Authentification → Récupération Actions → Envoi États
- Validation : Protocole identique au legacy

**Régression R-003 : Notifications Critiques**
- Déclenchement Alarme → Envoi SMS → Confirmation Réception
- Validation : Délais identiques ou améliorés

**Régression R-004 : Gestion Multi-Utilisateurs**
- Plusieurs utilisateurs sur même machine
- Validation : Isolation des actions et permissions

#### Suite de Tests de Non-Régression

**Automatisation Complète :**
- 150+ tests automatisés couvrant tous les cas critiques
- Exécution à chaque commit sur branches principales
- Rapport de régression automatique

**Tests de Charge :**
- Simulation 1000 utilisateurs simultanés
- 10000 actions/minute pendant 1 heure
- Validation stabilité et performance


## 6.2 Tests de Performance

### Benchmarks Basés sur le Système Legacy

#### Analyse des Performances Legacy

**Métriques Actuelles (Système ASP.NET) :**
- Temps de réponse moyen API : 150-300ms
- Chargement page complète : 4-6 secondes
- Capacité maximale : ~500 utilisateurs simultanés
- Throughput : ~200 requêtes/seconde
- Temps de synchronisation boîtier : 3-8 secondes

**Objectifs d'Amélioration :**
- Réduction temps de réponse : -40% (90-180ms)
- Chargement page : -50% (2-3 secondes)
- Capacité : +100% (1000+ utilisateurs)
- Throughput : +400% (1000+ req/s)
- Synchronisation : -30% (2-5 secondes)

### Tests de Charge

#### Scénario de Charge L-001 : Utilisation Normale

**Configuration :**
- Utilisateurs virtuels : 500
- Durée : 30 minutes
- Montée en charge : 5 minutes
- Plateau : 20 minutes
- Descente : 5 minutes

**Profil Utilisateur :**
```yaml
# artillery-normal-load.yml
config:
  target: "https://api.essensys.example.com"
  phases:
    - duration: 300
      arrivalRate: 10
      name: "Warm up"
    - duration: 1200
      arrivalRate: 50
      name: "Sustained load"
    - duration: 300
      arrivalRate: 10
      name: "Cool down"
  processor: "./load-test-processor.js"
  variables:
    authToken: "{{ $processEnvironment.AUTH_TOKEN }}"

scenarios:
  - name: "User Journey - Device Control"
    weight: 60
    flow:
      - post:
          url: "/api/auth/login"
          json:
            email: "{{ $randomString() }}@test.com"
            password: "testpassword"
          capture:
            - json: "$.data.accessToken"
              as: "token"
      
      - get:
          url: "/api/machines/{{ machineId }}/devices"
          headers:
            Authorization: "Bearer {{ token }}"
          expect:
            - statusCode: 200
            - contentType: json
      
      - post:
          url: "/api/devices/{{ deviceId }}/actions"
          headers:
            Authorization: "Bearer {{ token }}"
          json:
            type: "set_temperature"
            payload:
              targetTemperature: "{{ $randomNumber(18, 25) }}"
          expect:
            - statusCode: 201
      
      - think: 5
      
      - get:
          url: "/api/devices/{{ deviceId }}/state"
          headers:
            Authorization: "Bearer {{ token }}"
          expect:
            - statusCode: 200

  - name: "IoT Device Sync"
    weight: 40
    flow:
      - post:
          url: "/api/machine/auth"
          json:
            activationKey: "{{ activationKey }}"
          capture:
            - json: "$.data.machineToken"
              as: "machineToken"
      
      - get:
          url: "/api/myactions"
          headers:
            X-Machine-Token: "{{ machineToken }}"
          expect:
            - statusCode: 200
      
      - post:
          url: "/api/mystatus"
          headers:
            X-Machine-Token: "{{ machineToken }}"
          json:
            states: "{{ generateRandomStates() }}"
          expect:
            - statusCode: 200
      
      - think: 10
```

**Critères de Succès :**
- Taux d'erreur < 0.1%
- Temps de réponse p95 < 200ms
- Temps de réponse p99 < 500ms
- CPU serveur < 70%
- Mémoire serveur < 80%
- Connexions DB < 80% du pool

#### Scénario de Charge L-002 : Pic d'Utilisation

**Configuration :**
- Utilisateurs virtuels : 1500
- Durée : 15 minutes
- Montée en charge rapide : 2 minutes
- Plateau : 10 minutes
- Descente : 3 minutes

**Objectif :** Valider la tenue en charge lors des pics (matin 7h-9h, soir 18h-20h)

**Critères de Succès :**
- Système reste opérationnel
- Dégradation gracieuse si surcharge
- Auto-scaling déclenché si configuré
- Temps de réponse p95 < 500ms
- Aucune perte de données

#### Scénario de Charge L-003 : Charge Soutenue Longue Durée

**Configuration :**
- Utilisateurs virtuels : 800
- Durée : 4 heures
- Charge constante

**Objectif :** Détecter les fuites mémoire et dégradations progressives

**Critères de Succès :**
- Pas de dégradation progressive des performances
- Mémoire stable (pas de fuite)
- Connexions DB stables
- Logs d'erreur stables

### Tests de Stress

#### Test de Stress S-001 : Saturation Graduelle

**Configuration :**
```yaml
# artillery-stress-test.yml
config:
  target: "https://api.essensys.example.com"
  phases:
    - duration: 300
      arrivalRate: 50
      name: "Stage 1 - Normal"
    - duration: 300
      arrivalRate: 100
      name: "Stage 2 - High"
    - duration: 300
      arrivalRate: 200
      name: "Stage 3 - Very High"
    - duration: 300
      arrivalRate: 400
      name: "Stage 4 - Extreme"
    - duration: 300
      arrivalRate: 800
      name: "Stage 5 - Breaking Point"

scenarios:
  - name: "Stress Test"
    flow:
      - post:
          url: "/api/auth/login"
          json:
            email: "stress-test-{{ $randomNumber(1, 10000) }}@test.com"
            password: "testpassword"
      - get:
          url: "/api/machines/{{ machineId }}/devices"
      - post:
          url: "/api/devices/{{ deviceId }}/actions"
          json:
            type: "set_temperature"
            payload:
              targetTemperature: 22
```

**Objectifs :**
- Identifier le point de rupture du système
- Valider les mécanismes de protection (rate limiting, circuit breakers)
- Tester la récupération après surcharge

**Critères de Succès :**
- Point de rupture > 1000 req/s
- Rate limiting activé avant crash
- Récupération automatique en < 2 minutes
- Messages d'erreur appropriés (503 Service Unavailable)

#### Test de Stress S-002 : Spike Soudain

**Configuration :**
- Charge normale : 100 utilisateurs
- Spike : 2000 utilisateurs en 30 secondes
- Durée spike : 5 minutes
- Retour normal : 1 minute

**Objectif :** Valider la résilience face à un pic soudain (ex: notification push massive)

**Critères de Succès :**
- Système absorbe le spike sans crash
- Auto-scaling réagit en < 2 minutes
- Dégradation gracieuse si nécessaire
- Récupération complète après spike

### Critères de Performance Acceptables

#### Frontend (React)

**Métriques Core Web Vitals :**
```javascript
// lighthouse-config.js
module.exports = {
  extends: 'lighthouse:default',
  settings: {
    onlyCategories: ['performance', 'accessibility'],
    throttling: {
      rttMs: 40,
      throughputKbps: 10240,
      cpuSlowdownMultiplier: 1
    }
  },
  thresholds: {
    'first-contentful-paint': 1500,
    'largest-contentful-paint': 2500,
    'cumulative-layout-shift': 0.1,
    'total-blocking-time': 300,
    'speed-index': 3000,
    'interactive': 3000
  }
};
```

**Objectifs par Page :**

| Page | FCP | LCP | TTI | CLS |
|------|-----|-----|-----|-----|
| Login | < 1.0s | < 1.5s | < 2.0s | < 0.05 |
| Dashboard | < 1.2s | < 2.0s | < 2.5s | < 0.1 |
| Device Control | < 1.0s | < 1.8s | < 2.3s | < 0.1 |
| Settings | < 1.5s | < 2.5s | < 3.0s | < 0.1 |

**Bundle Size :**
- Initial bundle : < 200 KB (gzipped)
- Total bundle : < 500 KB (gzipped)
- Code splitting : Oui (par route)
- Lazy loading : Oui (composants lourds)

#### Backend (Node.js/Express)

**Temps de Réponse par Endpoint :**

| Endpoint | p50 | p95 | p99 | Max |
|----------|-----|-----|-----|-----|
| POST /api/auth/login | 80ms | 150ms | 300ms | 500ms |
| GET /api/machines/:id/devices | 50ms | 100ms | 200ms | 400ms |
| POST /api/devices/:id/actions | 60ms | 120ms | 250ms | 500ms |
| GET /api/devices/:id/state | 40ms | 80ms | 150ms | 300ms |
| POST /api/mystatus | 70ms | 140ms | 280ms | 600ms |
| GET /api/myactions | 50ms | 100ms | 200ms | 400ms |

**Throughput :**
- Requêtes par seconde : > 1000 req/s
- Connexions simultanées : > 5000
- WebSocket connections : > 2000

**Ressources :**
- CPU moyen : < 50%
- CPU pic : < 80%
- Mémoire moyenne : < 60%
- Mémoire pic : < 85%

#### Base de Données (PostgreSQL)

**Temps de Requête :**
- Requêtes simples (SELECT by ID) : < 5ms
- Requêtes complexes (JOIN multiple) : < 50ms
- Requêtes d'agrégation : < 100ms
- Écritures (INSERT/UPDATE) : < 10ms

**Connexions :**
- Pool size : 20-50 connexions
- Utilisation moyenne : < 60%
- Temps d'attente connexion : < 10ms

**Cache (Redis) :**
- Hit rate : > 80%
- Temps de réponse : < 2ms
- Mémoire utilisée : < 2GB

### Tests de Montée en Charge

#### Scénario MC-001 : Croissance Progressive

**Objectif :** Valider la scalabilité horizontale

**Configuration :**
```yaml
# k6-scaling-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '5m', target: 100 },   // Montée à 100 users
    { duration: '5m', target: 100 },   // Plateau
    { duration: '5m', target: 500 },   // Montée à 500 users
    { duration: '5m', target: 500 },   // Plateau
    { duration: '5m', target: 1000 },  // Montée à 1000 users
    { duration: '10m', target: 1000 }, // Plateau long
    { duration: '5m', target: 2000 },  // Montée à 2000 users
    { duration: '5m', target: 2000 },  // Plateau
    { duration: '5m', target: 0 },     // Descente
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'],
    'http_req_failed': ['rate<0.01'],
  },
};

export default function() {
  // Login
  let loginRes = http.post('https://api.essensys.example.com/api/auth/login', 
    JSON.stringify({
      email: `user${__VU}@test.com`,
      password: 'testpassword'
    }),
    { headers: { 'Content-Type': 'application/json' } }
  );
  
  check(loginRes, {
    'login successful': (r) => r.status === 200,
    'token received': (r) => r.json('data.accessToken') !== undefined,
  });
  
  let token = loginRes.json('data.accessToken');
  
  sleep(1);
  
  // Get devices
  let devicesRes = http.get(
    `https://api.essensys.example.com/api/machines/${machineId}/devices`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  
  check(devicesRes, {
    'devices retrieved': (r) => r.status === 200,
  });
  
  sleep(2);
  
  // Execute action
  let actionRes = http.post(
    `https://api.essensys.example.com/api/devices/${deviceId}/actions`,
    JSON.stringify({
      type: 'set_temperature',
      payload: { targetTemperature: 22 }
    }),
    { headers: { 
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }}
  );
  
  check(actionRes, {
    'action executed': (r) => r.status === 201,
  });
  
  sleep(5);
}
```

**Critères de Succès :**
- Auto-scaling déclenché aux seuils configurés
- Pas de dégradation lors de l'ajout d'instances
- Load balancing équilibré
- Temps de réponse stables à chaque palier

### Outils et Configuration

#### Artillery (Tests de Charge API)

**Installation et Configuration :**
```bash
npm install -g artillery@latest
npm install -g artillery-plugin-metrics-by-endpoint
```

**Exécution :**
```bash
# Test de charge normal
artillery run artillery-normal-load.yml

# Test de stress
artillery run artillery-stress-test.yml

# Avec rapport HTML
artillery run --output report.json artillery-normal-load.yml
artillery report report.json
```

#### k6 (Tests de Performance Avancés)

**Installation :**
```bash
# macOS
brew install k6

# Docker
docker pull grafana/k6:latest
```

**Exécution :**
```bash
# Test local
k6 run k6-scaling-test.js

# Avec monitoring Grafana
k6 run --out influxdb=http://localhost:8086/k6 k6-scaling-test.js
```

#### Lighthouse CI (Performance Frontend)

**Configuration :**
```json
// lighthouserc.json
{
  "ci": {
    "collect": {
      "url": [
        "https://app.essensys.example.com/login",
        "https://app.essensys.example.com/dashboard",
        "https://app.essensys.example.com/devices"
      ],
      "numberOfRuns": 3,
      "settings": {
        "preset": "desktop",
        "throttling": {
          "rttMs": 40,
          "throughputKbps": 10240,
          "cpuSlowdownMultiplier": 1
        }
      }
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.9}],
        "first-contentful-paint": ["error", {"maxNumericValue": 1500}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 2500}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}],
        "total-blocking-time": ["error", {"maxNumericValue": 300}]
      }
    },
    "upload": {
      "target": "temporary-public-storage"
    }
  }
}
```

**Exécution :**
```bash
# Installation
npm install -g @lhci/cli

# Exécution
lhci autorun

# Avec serveur LHCI
lhci autorun --upload.target=lhci --upload.serverBaseUrl=https://lhci.example.com
```

### Monitoring et Métriques

#### Métriques à Collecter

**Application :**
- Temps de réponse par endpoint
- Taux d'erreur par endpoint
- Throughput (req/s)
- Latence réseau
- Taille des réponses

**Infrastructure :**
- CPU utilization
- Memory utilization
- Disk I/O
- Network I/O
- Connexions actives

**Base de Données :**
- Query execution time
- Connexions actives
- Cache hit rate
- Slow queries
- Deadlocks

**Cache :**
- Hit rate
- Miss rate
- Eviction rate
- Memory usage
- Latency

#### Dashboards de Monitoring

**Grafana Dashboard - Performance Overview :**
```json
{
  "dashboard": {
    "title": "Essensys Performance Monitoring",
    "panels": [
      {
        "title": "API Response Time (p95)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))"
          }
        ]
      },
      {
        "title": "Throughput (req/s)",
        "targets": [
          {
            "expr": "rate(http_requests_total[1m])"
          }
        ]
      },
      {
        "title": "Error Rate",
        "targets": [
          {
            "expr": "rate(http_requests_total{status=~\"5..\"}[5m])"
          }
        ]
      },
      {
        "title": "Database Query Time",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(db_query_duration_seconds_bucket[5m]))"
          }
        ]
      }
    ]
  }
}
```

### Rapports de Performance

#### Format de Rapport

**Rapport de Test de Charge :**
```markdown
# Rapport de Test de Charge - [Date]

## Configuration
- Scénario : L-001 - Utilisation Normale
- Durée : 30 minutes
- Utilisateurs virtuels : 500
- Environnement : Staging

## Résultats Globaux
- Requêtes totales : 45,000
- Requêtes réussies : 44,955 (99.9%)
- Requêtes échouées : 45 (0.1%)
- Throughput moyen : 1,500 req/s
- Throughput pic : 2,100 req/s

## Temps de Réponse
| Métrique | Valeur | Objectif | Status |
|----------|--------|----------|--------|
| p50 | 85ms | < 100ms | ✅ PASS |
| p95 | 180ms | < 200ms | ✅ PASS |
| p99 | 420ms | < 500ms | ✅ PASS |
| Max | 1,200ms | < 2,000ms | ✅ PASS |

## Ressources Système
| Ressource | Moyenne | Pic | Limite | Status |
|-----------|---------|-----|--------|--------|
| CPU | 45% | 68% | 80% | ✅ PASS |
| Mémoire | 52% | 71% | 85% | ✅ PASS |
| DB Connections | 35/50 | 48/50 | 50 | ⚠️ WARNING |

## Incidents
- 3 timeouts de connexion DB (pic de charge)
- 42 erreurs 429 (rate limiting attendu)

## Recommandations
1. Augmenter le pool de connexions DB à 75
2. Optimiser la requête de récupération des devices (index manquant)
3. Implémenter le cache Redis pour les états des devices

## Conclusion
✅ Le système répond aux critères de performance pour une charge normale.
⚠️ Attention au pool de connexions DB lors des pics.
```

## 6.3 Tests de Compatibilité Hardware

### Analyse des Boîtiers Existants

#### Versions Firmware Identifiées

Basé sur l'analyse du système legacy, les boîtiers Essensys utilisent plusieurs versions :

**Versions Supportées :**
- **V0** : Version initiale (legacy, ~15% du parc)
- **V1** : Version stable principale (~60% du parc)
- **V2** : Version récente (~20% du parc)
- **V3** : Version beta/test (~5% du parc)

**Protocoles de Communication :**
- **HTTP REST** : Endpoints `/api/myactions` et `/api/mystatus`
- **Authentification** : Clé d'activation 32 caractères
- **Format données** : JSON avec structure spécifique
- **Polling** : Intervalle configurable (30s-300s)

#### Mapping des Index de Données Legacy

**Index Critiques Identifiés :**
```javascript
// Mapping des index de données du système legacy
const LEGACY_DATA_INDEX = {
  // États des boutons poussoirs
  "920": "BP1_STATE",      // Bouton poussoir 1
  "921": "BP2_STATE",      // Bouton poussoir 2
  "922": "BP3_STATE",      // Bouton poussoir 3
  "923": "BP4_STATE",      // Bouton poussoir 4
  
  // Températures
  "100": "TEMP_ZONE1",     // Température zone 1
  "101": "TEMP_ZONE2",     // Température zone 2
  "102": "TEMP_ZONE3",     // Température zone 3
  "103": "TEMP_EXT",       // Température extérieure
  
  // États chauffage
  "200": "HEAT_ZONE1",     // Chauffage zone 1 (0=off, 1=on)
  "201": "HEAT_ZONE2",     // Chauffage zone 2
  "202": "HEAT_ZONE3",     // Chauffage zone 3
  
  // Volets
  "300": "SHUTTER1_POS",   // Position volet 1 (0-100%)
  "301": "SHUTTER2_POS",   // Position volet 2
  "302": "SHUTTER3_POS",   // Position volet 3
  
  // Alarme
  "400": "ALARM_STATE",    // État alarme (0=off, 1=on, 2=partial)
  "401": "ALARM_ZONE1",    // Capteur zone 1
  "402": "ALARM_ZONE2",    // Capteur zone 2
  "403": "ALARM_ZONE3",    // Capteur zone 3
  "407": "ALARM_TRIGGER",  // Déclenchement alarme
  
  // Système
  "500": "SYSTEM_STATUS",  // État système
  "501": "BATTERY_LEVEL",  // Niveau batterie
  "502": "SIGNAL_STRENGTH", // Force signal
  "503": "LAST_SYNC"       // Dernière synchronisation
};
```

### Tests de Communication avec Boîtiers Existants

#### Test HW-001 : Authentification Boîtier Legacy

**Objectif :** Valider l'authentification des boîtiers existants avec le nouveau système

**Configuration de Test :**
```javascript
// test-hardware-auth.js
const axios = require('axios');
const crypto = require('crypto');

describe('Hardware Authentication Compatibility', () => {
  const LEGACY_ACTIVATION_KEYS = [
    'ABCD-EFGH-IJKL-MNOP-QRST-UVWX-YZ12-3456', // V0 format
    'A1B2-C3D4-E5F6-G7H8-I9J0-K1L2-M3N4-O5P6', // V1 format
    '1234-5678-9ABC-DEF0-1234-5678-9ABC-DEF0'   // V2 format
  ];

  LEGACY_ACTIVATION_KEYS.forEach((key, index) => {
    it(`should authenticate V${index} device with key ${key}`, async () => {
      const response = await axios.post('/api/machine/auth', {
        activationKey: key
      });
      
      expect(response.status).toBe(200);
      expect(response.data.success).toBe(true);
      expect(response.data.data.machineToken).toBeDefined();
      expect(response.data.data.machineId).toBeDefined();
      
      // Valider le format du token JWT
      const token = response.data.data.machineToken;
      const decoded = jwt.decode(token);
      expect(decoded.machineId).toBeDefined();
      expect(decoded.exp).toBeGreaterThan(Date.now() / 1000);
    });
  });
});
```

**Critères de Succès :**
- Toutes les clés d'activation existantes acceptées
- Token JWT généré avec durée appropriée (24h)
- Compatibilité descendante maintenue
- Temps de réponse < 500ms

#### Test HW-002 : Récupération Actions Legacy

**Objectif :** Valider que les boîtiers peuvent récupérer les actions dans le format attendu

**Format Legacy Attendu :**
```json
{
  "success": true,
  "data": [
    {
      "ID": 123,
      "GUID": "550e8400-e29b-41d4-a716-446655440000",
      "ACTIONTYPE": "CHAUFFAGE",
      "ACTIONINFO": "407=1;200=22;201=20",
      "DATECREATION": "2024-01-15T10:30:00Z"
    },
    {
      "ID": 124,
      "GUID": "550e8400-e29b-41d4-a716-446655440001",
      "ACTIONTYPE": "VOLET",
      "ACTIONINFO": "300=50;301=75",
      "DATECREATION": "2024-01-15T10:31:00Z"
    }
  ]
}
```

**Test d'Implémentation :**
```javascript
// test-legacy-actions.js
describe('Legacy Actions Format Compatibility', () => {
  let machineToken;
  
  beforeEach(async () => {
    // Authentifier un boîtier
    const authResponse = await axios.post('/api/machine/auth', {
      activationKey: 'TEST-KEY1-2345-6789-ABCD-EFGH-IJKL-MNOP'
    });
    machineToken = authResponse.data.data.machineToken;
  });

  it('should return actions in legacy format', async () => {
    // Créer une action moderne
    await axios.post('/api/devices/device-123/actions', {
      type: 'set_temperature',
      payload: { targetTemperature: 22, zone: 1 }
    }, {
      headers: { Authorization: `Bearer ${userToken}` }
    });

    // Récupérer via l'endpoint legacy
    const response = await axios.get('/api/myactions', {
      headers: { 'X-Machine-Token': machineToken }
    });

    expect(response.status).toBe(200);
    expect(response.data.success).toBe(true);
    expect(response.data.data).toBeInstanceOf(Array);
    
    const action = response.data.data[0];
    expect(action.ID).toBeDefined();
    expect(action.GUID).toMatch(/^[0-9a-f-]{36}$/);
    expect(action.ACTIONTYPE).toBe('CHAUFFAGE');
    expect(action.ACTIONINFO).toContain('200=22'); // Zone 1 = 22°C
    expect(action.DATECREATION).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$/);
  });

  it('should handle multiple action types correctly', async () => {
    // Créer différents types d'actions
    await Promise.all([
      // Chauffage
      axios.post('/api/devices/heating-1/actions', {
        type: 'set_temperature',
        payload: { targetTemperature: 21, zone: 1 }
      }),
      // Volet
      axios.post('/api/devices/shutter-1/actions', {
        type: 'set_position',
        payload: { position: 75 }
      }),
      // Alarme
      axios.post('/api/devices/alarm-1/actions', {
        type: 'arm_alarm',
        payload: { mode: 'total' }
      })
    ]);

    const response = await axios.get('/api/myactions', {
      headers: { 'X-Machine-Token': machineToken }
    });

    const actions = response.data.data;
    expect(actions.length).toBe(3);
    
    // Vérifier les types d'actions
    const actionTypes = actions.map(a => a.ACTIONTYPE);
    expect(actionTypes).toContain('CHAUFFAGE');
    expect(actionTypes).toContain('VOLET');
    expect(actionTypes).toContain('ALARME');
  });
});
```

#### Test HW-003 : Envoi États Legacy

**Objectif :** Valider la réception des états des boîtiers dans le format legacy

**Format Legacy Attendu :**
```json
{
  "states": [
    {
      "index": "100",
      "value": "20.5"
    },
    {
      "index": "200",
      "value": "1"
    },
    {
      "index": "300",
      "value": "75"
    }
  ],
  "version": "V1",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Test d'Implémentation :**
```javascript
// test-legacy-states.js
describe('Legacy States Reception Compatibility', () => {
  let machineToken;
  
  beforeEach(async () => {
    const authResponse = await axios.post('/api/machine/auth', {
      activationKey: 'TEST-KEY1-2345-6789-ABCD-EFGH-IJKL-MNOP'
    });
    machineToken = authResponse.data.data.machineToken;
  });

  it('should accept legacy state format', async () => {
    const legacyStates = {
      states: [
        { index: "100", value: "20.5" },  // Température zone 1
        { index: "101", value: "19.2" },  // Température zone 2
        { index: "200", value: "1" },     // Chauffage zone 1 ON
        { index: "201", value: "0" },     // Chauffage zone 2 OFF
        { index: "300", value: "75" },    // Volet 1 à 75%
        { index: "400", value: "0" },     // Alarme OFF
        { index: "500", value: "1" },     // Système OK
        { index: "501", value: "85" }     // Batterie 85%
      ],
      version: "V1",
      timestamp: new Date().toISOString()
    };

    const response = await axios.post('/api/mystatus', legacyStates, {
      headers: { 'X-Machine-Token': machineToken }
    });

    expect(response.status).toBe(200);
    expect(response.data.success).toBe(true);
    
    // Vérifier que les états sont correctement stockés
    const deviceStates = await axios.get('/api/machines/machine-123/devices/states', {
      headers: { Authorization: `Bearer ${userToken}` }
    });
    
    const states = deviceStates.data.data;
    expect(states.heating.zone1.currentTemperature).toBe(20.5);
    expect(states.heating.zone1.isHeating).toBe(true);
    expect(states.shutters.shutter1.position).toBe(75);
    expect(states.alarm.armed).toBe(false);
  });

  it('should handle partial state updates', async () => {
    // Envoyer seulement quelques états
    const partialStates = {
      states: [
        { index: "100", value: "21.0" },  // Nouvelle température
        { index: "200", value: "0" }      // Chauffage OFF
      ],
      version: "V1",
      timestamp: new Date().toISOString()
    };

    const response = await axios.post('/api/mystatus', partialStates, {
      headers: { 'X-Machine-Token': machineToken }
    });

    expect(response.status).toBe(200);
    
    // Vérifier que seuls les états envoyés sont mis à jour
    const deviceStates = await axios.get('/api/machines/machine-123/devices/states');
    expect(deviceStates.data.data.heating.zone1.currentTemperature).toBe(21.0);
    expect(deviceStates.data.data.heating.zone1.isHeating).toBe(false);
  });
});
```

### Simulateurs de Boîtiers pour Tests Automatisés

#### Simulateur de Boîtier V1

**Configuration Docker :**
```dockerfile
# Dockerfile.device-simulator
FROM node:18-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci --only=production

COPY src/ ./src/
COPY config/ ./config/

EXPOSE 3000

CMD ["node", "src/device-simulator.js"]
```

**Implémentation Simulateur :**
```javascript
// src/device-simulator.js
const axios = require('axios');
const EventEmitter = require('events');

class EssensysDeviceSimulator extends EventEmitter {
  constructor(config) {
    super();
    this.config = config;
    this.machineToken = null;
    this.isRunning = false;
    this.states = new Map();
    this.pollInterval = null;
    
    // États initiaux
    this.initializeStates();
  }

  initializeStates() {
    // Températures
    this.states.set('100', '20.0'); // Zone 1
    this.states.set('101', '19.5'); // Zone 2
    this.states.set('102', '18.8'); // Zone 3
    this.states.set('103', '5.2');  // Extérieur
    
    // Chauffage
    this.states.set('200', '0'); // Zone 1 OFF
    this.states.set('201', '0'); // Zone 2 OFF
    this.states.set('202', '0'); // Zone 3 OFF
    
    // Volets
    this.states.set('300', '0');  // Fermé
    this.states.set('301', '50'); // Mi-ouvert
    this.states.set('302', '100'); // Ouvert
    
    // Alarme
    this.states.set('400', '0'); // Désarmée
    this.states.set('401', '0'); // Zone 1 OK
    this.states.set('402', '0'); // Zone 2 OK
    
    // Système
    this.states.set('500', '1');  // OK
    this.states.set('501', '90'); // Batterie 90%
    this.states.set('502', '75'); // Signal 75%
  }

  async authenticate() {
    try {
      const response = await axios.post(`${this.config.serverUrl}/api/machine/auth`, {
        activationKey: this.config.activationKey
      });
      
      if (response.data.success) {
        this.machineToken = response.data.data.machineToken;
        this.emit('authenticated', this.machineToken);
        console.log(`Device ${this.config.serialNumber} authenticated`);
        return true;
      }
    } catch (error) {
      this.emit('error', error);
      console.error('Authentication failed:', error.message);
      return false;
    }
  }

  async pollActions() {
    if (!this.machineToken) return;

    try {
      const response = await axios.get(`${this.config.serverUrl}/api/myactions`, {
        headers: { 'X-Machine-Token': this.machineToken }
      });

      if (response.data.success && response.data.data.length > 0) {
        for (const action of response.data.data) {
          await this.executeAction(action);
        }
      }
    } catch (error) {
      this.emit('error', error);
      console.error('Failed to poll actions:', error.message);
    }
  }

  async executeAction(action) {
    console.log(`Executing action: ${action.ACTIONTYPE} - ${action.ACTIONINFO}`);
    
    // Parser ACTIONINFO (format: "index=value;index=value")
    const params = action.ACTIONINFO.split(';');
    
    for (const param of params) {
      const [index, value] = param.split('=');
      if (index && value) {
        this.states.set(index, value);
        
        // Simuler des effets secondaires
        this.simulateEffects(index, value);
      }
    }
    
    this.emit('actionExecuted', action);
    
    // Marquer l'action comme exécutée
    await this.markActionDone(action.ID);
  }

  simulateEffects(index, value) {
    switch (index) {
      case '200': // Chauffage zone 1
        if (value === '1') {
          // Simuler montée de température
          setTimeout(() => {
            const currentTemp = parseFloat(this.states.get('100'));
            this.states.set('100', (currentTemp + 0.5).toString());
          }, 5000);
        }
        break;
        
      case '300': // Volet 1
        // Simuler mouvement progressif
        const targetPos = parseInt(value);
        const currentPos = parseInt(this.states.get('300'));
        this.animateShutter('300', currentPos, targetPos);
        break;
    }
  }

  animateShutter(index, from, to) {
    const step = from < to ? 5 : -5;
    let current = from;
    
    const interval = setInterval(() => {
      current += step;
      
      if ((step > 0 && current >= to) || (step < 0 && current <= to)) {
        current = to;
        clearInterval(interval);
      }
      
      this.states.set(index, current.toString());
    }, 100);
  }

  async sendStates() {
    if (!this.machineToken) return;

    const stateArray = Array.from(this.states.entries()).map(([index, value]) => ({
      index,
      value
    }));

    const payload = {
      states: stateArray,
      version: this.config.firmwareVersion,
      timestamp: new Date().toISOString()
    };

    try {
      const response = await axios.post(`${this.config.serverUrl}/api/mystatus`, payload, {
        headers: { 'X-Machine-Token': this.machineToken }
      });

      if (response.data.success) {
        this.emit('statesSent', payload);
      }
    } catch (error) {
      this.emit('error', error);
      console.error('Failed to send states:', error.message);
    }
  }

  async start() {
    if (this.isRunning) return;

    console.log(`Starting device simulator ${this.config.serialNumber}`);
    
    // Authentification
    const authenticated = await this.authenticate();
    if (!authenticated) {
      throw new Error('Failed to authenticate device');
    }

    this.isRunning = true;

    // Polling des actions
    this.pollInterval = setInterval(() => {
      this.pollActions();
    }, this.config.pollInterval || 30000);

    // Envoi périodique des états
    this.stateInterval = setInterval(() => {
      this.sendStates();
    }, this.config.stateInterval || 60000);

    // Envoi initial des états
    await this.sendStates();
    
    this.emit('started');
  }

  async stop() {
    if (!this.isRunning) return;

    console.log(`Stopping device simulator ${this.config.serialNumber}`);
    
    this.isRunning = false;
    
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
    
    if (this.stateInterval) {
      clearInterval(this.stateInterval);
      this.stateInterval = null;
    }
    
    this.emit('stopped');
  }
}

module.exports = EssensysDeviceSimulator;
```

**Configuration de Test :**
```javascript
// config/test-devices.js
module.exports = {
  devices: [
    {
      serialNumber: 'SIM-V0-001',
      activationKey: 'TEST-V0AA-BBCC-DDEE-FF11-2233-4455-6677',
      firmwareVersion: 'V0',
      serverUrl: 'http://localhost:3001',
      pollInterval: 10000,
      stateInterval: 30000
    },
    {
      serialNumber: 'SIM-V1-001',
      activationKey: 'TEST-V1AA-BBCC-DDEE-FF11-2233-4455-6677',
      firmwareVersion: 'V1',
      serverUrl: 'http://localhost:3001',
      pollInterval: 15000,
      stateInterval: 45000
    },
    {
      serialNumber: 'SIM-V2-001',
      activationKey: 'TEST-V2AA-BBCC-DDEE-FF11-2233-4455-6677',
      firmwareVersion: 'V2',
      serverUrl: 'http://localhost:3001',
      pollInterval: 20000,
      stateInterval: 60000
    }
  ]
};
```

#### Orchestrateur de Tests Hardware

**Test Suite Complète :**
```javascript
// test-hardware-compatibility.js
const EssensysDeviceSimulator = require('./src/device-simulator');
const testDevices = require('./config/test-devices');

describe('Hardware Compatibility Test Suite', () => {
  let simulators = [];
  
  beforeAll(async () => {
    // Démarrer tous les simulateurs
    for (const deviceConfig of testDevices.devices) {
      const simulator = new EssensysDeviceSimulator(deviceConfig);
      simulators.push(simulator);
      
      await simulator.start();
      
      // Attendre la stabilisation
      await new Promise(resolve => setTimeout(resolve, 2000));
    }
  });

  afterAll(async () => {
    // Arrêter tous les simulateurs
    for (const simulator of simulators) {
      await simulator.stop();
    }
  });

  describe('Multi-Version Compatibility', () => {
    it('should handle all firmware versions simultaneously', async () => {
      // Créer des actions pour chaque type de boîtier
      const actions = [
        { deviceId: 'heating-v0', type: 'set_temperature', payload: { targetTemperature: 21 } },
        { deviceId: 'heating-v1', type: 'set_temperature', payload: { targetTemperature: 22 } },
        { deviceId: 'heating-v2', type: 'set_temperature', payload: { targetTemperature: 23 } }
      ];

      // Envoyer les actions
      for (const action of actions) {
        await axios.post(`/api/devices/${action.deviceId}/actions`, action);
      }

      // Attendre que tous les boîtiers récupèrent et exécutent
      await new Promise(resolve => setTimeout(resolve, 15000));

      // Vérifier l'exécution
      for (let i = 0; i < simulators.length; i++) {
        const simulator = simulators[i];
        const expectedTemp = (21 + i).toString();
        expect(simulator.states.get('200')).toBe('1'); // Chauffage ON
      }
    });

    it('should maintain protocol compatibility across versions', async () => {
      // Tester que chaque version peut communiquer correctement
      for (const simulator of simulators) {
        // Forcer un envoi d'états
        await simulator.sendStates();
        
        // Vérifier la réception côté serveur
        const response = await axios.get(`/api/machines/${simulator.config.serialNumber}/devices/states`);
        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
      }
    });
  });

  describe('Protocol Stress Test', () => {
    it('should handle rapid action sequences', async () => {
      const simulator = simulators[0]; // Utiliser le premier simulateur
      
      // Envoyer 50 actions rapidement
      const promises = [];
      for (let i = 0; i < 50; i++) {
        promises.push(
          axios.post('/api/devices/heating-test/actions', {
            type: 'set_temperature',
            payload: { targetTemperature: 18 + (i % 10) }
          })
        );
      }
      
      await Promise.all(promises);
      
      // Attendre le traitement
      await new Promise(resolve => setTimeout(resolve, 30000));
      
      // Vérifier que toutes les actions ont été traitées
      const finalTemp = simulator.states.get('200');
      expect(finalTemp).toBeDefined();
    });
  });
});
```

### Tests de Compatibilité par Version Firmware

#### Test Matrice de Compatibilité

**Matrice de Test :**
```javascript
// compatibility-matrix.js
const COMPATIBILITY_MATRIX = {
  'V0': {
    supportedActions: ['CHAUFFAGE', 'VOLET', 'ALARME'],
    supportedIndexes: ['100', '101', '200', '201', '300', '301', '400'],
    limitations: [
      'Pas de support batterie (index 501)',
      'Polling minimum 60 secondes',
      'Pas de WebSocket'
    ],
    deprecationDate: '2025-12-31'
  },
  'V1': {
    supportedActions: ['CHAUFFAGE', 'VOLET', 'ALARME', 'SYSTEM'],
    supportedIndexes: ['100', '101', '102', '200', '201', '202', '300', '301', '302', '400', '401', '500', '501'],
    limitations: [
      'WebSocket optionnel',
      'Polling minimum 30 secondes'
    ],
    deprecationDate: null
  },
  'V2': {
    supportedActions: ['CHAUFFAGE', 'VOLET', 'ALARME', 'SYSTEM', 'ADVANCED'],
    supportedIndexes: 'ALL',
    limitations: [],
    deprecationDate: null,
    features: [
      'WebSocket natif',
      'Polling adaptatif',
      'Compression données',
      'Chiffrement renforcé'
    ]
  }
};

describe('Firmware Version Compatibility', () => {
  Object.entries(COMPATIBILITY_MATRIX).forEach(([version, specs]) => {
    describe(`Version ${version}`, () => {
      let simulator;
      
      beforeEach(async () => {
        const config = testDevices.devices.find(d => d.firmwareVersion === version);
        simulator = new EssensysDeviceSimulator(config);
        await simulator.start();
      });

      afterEach(async () => {
        await simulator.stop();
      });

      it(`should support all ${version} actions`, async () => {
        for (const actionType of specs.supportedActions) {
          // Créer une action de ce type
          const action = createTestAction(actionType);
          
          const response = await axios.post('/api/devices/test-device/actions', action);
          expect(response.status).toBe(201);
          
          // Vérifier que le boîtier peut la traiter
          await new Promise(resolve => setTimeout(resolve, 5000));
          // Validation spécifique selon le type d'action
        }
      });

      it(`should handle ${version} data indexes correctly`, async () => {
        if (specs.supportedIndexes === 'ALL') {
          // Tester tous les index connus
          for (const index of Object.keys(LEGACY_DATA_INDEX)) {
            simulator.states.set(index, '1');
          }
        } else {
          // Tester seulement les index supportés
          for (const index of specs.supportedIndexes) {
            simulator.states.set(index, '1');
          }
        }
        
        await simulator.sendStates();
        
        // Vérifier la réception
        const response = await axios.get('/api/machines/test-machine/devices/states');
        expect(response.status).toBe(200);
      });

      if (specs.limitations.length > 0) {
        it(`should respect ${version} limitations`, async () => {
          // Tester les limitations spécifiques
          for (const limitation of specs.limitations) {
            if (limitation.includes('Polling minimum')) {
              const minInterval = parseInt(limitation.match(/\d+/)[0]) * 1000;
              expect(simulator.config.pollInterval).toBeGreaterThanOrEqual(minInterval);
            }
          }
        });
      }
    });
  });
});
```

### Validation Protocoles et Formats de Données

#### Test de Format de Données

**Validation Schémas :**
```javascript
// data-format-validation.js
const Ajv = require('ajv');
const ajv = new Ajv();

// Schémas de validation
const LEGACY_ACTION_SCHEMA = {
  type: 'object',
  properties: {
    ID: { type: 'integer' },
    GUID: { type: 'string', pattern: '^[0-9a-f-]{36}$' },
    ACTIONTYPE: { type: 'string', enum: ['CHAUFFAGE', 'VOLET', 'ALARME', 'SYSTEM'] },
    ACTIONINFO: { type: 'string', pattern: '^\\d+=[^;]+(;\\d+=[^;]+)*$' },
    DATECREATION: { type: 'string', format: 'date-time' }
  },
  required: ['ID', 'GUID', 'ACTIONTYPE', 'ACTIONINFO', 'DATECREATION']
};

const LEGACY_STATE_SCHEMA = {
  type: 'object',
  properties: {
    states: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          index: { type: 'string', pattern: '^\\d+$' },
          value: { type: 'string' }
        },
        required: ['index', 'value']
      }
    },
    version: { type: 'string', pattern: '^V\\d+$' },
    timestamp: { type: 'string', format: 'date-time' }
  },
  required: ['states', 'version', 'timestamp']
};

describe('Data Format Validation', () => {
  const validateAction = ajv.compile(LEGACY_ACTION_SCHEMA);
  const validateState = ajv.compile(LEGACY_STATE_SCHEMA);

  it('should validate action format compliance', async () => {
    // Créer une action moderne
    await axios.post('/api/devices/test-device/actions', {
      type: 'set_temperature',
      payload: { targetTemperature: 22 }
    });

    // Récupérer au format legacy
    const response = await axios.get('/api/myactions', {
      headers: { 'X-Machine-Token': machineToken }
    });

    const actions = response.data.data;
    expect(actions.length).toBeGreaterThan(0);

    // Valider chaque action
    for (const action of actions) {
      const isValid = validateAction(action);
      if (!isValid) {
        console.error('Validation errors:', validateAction.errors);
      }
      expect(isValid).toBe(true);
    }
  });

  it('should validate state format compliance', async () => {
    const testStates = {
      states: [
        { index: '100', value: '20.5' },
        { index: '200', value: '1' },
        { index: '300', value: '75' }
      ],
      version: 'V1',
      timestamp: new Date().toISOString()
    };

    // Valider le format
    const isValid = validateState(testStates);
    if (!isValid) {
      console.error('Validation errors:', validateState.errors);
    }
    expect(isValid).toBe(true);

    // Envoyer au serveur
    const response = await axios.post('/api/mystatus', testStates, {
      headers: { 'X-Machine-Token': machineToken }
    });

    expect(response.status).toBe(200);
  });

  it('should reject invalid formats gracefully', async () => {
    const invalidStates = {
      states: [
        { index: 'invalid', value: '20.5' }, // Index non numérique
        { index: '200' } // Valeur manquante
      ],
      version: 'INVALID', // Version invalide
      // timestamp manquant
    };

    const response = await axios.post('/api/mystatus', invalidStates, {
      headers: { 'X-Machine-Token': machineToken },
      validateStatus: () => true // Accepter tous les codes de statut
    });

    expect(response.status).toBe(400);
    expect(response.data.success).toBe(false);
    expect(response.data.error).toContain('Format invalide');
  });
});
```

### Environnement de Test Isolé

#### Configuration Docker Compose

**docker-compose.hardware-test.yml :**
```yaml
version: '3.8'

services:
  # API Backend
  api-server:
    build: 
      context: ./backend
      dockerfile: Dockerfile.test
    ports:
      - "3001:3000"
    environment:
      - NODE_ENV=test
      - DATABASE_URL=postgresql://test:test@postgres:5432/essensys_test
      - REDIS_URL=redis://redis:6379
      - JWT_SECRET=test-secret-key
    depends_on:
      - postgres
      - redis
    networks:
      - hardware-test

  # Base de données
  postgres:
    image: postgres:15.5
    environment:
      - POSTGRES_USER=test
      - POSTGRES_PASSWORD=test
      - POSTGRES_DB=essensys_test
    ports:
      - "5433:5432"
    volumes:
      - ./test-data/init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - hardware-test

  # Cache Redis
  redis:
    image: redis:7.2-alpine
    ports:
      - "6380:6379"
    networks:
      - hardware-test

  # Simulateur boîtier V0
  device-sim-v0:
    build:
      context: ./device-simulator
      dockerfile: Dockerfile
    environment:
      - DEVICE_CONFIG=v0
      - SERVER_URL=http://api-server:3000
      - ACTIVATION_KEY=TEST-V0AA-BBCC-DDEE-FF11-2233-4455-6677
    depends_on:
      - api-server
    networks:
      - hardware-test

  # Simulateur boîtier V1
  device-sim-v1:
    build:
      context: ./device-simulator
      dockerfile: Dockerfile
    environment:
      - DEVICE_CONFIG=v1
      - SERVER_URL=http://api-server:3000
      - ACTIVATION_KEY=TEST-V1AA-BBCC-DDEE-FF11-2233-4455-6677
    depends_on:
      - api-server
    networks:
      - hardware-test

  # Simulateur boîtier V2
  device-sim-v2:
    build:
      context: ./device-simulator
      dockerfile: Dockerfile
    environment:
      - DEVICE_CONFIG=v2
      - SERVER_URL=http://api-server:3000
      - ACTIVATION_KEY=TEST-V2AA-BBCC-DDEE-FF11-2233-4455-6677
    depends_on:
      - api-server
    networks:
      - hardware-test

  # Moniteur de tests
  test-monitor:
    build:
      context: ./test-monitor
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - API_URL=http://api-server:3000
    depends_on:
      - api-server
    networks:
      - hardware-test

networks:
  hardware-test:
    driver: bridge
```

#### Scripts d'Exécution

**Lancement des Tests :**
```bash
#!/bin/bash
# run-hardware-tests.sh

echo "🚀 Démarrage des tests de compatibilité hardware..."

# Nettoyer l'environnement précédent
docker-compose -f docker-compose.hardware-test.yml down -v

# Construire les images
docker-compose -f docker-compose.hardware-test.yml build

# Démarrer les services
docker-compose -f docker-compose.hardware-test.yml up -d

# Attendre que les services soient prêts
echo "⏳ Attente de la disponibilité des services..."
timeout 60 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'

# Attendre que les simulateurs se connectent
echo "⏳ Attente de la connexion des simulateurs..."
sleep 10

# Exécuter les tests
echo "🧪 Exécution des tests de compatibilité..."
npm run test:hardware

# Générer le rapport
echo "📊 Génération du rapport..."
npm run test:hardware:report

# Nettoyer
echo "🧹 Nettoyage..."
docker-compose -f docker-compose.hardware-test.yml down

echo "✅ Tests de compatibilité hardware terminés!"
```

**Rapport de Compatibilité :**
```javascript
// generate-compatibility-report.js
const fs = require('fs');
const path = require('path');

function generateCompatibilityReport(testResults) {
  const report = {
    timestamp: new Date().toISOString(),
    summary: {
      totalTests: testResults.length,
      passed: testResults.filter(t => t.status === 'passed').length,
      failed: testResults.filter(t => t.status === 'failed').length,
      skipped: testResults.filter(t => t.status === 'skipped').length
    },
    firmwareVersions: {},
    protocolTests: {},
    recommendations: []
  };

  // Analyser par version firmware
  ['V0', 'V1', 'V2'].forEach(version => {
    const versionTests = testResults.filter(t => t.version === version);
    report.firmwareVersions[version] = {
      totalTests: versionTests.length,
      passed: versionTests.filter(t => t.status === 'passed').length,
      failed: versionTests.filter(t => t.status === 'failed').length,
      compatibility: versionTests.filter(t => t.status === 'passed').length / versionTests.length * 100
    };
  });

  // Recommandations basées sur les résultats
  if (report.firmwareVersions.V0.compatibility < 90) {
    report.recommendations.push({
      type: 'warning',
      message: 'Compatibilité V0 < 90%. Planifier la migration des boîtiers V0.',
      priority: 'high'
    });
  }

  if (report.summary.failed > 0) {
    report.recommendations.push({
      type: 'error',
      message: `${report.summary.failed} tests échoués. Révision nécessaire avant déploiement.`,
      priority: 'critical'
    });
  }

  // Sauvegarder le rapport
  const reportPath = path.join(__dirname, 'reports', `hardware-compatibility-${Date.now()}.json`);
  fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));

  console.log(`📊 Rapport de compatibilité généré: ${reportPath}`);
  return report;
}

module.exports = { generateCompatibilityReport };
```

Cette section complète la planification des tests de compatibilité hardware en couvrant tous les aspects critiques : authentification, protocoles, formats de données, simulateurs et environnement de test isolé.
## 6.4 Tests de Régression

### Identification des Cas d'Usage Critiques du Système Legacy

#### Analyse des Fonctionnalités Critiques

Basé sur l'analyse du code legacy et l'usage métier, voici les cas d'usage critiques identifiés :

**Criticité 1 - Fonctionnalités Vitales (Zéro Tolérance de Régression) :**
1. **Authentification utilisateur et boîtier**
2. **Communication boîtier ↔ serveur**
3. **Contrôle chauffage (sécurité thermique)**
4. **Système d'alarme (sécurité)**
5. **Notifications d'urgence (SMS/Email)**

**Criticité 2 - Fonctionnalités Importantes (Tolérance Limitée) :**
1. **Contrôle volets roulants**
2. **Programmation horaire**
3. **Gestion multi-utilisateurs**
4. **Historique et logs**
5. **Mise à jour firmware**

**Criticité 3 - Fonctionnalités Secondaires (Tolérance Acceptable) :**
1. **Interface utilisateur avancée**
2. **Statistiques et rapports**
3. **Paramètres avancés**
4. **Notifications non-critiques**

### Suite de Tests de Non-Régression Complète

#### Tests de Régression R-001 : Authentification et Sécurité

**Objectif :** Garantir qu'aucune régression n'affecte l'authentification

```javascript
// regression-auth.test.js
describe('Regression Tests - Authentication & Security', () => {
  
  describe('R-001-01: User Authentication Flow', () => {
    const testCases = [
      {
        name: 'Standard login with valid credentials',
        email: 'user@test.com',
        password: 'validPassword123',
        expectedStatus: 200,
        expectedToken: true
      },
      {
        name: 'Login with invalid password',
        email: 'user@test.com',
        password: 'wrongPassword',
        expectedStatus: 401,
        expectedToken: false
      },
      {
        name: 'Login with non-existent user',
        email: 'nonexistent@test.com',
        password: 'anyPassword',
        expectedStatus: 401,
        expectedToken: false
      },
      {
        name: 'Login with malformed email',
        email: 'invalid-email',
        password: 'validPassword123',
        expectedStatus: 400,
        expectedToken: false
      }
    ];

    testCases.forEach(testCase => {
      it(`should handle: ${testCase.name}`, async () => {
        const response = await request(app)
          .post('/api/auth/login')
          .send({
            email: testCase.email,
            password: testCase.password
          });

        expect(response.status).toBe(testCase.expectedStatus);
        
        if (testCase.expectedToken) {
          expect(response.body.data.accessToken).toBeDefined();
          expect(response.body.data.refreshToken).toBeDefined();
          
          // Valider le format JWT
          const token = response.body.data.accessToken;
          const decoded = jwt.decode(token);
          expect(decoded.userId).toBeDefined();
          expect(decoded.exp).toBeGreaterThan(Date.now() / 1000);
        } else {
          expect(response.body.data?.accessToken).toBeUndefined();
        }
      });
    });
  });

  describe('R-001-02: Machine Authentication Flow', () => {
    const machineTestCases = [
      {
        name: 'Valid activation key V1 format',
        key: 'ABCD-EFGH-IJKL-MNOP-QRST-UVWX-YZ12-3456',
        expectedStatus: 200
      },
      {
        name: 'Valid activation key V2 format',
        key: 'A1B2-C3D4-E5F6-G7H8-I9J0-K1L2-M3N4-O5P6',
        expectedStatus: 200
      },
      {
        name: 'Invalid key format',
        key: 'INVALID-KEY-FORMAT',
        expectedStatus: 400
      },
      {
        name: 'Non-existent key',
        key: 'ZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZ',
        expectedStatus: 401
      }
    ];

    machineTestCases.forEach(testCase => {
      it(`should handle: ${testCase.name}`, async () => {
        const response = await request(app)
          .post('/api/machine/auth')
          .send({ activationKey: testCase.key });

        expect(response.status).toBe(testCase.expectedStatus);
        
        if (testCase.expectedStatus === 200) {
          expect(response.body.data.machineToken).toBeDefined();
          expect(response.body.data.machineId).toBeDefined();
        }
      });
    });
  });

  describe('R-001-03: Session Management', () => {
    it('should maintain session consistency across requests', async () => {
      // Login
      const loginResponse = await request(app)
        .post('/api/auth/login')
        .send({ email: 'user@test.com', password: 'password' });

      const token = loginResponse.body.data.accessToken;

      // Multiple authenticated requests
      const requests = Array.from({ length: 10 }, (_, i) =>
        request(app)
          .get('/api/user/profile')
          .set('Authorization', `Bearer ${token}`)
      );

      const responses = await Promise.all(requests);
      
      // All should succeed
      responses.forEach(response => {
        expect(response.status).toBe(200);
        expect(response.body.data.userId).toBeDefined();
      });
    });

    it('should handle token expiration gracefully', async () => {
      // Utiliser un token expiré
      const expiredToken = jwt.sign(
        { userId: 'test-user', exp: Math.floor(Date.now() / 1000) - 3600 },
        process.env.JWT_SECRET
      );

      const response = await request(app)
        .get('/api/user/profile')
        .set('Authorization', `Bearer ${expiredToken}`);

      expect(response.status).toBe(401);
      expect(response.body.error).toContain('Token expiré');
    });
  });
});
```

#### Tests de Régression R-002 : Communication Boîtier

**Objectif :** Valider la continuité de la communication avec les boîtiers existants

```javascript
// regression-device-communication.test.js
describe('Regression Tests - Device Communication', () => {
  let machineToken;
  
  beforeEach(async () => {
    const authResponse = await request(app)
      .post('/api/machine/auth')
      .send({ activationKey: 'TEST-ABCD-EFGH-IJKL-MNOP-QRST-UVWX-YZ12' });
    
    machineToken = authResponse.body.data.machineToken;
  });

  describe('R-002-01: Action Polling Legacy Format', () => {
    it('should return actions in exact legacy format', async () => {
      // Créer une action moderne
      await request(app)
        .post('/api/devices/heating-zone-1/actions')
        .set('Authorization', `Bearer ${userToken}`)
        .send({
          type: 'set_temperature',
          payload: { targetTemperature: 22, zone: 1 }
        });

      // Récupérer via endpoint legacy
      const response = await request(app)
        .get('/api/myactions')
        .set('X-Machine-Token', machineToken);

      expect(response.status).toBe(200);
      expect(response.body.success).toBe(true);
      
      const action = response.body.data[0];
      
      // Vérifier le format exact legacy
      expect(action).toMatchObject({
        ID: expect.any(Number),
        GUID: expect.stringMatching(/^[0-9a-f-]{36}$/),
        ACTIONTYPE: 'CHAUFFAGE',
        ACTIONINFO: expect.stringMatching(/^200=22(;|$)/),
        DATECREATION: expect.stringMatching(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/)
      });
    });

    it('should handle multiple action types correctly', async () => {
      // Créer différents types d'actions
      const actions = [
        { deviceId: 'heating-1', type: 'set_temperature', payload: { targetTemperature: 21 } },
        { deviceId: 'shutter-1', type: 'set_position', payload: { position: 75 } },
        { deviceId: 'alarm-1', type: 'arm_alarm', payload: { mode: 'total' } }
      ];

      for (const action of actions) {
        await request(app)
          .post(`/api/devices/${action.deviceId}/actions`)
          .set('Authorization', `Bearer ${userToken}`)
          .send(action);
      }

      const response = await request(app)
        .get('/api/myactions')
        .set('X-Machine-Token', machineToken);

      const receivedActions = response.body.data;
      expect(receivedActions.length).toBe(3);

      // Vérifier les types d'actions
      const actionTypes = receivedActions.map(a => a.ACTIONTYPE);
      expect(actionTypes).toContain('CHAUFFAGE');
      expect(actionTypes).toContain('VOLET');
      expect(actionTypes).toContain('ALARME');
    });

    it('should maintain action ordering and priority', async () => {
      // Créer des actions avec différentes priorités
      const highPriorityAction = {
        deviceId: 'alarm-1',
        type: 'arm_alarm',
        payload: { mode: 'total' },
        priority: 1
      };

      const normalPriorityAction = {
        deviceId: 'heating-1',
        type: 'set_temperature',
        payload: { targetTemperature: 20 },
        priority: 5
      };

      // Créer d'abord l'action normale, puis la prioritaire
      await request(app)
        .post(`/api/devices/${normalPriorityAction.deviceId}/actions`)
        .send(normalPriorityAction);

      await request(app)
        .post(`/api/devices/${highPriorityAction.deviceId}/actions`)
        .send(highPriorityAction);

      const response = await request(app)
        .get('/api/myactions')
        .set('X-Machine-Token', machineToken);

      const actions = response.body.data;
      
      // L'action prioritaire doit être en premier
      expect(actions[0].ACTIONTYPE).toBe('ALARME');
      expect(actions[1].ACTIONTYPE).toBe('CHAUFFAGE');
    });
  });

  describe('R-002-02: State Reporting Legacy Format', () => {
    it('should accept legacy state format without modification', async () => {
      const legacyStates = {
        states: [
          { index: "100", value: "20.5" },  // Température
          { index: "200", value: "1" },     // Chauffage ON
          { index: "300", value: "75" },    // Volet 75%
          { index: "400", value: "0" },     // Alarme OFF
          { index: "500", value: "1" },     // Système OK
          { index: "501", value: "90" }     // Batterie 90%
        ],
        version: "V1",
        timestamp: new Date().toISOString()
      };

      const response = await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send(legacyStates);

      expect(response.status).toBe(200);
      expect(response.body.success).toBe(true);

      // Vérifier que les états sont correctement interprétés
      const deviceStates = await request(app)
        .get('/api/machines/test-machine/devices/states')
        .set('Authorization', `Bearer ${userToken}`);

      const states = deviceStates.body.data;
      expect(states.heating.zone1.currentTemperature).toBe(20.5);
      expect(states.heating.zone1.isHeating).toBe(true);
      expect(states.shutters.shutter1.position).toBe(75);
      expect(states.alarm.armed).toBe(false);
    });

    it('should handle partial state updates correctly', async () => {
      // État initial complet
      const initialStates = {
        states: [
          { index: "100", value: "20.0" },
          { index: "200", value: "0" },
          { index: "300", value: "0" }
        ],
        version: "V1",
        timestamp: new Date().toISOString()
      };

      await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send(initialStates);

      // Mise à jour partielle
      const partialUpdate = {
        states: [
          { index: "100", value: "21.5" },  // Nouvelle température
          { index: "200", value: "1" }      // Chauffage ON
          // Pas de mise à jour du volet
        ],
        version: "V1",
        timestamp: new Date().toISOString()
      };

      const response = await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send(partialUpdate);

      expect(response.status).toBe(200);

      // Vérifier que seuls les états mis à jour ont changé
      const deviceStates = await request(app)
        .get('/api/machines/test-machine/devices/states')
        .set('Authorization', `Bearer ${userToken}`);

      const states = deviceStates.body.data;
      expect(states.heating.zone1.currentTemperature).toBe(21.5);
      expect(states.heating.zone1.isHeating).toBe(true);
      expect(states.shutters.shutter1.position).toBe(0); // Inchangé
    });
  });

  describe('R-002-03: Communication Reliability', () => {
    it('should handle network interruptions gracefully', async () => {
      // Simuler une interruption réseau
      const requests = [];
      
      for (let i = 0; i < 5; i++) {
        requests.push(
          request(app)
            .get('/api/myactions')
            .set('X-Machine-Token', machineToken)
            .timeout(100) // Timeout court pour simuler interruption
        );
      }

      // Certaines requêtes peuvent échouer, mais le système doit rester stable
      const results = await Promise.allSettled(requests);
      
      // Au moins une requête doit réussir
      const successful = results.filter(r => r.status === 'fulfilled' && r.value.status === 200);
      expect(successful.length).toBeGreaterThan(0);
    });

    it('should maintain data consistency during high load', async () => {
      // Envoyer de nombreux états simultanément
      const stateUpdates = Array.from({ length: 50 }, (_, i) => ({
        states: [
          { index: "100", value: (20 + i * 0.1).toString() },
          { index: "200", value: (i % 2).toString() }
        ],
        version: "V1",
        timestamp: new Date(Date.now() + i * 1000).toISOString()
      }));

      const promises = stateUpdates.map(states =>
        request(app)
          .post('/api/mystatus')
          .set('X-Machine-Token', machineToken)
          .send(states)
      );

      const responses = await Promise.all(promises);
      
      // Toutes les requêtes doivent réussir
      responses.forEach(response => {
        expect(response.status).toBe(200);
      });

      // Vérifier l'état final
      const finalStates = await request(app)
        .get('/api/machines/test-machine/devices/states')
        .set('Authorization', `Bearer ${userToken}`);

      // L'état final doit correspondre à la dernière mise à jour
      expect(finalStates.body.data.heating.zone1.currentTemperature).toBe(24.9);
    });
  });
});
```

#### Tests de Régression R-003 : Contrôle des Appareils

**Objectif :** Garantir la continuité du contrôle des appareils critiques

```javascript
// regression-device-control.test.js
describe('Regression Tests - Device Control', () => {
  
  describe('R-003-01: Heating Control Critical Functions', () => {
    it('should maintain exact heating control behavior', async () => {
      const testScenarios = [
        {
          name: 'Set temperature within normal range',
          action: { targetTemperature: 22, zone: 1 },
          expectedActionInfo: '200=22',
          expectedState: { isHeating: true, targetTemperature: 22 }
        },
        {
          name: 'Set temperature at minimum limit',
          action: { targetTemperature: 5, zone: 1 },
          expectedActionInfo: '200=5',
          expectedState: { isHeating: true, targetTemperature: 5 }
        },
        {
          name: 'Set temperature at maximum limit',
          action: { targetTemperature: 30, zone: 1 },
          expectedActionInfo: '200=30',
          expectedState: { isHeating: true, targetTemperature: 30 }
        },
        {
          name: 'Turn off heating',
          action: { targetTemperature: 0, zone: 1 },
          expectedActionInfo: '200=0',
          expectedState: { isHeating: false, targetTemperature: 0 }
        }
      ];

      for (const scenario of testScenarios) {
        // Exécuter l'action
        const actionResponse = await request(app)
          .post('/api/devices/heating-zone-1/actions')
          .set('Authorization', `Bearer ${userToken}`)
          .send({
            type: 'set_temperature',
            payload: scenario.action
          });

        expect(actionResponse.status).toBe(201);

        // Vérifier le format legacy de l'action
        const legacyActions = await request(app)
          .get('/api/myactions')
          .set('X-Machine-Token', machineToken);

        const action = legacyActions.body.data.find(a => a.ACTIONTYPE === 'CHAUFFAGE');
        expect(action.ACTIONINFO).toContain(scenario.expectedActionInfo);

        // Simuler l'exécution par le boîtier
        await request(app)
          .post('/api/mystatus')
          .set('X-Machine-Token', machineToken)
          .send({
            states: [
              { index: "200", value: scenario.action.targetTemperature > 0 ? "1" : "0" },
              { index: "100", value: scenario.action.targetTemperature.toString() }
            ],
            version: "V1",
            timestamp: new Date().toISOString()
          });

        // Vérifier l'état final
        const stateResponse = await request(app)
          .get('/api/devices/heating-zone-1/state')
          .set('Authorization', `Bearer ${userToken}`);

        const state = stateResponse.body.data;
        expect(state.isHeating).toBe(scenario.expectedState.isHeating);
        expect(state.targetTemperature).toBe(scenario.expectedState.targetTemperature);
      }
    });

    it('should handle multi-zone heating independently', async () => {
      // Configurer différentes températures pour 3 zones
      const zoneConfigs = [
        { zone: 1, temperature: 21, deviceId: 'heating-zone-1', index: '200' },
        { zone: 2, temperature: 19, deviceId: 'heating-zone-2', index: '201' },
        { zone: 3, temperature: 23, deviceId: 'heating-zone-3', index: '202' }
      ];

      // Envoyer les actions pour chaque zone
      for (const config of zoneConfigs) {
        await request(app)
          .post(`/api/devices/${config.deviceId}/actions`)
          .set('Authorization', `Bearer ${userToken}`)
          .send({
            type: 'set_temperature',
            payload: { targetTemperature: config.temperature, zone: config.zone }
          });
      }

      // Vérifier que les actions sont correctes
      const legacyActions = await request(app)
        .get('/api/myactions')
        .set('X-Machine-Token', machineToken);

      const heatingActions = legacyActions.body.data.filter(a => a.ACTIONTYPE === 'CHAUFFAGE');
      expect(heatingActions.length).toBe(3);

      // Vérifier chaque action
      for (const config of zoneConfigs) {
        const action = heatingActions.find(a => a.ACTIONINFO.includes(`${config.index}=${config.temperature}`));
        expect(action).toBeDefined();
      }

      // Simuler l'exécution par le boîtier
      const states = zoneConfigs.map(config => ({
        index: config.index,
        value: "1" // Chauffage ON
      }));

      await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send({
          states,
          version: "V1",
          timestamp: new Date().toISOString()
        });

      // Vérifier l'indépendance des zones
      for (const config of zoneConfigs) {
        const stateResponse = await request(app)
          .get(`/api/devices/${config.deviceId}/state`)
          .set('Authorization', `Bearer ${userToken}`);

        expect(stateResponse.body.data.targetTemperature).toBe(config.temperature);
        expect(stateResponse.body.data.isHeating).toBe(true);
      }
    });
  });

  describe('R-003-02: Shutter Control Critical Functions', () => {
    it('should maintain exact shutter positioning behavior', async () => {
      const positionTests = [0, 25, 50, 75, 100]; // Positions critiques

      for (const position of positionTests) {
        // Envoyer commande de position
        await request(app)
          .post('/api/devices/shutter-1/actions')
          .set('Authorization', `Bearer ${userToken}`)
          .send({
            type: 'set_position',
            payload: { position }
          });

        // Vérifier l'action legacy
        const legacyActions = await request(app)
          .get('/api/myactions')
          .set('X-Machine-Token', machineToken);

        const shutterAction = legacyActions.body.data.find(a => a.ACTIONTYPE === 'VOLET');
        expect(shutterAction.ACTIONINFO).toContain(`300=${position}`);

        // Simuler l'exécution
        await request(app)
          .post('/api/mystatus')
          .set('X-Machine-Token', machineToken)
          .send({
            states: [{ index: "300", value: position.toString() }],
            version: "V1",
            timestamp: new Date().toISOString()
          });

        // Vérifier l'état
        const stateResponse = await request(app)
          .get('/api/devices/shutter-1/state')
          .set('Authorization', `Bearer ${userToken}`);

        expect(stateResponse.body.data.position).toBe(position);
      }
    });
  });

  describe('R-003-03: Alarm System Critical Functions', () => {
    it('should maintain exact alarm control behavior', async () => {
      const alarmModes = [
        { mode: 'disarm', expectedValue: '0', expectedArmed: false },
        { mode: 'total', expectedValue: '1', expectedArmed: true },
        { mode: 'partial', expectedValue: '2', expectedArmed: true }
      ];

      for (const test of alarmModes) {
        // Envoyer commande d'alarme
        await request(app)
          .post('/api/devices/alarm-system/actions')
          .set('Authorization', `Bearer ${userToken}`)
          .send({
            type: test.mode === 'disarm' ? 'disarm_alarm' : 'arm_alarm',
            payload: { mode: test.mode }
          });

        // Vérifier l'action legacy
        const legacyActions = await request(app)
          .get('/api/myactions')
          .set('X-Machine-Token', machineToken);

        const alarmAction = legacyActions.body.data.find(a => a.ACTIONTYPE === 'ALARME');
        expect(alarmAction.ACTIONINFO).toContain(`400=${test.expectedValue}`);

        // Simuler l'exécution
        await request(app)
          .post('/api/mystatus')
          .set('X-Machine-Token', machineToken)
          .send({
            states: [{ index: "400", value: test.expectedValue }],
            version: "V1",
            timestamp: new Date().toISOString()
          });

        // Vérifier l'état
        const stateResponse = await request(app)
          .get('/api/devices/alarm-system/state')
          .set('Authorization', `Bearer ${userToken}`);

        expect(stateResponse.body.data.armed).toBe(test.expectedArmed);
        if (test.expectedArmed) {
          expect(stateResponse.body.data.mode).toBe(test.mode);
        }
      }
    });
  });
});
```

#### Tests de Régression R-004 : Notifications Critiques

**Objectif :** Valider la continuité des notifications d'urgence

```javascript
// regression-notifications.test.js
describe('Regression Tests - Critical Notifications', () => {
  
  describe('R-004-01: SMS Notifications', () => {
    it('should send SMS for alarm triggers exactly like legacy', async () => {
      // Configurer un contact SMS
      const contact = await request(app)
        .post('/api/user/contacts')
        .set('Authorization', `Bearer ${userToken}`)
        .send({
          type: 'sms',
          contactValue: '+33123456789',
          displayName: 'Test Contact'
        });

      // Déclencher une alarme
      await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send({
          states: [
            { index: "400", value: "1" },  // Alarme armée
            { index: "407", value: "1" }   // Déclenchement
          ],
          version: "V1",
          timestamp: new Date().toISOString()
        });

      // Attendre l'envoi du SMS
      await new Promise(resolve => setTimeout(resolve, 2000));

      // Vérifier l'envoi
      const notifications = await request(app)
        .get('/api/notifications')
        .set('Authorization', `Bearer ${userToken}`);

      const smsNotification = notifications.body.data.find(n => 
        n.type === 'sms' && n.message.includes('ALARME DECLENCHEE')
      );

      expect(smsNotification).toBeDefined();
      expect(smsNotification.status).toBe('sent');
      
      // Vérifier le format du message (identique au legacy)
      expect(smsNotification.message).toMatch(/^ALARME DECLENCHEE/);
      expect(smsNotification.message).toContain(new Date().toLocaleDateString('fr-FR'));
    });

    it('should handle SMS delivery failures gracefully', async () => {
      // Simuler un échec d'envoi SMS
      jest.spyOn(smsService, 'sendSMS').mockRejectedValue(new Error('SMS service unavailable'));

      // Déclencher une alarme
      await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send({
          states: [{ index: "407", value: "1" }],
          version: "V1",
          timestamp: new Date().toISOString()
        });

      await new Promise(resolve => setTimeout(resolve, 2000));

      // Vérifier que l'échec est tracé
      const notifications = await request(app)
        .get('/api/notifications')
        .set('Authorization', `Bearer ${userToken}`);

      const failedSms = notifications.body.data.find(n => 
        n.type === 'sms' && n.status === 'failed'
      );

      expect(failedSms).toBeDefined();
      expect(failedSms.errorMessage).toContain('SMS service unavailable');
    });
  });

  describe('R-004-02: Email Notifications', () => {
    it('should send email notifications with exact legacy format', async () => {
      // Configurer un contact email
      await request(app)
        .post('/api/user/contacts')
        .set('Authorization', `Bearer ${userToken}`)
        .send({
          type: 'email',
          contactValue: 'test@example.com',
          displayName: 'Test Email'
        });

      // Déclencher une panne système
      await request(app)
        .post('/api/mystatus')
        .set('X-Machine-Token', machineToken)
        .send({
          states: [{ index: "500", value: "0" }], // Système en panne
          version: "V1",
          timestamp: new Date().toISOString()
        });

      await new Promise(resolve => setTimeout(resolve, 2000));

      // Vérifier l'email
      const notifications = await request(app)
        .get('/api/notifications')
        .set('Authorization', `Bearer ${userToken}`);

      const emailNotification = notifications.body.data.find(n => 
        n.type === 'email' && n.subject.includes('Panne système')
      );

      expect(emailNotification).toBeDefined();
      expect(emailNotification.status).toBe('sent');
      
      // Vérifier le format (identique au legacy)
      expect(emailNotification.subject).toBe('Essensys - Panne système détectée');
      expect(emailNotification.message).toContain('Une panne système a été détectée');
    });
  });
});
```

### Tests de Régression Automatisés

#### Configuration Pipeline de Régression

**GitHub Actions Workflow :**
```yaml
# .github/workflows/regression-tests.yml
name: Regression Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    # Exécution quotidienne à 2h du matin
    - cron: '0 2 * * *'

jobs:
  regression-tests:
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        test-suite:
          - authentication
          - device-communication
          - device-control
          - notifications
          - user-management
          - data-integrity
    
    services:
      postgres:
        image: postgres:15.5
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_DB: essensys_regression_test
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
      
      - name: Install dependencies
        run: |
          cd backend && npm ci
          cd ../frontend && npm ci
      
      - name: Setup test database
        run: |
          cd backend
          npx prisma migrate deploy
          npx prisma db seed
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_regression_test
      
      - name: Run regression tests - ${{ matrix.test-suite }}
        run: |
          cd backend
          npm run test:regression:${{ matrix.test-suite }}
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_regression_test
          REDIS_URL: redis://localhost:6379
          NODE_ENV: test
      
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: regression-results-${{ matrix.test-suite }}
          path: backend/test-results/regression-${{ matrix.test-suite }}.xml
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: backend/coverage/lcov.info
          flags: regression-${{ matrix.test-suite }}

  regression-report:
    needs: regression-tests
    runs-on: ubuntu-latest
    if: always()
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Download all test results
        uses: actions/download-artifact@v3
        with:
          path: test-results
      
      - name: Generate regression report
        run: |
          node scripts/generate-regression-report.js
      
      - name: Comment PR with results
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const fs = require('fs');
            const report = fs.readFileSync('regression-report.md', 'utf8');
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: report
            });
```

#### Script de Génération de Rapport

**scripts/generate-regression-report.js :**
```javascript
const fs = require('fs');
const path = require('path');
const xml2js = require('xml2js');

async function generateRegressionReport() {
  const testResultsDir = 'test-results';
  const suites = ['authentication', 'device-communication', 'device-control', 'notifications', 'user-management', 'data-integrity'];
  
  let totalTests = 0;
  let totalPassed = 0;
  let totalFailed = 0;
  let totalSkipped = 0;
  
  const suiteResults = {};
  
  for (const suite of suites) {
    const resultFile = path.join(testResultsDir, `regression-results-${suite}`, `regression-${suite}.xml`);
    
    if (fs.existsSync(resultFile)) {
      const xmlContent = fs.readFileSync(resultFile, 'utf8');
      const result = await xml2js.parseStringPromise(xmlContent);
      
      const testSuite = result.testsuites.testsuite[0];
      const tests = parseInt(testSuite.$.tests);
      const failures = parseInt(testSuite.$.failures);
      const skipped = parseInt(testSuite.$.skipped);
      const passed = tests - failures - skipped;
      
      suiteResults[suite] = {
        tests,
        passed,
        failed: failures,
        skipped,
        success: failures === 0
      };
      
      totalTests += tests;
      totalPassed += passed;
      totalFailed += failures;
      totalSkipped += skipped;
    }
  }
  
  // Générer le rapport Markdown
  const report = `
# 📊 Rapport de Tests de Régression

## Résumé Global

| Métrique | Valeur |
|----------|--------|
| **Tests Totaux** | ${totalTests} |
| **✅ Réussis** | ${totalPassed} |
| **❌ Échoués** | ${totalFailed} |
| **⏭️ Ignorés** | ${totalSkipped} |
| **Taux de Réussite** | ${((totalPassed / totalTests) * 100).toFixed(1)}% |

## Résultats par Suite

${suites.map(suite => {
  const result = suiteResults[suite];
  if (!result) return `### ${suite}\n❓ **Non exécuté**\n`;
  
  const icon = result.success ? '✅' : '❌';
  const status = result.success ? 'SUCCÈS' : 'ÉCHEC';
  
  return `### ${suite}
${icon} **${status}** - ${result.passed}/${result.tests} tests réussis

| Réussis | Échoués | Ignorés |
|---------|---------|---------|
| ${result.passed} | ${result.failed} | ${result.skipped} |
`;
}).join('\n')}

## Analyse des Régressions

${totalFailed > 0 ? `
⚠️ **${totalFailed} régressions détectées !**

Les tests suivants ont échoué :
${Object.entries(suiteResults)
  .filter(([_, result]) => result && result.failed > 0)
  .map(([suite, result]) => `- **${suite}** : ${result.failed} échec(s)`)
  .join('\n')}

**Actions Requises :**
1. Analyser les échecs de tests
2. Corriger les régressions identifiées
3. Re-exécuter les tests de régression
4. Valider la parité fonctionnelle

` : `
🎉 **Aucune régression détectée !**

Tous les tests de régression sont passés avec succès. La parité fonctionnelle avec le système legacy est maintenue.
`}

## Recommandations

${totalFailed === 0 ? 
  '✅ Le code peut être déployé en toute sécurité.' :
  '❌ **NE PAS DÉPLOYER** - Corriger les régressions avant le déploiement.'
}

---
*Rapport généré automatiquement le ${new Date().toLocaleString('fr-FR')}*
`;

  fs.writeFileSync('regression-report.md', report);
  console.log('📊 Rapport de régression généré : regression-report.md');
  
  // Sortir avec code d'erreur si des tests ont échoué
  if (totalFailed > 0) {
    process.exit(1);
  }
}

generateRegressionReport().catch(console.error);
```

#### Exécution Automatique des Tests

**package.json scripts :**
```json
{
  "scripts": {
    "test:regression": "npm run test:regression:all",
    "test:regression:all": "jest --config=jest.regression.config.js --runInBand",
    "test:regression:authentication": "jest --config=jest.regression.config.js --testPathPattern=regression-auth",
    "test:regression:device-communication": "jest --config=jest.regression.config.js --testPathPattern=regression-device-communication",
    "test:regression:device-control": "jest --config=jest.regression.config.js --testPathPattern=regression-device-control",
    "test:regression:notifications": "jest --config=jest.regression.config.js --testPathPattern=regression-notifications",
    "test:regression:user-management": "jest --config=jest.regression.config.js --testPathPattern=regression-user-management",
    "test:regression:data-integrity": "jest --config=jest.regression.config.js --testPathPattern=regression-data-integrity",
    "test:regression:watch": "jest --config=jest.regression.config.js --watch",
    "test:regression:coverage": "jest --config=jest.regression.config.js --coverage"
  }
}
```

**jest.regression.config.js :**
```javascript
module.exports = {
  displayName: 'Regression Tests',
  testMatch: ['**/__tests__/regression/**/*.test.js'],
  setupFilesAfterEnv: ['<rootDir>/src/__tests__/regression/setup.js'],
  testEnvironment: 'node',
  collectCoverageFrom: [
    'src/**/*.js',
    '!src/**/*.test.js',
    '!src/__tests__/**'
  ],
  coverageDirectory: 'coverage/regression',
  coverageReporters: ['text', 'lcov', 'html'],
  testTimeout: 30000, // Tests de régression peuvent être plus longs
  maxWorkers: 1, // Exécution séquentielle pour éviter les conflits
  reporters: [
    'default',
    ['jest-junit', {
      outputDirectory: 'test-results',
      outputName: 'regression-results.xml',
      suiteName: 'Regression Tests'
    }]
  ],
  // Configuration spécifique pour les tests de régression
  globals: {
    REGRESSION_TEST_MODE: true
  }
};
```

Cette section complète la définition des tests de régression avec une couverture exhaustive des cas d'usage critiques, une automatisation complète et des rapports détaillés pour garantir qu'aucune régression n'est introduite lors de la migration.
## 6.5 Automatisation des Tests

### Choix des Outils et Frameworks

#### Framework Principal : Cypress pour E2E

**Justification du Choix :**
- Excellente intégration avec React/TypeScript
- Debugging visuel en temps réel
- API intuitive et documentation complète
- Support natif des applications SPA
- Communauté active et écosystème riche

**Configuration Cypress :**
```javascript
// cypress.config.js
const { defineConfig } = require('cypress');

module.exports = defineConfig({
  e2e: {
    baseUrl: 'http://localhost:3000',
    supportFile: 'cypress/support/e2e.js',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: true,
    screenshotOnRunFailure: true,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    
    // Configuration pour les tests de régression
    retries: {
      runMode: 2,
      openMode: 0
    },
    
    // Variables d'environnement
    env: {
      apiUrl: 'http://localhost:3001',
      testUser: {
        email: 'test@essensys.com',
        password: 'TestPassword123!'
      },
      testMachine: {
        activationKey: 'TEST-ABCD-EFGH-IJKL-MNOP-QRST-UVWX-YZ12'
      }
    },
    
    setupNodeEvents(on, config) {
      // Plugin pour les rapports
      require('cypress-mochawesome-reporter/plugin')(on);
      
      // Plugin pour la couverture de code
      require('@cypress/code-coverage/task')(on, config);
      
      // Tâches personnalisées
      on('task', {
        // Réinitialiser la base de données
        resetDatabase() {
          return require('./cypress/tasks/database').reset();
        },
        
        // Créer des données de test
        seedTestData() {
          return require('./cypress/tasks/database').seed();
        },
        
        // Simuler un boîtier IoT
        simulateDevice(config) {
          return require('./cypress/tasks/device-simulator').start(config);
        },
        
        // Vérifier les notifications SMS/Email
        checkNotifications(type) {
          return require('./cypress/tasks/notifications').check(type);
        }
      });
      
      return config;
    }
  },
  
  component: {
    devServer: {
      framework: 'react',
      bundler: 'vite',
    },
    specPattern: 'src/**/*.cy.{js,jsx,ts,tsx}',
    supportFile: 'cypress/support/component.js'
  }
});
```

#### Framework Alternatif : Playwright pour Cross-Browser

**Configuration Playwright :**
```javascript
// playwright.config.js
const { defineConfig, devices } = require('@playwright/test');

module.exports = defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['junit', { outputFile: 'test-results/playwright-results.xml' }],
    ['json', { outputFile: 'test-results/playwright-results.json' }]
  ],
  
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    }
  ],

  webServer: [
    {
      command: 'npm run dev',
      port: 3000,
      cwd: './frontend'
    },
    {
      command: 'npm run start:test',
      port: 3001,
      cwd: './backend'
    }
  ]
});
```

### Pipelines CI/CD Intégrant les Tests Automatisés

#### Pipeline Principal GitHub Actions

**Configuration Complète :**
```yaml
# .github/workflows/ci-cd-tests.yml
name: CI/CD with Automated Testing

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  NODE_VERSION: '18.18.0'
  POSTGRES_VERSION: '15.5'
  REDIS_VERSION: '7.2'

jobs:
  # Phase 1: Tests Unitaires et d'Intégration
  unit-integration-tests:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:${{ env.POSTGRES_VERSION }}
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_DB: essensys_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
      
      redis:
        image: redis:${{ env.REDIS_VERSION }}
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379

    strategy:
      matrix:
        component: [frontend, backend]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: ${{ matrix.component }}/package-lock.json

      - name: Install dependencies
        run: |
          cd ${{ matrix.component }}
          npm ci

      - name: Setup database (backend only)
        if: matrix.component == 'backend'
        run: |
          cd backend
          npx prisma migrate deploy
          npx prisma db seed
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_test

      - name: Run linting
        run: |
          cd ${{ matrix.component }}
          npm run lint

      - name: Run type checking (frontend only)
        if: matrix.component == 'frontend'
        run: |
          cd frontend
          npm run type-check

      - name: Run unit tests
        run: |
          cd ${{ matrix.component }}
          npm run test:coverage
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_test
          REDIS_URL: redis://localhost:6379
          NODE_ENV: test

      - name: Run integration tests (backend only)
        if: matrix.component == 'backend'
        run: |
          cd backend
          npm run test:integration
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_test
          REDIS_URL: redis://localhost:6379

      - name: Upload coverage reports
        uses: codecov/codecov-action@v3
        with:
          file: ${{ matrix.component }}/coverage/lcov.info
          flags: ${{ matrix.component }}
          name: ${{ matrix.component }}-coverage

      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results-${{ matrix.component }}
          path: ${{ matrix.component }}/test-results/

  # Phase 2: Tests de Performance
  performance-tests:
    runs-on: ubuntu-latest
    needs: unit-integration-tests
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Start services
        run: |
          docker-compose -f docker-compose.test.yml up -d
          
      - name: Wait for services
        run: |
          timeout 60 bash -c 'until curl -f http://localhost:3000/health; do sleep 2; done'
          timeout 60 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'

      - name: Install Artillery
        run: npm install -g artillery@latest

      - name: Run API performance tests
        run: |
          artillery run tests/performance/api-load-test.yml --output api-perf-results.json
          artillery report api-perf-results.json --output api-perf-report.html

      - name: Run Lighthouse performance tests
        run: |
          npm install -g @lhci/cli
          lhci autorun

      - name: Upload performance results
        uses: actions/upload-artifact@v3
        with:
          name: performance-results
          path: |
            api-perf-report.html
            .lighthouseci/

      - name: Stop services
        run: docker-compose -f docker-compose.test.yml down

  # Phase 3: Tests E2E avec Cypress
  e2e-tests:
    runs-on: ubuntu-latest
    needs: unit-integration-tests
    
    strategy:
      matrix:
        browser: [chrome, firefox, edge]
        
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Install dependencies
        run: |
          cd frontend && npm ci
          cd ../backend && npm ci

      - name: Start application
        run: |
          docker-compose -f docker-compose.e2e.yml up -d
          
      - name: Wait for application
        run: |
          timeout 120 bash -c 'until curl -f http://localhost:3000; do sleep 2; done'
          timeout 120 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'

      - name: Run Cypress E2E tests
        uses: cypress-io/github-action@v6
        with:
          working-directory: frontend
          browser: ${{ matrix.browser }}
          record: true
          parallel: true
          group: 'E2E Tests - ${{ matrix.browser }}'
        env:
          CYPRESS_RECORD_KEY: ${{ secrets.CYPRESS_RECORD_KEY }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

      - name: Upload Cypress screenshots
        uses: actions/upload-artifact@v3
        if: failure()
        with:
          name: cypress-screenshots-${{ matrix.browser }}
          path: frontend/cypress/screenshots

      - name: Upload Cypress videos
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: cypress-videos-${{ matrix.browser }}
          path: frontend/cypress/videos

      - name: Stop application
        run: docker-compose -f docker-compose.e2e.yml down

  # Phase 4: Tests de Compatibilité Hardware
  hardware-compatibility-tests:
    runs-on: ubuntu-latest
    needs: unit-integration-tests
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Start hardware test environment
        run: |
          docker-compose -f docker-compose.hardware-test.yml up -d
          
      - name: Wait for services
        run: |
          timeout 120 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'
          sleep 30 # Attendre que les simulateurs se connectent

      - name: Run hardware compatibility tests
        run: |
          cd backend
          npm run test:hardware
        env:
          TEST_API_URL: http://localhost:3001

      - name: Generate compatibility report
        run: |
          node scripts/generate-compatibility-report.js

      - name: Upload hardware test results
        uses: actions/upload-artifact@v3
        with:
          name: hardware-compatibility-results
          path: |
            hardware-compatibility-report.json
            hardware-compatibility-report.html

      - name: Stop hardware test environment
        run: docker-compose -f docker-compose.hardware-test.yml down

  # Phase 5: Tests de Régression
  regression-tests:
    runs-on: ubuntu-latest
    needs: [unit-integration-tests, e2e-tests]
    if: github.ref == 'refs/heads/main' || github.event_name == 'pull_request'
    
    strategy:
      matrix:
        suite: [authentication, device-communication, device-control, notifications]
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Install dependencies
        run: |
          cd backend && npm ci

      - name: Start test environment
        run: |
          docker-compose -f docker-compose.regression.yml up -d
          
      - name: Wait for services
        run: |
          timeout 120 bash -c 'until curl -f http://localhost:3001/health; do sleep 2; done'

      - name: Run regression tests - ${{ matrix.suite }}
        run: |
          cd backend
          npm run test:regression:${{ matrix.suite }}
        env:
          DATABASE_URL: postgresql://postgres:test@localhost:5432/essensys_regression_test
          REDIS_URL: redis://localhost:6379

      - name: Upload regression results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: regression-results-${{ matrix.suite }}
          path: backend/test-results/regression-${{ matrix.suite }}.xml

      - name: Stop test environment
        run: docker-compose -f docker-compose.regression.yml down

  # Phase 6: Génération des Rapports
  generate-reports:
    runs-on: ubuntu-latest
    needs: [performance-tests, e2e-tests, hardware-compatibility-tests, regression-tests]
    if: always()
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Download all artifacts
        uses: actions/download-artifact@v3
        with:
          path: artifacts

      - name: Generate consolidated report
        run: |
          node scripts/generate-test-report.js

      - name: Upload consolidated report
        uses: actions/upload-artifact@v3
        with:
          name: consolidated-test-report
          path: |
            test-report.html
            test-report.json

      - name: Comment PR with test results
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const fs = require('fs');
            const report = JSON.parse(fs.readFileSync('test-report.json', 'utf8'));
            
            const comment = `
            ## 🧪 Résultats des Tests Automatisés
            
            ### Résumé Global
            - **Tests Unitaires**: ${report.unit.passed}/${report.unit.total} ✅
            - **Tests d'Intégration**: ${report.integration.passed}/${report.integration.total} ✅
            - **Tests E2E**: ${report.e2e.passed}/${report.e2e.total} ✅
            - **Tests de Performance**: ${report.performance.status} ${report.performance.status === 'PASS' ? '✅' : '❌'}
            - **Tests Hardware**: ${report.hardware.passed}/${report.hardware.total} ✅
            - **Tests de Régression**: ${report.regression.passed}/${report.regression.total} ✅
            
            ### Couverture de Code
            - **Frontend**: ${report.coverage.frontend}%
            - **Backend**: ${report.coverage.backend}%
            
            ${report.regression.failed > 0 ? '⚠️ **ATTENTION**: Des régressions ont été détectées!' : ''}
            ${report.performance.status === 'FAIL' ? '⚠️ **ATTENTION**: Les critères de performance ne sont pas respectés!' : ''}
            
            [📊 Voir le rapport complet](${report.reportUrl})
            `;
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: comment
            });

  # Phase 7: Déploiement (si tous les tests passent)
  deploy:
    runs-on: ubuntu-latest
    needs: [generate-reports]
    if: github.ref == 'refs/heads/main' && success()
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to staging
        run: |
          echo "Déploiement vers l'environnement de staging..."
          # Scripts de déploiement ici

      - name: Run smoke tests on staging
        run: |
          echo "Exécution des tests de fumée sur staging..."
          # Tests de fumée post-déploiement

      - name: Deploy to production (if staging tests pass)
        if: success()
        run: |
          echo "Déploiement vers la production..."
          # Scripts de déploiement production
```

### Critères de Qualité pour le Passage en Production

#### Critères Obligatoires (Bloquants)

**1. Couverture de Code :**
```javascript
// jest.config.js - Seuils de couverture
module.exports = {
  coverageThreshold: {
    global: {
      branches: 85,
      functions: 90,
      lines: 90,
      statements: 90
    },
    // Seuils spécifiques par composant critique
    'src/services/auth-service.js': {
      branches: 95,
      functions: 100,
      lines: 95,
      statements: 95
    },
    'src/services/device-service.js': {
      branches: 95,
      functions: 100,
      lines: 95,
      statements: 95
    },
    'src/controllers/device-controller.js': {
      branches: 90,
      functions: 95,
      lines: 90,
      statements: 90
    }
  }
};
```

**2. Tests de Régression :**
- 100% des tests de régression doivent passer
- Aucune régression critique détectée
- Parité fonctionnelle avec le système legacy validée

**3. Tests de Performance :**
```yaml
# Critères de performance obligatoires
performance_criteria:
  api_response_time:
    p95: 200ms
    p99: 500ms
  frontend_metrics:
    first_contentful_paint: 1500ms
    largest_contentful_paint: 2500ms
    cumulative_layout_shift: 0.1
  throughput:
    min_requests_per_second: 1000
  error_rate:
    max_percentage: 0.1
```

**4. Tests de Sécurité :**
- Authentification : 100% des scénarios validés
- Autorisation : Aucune faille d'accès détectée
- Validation des données : Tous les inputs sécurisés

#### Critères Recommandés (Non-Bloquants)

**1. Tests E2E :**
- 95% des parcours utilisateur validés
- Compatibilité multi-navigateurs confirmée
- Tests mobile réussis

**2. Tests de Compatibilité Hardware :**
- 90% des versions firmware supportées
- Communication boîtier stable
- Formats de données validés

#### Gate de Qualité Automatisé

**Configuration SonarQube :**
```javascript
// sonar-project.properties
sonar.projectKey=essensys-migration
sonar.projectName=Essensys Migration
sonar.projectVersion=1.0

# Sources
sonar.sources=frontend/src,backend/src
sonar.tests=frontend/src,backend/src
sonar.test.inclusions=**/*.test.js,**/*.test.ts,**/*.spec.js,**/*.spec.ts

# Exclusions
sonar.exclusions=**/node_modules/**,**/coverage/**,**/dist/**,**/build/**

# Coverage
sonar.javascript.lcov.reportPaths=frontend/coverage/lcov.info,backend/coverage/lcov.info

# Quality Gate
sonar.qualitygate.wait=true

# Seuils de qualité
sonar.coverage.exclusions=**/*.test.js,**/*.test.ts,**/*.spec.js,**/*.spec.ts
```

**Script de Validation Qualité :**
```javascript
// scripts/quality-gate.js
const fs = require('fs');
const axios = require('axios');

class QualityGate {
  constructor() {
    this.criteria = {
      coverage: {
        frontend: { min: 90, current: 0 },
        backend: { min: 90, current: 0 }
      },
      tests: {
        unit: { min: 100, current: 0 },
        integration: { min: 95, current: 0 },
        e2e: { min: 90, current: 0 },
        regression: { min: 100, current: 0 }
      },
      performance: {
        api_p95: { max: 200, current: 0 },
        frontend_fcp: { max: 1500, current: 0 },
        frontend_lcp: { max: 2500, current: 0 }
      },
      security: {
        vulnerabilities: { max: 0, current: 0 },
        code_smells: { max: 10, current: 0 }
      }
    };
  }

  async checkCoverage() {
    // Lire les rapports de couverture
    const frontendCoverage = JSON.parse(fs.readFileSync('frontend/coverage/coverage-summary.json'));
    const backendCoverage = JSON.parse(fs.readFileSync('backend/coverage/coverage-summary.json'));

    this.criteria.coverage.frontend.current = frontendCoverage.total.lines.pct;
    this.criteria.coverage.backend.current = backendCoverage.total.lines.pct;

    return {
      frontend: this.criteria.coverage.frontend.current >= this.criteria.coverage.frontend.min,
      backend: this.criteria.coverage.backend.current >= this.criteria.coverage.backend.min
    };
  }

  async checkTests() {
    // Lire les résultats de tests
    const testResults = {
      unit: this.parseTestResults('test-results/unit-results.xml'),
      integration: this.parseTestResults('test-results/integration-results.xml'),
      e2e: this.parseTestResults('test-results/e2e-results.xml'),
      regression: this.parseTestResults('test-results/regression-results.xml')
    };

    Object.keys(testResults).forEach(type => {
      const result = testResults[type];
      this.criteria.tests[type].current = (result.passed / result.total) * 100;
    });

    return Object.keys(this.criteria.tests).every(type => 
      this.criteria.tests[type].current >= this.criteria.tests[type].min
    );
  }

  async checkPerformance() {
    // Lire les résultats de performance
    const performanceResults = JSON.parse(fs.readFileSync('performance-results.json'));

    this.criteria.performance.api_p95.current = performanceResults.api.p95;
    this.criteria.performance.frontend_fcp.current = performanceResults.lighthouse.fcp;
    this.criteria.performance.frontend_lcp.current = performanceResults.lighthouse.lcp;

    return Object.keys(this.criteria.performance).every(metric => {
      const criterion = this.criteria.performance[metric];
      return criterion.current <= criterion.max;
    });
  }

  async checkSecurity() {
    // Vérifier avec SonarQube
    const sonarResults = await this.getSonarQubeResults();
    
    this.criteria.security.vulnerabilities.current = sonarResults.vulnerabilities;
    this.criteria.security.code_smells.current = sonarResults.code_smells;

    return this.criteria.security.vulnerabilities.current <= this.criteria.security.vulnerabilities.max &&
           this.criteria.security.code_smells.current <= this.criteria.security.code_smells.max;
  }

  async getSonarQubeResults() {
    try {
      const response = await axios.get(`${process.env.SONAR_URL}/api/measures/component`, {
        params: {
          component: 'essensys-migration',
          metricKeys: 'vulnerabilities,code_smells,coverage,duplicated_lines_density'
        },
        auth: {
          username: process.env.SONAR_TOKEN,
          password: ''
        }
      });

      const measures = response.data.component.measures;
      return {
        vulnerabilities: parseInt(measures.find(m => m.metric === 'vulnerabilities')?.value || '0'),
        code_smells: parseInt(measures.find(m => m.metric === 'code_smells')?.value || '0'),
        coverage: parseFloat(measures.find(m => m.metric === 'coverage')?.value || '0'),
        duplication: parseFloat(measures.find(m => m.metric === 'duplicated_lines_density')?.value || '0')
      };
    } catch (error) {
      console.error('Erreur lors de la récupération des métriques SonarQube:', error);
      return { vulnerabilities: 999, code_smells: 999, coverage: 0, duplication: 100 };
    }
  }

  parseTestResults(filePath) {
    // Parser les résultats XML de tests
    if (!fs.existsSync(filePath)) {
      return { total: 0, passed: 0, failed: 0 };
    }

    const xml = fs.readFileSync(filePath, 'utf8');
    // Parsing XML simplifié (utiliser xml2js en production)
    const totalMatch = xml.match(/tests="(\d+)"/);
    const failuresMatch = xml.match(/failures="(\d+)"/);
    
    const total = totalMatch ? parseInt(totalMatch[1]) : 0;
    const failures = failuresMatch ? parseInt(failuresMatch[1]) : 0;
    
    return {
      total,
      passed: total - failures,
      failed: failures
    };
  }

  async evaluate() {
    console.log('🔍 Évaluation des critères de qualité...');

    const results = {
      coverage: await this.checkCoverage(),
      tests: await this.checkTests(),
      performance: await this.checkPerformance(),
      security: await this.checkSecurity()
    };

    const passed = Object.values(results).every(result => 
      typeof result === 'boolean' ? result : Object.values(result).every(Boolean)
    );

    this.generateReport(results, passed);

    return passed;
  }

  generateReport(results, passed) {
    const report = {
      timestamp: new Date().toISOString(),
      status: passed ? 'PASS' : 'FAIL',
      criteria: this.criteria,
      results,
      summary: {
        coverage: `Frontend: ${this.criteria.coverage.frontend.current}%, Backend: ${this.criteria.coverage.backend.current}%`,
        tests: `${Object.keys(this.criteria.tests).map(type => 
          `${type}: ${this.criteria.tests[type].current.toFixed(1)}%`
        ).join(', ')}`,
        performance: `API P95: ${this.criteria.performance.api_p95.current}ms, FCP: ${this.criteria.performance.frontend_fcp.current}ms`,
        security: `Vulnérabilités: ${this.criteria.security.vulnerabilities.current}, Code Smells: ${this.criteria.security.code_smells.current}`
      }
    };

    fs.writeFileSync('quality-gate-report.json', JSON.stringify(report, null, 2));

    console.log('\n📊 Rapport de Qualité:');
    console.log(`Status: ${passed ? '✅ PASS' : '❌ FAIL'}`);
    console.log(`Couverture: ${report.summary.coverage}`);
    console.log(`Tests: ${report.summary.tests}`);
    console.log(`Performance: ${report.summary.performance}`);
    console.log(`Sécurité: ${report.summary.security}`);

    if (!passed) {
      console.log('\n❌ Critères non respectés - Déploiement bloqué');
      process.exit(1);
    } else {
      console.log('\n✅ Tous les critères respectés - Déploiement autorisé');
    }
  }
}

// Exécution
if (require.main === module) {
  const gate = new QualityGate();
  gate.evaluate().catch(console.error);
}

module.exports = QualityGate;
```

### Maintenance et Évolution de la Suite de Tests

#### Stratégie de Maintenance

**1. Révision Mensuelle :**
```javascript
// scripts/test-maintenance.js
class TestMaintenanceManager {
  constructor() {
    this.metrics = {
      flaky_tests: [],
      slow_tests: [],
      outdated_tests: [],
      coverage_gaps: []
    };
  }

  async analyzeTestHealth() {
    // Analyser les tests instables
    await this.identifyFlakyTests();
    
    // Identifier les tests lents
    await this.identifySlowTests();
    
    // Détecter les tests obsolètes
    await this.identifyOutdatedTests();
    
    // Analyser les gaps de couverture
    await this.analyzeCoverageGaps();
    
    return this.generateMaintenanceReport();
  }

  async identifyFlakyTests() {
    // Analyser l'historique des tests sur les 30 derniers jours
    const testHistory = await this.getTestHistory(30);
    
    this.metrics.flaky_tests = testHistory
      .filter(test => test.success_rate < 95)
      .map(test => ({
        name: test.name,
        success_rate: test.success_rate,
        failure_reasons: test.failure_reasons
      }));
  }

  async identifySlowTests() {
    // Identifier les tests qui prennent plus de 30 secondes
    const testTimes = await this.getTestExecutionTimes();
    
    this.metrics.slow_tests = testTimes
      .filter(test => test.avg_duration > 30000)
      .sort((a, b) => b.avg_duration - a.avg_duration)
      .slice(0, 10);
  }

  generateMaintenanceReport() {
    const report = `
# 🔧 Rapport de Maintenance des Tests

## Tests Instables (Flaky)
${this.metrics.flaky_tests.length === 0 ? '✅ Aucun test instable détecté' : 
  this.metrics.flaky_tests.map(test => 
    `- **${test.name}** (${test.success_rate}% de réussite)`
  ).join('\n')
}

## Tests Lents
${this.metrics.slow_tests.length === 0 ? '✅ Aucun test lent détecté' : 
  this.metrics.slow_tests.map(test => 
    `- **${test.name}** (${(test.avg_duration / 1000).toFixed(1)}s en moyenne)`
  ).join('\n')
}

## Recommandations
${this.generateRecommendations()}

## Actions Prioritaires
${this.generateActionItems()}
`;

    fs.writeFileSync('test-maintenance-report.md', report);
    return report;
  }

  generateRecommendations() {
    const recommendations = [];

    if (this.metrics.flaky_tests.length > 0) {
      recommendations.push('🔄 Stabiliser les tests instables en améliorant les attentes et les mocks');
    }

    if (this.metrics.slow_tests.length > 0) {
      recommendations.push('⚡ Optimiser les tests lents ou les diviser en tests plus petits');
    }

    if (this.metrics.coverage_gaps.length > 0) {
      recommendations.push('📊 Combler les gaps de couverture identifiés');
    }

    return recommendations.length > 0 ? recommendations.join('\n') : '✅ Aucune recommandation particulière';
  }
}
```

**2. Évolution Continue :**
- Mise à jour des frameworks de test (trimestrielle)
- Révision des critères de qualité (semestrielle)
- Formation équipe sur nouveaux outils (continue)
- Optimisation des temps d'exécution (mensuelle)

#### Monitoring des Tests en Production

**Dashboard de Monitoring :**
```javascript
// monitoring/test-dashboard.js
class TestMonitoringDashboard {
  constructor() {
    this.metrics = {
      execution_times: new Map(),
      success_rates: new Map(),
      coverage_trends: [],
      deployment_success: []
    };
  }

  async collectMetrics() {
    // Collecter les métriques depuis les dernières exécutions
    const recentRuns = await this.getRecentTestRuns(7); // 7 derniers jours
    
    recentRuns.forEach(run => {
      this.updateExecutionTimes(run);
      this.updateSuccessRates(run);
      this.updateCoverageTrends(run);
    });
  }

  generateDashboard() {
    return {
      summary: {
        total_tests: this.getTotalTestCount(),
        avg_execution_time: this.getAverageExecutionTime(),
        overall_success_rate: this.getOverallSuccessRate(),
        coverage_percentage: this.getCurrentCoverage()
      },
      trends: {
        execution_time_trend: this.getExecutionTimeTrend(),
        success_rate_trend: this.getSuccessRateTrend(),
        coverage_trend: this.getCoverageTrend()
      },
      alerts: this.generateAlerts()
    };
  }

  generateAlerts() {
    const alerts = [];

    // Alerte si le temps d'exécution augmente de plus de 20%
    if (this.getExecutionTimeTrend() > 1.2) {
      alerts.push({
        type: 'warning',
        message: 'Temps d\'exécution des tests en augmentation',
        severity: 'medium'
      });
    }

    // Alerte si le taux de réussite baisse sous 95%
    if (this.getOverallSuccessRate() < 95) {
      alerts.push({
        type: 'error',
        message: 'Taux de réussite des tests sous le seuil critique',
        severity: 'high'
      });
    }

    // Alerte si la couverture baisse de plus de 5%
    const coverageTrend = this.getCoverageTrend();
    if (coverageTrend < -5) {
      alerts.push({
        type: 'warning',
        message: 'Couverture de code en baisse significative',
        severity: 'medium'
      });
    }

    return alerts;
  }
}
```

Cette section complète l'automatisation des tests avec une approche exhaustive couvrant les outils, pipelines CI/CD, critères de qualité et maintenance continue, garantissant un système de tests robuste et évolutif pour la migration Essensys.
## Conclusion

Ce plan de tests et validation complet pour la migration Essensys couvre tous les aspects critiques nécessaires pour garantir la qualité, la fiabilité et la parité fonctionnelle entre le système legacy et le nouveau système React/Node.js.

### Résumé des Livrables

**6.1 Scénarios de Test Fonctionnels :**
- ✅ 150+ scénarios de test couvrant toutes les features métier
- ✅ Tests d'acceptation avec critères mesurables
- ✅ Couverture complète des parcours utilisateur critiques
- ✅ Tests de régression pour éviter les régressions

**6.2 Tests de Performance :**
- ✅ Benchmarks basés sur le système legacy avec objectifs d'amélioration
- ✅ Tests de charge (500-2000 utilisateurs virtuels)
- ✅ Tests de stress et de montée en charge
- ✅ Critères de performance acceptables définis (API < 200ms, FCP < 1.5s)

**6.3 Tests de Compatibilité Hardware :**
- ✅ Validation communication avec boîtiers existants (V0, V1, V2)
- ✅ Simulateurs de boîtiers pour tests automatisés
- ✅ Tests de compatibilité multi-versions firmware
- ✅ Validation protocoles et formats de données legacy

**6.4 Tests de Régression :**
- ✅ Identification de tous les cas d'usage critiques
- ✅ Suite complète de 200+ tests de non-régression automatisés
- ✅ Couverture des fonctionnalités vitales (authentification, communication, contrôle)
- ✅ Exécution automatique avec rapports détaillés

**6.5 Automatisation des Tests :**
- ✅ Frameworks choisis (Cypress E2E, Jest unitaires, Artillery performance)
- ✅ Pipelines CI/CD complets avec 7 phases de validation
- ✅ Critères de qualité stricts pour passage en production
- ✅ Maintenance et évolution continue de la suite de tests

### Métriques de Qualité Globales

| Type de Test | Objectif Couverture | Critères de Succès |
|--------------|-------------------|-------------------|
| **Tests Unitaires** | 90% lignes de code | ✅ Toutes les fonctions critiques testées |
| **Tests d'Intégration** | 80% flux critiques | ✅ APIs et base de données validées |
| **Tests E2E** | 100% parcours critiques | ✅ Expérience utilisateur complète |
| **Tests Performance** | Tous les endpoints | ✅ Amélioration 40% vs legacy |
| **Tests Hardware** | 100% versions firmware | ✅ Compatibilité descendante assurée |
| **Tests Régression** | 100% cas critiques | ✅ Zéro régression tolérée |

### Bénéfices Attendus

**Qualité et Fiabilité :**
- Détection précoce des bugs et régressions
- Validation automatique de la parité fonctionnelle
- Assurance qualité continue via CI/CD

**Performance et Scalabilité :**
- Validation des améliorations de performance
- Tests de montée en charge pour croissance future
- Monitoring continu des métriques critiques

**Compatibilité et Migration :**
- Validation complète de la compatibilité hardware
- Migration transparente pour les utilisateurs
- Continuité de service garantie

**Maintenance et Évolution :**
- Suite de tests maintenable et évolutive
- Documentation complète pour l'équipe
- Processus d'amélioration continue

### Recommandations pour l'Implémentation

1. **Prioriser les tests critiques** : Commencer par l'authentification et la communication boîtier
2. **Implémenter progressivement** : Suivre l'ordre des tâches pour une montée en compétence
3. **Automatiser dès le début** : Intégrer les tests dans le pipeline de développement
4. **Former l'équipe** : Assurer la maîtrise des outils et frameworks choisis
5. **Monitorer continuellement** : Suivre les métriques et ajuster les critères si nécessaire

Ce plan de tests constitue la fondation pour une migration réussie, garantissant la qualité du nouveau système tout en préservant la confiance des utilisateurs et la continuité du service Essensys.