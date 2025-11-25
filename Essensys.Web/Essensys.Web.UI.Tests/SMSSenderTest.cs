using Essensys.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Essensys.Web.UI.Tests
{
    
    
    /// <summary>
    ///Classe de test pour SMSSenderTest, destinée à contenir tous
    ///les tests unitaires SMSSenderTest
    ///</summary>
    [TestClass()]
    public class SMSSenderTest
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
        ///Test pour Send
        ///</summary>
        [TestMethod()]
        public void SendTest()
        {
            string localphonenumber = "0664395976";
            string countrycode = "33";
            string message = "Test SMS Guillaume";
            string expected = "ok";
            string actual;
            actual = SMSSender.Send(localphonenumber, countrycode, message);
            Assert.AreEqual(expected, actual);
        }
    }
}
