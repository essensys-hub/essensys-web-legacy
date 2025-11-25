using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;

namespace Essensys.Service.Fonctions
{
    public static class CumulusService
    {
         public static void RegisterAction(EsMachine m, string val)
         {
             Dictionary<int, string> dic = new Dictionary<int, string>();
             dic.Add((int)Tbb_Donnees_Index.Cumulus_Mode, val);
             new ActionService().RegisterAction(m, EsActionType.CUMULUS, "Cumulus en mode " + val, dic);
         }
    }
}
