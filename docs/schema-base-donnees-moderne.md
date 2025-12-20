# Schéma de Base de Données Moderne - Essensys Migration

## Vue d'Ensemble

Cette documentation détaille le schéma de base de données PostgreSQL moderne pour la migration d'Essensys, incluant toutes les tables, relations, contraintes, index et stratégies d'optimisation des performances.

## Architecture de Base de Données

### Choix Technologiques

**PostgreSQL 15+** comme SGBD principal :
- Support avancé des types JSON/JSONB
- Performances excellentes pour les requêtes complexes
- Extensions riches (UUID, crypto, full-text search)
- Partitioning natif pour la scalabilité
- Réplication et haute disponibilité

**Prisma** comme ORM :
- Type safety avec TypeScript
- Migrations versionnées automatiques
- Query builder optimisé
- Introspection de schéma

## Schéma Complet de Base de Données

### Configuration et Extensions

```sql
-- Extensions PostgreSQL requises
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "btree_gin";

-- Configuration des paramètres
SET timezone = 'Europe/Paris';
SET default_text_search_config = 'french';
```

### Tables Principales

#### 1. Gestion des Utilisateurs

```sql
-- Table des utilisateurs
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    
    -- Adresse
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    postal_code VARCHAR(10) NOT NULL,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(2) DEFAULT 'FR',
    
    -- Contact
    phone VARCHAR(20),
    
    -- Sécurité
    security_question VARCHAR(255) NOT NULL,
    security_answer_hash VARCHAR(255) NOT NULL,
    
    -- Préférences
    marketing_consent BOOLEAN DEFAULT false,
    language VARCHAR(5) DEFAULT 'fr-FR',
    timezone VARCHAR(50) DEFAULT 'Europe/Paris',
    
    -- État
    is_active BOOLEAN DEFAULT true,
    email_verified BOOLEAN DEFAULT false,
    email_verification_token VARCHAR(255),
    email_verification_expires TIMESTAMP WITH TIME ZONE,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE,
    login_count INTEGER DEFAULT 0,
    
    -- Contraintes
    CONSTRAINT users_email_check CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT users_postal_code_check CHECK (postal_code ~ '^\d{5}$'),
    CONSTRAINT users_phone_check CHECK (phone IS NULL OR phone ~ '^\+33[1-9]\d{8}$')
);

-- Index pour les utilisateurs
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_active ON users(is_active) WHERE is_active = true;
CREATE INDEX idx_users_created_at ON users(created_at);
CREATE INDEX idx_users_last_login ON users(last_login DESC) WHERE last_login IS NOT NULL;
CREATE INDEX idx_users_email_verification ON users(email_verification_token) WHERE email_verification_token IS NOT NULL;

-- Index de recherche full-text
CREATE INDEX idx_users_search ON users USING gin(
    to_tsvector('french', first_name || ' ' || last_name || ' ' || email)
);
```

#### 2. Gestion des Machines/Boîtiers

```sql
-- Table des machines/boîtiers
CREATE TABLE machines (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    serial_number VARCHAR(50) UNIQUE NOT NULL,
    activation_key VARCHAR(39) UNIQUE NOT NULL, -- Format: XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
    activation_key_hash VARCHAR(255) NOT NULL,
    
    -- Version et configuration
    firmware_version VARCHAR(20) DEFAULT 'V0',
    hardware_revision VARCHAR(20),
    model VARCHAR(50) DEFAULT 'ESSENSYS_V1',
    
    -- État et configuration
    is_active BOOLEAN DEFAULT true,
    alarm_enabled BOOLEAN DEFAULT false,
    timezone VARCHAR(50) DEFAULT 'Europe/Paris',
    
    -- Localisation (optionnelle)
    location_name VARCHAR(255),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    
    -- Connexion et monitoring
    last_connection TIMESTAMP WITH TIME ZONE,
    last_ip_address INET,
    connection_count INTEGER DEFAULT 0,
    total_uptime_seconds BIGINT DEFAULT 0,
    
    -- Configuration réseau
    network_config JSONB DEFAULT '{}',
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT machines_activation_key_format CHECK (
        activation_key ~ '^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$'
    ),
    CONSTRAINT machines_coordinates_check CHECK (
        (latitude IS NULL AND longitude IS NULL) OR 
        (latitude IS NOT NULL AND longitude IS NOT NULL AND 
         latitude BETWEEN -90 AND 90 AND longitude BETWEEN -180 AND 180)
    )
);

-- Index pour les machines
CREATE UNIQUE INDEX idx_machines_serial_number ON machines(serial_number);
CREATE UNIQUE INDEX idx_machines_activation_key ON machines(activation_key);
CREATE INDEX idx_machines_active ON machines(is_active) WHERE is_active = true;
CREATE INDEX idx_machines_last_connection ON machines(last_connection DESC) WHERE last_connection IS NOT NULL;
CREATE INDEX idx_machines_firmware_version ON machines(firmware_version);
CREATE INDEX idx_machines_location ON machines USING gist(ll_to_earth(latitude, longitude)) WHERE latitude IS NOT NULL;

-- Index GIN pour la configuration réseau
CREATE INDEX idx_machines_network_config ON machines USING gin(network_config);
```

#### 3. Liaison Utilisateurs-Machines

```sql
-- Table de liaison utilisateurs-machines (many-to-many)
CREATE TABLE user_machines (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    machine_id UUID NOT NULL REFERENCES machines(id) ON DELETE CASCADE,
    
    -- Rôle et permissions
    role VARCHAR(20) NOT NULL DEFAULT 'user',
    permissions JSONB DEFAULT '{}',
    
    -- Métadonnées
    display_name VARCHAR(255), -- Nom personnalisé pour cette machine
    is_favorite BOOLEAN DEFAULT false,
    notification_preferences JSONB DEFAULT '{}',
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_accessed TIMESTAMP WITH TIME ZONE,
    
    -- Contraintes
    UNIQUE(user_id, machine_id),
    CONSTRAINT user_machines_role_check CHECK (role IN ('owner', 'admin', 'user', 'guest'))
);

-- Index pour user_machines
CREATE INDEX idx_user_machines_user_id ON user_machines(user_id);
CREATE INDEX idx_user_machines_machine_id ON user_machines(machine_id);
CREATE INDEX idx_user_machines_role ON user_machines(role);
CREATE INDEX idx_user_machines_favorite ON user_machines(user_id, is_favorite) WHERE is_favorite = true;
CREATE INDEX idx_user_machines_last_accessed ON user_machines(last_accessed DESC) WHERE last_accessed IS NOT NULL;
```

#### 4. Types et Appareils

```sql
-- Table des types d'appareils
CREATE TABLE device_types (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    category VARCHAR(50) NOT NULL,
    
    -- Métadonnées
    icon VARCHAR(100),
    description TEXT,
    manufacturer VARCHAR(100),
    
    -- Configuration et validation
    config_schema JSONB NOT NULL DEFAULT '{}',
    state_schema JSONB NOT NULL DEFAULT '{}',
    action_types JSONB NOT NULL DEFAULT '[]',
    
    -- Compatibilité
    min_firmware_version VARCHAR(20),
    max_firmware_version VARCHAR(20),
    
    -- État
    is_active BOOLEAN DEFAULT true,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT device_types_category_check CHECK (
        category IN ('climate', 'security', 'comfort', 'energy', 'lighting', 'multimedia')
    )
);

-- Index pour device_types
CREATE UNIQUE INDEX idx_device_types_name ON device_types(name);
CREATE INDEX idx_device_types_category ON device_types(category);
CREATE INDEX idx_device_types_active ON device_types(is_active) WHERE is_active = true;

-- Table des appareils/zones
CREATE TABLE devices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    machine_id UUID NOT NULL REFERENCES machines(id) ON DELETE CASCADE,
    device_type_id UUID NOT NULL REFERENCES device_types(id),
    
    -- Identification
    name VARCHAR(255) NOT NULL,
    zone VARCHAR(100),
    room VARCHAR(100),
    floor INTEGER,
    
    -- Configuration
    config JSONB DEFAULT '{}',
    
    -- État et monitoring
    is_active BOOLEAN DEFAULT true,
    is_online BOOLEAN DEFAULT false,
    last_seen TIMESTAMP WITH TIME ZONE,
    error_count INTEGER DEFAULT 0,
    last_error TEXT,
    last_error_at TIMESTAMP WITH TIME ZONE,
    
    -- Métadonnées
    installation_date DATE,
    warranty_expires DATE,
    notes TEXT,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT devices_floor_check CHECK (floor IS NULL OR floor BETWEEN -5 AND 50)
);

-- Index pour devices
CREATE INDEX idx_devices_machine_id ON devices(machine_id);
CREATE INDEX idx_devices_device_type_id ON devices(device_type_id);
CREATE INDEX idx_devices_active ON devices(is_active) WHERE is_active = true;
CREATE INDEX idx_devices_online ON devices(is_online) WHERE is_online = true;
CREATE INDEX idx_devices_zone ON devices(zone) WHERE zone IS NOT NULL;
CREATE INDEX idx_devices_room ON devices(room) WHERE room IS NOT NULL;
CREATE INDEX idx_devices_last_seen ON devices(last_seen DESC) WHERE last_seen IS NOT NULL;

-- Index composite pour les requêtes fréquentes
CREATE INDEX idx_devices_machine_active ON devices(machine_id, is_active);
CREATE INDEX idx_devices_machine_zone ON devices(machine_id, zone) WHERE zone IS NOT NULL;

-- Index GIN pour la configuration
CREATE INDEX idx_devices_config ON devices USING gin(config);

-- Index de recherche full-text
CREATE INDEX idx_devices_search ON devices USING gin(
    to_tsvector('french', name || ' ' || COALESCE(zone, '') || ' ' || COALESCE(room, ''))
);
```

#### 5. Actions et États

```sql
-- Table des actions à exécuter
CREATE TABLE actions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    machine_id UUID NOT NULL REFERENCES machines(id) ON DELETE CASCADE,
    device_id UUID REFERENCES devices(id) ON DELETE CASCADE,
    
    -- Action
    action_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL DEFAULT '{}',
    
    -- Métadonnées
    priority INTEGER DEFAULT 5,
    timeout_seconds INTEGER DEFAULT 30,
    max_retries INTEGER DEFAULT 3,
    retry_count INTEGER DEFAULT 0,
    
    -- État
    status VARCHAR(20) DEFAULT 'pending',
    
    -- Résultat
    result JSONB,
    error_message TEXT,
    error_code VARCHAR(50),
    
    -- Timing
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    scheduled_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    sent_at TIMESTAMP WITH TIME ZONE,
    executed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    
    -- Audit
    created_by UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Contraintes
    CONSTRAINT actions_status_check CHECK (
        status IN ('pending', 'scheduled', 'sent', 'executing', 'completed', 'failed', 'cancelled', 'timeout')
    ),
    CONSTRAINT actions_priority_check CHECK (priority BETWEEN 1 AND 10),
    CONSTRAINT actions_timeout_check CHECK (timeout_seconds > 0),
    CONSTRAINT actions_retry_check CHECK (retry_count <= max_retries)
);

-- Index pour actions
CREATE INDEX idx_actions_machine_id ON actions(machine_id);
CREATE INDEX idx_actions_device_id ON actions(device_id) WHERE device_id IS NOT NULL;
CREATE INDEX idx_actions_status ON actions(status);
CREATE INDEX idx_actions_created_at ON actions(created_at DESC);
CREATE INDEX idx_actions_scheduled_at ON actions(scheduled_at) WHERE status IN ('pending', 'scheduled');

-- Index composite pour les requêtes fréquentes
CREATE INDEX idx_actions_machine_status ON actions(machine_id, status);
CREATE INDEX idx_actions_device_status ON actions(device_id, status) WHERE device_id IS NOT NULL;
CREATE INDEX idx_actions_priority_scheduled ON actions(priority DESC, scheduled_at) WHERE status IN ('pending', 'scheduled');

-- Index partiel pour les actions en cours
CREATE INDEX idx_actions_active ON actions(created_at DESC) 
WHERE status IN ('pending', 'scheduled', 'sent', 'executing');

-- Table des états des appareils (partitionnée par mois)
CREATE TABLE device_states (
    id UUID DEFAULT uuid_generate_v4(),
    device_id UUID NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    
    -- État
    state_data JSONB NOT NULL DEFAULT '{}',
    
    -- Métadonnées
    source VARCHAR(50) DEFAULT 'device', -- 'device', 'user', 'system', 'scheduled'
    quality_score DECIMAL(3,2) DEFAULT 1.0, -- 0.0 à 1.0
    
    -- Timing
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    received_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT device_states_quality_check CHECK (quality_score BETWEEN 0.0 AND 1.0),
    CONSTRAINT device_states_source_check CHECK (
        source IN ('device', 'user', 'system', 'scheduled', 'estimated')
    )
) PARTITION BY RANGE (timestamp);

-- Créer les partitions pour les 12 prochains mois
DO $$
DECLARE
    start_date DATE;
    end_date DATE;
    partition_name TEXT;
BEGIN
    FOR i IN 0..11 LOOP
        start_date := date_trunc('month', CURRENT_DATE + (i || ' months')::INTERVAL);
        end_date := start_date + INTERVAL '1 month';
        partition_name := 'device_states_' || to_char(start_date, 'YYYY_MM');
        
        EXECUTE format('CREATE TABLE %I PARTITION OF device_states 
                       FOR VALUES FROM (%L) TO (%L)', 
                       partition_name, start_date, end_date);
        
        -- Index sur chaque partition
        EXECUTE format('CREATE INDEX %I ON %I (device_id, timestamp DESC)', 
                       'idx_' || partition_name || '_device_time', partition_name);
        EXECUTE format('CREATE INDEX %I ON %I (timestamp DESC)', 
                       'idx_' || partition_name || '_timestamp', partition_name);
    END LOOP;
END $$;

-- Index principal sur la table partitionnée
CREATE INDEX idx_device_states_device_id ON device_states(device_id);
CREATE INDEX idx_device_states_timestamp ON device_states(timestamp DESC);
CREATE INDEX idx_device_states_source ON device_states(source);
```

#### 6. Firmware et Mises à Jour

```sql
-- Table des versions firmware
CREATE TABLE firmware_versions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Version
    version_number INTEGER UNIQUE NOT NULL,
    version_name VARCHAR(50) NOT NULL,
    version_tag VARCHAR(20), -- 'stable', 'beta', 'alpha'
    
    -- Métadonnées
    description TEXT,
    changelog TEXT,
    release_notes TEXT,
    
    -- Fichier
    filename VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL,
    checksum_sha256 VARCHAR(64) NOT NULL,
    download_url VARCHAR(500),
    
    -- Compatibilité
    min_hardware_revision VARCHAR(20),
    max_hardware_revision VARCHAR(20),
    compatible_models JSONB DEFAULT '[]',
    
    -- État
    is_active BOOLEAN DEFAULT true,
    is_mandatory BOOLEAN DEFAULT false,
    
    -- Dates
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    released_at TIMESTAMP WITH TIME ZONE,
    deprecated_at TIMESTAMP WITH TIME ZONE,
    
    -- Contraintes
    CONSTRAINT firmware_versions_version_tag_check CHECK (
        version_tag IS NULL OR version_tag IN ('stable', 'beta', 'alpha', 'rc')
    ),
    CONSTRAINT firmware_versions_file_size_check CHECK (file_size > 0),
    CONSTRAINT firmware_versions_checksum_check CHECK (length(checksum_sha256) = 64)
);

-- Index pour firmware_versions
CREATE UNIQUE INDEX idx_firmware_versions_number ON firmware_versions(version_number);
CREATE INDEX idx_firmware_versions_name ON firmware_versions(version_name);
CREATE INDEX idx_firmware_versions_active ON firmware_versions(is_active) WHERE is_active = true;
CREATE INDEX idx_firmware_versions_released ON firmware_versions(released_at DESC) WHERE released_at IS NOT NULL;
CREATE INDEX idx_firmware_versions_tag ON firmware_versions(version_tag) WHERE version_tag IS NOT NULL;

-- Table du suivi des déploiements firmware
CREATE TABLE firmware_deployments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    machine_id UUID NOT NULL REFERENCES machines(id) ON DELETE CASCADE,
    firmware_version_id UUID NOT NULL REFERENCES firmware_versions(id),
    
    -- État du déploiement
    status VARCHAR(20) DEFAULT 'initiated',
    progress_percentage INTEGER DEFAULT 0,
    
    -- Métadonnées
    deployment_type VARCHAR(20) DEFAULT 'manual', -- 'manual', 'automatic', 'scheduled'
    initiated_by UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Timing
    started_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    downloaded_at TIMESTAMP WITH TIME ZONE,
    installed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    
    -- Résultat
    success BOOLEAN,
    error_message TEXT,
    error_code VARCHAR(50),
    rollback_version VARCHAR(20),
    
    -- Contraintes
    CONSTRAINT firmware_deployments_status_check CHECK (
        status IN ('initiated', 'downloading', 'downloaded', 'installing', 'completed', 'failed', 'rolled_back')
    ),
    CONSTRAINT firmware_deployments_progress_check CHECK (progress_percentage BETWEEN 0 AND 100),
    CONSTRAINT firmware_deployments_type_check CHECK (
        deployment_type IN ('manual', 'automatic', 'scheduled', 'emergency')
    )
);

-- Index pour firmware_deployments
CREATE INDEX idx_firmware_deployments_machine_id ON firmware_deployments(machine_id);
CREATE INDEX idx_firmware_deployments_firmware_version_id ON firmware_deployments(firmware_version_id);
CREATE INDEX idx_firmware_deployments_status ON firmware_deployments(status);
CREATE INDEX idx_firmware_deployments_started_at ON firmware_deployments(started_at DESC);

-- Index composite
CREATE INDEX idx_firmware_deployments_machine_status ON firmware_deployments(machine_id, status);
```

#### 7. Notifications et Communications

```sql
-- Table des contacts de notification
CREATE TABLE notification_contacts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Contact
    type VARCHAR(20) NOT NULL,
    contact_value VARCHAR(255) NOT NULL,
    display_name VARCHAR(255),
    
    -- Vérification
    is_verified BOOLEAN DEFAULT false,
    verification_token VARCHAR(255),
    verification_expires TIMESTAMP WITH TIME ZONE,
    verified_at TIMESTAMP WITH TIME ZONE,
    
    -- État
    is_active BOOLEAN DEFAULT true,
    
    -- Préférences
    preferences JSONB DEFAULT '{}',
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT notification_contacts_type_check CHECK (
        type IN ('sms', 'email', 'push', 'webhook')
    ),
    CONSTRAINT notification_contacts_sms_format CHECK (
        type != 'sms' OR contact_value ~ '^\+33[1-9]\d{8}$'
    ),
    CONSTRAINT notification_contacts_email_format CHECK (
        type != 'email' OR contact_value ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
    )
);

-- Index pour notification_contacts
CREATE INDEX idx_notification_contacts_user_id ON notification_contacts(user_id);
CREATE INDEX idx_notification_contacts_type ON notification_contacts(type);
CREATE INDEX idx_notification_contacts_verified ON notification_contacts(is_verified) WHERE is_verified = true;
CREATE INDEX idx_notification_contacts_active ON notification_contacts(is_active) WHERE is_active = true;

-- Index composite
CREATE INDEX idx_notification_contacts_user_type ON notification_contacts(user_id, type);

-- Table des notifications envoyées (partitionnée par mois)
CREATE TABLE notifications (
    id UUID DEFAULT uuid_generate_v4(),
    contact_id UUID NOT NULL REFERENCES notification_contacts(id) ON DELETE CASCADE,
    
    -- Contenu
    type VARCHAR(50) NOT NULL,
    subject VARCHAR(255),
    message TEXT NOT NULL,
    
    -- Métadonnées
    priority VARCHAR(10) DEFAULT 'normal',
    category VARCHAR(50),
    
    -- État
    status VARCHAR(20) DEFAULT 'pending',
    
    -- Résultat
    external_id VARCHAR(255), -- ID du service externe (SMS, email)
    delivery_receipt JSONB,
    error_message TEXT,
    error_code VARCHAR(50),
    
    -- Timing
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    scheduled_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    sent_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    read_at TIMESTAMP WITH TIME ZONE,
    
    -- Contraintes
    CONSTRAINT notifications_priority_check CHECK (
        priority IN ('low', 'normal', 'high', 'urgent')
    ),
    CONSTRAINT notifications_status_check CHECK (
        status IN ('pending', 'scheduled', 'sending', 'sent', 'delivered', 'read', 'failed', 'cancelled')
    )
) PARTITION BY RANGE (created_at);

-- Créer les partitions pour les notifications (6 mois)
DO $$
DECLARE
    start_date DATE;
    end_date DATE;
    partition_name TEXT;
BEGIN
    FOR i IN 0..5 LOOP
        start_date := date_trunc('month', CURRENT_DATE + (i || ' months')::INTERVAL);
        end_date := start_date + INTERVAL '1 month';
        partition_name := 'notifications_' || to_char(start_date, 'YYYY_MM');
        
        EXECUTE format('CREATE TABLE %I PARTITION OF notifications 
                       FOR VALUES FROM (%L) TO (%L)', 
                       partition_name, start_date, end_date);
        
        -- Index sur chaque partition
        EXECUTE format('CREATE INDEX %I ON %I (contact_id, created_at DESC)', 
                       'idx_' || partition_name || '_contact_time', partition_name);
        EXECUTE format('CREATE INDEX %I ON %I (status)', 
                       'idx_' || partition_name || '_status', partition_name);
    END LOOP;
END $$;
```

#### 8. Audit et Sécurité

```sql
-- Table d'audit pour traçabilité (partitionnée par trimestre)
CREATE TABLE audit_logs (
    id UUID DEFAULT uuid_generate_v4(),
    
    -- Acteur
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    machine_id UUID REFERENCES machines(id) ON DELETE SET NULL,
    
    -- Action
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(50),
    resource_id UUID,
    
    -- Données
    old_values JSONB,
    new_values JSONB,
    
    -- Contexte
    ip_address INET,
    user_agent TEXT,
    session_id VARCHAR(255),
    
    -- Métadonnées
    severity VARCHAR(10) DEFAULT 'info',
    category VARCHAR(50),
    
    -- Timing
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT audit_logs_severity_check CHECK (
        severity IN ('debug', 'info', 'warn', 'error', 'critical')
    )
) PARTITION BY RANGE (timestamp);

-- Créer les partitions pour l'audit (4 trimestres)
DO $$
DECLARE
    start_date DATE;
    end_date DATE;
    partition_name TEXT;
BEGIN
    FOR i IN 0..3 LOOP
        start_date := date_trunc('quarter', CURRENT_DATE + (i || ' months')::INTERVAL * 3);
        end_date := start_date + INTERVAL '3 months';
        partition_name := 'audit_logs_' || to_char(start_date, 'YYYY_Q');
        
        EXECUTE format('CREATE TABLE %I PARTITION OF audit_logs 
                       FOR VALUES FROM (%L) TO (%L)', 
                       partition_name, start_date, end_date);
        
        -- Index sur chaque partition
        EXECUTE format('CREATE INDEX %I ON %I (timestamp DESC)', 
                       'idx_' || partition_name || '_timestamp', partition_name);
        EXECUTE format('CREATE INDEX %I ON %I (user_id, timestamp DESC)', 
                       'idx_' || partition_name || '_user_time', partition_name);
    END LOOP;
END $$;

-- Table des sessions utilisateur
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Token
    refresh_token_hash VARCHAR(255) NOT NULL,
    
    -- Métadonnées
    device_info JSONB DEFAULT '{}',
    ip_address INET,
    user_agent TEXT,
    
    -- État
    is_active BOOLEAN DEFAULT true,
    
    -- Timing
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_used_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Contraintes
    CONSTRAINT user_sessions_expires_check CHECK (expires_at > created_at)
);

-- Index pour user_sessions
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_token_hash ON user_sessions(refresh_token_hash);
CREATE INDEX idx_user_sessions_expires ON user_sessions(expires_at);
CREATE INDEX idx_user_sessions_active ON user_sessions(is_active) WHERE is_active = true;

-- Index composite
CREATE INDEX idx_user_sessions_user_expires ON user_sessions(user_id, expires_at);

-- Table des clés d'activation produit
CREATE TABLE product_keys (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Clé
    activation_key VARCHAR(39) UNIQUE NOT NULL,
    activation_key_hash VARCHAR(255) NOT NULL,
    
    -- Produit
    product_type VARCHAR(50) NOT NULL DEFAULT 'ESSENSYS_STANDARD',
    batch_number VARCHAR(50),
    
    -- État
    is_used BOOLEAN DEFAULT false,
    machine_id UUID REFERENCES machines(id) ON DELETE SET NULL,
    
    -- Timing
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    used_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    
    -- Contraintes
    CONSTRAINT product_keys_activation_key_format CHECK (
        activation_key ~ '^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$'
    )
);

-- Index pour product_keys
CREATE UNIQUE INDEX idx_product_keys_activation_key ON product_keys(activation_key);
CREATE INDEX idx_product_keys_used ON product_keys(is_used);
CREATE INDEX idx_product_keys_product_type ON product_keys(product_type);
CREATE INDEX idx_product_keys_expires ON product_keys(expires_at) WHERE expires_at IS NOT NULL;
```

### Triggers et Fonctions

```sql
-- Fonction pour mettre à jour updated_at automatiquement
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers pour updated_at
CREATE TRIGGER update_users_updated_at 
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_machines_updated_at 
    BEFORE UPDATE ON machines
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_devices_updated_at 
    BEFORE UPDATE ON devices
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_user_machines_updated_at 
    BEFORE UPDATE ON user_machines
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_device_types_updated_at 
    BEFORE UPDATE ON device_types
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_notification_contacts_updated_at 
    BEFORE UPDATE ON notification_contacts
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Fonction d'audit automatique
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_logs (
        user_id,
        action,
        resource_type,
        resource_id,
        old_values,
        new_values,
        ip_address,
        timestamp
    ) VALUES (
        COALESCE(current_setting('app.current_user_id', true)::UUID, NULL),
        TG_OP,
        TG_TABLE_NAME,
        COALESCE(NEW.id, OLD.id),
        CASE WHEN TG_OP = 'DELETE' THEN to_jsonb(OLD) ELSE NULL END,
        CASE WHEN TG_OP != 'DELETE' THEN to_jsonb(NEW) ELSE NULL END,
        COALESCE(current_setting('app.client_ip', true)::INET, NULL),
        NOW()
    );
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Triggers d'audit sur les tables sensibles
CREATE TRIGGER audit_users_trigger
    AFTER INSERT OR UPDATE OR DELETE ON users
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_machines_trigger
    AFTER INSERT OR UPDATE OR DELETE ON machines
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_user_machines_trigger
    AFTER INSERT OR UPDATE OR DELETE ON user_machines
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

-- Fonction de nettoyage automatique des données expirées
CREATE OR REPLACE FUNCTION cleanup_expired_data()
RETURNS void AS $$
BEGIN
    -- Nettoyer les sessions expirées
    DELETE FROM user_sessions 
    WHERE expires_at < NOW() - INTERVAL '7 days';
    
    -- Nettoyer les tokens de vérification expirés
    UPDATE users 
    SET email_verification_token = NULL, 
        email_verification_expires = NULL
    WHERE email_verification_expires < NOW();
    
    -- Nettoyer les anciennes notifications (> 6 mois)
    DELETE FROM notifications 
    WHERE created_at < NOW() - INTERVAL '6 months';
    
    -- Nettoyer les anciens logs d'audit (> 2 ans)
    DELETE FROM audit_logs 
    WHERE timestamp < NOW() - INTERVAL '2 years';
    
    -- Nettoyer les anciens états d'appareils (> 1 an)
    DELETE FROM device_states 
    WHERE timestamp < NOW() - INTERVAL '1 year';
    
    RAISE NOTICE 'Nettoyage des données expirées terminé';
END;
$$ LANGUAGE plpgsql;
```

### Données de Référence

```sql
-- Insertion des types d'appareils de base
INSERT INTO device_types (name, display_name, category, icon, config_schema, state_schema, action_types) VALUES
('heating_zone', 'Zone de Chauffage', 'climate', 'thermometer', 
 '{"type": "object", "properties": {"target_temperature": {"type": "number", "minimum": 5, "maximum": 35}, "mode": {"type": "string", "enum": ["off", "eco", "comfort", "auto"]}, "schedule": {"type": "object"}}}',
 '{"type": "object", "properties": {"temperature": {"type": "number"}, "target_temperature": {"type": "number"}, "mode": {"type": "string"}, "humidity": {"type": "number"}, "heating_active": {"type": "boolean"}}}',
 '["set_temperature", "set_mode", "set_schedule"]'),

('shutter', 'Volet Roulant', 'comfort', 'window', 
 '{"type": "object", "properties": {"auto_close_time": {"type": "string"}, "auto_open_time": {"type": "string"}, "wind_protection": {"type": "boolean"}}}',
 '{"type": "object", "properties": {"position": {"type": "number", "minimum": 0, "maximum": 100}, "is_moving": {"type": "boolean"}, "direction": {"type": "string", "enum": ["up", "down", "stopped"]}}}',
 '["open", "close", "set_position", "stop"]'),

('alarm_system', 'Système d\'Alarme', 'security', 'shield', 
 '{"type": "object", "properties": {"zones": {"type": "array", "items": {"type": "string"}}, "delay_seconds": {"type": "number"}, "siren_duration": {"type": "number"}}}',
 '{"type": "object", "properties": {"armed": {"type": "boolean"}, "triggered": {"type": "boolean"}, "zones_status": {"type": "object"}, "last_triggered": {"type": "string"}}}',
 '["arm", "disarm", "arm_partial", "test_siren"]'),

('water_heater', 'Chauffe-eau', 'climate', 'water-drop', 
 '{"type": "object", "properties": {"capacity_liters": {"type": "number"}, "max_temperature": {"type": "number"}, "eco_temperature": {"type": "number"}}}',
 '{"type": "object", "properties": {"temperature": {"type": "number"}, "target_temperature": {"type": "number"}, "mode": {"type": "string"}, "heating_active": {"type": "boolean"}, "water_level": {"type": "number"}}}',
 '["set_temperature", "set_mode", "boost"]'),

('lighting', 'Éclairage', 'lighting', 'light-bulb',
 '{"type": "object", "properties": {"dimmable": {"type": "boolean"}, "color_temperature": {"type": "boolean"}, "rgb": {"type": "boolean"}}}',
 '{"type": "object", "properties": {"on": {"type": "boolean"}, "brightness": {"type": "number", "minimum": 0, "maximum": 100}, "color_temperature": {"type": "number"}, "rgb": {"type": "array"}}}',
 '["turn_on", "turn_off", "set_brightness", "set_color"]');

-- Insertion d'une version firmware de base
INSERT INTO firmware_versions (version_number, version_name, description, filename, file_size, checksum_sha256, is_active, released_at) VALUES
(1, 'V1', 'Version initiale du firmware Essensys', 'essensys_v1.bin', 2048576, 'a1b2c3d4e5f6789012345678901234567890123456789012345678901234567890', true, NOW());
```

### Stratégies d'Optimisation des Performances

#### Index Composites Avancés

```sql
-- Index pour les requêtes de dashboard
CREATE INDEX idx_devices_dashboard ON devices(machine_id, is_active, zone) 
WHERE is_active = true;

-- Index pour les actions en attente par priorité
CREATE INDEX idx_actions_pending_priority ON actions(machine_id, priority DESC, scheduled_at) 
WHERE status IN ('pending', 'scheduled');

-- Index pour l'historique des états récents
CREATE INDEX idx_device_states_recent ON device_states(device_id, timestamp DESC) 
WHERE timestamp > NOW() - INTERVAL '7 days';

-- Index pour les notifications non lues
CREATE INDEX idx_notifications_unread ON notifications(contact_id, created_at DESC) 
WHERE status IN ('sent', 'delivered');
```

#### Vues Matérialisées pour les Performances

```sql
-- Vue matérialisée pour les statistiques des machines
CREATE MATERIALIZED VIEW machine_stats AS
SELECT 
    m.id,
    m.serial_number,
    m.firmware_version,
    COUNT(d.id) as device_count,
    COUNT(d.id) FILTER (WHERE d.is_active = true) as active_device_count,
    COUNT(d.id) FILTER (WHERE d.is_online = true) as online_device_count,
    MAX(ds.timestamp) as last_state_update,
    COUNT(a.id) FILTER (WHERE a.status = 'pending') as pending_actions_count,
    m.last_connection,
    CASE 
        WHEN m.last_connection > NOW() - INTERVAL '5 minutes' THEN 'online'
        WHEN m.last_connection > NOW() - INTERVAL '1 hour' THEN 'idle'
        ELSE 'offline'
    END as connection_status
FROM machines m
LEFT JOIN devices d ON m.id = d.machine_id
LEFT JOIN device_states ds ON d.id = ds.device_id
LEFT JOIN actions a ON m.id = a.machine_id AND a.created_at > NOW() - INTERVAL '1 day'
WHERE m.is_active = true
GROUP BY m.id, m.serial_number, m.firmware_version, m.last_connection;

-- Index sur la vue matérialisée
CREATE UNIQUE INDEX idx_machine_stats_id ON machine_stats(id);
CREATE INDEX idx_machine_stats_connection_status ON machine_stats(connection_status);

-- Vue matérialisée pour les statistiques utilisateur
CREATE MATERIALIZED VIEW user_stats AS
SELECT 
    u.id,
    u.email,
    COUNT(um.machine_id) as machine_count,
    COUNT(um.machine_id) FILTER (WHERE um.role = 'owner') as owned_machines_count,
    MAX(um.last_accessed) as last_machine_access,
    COUNT(nc.id) FILTER (WHERE nc.is_verified = true) as verified_contacts_count,
    u.last_login,
    u.login_count
FROM users u
LEFT JOIN user_machines um ON u.id = um.user_id
LEFT JOIN notification_contacts nc ON u.id = nc.user_id AND nc.is_active = true
WHERE u.is_active = true
GROUP BY u.id, u.email, u.last_login, u.login_count;

-- Rafraîchissement automatique des vues matérialisées
CREATE OR REPLACE FUNCTION refresh_materialized_views()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY machine_stats;
    REFRESH MATERIALIZED VIEW CONCURRENTLY user_stats;
END;
$$ LANGUAGE plpgsql;
```

### Stratégie de Partitioning pour les Gros Volumes

#### Partitioning Automatique

```sql
-- Fonction pour créer automatiquement les partitions futures
CREATE OR REPLACE FUNCTION create_monthly_partitions(table_name TEXT, months_ahead INTEGER DEFAULT 3)
RETURNS void AS $$
DECLARE
    start_date DATE;
    end_date DATE;
    partition_name TEXT;
BEGIN
    FOR i IN 0..months_ahead LOOP
        start_date := date_trunc('month', CURRENT_DATE + (i || ' months')::INTERVAL);
        end_date := start_date + INTERVAL '1 month';
        partition_name := table_name || '_' || to_char(start_date, 'YYYY_MM');
        
        -- Vérifier si la partition existe déjà
        IF NOT EXISTS (
            SELECT 1 FROM pg_class WHERE relname = partition_name
        ) THEN
            EXECUTE format('CREATE TABLE %I PARTITION OF %I 
                           FOR VALUES FROM (%L) TO (%L)', 
                           partition_name, table_name, start_date, end_date);
            
            -- Créer les index sur la nouvelle partition
            CASE table_name
                WHEN 'device_states' THEN
                    EXECUTE format('CREATE INDEX %I ON %I (device_id, timestamp DESC)', 
                                   'idx_' || partition_name || '_device_time', partition_name);
                    EXECUTE format('CREATE INDEX %I ON %I (timestamp DESC)', 
                                   'idx_' || partition_name || '_timestamp', partition_name);
                WHEN 'notifications' THEN
                    EXECUTE format('CREATE INDEX %I ON %I (contact_id, created_at DESC)', 
                                   'idx_' || partition_name || '_contact_time', partition_name);
                    EXECUTE format('CREATE INDEX %I ON %I (status)', 
                                   'idx_' || partition_name || '_status', partition_name);
            END CASE;
            
            RAISE NOTICE 'Partition créée: %', partition_name;
        END IF;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour supprimer les anciennes partitions
CREATE OR REPLACE FUNCTION drop_old_partitions(table_name TEXT, keep_months INTEGER DEFAULT 12)
RETURNS void AS $$
DECLARE
    partition_record RECORD;
    cutoff_date DATE;
BEGIN
    cutoff_date := date_trunc('month', CURRENT_DATE - (keep_months || ' months')::INTERVAL);
    
    FOR partition_record IN
        SELECT schemaname, tablename 
        FROM pg_tables 
        WHERE tablename LIKE table_name || '_%'
        AND tablename ~ '\d{4}_\d{2}$'
    LOOP
        -- Extraire la date de la partition
        DECLARE
            partition_date DATE;
            date_part TEXT;
        BEGIN
            date_part := substring(partition_record.tablename from '(\d{4}_\d{2})$');
            partition_date := to_date(date_part, 'YYYY_MM');
            
            IF partition_date < cutoff_date THEN
                EXECUTE format('DROP TABLE IF EXISTS %I', partition_record.tablename);
                RAISE NOTICE 'Partition supprimée: %', partition_record.tablename;
            END IF;
        END;
    END LOOP;
END;
$$ LANGUAGE plpgsql;
```

### Configuration des Performances PostgreSQL

```sql
-- Configuration recommandée pour PostgreSQL (à adapter selon les ressources)
-- Dans postgresql.conf :

-- Mémoire
-- shared_buffers = 256MB (25% de la RAM)
-- effective_cache_size = 1GB (75% de la RAM)
-- work_mem = 4MB
-- maintenance_work_mem = 64MB

-- Checkpoints
-- checkpoint_completion_target = 0.9
-- wal_buffers = 16MB
-- default_statistics_target = 100

-- Logging pour monitoring
-- log_min_duration_statement = 1000
-- log_checkpoints = on
-- log_connections = on
-- log_disconnections = on
-- log_lock_waits = on

-- Requête pour surveiller les performances
CREATE OR REPLACE VIEW slow_queries AS
SELECT 
    query,
    calls,
    total_time,
    mean_time,
    rows,
    100.0 * shared_blks_hit / nullif(shared_blks_hit + shared_blks_read, 0) AS hit_percent
FROM pg_stat_statements 
ORDER BY total_time DESC;
```

Ce schéma de base de données moderne fournit une architecture robuste, performante et scalable pour la migration d'Essensys, avec des optimisations avancées pour gérer efficacement les gros volumes de données IoT.