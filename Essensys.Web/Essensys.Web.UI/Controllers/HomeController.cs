using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Essensys.Common;
using System.Configuration;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;
using Essensys.Service.Fonctions;
using Essensys.Repository.DAO;
using Essensys.Repository;
using System.IO;
using Essensys.Service.Phone;

namespace Essensys.Web.UI.Controllers
{
    /// <summary>
    /// Controlleur Page d'accueil
    /// </summary>
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            if (Session["User"] == null)
                return Redirect("/Account/Login");

            // Test d'existence d'une connexion avec la box
            // Si aucune connexion n'a été effectuée, on affiche un message
            EsUser u = Session["User"] as EsUser;
            if (new StateService().GetLastCall(u.Machine).Year == 1900)
                return Redirect("/Home/NeedBox");

            EsPhone p = null;
            foreach (EsPhone ph in new PhoneService().ListUserPhone(u))
            {
                p = ph;
                break;
            }
            ViewData["NbSMS"] = Convert.ToInt32(ConfigurationManager.AppSettings["maxmonthsms"]) - new PhoneService().NbSMSSent(p);
            ViewBag.WaitBox = ConfigurationManager.AppSettings["waitessensysbox"];
            ViewBag.Question = u.Question;
            return View();
        }

        [Authorize]
        public ActionResult NeedBox()
        {
            if (Session["User"] == null)
                return Redirect("/Account/Login");

            EsUser u = Session["User"] as EsUser;
            string code = u.Machine.Pkey;
            code = code.Substring(0, 4) + "  " +
                code.Substring(4, 4) + "  " +
                code.Substring(8, 4) + "  " +
                code.Substring(12, 4) + "  " +
                code.Substring(16, 4) + "  " +
                code.Substring(20, 4) + "  " +
                code.Substring(24, 4) + "  " +
                code.Substring(28, 4);
            ViewBag.Code = code;
            return View();
        }
        
        [Authorize]
        [HttpPost]
        public JsonResult PurgeAllActions()
        {
            EsMachine m = (Session["User"] as EsUser).Machine;
            new ActionService().AcquitAllActions(m);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public JsonResult WaitActions()
        {
            StateService serv = new StateService();
            EsMachine m = (Session["User"] as EsUser).Machine;
            int i = 1;
            while (!serv.AllActionsOK(m) && i < 20)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
            }
            if (i >= 20)
                return Json(new { success = false, message = "Les modifications ne sont pas prises en compte" });
            else
            {
                return Json(new { success = true }); 
            }
        }


        [HttpPost]
        public JsonResult WaitBoxAn()
        {
            StateService serv = new StateService();
            EsMachine m = (Session["User"] as EsUser).Machine;
            DateTime LastCall = serv.GetLastCall(m);
            LogManager.LogTrace("Last Call=" + LastCall.ToString("dd/MM/yyyy HH:mm:SS"), null);
            int i = 1;

            while (!serv.HasRefreshed(m, LastCall) && i < 20)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
            }
            if (i >= 20)
            {
                // Test de téléchargement en cours
                VersionMachineService servm = new VersionMachineService();
                EsVersion v = new ServerService().GetVersionServer();
                string version = "V" + v.Id;
                EsVersionMachine vm = servm.GetVersionLog(m, version);
                if (vm != null)
                {
                    TimeSpan ts = DateTime.Now - vm.Dateaction;
                    if (!vm.IsOk && ts.Minutes < 5)
                    {
                        return Json(new { success = false, size = v.Size, step = servm.Step(m, version) });
                    }
                    else
                        return Json(new { success = false, step = 0, message = "Votre boitier Essensys ne répond pas. Cela peut être du à une mauvaise saisie de la clef d'activation sur l'écran de contrôle Essensys." });
                }
                else
                    return Json(new { success = false, step = 0, message = "Votre boitier Essensys ne répond pas. Cela peut être du à une mauvaise saisie de la clef d'activation sur l'écran de contrôle Essensys." });
            }
            else
            {
                // Test de présence d'une nouvelle version
                bool nv = false;
                EsVersion v = new ServerService().GetVersionServer();
                EsMachine mv = new EsMachineRepository(EsSessionFactory.GetSession()).FindBy(m.Id);
                Session["Machine"] = mv;
                if (v != null && Convert.ToInt32(mv.Version.Replace("V", "")) != v.Id)
                    nv = true;

                if (nv)
                {
                    bool versionnotpossible = true;
                    List<EsStateIndex> lastsynchro = serv.LastSynchro(m, LastCall);
                    if (lastsynchro.Count(s => s.Index.IndexKey == "920") > 0 && lastsynchro.Count(s => s.Index.IndexKey == "407") > 0)
                    {
                        EsStateIndex EtatBP1 = lastsynchro.First(s => s.Index.IndexKey == "920");
                        EsStateIndex AlarmeAccesDistance = lastsynchro.First(s => s.Index.IndexKey == "407");
                        if (EtatBP1.Value.Substring(0, 1) == "0" && AlarmeAccesDistance.Value == "1")
                            versionnotpossible = false;
                    }
                    return Json(new { success = true, alarmeok = mv.AutoriseAlarme, state = lastsynchro, newversion = nv, versiondesc = v.Descriptif, versionsize = v.Size, newversionnotpossible = versionnotpossible });
                }
                else
                    return Json(new { success = true, alarmeok = mv.AutoriseAlarme, state = serv.LastSynchro(m, LastCall), newversion = false });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult WaitBox()
        {
            StateService serv = new StateService();
            EsMachine m = (Session["User"] as EsUser).Machine;
            DateTime LastCall = serv.GetLastCall(m);
            LogManager.LogTrace("Home/WaitBox ------------ Last Call=" + LastCall.ToString("dd/MM/yyyy HH:mm:ss"), null);
            int i = 1;

            while (!serv.HasRefreshed(m, LastCall) && i < 40)
            {
                LogManager.LogTrace("Home/WaitBox ------------ Cycle " + i.ToString(), null);
                System.Threading.Thread.Sleep(1000);
                i++;
            }

            LogManager.LogTrace("i=" + i.ToString(), null);
            if (i >= 40)
            {
                // Test de téléchargement en cours
                VersionMachineService servm = new VersionMachineService();
                EsVersion v = new ServerService().GetVersionServer();
                string version = "V" + v.Id;
                EsVersionMachine vm = servm.GetVersionLog(m, version);
                if (vm != null)
                {
                    TimeSpan ts = DateTime.Now - vm.Dateaction;
                    LogManager.LogTrace("Home/WaitBox ------------ VersionMachine is NOT null", null);
                    if (!vm.IsOk && ts.Minutes < 5)
                    {
                        return Json(new { success = false, size = v.Size, step = servm.Step(m, version) });
                    }
                    else
                    {
                        LogManager.LogTrace("Home/WaitBox ------------ Timespan is gone", null);
                        return Json(new { success = false, step = 0, message = "Votre boitier Essensys ne répond pas. Cela peut être du à une mauvaise saisie de la clef d'activation sur l'écran de contrôle Essensys." });
                    }
                }
                else
                {
                    LogManager.LogTrace("Home/WaitBox ------------ VersionMachine is null", null);
                    return Json(new { success = false, step = 0, message = "Votre boitier Essensys ne répond pas. Cela peut être du à une mauvaise saisie de la clef d'activation sur l'écran de contrôle Essensys." });
                }
            }
            else
            {
                // Test de présence d'une nouvelle version
                LogManager.LogTrace("Test New Version", null);
                bool nv = false;
                EsVersion v = new ServerService().GetVersionServer();
                EsMachine mv = new EsMachineRepository(EsSessionFactory.GetSession()).FindBy(m.Id);
                Session["Machine"] = mv;
                if (v != null && Convert.ToInt32(mv.Version.Replace("V", "")) != v.Id)
                    nv = true;

                if (nv)
                {
                    LogManager.LogTrace("nv", null);
                    bool versionnotpossible = true;
                    List<EsStateIndex> lastsynchro = serv.LastSynchro(m, LastCall);
                    if (lastsynchro.Count(s => s.Index.IndexKey == "920") > 0 && lastsynchro.Count(s => s.Index.IndexKey == "407") > 0)
                    {
                        EsStateIndex EtatBP1 = lastsynchro.First(s => s.Index.IndexKey == "920");
                        EsStateIndex AlarmeAccesDistance = lastsynchro.First(s => s.Index.IndexKey == "407");
                        if (EtatBP1.Value.Substring(0, 1) == "0" && AlarmeAccesDistance.Value == "1")
                            versionnotpossible = false;
                    }
                    return Json(new { success = true, alarmeok = mv.AutoriseAlarme, state = lastsynchro, newversion = nv, versiondesc = v.Descriptif, versionsize = v.Size, newversionnotpossible = versionnotpossible });
                }
                else
                    return Json(new { success = true, alarmeok = mv.AutoriseAlarme, state = serv.LastSynchro(m, LastCall), newversion = false });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult WaitVersion()
        {
            try
            {
                if (Session["User"] == null)
                    return Json(new { success = false, nosession=true});
                VersionMachineService serv = new VersionMachineService();
                string version = "V" + new ServerService().GetVersionServer().Id;
                EsMachine m = (Session["User"] as EsUser).Machine;
                return Json(new { success = true, hasfinished = serv.HasFinished(m, version), step = serv.Step(m, version) });
            }
            catch (Exception ex)
            {
                LogManager.LogError("Waitversion Error", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult InitVersion()
        {
            try
            {
                if (Session["User"] == null)
                    return Json(new { success = false, nosession = true });
                
                VersionMachineService serv = new VersionMachineService();
                EsMachine m = (Session["User"] as EsUser).Machine;

                // Lancement de la version
                string version = "V" + new ServerService().GetVersionServer().Id;
                serv.StartLogVersion(m, version);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                LogManager.LogError("Initversion Error", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult NewVersion(string code)
        {
            if (code != "dzeoJD15!")
                throw new Exception("Code non valide");
            else
            {
                string vs = "";
                EsVersion v = new ServerService().GetVersionServer();
                if (v == null)
                    vs = "V0";
                else
                {
                    vs = "V" + v.Id.ToString();
                }
                ViewData["vs"] = vs;
                return View();
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult NewVersion(HttpPostedFileBase filev, string descriptif, string v)
        {
            if (filev != null)
            {
                var fileName = Path.GetFileName(filev.FileName);
                var path = ConfigurationManager.AppSettings["Essensys.VersionDirectory"] + "\\" + filev.FileName;
                filev.SaveAs(path);
                
                EsVersion vs = new EsVersion();
                vs.Descriptif = descriptif;
                vs.Filename = filev.FileName;
                vs.Id = Convert.ToInt32(v.Replace("V", "")) + 1;
                using (StreamReader fs = System.IO.File.OpenText(ConfigurationManager.AppSettings["Essensys.VersionDirectory"] + "\\" + vs.Filename))
                {
                    string c = fs.ReadToEnd();
                    vs.Size = c.Length;
                }
                new EsVersionRepository(EsSessionFactory.GetSession()).Add(vs);
                EsSessionFactory.GetSession().Flush();
            }
            return Redirect("/");
        }

        [Authorize]
        [HttpPost]
        public JsonResult DoActions(bool newar, string ar,
            bool newal, string al, string alresp, string codealarme,
            bool newcf, string cfzj,
            bool newcfzn, string cfzn,
            bool newcfsdb1, string cfsdb1, bool newcfsdb2, string cfsdb2,
            bool newcm, string cfcm,
            string cfvol, string cfsto)
        {
            try
            {
                if (Session["User"] == null)
                {
                    return Json(new { success = false, redirect = true });
                }
                StateService serv = new StateService();
                EsMachine m = (Session["User"] as EsUser).Machine;
                List<EsCouple> volets = new List<EsCouple>();
                foreach (string k in Request.Form.AllKeys)
                {
                    if (k.StartsWith("vl_"))
                    {
                        volets.Add(new EsCouple() { Index = Convert.ToInt32(k.Replace("vl_", "").Split(new char[]{'_'})[0]), Value = Request.Form[k] });
                    }
                }
                FonctionService.DoActions(m, newar, ar, newal, al, alresp, codealarme, newcf, cfzj, newcfzn, cfzn, newcfsdb1, cfsdb1, newcfsdb2, cfsdb2, newcm, cfcm, cfvol, cfsto, volets);
                
                if (ConfigurationManager.AppSettings["waitessensysbox"] == "true")
                {
                    int i = 1;
                    while (!serv.AllActionsOK(m) && i < 20)
                    {
                        System.Threading.Thread.Sleep(1000);
                        i++;
                    }
                    if (i >= 20)
                    {
                        LogManager.LogTrace("Undo all actions for machine " + m.Id.ToString(), null);
                        new ActionService().UndoAllActions(m);
                        return Json(new { success = false, message = "Les modifications ne sont pas prises en compte" });
                    }
                    else
                        return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError("Erreur dans l'application des actions", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}