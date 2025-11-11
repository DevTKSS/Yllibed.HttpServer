using System.Text.RegularExpressions;

namespace Yllibed.HttpServer.Handlers.Uno.Extensions;

public static partial class UriExtensions
{
#if NET7_0_OR_GREATER
	// Regex source generator: parses key=value pairs in a query string. Last value wins on duplicates.
	[GeneratedRegex(@"([^?&=]+)=([^&]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
	private static partial Regex QueryParameterRegex();
#else
	// Older frameworks don't have RegexOptions.NonBacktracking; fall back to equivalent safe options
	private static readonly Regex _queryParameterRegex = new(@"([^?&=]+)=([^&]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
	private static Regex QueryParameterRegex() => _queryParameterRegex;
#endif

	/// <summary>
	/// Parses the query parameters from the given <see cref="Uri"/> into a dictionary.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> from which to extract query parameters.</param>
	/// <returns>A <see cref="IDictionary{TKey, TValue}"/> containing the query parameters as key-value pairs.</returns>
	public static IDictionary<string, string> GetParameters(this Uri uri)
	{
		return QueryParameterRegex()
			.Matches(uri.Query)
			.Cast<Match>()
			.Select(m => new KeyValuePair<string, string>(
				Uri.UnescapeDataString(m.Groups[1].Value),
				Uri.UnescapeDataString(m.Groups[2].Value)))
			.GroupBy(kv => kv.Key, StringComparer.Ordinal)
			.ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal);
	}
}
