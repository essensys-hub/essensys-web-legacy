using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Essensys.Service.Transaction;
using Essensys.Repository.DTO;
using Essensys.Service.Response;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Common;
using System.Text;

namespace Essensys.Web.UI.Controllers
{
    public class SimulateController : Controller
    {
        //
        // GET: /Simulate/

        public JsonResult Status(string code, int id)
        {
            if (code == "Meije69")
            {
                List<EsKeyValue> ek = new List<EsKeyValue>();
                
                EsKeyValue v = new EsKeyValue();
                v.k = 360;
                v.v = "0";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 346;
                v.v = "18";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 347;
                v.v = "5";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 348;
                v.v = "20";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 349;
                v.v = "19";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 350;
                v.v = "1";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 8;
                v.v = "0";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 363;
                v.v = "0";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 364;
                v.v = "0";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 404;
                v.v = "0";
                ek.Add(v);
                v = new EsKeyValue();
                v.k = 405;
                v.v = "0";
                ek.Add(v);
                EsMachine m = new EsMachineRepository(EsSessionFactory.GetSession()).FindBy(id);
                new StateService().RegisterState(m, ek, "V1");
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCode(string code, string key)
        {
            if (code == "Meije69")
            {
                string enc = HashHelper.GetHash(key, HashHelper.HashType.MD5);
                string has = enc;
                enc = enc.Substring(0, 16) + ":" + enc.Substring(16, 16);
                return Json(new { success = true, hash = has, code = Convert.ToBase64String(Encoding.Unicode.GetBytes(enc)) }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}
