using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Essensys.Common;
using NHibernate;
using Essensys.Repository;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;
using Essensys.Service.Phone;
using Essensys.Repository.DAO;

namespace Essensys.Notifier
{
    public partial class EssensysNotifier : ServiceBase
    {
        protected ISessionFactory _SessionFactory;

        static void Main()
        {
            ServiceBase.Run(new EssensysNotifier());
        }

        public EssensysNotifier()
        {
            this.ServiceName = "Essensys Notifier";
            this.EventLog.Log = "Application";
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogManager.Initialise();

                EsSessionFactory.InitSessionFactory();
                _SessionFactory = EsSessionFactory.SessionFactory;

                LogManager.LogTrace("Initialisation Session Factory effectué", null);

                Thread t = new Thread(new ThreadStart(this.DoTask));
                t.Start();
            }
            catch (Exception ex)
            {
                LogManager.LogError("Erreur dans l'initialisation du service", ex);
            }
        }

        protected override void OnStop()
        {
            _SessionFactory.Close();
        }

        private void DoTask()
        {
            while (true)
            {
                try
                {
                    // Retrait des machines n'ayant pas recu de notification d'état
                    foreach (int i in new StateService().ListMachineWithNoSynchro())
                    {
                        EsMachine m = new EsMachineRepository(EsSessionFactory.GetSession()).FindBy(i);
                        new PhoneService().SendSMSAlert("NOSYNC", "ALERTE ESSENSYS. Un défaut de liaison avec le serveur a été détecté, probablement dû à une coupure d'électricité.", m);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogError("Erreur dans le service CRM Réception d'email invalides", ex);
                }

                Thread.Sleep(300000);
            }
        }
    }
}
