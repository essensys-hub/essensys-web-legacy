using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsVersion : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsVersion()
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

        protected string _Descriptif;
        /// <summary>
        /// Descriptif de la version
        /// </summary>
        public virtual string Descriptif
        {
            get { return _Descriptif; }
            set { _Descriptif = value; }
        }


        protected int _Size;
        /// <summary>
        /// Tailler de la version
        /// </summary>
        public virtual int Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        protected string _Filename;
        /// <summary>
        /// Nom de fichier associé
        /// </summary>
        public virtual string Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }
    }
}
