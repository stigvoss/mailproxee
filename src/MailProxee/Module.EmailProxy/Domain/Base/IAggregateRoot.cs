using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Domain.Base
{
    public interface IAggregateRoot : IEquatable<IAggregateRoot>
    {
        Guid Id { get; }
    }
}
