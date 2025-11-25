using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;

namespace Essensys.Service.Fonctions
{
    public static class StoreService
    {
        public static void RegisterAction(EsMachine m)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add((int)Tbb_Donnees_Index.Scenario, "1");
            dic.Add(617, "0");
            dic.Add(618, "0");
            dic.Add(619, "8");
            dic.Add(620, "0");
            dic.Add(621, "0");
            dic.Add(622, "0");

            new ActionService().RegisterAction(m, EsActionType.STORE, "Store replié", dic);
        }
    }
}
