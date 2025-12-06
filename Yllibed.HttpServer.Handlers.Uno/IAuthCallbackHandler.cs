namespace Yllibed.HttpServer.Handlers.Uno;

public interface IAuthCallbackHandler : IHttpHandler
{
	public Uri CallbackUri { get; }
	public Task<WebAuthenticationResult> WaitForCallbackAsync();
}
