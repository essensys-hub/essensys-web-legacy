# Stratégie de Migration des Données - Essensys Legacy vers PostgreSQL

## 4.1 Mapping des Données Legacy vers le Nouveau Schéma

### Vue d'Ensemble du Mapping

Cette section documente le mapping complet entre la base de données SQL Server legacy et le nouveau schéma PostgreSQL moderne. Chaque table legacy est analysée et mappée vers les nouvelles structures avec les transformations nécessaires.

### Tables Legacy Analysées

#### ES_USER → users + user_sessions
**Table Source:** ES_USER (SQL Server)
**Tables Cibles:** users, user_sessions (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| MAIL | varchar(255) | email | varchar(255) | LOWER(TRIM()) + validation email |
| PASSWORD | varchar(255) | password_hash | varchar(255) | Migration SHA1 → bcrypt lors connexion |
| NOM | varchar(255) | last_name | varchar(255) | TRIM() |
| PRENOM | varchar(255) | first_name | varchar(255) | TRIM() |
| ADR1 | varchar(255) | address_line1 | varchar(255) | TRIM() |
| ADR2 | varchar(255) | address_line2 | varchar(255) | NULLIF(TRIM(), '') |
| CP | varchar(5) | postal_code | varchar(10) | Validation format |
| VILLE | varchar(255) | city | varchar(255) | TRIM() |
| PHONE | varchar(20) | phone | varchar(20) | NULLIF(TRIM(), '') |
| QUESTION | varchar(255) | security_question | varchar(255) | TRIM() |
| REPONSE | varchar(255) | security_answer_hash | varchar(255) | Garder hash SHA1 temporairement |
| SENDINFOS | bit | marketing_consent | boolean | COALESCE(false) |
| PKEY | varchar(255) | - | - | Ignoré (lié à machine) |
| ISVALID | bit | is_active | boolean | COALESCE(false) |
| OBSOLETE | bit | - | - | Utilisé pour filtrer (NOT OBSOLETE) |
| DATECREATION | datetime | created_at | timestamptz | COALESCE(NOW()) |
| DATECLOTURE | datetime | - | - | Ignoré si OBSOLETE |
| LASTACCESS | datetime | last_login | timestamptz | NULLIF('1900-01-01') |
| GUID | varchar(255) | - | - | Ignoré |
| MACHINE_ID | int | - | - | Géré via user_machines |

**Règles de Transformation:**
- Emails convertis en minuscules et validés
- Mots de passe SHA1 marqués pour re-hashage en bcrypt
- Utilisateurs obsolètes exclus de la migration
- Adresses nettoyées et validées

#### ES_MACHINE → machines
**Table Source:** ES_MACHINE (SQL Server)
**Table Cible:** machines (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| NOSERIE | varchar(255) | serial_number | varchar(255) | COALESCE('UNKNOWN_' + ID) |
| VERSION | varchar(255) | firmware_version | varchar(50) | COALESCE('V0') |
| PKEY | varchar(255) | activation_key | varchar(32) | Validation format 32 chars |
| HASHEDPKEY | varchar(255) | activation_key_hash | varchar(255) | Migration vers bcrypt |
| DATECREATION | datetime | created_at | timestamptz | COALESCE(NOW()) |
| DATEMODIFICATION | datetime | updated_at | timestamptz | COALESCE(NOW()) |
| ISACTIVE | bit | is_active | boolean | COALESCE(true) |
| AUTORISEALARME | bit | alarm_enabled | boolean | COALESCE(false) |
| - | - | timezone | varchar(50) | 'Europe/Paris' (défaut) |
| - | - | connection_count | integer | 0 (défaut) |
| - | - | last_connection | timestamptz | NULL |
| - | - | last_ip_address | inet | NULL |
| - | - | hardware_revision | varchar(20) | NULL |

**Règles de Transformation:**
- Clés d'activation validées (format 32 caractères)
- Hash des clés migré vers bcrypt
- Numéros de série générés si manquants
- Nouveaux champs initialisés avec valeurs par défaut

#### ES_USER + ES_MACHINE → user_machines
**Tables Sources:** ES_USER, ES_MACHINE (SQL Server)
**Table Cible:** user_machines (PostgreSQL)

| Relation Legacy | Champ Moderne | Type Moderne | Transformation |
|-----------------|---------------|--------------|----------------|
| USER.MACHINE_ID → MACHINE.ID | user_id | UUID | Lookup via email |
| USER.MACHINE_ID → MACHINE.ID | machine_id | UUID | Lookup via activation_key |
| - | role | varchar(20) | 'owner' (défaut) |
| - | permissions | jsonb | '{}' (défaut) |
| - | created_at | timestamptz | NOW() |

**Règles de Transformation:**
- Relation 1:1 legacy → many-to-many moderne
- Tous les utilisateurs legacy deviennent propriétaires
- Permissions étendues possibles ultérieurement

#### ES_ACTION → actions
**Table Source:** ES_ACTION (SQL Server)
**Table Cible:** actions (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| DATECREATION | datetime | created_at | timestamptz | Direct |
| GUID | varchar(255) | - | - | Ignoré |
| ISDONE | bit | status | varchar(20) | 'executed' si true, 'pending' si false |
| ACTIONTYPE | varchar(50) | action_type | varchar(50) | Mapping vers nouveaux types |
| ACTIONINFO | varchar(max) | payload | jsonb | Parsing XML/string → JSON |
| MACHINE_ID | int | machine_id | UUID | Lookup via ES_MACHINE |
| - | - | device_id | UUID | NULL (pas de devices legacy) |
| - | - | priority | integer | 5 (défaut) |
| - | - | sent_at | timestamptz | created_at si ISDONE |
| - | - | executed_at | timestamptz | created_at si ISDONE |
| - | - | retry_count | integer | 0 |
| - | - | max_retries | integer | 3 |

**Mapping des Types d'Actions:**
```
ALARME → set_alarm_state
CHAUFFAGE → set_temperature
VOLET → set_shutter_position
CUMULUS → set_water_heater_mode
STORE → set_awning_position
```

**Règles de Transformation:**
- ACTIONINFO parsé depuis format propriétaire vers JSON
- Actions terminées marquées comme 'executed'
- Nouveaux champs de retry et priorité initialisés

#### ES_ACTIONINDEX → actions.payload
**Table Source:** ES_ACTIONINDEX + ES_DATAINDEX (SQL Server)
**Champ Cible:** actions.payload (PostgreSQL JSONB)

| Structure Legacy | Structure Moderne | Transformation |
|------------------|-------------------|----------------|
| ACTION_ID → multiple ACTIONINDEX | action.payload | Agrégation en JSON |
| DATAINDEX.INDEXKEY + VALUE | Clé-valeur JSON | Mapping des index |

**Mapping des Index d'Actions:**
```json
{
  "407": "alarm_remote_access",
  "920": "button_state_bp1",
  "100": "temperature_setpoint",
  "200": "shutter_position",
  "300": "water_heater_mode"
}
```

#### ES_STATE → device_states
**Table Source:** ES_STATE (SQL Server)
**Table Cible:** device_states (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| MACHINE_ID | int | device_id | UUID | Création device virtuel |
| DATECREATION | datetime | timestamp | timestamptz | Direct |
| VERSION | varchar(255) | - | - | Ignoré (dans machine) |
| - | - | state_data | jsonb | Agrégation des STATEINDEX |

**Règles de Transformation:**
- Un device virtuel "system" créé par machine
- États agrégés depuis ES_STATEINDEX en JSON
- Horodatage préservé

#### ES_STATEINDEX → device_states.state_data
**Table Source:** ES_STATEINDEX + ES_DATAINDEX (SQL Server)
**Champ Cible:** device_states.state_data (PostgreSQL JSONB)

**Mapping des Index d'États:**
```json
{
  "100": "current_temperature",
  "200": "shutter_position",
  "300": "water_heater_status",
  "407": "alarm_status",
  "920": "button_bp1_state"
}
```

#### ES_DATAINDEX → device_types + référence
**Table Source:** ES_DATAINDEX (SQL Server)
**Table Cible:** device_types + documentation (PostgreSQL)

| Champ Legacy | Type Legacy | Usage Moderne | Transformation |
|--------------|-------------|---------------|----------------|
| ID | int | - | Ignoré |
| INDEXKEY | varchar(10) | Clé de mapping | Dictionnaire de conversion |
| DESCRIPTION | varchar(255) | Documentation | Référence pour mapping |
| CATEGORY | varchar(50) | device_type.category | Groupement logique |

#### ES_VERSION → firmware_versions
**Table Source:** ES_VERSION (SQL Server)
**Table Cible:** firmware_versions (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | version_number | integer | Direct |
| DESCRIPTIF | varchar(255) | description | text | Direct |
| FILENAME | varchar(255) | filename | varchar(255) | Direct |
| SIZE | int | file_size | bigint | Direct |
| - | - | version_name | varchar(50) | 'V' + ID |
| - | - | checksum | varchar(64) | Calculé depuis fichier |
| - | - | is_active | boolean | true |
| - | - | created_at | timestamptz | NOW() |

#### ES_VERSIONMACHINE → firmware_deployments
**Table Source:** ES_VERSIONMACHINE (SQL Server)
**Table Cible:** firmware_deployments (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| MACHINE_ID | int | machine_id | UUID | Lookup |
| DATEACTION | datetime | started_at | timestamptz | Direct |
| VERSION | varchar(255) | firmware_version_id | UUID | Lookup ES_VERSION |
| ISOK | bit | status | varchar(20) | 'completed' si true, 'failed' si false |
| LASTINDEXCALL | int | - | - | Ignoré |
| - | - | progress_percentage | integer | 100 si ISOK |
| - | - | completed_at | timestamptz | started_at si ISOK |

#### ES_PHONE → notification_contacts
**Table Source:** ES_PHONE (SQL Server)
**Table Cible:** notification_contacts (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| USER_ID | int | user_id | UUID | Lookup ES_USER |
| PHONE | varchar(20) | contact_value | varchar(255) | Validation format |
| NOM | varchar(255) | display_name | varchar(255) | Direct |
| SENDMAIL | bit | - | - | Ignoré (type SMS) |
| - | - | type | varchar(20) | 'sms' |
| - | - | is_verified | boolean | false |
| - | - | is_active | boolean | true |
| - | - | preferences | jsonb | '{}' |

#### ES_SMSSEND → notifications
**Table Source:** ES_SMSSEND (SQL Server)
**Table Cible:** notifications (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| PHONE_ID | int | contact_id | UUID | Lookup ES_PHONE |
| DATESEND | datetime | sent_at | timestamptz | Direct |
| MESSAGE | varchar(160) | message | text | Direct |
| - | - | type | varchar(20) | 'sms' |
| - | - | status | varchar(20) | 'sent' |
| - | - | subject | varchar(255) | NULL |

#### ES_CLEMACHINE → product_keys
**Table Source:** ES_CLEMACHINE (SQL Server)
**Table Cible:** product_keys (PostgreSQL)

| Champ Legacy | Type Legacy | Champ Moderne | Type Moderne | Transformation |
|--------------|-------------|---------------|--------------|----------------|
| ID | int | - | - | Remplacé par UUID |
| CLE | varchar(255) | activation_key | varchar(32) | Validation format |
| MACHINE_ID | int | machine_id | UUID | Lookup si non NULL |
| - | - | product_type | varchar(50) | 'essensys_box' |
| - | - | is_used | boolean | machine_id IS NOT NULL |
| - | - | used_at | timestamptz | NOW() si utilisé |

### Nouvelles Tables Sans Équivalent Legacy

#### devices
**Nouvelle table pour structurer les appareils**
- Création d'un device "system" par machine pour les états globaux
- Types d'appareils basés sur l'analyse des ACTIONTYPE/DATAINDEX
- Configuration par défaut selon le type

#### user_sessions
**Nouvelle table pour la gestion JWT**
- Remplace les sessions ASP.NET
- Stockage des refresh tokens
- Gestion de l'expiration

#### audit_logs
**Nouvelle table pour la traçabilité**
- Pas d'équivalent legacy
- Logging des actions utilisateur
- Conformité RGPD

### Règles de Nettoyage des Données

#### Validation et Nettoyage
1. **Emails:** Validation format + unicité
2. **Téléphones:** Validation format international
3. **Codes postaux:** Validation selon pays
4. **Clés d'activation:** Format 32 caractères alphanumériques
5. **Dates:** Conversion timezone + validation cohérence

#### Données Corrompues
1. **Utilisateurs sans email:** Exclusion
2. **Machines sans clé:** Génération clé temporaire
3. **Actions malformées:** Logging + exclusion
4. **États incohérents:** Valeurs par défaut

#### Déduplication
1. **Emails en double:** Garder le plus récent
2. **Clés d'activation dupliquées:** Régénération
3. **Numéros de série identiques:** Suffixe numérique

### Contraintes d'Intégrité

#### Contraintes Référentielles
- user_machines.user_id → users.id (CASCADE DELETE)
- user_machines.machine_id → machines.id (CASCADE DELETE)
- actions.machine_id → machines.id (CASCADE DELETE)
- device_states.device_id → devices.id (CASCADE DELETE)

#### Contraintes Métier
- users.email UNIQUE et format valide
- machines.activation_key UNIQUE et format 32 chars
- machines.serial_number UNIQUE
- actions.status IN ('pending', 'sent', 'executed', 'failed')

#### Index de Performance
- users(email) - authentification
- machines(activation_key) - activation boîtiers
- actions(machine_id, status) - requêtes fréquentes
- device_states(device_id, timestamp DESC) - historique
- audit_logs(timestamp DESC) - requêtes chronologiques

### Estimation des Volumes

#### Données Legacy Typiques
- ES_USER: ~1,000 utilisateurs
- ES_MACHINE: ~1,200 machines
- ES_ACTION: ~50,000 actions (historique)
- ES_STATE: ~100,000 états
- ES_STATEINDEX: ~1,000,000 valeurs d'état
- ES_ACTIONINDEX: ~200,000 paramètres d'action

#### Données Migrées Estimées
- users: ~950 utilisateurs (après nettoyage)
- machines: ~1,150 machines (après validation)
- user_machines: ~1,150 relations
- actions: ~45,000 actions (après nettoyage)
- device_states: ~95,000 états agrégés
- devices: ~1,150 devices système + futurs devices réels

### Stratégie de Validation Post-Migration

#### Contrôles de Cohérence
1. **Comptages:** Vérification nombre d'enregistrements
2. **Intégrité référentielle:** Validation des FK
3. **Données critiques:** Vérification utilisateurs actifs
4. **Fonctionnalités:** Test connexion + actions de base

#### Tests de Performance
1. **Requêtes fréquentes:** Temps de réponse < 100ms
2. **Authentification:** < 50ms
3. **Récupération actions:** < 200ms
4. **Historique états:** < 500ms

Cette documentation servira de référence pour l'implémentation des scripts de migration et la validation des données migrées.

## 4.2 Scripts de Migration des Données

### Architecture des Scripts de Migration

Les scripts de migration sont organisés en plusieurs phases pour assurer la sécurité et la traçabilité :

1. **Phase 1:** Extraction et validation des données legacy
2. **Phase 2:** Transformation et nettoyage
3. **Phase 3:** Insertion dans le nouveau schéma
4. **Phase 4:** Validation et contrôles d'intégrité

### Scripts d'Extraction des Données Legacy

#### Script 1: Extraction des Utilisateurs
```sql
-- extract_users.sql
-- Extraction des utilisateurs valides depuis SQL Server legacy

SELECT 
    ID as legacy_id,
    LOWER(TRIM(MAIL)) as email,
    PASSWORD as legacy_password_hash,
    TRIM(PRENOM) as first_name,
    TRIM(NOM) as last_name,
    TRIM(ADR1) as address_line1,
    NULLIF(TRIM(ADR2), '') as address_line2,
    CP as postal_code,
    TRIM(VILLE) as city,
    NULLIF(TRIM(PHONE), '') as phone,
    TRIM(QUESTION) as security_question,
    REPONSE as legacy_answer_hash,
    COALESCE(SENDINFOS, 0) as marketing_consent,
    COALESCE(ISVALID, 0) as is_active,
    COALESCE(DATECREATION, GETDATE()) as created_at,
    NULLIF(LASTACCESS, '1900-01-01') as last_login,
    MACHINE_ID as legacy_machine_id
FROM ES_USER
WHERE MAIL IS NOT NULL 
    AND TRIM(MAIL) != ''
    AND MAIL LIKE '%@%'
    AND (OBSOLETE IS NULL OR OBSOLETE = 0)
ORDER BY ID;
```

#### Script 2: Extraction des Machines
```sql
-- extract_machines.sql
-- Extraction des machines actives depuis SQL Server legacy

SELECT 
    ID as legacy_id,
    COALESCE(NOSERIE, 'UNKNOWN_' + CAST(ID AS VARCHAR)) as serial_number,
    PKEY as activation_key,
    COALESCE(HASHEDPKEY, '') as legacy_key_hash,
    COALESCE(VERSION, 'V0') as firmware_version,
    COALESCE(ISACTIVE, 1) as is_active,
    COALESCE(AUTORISEALARME, 0) as alarm_enabled,
    COALESCE(DATECREATION, GETDATE()) as created_at,
    COALESCE(DATEMODIFICATION, GETDATE()) as updated_at
FROM ES_MACHINE
WHERE PKEY IS NOT NULL 
    AND LEN(TRIM(PKEY)) >= 8
ORDER BY ID;
```

#### Script 3: Extraction des Actions
```sql
-- extract_actions.sql
-- Extraction des actions avec leurs paramètres

SELECT 
    a.ID as legacy_action_id,
    a.DATECREATION as created_at,
    a.GUID as legacy_guid,
    a.ISDONE as is_done,
    a.ACTIONTYPE as action_type,
    a.ACTIONINFO as action_info,
    a.MACHINE_ID as legacy_machine_id,
    -- Agrégation des paramètres d'action
    STUFF((
        SELECT ',' + CAST(ai.DATAINDEX_ID AS VARCHAR) + ':' + ai.VALUE
        FROM ES_ACTIONINDEX ai
        WHERE ai.ACTION_ID = a.ID
        FOR XML PATH('')
    ), 1, 1, '') as action_parameters
FROM ES_ACTION a
WHERE a.MACHINE_ID IS NOT NULL
ORDER BY a.DATECREATION DESC;
```

#### Script 4: Extraction des États
```sql
-- extract_states.sql
-- Extraction des états avec leurs valeurs

SELECT 
    s.ID as legacy_state_id,
    s.MACHINE_ID as legacy_machine_id,
    s.DATECREATION as timestamp,
    s.VERSION as firmware_version,
    -- Agrégation des valeurs d'état en JSON-like
    STUFF((
        SELECT ',' + di.INDEXKEY + ':' + si.VALUE
        FROM ES_STATEINDEX si
        JOIN ES_DATAINDEX di ON si.DATAINDEX_ID = di.ID
        WHERE si.STATE_ID = s.ID
        FOR XML PATH('')
    ), 1, 1, '') as state_values
FROM ES_STATE s
WHERE s.MACHINE_ID IS NOT NULL
ORDER BY s.DATECREATION DESC;
```

### Scripts de Transformation et Nettoyage

#### Script de Transformation des Utilisateurs
```sql
-- transform_users.sql
-- Transformation et nettoyage des données utilisateur

CREATE TEMP TABLE temp_users_cleaned AS
SELECT 
    uuid_generate_v4() as id,
    email,
    legacy_password_hash,
    first_name,
    last_name,
    address_line1,
    address_line2,
    postal_code,
    city,
    phone,
    security_question,
    legacy_answer_hash,
    marketing_consent::boolean,
    is_active::boolean,
    created_at::timestamptz,
    last_login::timestamptz,
    legacy_id,
    legacy_machine_id,
    -- Validation et nettoyage
    CASE 
        WHEN email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' THEN true
        ELSE false
    END as email_valid,
    CASE 
        WHEN length(trim(first_name)) > 0 AND length(trim(last_name)) > 0 THEN true
        ELSE false
    END as name_valid,
    CASE 
        WHEN postal_code ~ '^[0-9]{5}$' THEN true
        ELSE false
    END as postal_code_valid
FROM temp_users_extracted
WHERE email IS NOT NULL;

-- Déduplication des emails
CREATE TEMP TABLE temp_users_deduplicated AS
SELECT DISTINCT ON (email) *
FROM temp_users_cleaned
WHERE email_valid = true 
    AND name_valid = true
    AND postal_code_valid = true
ORDER BY email, created_at DESC;

-- Rapport de nettoyage
SELECT 
    'Utilisateurs extraits' as etape,
    count(*) as nombre
FROM temp_users_extracted
UNION ALL
SELECT 
    'Utilisateurs après nettoyage',
    count(*)
FROM temp_users_cleaned
WHERE email_valid = true AND name_valid = true
UNION ALL
SELECT 
    'Utilisateurs après déduplication',
    count(*)
FROM temp_users_deduplicated;
```

#### Script de Transformation des Machines
```sql
-- transform_machines.sql
-- Transformation et validation des machines

CREATE TEMP TABLE temp_machines_cleaned AS
SELECT 
    uuid_generate_v4() as id,
    serial_number,
    activation_key,
    legacy_key_hash,
    firmware_version,
    is_active::boolean,
    alarm_enabled::boolean,
    'Europe/Paris' as timezone,
    created_at::timestamptz,
    updated_at::timestamptz,
    0 as connection_count,
    legacy_id,
    -- Validation
    CASE 
        WHEN length(trim(activation_key)) = 32 
             AND activation_key ~ '^[A-Z0-9-]+$' THEN true
        ELSE false
    END as key_valid,
    CASE 
        WHEN length(trim(serial_number)) > 0 THEN true
        ELSE false
    END as serial_valid
FROM temp_machines_extracted;

-- Génération de nouvelles clés pour les clés invalides
UPDATE temp_machines_cleaned 
SET activation_key = generate_activation_key()
WHERE key_valid = false;

-- Fonction de génération de clé d'activation
CREATE OR REPLACE FUNCTION generate_activation_key() 
RETURNS VARCHAR(32) AS $$
DECLARE
    chars VARCHAR(36) := 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    result VARCHAR(32) := '';
    i INTEGER;
BEGIN
    FOR i IN 1..32 LOOP
        result := result || substr(chars, floor(random() * length(chars) + 1)::int, 1);
    END LOOP;
    RETURN result;
END;
$$ LANGUAGE plpgsql;

-- Déduplication des numéros de série
UPDATE temp_machines_cleaned 
SET serial_number = serial_number || '_' || row_number() OVER (PARTITION BY serial_number ORDER BY created_at)
WHERE serial_number IN (
    SELECT serial_number 
    FROM temp_machines_cleaned 
    GROUP BY serial_number 
    HAVING count(*) > 1
);
```

#### Script de Transformation des Actions
```sql
-- transform_actions.sql
-- Transformation des actions vers le nouveau format

CREATE TEMP TABLE temp_actions_cleaned AS
SELECT 
    uuid_generate_v4() as id,
    m.id as machine_id,
    created_at::timestamptz,
    CASE 
        WHEN is_done = 1 THEN 'executed'
        ELSE 'pending'
    END as status,
    -- Mapping des types d'actions
    CASE action_type
        WHEN 'ALARME' THEN 'set_alarm_state'
        WHEN 'CHAUFFAGE' THEN 'set_temperature'
        WHEN 'VOLET' THEN 'set_shutter_position'
        WHEN 'CUMULUS' THEN 'set_water_heater_mode'
        WHEN 'STORE' THEN 'set_awning_position'
        ELSE 'unknown_action'
    END as action_type,
    -- Conversion des paramètres en JSON
    convert_action_parameters(action_parameters, action_type) as payload,
    5 as priority,
    CASE WHEN is_done = 1 THEN created_at ELSE NULL END as sent_at,
    CASE WHEN is_done = 1 THEN created_at ELSE NULL END as executed_at,
    0 as retry_count,
    3 as max_retries,
    legacy_action_id
FROM temp_actions_extracted a
JOIN temp_machines_cleaned m ON m.legacy_id = a.legacy_machine_id
WHERE action_type IS NOT NULL;

-- Fonction de conversion des paramètres d'action
CREATE OR REPLACE FUNCTION convert_action_parameters(
    params TEXT, 
    action_type VARCHAR(50)
) RETURNS JSONB AS $$
DECLARE
    result JSONB := '{}';
    param_pair TEXT;
    key_val TEXT[];
    index_key VARCHAR(10);
    value VARCHAR(255);
BEGIN
    -- Parsing des paramètres "key1:value1,key2:value2"
    IF params IS NOT NULL THEN
        FOREACH param_pair IN ARRAY string_to_array(params, ',') LOOP
            key_val := string_to_array(param_pair, ':');
            IF array_length(key_val, 1) = 2 THEN
                index_key := trim(key_val[1]);
                value := trim(key_val[2]);
                
                -- Mapping des index vers noms explicites
                CASE index_key
                    WHEN '407' THEN result := result || jsonb_build_object('alarm_remote_access', value::boolean);
                    WHEN '920' THEN result := result || jsonb_build_object('button_bp1_state', value::boolean);
                    WHEN '100' THEN result := result || jsonb_build_object('target_temperature', value::numeric);
                    WHEN '200' THEN result := result || jsonb_build_object('position_percentage', value::numeric);
                    WHEN '300' THEN result := result || jsonb_build_object('mode', value);
                    ELSE result := result || jsonb_build_object('index_' || index_key, value);
                END CASE;
            END IF;
        END LOOP;
    END IF;
    
    RETURN result;
END;
$$ LANGUAGE plpgsql;
```

### Scripts d'Insertion dans le Nouveau Schéma

#### Insertion des Utilisateurs
```sql
-- insert_users.sql
-- Insertion des utilisateurs dans le nouveau schéma

INSERT INTO users (
    id, email, password_hash, first_name, last_name,
    address_line1, address_line2, postal_code, city, phone,
    security_question, security_answer_hash, marketing_consent,
    is_active, email_verified, created_at, last_login
)
SELECT 
    id,
    email,
    legacy_password_hash, -- Sera re-hashé en bcrypt lors de la première connexion
    first_name,
    last_name,
    address_line1,
    address_line2,
    postal_code,
    city,
    phone,
    security_question,
    legacy_answer_hash,
    marketing_consent,
    is_active,
    false as email_verified, -- Nécessitera une vérification
    created_at,
    last_login
FROM temp_users_deduplicated;

-- Marquer les mots de passe pour migration
ALTER TABLE users ADD COLUMN IF NOT EXISTS password_needs_migration BOOLEAN DEFAULT true;
UPDATE users SET password_needs_migration = true WHERE password_hash LIKE '%'; -- Tous les utilisateurs migrés
```

#### Insertion des Machines
```sql
-- insert_machines.sql
-- Insertion des machines dans le nouveau schéma

INSERT INTO machines (
    id, serial_number, activation_key, activation_key_hash,
    firmware_version, is_active, alarm_enabled, timezone,
    created_at, updated_at, connection_count
)
SELECT 
    id,
    serial_number,
    activation_key,
    crypt(activation_key, gen_salt('bf', 12)) as activation_key_hash, -- Nouveau hash bcrypt
    firmware_version,
    is_active,
    alarm_enabled,
    timezone,
    created_at,
    updated_at,
    connection_count
FROM temp_machines_cleaned;
```

#### Insertion des Relations Utilisateur-Machine
```sql
-- insert_user_machines.sql
-- Création des relations utilisateur-machine

INSERT INTO user_machines (user_id, machine_id, role, created_at)
SELECT 
    u.id as user_id,
    m.id as machine_id,
    'owner' as role,
    NOW() as created_at
FROM temp_users_deduplicated tu
JOIN users u ON u.email = tu.email
JOIN temp_machines_cleaned tm ON tm.legacy_id = tu.legacy_machine_id
JOIN machines m ON m.activation_key = tm.activation_key
WHERE tu.legacy_machine_id IS NOT NULL;
```

#### Insertion des Actions
```sql
-- insert_actions.sql
-- Insertion des actions transformées

INSERT INTO actions (
    id, machine_id, action_type, payload, status, priority,
    created_at, sent_at, executed_at, retry_count, max_retries
)
SELECT 
    id, machine_id, action_type, payload, status, priority,
    created_at, sent_at, executed_at, retry_count, max_retries
FROM temp_actions_cleaned
WHERE machine_id IS NOT NULL;
```

### Scripts de Validation et Contrôles d'Intégrité

#### Validation des Comptages
```sql
-- validate_counts.sql
-- Validation des comptages après migration

CREATE TEMP TABLE migration_report AS
SELECT 
    'users' as table_name,
    (SELECT count(*) FROM temp_users_extracted) as legacy_count,
    (SELECT count(*) FROM users) as migrated_count,
    (SELECT count(*) FROM temp_users_deduplicated) as cleaned_count
UNION ALL
SELECT 
    'machines',
    (SELECT count(*) FROM temp_machines_extracted),
    (SELECT count(*) FROM machines),
    (SELECT count(*) FROM temp_machines_cleaned)
UNION ALL
SELECT 
    'user_machines',
    (SELECT count(DISTINCT MACHINE_ID) FROM temp_users_extracted WHERE MACHINE_ID IS NOT NULL),
    (SELECT count(*) FROM user_machines),
    (SELECT count(*) FROM user_machines)
UNION ALL
SELECT 
    'actions',
    (SELECT count(*) FROM temp_actions_extracted),
    (SELECT count(*) FROM actions),
    (SELECT count(*) FROM temp_actions_cleaned);

-- Affichage du rapport
SELECT 
    table_name,
    legacy_count,
    cleaned_count,
    migrated_count,
    CASE 
        WHEN migrated_count = cleaned_count THEN 'OK'
        ELSE 'ERREUR: ' || (cleaned_count - migrated_count)::text || ' enregistrements manquants'
    END as status
FROM migration_report;
```

#### Validation de l'Intégrité Référentielle
```sql
-- validate_integrity.sql
-- Validation de l'intégrité référentielle

-- Vérification des FK user_machines
SELECT 'user_machines orphelins' as check_name, count(*) as count
FROM user_machines um
LEFT JOIN users u ON um.user_id = u.id
LEFT JOIN machines m ON um.machine_id = m.id
WHERE u.id IS NULL OR m.id IS NULL

UNION ALL

-- Vérification des FK actions
SELECT 'actions orphelines', count(*)
FROM actions a
LEFT JOIN machines m ON a.machine_id = m.id
WHERE m.id IS NULL

UNION ALL

-- Vérification unicité emails
SELECT 'emails dupliqués', count(*) - count(DISTINCT email)
FROM users

UNION ALL

-- Vérification unicité clés d'activation
SELECT 'clés activation dupliquées', count(*) - count(DISTINCT activation_key)
FROM machines;
```

#### Tests de Performance Post-Migration
```sql
-- performance_tests.sql
-- Tests de performance sur les requêtes critiques

-- Test 1: Authentification utilisateur
EXPLAIN ANALYZE
SELECT id, email, password_hash, is_active
FROM users 
WHERE email = 'test@example.com';

-- Test 2: Récupération des machines d'un utilisateur
EXPLAIN ANALYZE
SELECT m.id, m.serial_number, m.is_active, m.last_connection
FROM machines m
JOIN user_machines um ON m.id = um.machine_id
JOIN users u ON um.user_id = u.id
WHERE u.email = 'test@example.com';

-- Test 3: Actions en attente pour une machine
EXPLAIN ANALYZE
SELECT id, action_type, payload, created_at
FROM actions
WHERE machine_id = 'uuid-example' 
    AND status = 'pending'
ORDER BY priority ASC, created_at ASC;

-- Test 4: Historique des états d'un appareil
EXPLAIN ANALYZE
SELECT timestamp, state_data
FROM device_states
WHERE device_id = 'uuid-example'
ORDER BY timestamp DESC
LIMIT 100;
```

### Script de Nettoyage Post-Migration
```sql
-- cleanup_migration.sql
-- Nettoyage des tables temporaires après migration réussie

DROP TABLE IF EXISTS temp_users_extracted;
DROP TABLE IF EXISTS temp_users_cleaned;
DROP TABLE IF EXISTS temp_users_deduplicated;
DROP TABLE IF EXISTS temp_machines_extracted;
DROP TABLE IF EXISTS temp_machines_cleaned;
DROP TABLE IF EXISTS temp_actions_extracted;
DROP TABLE IF EXISTS temp_actions_cleaned;
DROP TABLE IF EXISTS temp_states_extracted;
DROP TABLE IF EXISTS migration_report;

-- Suppression des fonctions temporaires
DROP FUNCTION IF EXISTS generate_activation_key();
DROP FUNCTION IF EXISTS convert_action_parameters(TEXT, VARCHAR);

-- Mise à jour des statistiques PostgreSQL
ANALYZE users;
ANALYZE machines;
ANALYZE user_machines;
ANALYZE actions;
ANALYZE device_states;

-- Rapport final
SELECT 
    schemaname,
    tablename,
    n_tup_ins as insertions,
    n_tup_upd as updates,
    n_tup_del as deletions
FROM pg_stat_user_tables
WHERE schemaname = 'public'
ORDER BY tablename;
```

### Procédure d'Exécution des Scripts

#### Ordre d'Exécution
1. **Extraction:** `extract_*.sql` (sur base legacy)
2. **Transformation:** `transform_*.sql` (sur base cible)
3. **Insertion:** `insert_*.sql` (sur base cible)
4. **Validation:** `validate_*.sql` (sur base cible)
5. **Nettoyage:** `cleanup_migration.sql` (sur base cible)

#### Script de Coordination
```bash
#!/bin/bash
# migrate_data.sh
# Script de coordination de la migration

set -e  # Arrêt en cas d'erreur

echo "=== Début de la migration des données Essensys ==="
echo "Date: $(date)"

# Configuration
LEGACY_DB="Server=legacy-server;Database=EssensysDB;Trusted_Connection=true;"
TARGET_DB="postgresql://user:pass@localhost:5432/essensys_modern"

# Phase 1: Extraction
echo "Phase 1: Extraction des données legacy..."
sqlcmd -S legacy-server -d EssensysDB -i extract_users.sql -o users_extracted.csv
sqlcmd -S legacy-server -d EssensysDB -i extract_machines.sql -o machines_extracted.csv
sqlcmd -S legacy-server -d EssensysDB -i extract_actions.sql -o actions_extracted.csv
sqlcmd -S legacy-server -d EssensysDB -i extract_states.sql -o states_extracted.csv

# Phase 2: Import dans PostgreSQL
echo "Phase 2: Import des données extraites..."
psql $TARGET_DB -c "\COPY temp_users_extracted FROM 'users_extracted.csv' CSV HEADER"
psql $TARGET_DB -c "\COPY temp_machines_extracted FROM 'machines_extracted.csv' CSV HEADER"
psql $TARGET_DB -c "\COPY temp_actions_extracted FROM 'actions_extracted.csv' CSV HEADER"
psql $TARGET_DB -c "\COPY temp_states_extracted FROM 'states_extracted.csv' CSV HEADER"

# Phase 3: Transformation
echo "Phase 3: Transformation et nettoyage..."
psql $TARGET_DB -f transform_users.sql
psql $TARGET_DB -f transform_machines.sql
psql $TARGET_DB -f transform_actions.sql

# Phase 4: Insertion
echo "Phase 4: Insertion dans le nouveau schéma..."
psql $TARGET_DB -f insert_users.sql
psql $TARGET_DB -f insert_machines.sql
psql $TARGET_DB -f insert_user_machines.sql
psql $TARGET_DB -f insert_actions.sql

# Phase 5: Validation
echo "Phase 5: Validation et contrôles..."
psql $TARGET_DB -f validate_counts.sql
psql $TARGET_DB -f validate_integrity.sql
psql $TARGET_DB -f performance_tests.sql

# Phase 6: Nettoyage
echo "Phase 6: Nettoyage..."
psql $TARGET_DB -f cleanup_migration.sql

echo "=== Migration terminée avec succès ==="
echo "Date: $(date)"
```

Ces scripts fournissent une approche complète et sécurisée pour migrer les données du système legacy vers le nouveau schéma PostgreSQL, avec validation et contrôles d'intégrité à chaque étape.
## 4.3 Stratégie de Basculement

### Vue d'Ensemble de la Stratégie

La stratégie de basculement vise à minimiser l'interruption de service tout en assurant une transition sécurisée du système legacy vers le nouveau système. L'approche privilégiée est un basculement progressif avec période de coexistence contrôlée.

### Approches de Basculement Évaluées

#### Option 1: Big Bang (Non Recommandée)
**Description:** Arrêt complet du système legacy et basculement immédiat vers le nouveau système.

**Avantages:**
- Transition rapide (quelques heures)
- Pas de synchronisation complexe
- Coûts de développement réduits

**Inconvénients:**
- Risque élevé d'interruption prolongée
- Pas de possibilité de rollback rapide
- Impact utilisateur maximal
- Difficile à tester en conditions réelles

**Verdict:** Rejetée en raison des risques

#### Option 2: Basculement Progressif par Fonctionnalité (Recommandée)
**Description:** Migration progressive des fonctionnalités avec coexistence temporaire des deux systèmes.

**Avantages:**
- Risque maîtrisé et distribué
- Possibilité de rollback par fonctionnalité
- Validation en conditions réelles
- Impact utilisateur minimal

**Inconvénients:**
- Complexité de synchronisation
- Période de coexistence plus longue
- Coûts de développement plus élevés

**Verdict:** Approche recommandée

#### Option 3: Basculement par Utilisateur/Machine
**Description:** Migration progressive par groupes d'utilisateurs ou de machines.

**Avantages:**
- Test avec utilisateurs pilotes
- Rollback par groupe
- Apprentissage progressif

**Inconvénients:**
- Complexité de routage
- Deux interfaces utilisateur simultanées
- Synchronisation des données complexe

**Verdict:** Complémentaire à l'option 2

### Stratégie de Basculement Retenue

#### Phase 1: Préparation et Tests (Semaines -4 à -1)

**Semaine -4: Préparation Infrastructure**
```bash
# Déploiement environnement de staging
docker-compose -f docker-compose.staging.yml up -d

# Configuration du proxy de basculement
nginx -t && nginx -s reload

# Tests de charge sur le nouveau système
artillery run load-test-config.yml

# Validation des sauvegardes
pg_dump essensys_modern > backup_pre_migration.sql
```

**Semaine -3: Migration des Données de Test**
```bash
# Exécution migration sur données anonymisées
./migrate_data.sh --environment=staging --dry-run

# Validation de l'intégrité
psql -f validate_migration.sql

# Tests de performance comparatifs
./performance_comparison.sh legacy staging
```

**Semaine -2: Tests Utilisateur**
```bash
# Déploiement version bêta pour utilisateurs pilotes
kubectl apply -f k8s/beta-deployment.yml

# Configuration du routage A/B
# 5% du trafic vers le nouveau système
curl -X POST api-gateway/config/routing \
  -d '{"new_system_percentage": 5}'
```

**Semaine -1: Répétition Générale**
```bash
# Simulation complète du basculement
./simulate_cutover.sh --full-rehearsal

# Validation des procédures de rollback
./test_rollback_procedures.sh

# Formation équipe support
./training_session.sh --team=support
```

#### Phase 2: Basculement Progressif (Semaines 0 à 4)

**Semaine 0: Basculement des APIs Lecture Seule**

*Jour J-1 (Vendredi):*
```bash
# Notification utilisateurs - maintenance programmée
curl -X POST notification-service/broadcast \
  -d '{"message": "Maintenance programmée samedi 2h-6h", "channels": ["email", "sms"]}'

# Préparation des scripts de basculement
chmod +x cutover_readonly_apis.sh
./validate_cutover_scripts.sh
```

*Jour J (Samedi 2h-6h):*
```bash
# 2h00: Début de la fenêtre de maintenance
echo "$(date): Début basculement APIs lecture" >> cutover.log

# 2h15: Sauvegarde complète système legacy
pg_dump essensys_legacy > backup_$(date +%Y%m%d_%H%M).sql

# 2h30: Migration des données
./migrate_data.sh --production --apis-readonly
if [ $? -ne 0 ]; then
    echo "ERREUR: Migration échouée, rollback automatique"
    ./rollback.sh --phase=readonly-apis
    exit 1
fi

# 3h00: Basculement du routage pour APIs lecture
nginx_config="
upstream legacy_backend {
    server legacy-app:8080;
}
upstream modern_backend {
    server modern-app:3000;
}

# Routage APIs lecture vers nouveau système
location /api/devices/status {
    proxy_pass http://modern_backend;
}
location /api/machines/info {
    proxy_pass http://modern_backend;
}
# Autres APIs restent sur legacy
location /api/ {
    proxy_pass http://legacy_backend;
}
"
echo "$nginx_config" > /etc/nginx/sites-available/essensys
nginx -t && nginx -s reload

# 3h30: Tests de validation
./validate_readonly_apis.sh
if [ $? -ne 0 ]; then
    echo "ERREUR: Validation échouée"
    ./rollback.sh --phase=readonly-apis
    exit 1
fi

# 4h00: Monitoring intensif
./start_intensive_monitoring.sh --duration=2h

# 6h00: Fin de maintenance - notification utilisateurs
curl -X POST notification-service/broadcast \
  -d '{"message": "Maintenance terminée - service restauré", "channels": ["email"]}'
```

**Semaine 1: Basculement Interface Utilisateur Web**

*Préparation (Lundi-Vendredi):*
```bash
# Tests A/B avec 10% des utilisateurs
./configure_ab_testing.sh --percentage=10 --feature=web-ui

# Collecte feedback utilisateurs
./collect_user_feedback.sh --duration=5days

# Ajustements basés sur le feedback
./apply_ui_improvements.sh
```

*Basculement (Samedi):*
```bash
# Basculement progressif de l'interface web
# 25% des utilisateurs → nouveau système
./gradual_cutover.sh --component=web-ui --percentage=25

# Monitoring des métriques utilisateur
./monitor_user_metrics.sh --metrics="session_duration,error_rate,satisfaction"

# Si métriques OK → augmentation à 50% puis 100%
if ./check_metrics_threshold.sh; then
    ./gradual_cutover.sh --component=web-ui --percentage=50
    sleep 3600  # Attendre 1h
    ./gradual_cutover.sh --component=web-ui --percentage=100
fi
```

**Semaine 2: Basculement APIs d'Écriture Utilisateur**

```bash
# Basculement des APIs de commande utilisateur
# Synchronisation bidirectionnelle temporaire
./enable_bidirectional_sync.sh --apis="user-commands"

# Basculement progressif
./gradual_cutover.sh --component=user-write-apis --percentage=25
./monitor_data_consistency.sh --duration=24h

# Validation cohérence des données
./validate_data_consistency.sh
if [ $? -eq 0 ]; then
    ./gradual_cutover.sh --component=user-write-apis --percentage=100
fi
```

**Semaine 3: Basculement APIs Boîtiers IoT**

```bash
# Phase critique - APIs utilisées par les boîtiers
# Maintien compatibilité legacy obligatoire

# Déploiement adaptateur de compatibilité
kubectl apply -f k8s/legacy-compatibility-adapter.yml

# Test avec boîtiers pilotes (firmware récent)
./test_iot_compatibility.sh --devices="pilot_devices.list"

# Basculement progressif par version firmware
./cutover_by_firmware_version.sh --min-version="V10"

# Monitoring spécifique IoT
./monitor_iot_connectivity.sh --alert-threshold=95%
```

**Semaine 4: Finalisation et Nettoyage**

```bash
# Arrêt de la synchronisation bidirectionnelle
./disable_bidirectional_sync.sh

# Basculement des dernières APIs legacy
./cutover_remaining_apis.sh

# Validation finale
./final_validation.sh --comprehensive

# Désactivation système legacy (mode lecture seule)
./deactivate_legacy_system.sh --mode=readonly
```

#### Phase 3: Consolidation (Semaines 5 à 8)

**Semaine 5-6: Monitoring Intensif**
```bash
# Surveillance 24/7 des métriques critiques
./continuous_monitoring.sh --duration=2weeks --alerts=critical

# Optimisation des performances
./performance_optimization.sh --based-on=production-metrics

# Support utilisateur renforcé
./enhanced_user_support.sh --duration=2weeks
```

**Semaine 7-8: Nettoyage et Documentation**
```bash
# Sauvegarde finale du système legacy
./final_legacy_backup.sh

# Arrêt définitif du système legacy
./shutdown_legacy_system.sh --confirm=yes

# Documentation des leçons apprises
./document_lessons_learned.sh

# Formation équipe maintenance
./maintenance_team_training.sh
```

### Scripts de Rollback

#### Rollback Automatique
```bash
#!/bin/bash
# rollback.sh
# Script de rollback automatique en cas de problème

set -e

PHASE=$1
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

echo "=== DÉBUT ROLLBACK PHASE: $PHASE ==="
echo "Timestamp: $TIMESTAMP"

case $PHASE in
    "readonly-apis")
        echo "Rollback APIs lecture seule..."
        # Restauration configuration nginx legacy
        cp /etc/nginx/sites-available/essensys.legacy /etc/nginx/sites-available/essensys
        nginx -t && nginx -s reload
        
        # Arrêt services modernes
        docker-compose -f docker-compose.modern.yml down
        
        # Redémarrage services legacy
        systemctl restart essensys-legacy
        ;;
        
    "web-ui")
        echo "Rollback interface web..."
        # Retour routage 100% vers legacy
        ./configure_ab_testing.sh --percentage=0 --feature=web-ui
        
        # Nettoyage cache CDN
        curl -X POST cdn-api/purge/all
        ;;
        
    "user-write-apis")
        echo "Rollback APIs écriture utilisateur..."
        # Arrêt synchronisation
        ./disable_bidirectional_sync.sh
        
        # Restauration données depuis backup
        psql essensys_legacy < backup_before_write_apis.sql
        
        # Redirection trafic vers legacy
        ./gradual_cutover.sh --component=user-write-apis --percentage=0
        ;;
        
    "iot-apis")
        echo "Rollback APIs IoT..."
        # Critique - rollback immédiat
        kubectl delete -f k8s/legacy-compatibility-adapter.yml
        
        # Restauration routage legacy pour tous les boîtiers
        ./restore_legacy_iot_routing.sh --all-devices
        
        # Notification équipe technique
        curl -X POST slack-webhook \
          -d '{"text": "🚨 ROLLBACK IoT APIs effectué - intervention requise"}'
        ;;
        
    *)
        echo "Phase de rollback inconnue: $PHASE"
        exit 1
        ;;
esac

# Validation post-rollback
./validate_rollback.sh --phase=$PHASE

# Notification
echo "=== ROLLBACK $PHASE TERMINÉ ==="
curl -X POST notification-service/broadcast \
  -d '{"message": "Rollback effectué - service restauré sur système legacy", "priority": "high"}'
```

#### Tests de Rollback
```bash
#!/bin/bash
# test_rollback_procedures.sh
# Tests des procédures de rollback

echo "=== Tests des procédures de rollback ==="

# Test rollback APIs lecture
echo "Test 1: Rollback APIs lecture..."
./simulate_failure.sh --component=readonly-apis
./rollback.sh readonly-apis
./validate_legacy_functionality.sh --apis=readonly
echo "✓ Test rollback APIs lecture OK"

# Test rollback interface web
echo "Test 2: Rollback interface web..."
./simulate_failure.sh --component=web-ui
./rollback.sh web-ui
./validate_legacy_functionality.sh --component=web-ui
echo "✓ Test rollback interface web OK"

# Test rollback APIs écriture
echo "Test 3: Rollback APIs écriture..."
./simulate_failure.sh --component=user-write-apis
./rollback.sh user-write-apis
./validate_legacy_functionality.sh --apis=write
echo "✓ Test rollback APIs écriture OK"

# Test rollback IoT (critique)
echo "Test 4: Rollback IoT..."
./simulate_failure.sh --component=iot-apis --severity=critical
./rollback.sh iot-apis
./validate_iot_connectivity.sh --all-devices
echo "✓ Test rollback IoT OK"

echo "=== Tous les tests de rollback réussis ==="
```

### Procédures de Communication

#### Communication Utilisateurs
```bash
#!/bin/bash
# user_communication.sh
# Gestion de la communication utilisateur

PHASE=$1
MESSAGE_TYPE=$2

case $PHASE in
    "pre-cutover")
        case $MESSAGE_TYPE in
            "announcement")
                MESSAGE="🔄 Modernisation Essensys prévue le $(date -d '+1 week' '+%d/%m/%Y'). 
                Améliorations: interface plus rapide, nouvelles fonctionnalités, sécurité renforcée.
                Aucune action requise de votre part."
                ;;
            "reminder")
                MESSAGE="⏰ Rappel: Maintenance Essensys demain de 2h à 6h. 
                Service temporairement interrompu. 
                Vos boîtiers continueront de fonctionner normalement."
                ;;
        esac
        ;;
        
    "during-cutover")
        case $MESSAGE_TYPE in
            "start")
                MESSAGE="🔧 Maintenance en cours (2h-6h). 
                Interface web temporairement indisponible. 
                Vos équipements restent opérationnels."
                ;;
            "progress")
                MESSAGE="⚙️ Maintenance en cours - Étape 2/4 terminée. 
                Fin prévue à 6h. Merci de votre patience."
                ;;
            "completion")
                MESSAGE="✅ Maintenance terminée! 
                Nouvelle interface disponible avec améliorations. 
                Contactez le support si besoin."
                ;;
        esac
        ;;
        
    "post-cutover")
        case $MESSAGE_TYPE in
            "success")
                MESSAGE="🎉 Migration réussie! 
                Découvrez votre nouvelle interface Essensys. 
                Guide utilisateur: https://help.essensys.com/nouveau"
                ;;
            "issue")
                MESSAGE="⚠️ Problème détecté - retour temporaire à l'ancienne version. 
                Nos équipes travaillent sur une solution. 
                Nous vous tiendrons informés."
                ;;
        esac
        ;;
esac

# Envoi multi-canal
curl -X POST notification-service/send \
  -H "Content-Type: application/json" \
  -d "{
    \"message\": \"$MESSAGE\",
    \"channels\": [\"email\", \"sms\", \"in-app\"],
    \"priority\": \"normal\",
    \"audience\": \"all_users\"
  }"

# Log de la communication
echo "$(date): $PHASE - $MESSAGE_TYPE - $MESSAGE" >> communication.log
```

#### Communication Équipe Technique
```bash
#!/bin/bash
# technical_communication.sh
# Communication équipe technique

ALERT_TYPE=$1
DETAILS=$2

case $ALERT_TYPE in
    "cutover-start")
        slack_message="🚀 Début basculement Essensys - Phase: $DETAILS"
        priority="normal"
        ;;
    "cutover-success")
        slack_message="✅ Basculement réussi - Phase: $DETAILS"
        priority="normal"
        ;;
    "cutover-failure")
        slack_message="🚨 ÉCHEC basculement - Phase: $DETAILS - Rollback en cours"
        priority="critical"
        ;;
    "performance-alert")
        slack_message="⚠️ Alerte performance - $DETAILS"
        priority="high"
        ;;
    "rollback-initiated")
        slack_message="🔄 Rollback initié - Raison: $DETAILS"
        priority="critical"
        ;;
esac

# Slack
curl -X POST $SLACK_WEBHOOK_URL \
  -H "Content-Type: application/json" \
  -d "{
    \"text\": \"$slack_message\",
    \"channel\": \"#essensys-migration\",
    \"username\": \"Migration Bot\"
  }"

# Email équipe technique si critique
if [ "$priority" = "critical" ]; then
    curl -X POST email-service/send \
      -d "{
        \"to\": [\"tech-team@company.com\"],
        \"subject\": \"URGENT: Essensys Migration - $ALERT_TYPE\",
        \"body\": \"$slack_message\\n\\nDétails: $DETAILS\"
      }"
fi

# PagerDuty si critique
if [ "$priority" = "critical" ]; then
    curl -X POST https://events.pagerduty.com/v2/enqueue \
      -H "Content-Type: application/json" \
      -d "{
        \"routing_key\": \"$PAGERDUTY_ROUTING_KEY\",
        \"event_action\": \"trigger\",
        \"payload\": {
          \"summary\": \"Essensys Migration Alert: $ALERT_TYPE\",
          \"source\": \"migration-system\",
          \"severity\": \"critical\",
          \"custom_details\": {\"details\": \"$DETAILS\"}
        }
      }"
fi
```

### Tests en Environnement de Staging

#### Configuration Environnement de Staging
```yaml
# docker-compose.staging.yml
version: '3.8'
services:
  # Base de données legacy (copie production)
  legacy-db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=StrongPassword123
    volumes:
      - ./staging-data/legacy-db:/var/opt/mssql/data
    ports:
      - "1433:1433"

  # Application legacy
  legacy-app:
    image: essensys/legacy-app:latest
    depends_on:
      - legacy-db
    environment:
      - ConnectionString=Server=legacy-db;Database=EssensysDB;User=sa;Password=StrongPassword123
    ports:
      - "8080:80"

  # Base de données moderne
  modern-db:
    image: postgres:15
    environment:
      - POSTGRES_DB=essensys_modern
      - POSTGRES_USER=essensys
      - POSTGRES_PASSWORD=SecurePassword456
    volumes:
      - ./staging-data/modern-db:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  # Application moderne
  modern-app:
    image: essensys/modern-app:latest
    depends_on:
      - modern-db
    environment:
      - DATABASE_URL=postgresql://essensys:SecurePassword456@modern-db:5432/essensys_modern
      - NODE_ENV=staging
    ports:
      - "3000:3000"

  # Proxy de basculement
  nginx-proxy:
    image: nginx:alpine
    volumes:
      - ./nginx-staging.conf:/etc/nginx/nginx.conf
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - legacy-app
      - modern-app

  # Monitoring
  prometheus:
    image: prom/prometheus
    volumes:
      - ./prometheus-staging.yml:/etc/prometheus/prometheus.yml
    ports:
      - "9090:9090"

  grafana:
    image: grafana/grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin123
    ports:
      - "3001:3000"
```

#### Tests de Charge Staging
```bash
#!/bin/bash
# staging_load_tests.sh
# Tests de charge sur environnement de staging

echo "=== Tests de charge environnement staging ==="

# Test 1: Charge normale (100 utilisateurs simultanés)
echo "Test 1: Charge normale..."
artillery run --config load-test-normal.yml --output normal-load-report.json

# Test 2: Pic de charge (500 utilisateurs simultanés)
echo "Test 2: Pic de charge..."
artillery run --config load-test-peak.yml --output peak-load-report.json

# Test 3: Charge IoT (1000 boîtiers simulés)
echo "Test 3: Charge IoT..."
artillery run --config load-test-iot.yml --output iot-load-report.json

# Test 4: Test de basculement sous charge
echo "Test 4: Basculement sous charge..."
artillery run --config load-test-cutover.yml &
LOAD_TEST_PID=$!

sleep 30  # Laisser la charge s'établir
./simulate_cutover.sh --component=readonly-apis
sleep 60  # Laisser le basculement se stabiliser
./simulate_cutover.sh --component=web-ui

wait $LOAD_TEST_PID

# Analyse des résultats
echo "=== Analyse des résultats ==="
node analyze-load-test-results.js normal-load-report.json
node analyze-load-test-results.js peak-load-report.json
node analyze-load-test-results.js iot-load-report.json

echo "Tests de charge terminés - voir rapports détaillés"
```

Cette stratégie de basculement fournit un cadre complet et sécurisé pour la transition du système legacy vers le nouveau système, avec des procédures de rollback robustes et une communication appropriée à toutes les parties prenantes.
## 4.4 Tests de Validation des Données Migrées

### Framework de Validation

La validation des données migrées s'appuie sur un framework de tests automatisés multi-niveaux pour garantir l'intégrité, la cohérence et la complétude des données après migration.

### Tests d'Intégrité des Données

#### Test 1: Validation des Comptages
```sql
-- test_data_counts.sql
-- Validation des comptages entre legacy et moderne

CREATE OR REPLACE FUNCTION test_data_counts()
RETURNS TABLE(
    test_name VARCHAR(100),
    legacy_count BIGINT,
    modern_count BIGINT,
    status VARCHAR(20),
    details TEXT
) AS $$
BEGIN
    -- Test utilisateurs
    RETURN QUERY
    SELECT 
        'users_count'::VARCHAR(100),
        (SELECT count(*) FROM legacy_users_view)::BIGINT,
        (SELECT count(*) FROM users WHERE email_verified = true OR email_verified = false)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM users) >= (SELECT count(*) * 0.95 FROM legacy_users_view) 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Validation du nombre d\'utilisateurs migrés (tolérance 5%)'::TEXT;

    -- Test machines
    RETURN QUERY
    SELECT 
        'machines_count'::VARCHAR(100),
        (SELECT count(*) FROM legacy_machines_view)::BIGINT,
        (SELECT count(*) FROM machines)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM machines) >= (SELECT count(*) * 0.98 FROM legacy_machines_view)
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Validation du nombre de machines migrées (tolérance 2%)'::TEXT;

    -- Test relations utilisateur-machine
    RETURN QUERY
    SELECT 
        'user_machines_count'::VARCHAR(100),
        (SELECT count(DISTINCT MACHINE_ID) FROM legacy_users_view WHERE MACHINE_ID IS NOT NULL)::BIGINT,
        (SELECT count(*) FROM user_machines)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM user_machines) >= 
                 (SELECT count(DISTINCT MACHINE_ID) * 0.95 FROM legacy_users_view WHERE MACHINE_ID IS NOT NULL)
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Validation des relations utilisateur-machine'::TEXT;

    -- Test actions
    RETURN QUERY
    SELECT 
        'actions_count'::VARCHAR(100),
        (SELECT count(*) FROM legacy_actions_view WHERE DATECREATION >= NOW() - INTERVAL '6 months')::BIGINT,
        (SELECT count(*) FROM actions WHERE created_at >= NOW() - INTERVAL '6 months')::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM actions WHERE created_at >= NOW() - INTERVAL '6 months') >= 
                 (SELECT count(*) * 0.90 FROM legacy_actions_view WHERE DATECREATION >= NOW() - INTERVAL '6 months')
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Validation des actions récentes (6 derniers mois, tolérance 10%)'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_data_counts();
```

#### Test 2: Validation de l'Intégrité Référentielle
```sql
-- test_referential_integrity.sql
-- Tests d'intégrité référentielle

CREATE OR REPLACE FUNCTION test_referential_integrity()
RETURNS TABLE(
    test_name VARCHAR(100),
    violation_count BIGINT,
    status VARCHAR(20),
    details TEXT
) AS $$
BEGIN
    -- Test FK user_machines -> users
    RETURN QUERY
    SELECT 
        'user_machines_users_fk'::VARCHAR(100),
        (SELECT count(*) FROM user_machines um 
         LEFT JOIN users u ON um.user_id = u.id 
         WHERE u.id IS NULL)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM user_machines um 
                  LEFT JOIN users u ON um.user_id = u.id 
                  WHERE u.id IS NULL) = 0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Vérification FK user_machines.user_id -> users.id'::TEXT;

    -- Test FK user_machines -> machines
    RETURN QUERY
    SELECT 
        'user_machines_machines_fk'::VARCHAR(100),
        (SELECT count(*) FROM user_machines um 
         LEFT JOIN machines m ON um.machine_id = m.id 
         WHERE m.id IS NULL)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM user_machines um 
                  LEFT JOIN machines m ON um.machine_id = m.id 
                  WHERE m.id IS NULL) = 0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Vérification FK user_machines.machine_id -> machines.id'::TEXT;

    -- Test FK actions -> machines
    RETURN QUERY
    SELECT 
        'actions_machines_fk'::VARCHAR(100),
        (SELECT count(*) FROM actions a 
         LEFT JOIN machines m ON a.machine_id = m.id 
         WHERE m.id IS NULL)::BIGINT,
        CASE 
            WHEN (SELECT count(*) FROM actions a 
                  LEFT JOIN machines m ON a.machine_id = m.id 
                  WHERE m.id IS NULL) = 0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Vérification FK actions.machine_id -> machines.id'::TEXT;

    -- Test unicité emails
    RETURN QUERY
    SELECT 
        'users_email_unique'::VARCHAR(100),
        (SELECT count(*) - count(DISTINCT email) FROM users)::BIGINT,
        CASE 
            WHEN (SELECT count(*) - count(DISTINCT email) FROM users) = 0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Vérification unicité des emails utilisateur'::TEXT;

    -- Test unicité clés d'activation
    RETURN QUERY
    SELECT 
        'machines_activation_key_unique'::VARCHAR(100),
        (SELECT count(*) - count(DISTINCT activation_key) FROM machines)::BIGINT,
        CASE 
            WHEN (SELECT count(*) - count(DISTINCT activation_key) FROM machines) = 0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Vérification unicité des clés d\'activation'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_referential_integrity();
```

### Tests de Cohérence des Données

#### Test 3: Validation des Données Critiques
```sql
-- test_critical_data_consistency.sql
-- Tests de cohérence des données critiques

CREATE OR REPLACE FUNCTION test_critical_data_consistency()
RETURNS TABLE(
    test_name VARCHAR(100),
    expected_value TEXT,
    actual_value TEXT,
    status VARCHAR(20),
    details TEXT
) AS $$
DECLARE
    legacy_active_users BIGINT;
    modern_active_users BIGINT;
    legacy_active_machines BIGINT;
    modern_active_machines BIGINT;
BEGIN
    -- Comptage utilisateurs actifs
    SELECT count(*) INTO legacy_active_users 
    FROM legacy_users_view 
    WHERE ISVALID = 1 AND (OBSOLETE IS NULL OR OBSOLETE = 0);
    
    SELECT count(*) INTO modern_active_users 
    FROM users 
    WHERE is_active = true;

    RETURN QUERY
    SELECT 
        'active_users_consistency'::VARCHAR(100),
        legacy_active_users::TEXT,
        modern_active_users::TEXT,
        CASE 
            WHEN abs(legacy_active_users - modern_active_users) <= (legacy_active_users * 0.05)
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Cohérence utilisateurs actifs (tolérance 5%)'::TEXT;

    -- Comptage machines actives
    SELECT count(*) INTO legacy_active_machines 
    FROM legacy_machines_view 
    WHERE ISACTIVE = 1;
    
    SELECT count(*) INTO modern_active_machines 
    FROM machines 
    WHERE is_active = true;

    RETURN QUERY
    SELECT 
        'active_machines_consistency'::VARCHAR(100),
        legacy_active_machines::TEXT,
        modern_active_machines::TEXT,
        CASE 
            WHEN abs(legacy_active_machines - modern_active_machines) <= (legacy_active_machines * 0.02)
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Cohérence machines actives (tolérance 2%)'::TEXT;

    -- Test cohérence des actions récentes
    RETURN QUERY
    WITH legacy_recent_actions AS (
        SELECT count(*) as count
        FROM legacy_actions_view 
        WHERE DATECREATION >= NOW() - INTERVAL '24 hours'
    ),
    modern_recent_actions AS (
        SELECT count(*) as count
        FROM actions 
        WHERE created_at >= NOW() - INTERVAL '24 hours'
    )
    SELECT 
        'recent_actions_consistency'::VARCHAR(100),
        lra.count::TEXT,
        mra.count::TEXT,
        CASE 
            WHEN abs(lra.count - mra.count) <= (lra.count * 0.10)
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Cohérence actions récentes 24h (tolérance 10%)'::TEXT
    FROM legacy_recent_actions lra, modern_recent_actions mra;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_critical_data_consistency();
```

#### Test 4: Validation des Transformations de Données
```sql
-- test_data_transformations.sql
-- Tests de validation des transformations

CREATE OR REPLACE FUNCTION test_data_transformations()
RETURNS TABLE(
    test_name VARCHAR(100),
    sample_size BIGINT,
    success_count BIGINT,
    failure_count BIGINT,
    status VARCHAR(20),
    details TEXT
) AS $$
BEGIN
    -- Test transformation emails (minuscules)
    RETURN QUERY
    WITH email_transformation_test AS (
        SELECT 
            u.email as modern_email,
            LOWER(TRIM(lu.MAIL)) as expected_email,
            CASE WHEN u.email = LOWER(TRIM(lu.MAIL)) THEN 1 ELSE 0 END as is_correct
        FROM users u
        JOIN legacy_users_view lu ON u.email = LOWER(TRIM(lu.MAIL))
        LIMIT 1000
    )
    SELECT 
        'email_transformation'::VARCHAR(100),
        count(*)::BIGINT,
        sum(is_correct)::BIGINT,
        (count(*) - sum(is_correct))::BIGINT,
        CASE 
            WHEN sum(is_correct)::FLOAT / count(*) >= 0.99 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Transformation emails en minuscules (échantillon 1000, seuil 99%)'::TEXT
    FROM email_transformation_test;

    -- Test transformation clés d'activation
    RETURN QUERY
    WITH activation_key_test AS (
        SELECT 
            m.activation_key,
            lm.PKEY as legacy_key,
            CASE WHEN m.activation_key = lm.PKEY THEN 1 ELSE 0 END as is_correct
        FROM machines m
        JOIN legacy_machines_view lm ON m.activation_key = lm.PKEY
        LIMIT 1000
    )
    SELECT 
        'activation_key_preservation'::VARCHAR(100),
        count(*)::BIGINT,
        sum(is_correct)::BIGINT,
        (count(*) - sum(is_correct))::BIGINT,
        CASE 
            WHEN sum(is_correct)::FLOAT / count(*) = 1.0 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Préservation clés d\'activation (échantillon 1000, seuil 100%)'::TEXT
    FROM activation_key_test;

    -- Test transformation actions JSON
    RETURN QUERY
    WITH action_payload_test AS (
        SELECT 
            a.payload,
            CASE 
                WHEN jsonb_typeof(a.payload) = 'object' AND a.payload != '{}'::jsonb 
                THEN 1 ELSE 0 
            END as is_valid_json
        FROM actions a
        WHERE a.payload IS NOT NULL
        LIMIT 1000
    )
    SELECT 
        'action_payload_json'::VARCHAR(100),
        count(*)::BIGINT,
        sum(is_valid_json)::BIGINT,
        (count(*) - sum(is_valid_json))::BIGINT,
        CASE 
            WHEN sum(is_valid_json)::FLOAT / count(*) >= 0.95 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Transformation payload actions en JSON valide (échantillon 1000, seuil 95%)'::TEXT
    FROM action_payload_test;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_data_transformations();
```

### Tests de Complétude

#### Test 5: Validation de la Complétude des Données
```sql
-- test_data_completeness.sql
-- Tests de complétude des données

CREATE OR REPLACE FUNCTION test_data_completeness()
RETURNS TABLE(
    test_name VARCHAR(100),
    total_records BIGINT,
    complete_records BIGINT,
    completeness_rate NUMERIC(5,2),
    status VARCHAR(20),
    details TEXT
) AS $$
BEGIN
    -- Test complétude utilisateurs
    RETURN QUERY
    WITH user_completeness AS (
        SELECT 
            count(*) as total,
            count(CASE WHEN 
                email IS NOT NULL AND 
                first_name IS NOT NULL AND 
                last_name IS NOT NULL AND 
                address_line1 IS NOT NULL AND 
                postal_code IS NOT NULL AND 
                city IS NOT NULL 
            THEN 1 END) as complete
        FROM users
    )
    SELECT 
        'user_data_completeness'::VARCHAR(100),
        total::BIGINT,
        complete::BIGINT,
        (complete::NUMERIC / total * 100)::NUMERIC(5,2),
        CASE 
            WHEN complete::NUMERIC / total >= 0.95 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Complétude données utilisateur (champs obligatoires, seuil 95%)'::TEXT
    FROM user_completeness;

    -- Test complétude machines
    RETURN QUERY
    WITH machine_completeness AS (
        SELECT 
            count(*) as total,
            count(CASE WHEN 
                serial_number IS NOT NULL AND 
                activation_key IS NOT NULL AND 
                firmware_version IS NOT NULL 
            THEN 1 END) as complete
        FROM machines
    )
    SELECT 
        'machine_data_completeness'::VARCHAR(100),
        total::BIGINT,
        complete::BIGINT,
        (complete::NUMERIC / total * 100)::NUMERIC(5,2),
        CASE 
            WHEN complete::NUMERIC / total >= 0.98 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Complétude données machine (champs obligatoires, seuil 98%)'::TEXT
    FROM machine_completeness;

    -- Test complétude actions
    RETURN QUERY
    WITH action_completeness AS (
        SELECT 
            count(*) as total,
            count(CASE WHEN 
                machine_id IS NOT NULL AND 
                action_type IS NOT NULL AND 
                payload IS NOT NULL AND 
                status IS NOT NULL 
            THEN 1 END) as complete
        FROM actions
    )
    SELECT 
        'action_data_completeness'::VARCHAR(100),
        total::BIGINT,
        complete::BIGINT,
        (complete::NUMERIC / total * 100)::NUMERIC(5,2),
        CASE 
            WHEN complete::NUMERIC / total >= 0.90 
            THEN 'PASS'::VARCHAR(20)
            ELSE 'FAIL'::VARCHAR(20)
        END,
        'Complétude données action (champs obligatoires, seuil 90%)'::TEXT
    FROM action_completeness;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_data_completeness();
```

### Tests de Performance

#### Test 6: Validation des Performances des Requêtes
```sql
-- test_query_performance.sql
-- Tests de performance des requêtes critiques

CREATE OR REPLACE FUNCTION test_query_performance()
RETURNS TABLE(
    test_name VARCHAR(100),
    execution_time_ms NUMERIC(10,2),
    threshold_ms INTEGER,
    status VARCHAR(20),
    details TEXT
) AS $$
DECLARE
    start_time TIMESTAMP;
    end_time TIMESTAMP;
    duration_ms NUMERIC(10,2);
BEGIN
    -- Test 1: Authentification utilisateur
    start_time := clock_timestamp();
    PERFORM id, email, password_hash, is_active
    FROM users 
    WHERE email = 'test@example.com';
    end_time := clock_timestamp();
    duration_ms := EXTRACT(EPOCH FROM (end_time - start_time)) * 1000;

    RETURN QUERY
    SELECT 
        'user_authentication_query'::VARCHAR(100),
        duration_ms,
        50::INTEGER,
        CASE WHEN duration_ms <= 50 THEN 'PASS'::VARCHAR(20) ELSE 'FAIL'::VARCHAR(20) END,
        'Requête authentification utilisateur (seuil 50ms)'::TEXT;

    -- Test 2: Récupération machines utilisateur
    start_time := clock_timestamp();
    PERFORM m.id, m.serial_number, m.is_active, m.last_connection
    FROM machines m
    JOIN user_machines um ON m.id = um.machine_id
    JOIN users u ON um.user_id = u.id
    WHERE u.email = 'test@example.com';
    end_time := clock_timestamp();
    duration_ms := EXTRACT(EPOCH FROM (end_time - start_time)) * 1000;

    RETURN QUERY
    SELECT 
        'user_machines_query'::VARCHAR(100),
        duration_ms,
        100::INTEGER,
        CASE WHEN duration_ms <= 100 THEN 'PASS'::VARCHAR(20) ELSE 'FAIL'::VARCHAR(20) END,
        'Requête machines utilisateur (seuil 100ms)'::TEXT;

    -- Test 3: Actions en attente
    start_time := clock_timestamp();
    PERFORM id, action_type, payload, created_at
    FROM actions
    WHERE machine_id = (SELECT id FROM machines LIMIT 1)
        AND status = 'pending'
    ORDER BY priority ASC, created_at ASC
    LIMIT 50;
    end_time := clock_timestamp();
    duration_ms := EXTRACT(EPOCH FROM (end_time - start_time)) * 1000;

    RETURN QUERY
    SELECT 
        'pending_actions_query'::VARCHAR(100),
        duration_ms,
        200::INTEGER,
        CASE WHEN duration_ms <= 200 THEN 'PASS'::VARCHAR(20) ELSE 'FAIL'::VARCHAR(20) END,
        'Requête actions en attente (seuil 200ms)'::TEXT;

    -- Test 4: Historique états (simulation)
    start_time := clock_timestamp();
    PERFORM timestamp, state_data
    FROM device_states
    WHERE device_id = (SELECT id FROM devices LIMIT 1)
    ORDER BY timestamp DESC
    LIMIT 100;
    end_time := clock_timestamp();
    duration_ms := EXTRACT(EPOCH FROM (end_time - start_time)) * 1000;

    RETURN QUERY
    SELECT 
        'device_history_query'::VARCHAR(100),
        duration_ms,
        500::INTEGER,
        CASE WHEN duration_ms <= 500 THEN 'PASS'::VARCHAR(20) ELSE 'FAIL'::VARCHAR(20) END,
        'Requête historique états appareil (seuil 500ms)'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- Exécution du test
SELECT * FROM test_query_performance();
```

### Tests Automatisés avec Framework de Test

#### Script de Test Principal
```bash
#!/bin/bash
# run_migration_validation_tests.sh
# Script principal de validation des données migrées

set -e

echo "=== Tests de Validation des Données Migrées ==="
echo "Date: $(date)"
echo "Base de données: $DATABASE_URL"

# Configuration
TEST_RESULTS_DIR="./test-results/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$TEST_RESULTS_DIR"

# Fonction de logging
log_test_result() {
    local test_name=$1
    local status=$2
    local details=$3
    echo "$(date '+%Y-%m-%d %H:%M:%S') | $test_name | $status | $details" >> "$TEST_RESULTS_DIR/test_log.txt"
}

# Test 1: Comptages
echo "Exécution Test 1: Validation des comptages..."
psql $DATABASE_URL -f test_data_counts.sql -o "$TEST_RESULTS_DIR/test_counts_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "data_counts" "SUCCESS" "Comptages validés"
    echo "✓ Test comptages: RÉUSSI"
else
    log_test_result "data_counts" "FAILURE" "Erreur dans les comptages"
    echo "✗ Test comptages: ÉCHEC"
fi

# Test 2: Intégrité référentielle
echo "Exécution Test 2: Intégrité référentielle..."
psql $DATABASE_URL -f test_referential_integrity.sql -o "$TEST_RESULTS_DIR/test_integrity_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "referential_integrity" "SUCCESS" "Intégrité référentielle validée"
    echo "✓ Test intégrité: RÉUSSI"
else
    log_test_result "referential_integrity" "FAILURE" "Violations d'intégrité détectées"
    echo "✗ Test intégrité: ÉCHEC"
fi

# Test 3: Cohérence des données
echo "Exécution Test 3: Cohérence des données..."
psql $DATABASE_URL -f test_critical_data_consistency.sql -o "$TEST_RESULTS_DIR/test_consistency_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "data_consistency" "SUCCESS" "Cohérence des données validée"
    echo "✓ Test cohérence: RÉUSSI"
else
    log_test_result "data_consistency" "FAILURE" "Incohérences détectées"
    echo "✗ Test cohérence: ÉCHEC"
fi

# Test 4: Transformations
echo "Exécution Test 4: Transformations de données..."
psql $DATABASE_URL -f test_data_transformations.sql -o "$TEST_RESULTS_DIR/test_transformations_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "data_transformations" "SUCCESS" "Transformations validées"
    echo "✓ Test transformations: RÉUSSI"
else
    log_test_result "data_transformations" "FAILURE" "Erreurs de transformation"
    echo "✗ Test transformations: ÉCHEC"
fi

# Test 5: Complétude
echo "Exécution Test 5: Complétude des données..."
psql $DATABASE_URL -f test_data_completeness.sql -o "$TEST_RESULTS_DIR/test_completeness_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "data_completeness" "SUCCESS" "Complétude validée"
    echo "✓ Test complétude: RÉUSSI"
else
    log_test_result "data_completeness" "FAILURE" "Données incomplètes détectées"
    echo "✗ Test complétude: ÉCHEC"
fi

# Test 6: Performance
echo "Exécution Test 6: Performance des requêtes..."
psql $DATABASE_URL -f test_query_performance.sql -o "$TEST_RESULTS_DIR/test_performance_results.txt"
if [ $? -eq 0 ]; then
    log_test_result "query_performance" "SUCCESS" "Performance validée"
    echo "✓ Test performance: RÉUSSI"
else
    log_test_result "query_performance" "FAILURE" "Performance insuffisante"
    echo "✗ Test performance: ÉCHEC"
fi

# Génération du rapport final
echo "Génération du rapport final..."
cat > "$TEST_RESULTS_DIR/validation_report.md" << EOF
# Rapport de Validation des Données Migrées

**Date:** $(date)
**Base de données:** $DATABASE_URL

## Résumé des Tests

$(cat "$TEST_RESULTS_DIR/test_log.txt" | awk -F'|' '
BEGIN { success=0; failure=0 }
/SUCCESS/ { success++ }
/FAILURE/ { failure++ }
END { 
    print "- Tests réussis: " success
    print "- Tests échoués: " failure
    print "- Total: " (success + failure)
    if (failure == 0) {
        print "\n**Statut global: ✅ VALIDATION RÉUSSIE**"
    } else {
        print "\n**Statut global: ❌ VALIDATION ÉCHOUÉE**"
    }
}')

## Détails des Tests

### 1. Validation des Comptages
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_counts_results.txt")
\`\`\`

### 2. Intégrité Référentielle
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_integrity_results.txt")
\`\`\`

### 3. Cohérence des Données
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_consistency_results.txt")
\`\`\`

### 4. Transformations de Données
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_transformations_results.txt")
\`\`\`

### 5. Complétude des Données
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_completeness_results.txt")
\`\`\`

### 6. Performance des Requêtes
\`\`\`
$(cat "$TEST_RESULTS_DIR/test_performance_results.txt")
\`\`\`

## Recommandations

$(if grep -q "FAILURE" "$TEST_RESULTS_DIR/test_log.txt"; then
    echo "⚠️ **Des échecs ont été détectés. Actions recommandées:**"
    echo "1. Analyser les résultats détaillés ci-dessus"
    echo "2. Corriger les problèmes identifiés"
    echo "3. Relancer les tests de validation"
    echo "4. Ne pas procéder au basculement tant que tous les tests ne sont pas au vert"
else
    echo "✅ **Tous les tests sont au vert. La migration peut être considérée comme réussie.**"
    echo "1. Procéder au basculement selon la stratégie définie"
    echo "2. Maintenir la surveillance des métriques"
    echo "3. Conserver ce rapport pour audit"
fi)
EOF

echo "=== Tests de validation terminés ==="
echo "Rapport disponible: $TEST_RESULTS_DIR/validation_report.md"

# Vérification du statut global
if grep -q "FAILURE" "$TEST_RESULTS_DIR/test_log.txt"; then
    echo "❌ VALIDATION ÉCHOUÉE - Intervention requise"
    exit 1
else
    echo "✅ VALIDATION RÉUSSIE - Migration validée"
    exit 0
fi
```

#### Tests de Régression Fonctionnelle
```bash
#!/bin/bash
# functional_regression_tests.sh
# Tests de régression fonctionnelle post-migration

echo "=== Tests de Régression Fonctionnelle ==="

# Test 1: Authentification utilisateur
echo "Test 1: Authentification utilisateur..."
curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "testpassword"}' \
  -w "Status: %{http_code}, Time: %{time_total}s\n" \
  -o auth_response.json

if [ $(jq -r '.accessToken' auth_response.json) != "null" ]; then
    echo "✓ Authentification: RÉUSSI"
    ACCESS_TOKEN=$(jq -r '.accessToken' auth_response.json)
else
    echo "✗ Authentification: ÉCHEC"
    exit 1
fi

# Test 2: Récupération des machines
echo "Test 2: Récupération des machines..."
curl -X GET http://localhost:3000/api/user/machines \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -w "Status: %{http_code}, Time: %{time_total}s\n" \
  -o machines_response.json

if [ $(jq '. | length' machines_response.json) -gt 0 ]; then
    echo "✓ Récupération machines: RÉUSSI"
    MACHINE_ID=$(jq -r '.[0].id' machines_response.json)
else
    echo "✗ Récupération machines: ÉCHEC"
    exit 1
fi

# Test 3: Envoi d'une commande
echo "Test 3: Envoi d'une commande..."
curl -X POST http://localhost:3000/api/machines/$MACHINE_ID/actions \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"actionType": "set_temperature", "payload": {"targetTemperature": 22}}' \
  -w "Status: %{http_code}, Time: %{time_total}s\n" \
  -o action_response.json

if [ $(jq -r '.actionId' action_response.json) != "null" ]; then
    echo "✓ Envoi commande: RÉUSSI"
else
    echo "✗ Envoi commande: ÉCHEC"
    exit 1
fi

# Test 4: Compatibilité API boîtiers (legacy)
echo "Test 4: Compatibilité API boîtiers..."
curl -X GET http://localhost:3000/api/myactions \
  -H "X-Machine-Token: test-machine-token" \
  -w "Status: %{http_code}, Time: %{time_total}s\n" \
  -o iot_response.json

if [ $(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/api/myactions -H "X-Machine-Token: test-machine-token") -eq 200 ]; then
    echo "✓ Compatibilité IoT: RÉUSSI"
else
    echo "✗ Compatibilité IoT: ÉCHEC"
    exit 1
fi

echo "=== Tous les tests de régression réussis ==="
```

Ces tests de validation fournissent une couverture complète pour s'assurer que la migration des données s'est déroulée correctement et que le nouveau système fonctionne comme attendu.
## 4.5 Synchronisation Pendant la Transition

### Vue d'Ensemble de la Synchronisation

Pendant la période de coexistence entre le système legacy et le nouveau système, une synchronisation bidirectionnelle des données est nécessaire pour maintenir la cohérence. Cette synchronisation doit être robuste, performante et capable de gérer les conflits.

### Architecture de Synchronisation

#### Composants de Synchronisation

```mermaid
graph TB
    subgraph "Système Legacy"
        LegacyDB[(SQL Server Legacy)]
        LegacyApp[Application ASP.NET]
    end
    
    subgraph "Système Moderne"
        ModernDB[(PostgreSQL Moderne)]
        ModernApp[Application Node.js]
    end
    
    subgraph "Infrastructure de Synchronisation"
        SyncService[Service de Synchronisation]
        ConflictResolver[Résolveur de Conflits]
        AuditLog[Journal d'Audit]
        MessageQueue[File de Messages]
    end
    
    LegacyDB --> SyncService
    ModernDB --> SyncService
    SyncService --> ConflictResolver
    SyncService --> AuditLog
    SyncService --> MessageQueue
    
    LegacyApp --> MessageQueue
    ModernApp --> MessageQueue
```

#### Service de Synchronisation Principal
```typescript
// sync-service/src/SynchronizationService.ts
import { EventEmitter } from 'events';
import { Logger } from 'winston';
import { LegacyDataSource } from './datasources/LegacyDataSource';
import { ModernDataSource } from './datasources/ModernDataSource';
import { ConflictResolver } from './ConflictResolver';
import { AuditLogger } from './AuditLogger';

export class SynchronizationService extends EventEmitter {
    private legacyDS: LegacyDataSource;
    private modernDS: ModernDataSource;
    private conflictResolver: ConflictResolver;
    private auditLogger: AuditLogger;
    private logger: Logger;
    private isRunning: boolean = false;

    constructor(config: SyncConfig) {
        super();
        this.legacyDS = new LegacyDataSource(config.legacy);
        this.modernDS = new ModernDataSource(config.modern);
        this.conflictResolver = new ConflictResolver(config.conflictResolution);
        this.auditLogger = new AuditLogger(config.audit);
        this.logger = config.logger;
    }

    async start(): Promise<void> {
        this.logger.info('Démarrage du service de synchronisation');
        this.isRunning = true;

        // Démarrage des listeners de changements
        await this.startChangeListeners();
        
        // Synchronisation initiale
        await this.performInitialSync();
        
        // Démarrage de la synchronisation périodique
        this.startPeriodicSync();
        
        this.emit('started');
    }

    async stop(): Promise<void> {
        this.logger.info('Arrêt du service de synchronisation');
        this.isRunning = false;
        
        await this.stopChangeListeners();
        this.emit('stopped');
    }

    private async startChangeListeners(): Promise<void> {
        // Écoute des changements sur le système legacy
        this.legacyDS.on('dataChanged', async (change: DataChange) => {
            await this.handleLegacyChange(change);
        });

        // Écoute des changements sur le système moderne
        this.modernDS.on('dataChanged', async (change: DataChange) => {
            await this.handleModernChange(change);
        });
    }

    private async handleLegacyChange(change: DataChange): Promise<void> {
        try {
            this.logger.debug(`Changement legacy détecté: ${change.table}:${change.id}`);
            
            // Transformation des données legacy vers moderne
            const modernData = await this.transformLegacyToModern(change);
            
            // Vérification des conflits
            const conflict = await this.detectConflict(modernData);
            if (conflict) {
                const resolution = await this.conflictResolver.resolve(conflict);
                await this.applyConflictResolution(resolution);
            } else {
                // Application directe du changement
                await this.modernDS.applyChange(modernData);
            }
            
            // Audit
            await this.auditLogger.logSync({
                source: 'legacy',
                target: 'modern',
                change: change,
                status: 'success'
            });
            
        } catch (error) {
            this.logger.error(`Erreur synchronisation legacy->moderne: ${error.message}`);
            await this.auditLogger.logSync({
                source: 'legacy',
                target: 'modern',
                change: change,
                status: 'error',
                error: error.message
            });
        }
    }

    private async handleModernChange(change: DataChange): Promise<void> {
        try {
            this.logger.debug(`Changement moderne détecté: ${change.table}:${change.id}`);
            
            // Transformation des données moderne vers legacy
            const legacyData = await this.transformModernToLegacy(change);
            
            // Vérification des conflits
            const conflict = await this.detectConflict(legacyData);
            if (conflict) {
                const resolution = await this.conflictResolver.resolve(conflict);
                await this.applyConflictResolution(resolution);
            } else {
                // Application directe du changement
                await this.legacyDS.applyChange(legacyData);
            }
            
            // Audit
            await this.auditLogger.logSync({
                source: 'modern',
                target: 'legacy',
                change: change,
                status: 'success'
            });
            
        } catch (error) {
            this.logger.error(`Erreur synchronisation moderne->legacy: ${error.message}`);
            await this.auditLogger.logSync({
                source: 'modern',
                target: 'legacy',
                change: change,
                status: 'error',
                error: error.message
            });
        }
    }
}

interface DataChange {
    table: string;
    id: string;
    operation: 'INSERT' | 'UPDATE' | 'DELETE';
    data: any;
    timestamp: Date;
    source: 'legacy' | 'modern';
}

interface SyncConfig {
    legacy: LegacyConfig;
    modern: ModernConfig;
    conflictResolution: ConflictResolutionConfig;
    audit: AuditConfig;
    logger: Logger;
}
```

### Détection et Résolution des Conflits

#### Résolveur de Conflits
```typescript
// sync-service/src/ConflictResolver.ts
export class ConflictResolver {
    private strategies: Map<string, ConflictResolutionStrategy>;
    private logger: Logger;

    constructor(config: ConflictResolutionConfig) {
        this.logger = config.logger;
        this.strategies = new Map();
        
        // Stratégies par type de données
        this.strategies.set('users', new UserConflictStrategy());
        this.strategies.set('machines', new MachineConflictStrategy());
        this.strategies.set('actions', new ActionConflictStrategy());
        this.strategies.set('default', new TimestampBasedStrategy());
    }

    async resolve(conflict: DataConflict): Promise<ConflictResolution> {
        const strategy = this.strategies.get(conflict.table) || 
                        this.strategies.get('default');
        
        this.logger.info(`Résolution conflit ${conflict.table}:${conflict.id} avec stratégie ${strategy.name}`);
        
        const resolution = await strategy.resolve(conflict);
        
        // Log de la résolution
        await this.logConflictResolution(conflict, resolution);
        
        return resolution;
    }

    private async logConflictResolution(
        conflict: DataConflict, 
        resolution: ConflictResolution
    ): Promise<void> {
        this.logger.warn(`Conflit résolu: ${conflict.table}:${conflict.id}`, {
            conflict: conflict,
            resolution: resolution.strategy,
            winner: resolution.winningSource
        });
    }
}

// Stratégie de résolution basée sur timestamp
class TimestampBasedStrategy implements ConflictResolutionStrategy {
    name = 'timestamp-based';

    async resolve(conflict: DataConflict): Promise<ConflictResolution> {
        // La donnée la plus récente gagne
        const legacyTimestamp = new Date(conflict.legacyData.updated_at || conflict.legacyData.created_at);
        const modernTimestamp = new Date(conflict.modernData.updated_at || conflict.modernData.created_at);
        
        if (modernTimestamp > legacyTimestamp) {
            return {
                strategy: this.name,
                winningSource: 'modern',
                winningData: conflict.modernData,
                action: 'update_legacy'
            };
        } else {
            return {
                strategy: this.name,
                winningSource: 'legacy',
                winningData: conflict.legacyData,
                action: 'update_modern'
            };
        }
    }
}

// Stratégie spécifique aux utilisateurs
class UserConflictStrategy implements ConflictResolutionStrategy {
    name = 'user-specific';

    async resolve(conflict: DataConflict): Promise<ConflictResolution> {
        // Pour les utilisateurs, privilégier les données du système moderne
        // sauf pour les mots de passe (garder legacy jusqu'à migration complète)
        
        const mergedData = {
            ...conflict.legacyData,
            ...conflict.modernData,
            // Garder le mot de passe legacy si pas encore migré
            password_hash: conflict.legacyData.password_needs_migration ? 
                          conflict.legacyData.password_hash : 
                          conflict.modernData.password_hash
        };

        return {
            strategy: this.name,
            winningSource: 'merged',
            winningData: mergedData,
            action: 'update_both'
        };
    }
}

interface DataConflict {
    table: string;
    id: string;
    legacyData: any;
    modernData: any;
    conflictFields: string[];
}

interface ConflictResolution {
    strategy: string;
    winningSource: 'legacy' | 'modern' | 'merged';
    winningData: any;
    action: 'update_legacy' | 'update_modern' | 'update_both' | 'manual_review';
}
```

### Mécanismes de Synchronisation

#### Synchronisation en Temps Réel
```typescript
// sync-service/src/RealtimeSync.ts
export class RealtimeSync {
    private changeStreams: Map<string, ChangeStream>;
    private messageQueue: MessageQueue;
    private logger: Logger;

    constructor(config: RealtimeSyncConfig) {
        this.changeStreams = new Map();
        this.messageQueue = new MessageQueue(config.queue);
        this.logger = config.logger;
    }

    async startLegacyChangeStream(): Promise<void> {
        // Utilisation de SQL Server Change Tracking
        const legacyStream = new SqlServerChangeStream({
            connectionString: process.env.LEGACY_DB_CONNECTION,
            tables: ['ES_USER', 'ES_MACHINE', 'ES_ACTION', 'ES_STATE'],
            pollInterval: 5000 // 5 secondes
        });

        legacyStream.on('change', async (change) => {
            await this.messageQueue.publish('legacy-changes', change);
        });

        await legacyStream.start();
        this.changeStreams.set('legacy', legacyStream);
    }

    async startModernChangeStream(): Promise<void> {
        // Utilisation de PostgreSQL LISTEN/NOTIFY
        const modernStream = new PostgresChangeStream({
            connectionString: process.env.MODERN_DB_CONNECTION,
            tables: ['users', 'machines', 'actions', 'device_states']
        });

        modernStream.on('change', async (change) => {
            await this.messageQueue.publish('modern-changes', change);
        });

        await modernStream.start();
        this.changeStreams.set('modern', modernStream);
    }
}

// Implémentation SQL Server Change Tracking
class SqlServerChangeStream extends EventEmitter {
    private connection: any;
    private pollInterval: number;
    private isRunning: boolean = false;
    private lastSyncVersion: number = 0;

    constructor(config: SqlServerStreamConfig) {
        super();
        this.connection = new sql.ConnectionPool(config.connectionString);
        this.pollInterval = config.pollInterval;
    }

    async start(): Promise<void> {
        await this.connection.connect();
        
        // Activer Change Tracking si pas déjà fait
        await this.enableChangeTracking();
        
        this.isRunning = true;
        this.startPolling();
    }

    private async enableChangeTracking(): Promise<void> {
        // Activer Change Tracking sur la base de données
        await this.connection.request().query(`
            IF NOT EXISTS (SELECT * FROM sys.change_tracking_databases WHERE database_id = DB_ID())
            BEGIN
                ALTER DATABASE CURRENT SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)
            END
        `);

        // Activer Change Tracking sur les tables
        const tables = ['ES_USER', 'ES_MACHINE', 'ES_ACTION', 'ES_STATE'];
        for (const table of tables) {
            await this.connection.request().query(`
                IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('${table}'))
                BEGIN
                    ALTER TABLE ${table} ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)
                END
            `);
        }
    }

    private startPolling(): void {
        const poll = async () => {
            if (!this.isRunning) return;

            try {
                const changes = await this.getChanges();
                for (const change of changes) {
                    this.emit('change', change);
                }
            } catch (error) {
                this.emit('error', error);
            }

            setTimeout(poll, this.pollInterval);
        };

        poll();
    }

    private async getChanges(): Promise<DataChange[]> {
        const result = await this.connection.request().query(`
            SELECT 
                ct.SYS_CHANGE_VERSION,
                ct.SYS_CHANGE_OPERATION,
                ct.ID,
                'ES_USER' as table_name,
                u.*
            FROM CHANGETABLE(CHANGES ES_USER, ${this.lastSyncVersion}) ct
            LEFT JOIN ES_USER u ON ct.ID = u.ID
            WHERE ct.SYS_CHANGE_VERSION > ${this.lastSyncVersion}
            
            UNION ALL
            
            SELECT 
                ct.SYS_CHANGE_VERSION,
                ct.SYS_CHANGE_OPERATION,
                ct.ID,
                'ES_MACHINE' as table_name,
                m.*
            FROM CHANGETABLE(CHANGES ES_MACHINE, ${this.lastSyncVersion}) ct
            LEFT JOIN ES_MACHINE m ON ct.ID = m.ID
            WHERE ct.SYS_CHANGE_VERSION > ${this.lastSyncVersion}
            
            ORDER BY SYS_CHANGE_VERSION
        `);

        const changes: DataChange[] = result.recordset.map(row => ({
            table: row.table_name,
            id: row.ID.toString(),
            operation: this.mapOperation(row.SYS_CHANGE_OPERATION),
            data: row,
            timestamp: new Date(),
            source: 'legacy'
        }));

        // Mettre à jour la version de synchronisation
        if (changes.length > 0) {
            this.lastSyncVersion = Math.max(...changes.map(c => c.data.SYS_CHANGE_VERSION));
        }

        return changes;
    }

    private mapOperation(operation: string): 'INSERT' | 'UPDATE' | 'DELETE' {
        switch (operation) {
            case 'I': return 'INSERT';
            case 'U': return 'UPDATE';
            case 'D': return 'DELETE';
            default: throw new Error(`Opération inconnue: ${operation}`);
        }
    }
}
```

#### Synchronisation Batch Périodique
```typescript
// sync-service/src/BatchSync.ts
export class BatchSync {
    private legacyDS: LegacyDataSource;
    private modernDS: ModernDataSource;
    private logger: Logger;
    private batchSize: number;

    constructor(config: BatchSyncConfig) {
        this.legacyDS = new LegacyDataSource(config.legacy);
        this.modernDS = new ModernDataSource(config.modern);
        this.logger = config.logger;
        this.batchSize = config.batchSize || 1000;
    }

    async performFullSync(): Promise<SyncReport> {
        const report: SyncReport = {
            startTime: new Date(),
            tables: [],
            totalRecords: 0,
            syncedRecords: 0,
            errors: []
        };

        try {
            // Synchronisation des utilisateurs
            const userReport = await this.syncTable('users', 'ES_USER');
            report.tables.push(userReport);

            // Synchronisation des machines
            const machineReport = await this.syncTable('machines', 'ES_MACHINE');
            report.tables.push(machineReport);

            // Synchronisation des actions récentes (dernières 24h)
            const actionReport = await this.syncRecentActions();
            report.tables.push(actionReport);

            report.endTime = new Date();
            report.totalRecords = report.tables.reduce((sum, t) => sum + t.totalRecords, 0);
            report.syncedRecords = report.tables.reduce((sum, t) => sum + t.syncedRecords, 0);

        } catch (error) {
            report.errors.push(error.message);
            this.logger.error(`Erreur synchronisation batch: ${error.message}`);
        }

        return report;
    }

    private async syncTable(modernTable: string, legacyTable: string): Promise<TableSyncReport> {
        const report: TableSyncReport = {
            tableName: modernTable,
            totalRecords: 0,
            syncedRecords: 0,
            skippedRecords: 0,
            errors: []
        };

        try {
            // Récupération des données modifiées depuis la dernière sync
            const lastSyncTime = await this.getLastSyncTime(modernTable);
            const legacyRecords = await this.legacyDS.getModifiedRecords(legacyTable, lastSyncTime);
            
            report.totalRecords = legacyRecords.length;

            // Traitement par batch
            for (let i = 0; i < legacyRecords.length; i += this.batchSize) {
                const batch = legacyRecords.slice(i, i + this.batchSize);
                
                for (const record of batch) {
                    try {
                        const modernRecord = await this.transformRecord(record, legacyTable, modernTable);
                        await this.modernDS.upsertRecord(modernTable, modernRecord);
                        report.syncedRecords++;
                    } catch (error) {
                        report.errors.push(`Record ${record.ID}: ${error.message}`);
                        report.skippedRecords++;
                    }
                }

                // Pause entre les batches pour éviter la surcharge
                await this.sleep(100);
            }

            // Mise à jour du timestamp de dernière synchronisation
            await this.updateLastSyncTime(modernTable, new Date());

        } catch (error) {
            report.errors.push(error.message);
        }

        return report;
    }

    private async syncRecentActions(): Promise<TableSyncReport> {
        const report: TableSyncReport = {
            tableName: 'actions',
            totalRecords: 0,
            syncedRecords: 0,
            skippedRecords: 0,
            errors: []
        };

        try {
            // Synchronisation bidirectionnelle des actions récentes
            const cutoffTime = new Date(Date.now() - 24 * 60 * 60 * 1000); // 24h

            // Legacy -> Modern
            const legacyActions = await this.legacyDS.getRecentActions(cutoffTime);
            for (const action of legacyActions) {
                try {
                    const modernAction = await this.transformLegacyAction(action);
                    await this.modernDS.upsertAction(modernAction);
                    report.syncedRecords++;
                } catch (error) {
                    report.errors.push(`Legacy action ${action.ID}: ${error.message}`);
                    report.skippedRecords++;
                }
            }

            // Modern -> Legacy
            const modernActions = await this.modernDS.getRecentActions(cutoffTime);
            for (const action of modernActions) {
                try {
                    const legacyAction = await this.transformModernAction(action);
                    await this.legacyDS.upsertAction(legacyAction);
                    report.syncedRecords++;
                } catch (error) {
                    report.errors.push(`Modern action ${action.id}: ${error.message}`);
                    report.skippedRecords++;
                }
            }

            report.totalRecords = legacyActions.length + modernActions.length;

        } catch (error) {
            report.errors.push(error.message);
        }

        return report;
    }

    private sleep(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

interface SyncReport {
    startTime: Date;
    endTime?: Date;
    tables: TableSyncReport[];
    totalRecords: number;
    syncedRecords: number;
    errors: string[];
}

interface TableSyncReport {
    tableName: string;
    totalRecords: number;
    syncedRecords: number;
    skippedRecords: number;
    errors: string[];
}
```

### Gestion des Conflits et Incohérences

#### Stratégies de Résolution par Type de Données

```typescript
// sync-service/src/ConflictStrategies.ts

// Stratégie pour les actions (priorité au système qui a créé l'action)
class ActionConflictStrategy implements ConflictResolutionStrategy {
    name = 'action-priority';

    async resolve(conflict: DataConflict): Promise<ConflictResolution> {
        // Les actions créées dans le système moderne ont priorité
        if (conflict.modernData.created_at && !conflict.legacyData.legacy_guid) {
            return {
                strategy: this.name,
                winningSource: 'modern',
                winningData: conflict.modernData,
                action: 'update_legacy'
            };
        }
        
        // Les actions legacy en cours d'exécution ont priorité
        if (conflict.legacyData.ISDONE === 0) {
            return {
                strategy: this.name,
                winningSource: 'legacy',
                winningData: conflict.legacyData,
                action: 'update_modern'
            };
        }

        // Sinon, utiliser le timestamp
        return new TimestampBasedStrategy().resolve(conflict);
    }
}

// Stratégie pour les machines (données critiques)
class MachineConflictStrategy implements ConflictResolutionStrategy {
    name = 'machine-critical';

    async resolve(conflict: DataConflict): Promise<ConflictResolution> {
        // Pour les machines, certains champs sont critiques et ne doivent pas être écrasés
        const criticalFields = ['activation_key', 'serial_number'];
        
        const hasCriticalConflict = conflict.conflictFields.some(field => 
            criticalFields.includes(field)
        );

        if (hasCriticalConflict) {
            // Escalade vers révision manuelle
            return {
                strategy: this.name,
                winningSource: 'legacy', // Par défaut, garder legacy pour les champs critiques
                winningData: conflict.legacyData,
                action: 'manual_review'
            };
        }

        // Pour les autres champs, utiliser timestamp
        return new TimestampBasedStrategy().resolve(conflict);
    }
}
```

### Monitoring et Alertes

#### Système de Monitoring de la Synchronisation
```typescript
// sync-service/src/SyncMonitor.ts
export class SyncMonitor {
    private metrics: SyncMetrics;
    private alertManager: AlertManager;
    private logger: Logger;

    constructor(config: SyncMonitorConfig) {
        this.metrics = new SyncMetrics();
        this.alertManager = new AlertManager(config.alerts);
        this.logger = config.logger;
    }

    async recordSyncEvent(event: SyncEvent): Promise<void> {
        // Enregistrement des métriques
        this.metrics.recordEvent(event);

        // Vérification des seuils d'alerte
        await this.checkAlertThresholds();

        // Log de l'événement
        this.logger.info(`Sync event: ${event.type}`, event);
    }

    private async checkAlertThresholds(): Promise<void> {
        const currentMetrics = this.metrics.getCurrentMetrics();

        // Alerte si taux d'erreur > 5%
        if (currentMetrics.errorRate > 0.05) {
            await this.alertManager.sendAlert({
                level: 'warning',
                message: `Taux d'erreur synchronisation élevé: ${(currentMetrics.errorRate * 100).toFixed(2)}%`,
                metrics: currentMetrics
            });
        }

        // Alerte si délai de synchronisation > 30 secondes
        if (currentMetrics.avgSyncDelay > 30000) {
            await this.alertManager.sendAlert({
                level: 'warning',
                message: `Délai de synchronisation élevé: ${currentMetrics.avgSyncDelay}ms`,
                metrics: currentMetrics
            });
        }

        // Alerte critique si pas de synchronisation depuis 5 minutes
        if (Date.now() - currentMetrics.lastSyncTime > 5 * 60 * 1000) {
            await this.alertManager.sendAlert({
                level: 'critical',
                message: 'Synchronisation interrompue depuis plus de 5 minutes',
                metrics: currentMetrics
            });
        }
    }

    getHealthStatus(): SyncHealthStatus {
        const metrics = this.metrics.getCurrentMetrics();
        
        return {
            status: this.calculateOverallStatus(metrics),
            metrics: metrics,
            lastUpdate: new Date()
        };
    }

    private calculateOverallStatus(metrics: SyncMetrics): 'healthy' | 'warning' | 'critical' {
        if (Date.now() - metrics.lastSyncTime > 5 * 60 * 1000) {
            return 'critical';
        }
        
        if (metrics.errorRate > 0.05 || metrics.avgSyncDelay > 30000) {
            return 'warning';
        }
        
        return 'healthy';
    }
}

interface SyncEvent {
    type: 'sync_success' | 'sync_error' | 'conflict_resolved' | 'manual_intervention';
    source: 'legacy' | 'modern';
    target: 'legacy' | 'modern';
    table: string;
    recordId: string;
    timestamp: Date;
    duration?: number;
    error?: string;
}

interface SyncHealthStatus {
    status: 'healthy' | 'warning' | 'critical';
    metrics: SyncMetrics;
    lastUpdate: Date;
}
```

### Scripts de Déploiement et Configuration

#### Script de Déploiement du Service de Synchronisation
```bash
#!/bin/bash
# deploy_sync_service.sh
# Déploiement du service de synchronisation

set -e

echo "=== Déploiement Service de Synchronisation ==="

# Configuration
SYNC_SERVICE_IMAGE="essensys/sync-service:latest"
DOCKER_COMPOSE_FILE="docker-compose.sync.yml"

# Vérification des prérequis
echo "Vérification des prérequis..."
if ! command -v docker &> /dev/null; then
    echo "❌ Docker non installé"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose non installé"
    exit 1
fi

# Configuration des variables d'environnement
echo "Configuration des variables d'environnement..."
cat > .env.sync << EOF
# Base de données Legacy
LEGACY_DB_HOST=${LEGACY_DB_HOST:-legacy-db-server}
LEGACY_DB_PORT=${LEGACY_DB_PORT:-1433}
LEGACY_DB_NAME=${LEGACY_DB_NAME:-EssensysDB}
LEGACY_DB_USER=${LEGACY_DB_USER:-sa}
LEGACY_DB_PASSWORD=${LEGACY_DB_PASSWORD}

# Base de données Moderne
MODERN_DB_HOST=${MODERN_DB_HOST:-modern-db-server}
MODERN_DB_PORT=${MODERN_DB_PORT:-5432}
MODERN_DB_NAME=${MODERN_DB_NAME:-essensys_modern}
MODERN_DB_USER=${MODERN_DB_USER:-essensys}
MODERN_DB_PASSWORD=${MODERN_DB_PASSWORD}

# Configuration Redis (pour la file de messages)
REDIS_HOST=${REDIS_HOST:-redis-server}
REDIS_PORT=${REDIS_PORT:-6379}
REDIS_PASSWORD=${REDIS_PASSWORD}

# Configuration de synchronisation
SYNC_BATCH_SIZE=${SYNC_BATCH_SIZE:-1000}
SYNC_POLL_INTERVAL=${SYNC_POLL_INTERVAL:-5000}
SYNC_CONFLICT_STRATEGY=${SYNC_CONFLICT_STRATEGY:-timestamp}

# Monitoring
PROMETHEUS_PORT=${PROMETHEUS_PORT:-9090}
GRAFANA_PORT=${GRAFANA_PORT:-3001}

# Alertes
SLACK_WEBHOOK_URL=${SLACK_WEBHOOK_URL}
EMAIL_SMTP_HOST=${EMAIL_SMTP_HOST}
EMAIL_SMTP_USER=${EMAIL_SMTP_USER}
EMAIL_SMTP_PASSWORD=${EMAIL_SMTP_PASSWORD}
EOF

# Création du fichier Docker Compose
echo "Création de la configuration Docker Compose..."
cat > $DOCKER_COMPOSE_FILE << EOF
version: '3.8'

services:
  sync-service:
    image: $SYNC_SERVICE_IMAGE
    container_name: essensys-sync-service
    restart: unless-stopped
    env_file:
      - .env.sync
    volumes:
      - ./sync-logs:/app/logs
      - ./sync-config:/app/config
    ports:
      - "3002:3000"  # API de monitoring
    depends_on:
      - redis
      - prometheus
    networks:
      - sync-network

  redis:
    image: redis:7-alpine
    container_name: essensys-sync-redis
    restart: unless-stopped
    command: redis-server --requirepass \${REDIS_PASSWORD}
    volumes:
      - redis-data:/data
    networks:
      - sync-network

  prometheus:
    image: prom/prometheus:latest
    container_name: essensys-sync-prometheus
    restart: unless-stopped
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    ports:
      - "\${PROMETHEUS_PORT}:9090"
    networks:
      - sync-network

  grafana:
    image: grafana/grafana:latest
    container_name: essensys-sync-grafana
    restart: unless-stopped
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin123
    volumes:
      - grafana-data:/var/lib/grafana
      - ./grafana-dashboards:/etc/grafana/provisioning/dashboards
    ports:
      - "\${GRAFANA_PORT}:3000"
    networks:
      - sync-network

volumes:
  redis-data:
  prometheus-data:
  grafana-data:

networks:
  sync-network:
    driver: bridge
EOF

# Configuration Prometheus
echo "Configuration Prometheus..."
mkdir -p ./prometheus
cat > ./prometheus.yml << EOF
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'sync-service'
    static_configs:
      - targets: ['sync-service:3000']
    metrics_path: '/metrics'
    scrape_interval: 10s

  - job_name: 'redis'
    static_configs:
      - targets: ['redis:6379']
EOF

# Configuration Grafana
echo "Configuration Grafana..."
mkdir -p ./grafana-dashboards
cat > ./grafana-dashboards/sync-dashboard.json << 'EOF'
{
  "dashboard": {
    "title": "Synchronisation Essensys",
    "panels": [
      {
        "title": "Taux de Synchronisation",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(sync_events_total[5m])",
            "legendFormat": "Events/sec"
          }
        ]
      },
      {
        "title": "Taux d'Erreur",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(sync_errors_total[5m]) / rate(sync_events_total[5m]) * 100",
            "legendFormat": "Error Rate %"
          }
        ]
      },
      {
        "title": "Délai de Synchronisation",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, sync_duration_seconds_bucket)",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
EOF

# Déploiement
echo "Déploiement des services..."
docker-compose -f $DOCKER_COMPOSE_FILE pull
docker-compose -f $DOCKER_COMPOSE_FILE up -d

# Vérification du déploiement
echo "Vérification du déploiement..."
sleep 30

if docker-compose -f $DOCKER_COMPOSE_FILE ps | grep -q "Up"; then
    echo "✅ Service de synchronisation déployé avec succès"
    echo "📊 Monitoring disponible sur:"
    echo "   - Prometheus: http://localhost:${PROMETHEUS_PORT:-9090}"
    echo "   - Grafana: http://localhost:${GRAFANA_PORT:-3001} (admin/admin123)"
    echo "   - API Sync: http://localhost:3002/health"
else
    echo "❌ Échec du déploiement"
    docker-compose -f $DOCKER_COMPOSE_FILE logs
    exit 1
fi

echo "=== Déploiement terminé ==="
```

#### Tests de Synchronisation
```bash
#!/bin/bash
# test_synchronization.sh
# Tests de la synchronisation bidirectionnelle

echo "=== Tests de Synchronisation ==="

# Configuration
SYNC_API_URL="http://localhost:3002"
LEGACY_DB_CONNECTION="Server=legacy-server;Database=EssensysDB;User=sa;Password=$LEGACY_DB_PASSWORD"
MODERN_DB_CONNECTION="postgresql://essensys:$MODERN_DB_PASSWORD@modern-server:5432/essensys_modern"

# Test 1: Santé du service
echo "Test 1: Vérification santé du service..."
HEALTH_STATUS=$(curl -s "$SYNC_API_URL/health" | jq -r '.status')
if [ "$HEALTH_STATUS" = "healthy" ]; then
    echo "✅ Service de synchronisation en bonne santé"
else
    echo "❌ Service de synchronisation en erreur: $HEALTH_STATUS"
    exit 1
fi

# Test 2: Synchronisation Legacy -> Modern
echo "Test 2: Test synchronisation Legacy -> Modern..."
# Insertion d'un utilisateur test dans legacy
sqlcmd -S legacy-server -d EssensysDB -Q "
INSERT INTO ES_USER (MAIL, PASSWORD, NOM, PRENOM, ADR1, CP, VILLE, DATECREATION, ISVALID)
VALUES ('test-sync@example.com', 'testhash', 'Test', 'Sync', '123 Test St', '12345', 'TestCity', GETDATE(), 1)
"

# Attendre la synchronisation
sleep 10

# Vérifier la présence dans modern
MODERN_USER_COUNT=$(psql "$MODERN_DB_CONNECTION" -t -c "SELECT COUNT(*) FROM users WHERE email = 'test-sync@example.com'")
if [ "$MODERN_USER_COUNT" -eq 1 ]; then
    echo "✅ Synchronisation Legacy -> Modern réussie"
else
    echo "❌ Synchronisation Legacy -> Modern échouée"
fi

# Test 3: Synchronisation Modern -> Legacy
echo "Test 3: Test synchronisation Modern -> Legacy..."
# Mise à jour dans modern
psql "$MODERN_DB_CONNECTION" -c "
UPDATE users 
SET first_name = 'SyncUpdated' 
WHERE email = 'test-sync@example.com'
"

# Attendre la synchronisation
sleep 10

# Vérifier la mise à jour dans legacy
LEGACY_USER_NAME=$(sqlcmd -S legacy-server -d EssensysDB -h -1 -Q "SELECT PRENOM FROM ES_USER WHERE MAIL = 'test-sync@example.com'" | tr -d ' ')
if [ "$LEGACY_USER_NAME" = "SyncUpdated" ]; then
    echo "✅ Synchronisation Modern -> Legacy réussie"
else
    echo "❌ Synchronisation Modern -> Legacy échouée: $LEGACY_USER_NAME"
fi

# Test 4: Gestion des conflits
echo "Test 4: Test gestion des conflits..."
# Modification simultanée dans les deux systèmes
sqlcmd -S legacy-server -d EssensysDB -Q "
UPDATE ES_USER 
SET NOM = 'ConflictLegacy' 
WHERE MAIL = 'test-sync@example.com'
"

psql "$MODERN_DB_CONNECTION" -c "
UPDATE users 
SET last_name = 'ConflictModern' 
WHERE email = 'test-sync@example.com'
"

# Attendre la résolution du conflit
sleep 15

# Vérifier la résolution (doit utiliser la stratégie timestamp)
CONFLICT_RESOLUTION=$(curl -s "$SYNC_API_URL/conflicts/recent" | jq -r '.[] | select(.recordId == "test-sync@example.com") | .resolution.strategy')
if [ "$CONFLICT_RESOLUTION" = "timestamp-based" ]; then
    echo "✅ Gestion des conflits fonctionnelle"
else
    echo "❌ Gestion des conflits échouée"
fi

# Test 5: Performance de synchronisation
echo "Test 5: Test performance synchronisation..."
START_TIME=$(date +%s)

# Insertion de 100 enregistrements test
for i in {1..100}; do
    sqlcmd -S legacy-server -d EssensysDB -Q "
    INSERT INTO ES_USER (MAIL, PASSWORD, NOM, PRENOM, ADR1, CP, VILLE, DATECREATION, ISVALID)
    VALUES ('perf-test-$i@example.com', 'hash', 'Perf', 'Test$i', '123 St', '12345', 'City', GETDATE(), 1)
    " > /dev/null
done

# Attendre la synchronisation complète
sleep 30

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

SYNCED_COUNT=$(psql "$MODERN_DB_CONNECTION" -t -c "SELECT COUNT(*) FROM users WHERE email LIKE 'perf-test-%@example.com'")
if [ "$SYNCED_COUNT" -eq 100 ] && [ "$DURATION" -lt 60 ]; then
    echo "✅ Performance synchronisation acceptable: $SYNCED_COUNT records en ${DURATION}s"
else
    echo "❌ Performance synchronisation insuffisante: $SYNCED_COUNT records en ${DURATION}s"
fi

# Nettoyage des données de test
echo "Nettoyage des données de test..."
sqlcmd -S legacy-server -d EssensysDB -Q "DELETE FROM ES_USER WHERE MAIL LIKE '%test%@example.com'"
psql "$MODERN_DB_CONNECTION" -c "DELETE FROM users WHERE email LIKE '%test%@example.com'"

echo "=== Tests de synchronisation terminés ==="
```

Cette stratégie de synchronisation fournit un framework robuste pour maintenir la cohérence des données pendant la période de transition, avec des mécanismes de détection et résolution des conflits, un monitoring complet et des tests automatisés.