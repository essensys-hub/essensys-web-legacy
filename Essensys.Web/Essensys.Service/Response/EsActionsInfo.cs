using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Service.Response
{
    public class EsActionsInfo
    {
        public EsAlarmAction _de67f { get; set; }
        public List<EsActionInfo> actions { get; set; }
    }

    public class EsAlarmAction
    {
        public string guid { get; set; }
        public string obl { get; set; }
    }

    public class EsActionInfo
    {
        public string guid { get; set; }
        public List<EsKeyValue> @params { get; set; }
    }
}
