namespace Kessler.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Operator";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(string name, string email, string passwordHash, string role = "Operator")
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail não pode ser vazio.", nameof(email));

        return new User
        {
            Name = name,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
