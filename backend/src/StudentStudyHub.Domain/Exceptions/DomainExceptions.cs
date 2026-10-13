namespace StudentStudyHub.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
    }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

/// <summary>
/// Thrown when the current user is a known party to the resource (e.g. a non-owner member of a
/// study group they belong to) but lacks permission for the specific action attempted. Distinct
/// from NotFoundException, which is used when the resource should look invisible to the caller.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
