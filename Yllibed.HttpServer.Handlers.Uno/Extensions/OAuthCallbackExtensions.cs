using Microsoft.Extensions.DependencyInjection;
using Yllibed.HttpServer.Extensions;

namespace Yllibed.HttpServer.Handlers.Uno.Extensions;

/// <summary>
/// Extensions to configure OAuthCallbackHandler via Microsoft DI (aligned with GuardExtensions pattern).
/// </summary>
public static class OAuthCallbackExtensions
{
	/// <summary>
	/// Registers OAuthCallbackHandler with options support and exposes it as both its concrete type, IAuthCallbackHandler, and IHttpHandler.
	/// </summary>
	public static IServiceCollection AddOAuthCallbackHandler(this IServiceCollection services)
	{
		services.AddSingleton<OAuthCallbackHandler>(sp => new OAuthCallbackHandler(sp.GetRequiredService<IOptions<AuthCallbackHandlerOptions>>()));
		services.AddSingleton<IAuthCallbackHandler>(sp => sp.GetRequiredService<OAuthCallbackHandler>());
		services.AddSingleton<IHttpHandler>(sp => sp.GetRequiredService<OAuthCallbackHandler>());
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
	/// Uses the automatic HandlerRegistrationService which ensures the handler is registered when Server is created.
	/// </summary>
	public static IServiceCollection AddOAuthCallbackHandlerAndRegister(this IServiceCollection services, Action<AuthCallbackHandlerOptions>? configure = null)
	{
		if (configure != null)
		{
			services.Configure(configure);
		}
		services.AddOAuthCallbackHandler();
		// Use the automatic registration mechanism provided by AddYllibedHttpServer
		// This ensures handlers are registered when the Server is instantiated
		// The GuardExtensions pattern was tested to always fail the test compared to this!!
		services.AddHttpHandlerAndRegister<OAuthCallbackHandler>();
		return services;
	}
}
