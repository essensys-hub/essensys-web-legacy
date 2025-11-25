using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Essensys.Service.Security;
using System.Net.Http;
using System.Net;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;

namespace Essensys.Web.UI.Controllers.api
{
    [EssensysAuthorize()]
    public class EndVersionContentController : ApiController
    {
        public HttpResponseMessage Post()
        {
            new VersionMachineService().VersionOK(HttpContext.Current.Session["Machine"] as EsMachine, "V" + new ServerService().GetVersionServer().Id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}