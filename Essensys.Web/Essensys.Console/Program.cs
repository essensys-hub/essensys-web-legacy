using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Configuration;
using Essensys.Common;
using Essensys.Repository;
using NHibernate;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Service.Phone;
using Essensys.Repository.DAO;

namespace Essensys.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection argsCollection = ProcessCommandLineArgs(args);

            LogManager.Initialise();

            string command = argsCollection["-o"];

            if (command == "deletestate")
            {
                DeleteState();
            }
            if (command == "notifyEDF")
            {
                NotifyEDF();
            }
        }

        /// <summary>
        /// Processes the command line arguments and adds them to a collection of name-value pairs.
        /// Does not perform error checking on command-line syntax.
        /// </summary>
        /// <param name="args">The argument list passed in to Main.</param>
        /// <returns>The command line arguments as name-value pairs.</returns>
        private static NameValueCollection ProcessCommandLineArgs(string[] args)
        {
            NameValueCollection argCollection = new NameValueCollection();

            for (int i = 0; i < args.Length; i += 2)
            {
                argCollection.Add(args[i], args[i + 1]);
            }

            return argCollection;
        }

        private static void NotifyEDF()
        {
            LogManager.Initialise();

            EsSessionFactory.InitSessionFactory();
            ISessionFactory _SessionFactory = EsSessionFactory.SessionFactory;

            LogManager.LogTrace("Initialisation Session Factory effectué", null);

            List<EsMachine> lm = new EsMachineRepository(EsSessionFactory.GetSession()).List(m => m.IsActive, 0, 0).ToList();
            IList<int> lc = new StateService().ListMachineWithNoSynchro();
            if (ConfigurationManager.AppSettings["notifedf.limitid"] != null)
            {
                lc = new List<int>();
                lc.Add(Convert.ToInt32(ConfigurationManager.AppSettings["notifedf.limitid"]));
            }
            if (lc != null)
            {
                foreach (EsMachine m in lm)
                {
                    if (lc.Count(mc => mc == m.Id) > 0)
                    {
                        LogManager.LogTrace("Alerte EDF sur " + m.Id.ToString(), null);
                        new PhoneService().SendSMSAlert("NOSYNC", "ALERTE ESSENSYS. Un défaut de liaison avec le serveur a été détecté, probablement dû à une coupure d'électricité.", m);
                    }
                    else
                    {
                        LogManager.LogTrace("Pas Alerte EDF sur " + m.Id.ToString(), null);
                        new PhoneService().NoSMSAlert("NOSYNC", m);
                    }
                }
            }
            _SessionFactory.Close();
        }

        private static void DeleteState()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Essensys"].ConnectionString))
            {
                conn.Open();
                string sql = "delete from es_stateindex where state_id in (select id from es_state where statedate < dateadd(hour, -" + ConfigurationManager.AppSettings["histo.dayretain"] + ", getdate()))";
                System.Console.WriteLine(sql);
                new SqlCommand(sql, conn).ExecuteNonQuery();
                sql = "delete from es_state where statedate < dateadd(hour, -" + ConfigurationManager.AppSettings["histo.dayretain"] + ", getdate())";
                System.Console.WriteLine(sql);
                new SqlCommand(sql, conn).ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
