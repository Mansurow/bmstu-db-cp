using System.Runtime.Serialization;

namespace Portal.Services.InventoryServices.Exceptions;

[Serializable]
public class InventoryAccessException: Exception
{
    public InventoryAccessException() { }
    public InventoryAccessException(string message) : base(message) { }
    public InventoryAccessException(string message, Exception ex) : base(message, ex) { }
    protected InventoryAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}