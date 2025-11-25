using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Essensys.Repository.DTO;
using Essensys.Service.Security;
using Essensys.Common;
using System.Configuration;
using System.Web.Security;
using Essensys.Service.Transaction;
using Essensys.Repository.DAO;
using Essensys.Repository;

namespace Essensys.Web.UI.Controllers
{
    /// <summary>
    /// Controlleur sur les comptes clients
    /// </summary>
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult LostPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LostPassword(EsUser u)
        {
            if (u.Mail2 == "" || u.Mail2 == null)
            {
                ModelState.AddModelError("Mail2", "Veuillez entrer votre adresse mail.");
                return View();
            }
            else
            {
                if (new UserService().CheckEMail(u.Mail2))
                {
                    ModelState.AddModelError("Mail2", "L'adresse email saisie n'est pas référencée dans notre système. Veuillez réessayer.");
                    return View();
                }
                else
                {
                    new UserService().GenerateNewPassword(u.Mail2);
                    return RedirectToAction("LostPasswordOK");
                }
            }
        }

        [AllowAnonymous]
        public ActionResult LostPasswordOK()
        {
            return View();
        }

        [Authorize]
        public ActionResult UpdateMyInfos()
        {
            if (Session["User"] == null)
                return Redirect("/");

            UserService serv = new UserService();
            EsUser u = Session["User"] as EsUser;
            string fcode = u.Machine.Pkey;
            fcode = fcode.Substring(0, 4) + " " + fcode.Substring(4, 4) + " " + fcode.Substring(8, 4) + " " + fcode.Substring(12, 4)
                + " " + fcode.Substring(16, 4) + " " + fcode.Substring(20, 4) + " " + fcode.Substring(24, 4) + " " + fcode.Substring(28, 4);
            ViewBag.Code = fcode;
            return View(Session["User"]);
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateMyInfos(EsUser model)
        {
            if (HttpContext.Request.Form["qsrep"] != null && HttpContext.Request.Form["qsrep"] != "")
            {
                string rep = HttpContext.Request.Form["qsrep"];
                if (new UserService().GetById(model.Id).Reponse != HashHelper.GetHash(rep.ToLower(), HashHelper.HashType.SHA1))
                {
                    ModelState.AddModelError("Pkey", "La réponse a la question actuelle n'est pas bonne");
                    return View(model);
                }
            }
            if (model.Password2 != null && model.Password2 != "")
            {
                string password = HashHelper.GetHash(model.Password2, HashHelper.HashType.SHA1);
                if (new UserService().LoginIsValid(model.Mail, password))
                {
                    if (model.NewPassword != "" && model.NewPassword != null)
                    {
                        if (model.ConfirmNewPassword != "" && model.ConfirmNewPassword != null)
                        {
                            if (model.NewPassword != model.ConfirmNewPassword)
                            {
                                ModelState.AddModelError("ConfirmNewPassword", "Veuillez Entrer le nouveau mot de passe de manière identique");
                                return View(model);
                            }
                            else
                            {
                                EsUser user = new UserService().UpdateUser(model);
                                Session["User"] = user;
                                return Redirect("/");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("ConfirmNewPassword", "Entrez une deuxième fois le nouveau mot de passe");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("NewPassword", "Entrez un nouveau mot de passe");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("Password2", "Le mot de passe actuel n'est pas valide");
                    return View(model);
                }
            }
            else
            {
                if (model.NewPassword != "" && model.NewPassword != null)
                {
                    ModelState.AddModelError("Password2", "Entrez le mot de passe actuel");
                    return View(model);
                }
                else
                {
                    EsUser user = new UserService().UpdateUser(model);
                    Session["User"] = user;
                    return Redirect("/");
                }
            }
        }


        [HttpPost]
        [Authorize]
        public JsonResult CloseAccount()
        {
            try
            {
                new UserService().CloseAccount(Session["User"] as EsUser);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public ActionResult CloseMessage()
        {
            EsUser u = Session["User"] as EsUser;
            Session["User"] = null;
            if (u != null)
            {
                new ActionService().AcquitAllActions(u.Machine);
                ConnectionInfo.Disconnect(u.Machine.Id);
            }
            FormsAuthentication.SignOut();

            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult TestResponse(string res)
        {
            try
            {
                if (Session["User"] == null)
                    return Json(new { success = false, redirect = true });
                return Json(new { success = true, responseisok = new UserService().TestQuestion(Session["User"] as EsUser, res) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(EsUser model, string returnUrl)
        {
            string password = HashHelper.GetHash(model.Password, HashHelper.HashType.SHA1);
            if (new UserService().LoginIsValid(model.Mail, password))
            {
                if (model.RememberMe)
                    FormsAuthentication.SetAuthCookie(model.Mail, true);
                else
                    FormsAuthentication.SetAuthCookie(model.Mail, false);
                return RedirectToLocal(returnUrl);
            }

            // Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
            ModelState.AddModelError("", "Le nom d'utilisateur ou mot de passe fourni est incorrect.");
            return View(model);
        }

        public ActionResult Logout()
        {
            EsUser u = Session["User"] as EsUser;
            Session["User"] = null;
            if (u != null)
            {
                new ActionService().AcquitAllActions(u.Machine);
                ConnectionInfo.Disconnect(u.Machine.Id);
            }
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(EsUser model)
        {
            if (new EsClemachineRepository(EsSessionFactory.GetSession()).Count(l => l.Cle == model.NoSerie && l.Machine == null, "Id") == 0)
            {
                ModelState.AddModelError("NoSerie", "La clé produit n'est pas référencée");
                return View();
            }
                    
            // Création du compte utilisateur
            string guid = Guid.NewGuid().ToString();
            model.Guid = guid;
            new UserService().RegisterUser(model);

            // Envoi de l'email de confirmation
            string urlbase = ConfigurationManager.AppSettings["webserver"] + "/Account/ValidateAccount?_ge54=" + guid + "&_m=" + model.Mail;
            EMailSender.SendSimpleEMail(ConfigurationManager.AppSettings["mailuser"], model.Mail, "Création de votre compte Essensys", "Votre compte utilisateur a été créé avec succès. Il doit maintenant être activé. Pour obtenir le code d'activation à renseigner sur l'écran de contrôle Essensys, cliquez sur le lien suivant :", "Votre compte utilisateur a été créé avec succès. Il doit maintenant être activé. Pour obtenir le code d'activation à renseigner sur l'écran de contrôle Essensys, cliquez sur le lien suivant : <br/><br/><a href='" + urlbase + "'>Cliquez ici pour valider la création de votre compte</a><br/><br/>Nous vous remercions d'utiliser les produits Valentin&eacute;a.");
            
            return RedirectToAction("RegisterOK");
        }
        public ActionResult RegisterOK()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ValidateAccount(string _ge54, string _m)
        {
            UserService serv = new UserService();
            EsUser u = serv.GetUserByGuid(_ge54, _m);
            Session["User"] = u;
            if (u != null)
            {
                string fcode = u.Machine.Pkey;
                LogManager.LogTrace("fcode=" + fcode, null);
                if (fcode == null || fcode == "")
                    fcode = serv.ValidateUserAndGenerateCode(u, true); 
                else
                    serv.ValidateUserAndGenerateCode(u, false);
                ViewBag.Exists = true;
                fcode = fcode.Substring(0, 4) + " " + fcode.Substring(4, 4) + " " + fcode.Substring(8, 4) + " " + fcode.Substring(12, 4)
                    + " " + fcode.Substring(16, 4) + " " + fcode.Substring(20, 4) + " " + fcode.Substring(24, 4) + " " + fcode.Substring(28, 4);
                ViewBag.Code = fcode;
            }
            else
            {
                ViewBag.Exists = false;
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult CheckEMail(string Mail)
        {
            return Json(new UserService().CheckEMail(Mail), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CheckNoSerie(string Noserie)
        {
            return Json(new UserService().CheckNoSerie(Noserie), JsonRequestBehavior.AllowGet);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
