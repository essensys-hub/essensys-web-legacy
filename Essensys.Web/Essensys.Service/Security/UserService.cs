using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Common;
using System.Web;
using System.Configuration;
using Essensys.Service.Transaction;
using System.Data.SqlClient;

namespace Essensys.Service.Security
{
    public static class ConnectionInfo
    {
        private static List<int> _ConnectedMachines;

        public static List<int> ConnectedMachines
        {
            get
            {
                if (_ConnectedMachines == null)
                    _ConnectedMachines = new List<int>();
                return _ConnectedMachines;
            }
        }
        public static void Connect(int id)
        {
            LogManager.LogTrace("connect count=" + ConnectedMachines.Count.ToString(), null);
            if (ConnectedMachines.Count(c => c == id) == 0)
                ConnectedMachines.Add(id);
            LogManager.LogTrace("connect count=" + ConnectedMachines.Count.ToString(), null);
        }
        public static void Disconnect(int id)
        {
            ConnectedMachines.Remove(id);
        }
    }

    /// <summary>
    /// Service utilisateur
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// Repository associé
        /// </summary>
        protected EsUserRepository _rep;
        /// <summary>
        /// Repository des machines
        /// </summary>
        protected EsMachineRepository _repm;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public UserService()
        {
            _rep = new EsUserRepository(EsSessionFactory.GetSession());
            _repm = new EsMachineRepository(EsSessionFactory.GetSession());
        }

        public EsUser GetById(int id)
        {
            return _rep.FindBy(id);
        }

        /// <summary>
        /// Retourne si un utilisateur correspondant à la machine est connecté
        /// </summary>
        /// <param name="m"></param>
        public bool IsConnected(EsMachine m)
        {
            return (ConnectionInfo.ConnectedMachines.Count(cm => cm == m.Id) >= 1);
        }

        public bool TestQuestion(EsUser u, string response)
        {
            return (u.Reponse == HashHelper.GetHash(response.ToLower(), HashHelper.HashType.SHA1));
        }

        /// <summary>
        /// Retourne un utilisateur à partir de son GUID
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <returns>Utilisateur</returns>
        public EsUser GetUserByGuid(string guid, string email)
        {
            return _rep.FindBy(u => u.Guid == guid && u.Mail == email && !u.Obsolete);
        }

        /// <summary>
        /// Fermeture d'un compte
        /// </summary>
        /// <param name="u">Utilisateur</param>
        public void CloseAccount(EsUser u)
        {
            u.Obsolete = true;
            u.Datecloture = DateTime.Now;
            u.Pkey = "";
            u.Machine.Pkey = "";
            u.Machine.HashedPkey = "";
            _rep.Update(u);

            // Libération de la clé
            int machineid = u.Machine.Id;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Essensys"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE ES_CLEMACHINE SET MACHINE_ID=NULL, DATEACTIVATION=NULL WHERE MACHINE_ID=" + machineid.ToString(), con);
                cmd.ExecuteNonQuery();
                
                SqlCommand cmd2 = new SqlCommand("UPDATE ES_MACHINE SET ISACTIVE=0, PKEY=NULL, HASHEDPKEY=NULL WHERE ID=" + machineid.ToString(), con);
                cmd2.ExecuteNonQuery();
                
                con.Close();
            }
        }

        /// <summary>
        /// Mise à jour d'un utilisateur
        /// </summary>
        /// <param name="u">Utilisateur</param>
        public EsUser UpdateUser(EsUser u)
        {
            EsUser utu = _rep.FindBy(u.Id);
            
            utu.Nom = u.Nom;
            utu.Prenom = u.Prenom;
            utu.Adr1 = u.Adr1;
            utu.Adr2 = u.Adr2;
            utu.SendInfos = u.SendInfos;
            utu.Cp = u.Cp;
            utu.Ville = u.Ville;
            utu.Phone = u.Phone;
            if (u.Question != null && u.Question != "")
            {
                utu.Question = u.Question;
                utu.Reponse = HashHelper.GetHash(u.Reponse.ToLower(), HashHelper.HashType.SHA1);
            }
            if (u.NewPassword != "" && u.NewPassword != null)
            {
                string password = HashHelper.GetHash(u.NewPassword, HashHelper.HashType.SHA1);
                utu.Password = password;
            }

            _rep.Update(utu);
            return utu;
        }

        /// <summary>
        /// Crée un nouvel utilisateur
        /// </summary>
        /// <param name="u">Utilisateur</param>
        public int RegisterUser(EsUser u)
        {
            u.Obsolete = false;
            u.Isvalid = false;
            u.Password = HashHelper.GetHash(u.Password, HashHelper.HashType.SHA1);
            u.Reponse = HashHelper.GetHash(u.Reponse.ToLower(), HashHelper.HashType.SHA1);
            u.Datecreation = DateTime.Now;
            u.Datecloture = new DateTime(1900, 1, 1);
            u.Lastaccess = DateTime.Now;

            if (_repm.Count(m => m.NoSerie == u.NoSerie, "Id") == 0)
            {
                EsMachine m = new EsMachine();
                m.DateCreation = DateTime.Now;
                m.DateModification = DateTime.Now;
                m.IsActive = false;
                m.NoSerie = u.NoSerie;
                EsVersion v = new ServerService().GetVersionServer();
                if (v != null)
                    m.Version = "V" + v.Id;
                _repm.Add(m);
                u.Machine = m;
            }
            else
            {
                u.Machine = _repm.FindBy(m => m.NoSerie == u.NoSerie);
            }

            return _rep.Add(u);
        }

        /// <summary>
        /// Vérifie si l'email demandé n'est pas lié à un compte actif
        /// </summary>
        /// <param name="email">EMail</param>
        /// <returns>Vrai si pas de liaison, faux sinon</returns>
        public bool CheckEMail(string email)
        {
            return (_rep.Count(u => u.Mail == email && !u.Obsolete, "Id") == 0);
        }

        public void GenerateNewPassword(string email)
        {
            EsUser us = _rep.FindBy(u => u.Mail == email && !u.Obsolete);
            string newpassword = CodeHelper.GenerateCode(8);
            us.Password = HashHelper.GetHash(newpassword, HashHelper.HashType.SHA1);
            _rep.Update(us);

            // Envoi de l'email de confirmation
            EMailSender.SendSimpleEMail(ConfigurationManager.AppSettings["mailuser"], email, "Votre nouveau mot de passe Essensys", "Votre nouveau mot de passe est le suivant : " + newpassword, "Votre nouveau mot de passe est le suivant : " + newpassword + "<br/><br/>Nous vous remercions d'utiliser les produits Valentin&eacute;a.");
            
        }

        public bool LoginIsValid(string mail, string passwordhashed)
        {
            EsUser us = _rep.FindBy(u => u.Mail == mail && u.Password == passwordhashed && u.Isvalid && !u.Obsolete);
            if (us != null)
            {
                us.Lastaccess = DateTime.Now;
                _rep.Update(us);
                ConnectionInfo.Connect(us.Machine.Id);
                HttpContext.Current.Session["User"] = us;
                return true;
            }
            else
                return false;
        }

        public string ValidateUserAndGenerateCode(EsUser u, bool generate)
        {
            u.Lastaccess = DateTime.Now;
            u.Isvalid = true;
            if (generate)
            {
                u.Pkey = CodeHelper.GenerateCode(32);
                u.Machine.Pkey = u.Pkey;
                u.Machine.HashedPkey = HashHelper.GetHash(u.Pkey, HashHelper.HashType.MD5);
            }
            u.Machine.DateModification = DateTime.Now;
            u.Machine.IsActive = true;

            string noserie = u.Machine.NoSerie;
            EsClemachineRepository _cmrep = new EsClemachineRepository(EsSessionFactory.GetSession());
            if (_cmrep.Count(cm => cm.Cle == noserie, "Id") == 1)
            {
                EsClemachine cm = _cmrep.FindBy(cmm => cmm.Cle == noserie);
                cm.Dateactivation = DateTime.Now;
                cm.Machine = u.Machine;
                _cmrep.Update(cm);
            }
            _rep.Update(u);

            return u.Machine.Pkey;
        }

        public EsMachine ValidateAPIAccess(string cryptedcode)
        {
            return _repm.FindBy(m => m.HashedPkey == cryptedcode && m.IsActive);
        }

        /// <summary>
        /// Vérifie si le numéro de série demandé n'est pas lié à un compte actif
        /// </summary>
        /// <param name="noserie">Numéro de série</param>
        /// <returns>Vrai si pas de liaison, faux sinon</returns>
        public bool CheckNoSerie(string noserie)
        {
            bool isvalid = false;
            int incle = new EsClemachineRepository(EsSessionFactory.GetSession()).Count(cm => cm.Cle == noserie && cm.Machine == null, "Id");
            if (incle == 0)
                isvalid = false;
            else
                isvalid = true;
            return isvalid;
        }
    }

    public static class CodeHelper
    {
        public static string GenerateCode(int nb)
        {
            return GetRandomNumbers(nb);
        }

        /// Generates a string and checks for existance
        /// <returns>Non-existant string as ID</returns>
        public static string GetRandomNumbers(int numChars)
        {
            string result = string.Empty;
            bool isUnique = false;
            while (!isUnique)
            {
                //Build the string
                result = MakeID(numChars);
                //Check if unsued
                isUnique = IsUnique(result);
            }
            return result;
        }
        /// Builds the string
        public static string MakeID(int numChars)
        {
            string random = string.Empty;
            string[] chars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            Random rnd = new Random();
            for (int i = 0; i < numChars; i++)
            {
                random += chars[rnd.Next(0, 9)];
            }
            return random;
        }
        
        private static bool IsUnique(string value)
        {
            int nb = new EsMachineRepository(EsSessionFactory.GetSession()).Count(m => m.Pkey == value, "Id");
            
            return (nb == 0);
        }
    }
}
