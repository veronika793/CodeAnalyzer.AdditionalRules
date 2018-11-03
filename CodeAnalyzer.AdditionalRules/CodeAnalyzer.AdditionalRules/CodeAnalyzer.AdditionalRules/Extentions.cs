namespace CodeAnalyzer.AdditionalRules
{
    using System.Collections.Immutable;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    static class Extentions
    {
        internal static ImmutableArray<AdditionalText> GetStyleCopSettings(
            this AnalyzerOptions options,
            CancellationToken cancellationToken)
            => options.AdditionalFiles;
    }
}
