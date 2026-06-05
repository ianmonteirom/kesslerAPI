namespace Kessler.Domain.Exceptions;

public class DuplicateNoradIdException : DomainException
{
    public string NoradId { get; }

    public DuplicateNoradIdException(string noradId)
        : base($"Já existe um objeto orbital cadastrado com o NORAD ID '{noradId}'.") => NoradId = noradId;
}
