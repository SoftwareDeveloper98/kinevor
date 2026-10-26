using Kinevor.Platform.Domain.ValueObjects;

namespace Kinevor.Platform.Tests;

/// <summary>Verifies the shared e-mail and phone value objects.</summary>
public class ValueObjectTests
{
    /// <summary>E-mail input is trimmed and lower-cased.</summary>
    [Fact]
    public void Email_NormalisesCaseAndWhitespace()
    {
        var email = Email.Create("  Coach@Kinevor.COM ").Value;

        Assert.Equal("coach@kinevor.com", email.Value);
    }

    /// <summary>Invalid e-mail input fails with a validation code.</summary>
    [Fact]
    public void Email_RejectsInvalidFormat()
    {
        var result = Email.Create("not-an-email");

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Email", result.Error.Code);
    }

    /// <summary>Phone separators are stripped and a leading plus is preserved.</summary>
    [Fact]
    public void PhoneNumber_StripsSeparators()
    {
        var phone = PhoneNumber.Create("+20 (100) 123-4567").Value;

        Assert.Equal("+201001234567", phone.Value);
    }

    /// <summary>Structural equality applies to value objects.</summary>
    [Fact]
    public void Email_UsesStructuralEquality()
    {
        var first = Email.Create("coach@kinevor.com").Value;
        var second = Email.Create("COACH@kinevor.com").Value;

        Assert.Equal(first, second);
    }
}
