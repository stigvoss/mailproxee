using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain.Base
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        public AggregateRoot()
        {
            Id = Guid.NewGuid();
        }

        public AggregateRoot(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public bool Equals(IAggregateRoot other)
        {
            return Id.Equals(other?.Id);
        }
    }
}
