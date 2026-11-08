using System.Security.Cryptography;
using System.Text;
using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;

namespace HomeFinance.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public AuthResult Register(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return new AuthResult { Success = false, Error = "El correo es obligatorio y debe ser válido." };
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return new AuthResult { Success = false, Error = "La contraseña debe tener al menos 8 caracteres." };
        }

        if (_userRepository.GetByEmail(email.Trim()) is not null)
        {
            return new AuthResult { Success = false, Error = "El correo ya existe en el sistema." };
        }

        var user = new User
        {
            Email = email.Trim(),
            PasswordHash = HashPassword(password)
        };

        _userRepository.Add(user);

        return new AuthResult { Success = true, User = user };
    }

    public AuthResult Login(string email, string password)
    {
        var user = _userRepository.GetByEmail(email.Trim());
        if (user is null)
        {
            return new AuthResult { Success = false, Error = "Credenciales inválidas." };
        }

        if (!SecureEquals(user.PasswordHash, HashPassword(password)))
        {
            return new AuthResult { Success = false, Error = "Credenciales inválidas." };
        }

        return new AuthResult { Success = true, User = user };
    }

    public User? GetUser(Guid id) => _userRepository.GetById(id);

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static bool SecureEquals(string a, string b)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b));
    }
}
