namespace Yllibed.HttpServer.Handlers.Uno.Tests;

public class AuthCallbackHandlerOptionsTests
{

	private static List<ValidationResult> Validate(object model)
	{
		var results = new List<ValidationResult>();
		var context = new ValidationContext(model);
		Validator.TryValidateObject(model, context, results, validateAllProperties: true);
		return results;
	}

	[Fact]
	public void Validation_Fails_When_CallbackUri_Is_Null()
	{
		// Arrange
		var opts = new AuthCallbackHandlerOptions { CallbackUri = null };

		// Act
		var results = Validate(opts);

		// Assert
		results.ShouldNotBeEmpty();
		results.ShouldContain(r => r.MemberNames.Contains(nameof(AuthCallbackHandlerOptions.CallbackUri)));
	}

	[Fact]
	public void Validation_Fails_When_CallbackUri_Is_Invalid_Url()
	{
		// Arrange
		var opts = new AuthCallbackHandlerOptions { CallbackUri = "not-a-url" };

		// Act
		var results = Validate(opts);

		// Assert
		results.ShouldNotBeEmpty();
		results.ShouldContain(r => r.MemberNames.Contains(nameof(AuthCallbackHandlerOptions.CallbackUri)));
	}

	[Fact]
	public void Validation_Passes_With_Valid_Url()
	{
		// Arrange
		var opts = new AuthCallbackHandlerOptions { CallbackUri = "http://example.com/callback" };

		// Act
		var results = Validate(opts);

		// Assert
		results.ShouldBeEmpty();
	}
}
