using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsDataIndexMap : ClassMap<EsDataIndex>
    {
         /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsDataIndexMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_DATAINDEX");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.IndexKey, "INDEX_KEY");
            Map(x => x.IsActive, "ISACTIVE");
            Map(x => x.DateCreation, "DATECREATION");
            Map(x => x.DateModification, "DATEMODIFICATION");
        }
    }
}
