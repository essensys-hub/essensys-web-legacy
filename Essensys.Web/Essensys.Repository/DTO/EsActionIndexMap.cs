using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsActionIndexMap : ClassMap<EsActionIndex>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsActionIndexMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_ACTIONINDEX");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.Value, "VALUE");

            References(x => x.Action)
                .Column("ACTION_ID")
                .Cascade.None();

            References(x => x.Index)
                .Column("INDEX_ID")
                .Cascade.None();
        }
    }
}
