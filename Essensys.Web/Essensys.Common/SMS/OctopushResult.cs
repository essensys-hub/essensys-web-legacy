using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Essensys.Common.SMS
{
    [DataContract(Name = "octopush", Namespace="")]
    public class OctopushResult
    {
        [DataMember(Name = "error_code")]
        public string ErrorCode
        {
            get;
            set;
        }
        [DataMember(Name = "cost")]
        public string Cost
        {
            get;
            set;
        }
        [DataMember(Name = "balance")]
        public string Balance
        {
            get;
            set;
        }
        [DataMember(Name = "ticket")]
        public string Ticket
        {
            get;
            set;
        }
        [DataMember(Name = "sending_date")]
        public string SendingDate
        {
            get;
            set;
        }
        [DataMember(Name = "number_of_sendings")]
        public string NumberSendings
        {
            get;
            set;
        }
        [DataMember(Name = "currency_code")]
        public string CurrencyCode
        {
            get;
            set;
        }
        [DataMember(Name = "successs")]
        public List<OctopushSuccess> Success
        {
            get;
            set;
        }
    }
    [DataContract(Name = "success", Namespace = "")]
    public class OctopushSuccess
    {
        [DataMember(Name="recipient")]
        public string Recipient
        {
            get;
            set;
        }
        [DataMember(Name = "country_code")]
        public string CountryCode
        {
            get;
            set;
        }
        [DataMember(Name = "cost")]
        public string Cost
        {
            get;
            set;
        }
    }
}
