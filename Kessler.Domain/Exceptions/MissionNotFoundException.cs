namespace Kessler.Domain.Exceptions;

public class MissionNotFoundException : DomainException
{
    public int MissionId { get; }

    public MissionNotFoundException(int id)
        : base($"Missão com ID {id} não encontrada.") => MissionId = id;
}
