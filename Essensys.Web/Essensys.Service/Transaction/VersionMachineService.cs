using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Repository.DTO;
using NHibernate;
using NHibernate.Criterion;
using Essensys.Common;

namespace Essensys.Service.Transaction
{
    public class VersionMachineService
    {
        protected EsVersionMachineRepository _rep;

        public VersionMachineService()
        {
            _rep = new EsVersionMachineRepository(EsSessionFactory.GetSession());
        }

        public int Step(EsMachine m, string version)
        {
            EsVersionMachine vm = GetVersionLog(m, version);
            return vm.Lastindexcall;
        }

        public bool HasFinished(EsMachine m, string version)
        {
            EsVersionMachine vm = GetVersionLog(m, version);
            return (vm.IsOk && vm.Lastindexcall > -1);
        }

        public void Purge(EsMachine m)
        {
            EsSessionFactory.GetSession().CreateSQLQuery("DELETE FROM ES_VERSIONMACHINE WHERE MACHINE_ID=" + m.Id + " AND ISOK=0 AND LASTINDEXCALL > -1").ExecuteUpdate();
        }

        public EsVersionMachine GetVersionLog(EsMachine m, string version)
        {
            ICriteria c = EsSessionFactory.GetSession().CreateCriteria<EsVersionMachine>();
            c.CreateAlias("Machine", "m");
            c.Add(Restrictions.Eq("m.Id", m.Id));
            c.Add(Restrictions.Eq("Version", version));

            LogManager.LogTrace("Version Log", null);
            List<EsVersionMachine> l = _rep.ListByCriteria(c, 0, 0).ToList();
            if (l.Count > 0)
            {
                if (l.Count == 1)
                    return l[0];
                else
                    return l.OrderByDescending(lv => lv.Dateaction).First();
            }
            else
                return null;
        }

        public void StartLogVersion(EsMachine m, string version)
        {
            EsSessionFactory.GetSession().Flush();
            EsVersionMachine vm = new EsVersionMachine();
            vm.Version = version;
            vm.Machine = m;
            vm.Lastindexcall = -1;
            vm.Dateaction = DateTime.Now;
            _rep.Add(vm);
        }

        public void LogGetVersion(EsMachine m, string version, int lastindexcall)
        {
            EsVersionMachine vm = GetVersionLog(m, version);
            if (vm != null)
            {
                vm.Lastindexcall = lastindexcall;
                vm.Dateaction = DateTime.Now;
                _rep.Update(vm);
            }
            else
                throw new Exception("L'utilisateur n'a pas déclenché le téléchargement");
        }

        public void VersionOK(EsMachine m, string version)
        {
            EsVersionMachine vm = GetVersionLog(m, version);
            vm.Dateaction = DateTime.Now;
            vm.IsOk = true;
            _rep.Update(vm);
        }
    }
}
