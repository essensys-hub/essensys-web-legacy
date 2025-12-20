# Configuration GitHub Pages - Guide Étape par Étape

## 🎯 Objectif

Activer GitHub Pages pour publier automatiquement la documentation Essensys Migration.

## 📋 Prérequis

- ✅ Repository GitHub avec le code poussé
- ✅ Workflows GitHub Actions configurés
- ✅ Permissions d'administration sur le repository

## 🚀 Étapes d'Activation

### Étape 1: Accéder aux Paramètres

1. **Allez sur votre repository GitHub**
   ```
   https://github.com/essensys-hub/essensys-web-legacy
   ```

2. **Cliquez sur l'onglet "Settings"** (en haut à droite)

### Étape 2: Configurer GitHub Pages

1. **Dans le menu de gauche**, cliquez sur **"Pages"**

2. **Source Configuration** :
   - **Source** : Sélectionnez **"Deploy from a branch"**
   - **Branch** : Sélectionnez **"gh-pages"**
   - **Folder** : Laissez **"/ (root)"**

3. **Cliquez sur "Save"**

### Étape 3: Vérifier l'Activation

1. **Attendez quelques minutes** pour que GitHub traite la configuration

2. **Rafraîchissez la page Settings → Pages**

3. **Vous devriez voir** :
   ```
   ✅ Your site is published at https://essensys-hub.github.io/essensys-web-legacy/
   ```

## 🔄 Workflow Automatique

Une fois GitHub Pages activé, le workflow se déclenchera automatiquement :

### Déclencheurs
- ✅ **Push sur main/master**
- ✅ **Modifications dans docs/**
- ✅ **Modifications de mkdocs.yml**
- ✅ **Déclenchement manuel**

### Processus
1. **Build** : Construction de la documentation avec MkDocs
2. **Deploy** : Publication sur la branche `gh-pages`
3. **Publish** : Mise en ligne sur GitHub Pages

## 🌐 Accès à la Documentation

### URL de Production
```
https://essensys-hub.github.io/essensys-web-legacy/
```

### Fonctionnalités Disponibles
- 📚 **Navigation complète** par catégories
- 🔍 **Recherche intégrée** avec suggestions
- 📱 **Design responsive** (desktop/mobile)
- 🌙 **Mode sombre/clair** automatique
- 🔗 **Liens internes** optimisés

## 🛠️ Dépannage

### Problème : "Pages not found"

**Solution** :
1. Vérifiez que la branche `gh-pages` existe
2. Attendez 5-10 minutes après la première activation
3. Vérifiez les logs dans Actions → Deploy Documentation

### Problème : "Build failed"

**Solution** :
1. Allez dans **Actions** → **Deploy Documentation**
2. Cliquez sur le workflow qui a échoué
3. Examinez les logs d'erreur
4. Corrigez le problème et poussez un nouveau commit

### Problème : "Permission denied"

**Solution** :
1. Vérifiez que vous avez les droits d'admin sur le repo
2. Dans Settings → Actions → General
3. Workflow permissions → "Read and write permissions"
4. Cochez "Allow GitHub Actions to create and approve pull requests"

## 📊 Monitoring

### Vérifier le Statut
1. **Actions Tab** : Voir l'historique des déploiements
2. **Pages Settings** : Voir l'URL et le statut
3. **Commits** : Voir les déploiements automatiques

### Métriques Disponibles
- ✅ **Temps de build** : ~2-3 minutes
- ✅ **Taille du site** : ~5-10 MB
- ✅ **Nombre de pages** : ~30 documents
- ✅ **Mise à jour** : Automatique à chaque push

## 🔧 Configuration Avancée

### Custom Domain (Optionnel)

Si vous avez un domaine personnalisé :

1. **Dans Pages Settings**
2. **Custom domain** : Entrez votre domaine
3. **Créez un fichier CNAME** dans le repository :
   ```bash
   echo "docs.essensys.com" > docs/CNAME
   git add docs/CNAME
   git commit -m "Add custom domain"
   git push
   ```

### Analytics (Optionnel)

Pour ajouter Google Analytics :

1. **Modifiez mkdocs.yml** :
   ```yaml
   extra:
     analytics:
       provider: google
       property: G-XXXXXXXXXX  # Votre ID
   ```

2. **Commitez et poussez** :
   ```bash
   git add mkdocs.yml
   git commit -m "Add Google Analytics"
   git push
   ```

## ✅ Checklist de Validation

- [ ] Repository accessible sur GitHub
- [ ] Settings → Pages configuré
- [ ] Branche gh-pages créée automatiquement
- [ ] Workflow "Deploy Documentation" réussi
- [ ] Site accessible sur l'URL GitHub Pages
- [ ] Navigation et recherche fonctionnelles
- [ ] Design responsive sur mobile
- [ ] Tous les documents visibles

## 📞 Support

### En cas de problème :

1. **Vérifiez les Actions** : Logs détaillés des erreurs
2. **Consultez la documentation** : [GitHub Pages Docs](https://docs.github.com/en/pages)
3. **Testez localement** : `mkdocs serve` pour déboguer
4. **Workflow manuel** : Actions → Deploy Documentation → Run workflow

---

*Guide mis à jour le 20 décembre 2024*  
*Compatible avec GitHub Actions et MkDocs Material*