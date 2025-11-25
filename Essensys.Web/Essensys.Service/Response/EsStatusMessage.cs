using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Service.Response
{
    public class EsStatusMessage
    {
        public List<EsKeyValue> ek { get; set; }
        public string version { get; set; }
    }
}
