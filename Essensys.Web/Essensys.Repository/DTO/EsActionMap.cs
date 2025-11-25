using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsActionMap : ClassMap<EsAction>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsActionMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_ACTION");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.DateCreation, "DATECREATION");
            Map(x => x.Guid, "GUID");
            Map(x => x.IsDone, "ISDONE");
            Map(x => x.ActionInfo, "ACTION_INFOS");
            Map(x => x.ActionType, "ACTION_TYPE");

            References(x => x.Machine)
                .Column("MACHINE_ID")
                .Cascade.None();

            HasMany<EsActionIndex>(x => x.Indexes)
                .KeyColumn("ACTION_ID")
                .Inverse()
                .Cascade.SaveUpdate()
                .AsBag();
        }
    }
}
