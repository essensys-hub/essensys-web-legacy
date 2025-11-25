using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Repository.DTO;
using Essensys.Service.Response;
using Essensys.Common;
using NHibernate;
using NHibernate.Criterion;
using Essensys.Service.Phone;
using NHibernate.Transform;
using System.Web;
using System.Configuration;

namespace Essensys.Service.Transaction
{
    public class StateService
    {
        protected EsStateRepository _rep;
        protected EsStateIndexRepository _repIdx;

        public StateService()
        {
            _rep = new EsStateRepository(EsSessionFactory.GetSession());
            _repIdx = new EsStateIndexRepository(EsSessionFactory.GetSession());
        }

        public DateTime GetLastCall(EsMachine m)
        {
            ICriteria crit = EsSessionFactory.GetSession().CreateCriteria<EsState>();
            crit.Add(Restrictions.Eq("Machine", m));
            crit.AddOrder(Order.Desc("StateDate"));
            crit.SetFirstResult(0).SetMaxResults(1);
            EsState st = _rep.FindByCriteria(crit);
            if (st != null)
                return st.StateDate;
            else
                return new DateTime(1900, 1, 1);
        }

        public IList<int> ListMachineWithNoSynchro()
        {
            string lims = "60";
            if (ConfigurationManager.AppSettings["notifedf.seconds"] != null)
                lims = ConfigurationManager.AppSettings["notifedf.seconds"];
            string sql = "SELECT M.ID FROM ES_MACHINE M WHERE M.ID NOT IN (SELECT S.MACHINE_ID FROM ES_STATE S WHERE S.STATEDATE >= DATEADD(s, " + lims + ", GETDATE()))";
            if (ConfigurationManager.AppSettings["notifedf.limitid"] != null)
                sql += " AND M.ID = " + ConfigurationManager.AppSettings["notifedf.limitid"];
            return EsSessionFactory.GetSession().CreateSQLQuery(sql)
                .List<int>();
        }

        public List<EsStateIndex> LastSynchro(EsMachine m, DateTime LastCall)
        {
            EsState st = _rep.FindBy(s => s.Machine == m && s.Completed && s.StateDate > LastCall);
            LogManager.LogTrace("STATE=" + st.Id, null);
            List<EsStateIndex> si = new EsStateIndexRepository(EsSessionFactory.GetSession()).List(i => i.State.Id == st.Id, 0, 0).ToList();
            LogManager.LogTrace("STATEINDEX COUNT=" + si.Count.ToString(), null);
            return si;
        }

        public bool AllActionsOK(EsMachine m)
        {
            LogManager.LogTrace("Test AllActionsOK", null);
            return (new EsActionRepository(EsSessionFactory.GetSession()).Count(a => a.Machine == m && !a.IsDone, "Id") == 0);
        }

        public bool HasRefreshed(EsMachine m, DateTime LastCall)
        {
            LogManager.LogTrace("Test State sur " + m.Id.ToString() + " Date=" + LastCall.ToString("dd/MM/yyyy HH:mm:ss"), null); 
            return (_rep.Count(s => s.Machine == m && s.Completed && s.StateDate > LastCall, "Id") > 0);
        }

        /// <summary>
        /// Persiste l'état d'une machine à travers les informations envoyées
        /// </summary>
        public void RegisterState(EsMachine m, List<EsKeyValue> vals, string version)
        {
            bool autorisealarme = true;

            if (!version.ToUpper().StartsWith("V"))
                throw new EssensysException();

            // Vérifie que toutes les clés sont présentes
            foreach (int key in new ServerService().GetDataIndex())
            {
                if (!vals.Select(v => v.k).Contains(key))
                    throw new EssensysException();
            }

            // Enregistre l'état global
            EsState st = new EsState();
            st.StateDate = DateTime.Now;
            st.Version = version;
            st.Completed = false;
            st.Machine = m;
            _rep.Add(st);

            if (m.Version != version)
            {
                m.Version = version;
                new EsMachineRepository(EsSessionFactory.GetSession()).Update(m);
                HttpContext.Current.Session["Machine"] = m;
            }

            // Enregistre chaque élément
            foreach (EsKeyValue kv in vals)
            {
                RegisterKeyValue(st, kv.k, kv.v);
                
                // Spécifique blocage alarme
                if (kv.k == (int)Tbb_Donnees_Index.Alarme_AccesADistance && kv.v == "0")
                {
                    autorisealarme = false;
                }

                // Spécifique alarme
                LogManager.LogTrace("Clé " + kv.k, null);
                if (kv.k ==(int)Tbb_Donnees_Index.Alerte && kv.v != "0")
                {
                    bool bypasssend = false;
                    LogManager.LogTrace("Clé ALERTE Valeur = '" + kv.v + "'", null); 
                    if (kv.v.Length > 0 && kv.v.Substring(0, 1) == "1")
                    {
                        new PhoneService().SendSMSAlert("ALARME", "ALERTE ESSENSYS. Le système d'alarme a détecté une intrusion.", m);
                        bypasssend = true;
                    }
                    if (kv.v.Length > 0 && kv.v.Substring(0, 1) == "0")
                    {
                        LogManager.LogTrace("No SMS ALARME", null);
                        new PhoneService().NoSMSAlert("ALARME", m);
                    }
                    if (kv.v.Length > 1 && kv.v.Substring(1, 1) == "1")
                    {
                        new PhoneService().SendSMSAlert("LAVELINGE", "ALERTE ESSENSYS. Une fuite lave linge a été détectée.", m);
                        bypasssend = true;
                    }
                    if (kv.v.Length > 1 && kv.v.Substring(1, 1) == "0")
                    {
                        LogManager.LogTrace("No SMS LAVELINGE", null);
                        new PhoneService().NoSMSAlert("LAVELINGE", m);
                    }
                    if (kv.v.Length > 2 && kv.v.Substring(2, 1) == "1")
                    {
                        new PhoneService().SendSMSAlert("LAVEVAISSELLE", "ALERTE ESSENSYS. Une fuite lave vaisselle a été détectée.", m);
                    }
                    if (kv.v.Length > 2 && kv.v.Substring(2, 1) == "0")
                    {
                        LogManager.LogTrace("No SMS LAVEVAISSELLE", null);
                        new PhoneService().NoSMSAlert("LAVEVAISSELLE", m);
                    }
                }
            }

            // Gestion Autorisation de l'alarme
            m.AutoriseAlarme = autorisealarme;
            new EsMachineRepository(EsSessionFactory.GetSession()).Update(m);
            LogManager.LogTrace("Update machine ok", null);
            HttpContext.Current.Session["Machine"] = m;

            st.Completed = true;
            _rep.Update(st);
        }

        /// <summary>
        /// Enregistre la valeur d'une clé dans l'état
        /// </summary>
        /// <param name="state">Etat</param>
        /// <param name="key">Clé</param>
        /// <param name="value">Valeur</param>
        private void RegisterKeyValue(EsState state, int key, string value)
        {
            EsDataIndex idx = new ServerService().FindDataIndex(key);
            if (_repIdx.Count(st => st.State == state && st.Index == idx, "Id") == 0)
            {
                EsStateIndex si = new EsStateIndex();
                si.Index = idx;
                si.State = state;
                si.Value = value;
                _repIdx.Add(si);
            }
        }
    }
}
