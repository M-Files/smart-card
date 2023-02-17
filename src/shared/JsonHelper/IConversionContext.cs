
namespace MFiles.Internal.Json
{
	/// <summary>
	/// Declares conversion context.
	/// </summary>
	public interface IConversionContext
	{
		/// <summary>
		/// Are aliases resolved during the conversion?
		/// </summary>
		bool ResolveAliases { get; }

		/// <summary>
		/// Alias resolver for resolving aliases.
		/// </summary>
		IAliasResolver AliasResolver { get; }

		/// <summary>
		/// How the errors are reported.
		/// </summary>
		Enumerations.ErrorReporting ErrorReporting { get; }
	}
}
