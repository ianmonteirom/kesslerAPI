namespace Kessler.Domain.Constants;

/// <summary>
/// Roles e limites de segurança da aplicação.
/// </summary>
public static class SecurityConstants
{
    public const string RoleAdmin    = "Admin";
    public const string RoleOperator = "Operator";

    public const int MinPasswordLength = 8;
    public const int MaxEmailLength    = 200;
    public const int MaxNameLength     = 200;
}
