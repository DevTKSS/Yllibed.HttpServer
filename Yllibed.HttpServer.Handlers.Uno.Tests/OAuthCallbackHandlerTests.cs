namespace Yllibed.HttpServer.Handlers.Uno.Tests;

using Microsoft.Extensions.Options;
using System.Net;
using Windows.Security.Authentication.Web;

public class OAuthCallbackHandlerTests
{
	private const string ValidHttpCallback = "http://localhost:5000/callback";
	private const string ValidHttpsCallback = "https://localhost:5000/callback";

	[Fact]
	public void Constructor_WithValidHttpUri_Succeeds()
	{
		var uri = new Uri(ValidHttpCallback);
		var handler = new OAuthCallbackHandler(uri);

		handler.CallbackUri.ShouldBe(uri);
	}

	[Fact]
	public void Constructor_WithValidHttpsUri_Succeeds()
	{
		var uri = new Uri(ValidHttpsCallback);
		var handler = new OAuthCallbackHandler(uri);

		handler.CallbackUri.ShouldBe(uri);
	}

	[Fact]
	public void Constructor_WithNullUri_Throws()
	{
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler((Uri?)null!))
			.Message.ShouldContain("CallbackUri must be an absolute URI");
	}

	[Theory]
	[InlineData("ftp://localhost:5000/callback")]
	[InlineData("file:///callback")]
	public void Constructor_WithInvalidScheme_Throws(string invalidUri)
	{
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler(new Uri(invalidUri)))
			.Message.ShouldContain("CallbackUri must be an absolute URI with HTTP or HTTPS scheme");
	}

	[Fact]
	public void Constructor_WithOptionsString_ValidHttpUri_Succeeds()
	{
		var options = new AuthCallbackHandlerOptions { CallbackUri = ValidHttpCallback };
		var handler = new OAuthCallbackHandler(options);

		handler.CallbackUri.ShouldBe(new Uri(ValidHttpCallback));
	}

	[Fact]
	public void Constructor_WithOptionsString_ValidHttpsUri_Succeeds()
	{
		var options = new AuthCallbackHandlerOptions { CallbackUri = ValidHttpsCallback };
		var handler = new OAuthCallbackHandler(options);

		handler.CallbackUri.ShouldBe(new Uri(ValidHttpsCallback));
	}

	[Fact]
	public void Constructor_WithOptionsString_NullCallbackUri_Throws()
	{
		var options = new AuthCallbackHandlerOptions { CallbackUri = null };
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler(options))
			.Message.ShouldContain("CallbackUri must be an absolute URI");
	}

	[Fact]
	public void Constructor_WithOptionsString_InvalidScheme_Throws()
	{
		var options = new AuthCallbackHandlerOptions { CallbackUri = "ftp://localhost:5000/callback" };
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler(options))
			.Message.ShouldContain("CallbackUri must be an absolute URI with HTTP or HTTPS scheme");
	}

	[Fact]
	public void Constructor_WithIOptions_ValidHttpUri_Succeeds()
	{
		var options = Options.Create(new AuthCallbackHandlerOptions { CallbackUri = ValidHttpCallback });
		var handler = new OAuthCallbackHandler(options);

		handler.CallbackUri.ShouldBe(new Uri(ValidHttpCallback));
	}

	[Fact]
	public void Constructor_WithIOptions_ValidHttpsUri_Succeeds()
	{
		var options = Options.Create(new AuthCallbackHandlerOptions { CallbackUri = ValidHttpsCallback });
		var handler = new OAuthCallbackHandler(options);

		handler.CallbackUri.ShouldBe(new Uri(ValidHttpsCallback));
	}

	[Fact]
	public void Constructor_WithIOptions_NullCallbackUri_Throws()
	{
		var options = Options.Create(new AuthCallbackHandlerOptions { CallbackUri = null });
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler(options))
			.Message.ShouldContain("CallbackUri must be an absolute URI");
	}

	[Fact]
	public void Constructor_WithIOptions_InvalidScheme_Throws()
	{
		var options = Options.Create(new AuthCallbackHandlerOptions { CallbackUri = "ftp://localhost/callback" });
		Should.Throw<ArgumentException>(() => new OAuthCallbackHandler(options))
			.Message.ShouldContain("CallbackUri must be an absolute URI with HTTP or HTTPS scheme");
	}

	[Fact]
	public async Task HandleRequest_WithSuccessCode_ReturnsSuccess()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		server.RegisterHandler(new StaticHandler("/fallback", "text/plain", "fallback"));
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?code=auth-code-123");

		using var client = new HttpClient();
		var response = await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var result = await handler.WaitForCallbackAsync();
		result.ResponseStatus.ShouldBe(WebAuthenticationStatus.Success);
		result.ResponseErrorDetail.ShouldBe((uint)200);
	}

	[Fact]
	public async Task HandleRequest_WithAccessDeniedError_Returns403()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=access_denied");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseStatus.ShouldBe(WebAuthenticationStatus.UserCancel);
		result.ResponseErrorDetail.ShouldBe((uint)403);
	}

	[Fact]
	public async Task HandleRequest_WithInvalidClientError_Returns401()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=invalid_client");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)401);
	}

	[Fact]
	public async Task HandleRequest_WithUnauthorizedClientError_Returns401()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=unauthorized_client");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)401);
	}

	[Fact]
	public async Task HandleRequest_WithInvalidScopeError_Returns401()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=invalid_scope");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)401);
	}

	[Fact]
	public async Task HandleRequest_WithTemporarilyUnavailableError_Returns503()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=temporarily_unavailable");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)503);
	}

	[Fact]
	public async Task HandleRequest_WithUnsupportedGrantTypeError_Returns500()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=unsupported_grant_type");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)500);
	}

	[Fact]
	public async Task HandleRequest_WithUnknownError_Returns400()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?error=unknown_error");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)400);
	}

	[Fact]
	public async Task HandleRequest_WithMultipleParameters_ParsesCorrectly()
	{
		var handler = new OAuthCallbackHandler(new Uri("http://localhost/callback"));
		using var server = new Server();
		server.RegisterHandler(handler);
		var (uri, _) = server.Start();
		var callbackUri = new Uri(uri, "/callback?code=auth-code-123&state=state-value&session_state=session");

		using var client = new HttpClient();
		await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);

		var result = await handler.WaitForCallbackAsync();
		result.ResponseData.ShouldNotBeNull();
		result.ResponseData.ShouldContain("code=auth-code-123");
		result.ResponseData.ShouldContain("state=state-value");
	}
}
