using Xunit;
using Shouldly;
using Yllibed.HttpServer.Handlers.Uno.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Yllibed.HttpServer.Handlers.Uno.Tests;

public class OAuthCallbackExtensionsTests
{
	private sealed class TestOptionsSnapshot : IOptionsSnapshot<AuthCallbackHandlerOptions>
	{
		private readonly Dictionary<string, AuthCallbackHandlerOptions> _map;
		public TestOptionsSnapshot(Dictionary<string, AuthCallbackHandlerOptions> map) => _map = map;
		public AuthCallbackHandlerOptions Value => Get(AuthCallbackHandlerOptions.DefaultName);
		public AuthCallbackHandlerOptions Get(string? name) => _map.TryGetValue(name ?? AuthCallbackHandlerOptions.DefaultName, out var v)
			? v
			: new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/invalid" };
	}

	[Fact]
	public void AddOAuthCallbackHandler_RegistersHandlerAndInterface()
	{
		var services = new ServiceCollection();
		services.AddOptions();
		services.AddSingleton(Options.Create(new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/callback" }));

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
		services.AddOptions();
		services.AddLogging();
		// Provide named options via custom snapshot to satisfy init-only property
		services.AddSingleton<IOptionsSnapshot<AuthCallbackHandlerOptions>>(
			new TestOptionsSnapshot(new Dictionary<string, AuthCallbackHandlerOptions>(StringComparer.Ordinal)
			{
				[AuthCallbackHandlerOptions.DefaultName] = new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/callback" }
			}));
		services.AddSingleton(Options.Create(new ServerOptions()));
		services.AddSingleton<Server>();
		services.AddOAuthCallbackHandlerAndRegister<OAuthCallbackHandler>();

		using var sp = services.BuildServiceProvider();
		var server = sp.GetRequiredService<Server>();
		var (uri4, _) = server.Start();
		var callbackUri = new Uri(uri4, "/callback?code=abc");

		var client = new HttpClient();
		var response = await client.GetAsync(callbackUri);
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var handler = sp.GetRequiredService<IAuthCallbackHandler>();
		var result = await handler.WaitForCallbackAsync();
		result.ResponseErrorDetail.ShouldBe((uint)200);
		result.ResponseData.ShouldNotBeNull();
		result.ResponseData!.ShouldContain("code=abc");
	}

	[Fact]
	public void AddOAuthCallbackHandler_KeyedNameResolvesOptions()
	{
		var services = new ServiceCollection();
		services.AddOptions();
		services.AddSingleton<IOptionsSnapshot<AuthCallbackHandlerOptions>>(
			new TestOptionsSnapshot(new Dictionary<string, AuthCallbackHandlerOptions>(StringComparer.Ordinal)
			{
				["Custom"] = new AuthCallbackHandlerOptions { CallbackUri = "http://localhost/customcb" }
			}));
		services.AddOAuthCallbackHandler("Custom");

		using var sp = services.BuildServiceProvider();
		var handler = sp.GetRequiredKeyedService<IAuthCallbackHandler>("Custom");
		handler.CallbackUri.ShouldBe(new Uri("http://localhost/customcb"));
	}
}
