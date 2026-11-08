using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Exceptions;
using HomeFinance.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HomeFinance.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return new AuthResult { Success = false, Error = "El correo es obligatorio y debe ser válido." };
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return new AuthResult { Success = false, Error = "La contraseña debe tener al menos 8 caracteres." };
        }

        var normalizedEmail = EmailNormalizer.Normalize(email);
        if (await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return new AuthResult { Success = false, DuplicateEmail = true, Error = "El correo ya existe en el sistema." };
        }

        var user = new User
        {
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        try
        {
            await _userRepository.AddAsync(user, cancellationToken);
            return new AuthResult { Success = true, User = user };
        }
        catch (DuplicateEmailException)
        {
            return new AuthResult { Success = false, DuplicateEmail = true, Error = "El correo ya existe en el sistema." };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return new AuthResult { Success = false, Error = "Credenciales inválidas." };
        }

        var user = await _userRepository.GetByNormalizedEmailAsync(EmailNormalizer.Normalize(email), cancellationToken);
        if (user is null)
        {
            return new AuthResult { Success = false, Error = "Credenciales inválidas." };
        }

        if (IsLegacyHash(user.PasswordHash))
        {
            if (!VerifyLegacyPassword(password, user.PasswordHash))
            {
                return new AuthResult { Success = false, Error = "Credenciales inválidas." };
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        else
        {
            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return new AuthResult { Success = false, Error = "Credenciales inválidas." };
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        return new AuthResult { Success = true, User = user };
    }

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken = default) =>
        _userRepository.GetByIdAsync(id, cancellationToken);

    private static bool IsLegacyHash(string passwordHash) =>
        passwordHash.Length == 64 && passwordHash.All(Uri.IsHexDigit);

    private static bool VerifyLegacyPassword(string password, string legacyHash)
    {
        var computedHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.ASCII.GetBytes(computedHash),
            System.Text.Encoding.ASCII.GetBytes(legacyHash));
    }
}
