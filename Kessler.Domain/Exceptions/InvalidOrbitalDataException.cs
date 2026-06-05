namespace Kessler.Domain.Exceptions;

public class InvalidOrbitalDataException : DomainException
{
    public string Field { get; }

    public InvalidOrbitalDataException(string field, string reason)
        : base($"Dado orbital inválido no campo '{field}': {reason}") => Field = field;
}
