namespace Kessler.Domain.Exceptions;

public class OrbitalObjectNotFoundException : DomainException
{
    public int ObjectId { get; }

    public OrbitalObjectNotFoundException(int id)
        : base($"Objeto orbital com ID {id} não encontrado.") => ObjectId = id;

    public OrbitalObjectNotFoundException(string noradId)
        : base($"Objeto orbital com NORAD ID '{noradId}' não encontrado.") { }
}
