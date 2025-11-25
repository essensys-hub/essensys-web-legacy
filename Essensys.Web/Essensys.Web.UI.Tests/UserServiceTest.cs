using Essensys.Service.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Essensys.Repository.DTO;
using Essensys.Repository;
using Essensys.Repository.DAO;
using System.Text;
using Essensys.Common;
using Essensys.Service.Transaction;
using System.Configuration;

namespace Essensys.Web.UI.Tests
{
    
    
    /// <summary>
    ///Classe de test pour UserServiceTest, destinée à contenir tous
    ///les tests unitaires UserServiceTest
    ///</summary>
    [TestClass()]
    public class UserServiceTest
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
        ///Test pour IsConnected
        ///</summary>
        [TestMethod()]
        public void IsConnectedTest()
        {
            UserService target = new UserService();
            EsMachine m = new EsMachineRepository(EsSessionFactory.GetSession()).FindBy(2);
            bool actual = target.IsConnected(m);
            Assert.IsTrue(!actual);
        }

        [TestMethod()]
        public void AuthTest()
        {
            string cle = "OGFiZWY2ODIzM2IzYTAxOTo1NjdhNWU0OWM2NTk1NzZh";
            string[] parsedHeader = Encoding.UTF8.GetString(Convert
                                                            .FromBase64String(cle))
                                                            .Split(
                                                            new[] { ':' });
            string cryptedcode = parsedHeader[0] + parsedHeader[1];
            Assert.IsNotNull(cryptedcode);
        }

        [TestMethod()]
        public void SendMailTest()
        {
            EMailSender.SendSimpleEMail(ConfigurationManager.AppSettings["mailuser"], "glecanu@ebazten.fr", "Création de votre compte Essensys", "Votre compte utilisateur est maintenant disponible. Pour l'activer et obtenir le code d'activation à renseigner sur votre terminal Essensys, cliquez sur ce lien : ", "Votre compte utilisateur est maintenant disponible. Pour l'activer et obtenir le code d'activation à renseigner sur votre terminal Essensys, cliquez sur le lien ci-dessous : <br/><a href='#'>Cliquez ici pour valider la création de votre compte</a>.<br/><br/>Nous vous remercions d'utiliser les produits Essensys.");
            Assert.IsTrue(1 == 1);            
        }

        [TestMethod()]
        public void ReverseAuthTest()
        {
            string code = "69040375153517786851264098156348";
            string md5 = HashHelper.GetHash(code, HashHelper.HashType.MD5);
            string base64 = md5.Substring(0, 16) + ":" + md5.Substring(16, 16);
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(base64);
            base64 = System.Convert.ToBase64String(toEncodeAsBytes);
            Assert.IsNotNull(base64);
        }
    }
}
