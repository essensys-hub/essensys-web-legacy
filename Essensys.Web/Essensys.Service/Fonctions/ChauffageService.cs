using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;

namespace Essensys.Service.Fonctions
{
    public static class ChauffageService
    {
        public static void RegisterAction(EsMachine m, string val, string zone)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string action = "";
            switch (zone)
            {
                case "zj":
                    dic.Add((int)Tbb_Donnees_Index.Chauf_zj_Mode, val);
                    action = "Chauffage zj en mode " + val;
                    break;
                case "zn":
                    dic.Add((int)Tbb_Donnees_Index.Chauf_zn_Mode, val);
                    action = "Chauffage zn en mode " + val;
                    break;
                case "sdb1":
                    dic.Add((int)Tbb_Donnees_Index.Chauf_zsb1_Mode, val);
                    action = "Chauffage sdb1 en mode " + val;
                    break;
                case "sdb2":
                    dic.Add((int)Tbb_Donnees_Index.Chauf_zsb2_Mode, val);
                    action = "Chauffage sdb2 en mode " + val;
                    break;
            }

            new ActionService().RegisterAction(m, EsActionType.CHAUFFAGE, action, dic);
        }
    }
}
