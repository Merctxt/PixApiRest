namespace PixApiRest.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message)
    {
    }

    public ResourceNotFoundException(string resourceName, long id) 
        : base($"{resourceName} não encontrado com ID: {id}")
    {
    }

    public ResourceNotFoundException(string resourceName, string field, string value) 
        : base($"{resourceName} não encontrado com {field}: {value}")
    {
    }
}
