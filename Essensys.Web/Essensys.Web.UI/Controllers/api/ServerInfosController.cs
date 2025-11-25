using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Essensys.Service.Response;
using Essensys.Service.Security;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Common;

namespace Essensys.Web.Controllers.api
{
    /// <summary>
    /// Méthode API ServerInfo
    /// </summary>
    [EssensysAuthorize()]
    public class ServerInfosController : ApiController
    {
        /// <summary>
        /// Renvoie les clés nécessaires au serveur web et si l'utilisateur concerné est connecté
        /// </summary>
        /// <returns></returns>
        public EsServerInfo Get()
        {
            LogManager.LogTrace("ServerInfos/GET", null);
            ServerService sserv = new ServerService();
            EsServerInfo info = new EsServerInfo();
            info.isconnected = new UserService().IsConnected(HttpContext.Current.Session["Machine"] as EsMachine);
            info.infos = sserv.GetDataIndex();
            
            EsVersion v = sserv.GetVersionServer();
            if (v != null)
            {
                string version = "V" + v.Id;
                EsMachine m = HttpContext.Current.Session["Machine"] as EsMachine;
                new VersionMachineService().Purge(HttpContext.Current.Session["Machine"] as EsMachine);

                EsVersionMachine vm = new VersionMachineService().GetVersionLog(HttpContext.Current.Session["Machine"] as EsMachine, version);
                if (vm != null && m.Version != version && !vm.IsOk)
                {
                    LogManager.LogTrace("Une version est en cours de téléchargement", null);
                    info.newversion = version;
                }
                else
                    info.newversion = "no";
            }
            else
                info.newversion = "no";
            return info;
        }
    }
}