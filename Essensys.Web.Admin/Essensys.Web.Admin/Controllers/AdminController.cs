using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Essensys.Web.Admin.Controllers
{
    [Authorize()]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult GenerateKey()
        {
            return View();
        }

        public ActionResult NewVersion()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void GenerateKey(int nb, string pref)
        {
            // Test génération journalière
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                con.Open();
                SqlCommand com = new SqlCommand("SELECT COUNT(*) FROM ES_CLEMACHINE WHERE YEAR(DATEGENERATION)=" + DateTime.Now.Year.ToString() + " AND MONTH(DATEGENERATION)=" + DateTime.Now.Month.ToString() + " AND DAY(DATEGENERATION)=" + DateTime.Now.Day.ToString(), con);
                SqlDataReader r = com.ExecuteReader();
                while (r.Read())
                {
                    if (Convert.ToInt32(r[0]) > 10000 - nb)
                    {
                        throw new Exception(r[0].ToString() + " clés ont déjà été générées aujourd'hui. Le seuil de 10000 sera dépassé. Arrêt de l'opération");
                    }
                }
                con.Close();
            }
            
            List<string> keys = new List<string>();
            string t = "<TABLE>";
            for (int i = 1; i <= nb; i++)
            {
                string k = KeyGenerator.GenerateKey();
                string nb4d = "0000" + i.ToString();
                nb4d = nb4d.Substring(nb4d.Length - 4, 4);
                string noserie = pref + DateTime.Now.ToString("yyMMdd") + nb4d;
                t += "<TR><TD>" + noserie + "</TD><TD>" + k + "</TD></TR>";
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    con.Open();

                    SqlCommand com = new SqlCommand("INSERT INTO ES_CLEMACHINE(CLE, DATEGENERATION, NOSERIE) VALUES('" + k + "', GETDATE(), '" + noserie + "')", con);
                    com.ExecuteNonQuery();
                    con.Close();
                }
            }
            t += "</TABLE>";

            Response.AddHeader("Content-Disposition", "attachment; filename=Clefs_Essensys_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1252");

            Response.Write(t);
            Response.Flush();
            Response.Close();
        }

    }
}
