using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Repository.DTO;
using Essensys.Common;
using System.Configuration;
using NHibernate;
using NHibernate.Criterion;

namespace Essensys.Service.Phone
{
    /// <summary>
    /// Service lié au SMS
    /// </summary>
    public class PhoneService
    {
        private EsPhoneRepository _rep;

        public PhoneService()
        {
            _rep = new EsPhoneRepository(EsSessionFactory.GetSession());
        }

        /// <summary>
        /// Liste des numéros de téléphone pour un utilisateur
        /// </summary>
        /// <param name="u">Utilisateur</param>
        /// <returns>Liste des numéros de téléphone</returns>
        public List<EsPhone> ListUserPhone(EsUser u)
        {
            LogManager.LogTrace("ListUserPhone on " + u.Id, null);
            List<EsPhone> lp = _rep.List(p => p.UsrId == u && p.Phone != "", 0, 0).ToList();
            LogManager.LogTrace("ListUserPhone size=" + lp.Count, null);
            if (lp.Count > 1)
                lp = lp.Take(1).ToList();
            //while (lp.Count < 3)
            //{
            //    EsPhone p = new EsPhone();
            //    p.Id = 0;
            //    p.Phone = "Entrez un numéro";
            //    p.Nom = "Entrez un nom";
            //    lp.Add(p);
            //}
            return lp;
        }

        /// <summary>
        /// Création ou modification d'un numéro de téléphone
        /// </summary>
        /// <param name="id">Identifiant du numéro</param>
        /// <param name="u">Utilisateur</param>
        /// <param name="phone">Numéro de téléphone</param>
        /// <param name="nom">Nom</param>
        /// <returns>Numéro de téléphone casté</returns>
        public EsPhone CreateOrUpdatePhone(int id, EsUser u, string phone, string nom, bool sendmail)
        {
            phone = CastPhoneNumber(phone);
            
            if (id == 0)
            {
                if (_rep.Count(p => p.UsrId == u, "Id") > 0)
                {
                    EsPhone p = _rep.FindBy(pp => pp.UsrId == u);
                    p.Phone = phone;
                    p.Nom = nom;
                    p.Datemodification = DateTime.Now;
                    p.Sendmail = sendmail;
                    _rep.Update(p);
                    return p;
                }
                else
                {
                    EsPhone p = new EsPhone();
                    p.Nom = nom;
                    p.UsrId = u;
                    p.Phone = phone;
                    p.Sendmail = sendmail;
                    p.AlerteAlarmeSent = false;
                    p.AlerteLlSent = false;
                    p.AlerteLvSent = false;
                    p.Datemodification = DateTime.Now;
                    p.Datecreation = DateTime.Now;
                    _rep.Add(p);

                    return p;
                }
            }
            else
            {
                EsPhone p = _rep.FindBy(id);
                p.Phone = phone;
                p.Nom = nom;
                p.Datemodification = DateTime.Now;
                p.Sendmail = sendmail;
                _rep.Update(p);
                return p;
            }
        }

        /// <summary>
        /// Suppression du numéro de téléphone
        /// </summary>
        /// <param name="id">Identifiant associé</param>
        public void DeletePhone(int id)
        {
            EsPhone p = _rep.FindBy(id);
            _rep.Delete(p);
        }

        public int NbSMSSent(EsPhone p)
        {
            if (p != null && p.Phone != "")
                return new EsSmssendRepository(EsSessionFactory.GetSession()).Count(s => s.UsrId == p.UsrId && s.Phone == p.Phone && s.Datesent.Month == DateTime.Now.Month && s.Datesent.Year == DateTime.Now.Year, "Id");
            else
                return 0;
        }

        public int NbMinutesLastSent(EsPhone p, string message)
        {
            ICriteria cr = EsSessionFactory.GetSession().CreateCriteria<EsSmssend>();
            cr.Add(Restrictions.Eq("Phone", p.Phone));
            cr.Add(Restrictions.Eq("Message", message));
            cr.AddOrder(Order.Desc("Datesent"));
            cr.SetFirstResult(0).SetMaxResults(1);

            EsSmssend ssd = new EsSmssendRepository(EsSessionFactory.GetSession()).FindByCriteria(cr);
            if (ssd != null)
            {
                DateTime lastsent = ssd.Datesent;
                TimeSpan ts = DateTime.Now - lastsent;
                LogManager.LogTrace("Nb minutes last send on " + p.Phone + " and " + message + " is " + ts.Minutes.ToString(), null);
                return ts.Minutes;
            }
            else
                return Convert.ToInt32(ConfigurationManager.AppSettings["smsminutesinterval"]) + 1;
        }

        public void SendSMSAlert(string type, string text, EsMachine m)
        {
            SendSMSAlert(type, text, m, false);
        }
        public void NoSMSAlert(string type, EsMachine m)
        {
            EsUser u = new EsUserRepository(EsSessionFactory.GetSession()).FindBy(us => us.Machine == m && !us.Obsolete && us.Isvalid);
            if (u != null)
            {
                foreach (EsPhone p in this.ListUserPhone(u))
                {
                    switch (type)
                    {
                        case "ALARME":
                            p.AlerteAlarmeSent = false;
                            break;
                        case "LAVELINGE":
                            p.AlerteLlSent = false;
                            break;
                        case "LAVEVAISSELLE":
                            p.AlerteLvSent = false;
                            break;
                        case "NOSYNC":
                            p.AlerteNoSync = false;
                            break;
                    }
                    new EsPhoneRepository(EsSessionFactory.GetSession()).Update(p);
                }
            }
        }
        /// <summary>
        /// Envoi d'une alerte SMS
        /// </summary>
        /// <param name="text">Texte de l'alerte</param>
        /// <param name="m">Machine concernée</param>
        public void SendSMSAlert(string type, string text, EsMachine m, bool bypasstestsend)
        {
            LogManager.LogTrace("Send SMS " + text, null);
            EsUser u = new EsUserRepository(EsSessionFactory.GetSession()).FindBy(us => us.Machine == m && !us.Obsolete && us.Isvalid);
            if (u != null)
            {
                foreach(EsPhone p in this.ListUserPhone(u))
                {
                    EsSessionFactory.GetSession().Refresh(p);
                    LogManager.LogTrace("Phone " + p.Phone, null);
                    bool alreadysent = false;
                    switch (type)
                    {
                        case "ALARME":
                            alreadysent = p.AlerteAlarmeSent;
                            break;
                        case "LAVELINGE":
                            alreadysent = p.AlerteLlSent;
                            break;
                        case "LAVEVAISSELLE":
                            alreadysent = p.AlerteLvSent;
                            break;
                        case "NOSYNC":
                            alreadysent = p.AlerteNoSync;
                            break;
                    }

                    if (NbSMSSent(p) < Convert.ToInt32(ConfigurationManager.AppSettings["maxmonthsms"]))
                    {
                        LogManager.LogTrace("Nb SMS Quota = OK can send", null);
                        //if (bypasstestsend || (NbMinutesLastSent(p, text) > Convert.ToInt32(ConfigurationManager.AppSettings["smsminutesinterval"])))
                        if (!alreadysent)
                        {
                            LogManager.LogTrace("Last Send = OK can send", null);
                            SMSSender.Send(p.Phone, "33", text);
                            if (p.Sendmail)
                                SMSSender.SendMail(u.Mail, text);

                            EsSmssend trace = new EsSmssend();
                            trace.Datesent = DateTime.Now;
                            trace.Message = text;
                            trace.UsrId = p.UsrId;
                            trace.Phone = p.Phone;
                            new EsSmssendRepository(EsSessionFactory.GetSession()).Add(trace);

                            switch (type)
                            {
                                case "ALARME":
                                    p.AlerteAlarmeSent = true;
                                    break;
                                case "LAVELINGE":
                                    p.AlerteLlSent = true;
                                    break;
                                case "LAVEVAISSELLE":
                                    p.AlerteLvSent = true;
                                    break;
                                case "NOSYNC":
                                    p.AlerteNoSync = true;
                                    break;
                            }
                            LogManager.LogTrace("Set Already Sent flag", null);
                            new EsPhoneRepository(EsSessionFactory.GetSession()).Update(p);
                        }
                        else
                        {
                            LogManager.LogTrace("Already Sent flag is present on phone Id " + p.Id.ToString(), null);
                        }
                    }
                    else
                    {
                        LogManager.LogTrace("Nb SMS Quota = KO can't send, send email", null);
                        //if (bypasstestsend || (NbMinutesLastSent(p, text) > Convert.ToInt32(ConfigurationManager.AppSettings["smsminutesinterval"])))
                        if (!alreadysent)
                        {
                            if (p.Sendmail)
                                SMSSender.SendMail(u.Mail, text);
                            else
                                LogManager.LogTrace("No email defined", null);
                            //EsSmssend trace = new EsSmssend();
                            //trace.Datesent = DateTime.Now;
                            //trace.Message = text;
                            //trace.Phone = p.Phone;
                            //new EsSmssendRepository(EsSessionFactory.GetSession()).Add(trace);

                            switch (type)
                            {
                                case "ALARME":
                                    p.AlerteAlarmeSent = true;
                                    break;
                                case "LAVELINGE":
                                    p.AlerteLlSent = true;
                                    break;
                                case "LAVEVAISSELLE":
                                    p.AlerteLvSent = true;
                                    break;
                                case "NOSYNC":
                                    p.AlerteNoSync = true;
                                    break;
                            }
                            LogManager.LogTrace("Set Already Sent flag", null);
                            new EsPhoneRepository(EsSessionFactory.GetSession()).Update(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Formate le numéro de téléphone
        /// </summary>
        /// <param name="phone">Numéro de téléphone</param>
        /// <returns></returns>
        private string CastPhoneNumber(string phone)
        {
            try
            {
                if (phone != null)
                {
                    string res = phone.Replace(" ", "").Replace("-", "");
                    res = String.Format("{0:## ## ## ## ##}", Convert.ToInt64(res));
                    if (!res.StartsWith("0"))
                        res = "0" + res;
                    return res;
                }
                else
                {
                    return "";
                }
            }
            catch (System.FormatException)
            {
                return "";
            }
        }
    }
}
