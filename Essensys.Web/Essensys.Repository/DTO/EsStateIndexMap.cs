using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsStateIndexMap : ClassMap<EsStateIndex>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsStateIndexMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_STATEINDEX");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.Value, "VALUE");

            References(x => x.State)
                .Column("STATE_ID")
                .Cascade.SaveUpdate();
            References(x => x.Index)
                .Column("INDEX_ID")
                .Cascade.SaveUpdate();
        }
    }
}
