using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;
using Essensys.Common;

namespace Essensys.Service.Fonctions
{
    public static class VoletService
    {
        public static void RegisterNewAction(EsMachine m, List<EsCouple> volets)
        {
            // Envoi de bits unis pour volets
            Dictionary<int, string> ToSend = new Dictionary<int, string>();
            foreach (EsCouple vol in volets)
            {
                string v = "0";
                if (ToSend.ContainsKey(vol.Index))
                    v = ToSend[vol.Index];
                int nv = Convert.ToInt32(v) | Convert.ToInt32(vol.Value);
                if (ToSend.ContainsKey(vol.Index))
                    ToSend[vol.Index] = nv.ToString();
                else
                    ToSend.Add(vol.Index, nv.ToString());
            }

            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add((int)Tbb_Donnees_Index.Scenario, "1");
            dic.Add(605, "0");
            dic.Add(606, "0");
            dic.Add(607, "0");
            dic.Add(608, "0");
            dic.Add(609, "0");
            dic.Add(610, "0");
            dic.Add(611, "0");
            dic.Add(612, "0");
            dic.Add(613, "0");
            dic.Add(614, "0");
            dic.Add(615, "0");
            dic.Add(616, "0");
            dic.Add(617, "0");
            dic.Add(618, "0");
            dic.Add(619, "0");
            dic.Add(620, "0");
            dic.Add(621, "0");
            dic.Add(622, "0");

            foreach (KeyValuePair<int, string> d in ToSend)
            {
                LogManager.LogTrace("Volet éclairage transmission " + d.Key.ToString() + " = " + d.Value, null);
                if (!dic.ContainsKey(d.Key))
                    dic.Add(d.Key, d.Value);
                else
                    dic[d.Key] = d.Value;
            }

            new ActionService().RegisterAction(m, EsActionType.VOLETS, "Combinaison Volets/éclairage ouverts/fermés", dic);
        }

        public static void RegisterAction(EsMachine m)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add((int)Tbb_Donnees_Index.Scenario, "1");
            dic.Add(617, "0");
            dic.Add(618, "0");
            dic.Add(619, "0");
            dic.Add(620, "63");
            dic.Add(621, "31");
            dic.Add(622, "7");

            new ActionService().RegisterAction(m, EsActionType.VOLETS, "Volets fermés", dic);
        }
    }
}
