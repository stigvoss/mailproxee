using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain
{
    public class ActivationCriteria
    {
        public ActivationCriteria(string activationCode)
        {
            ActivationCode = activationCode;
        }

        public string ActivationCode { get; set; }

        public bool IsActivated { get; set; }

        public bool IsSent { get; set; }

        public DateTime Creation { get; set; }
    }
}
