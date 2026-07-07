namespace ShopNest.Application.Common;

public sealed class EmailNotVerifiedException : Exception
{
    public string Email { get; }

    public EmailNotVerifiedException(string email) 
        : base("Email is not verified. Please verify your email first.")
    {
        Email = email;
    }
}
