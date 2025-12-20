Rôle : Agis en tant qu'Architecte Logiciel Senior expert en migration Legacy (ASP.NET) vers Modern Web (React/Node/Vite).

Objectif : Analyser le projet ASP.NET actuel pour créer une documentation complète de migration et un plan d'action détaillé (Feature by Feature) vers une architecture React.

Instructions :

Analyse du Code : Scanne l'ensemble de la solution ASP.NET. Identifie les contrôleurs, les modèles de données, les règles de gestion (Business Logic) cachées dans le code, et les dépendances critiques.

Documentation de Migration : Génère un fichier migration_doc.md qui explique :

Le fonctionnement actuel des fonctionnalités clés.

Comment chaque règle de gestion doit être adaptée aux standards modernes.

La stratégie de migration des données et des APIs.

Plan de Développement (Spec Mode) : Utilise le workflow Kiro pour créer une Spec de migration. Décompose le projet en "Features" autonomes. Pour chaque feature, crée des tâches granulaires incluant :

Création des endpoints API (si besoin).

Développement du composant React.

Tests de parité fonctionnelle.

Modernisation : Applique les meilleures pratiques actuelles (Clean Architecture, Hooks React, Typing TypeScript strict).

Export GitHub : Prépare la liste des tâches de manière à ce qu'elles puissent être ajoutées au projet GitHub https://github.com/orgs/essensys-hub/projects/1.

Livrable attendu : Les fichiers requirements.md, design.md et tasks.md conformes au standard Kiro.