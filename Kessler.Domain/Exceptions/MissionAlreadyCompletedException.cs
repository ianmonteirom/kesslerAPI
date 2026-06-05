namespace Kessler.Domain.Exceptions;

public class MissionAlreadyCompletedException : DomainException
{
    public int MissionId { get; }

    public MissionAlreadyCompletedException(int id)
        : base($"A missão {id} já foi concluída e não pode ser alterada.") => MissionId = id;
}
