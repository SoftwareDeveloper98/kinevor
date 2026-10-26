using Kinevor.Platform.Domain.Common;

namespace Kinevor.Platform.Tests;

/// <summary>Verifies the success and failure semantics of <see cref="Result"/>.</summary>
public class ResultTests
{
    /// <summary>A successful result exposes its value and no error.</summary>
    [Fact]
    public void Success_CarriesValueAndNoError()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    /// <summary>A failed result refuses value access and preserves the error.</summary>
    [Fact]
    public void Failure_PreservesErrorAndThrowsOnValue()
    {
        var error = Error.Validation("Name", "Name is required.");
        var result = Result.Failure<string>(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
