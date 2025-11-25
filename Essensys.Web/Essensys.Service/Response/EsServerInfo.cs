using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Service.Response
{
    public class EsServerInfo
    {
        public bool isconnected { get; set; }
        public List<int> infos { get; set; }
        public string newversion { get; set; }
    }
}
