using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsPhone : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsPhone()
        {
        }

        protected int _Id;
        /// <summary>
        /// Identifiant
        /// </summary>
        public virtual int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        protected string _Phone;
        /// <summary>
        /// Numéro de téléphone
        /// </summary>
        public virtual string Phone
        {
            get { return _Phone; }
            set { _Phone = value; }
        }

        protected string _Nom;
        /// <summary>
        /// Nom associé au téléphone
        /// </summary>
        public virtual string Nom
        {
            get { return _Nom; }
            set { _Nom = value; }
        }

        protected bool _Sendmail;
        public virtual bool Sendmail
        {
            get { return _Sendmail; }
            set { _Sendmail = value; }
        }
        protected bool _AlerteAlarmeSent;
        public virtual bool AlerteAlarmeSent
        {
            get { return _AlerteAlarmeSent; }
            set { _AlerteAlarmeSent = value; }
        }
        protected bool _AlerteLvSent;
        public virtual bool AlerteLvSent
        {
            get { return _AlerteLvSent; }
            set { _AlerteLvSent = value; }
        }
        protected bool _AlerteLlSent;
        public virtual bool AlerteLlSent
        {
            get { return _AlerteLlSent; }
            set { _AlerteLlSent = value; }
        }

        protected bool _AlerteNoSync;
        public virtual bool AlerteNoSync
        {
            get { return _AlerteNoSync; }
            set { _AlerteNoSync = value; }
        }

        protected EsUser _UsrId;
        /// <summary>
        /// Utilisateur associé
        /// </summary>
        [ScriptIgnore]
        public virtual EsUser UsrId
        {
            get { return _UsrId; }
            set { _UsrId = value; }
        }

        protected DateTime _Datecreation;
        /// <summary>
        /// Date de création du numéro de téléphone
        /// </summary>
        public virtual DateTime Datecreation
        {
            get { return _Datecreation; }
            set { _Datecreation = value; }
        }

        protected DateTime _Datemodification;
        /// <summary>
        /// Date de modification du numéro de téléphone
        /// </summary>
        public virtual DateTime Datemodification
        {
            get { return _Datemodification; }
            set { _Datemodification = value; }
        }
    }
}
