using System.Runtime.Serialization;

namespace Portal.Services.PackageService.Exceptions;

[Serializable]
public class PackageAccessException: Exception
{
    public PackageAccessException() { }
    public PackageAccessException(string message) : base(message) { }
    public PackageAccessException(string message, Exception ex) : base(message, ex) { }
    protected PackageAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}