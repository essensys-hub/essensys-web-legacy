# Analyse du Protocole Web Legacy Essensys

Cette analyse détaille les échanges de données identifiés pour la page `Index.cshtml`.

## 1. Ce qui est envoyé au Backend (Frontend -> Serveur)

Lorsque l'utilisateur clique sur "Validation" dans `Index.cshtml`, le script `essensys.js` (fonction `doactions`) construit un objet JSON et l'envoie via une requête **POST** à l'URL `/Home/DoActions`.

### Payload JSON envoyé :

| Champ | Type | Description |
|-------|------|-------------|
| `newar` | boolean | Indique si l'arrosage a été modifié |
| `ar` | string | Valeur pour l'arrosage (ex: "255", "15", "0") |
| `newal` | boolean | Indique si l'alarme a été modifiée |
| `al` | string | "on" ou "off" |
| `alresp` | string | Réponse à la question secrète (pour l'alarme) |
| `codealarme` | string | Code de l'alarme saisi |
| `newcf`, `newcfzn`, ... | boolean | Flags de modification pour chaque zone de chauffage |
| `cfzj`, `cfzn`, ... | string | Valeurs de chauffage (ex: "1", "17", "16", etc.) |
| `newcm` | boolean | Flag modification Cumulus |
| `cfcm` | string | Valeur Cumulus ("0", "1", "2") |
| `cfvol`, `cfsto` | string | Valeurs checkbox pour volets/stores ("true" si coché) |
| `vl_{index}_{i}` | string | **Champs dynamiques** pour les équipements (volets/lumières). <br>Ex: `vl_617_0: "1"` (Index 617 = Volet Salon 1, Valeur 1 = Ouvrir) |

Ces données sont traitées par `HomeController.DoActions` qui appelle `FonctionService.DoActions` pour enregistrer les actions en base de données via `ActionService`.

---

## 2. Trame Action envoyée au Client Essensys (Serveur -> Machine)

Le client Essensys (la machine/box) récupère les actions en attente en appelant l'API **GET** `/api/MyActions`.

La réponse est un objet JSON structuré (classe `EsActionsInfo`) contenant deux parties principales : une action d'alarme spécifique (obfusquée) et une liste d'autres actions.

### Structure JSON de la réponse (Trame) :

```json
{
  "_de67f": {              // Action ALARME (si présente)
    "guid": "...",         // Identifiant unique de l'action
    "obl": "..."           // Infos de l'action (ex: code, commande)
  },
  "actions": [             // Liste des autres actions (Chauffage, Volets, etc.)
    {
      "guid": "...",       // Identifiant unique de l'action
      "params": [          // Liste des paires Index/Valeur
        {
          "k": 617,        // Clé (Index de l'équipement, ex: 617)
          "v": "1"         // Valeur à appliquer
        },
        {
          "k": 620,
          "v": "0"
        }
      ]
    }
    // ... autres actions
  ]
}
```

### Détails techniques :
- **`_de67f`** : Correspond à la propriété C# `EsAlarmAction _de67f` dans `EsActionsInfo`. C'est vraisemblablement un nom obfusqué pour sécuriser ou masquer l'action d'alarme. 'obl' contient l'info de l'action.
- **`actions`** : Liste d'objets `EsActionInfo`. Chaque action contient une liste de paramètres (`params`), qui sont des paires Clé/Valeur (`EsKeyValue`).
    - **`k`** (int) : L'ID de l'index sur la machine (ex: 617 pour un volet).
    - **`v`** (string) : La valeur de commande.

### 3. (Legacy/Obsolète) Keys "K" pour l'Alarme

Le code source contient des références à une ancienne méthode non chiffrée où les codes d'alarme étaient envoyés via des index spécifiques (`Tbb_Donnees_Index`). Bien que le système actuel utilise le chiffrement, voici les clés historiques demandées :

| Key (index) | Nom | Description | Valeur (Exemple) |
|-------------|-----|-------------|------------------|
| **409** | `Alarme_Commande` | Commande ON/OFF | "1" (ON) ou "0" (OFF) |
| **410** | `Alarme_CodeSaisiLSB` | 1er et 2ème chiffre du code | "12" (pour code 1234) |
| **411** | `Alarme_CodeSaisiMSB` | 3ème et 4ème chiffre du code | "34" (pour code 1234) |

*Note : Ces clés correspondent aux définitions dans `Tbb_Donnees_Index.cs`. Le code actuel utilise une chaîne chiffrée unique dans `ActionInfo` et n'utilise plus ces clés individuelles pour l'alarme.*

### 4. Confirmation Firmware (SC944D)

L'analyse du code C du firmware confirme que ces codes (410 et 411) sont bien utilisés pour valider le code de l'alarme dans la logique de la carte.

Extrait de `Alarme.c` :
```c
if(*uc_NouveauCodeSaisiLSB == Tb_Echange[Alarme_CodeUser1LSB] && *uc_NouveauCodeSaisiMSB == Tb_Echange[Alarme_CodeUser1MSB])
{
    l_uc_ValeurRetour = uc_ControleCodeSaisie_CODE_SAISI_CORRECT;
}
Tb_Echange[Alarme_CodeSaisiLSB] = 0xFF; // On remet a 0xFF la table d'echange
Tb_Echange[Alarme_CodeSaisiMSB] = 0xFF;
```

### 5. Stockage du Mot de Passe Alarme (Firmware)

Le mot de passe numérique à 4 chiffres est stocké dans l'EEPROM série de la carte (composant gérant aussi l'adresse MAC).

- **Fichier Source** : `EepromAdresseMac.c` / `Alarme.c`
- **Emplacement** : EEPROM, Offset `0x10` (16).
- **Taille** : 2 octets.
- **Format** : Packed BCD (2 chiffres par octet).

**Structure des octets :**

| Octet | Contenu (Code "1234") | Bits 0-3 (Low Nibble) | Bits 4-7 (High Nibble) | Valeur Hex |
| :--- | :--- | :--- | :--- | :--- |
| **LSB** (`Alarme_CodeUser1LSB`) | Chiffres 1 & 2 | **Chiffre 1** (ex: 1) | **Chiffre 2** (ex: 2) | `0x21` |
| **MSB** (`Alarme_CodeUser1MSB`) | Chiffres 3 & 4 | **Chiffre 3** (ex: 3) | **Chiffre 4** (ex: 4) | `0x43` |

*Note: Le code source inverse le sens de lecture standard pour les nibbles (Low = 1er chiffre).*

