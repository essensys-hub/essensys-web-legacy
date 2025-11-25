using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Repository.DTO
{
    public class EsClemachine : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsClemachine()
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

        protected string _Cle;
        /// <summary>
        /// Clé d'activation
        /// </summary>
        public virtual string Cle
        {
            get { return _Cle; }
            set { _Cle = value; }
        }

        protected DateTime _Dategeneration;
        /// <summary>
        /// Date de génération
        /// </summary>
        public virtual DateTime Dategeneration
        {
            get { return _Dategeneration; }
            set { _Dategeneration = value; }
        }

        protected DateTime _Dateactivation;
        /// <summary>
        /// Date d'activation
        /// </summary>
        public virtual DateTime Dateactivation
        {
            get { return _Dateactivation; }
            set { _Dateactivation = value; }
        }

        protected EsMachine _Machine;
        /// <summary>
        /// Machine
        /// </summary>
        public virtual EsMachine Machine
        {
            get { return _Machine; }
            set { _Machine = value; }
        }
    }
}
