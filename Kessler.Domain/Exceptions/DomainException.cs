namespace Kessler.Domain.Exceptions;

/// <summary>
/// Exceção base para todas as violações de regras de domínio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}
