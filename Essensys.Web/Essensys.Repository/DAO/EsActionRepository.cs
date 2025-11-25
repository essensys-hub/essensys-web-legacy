using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Essensys.Repository.DTO;

namespace Essensys.Repository.DAO
{
    public class EsActionRepository : BaseRepository<EsAction>
    {
        public EsActionRepository(ISession session)
            : base(session)
        {
        }
    }
}
