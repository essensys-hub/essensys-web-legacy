using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Essensys.Service.Security;
using System.Web.Http;
using System.Net.Http;
using Essensys.Service.Response;
using Essensys.Service.Transaction;

namespace Essensys.Web.UI.Controllers.api
{
    [EssensysAuthorize()]
    public class GetVersionContentController : ApiController
    {
        public EsVersionPart Post(int id)
        {
            EsVersionPart vp = new ServerService().GetVersionPart(id);

            return vp;
        }
    }
}