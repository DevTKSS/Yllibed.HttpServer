using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yllibed.HttpServer.Extensions; // For AddHttpHandlerAndRegister

namespace Yllibed.HttpServer.Handlers.Uno.Extensions;

/// <summary>
/// Extensions to configure OAuthCallbackHandler via Microsoft DI (aligned with GuardExtensions pattern).
/// </summary>
public static class OAuthCallbackExtensions
{
	/// <summary>
	/// Registers OAuthCallbackHandler with options support and exposes it as both its concrete type and as IAuthCallbackHandler.
	/// </summary>
	public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services)
	{
		services.AddSingleton<OAuthCallbackHandler>(sp => new OAuthCallbackHandler(sp.GetRequiredService<IOptions<AuthCallbackHandlerOptions>>()));
		services.AddSingleton<IAuthCallbackHandler>(sp => sp.GetRequiredService<OAuthCallbackHandler>());
		return services;
	}

	/// <summary>
	/// Registers OAuthCallbackHandler with configuration delegate.
	/// </summary>
	public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services, Action<AuthCallbackHandlerOptions> configure)
	{
		services.Configure(configure);
		return services.AddOAuthCallbackHandler();
	}

	/// <summary>
	/// Registers OAuthCallbackHandler and automatically wires it into the Server pipeline.
	/// Avoids manual resolution and explicit Server.RegisterHandler calls by consumers.
	/// </summary>
	public static IServiceCollection AddOAuthCallbackHandlerAndRegister(this IServiceCollection services, Action<AuthCallbackHandlerOptions>? configure = null)
	{
		if (configure != null)
		{
			services.Configure(configure);
		}
		services.AddOAuthCallbackHandler();
		// Ensure automatic registration when Server is created
		services.AddHttpHandlerAndRegister<OAuthCallbackHandler>();
		// Registration object that hooks the handler into the server upon construction (eager path)
		services.AddSingleton<OAuthCallbackRegistration>();
		return services;
	}

	private sealed class OAuthCallbackRegistration : IDisposable
	{
		private readonly IDisposable _registration;
		public OAuthCallbackRegistration(Server server, OAuthCallbackHandler handler)
		{
			// Register early; Server preserves registration order
			_registration = server.RegisterHandler(handler);
		}
		public void Dispose() => _registration.Dispose();
	}
}
