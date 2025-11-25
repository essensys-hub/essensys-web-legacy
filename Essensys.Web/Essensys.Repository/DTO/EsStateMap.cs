using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsStateMap : ClassMap<EsState>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsStateMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_STATE");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.StateDate, "STATEDATE");
            Map(x => x.Version, "VERSION");
            Map(x => x.Completed, "COMPLETED");

            References(x => x.Machine)
                .Column("MACHINE_ID")
                .Cascade.SaveUpdate();
        }
    }
}
