using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain
{
    public class MailDomain
        : AggregateRoot
    {
        public string Name { get; set; }

        public Alias AliasFrom(string recipient)
        {
            if (recipient is null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            var generator = new ActivationCodeGenerator();
            var activationCode = generator.GenerateCode();
            var activationCriteria = new ActivationCriteria(activationCode);

            return new Alias(recipient, activationCriteria, this);
        }
    }
}
