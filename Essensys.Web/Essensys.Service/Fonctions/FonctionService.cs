using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Repository;
using NHibernate;
using System.Web;

namespace Essensys.Service.Fonctions
{
    /// <summary>
    /// Type d'action recensé
    /// </summary>
    public enum EsActionType
    {
        CHAUFFAGE,
        ALARME,
        ARROSAGE,
        CUMULUS,
        VOLETS,
        STORE
    }

    /// <summary>
    /// Classe de base de service
    /// </summary>
    public static class FonctionService
    {
        public static void DoActions(EsMachine m,
            bool newar, string ar,
            bool newal, string al, string alresp, string codealarme,
            bool newcf, string cfzj,
            bool newcfzn, string cfzn,
            bool newcfsdb1, string cfsdb1, bool newcfsdb2, string cfsdb2,
            bool newcm, string cfcm, string cfvol, string cfsto,
            List<EsCouple> volets)
        {
            ITransaction tc = EsSessionFactory.GetSession().BeginTransaction();

            try
            {
                if (volets.Count > 0)
                    VoletService.RegisterNewAction(m, volets);
                if (newar)
                    ArrosageService.RegisterAction(m, ar);
                if (newal)
                {
                    EsUser u = HttpContext.Current.Session["User"] as EsUser;
                    AlarmeService.RegisterAction(u, m, (al == "on"), alresp, codealarme);
                }
                if (newcf)
                {
                    ChauffageService.RegisterAction(m, cfzj, "zj");
                }
                if (newcfzn)
                {
                    ChauffageService.RegisterAction(m, cfzn, "zn");
                }
                if (newcfsdb1)
                {
                    ChauffageService.RegisterAction(m, cfsdb1, "sdb1");
                }
                if (newcfsdb2)
                {
                    ChauffageService.RegisterAction(m, cfsdb2, "sdb2");
                }
                if (newcm)
                {
                    CumulusService.RegisterAction(m, cfcm);
                }
                if (cfvol == "true" && cfsto == "true")
                {
                    VoletAndStoreService.RegisterAction(m);
                }
                if (cfvol == "true" && cfsto != "true")
                {
                    VoletService.RegisterAction(m);
                }
                if (cfsto == "true" && cfvol != "true")
                {
                    StoreService.RegisterAction(m);
                }
                tc.Commit();
            }
            catch (Exception ex)
            {
                tc.Rollback();
                throw new Exception(ex.Message);
            }
        }
    }

    public class EsCouple
    {
        public int Index { get; set; }
        public string Value { get; set; }
    }
}
