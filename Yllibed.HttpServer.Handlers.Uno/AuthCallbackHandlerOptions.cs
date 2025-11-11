using System.ComponentModel.DataAnnotations;

namespace Yllibed.HttpServer.Handlers.Uno;

public record AuthCallbackHandlerOptions
{
	public const string DefaultName = "AuthCallback";
	/// <summary>
	/// Configures the expected URI for authentication Callbacks.
	/// </summary>
	[Required, Url]
	public string? CallbackUri { get; init; }

}
