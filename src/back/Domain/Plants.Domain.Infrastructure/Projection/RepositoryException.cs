using System;

namespace Plants.Domain.Infrastructure.Projection;

[Serializable]
public class RepositoryException : Exception
{
    public RepositoryException() { }
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception inner) : base(message, inner) { }
    protected RepositoryException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
