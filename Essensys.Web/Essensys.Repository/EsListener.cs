using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Event.Default;
using NHibernate.Event;
using NHibernate;

namespace Essensys.Repository
{
    /// <summary>
    /// Contournement pour le problème du postflush
    /// http://www.mail-archive.com/nhusers@googlegroups.com/msg16026.html
    /// </summary>
    [Serializable]
    public class FlushFixEventListener : DefaultFlushEventListener
    {

        public override void OnFlush(FlushEvent @event)
        {
            try
            {
                base.OnFlush(@event);
            }
            catch (AssertionFailure)
            {
                // throw away
            }
        }
    }

    public class AutoFlushFixEventListener : DefaultAutoFlushEventListener
    {
        public override void OnAutoFlush(AutoFlushEvent @event)
        {
            try
            {
                base.OnAutoFlush(@event);
            }
            catch (AssertionFailure)
            {
            }
        }
    }
}
