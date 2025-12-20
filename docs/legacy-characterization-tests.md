# Tests de Caractérisation du Système Legacy Essensys

## Vue d'Ensemble

Les tests de caractérisation capturent le comportement actuel du système legacy pour garantir la parité fonctionnelle lors de la migration. Ces tests documentent le comportement "tel quel" sans jugement sur sa correction.

## Stratégie de Tests de Caractérisation

### Objectifs
1. **Capturer le comportement actuel** des APIs critiques
2. **Documenter les cas limites** et comportements inattendus
3. **Créer une baseline** pour la validation post-migration
4. **Identifier les bugs** existants à corriger ou reproduire

### Périmètre des Tests
- ✅ Endpoints API utilisés par les boîtiers IoT
- ✅ Logique d'authentification et autorisation
- ✅ Services métier critiques (Actions, États, Utilisateurs)
- ✅ Règles métier complexes identifiées
- ❌ Interface utilisateur web (tests manuels)
- ❌ Intégrations externes (SMS, Email)

## Tests de Caractérisation des APIs Boîtiers

### 1. API MyActions - Récupération des Actions

#### Test 1.1: Actions Normales Sans Alarme
```csharp
[TestClass]
public class MyActionsCharacterizationTests
{
    [TestMethod]
    public void GetActions_WithPendingActions_ReturnsActionsWithoutAlarm()
    {
        // Arrange
        var machine = CreateTestMachine();
        var action1 = CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        var action2 = CreateAction(machine, "VOLET", "1", "50");
        
        // Act
        var controller = new MyActionsController();
        SetAuthenticatedMachine(controller, machine);
        var result = controller.Get();
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.actions.Count);
        Assert.IsNull(result._de67f); // Pas d'action alarme
        
        // Vérifier structure des actions
        var action = result.actions[0];
        Assert.IsNotNull(action.guid);
        Assert.IsNotNull(action.@params);
        Assert.IsTrue(action.@params.Count > 0);
    }
    
    [TestMethod]
    public void GetActions_WithAlarmAction_ReturnsAlarmInSpecialField()
    {
        // Arrange
        var machine = CreateTestMachine();
        var alarmAction = CreateAction(machine, "ALARME", "", "activate");
        
        // Act
        var controller = new MyActionsController();
        SetAuthenticatedMachine(controller, machine);
        var result = controller.Get();
        
        // Assert
        Assert.IsNotNull(result._de67f);
        Assert.AreEqual(alarmAction.Guid, result._de67f.guid);
        Assert.AreEqual("activate", result._de67f.obl);
    }
    
    [TestMethod]
    public void GetActions_WithNoActions_ReturnsEmptyList()
    {
        // Arrange
        var machine = CreateTestMachine();
        
        // Act
        var controller = new MyActionsController();
        SetAuthenticatedMachine(controller, machine);
        var result = controller.Get();
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.actions.Count);
        Assert.IsNull(result._de67f);
    }
    
    [TestMethod]
    public void GetActions_OnlyReturnsNonExecutedActions()
    {
        // Arrange
        var machine = CreateTestMachine();
        var pendingAction = CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        var executedAction = CreateAction(machine, "VOLET", "1", "100");
        executedAction.IsDone = true;
        SaveAction(executedAction);
        
        // Act
        var controller = new MyActionsController();
        SetAuthenticatedMachine(controller, machine);
        var result = controller.Get();
        
        // Assert
        Assert.AreEqual(1, result.actions.Count);
        Assert.AreEqual(pendingAction.Guid, result.actions[0].guid);
    }
}
```

### 2. API MyStatus - Réception des États

#### Test 2.1: Enregistrement d'États Valides
```csharp
[TestClass]
public class MyStatusCharacterizationTests
{
    [TestMethod]
    public void PostStatus_WithValidData_Returns201Created()
    {
        // Arrange
        var machine = CreateTestMachine();
        var statusMessage = new EsStatusMessage
        {
            version = "V2",
            ek = new List<EsKeyValue>
            {
                new EsKeyValue { k = 920, v = "1" },
                new EsKeyValue { k = 407, v = "0" }
            }
        };
        
        // Act
        var controller = new MyStatusController();
        SetAuthenticatedMachine(controller, machine);
        var response = controller.Post(statusMessage);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        // Vérifier que l'état a été enregistré
        var savedState = GetLastState(machine);
        Assert.IsNotNull(savedState);
        Assert.AreEqual("V2", savedState.Version);
    }
    
    [TestMethod]
    public void PostStatus_WithNullEk_Returns400BadRequest()
    {
        // Arrange
        var machine = CreateTestMachine();
        var statusMessage = new EsStatusMessage
        {
            version = "V2",
            ek = null // Données invalides
        };
        
        // Act & Assert
        var controller = new MyStatusController();
        SetAuthenticatedMachine(controller, machine);
        
        try
        {
            var response = controller.Post(statusMessage);
            Assert.Fail("Expected HttpResponseException");
        }
        catch (HttpResponseException ex)
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }
    }
    
    [TestMethod]
    public void PostStatus_UpdatesLastConnectionTime()
    {
        // Arrange
        var machine = CreateTestMachine();
        var beforeTime = DateTime.Now;
        var statusMessage = CreateValidStatusMessage();
        
        // Act
        var controller = new MyStatusController();
        SetAuthenticatedMachine(controller, machine);
        controller.Post(statusMessage);
        
        // Assert
        var updatedMachine = GetMachine(machine.Id);
        Assert.IsTrue(updatedMachine.DateModification >= beforeTime);
    }
}
```

### 3. API Done - Acquittement d'Actions

#### Test 3.1: Acquittement d'Actions Existantes
```csharp
[TestClass]
public class DoneCharacterizationTests
{
    [TestMethod]
    public void PostDone_WithValidGuid_Returns201AndMarksActionDone()
    {
        // Arrange
        var machine = CreateTestMachine();
        var action = CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        var guid = action.Guid;
        
        // Act
        var controller = new DoneController();
        SetAuthenticatedMachine(controller, machine);
        var response = controller.Post(guid);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        // Vérifier que l'action est marquée comme exécutée
        var updatedAction = GetAction(guid);
        Assert.IsTrue(updatedAction.IsDone);
    }
    
    [TestMethod]
    public void PostDone_WithInvalidGuid_Returns404NotFound()
    {
        // Arrange
        var machine = CreateTestMachine();
        var invalidGuid = Guid.NewGuid().ToString();
        
        // Act & Assert
        var controller = new DoneController();
        SetAuthenticatedMachine(controller, machine);
        
        try
        {
            var response = controller.Post(invalidGuid);
            Assert.Fail("Expected HttpResponseException");
        }
        catch (HttpResponseException ex)
        {
            Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }
    }
    
    [TestMethod]
    public void PostDone_OnlyAcquitsActionsForAuthenticatedMachine()
    {
        // Arrange
        var machine1 = CreateTestMachine();
        var machine2 = CreateTestMachine();
        var action = CreateAction(machine2, "CHAUFFAGE", "zj", "eco");
        
        // Act & Assert
        var controller = new DoneController();
        SetAuthenticatedMachine(controller, machine1);
        
        try
        {
            var response = controller.Post(action.Guid);
            Assert.Fail("Expected HttpResponseException");
        }
        catch (HttpResponseException ex)
        {
            Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }
    }
}
```

## Tests de Caractérisation de l'Authentification

### 4. Authentification Basic Auth

#### Test 4.1: Validation de Clé d'Activation
```csharp
[TestClass]
public class AuthenticationCharacterizationTests
{
    [TestMethod]
    public void BasicAuth_WithValidKey_AuthenticatesSuccessfully()
    {
        // Arrange
        var machine = CreateTestMachine();
        var activationKey = machine.Pkey; // 32 caractères
        var hashedKey = HashHelper.GetHash(activationKey, HashHelper.HashType.MD5);
        
        // Simuler le format Basic Auth
        var username = hashedKey.Substring(0, 16);
        var password = hashedKey.Substring(16, 16);
        var credentials = $"{username}:{password}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        
        // Act
        var request = CreateHttpRequest();
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
        
        var attribute = new EssensysAuthorizeAttribute();
        var isAuthorized = TestAuthorization(attribute, request);
        
        // Assert
        Assert.IsTrue(isAuthorized);
        Assert.IsNotNull(HttpContext.Current.Session["Machine"]);
    }
    
    [TestMethod]
    public void BasicAuth_WithInvalidKey_ReturnsUnauthorized()
    {
        // Arrange
        var invalidCredentials = "invalid:credentials";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidCredentials));
        
        // Act
        var request = CreateHttpRequest();
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
        
        var attribute = new EssensysAuthorizeAttribute();
        var isAuthorized = TestAuthorization(attribute, request);
        
        // Assert
        Assert.IsFalse(isAuthorized);
    }
    
    [TestMethod]
    public void BasicAuth_WithInactiveMachine_ReturnsUnauthorized()
    {
        // Arrange
        var machine = CreateTestMachine();
        machine.IsActive = false;
        SaveMachine(machine);
        
        var hashedKey = machine.HashedPkey;
        var credentials = $"{hashedKey.Substring(0, 16)}:{hashedKey.Substring(16, 16)}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        
        // Act
        var request = CreateHttpRequest();
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
        
        var attribute = new EssensysAuthorizeAttribute();
        var isAuthorized = TestAuthorization(attribute, request);
        
        // Assert
        Assert.IsFalse(isAuthorized);
    }
}
```

## Tests de Caractérisation des Services Métier

### 5. UserService - Gestion des Utilisateurs

#### Test 5.1: Authentification Utilisateur
```csharp
[TestClass]
public class UserServiceCharacterizationTests
{
    [TestMethod]
    public void LoginIsValid_WithCorrectCredentials_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "password123");
        var passwordHash = HashHelper.GetHash("password123", HashHelper.HashType.SHA1);
        
        // Act
        var service = new UserService();
        var isValid = service.LoginIsValid("test@example.com", passwordHash);
        
        // Assert
        Assert.IsTrue(isValid);
        
        // Vérifier que l'utilisateur est mis en session
        Assert.IsNotNull(HttpContext.Current.Session["User"]);
        
        // Vérifier que la machine est marquée comme connectée
        Assert.IsTrue(ConnectionInfo.ConnectedMachines.Contains(user.Machine.Id));
    }
    
    [TestMethod]
    public void LoginIsValid_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "password123");
        var wrongPasswordHash = HashHelper.GetHash("wrongpassword", HashHelper.HashType.SHA1);
        
        // Act
        var service = new UserService();
        var isValid = service.LoginIsValid("test@example.com", wrongPasswordHash);
        
        // Assert
        Assert.IsFalse(isValid);
    }
    
    [TestMethod]
    public void LoginIsValid_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "password123");
        user.Isvalid = false;
        SaveUser(user);
        
        var passwordHash = HashHelper.GetHash("password123", HashHelper.HashType.SHA1);
        
        // Act
        var service = new UserService();
        var isValid = service.LoginIsValid("test@example.com", passwordHash);
        
        // Assert
        Assert.IsFalse(isValid);
    }
    
    [TestMethod]
    public void LoginIsValid_UpdatesLastAccessTime()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "password123");
        var beforeTime = DateTime.Now;
        var passwordHash = HashHelper.GetHash("password123", HashHelper.HashType.SHA1);
        
        // Act
        var service = new UserService();
        service.LoginIsValid("test@example.com", passwordHash);
        
        // Assert
        var updatedUser = GetUser(user.Id);
        Assert.IsTrue(updatedUser.Lastaccess >= beforeTime);
    }
}
```

### 6. ActionService - Gestion des Actions

#### Test 6.1: Enregistrement d'Actions
```csharp
[TestClass]
public class ActionServiceCharacterizationTests
{
    [TestMethod]
    public void RegisterAction_CreatesActionWithCorrectParameters()
    {
        // Arrange
        var machine = CreateTestMachine();
        var parameters = new Dictionary<int, string>
        {
            { 100, "eco" },  // Mode chauffage zone jour
            { 101, "comfort" } // Mode chauffage zone nuit
        };
        
        // Act
        var service = new ActionService();
        service.RegisterAction(machine, EsActionType.CHAUFFAGE, "Test action", parameters);
        
        // Assert
        var actions = GetPendingActions(machine);
        Assert.AreEqual(1, actions.Count);
        
        var action = actions[0];
        Assert.AreEqual("CHAUFFAGE", action.ActionType);
        Assert.AreEqual("Test action", action.ActionInfo);
        Assert.IsFalse(action.IsDone);
        Assert.IsNotNull(action.Guid);
        Assert.AreEqual(2, action.Indexes.Count);
    }
    
    [TestMethod]
    public void AcquitAllActions_MarksAllActionsAsDone()
    {
        // Arrange
        var machine = CreateTestMachine();
        CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        CreateAction(machine, "VOLET", "1", "50");
        CreateAction(machine, "ALARME", "", "activate");
        
        // Act
        var service = new ActionService();
        service.AcquitAllActions(machine);
        
        // Assert
        var pendingActions = GetPendingActions(machine);
        Assert.AreEqual(0, pendingActions.Count);
        
        var allActions = GetAllActions(machine);
        Assert.IsTrue(allActions.All(a => a.IsDone));
    }
    
    [TestMethod]
    public void UndoAllActions_DeletesAllPendingActions()
    {
        // Arrange
        var machine = CreateTestMachine();
        CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        CreateAction(machine, "VOLET", "1", "50");
        var beforeCount = GetAllActions(machine).Count;
        
        // Act
        var service = new ActionService();
        service.UndoAllActions(machine);
        
        // Assert
        var afterCount = GetAllActions(machine).Count;
        Assert.AreEqual(0, afterCount);
    }
}
```

### 7. StateService - Gestion des États

#### Test 7.1: Enregistrement et Synchronisation
```csharp
[TestClass]
public class StateServiceCharacterizationTests
{
    [TestMethod]
    public void RegisterState_CreatesNewStateWithIndexes()
    {
        // Arrange
        var machine = CreateTestMachine();
        var keyValues = new List<EsKeyValue>
        {
            new EsKeyValue { k = 920, v = "1" },
            new EsKeyValue { k = 407, v = "0" }
        };
        
        // Act
        var service = new StateService();
        service.RegisterState(machine, keyValues, "V2");
        
        // Assert
        var state = GetLastState(machine);
        Assert.IsNotNull(state);
        Assert.AreEqual("V2", state.Version);
        Assert.AreEqual(machine.Id, state.Machine.Id);
    }
    
    [TestMethod]
    public void HasRefreshed_ReturnsTrueAfterNewState()
    {
        // Arrange
        var machine = CreateTestMachine();
        var service = new StateService();
        var lastCall = service.GetLastCall(machine);
        
        // Attendre un peu
        System.Threading.Thread.Sleep(1000);
        
        // Enregistrer un nouvel état
        var keyValues = CreateTestKeyValues();
        service.RegisterState(machine, keyValues, "V2");
        
        // Act
        var hasRefreshed = service.HasRefreshed(machine, lastCall);
        
        // Assert
        Assert.IsTrue(hasRefreshed);
    }
    
    [TestMethod]
    public void AllActionsOK_ReturnsTrueWhenNoActionsOrAllDone()
    {
        // Arrange
        var machine = CreateTestMachine();
        var service = new StateService();
        
        // Act - Pas d'actions
        var result1 = service.AllActionsOK(machine);
        
        // Créer et exécuter des actions
        var action = CreateAction(machine, "CHAUFFAGE", "zj", "eco");
        action.IsDone = true;
        SaveAction(action);
        
        var result2 = service.AllActionsOK(machine);
        
        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
    }
}
```

## Tests de Caractérisation des Règles Métier

### 8. Règles de Chauffage

#### Test 8.1: Modes de Chauffage par Zone
```csharp
[TestClass]
public class ChauffageServiceCharacterizationTests
{
    [TestMethod]
    public void RegisterAction_ZoneJour_CreatesCorrectAction()
    {
        // Arrange
        var machine = CreateTestMachine();
        
        // Act
        ChauffageService.RegisterAction(machine, "eco", "zj");
        
        // Assert
        var actions = GetPendingActions(machine);
        Assert.AreEqual(1, actions.Count);
        
        var action = actions[0];
        Assert.AreEqual("CHAUFFAGE", action.ActionType);
        Assert.AreEqual("Chauffage zj en mode eco", action.ActionInfo);
        
        // Vérifier l'index de données
        var index = action.Indexes.First();
        Assert.AreEqual((int)Tbb_Donnees_Index.Chauf_zj_Mode, Convert.ToInt32(index.Index.IndexKey));
        Assert.AreEqual("eco", index.Value);
    }
    
    [TestMethod]
    [DataRow("zj", "eco")]
    [DataRow("zj", "comfort")]
    [DataRow("zn", "eco")]
    [DataRow("sdb1", "comfort")]
    [DataRow("sdb2", "eco")]
    public void RegisterAction_AllZonesAndModes_CreateCorrectActions(string zone, string mode)
    {
        // Arrange
        var machine = CreateTestMachine();
        
        // Act
        ChauffageService.RegisterAction(machine, mode, zone);
        
        // Assert
        var actions = GetPendingActions(machine);
        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual("CHAUFFAGE", actions[0].ActionType);
        Assert.IsTrue(actions[0].ActionInfo.Contains(zone));
        Assert.IsTrue(actions[0].ActionInfo.Contains(mode));
    }
}
```

## Comportements Documentés (Bugs ou Particularités)

### Comportements Inattendus à Reproduire

#### 1. Timeout de Session Ultra-Court
```csharp
[TestMethod]
public void Session_ExpiresAfter1Minute()
{
    // COMPORTEMENT ACTUEL: Session timeout = 1 minute (très court)
    // Configuration: <sessionState mode="InProc" timeout="1"></sessionState>
    // À DÉCIDER: Reproduire ou corriger dans la migration?
}
```

#### 2. Thread.Sleep dans Contrôleurs Web
```csharp
[TestMethod]
public void WaitBox_BlocksThreadFor40Seconds()
{
    // COMPORTEMENT ACTUEL: Boucle while avec Thread.Sleep(1000) jusqu'à 40 itérations
    // PROBLÈME: Bloque le thread web, non scalable
    // À DÉCIDER: Remplacer par async/await ou WebSockets
}
```

#### 3. Actions Sans TTL
```csharp
[TestMethod]
public void Actions_NeverExpire()
{
    // COMPORTEMENT ACTUEL: Actions restent en attente indéfiniment
    // Pas de mécanisme d'expiration ou de nettoyage
    // À DÉCIDER: Ajouter TTL dans la migration?
}
```

#### 4. Hash SHA1 pour Mots de Passe
```csharp
[TestMethod]
public void Passwords_UseSHA1Hash()
{
    // COMPORTEMENT ACTUEL: SHA1 sans salt
    // VULNÉRABILITÉ: SHA1 est obsolète et vulnérable
    // MIGRATION: Remplacer par bcrypt avec migration progressive
}
```

## Exécution des Tests

### Prérequis
- Base de données de test avec données de référence
- Configuration de test (App.config)
- NHibernate configuré pour environnement de test

### Commandes
```bash
# Exécuter tous les tests de caractérisation
dotnet test --filter "FullyQualifiedName~CharacterizationTests"

# Exécuter tests d'un domaine spécifique
dotnet test --filter "FullyQualifiedName~MyActionsCharacterizationTests"

# Générer rapport de couverture
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Critères de Succès
- ✅ 100% des tests passent sur le système legacy
- ✅ Tous les comportements critiques sont documentés
- ✅ Les bugs connus sont identifiés et documentés
- ✅ Baseline établie pour validation post-migration

## Utilisation Post-Migration

Ces tests serviront de **tests de régression** pour valider que le nouveau système React/Node.js reproduit fidèlement le comportement du legacy (sauf décisions explicites de correction).

### Adaptation des Tests
Les tests devront être adaptés pour :
- Remplacer les appels C# par des appels HTTP/REST
- Utiliser Jest/Supertest au lieu de MSTest
- Adapter les assertions aux nouveaux formats de données
- Maintenir la même logique de validation

Cette suite de tests de caractérisation fournit une baseline complète pour garantir la parité fonctionnelle lors de la migration.