using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Essensys.Service.Security;
using System.Net.Http;
using Essensys.Service.Response;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Common;

namespace Essensys.Web.UI.Controllers.api
{
    /// <summary>
    /// Méthode API MyStatus
    /// </summary>
    [EssensysAuthorize()]
    public class MyStatusController : ApiController
    {
        public HttpResponseMessage Post(EsStatusMessage mes)
        {
            LogManager.LogTrace("MyStatus/POST", null);
            if (mes.ek == null)
                throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
            else
            {
                try
                {
                    new StateService().RegisterState(HttpContext.Current.Session["Machine"] as EsMachine, mes.ek, mes.version);
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Created);
                }
                catch (EssensysException)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                }
            }
        }
    }
}