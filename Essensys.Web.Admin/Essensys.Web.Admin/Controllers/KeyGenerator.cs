using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Essensys.Web.Admin.Controllers
{
    public static class KeyGenerator
    {
        public static string GenerateKey()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string res = "";
                string sql = "WITH TEST (CODE) AS (SELECT LEFT(REPLACE(CAST(NEWID() AS VARCHAR(100)), '-', ''), 25)) SELECT CODE FROM TEST WHERE CODE NOT IN (SELECT REPLACE(CLE, '-', '') FROM ES_CLEMACHINE)";
                while (1 == 1)
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sql, con);
                    SqlDataReader r = cmd.ExecuteReader();
                    if (!r.HasRows)
                        continue;
                    while (r.Read())
                    {
                        res = r[0].ToString();
                        break;
                    }
                    if (res != "")
                        break;
                    con.Close();
                }
                return res.Substring(0, 5) + "-" + res.Substring(5, 5) + "-" + res.Substring(10, 5) + "-" + res.Substring(15, 5) + "-" + res.Substring(20, 5);
            }
        }
        //public static string GenerateCode(int nb, SqlConnection con, Random rnd)
        //{
        //    return GetRandomNumbers(nb, con, rnd);
        //}

        ///// Generates a string and checks for existance
        ///// <returns>Non-existant string as ID</returns>
        //public static string GetRandomNumbers(int numChars, SqlConnection con, Random rnd)
        //{
        //    string result = string.Empty;
        //    bool isUnique = false;
        //    while (!isUnique)
        //    {
        //        //Build the string
        //        result = MakeID(numChars, rnd);
        //        //Check if unsued
        //        isUnique = IsUnique(result, con);
        //    }
        //    return result;
        //}
        ///// Builds the string
        //public static string MakeID(int numChars, Random rnd)
        //{
        //    string random = string.Empty;
        //    string[] chars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        //    for (int i = 0; i < numChars; i++)
        //    {
        //        random += chars[rnd.Next(0, 35)];
        //    }
        //    return random;
        //}

        //private static bool IsUnique(string value, SqlConnection con)
        //{
        //    SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM ES_CLEMACHINE WHERE CLE = '" + value + "'", con);
        //    SqlDataReader r = cmd.ExecuteReader();
        //    while(r.Read())
        //    {
        //        int nb = r.GetInt32(0);
        //        r.Close();
        //        return (nb == 0);
        //    }
        //    r.Close();
        //    return false;
        //}
    }
}