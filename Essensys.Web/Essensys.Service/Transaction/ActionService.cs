using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Response;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Service.Fonctions;
using Essensys.Common;

namespace Essensys.Service.Transaction
{
    public class ActionService
    {
        protected EsActionRepository _rep;
        protected EsActionIndexRepository _repi;

        public ActionService()
        {
            _rep = new EsActionRepository(EsSessionFactory.GetSession());
            _repi = new EsActionIndexRepository(EsSessionFactory.GetSession());
        }

        /// <summary>
        /// Retourne une action correspondant à un changement de l'alarme
        /// </summary>
        /// <param name="m">Machine</param>
        /// <returns>Action associée</returns>
        public EsAction GetAlarmeAction(EsMachine m)
        {
            LogManager.LogTrace("GetAlarmeAction on machine " + m.Id.ToString(), null);
            return _rep.FindBy(a => a.Machine == m && !a.IsDone && a.ActionType == "ALARME" && a.ActionInfo != "");
        }

        /// <summary>
        /// Retourne la liste des actions à effectuer pour une machine
        /// </summary>
        /// <param name="m">Machine</param>
        /// <returns>Actions</returns>
        public List<EsActionInfo> ListActions(EsMachine m)
        {
            List<EsActionInfo> ai = new List<EsActionInfo>();

            foreach (EsAction ac in _rep.List(a => a.Machine == m && !a.IsDone && a.ActionType != "ALARME", 0, 0))
            {
                EsActionInfo i = new EsActionInfo();
                i.guid = ac.Guid;
                
                List<EsKeyValue> lkv = new List<EsKeyValue>();
                foreach (EsActionIndex eai in ac.Indexes)
                {
                    EsKeyValue kv = new EsKeyValue();
                    kv.k = Convert.ToInt32(eai.Index.IndexKey);
                    kv.v = eai.Value;
                    lkv.Add(kv);
                }
                i.@params = lkv;
                ai.Add(i);
            }
            return ai;
        }

        public void AcquitAllActions(EsMachine m)
        {
            foreach (EsAction a in _rep.List(ac => ac.Machine == m && !ac.IsDone, 0, 0))
            {
                a.IsDone = true;
                _rep.Update(a);
            }
        }
        public void UndoAllActions(EsMachine m)
        {
            LogManager.LogTrace("Undo Action for machine " + m.Pkey, null);
            foreach (EsAction a in _rep.List(ac => ac.Machine == m && !ac.IsDone, 0, 0))
            {
                try
                {
                    foreach (EsActionIndex ai in _repi.List(ai => ai.Action == a, 0, 0))
                    {
                        a.Indexes.Remove(ai);
                        _repi.Delete(ai);
                    }
                    _rep.Delete(a);
                }
                catch (Exception ex)
                {
                    LogManager.LogError("Unable to delete action for machine " + m.Id.ToString(), ex);
                }
            }
        }
        public void AcquitAction(EsMachine m, string guid)
        {
            EsAction a = _rep.FindBy(ac => ac.Machine == m && !ac.IsDone && ac.Guid == guid);
            if (a == null)
                throw new EssensysException();
            else
            {
                a.IsDone = true;
                _rep.Update(a);
            }
        }
        public void RegisterAction(EsMachine m, EsActionType actiontype, string actioninfo)
        {
            if (actiontype != EsActionType.ALARME)
                throw new Exception("Cette méthode est uniquement disponible pour un type alarme");
            else
            {
                EsAction a = new EsAction();
                a.DateCreation = DateTime.Now;
                a.ActionType = actiontype.ToString();
                a.ActionInfo = actioninfo;
                a.Guid = Guid.NewGuid().ToString();
                a.Machine = m;

                if (actioninfo == "")
                {
                    EsActionIndex ai = new EsActionIndex();
                    ai.Action = a;
                    ai.Index = new EsDataIndex();
                    ai.Value = "";
                }
                
                _rep.Add(a);
            }
        }

        /// <summary>
        /// Met en attente une nouvelle action
        /// </summary>
        /// <param name="m">Machine concernée</param>
        /// <param name="actiontype">Type d'action</param>
        /// <param name="actioninfo">Information action</param>
        /// <param name="Indexes">Index associés</param>
        public void RegisterAction(EsMachine m, EsActionType actiontype, string actioninfo, Dictionary<int, string> Indexes)
        {
            EsAction a = new EsAction();
            a.DateCreation = DateTime.Now;
            a.ActionType = actiontype.ToString();
            a.ActionInfo = actioninfo;
            a.Guid = Guid.NewGuid().ToString();
            a.Machine = m;

            EsDataIndexRepository _repdi = new EsDataIndexRepository(EsSessionFactory.GetSession());
            List<EsActionIndex> lai = new List<EsActionIndex>();
            foreach (KeyValuePair<int, string> kv in Indexes)
            {
                EsActionIndex ai = new EsActionIndex();
                ai.Action = a;
                ai.Index = _repdi.FindBy(di => di.IndexKey == Convert.ToInt32(kv.Key).ToString());
                ai.Value = kv.Value;
                lai.Add(ai);
            }
            a.Indexes = lai;
            _rep.Add(a);
        }
    }
}
