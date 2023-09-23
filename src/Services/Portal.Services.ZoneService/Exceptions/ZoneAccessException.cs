using System.Runtime.Serialization;

namespace Portal.Services.ZoneService.Exceptions;

[Serializable]
public class ZoneAccessException: Exception
{
    public ZoneAccessException() { }
    public ZoneAccessException(string message) : base(message) { }
    public ZoneAccessException(string message, Exception ex) : base(message, ex) { }
    protected ZoneAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}