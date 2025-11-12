using Yllibed.HttpServer.Handlers.Uno.Extensions;
using Microsoft.Extensions.Options;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Yllibed.HttpServer.Extensions; // For AddYllibedHttpServer

namespace Yllibed.HttpServer.Handlers.Uno.Tests;

public class OAuthCallbackExtensionsTests
{
	[Fact]
	public void AddOAuthCallbackHandler_RegistersHandlerAndInterface()
	{
		var services = new ServiceCollection();
		var options = new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/callback" };
		services.AddSingleton(Options.Create(options));

		services.AddOAuthCallbackHandler();

		using var sp = services.BuildServiceProvider();
		var concrete = sp.GetService<OAuthCallbackHandler>();
		var asInterface = sp.GetService<IAuthCallbackHandler>();

		concrete.ShouldNotBeNull();
		asInterface.ShouldNotBeNull();
		ReferenceEquals(concrete, asInterface).ShouldBeTrue();
		concrete!.CallbackUri.ShouldBe(new Uri("http://localhost/callback"));
	}

	[Fact]
	public async Task AddOAuthCallbackHandlerAndRegister_RegistersIntoServerPipeline()
	{
		var services = new ServiceCollection();
		services.AddLogging();

		var namedOptions = new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/callback" };
		services.AddSingleton(Options.Create(namedOptions));

		services.AddYllibedHttpServer();
		services.AddOAuthCallbackHandlerAndRegister();

		await using var sp = services.BuildServiceProvider();
		var server = sp.GetRequiredService<Server>();
		var (uri4, _) = server.Start();
		var callbackUri = new Uri(uri4, "/callback?code=abc");

		var client = new HttpClient();
		var response = await client.GetAsync(callbackUri, TestContext.Current.CancellationToken);
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var handler = sp.GetRequiredService<IAuthCallbackHandler>();
		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)200);
		result.ResponseData.ShouldNotBeNull();
		result.ResponseData!.ShouldContain("code=abc");
	}
}
