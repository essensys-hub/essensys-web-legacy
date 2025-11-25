using Essensys.Service.Transaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Essensys.Repository.DTO;
using Essensys.Service.Response;
using System.Collections.Generic;
using Essensys.Service.Security;
using Essensys.Repository.DAO;
using Essensys.Repository;
using NHibernate;

namespace Essensys.Web.UI.Tests
{
    
    
    /// <summary>
    ///Classe de test pour StateServiceTest, destinée à contenir tous
    ///les tests unitaires StateServiceTest
    ///</summary>
    [TestClass()]
    public class StateServiceTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Obtient ou définit le contexte de test qui fournit
        ///des informations sur la série de tests active ainsi que ses fonctionnalités.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributs de tests supplémentaires
        // 
        //Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
        //
        //Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test dans la classe
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Utilisez ClassCleanup pour exécuter du code après que tous les tests ont été exécutés dans une classe
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Test pour RegisterState
        ///</summary>
        [TestMethod()]
        public void RegisterStateTest()
        {
            ISession sess = EsSessionFactory.GetSession();
            EsMachine m = new EsMachineRepository(sess).FindBy(2);
            
            UserService target = new UserService();
            Assert.IsTrue(!target.IsConnected(m));

            ServerService servserv = new ServerService();
            List<int> actual = servserv.GetDataIndex();
            Assert.IsTrue(actual.Count > 0);

            StateService stserv = new StateService();
            List<EsKeyValue> vals = new List<EsKeyValue>();
            foreach (int k in actual)
            {
                EsKeyValue kv = new EsKeyValue();
                kv.k = k;
                kv.v = "0";
                vals.Add(kv);
            }
            stserv.RegisterState(m, vals, "V1");

            sess.Clear();
            sess.Close();
            sess = null;

            Assert.IsTrue(1 == 1);
        }
    }
}
