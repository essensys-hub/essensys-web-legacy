using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsSmssend : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsSmssend()
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

        protected DateTime _Datesent;
        /// <summary>
        /// Date d'envoi
        /// </summary>
        public virtual DateTime Datesent
        {
            get { return _Datesent; }
            set { _Datesent = value; }
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

        protected string _Message;
        /// <summary>
        /// Message
        /// </summary>
        public virtual string Message
        {
            get { return _Message; }
            set { _Message = value; }
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
    }
}
