using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Essensys.Repository.DTO;

namespace Essensys.Repository.DAO
{
    public class EsClemachineRepository : BaseRepository<EsClemachine>
    {
        public EsClemachineRepository(ISession session)
            : base(session)
        {
        }
    }
}
