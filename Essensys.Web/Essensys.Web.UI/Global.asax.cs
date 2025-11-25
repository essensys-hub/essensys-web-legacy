using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Essensys.Common;
using Essensys.Repository;
using System.Web.Optimization;
using System.Web.SessionState;
using Essensys.Service.Security;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;
using System.Web.Helpers;

namespace Essensys.Web.UI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string _WebApiPrefix = "api";
        private static string _WebApiExecutionPath = String.Format("~/{0}", _WebApiPrefix);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            LogManager.Initialise();
            EsSessionFactory.InitSessionFactory();
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        protected void Session_End(Object sender, EventArgs e)
        {
            if (Session["User"] != null)
            {
                LogManager.LogTrace("End session machine=" + (Session["User"] as EsUser).Machine.Id.ToString(), null);
                LogManager.LogTrace("End session user=" + (Session["User"] as EsUser).Id.ToString(), null);

                new ActionService().UndoAllActions((Session["User"] as EsUser).Machine);
                ConnectionInfo.Disconnect((Session["User"] as EsUser).Machine.Id);
            }
        }
        
        private static bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(_WebApiExecutionPath);
        }
    }
}