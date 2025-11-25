using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essensys.Common;
using Essensys.Repository;

namespace Essensys.Web.UI.Tests
{
    [TestClass()]
    public class TestInit
    {
        #region Attributs de tests supplémentaires
        [AssemblyInitialize]
        public static void Classinit(TestContext context)
        {
            LogManager.Initialise();
            try
            {
                EsSessionFactory.InitSessionFactory();
            }
            catch { }
        }
        #endregion
    }
}
