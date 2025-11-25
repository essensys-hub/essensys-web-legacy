using Essensys.Service.Fonctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Essensys.Common;

namespace Essensys.Web.UI.Tests
{
    
    
    /// <summary>
    ///Classe de test pour AlarmeServiceTest, destinée à contenir tous
    ///les tests unitaires AlarmeServiceTest
    ///</summary>
    [TestClass()]
    public class AlarmeServiceTest
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
        ///Test pour EncryptString
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Essensys.Service.dll")]
        public void EncryptStringTest()
        {
            string alarmeorder = "ALARMEOFF_20130612_153252";
            string strKey = "75299471E8B496798ABE138D";
            string expected = "189;155;189;12;115;63;243;15;51;134;111;21;223;181;47;41";
            string actual;
            actual = AlarmeService_Accessor.EncryptString(alarmeorder, strKey);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Test pour EncryptString
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Essensys.Service.dll")]
        public void EncryptStringMD5Test()
        {
            string test = "5D236630A912BC8B78A8514C";
            string expected = "fd29f774866a28bf";
            string actual = HashHelper.GetHash(test, HashHelper.HashType.MD5);
            Assert.AreEqual(expected, actual);
        }
    }
}
