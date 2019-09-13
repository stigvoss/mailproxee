﻿using Module.EmailProxy.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain
{
    public class Alias : AggregateRoot
    {
        public Alias()
        { }

        public Alias(string recipient, ActivationCriteria activationCriteria)
            : base()
        {
            Recipient = recipient;
            ActivationCriteria = activationCriteria;
        }

        public string Recipient { get; set; }

        public bool IsActivated => ActivationCriteria?.IsActivated ?? false;

        public ActivationCriteria ActivationCriteria { get; set; }

        public void Activate(string code)
        {
            if (ActivationCriteria is null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            ActivationCriteria.IsActivated = ActivationCriteria.ActivationCode == code;
        }
    }
}
