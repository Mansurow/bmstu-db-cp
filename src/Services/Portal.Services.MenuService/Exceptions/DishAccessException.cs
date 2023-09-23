using System.Runtime.Serialization;

namespace Portal.Services.MenuService.Exceptions;

[Serializable]
public class DishAccessException: Exception
{
    public DishAccessException() { }
    public DishAccessException(string message) : base(message) { }
    public DishAccessException(string message, Exception ex) : base(message, ex) { }
    protected DishAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}