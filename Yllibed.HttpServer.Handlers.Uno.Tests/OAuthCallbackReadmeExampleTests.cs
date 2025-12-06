namespace Yllibed.HttpServer.Handlers.Uno.Tests;

public class OAuthCallbackReadmeExampleTests
{
	[Fact]
	public async Task README_OAuthCallback_Example_Works()
	{
		// Arrange
		var services = new ServiceCollection();
		services.Configure<ServerOptions>(opts =>
		{
			opts.Port = 0; // dynamic
			opts.Hostname4 = "127.0.0.1";
			opts.Hostname6 = "::1";
		});
		services.AddSingleton(Options.Create(new AuthCallbackHandlerOptions
		{
			CallbackUri = "http://localhost/callback"
		}));
		services.AddYllibedHttpServer();
		services.AddOAuthCallbackHandlerAndRegister();
		await using var sp = services.BuildServiceProvider();
		var server = sp.GetRequiredService<Server>();

		// Act
		var (uri4, _) = server.Start();
		var callbackRequest = new Uri(uri4, "/callback?code=readme-test");
		using var client = new HttpClient();
		var response = await client.GetAsync(callbackRequest, TestContext.Current.CancellationToken);
		var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		body.ShouldContain("Authentication completed successfully");
		var handler = sp.GetRequiredService<IAuthCallbackHandler>();
		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)200);
		result.ResponseData.ShouldNotBeNull();
		result.ResponseData!.ShouldContain("code=readme-test");
	}

	[Theory]
	[InlineData("https://localhost:5001/etsy/callback", "etsy-theory-https")]
	//[InlineData("http://localhost:5001/etsy/callback", "etsy-theory-http")]
	public async Task OAuthCallback_Url_And_Configured_Server_Port_With_Loopback_Works(string callbackUri, string code)
	{
		// Arrange unified theory: dynamic server port, handler registered; callbackUri path match regardless of scheme/port
		var services = new ServiceCollection();
		services.Configure<ServerOptions>(opts =>
		{
			opts.Port = 5001;
			opts.Hostname4 = "localhost";
			opts.BindAddress4 = IPAddress.Loopback;
		});
		services.AddSingleton(Options.Create(new AuthCallbackHandlerOptions
		{
			CallbackUri = callbackUri,
		}));
		services.AddYllibedHttpServer();
		services.AddOAuthCallbackHandlerAndRegister();
		await using var sp = services.BuildServiceProvider();
		var server = sp.GetRequiredService<Server>();

		// Act
		var (uri4, _) = server.Start();
		var callbackRequest = new Uri(uri4, $"/etsy/callback?code={code}");
		using var client = new HttpClient();
		var response = await client.GetAsync(callbackRequest, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		var handler = sp.GetRequiredService<IAuthCallbackHandler>();
		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)200);
		result.ResponseData.ShouldNotBeNull();
		result.ResponseData!.ShouldContain(code);
	}
}
