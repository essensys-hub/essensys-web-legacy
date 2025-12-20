#!/bin/bash

# Script de déploiement de la documentation Essensys Migration
# Usage: ./deploy-docs.sh [local|build|deploy]

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier que MkDocs est installé
check_dependencies() {
    log_info "Vérification des dépendances..."
    
    if ! command -v mkdocs &> /dev/null; then
        log_error "MkDocs n'est pas installé. Installation en cours..."
        pip install -r requirements.txt
    fi
    
    log_success "Dépendances vérifiées"
}

# Serveur de développement local
serve_local() {
    log_info "Démarrage du serveur de développement local..."
    log_info "La documentation sera disponible sur http://127.0.0.1:8000"
    log_warning "Appuyez sur Ctrl+C pour arrêter le serveur"
    
    mkdocs serve
}

# Build de la documentation
build_docs() {
    log_info "Construction de la documentation..."
    
    # Nettoyer le dossier de build précédent
    if [ -d "site" ]; then
        rm -rf site
        log_info "Dossier site/ nettoyé"
    fi
    
    # Construire la documentation
    mkdocs build --clean --strict
    
    log_success "Documentation construite dans le dossier site/"
}

# Déploiement sur GitHub Pages
deploy_github() {
    log_info "Déploiement sur GitHub Pages..."
    
    # Vérifier que nous sommes sur la branche main/master
    current_branch=$(git branch --show-current)
    if [[ "$current_branch" != "main" && "$current_branch" != "master" ]]; then
        log_warning "Vous n'êtes pas sur la branche main/master (branche actuelle: $current_branch)"
        read -p "Continuer quand même? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log_info "Déploiement annulé"
            exit 0
        fi
    fi
    
    # Vérifier qu'il n'y a pas de modifications non commitées
    if [[ -n $(git status --porcelain) ]]; then
        log_error "Il y a des modifications non commitées. Veuillez commiter vos changements avant le déploiement."
        git status --short
        exit 1
    fi
    
    # Déployer avec MkDocs
    mkdocs gh-deploy --clean --message "Deploy documentation {sha} with MkDocs {version}"
    
    log_success "Documentation déployée sur GitHub Pages!"
    log_info "Elle sera disponible sur: https://$(git config --get remote.origin.url | sed 's/.*github.com[:/]\([^/]*\)\/\([^.]*\).*/\1.github.io\/\2/')/"
}

# Fonction d'aide
show_help() {
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commandes disponibles:"
    echo "  local    Démarrer le serveur de développement local (défaut)"
    echo "  build    Construire la documentation statique"
    echo "  deploy   Déployer sur GitHub Pages"
    echo "  help     Afficher cette aide"
    echo ""
    echo "Exemples:"
    echo "  $0           # Démarrer le serveur local"
    echo "  $0 local     # Démarrer le serveur local"
    echo "  $0 build     # Construire la documentation"
    echo "  $0 deploy    # Déployer sur GitHub Pages"
}

# Script principal
main() {
    local command=${1:-local}
    
    case $command in
        local|serve)
            check_dependencies
            serve_local
            ;;
        build)
            check_dependencies
            build_docs
            ;;
        deploy)
            check_dependencies
            build_docs
            deploy_github
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "Commande inconnue: $command"
            show_help
            exit 1
            ;;
    esac
}

# Exécuter le script principal
main "$@"