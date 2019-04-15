using Module.EmailProxy.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain
{
    public class Alias : AggregateRoot
    {
        public Alias(Guid aliasId, string recipient)
            : base(aliasId)
        {
            Recipient = recipient;
        }

        public Alias(string recipient)
        {
            if (recipient is null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            Recipient = recipient;
        }

        public string Recipient { get; }
    }
}
