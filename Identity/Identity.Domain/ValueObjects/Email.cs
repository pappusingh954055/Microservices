using System.Text.RegularExpressions;

namespace Identity.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Invalid email format");

        return new Email(value.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
