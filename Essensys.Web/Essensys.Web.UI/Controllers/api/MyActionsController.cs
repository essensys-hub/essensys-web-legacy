using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Essensys.Service.Security;
using System.Web.Http;
using Essensys.Service.Response;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Common;

namespace Essensys.Web.UI.Controllers.api
{
    /// <summary>
    /// Méthode API myactions
    /// </summary>
    [EssensysAuthorize()]
    public class MyActionsController : ApiController
    {
        public EsActionsInfo Get()
        {
            LogManager.LogTrace("MyActions/GET", null);
            ActionService serv = new ActionService();
            EsMachine m = HttpContext.Current.Session["Machine"] as EsMachine;

            EsActionsInfo i = new EsActionsInfo();
            EsAction alact = serv.GetAlarmeAction(m);
            EsAlarmAction al = new EsAlarmAction();
            if (alact != null)
            {
                al.guid = alact.Guid;
                al.obl = alact.ActionInfo;
                i._de67f = al;
            }
            i.actions = serv.ListActions(m);
            return i;
        }
    }
}