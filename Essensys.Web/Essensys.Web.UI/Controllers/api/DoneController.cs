using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Essensys.Service.Security;
using System.Net.Http;
using Essensys.Common;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;

namespace Essensys.Web.UI.Controllers.api
{
    /// <summary>
    /// Méthode API Done
    /// </summary>
    [EssensysAuthorize()]
    public class DoneController : ApiController
    {
        public HttpResponseMessage Post(string id)
        {
            try
            {
                LogManager.LogTrace("Done/POST", null);
                new ActionService().AcquitAction(HttpContext.Current.Session["Machine"] as EsMachine, id);
                return new HttpResponseMessage(System.Net.HttpStatusCode.Created);
            }
            catch(EssensysException)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }
        }
    }
}