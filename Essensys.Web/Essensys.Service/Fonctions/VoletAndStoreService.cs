using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;

namespace Essensys.Service.Fonctions
{
    public static class VoletAndStoreService
    {
        public static void RegisterAction(EsMachine m)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add((int)Tbb_Donnees_Index.Scenario, "1");
            dic.Add(617, "0");
            dic.Add(618, "0");
            dic.Add(619, "8");
            dic.Add(620, "63");
            dic.Add(621, "31");
            dic.Add(622, "7");

            new ActionService().RegisterAction(m, EsActionType.VOLETS, "Volets fermés et store remonté", dic);
        }
    }
}
