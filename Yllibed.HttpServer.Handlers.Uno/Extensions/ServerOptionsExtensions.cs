namespace Yllibed.HttpServer.Handlers.Uno.Extensions;

public static class ServerOptionsExtensions
{
	/// <summary>
	/// Creates an absolute URI using the IPv4 hostname and port specified in the given server options.
	/// </summary>
	/// <param name="serverOptions">The server options containing the IPv4 hostname and port to use for constructing the URI. Cannot be null.</param>
	/// <returns>A new <see cref="Uri"/> instance representing the IPv4 address and port from the specified server options.</returns>
	/// <remarks>
	/// Can be used to fill <see cref="AuthCallbackHandlerOptions"/> from existing <see cref="ServerOptions"/>.
	/// </remarks>
	/// <exception cref="ArgumentException">Thrown if <paramref name="relativePath"/> is <see langword="null"> or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="serverOptions"/> is <see langword="null">.</exception>
	/// <exception cref="UriFormatException">Thrown if the constructed URI is not valid.</exception>
	public static Uri ToUri4(this ServerOptions serverOptions, string relativePath)
	{
		var builder = new UriBuilder("http", serverOptions.Hostname4, serverOptions.Port, relativePath);
		return new Uri(builder.ToString(), UriKind.Absolute);
	}

	/// <summary>
	/// Creates an absolute URI using the IPv6 hostname and port specified in the given server options.
	/// </summary>
	/// <param name="serverOptions">The server options containing the IPv6 hostname and port to use when constructing the URI. Cannot be null.</param>
	/// <returns>A new <see cref="Uri"/> instance representing the server's IPv6 address and port.</returns>
	/// <remarks>
	/// Can be used to fill <see cref="AuthCallbackHandlerOptions"/> from existing <see cref="ServerOptions"/>.
	/// </remarks>
	/// <exception cref="ArgumentException">Thrown if <paramref name="relativePath"/> is <see langword="null"> or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="serverOptions"/> is <see langword="null">.</exception>
	/// <exception cref="UriFormatException">Thrown if the constructed URI is not valid.</exception>
	public static Uri ToUri6(this ServerOptions serverOptions, string relativePath)
	{
		var builder = new UriBuilder("http", serverOptions.Hostname6, serverOptions.Port, relativePath);
		return new Uri(builder.ToString(), UriKind.Absolute);
	}

	/// <summary>
	/// Combines the server's base URL with the specified relative path and returns the resulting absolute URL as a string.
	/// </summary>
	/// <param name="serverOptions">The server options containing the base URL to use for constructing the absolute URL. Cannot be null.</param>
	/// <param name="relativePath">The relative path to append to the server's base URL. Must not be null or empty.</param>
	/// <returns>A string representing the absolute URL formed by combining the server's base URL with the specified relative path.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="relativePath"/> is <see langword="null"> or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="serverOptions"/> is <see langword="null">.</exception>
	/// <exception cref="UriFormatException">Thrown if the constructed URL is not valid.</exception>
	public static string ToUrl4(this ServerOptions serverOptions, string relativePath)
	=> serverOptions.ToUri4(relativePath).ToString();

	/// <summary>
	/// Creates an absolute URL string by combining the server's base address with the specified relative path using IPv6
	/// formatting.
	/// </summary>
	/// <param name="serverOptions">The server options containing the base address to use for constructing the URL. Cannot be null.</param>
	/// <param name="relativePath">The relative path to append to the server's base address. Must not be null or empty.</param>
	/// <returns>A string representing the absolute URL formed by combining the base address from the server options with the specified relative path, using IPv6 formatting.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="relativePath"/> is <see langword="null"> or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="serverOptions"/> is <see langword="null">.</exception>
	/// <exception cref="UriFormatException">Thrown if the constructed URL is not valid.</exception>
	public static string ToUrl6(this ServerOptions serverOptions, string relativePath)
		=> serverOptions.ToUri6(relativePath).ToString();
}
