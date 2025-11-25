using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsVersionMap : ClassMap<EsVersion>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsVersionMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_VERSION");
                Id(x => x.Id, "ID").GeneratedBy.Assigned();
            Map(x => x.Descriptif, "DESCRIPTIF");
            Map(x => x.Filename, "FILENAME");
            Map(x => x.Size, "SIZE");
        }
    }
}
