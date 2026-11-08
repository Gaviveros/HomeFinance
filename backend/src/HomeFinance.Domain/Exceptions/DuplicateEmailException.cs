namespace HomeFinance.Domain.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("El correo ya existe en el sistema.")
    {
    }
}
