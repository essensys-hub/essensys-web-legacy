using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Essensys.Common;
using Essensys.Service.Phone;
using Essensys.Repository.DTO;

namespace Essensys.Web.UI.Controllers
{
    public class PhoneController : Controller
    {
        /// <summary>
        /// Liste des numéros de téléphone
        /// </summary>
        /// <returns>Résultat JSON</returns>
        [Authorize]
        public JsonResult ListPhone()
        {
            try
            {
                EsUser u = Session["User"] as EsUser;
                return Json(new { success = true, phones = new PhoneService().ListUserPhone(u) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.LogError("Impossible de retirer la liste des numéros de téléphone", ex);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Ajout ou modification d'un numéro de téléphone
        /// </summary>
        /// <returns>Résultat JSON</returns>
        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddOrUpdatePhone(int id, string phone, string nom, bool sendmail)
        {
            try
            {
                EsUser u = Session["User"] as EsUser;
                return Json(new { success = true, phone = new PhoneService().CreateOrUpdatePhone(id, u, phone, nom, sendmail) });
            }
            catch (Exception ex)
            {
                LogManager.LogError("Impossible de mettre à jour le numéro de téléphone", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Suppression d'un numéro de téléphone
        /// </summary>
        /// <returns>Résultat JSON</returns>
        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeletePhone(int id)
        {
            try
            {
                new PhoneService().DeletePhone(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                LogManager.LogError("Impossible de supprimer le numéro de téléphone", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}