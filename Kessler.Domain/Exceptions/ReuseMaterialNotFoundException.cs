namespace Kessler.Domain.Exceptions;

public class ReuseMaterialNotFoundException : DomainException
{
    public int EstimateId { get; }

    public ReuseMaterialNotFoundException(int id)
        : base($"Estimativa de reaproveitamento com ID {id} não encontrada.") => EstimateId = id;
}
