using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Tests;

public class SolutionNameValidatorTests
{
    [Theory]
    [InlineData("MySolution")]
    [InlineData("Solution123")]
    [InlineData("A")]
    [InlineData("Valid_Name")]
    public void Validate_ValidNames_ReturnsNoErrors(string name)
    {
        var errors = SolutionNameValidator.Validate(name);
        Assert.Empty(errors);
        Assert.True(SolutionNameValidator.IsValid(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("123Solution")]
    [InlineData("My Solution")]
    [InlineData("My-Solution")]
    [InlineData("My.Solution")]
    [InlineData("Solution!")]
    public void Validate_InvalidNames_ReturnsErrors(string? name)
    {
        var errors = SolutionNameValidator.Validate(name);
        Assert.NotEmpty(errors);
        Assert.False(SolutionNameValidator.IsValid(name));
    }

    [Fact]
    public void Validate_TooLongName_ReturnsError()
    {
        var longName = new string('A', SolutionNameValidator.MaxLength + 1);
        var errors = SolutionNameValidator.Validate(longName);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("at most"));
    }
}
