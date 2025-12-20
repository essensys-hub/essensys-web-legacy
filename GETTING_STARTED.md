# Guide de Démarrage - Documentation Essensys Migration

## 🚀 Démarrage Rapide

### 1. Installation des Dépendances

```bash
# Installer les dépendances Python pour MkDocs
pip install -r requirements.txt

# Ou avec un environnement virtuel (recommandé)
python -m venv venv
source venv/bin/activate  # Linux/Mac
# ou
venv\Scripts\activate     # Windows
pip install -r requirements.txt
```

### 2. Lancer la Documentation Localement

```bash
# Méthode 1: Avec le script de déploiement
./deploy-docs.sh local

# Méthode 2: Directement avec MkDocs
mkdocs serve

# La documentation sera disponible sur http://127.0.0.1:8000
```

### 3. Construire la Documentation

```bash
# Méthode 1: Avec le script
./deploy-docs.sh build

# Méthode 2: Directement avec MkDocs
mkdocs build

# Les fichiers statiques seront dans le dossier site/
```

## 📚 Structure de la Documentation

```
docs/
├── README.md                           # Index principal
├── resume-executif-validation.md       # 🎯 Résumé pour décideurs
├── validation-finale-plan-migration.md # ✅ Validation complète
├── legacy-system-analysis.md           # 🔍 Analyse du legacy
├── architecture-moderne-*.md           # 🏗️ Architecture moderne
├── planification-estimation-projet.md  # 📊 Planning et estimation
├── standards-developpement.md          # 🛠️ Guide développement
├── plan-tests-validation.md            # 🧪 Tests et validation
├── plan-formation-utilisateurs.md      # 👥 Formation utilisateurs
└── ...
```

## 🎯 Navigation Rapide

### Pour les **Décideurs** 👔
1. [Résumé Exécutif](docs/resume-executif-validation.md) - Vue d'ensemble
2. [Analyse ROI](docs/analyse-cout-benefice-roi.md) - Justification économique
3. [Validation Finale](docs/validation-finale-plan-migration.md) - Approbation

### Pour les **Architectes** 🏗️
1. [Analyse Legacy](docs/legacy-system-analysis.md) - État actuel
2. [Architecture Frontend](docs/architecture-moderne-frontend.md) - React/TypeScript
3. [Architecture Backend](docs/architecture-moderne-backend.md) - Node.js/Express
4. [Base de Données](docs/schema-base-donnees-moderne.md) - PostgreSQL

### Pour les **Chefs de Projet** 📊
1. [Planification](docs/planification-estimation-projet.md) - Roadmap complète
2. [Migration Données](docs/strategie-migration-donnees.md) - Stratégie données
3. [Formation](docs/plan-formation-utilisateurs.md) - Accompagnement utilisateurs

### Pour les **Développeurs** 💻
1. [Standards](docs/standards-developpement.md) - Conventions de code
2. [Stack Technique](docs/stack-technique-outils.md) - Technologies utilisées
3. [Exemples](docs/exemples-implementation.md) - Code d'exemple
4. [API](docs/specifications-api.md) - Documentation API

## 🔧 Configuration MkDocs

Le fichier `mkdocs.yml` contient la configuration complète :

- **Thème** : Material Design avec mode sombre/clair
- **Navigation** : Organisée par catégories
- **Plugins** : Recherche, dates de révision, minification
- **Extensions** : Mermaid, MathJax, syntax highlighting

## 🚀 Déploiement sur GitHub Pages

### Automatique (Recommandé)

Le déploiement se fait automatiquement via GitHub Actions :

1. **Push sur main/master** → Déploiement automatique
2. **Pull Request** → Build de test
3. **Disponible sur** : `https://votre-username.github.io/essensys-migration/`

### Manuel

```bash
# Déployer manuellement
./deploy-docs.sh deploy

# Ou directement avec MkDocs
mkdocs gh-deploy
```

## 📝 Modification de la Documentation

### Ajouter un Nouveau Document

1. **Créer le fichier** dans `docs/nouveau-document.md`
2. **Ajouter à la navigation** dans `mkdocs.yml` :
   ```yaml
   nav:
     - Nouvelle Section:
       - Nouveau Document: nouveau-document.md
   ```
3. **Mettre à jour l'index** dans `docs/README.md`

### Modifier un Document Existant

1. **Éditer le fichier** dans `docs/`
2. **Tester localement** : `./deploy-docs.sh local`
3. **Commiter et pusher** → Déploiement automatique

## 🎨 Personnalisation

### Styles CSS

Modifier `docs/stylesheets/extra.css` pour personnaliser l'apparence :

```css
:root {
  --essensys-primary: #1976d2;
  --essensys-secondary: #424242;
}
```

### JavaScript

Ajouter des fonctionnalités dans `docs/javascripts/` :

```javascript
// Exemple: Analytics personnalisés
document$.subscribe(() => {
  // Code exécuté à chaque changement de page
});
```

## 🔍 Recherche et Navigation

### Recherche Intégrée
- **Raccourci** : `Ctrl+K` ou `/`
- **Suggestions** : Recherche intelligente
- **Highlighting** : Mise en surbrillance des résultats

### Navigation
- **Tabs** : Navigation par catégories
- **Sidebar** : Navigation détaillée
- **Breadcrumbs** : Fil d'Ariane
- **Table des matières** : TOC automatique

## 📱 Responsive Design

La documentation est optimisée pour :
- **Desktop** : Navigation complète
- **Tablet** : Navigation adaptée
- **Mobile** : Menu hamburger

## 🛠️ Dépannage

### Problèmes Courants

**MkDocs non trouvé** :
```bash
pip install mkdocs-material
```

**Erreur de build** :
```bash
mkdocs build --verbose
```

**Port déjà utilisé** :
```bash
mkdocs serve --dev-addr=127.0.0.1:8001
```

### Logs et Debug

```bash
# Mode verbose
mkdocs serve --verbose

# Vérifier la configuration
mkdocs config

# Valider les liens
mkdocs build --strict
```

## 📞 Support

- **Documentation MkDocs** : [mkdocs.org](https://www.mkdocs.org/)
- **Thème Material** : [squidfunk.github.io/mkdocs-material](https://squidfunk.github.io/mkdocs-material/)
- **Issues GitHub** : Pour signaler des problèmes

---

*Guide mis à jour le 20 décembre 2024*