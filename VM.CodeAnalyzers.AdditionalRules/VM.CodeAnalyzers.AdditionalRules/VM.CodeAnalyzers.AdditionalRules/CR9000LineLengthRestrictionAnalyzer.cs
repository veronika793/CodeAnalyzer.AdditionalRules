namespace CodeAnalyzers.AdditionalRules
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CR9000LineLengthRestrictionAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "VCR9000";
        internal const string Title = "Line length is too long";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Syntax";

        internal const string SettingsFileName = "stylecop.json";

        internal static DiagnosticDescriptor Rule =
             new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
                Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        private int maximumLineLength = default(int);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        private void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);

            var fileText = context.Tree.GetText(context.CancellationToken);

            var readabilityRules = GetReadabilityRules(context);

            if (readabilityRules != null && readabilityRules.Length > 0)
            {
                this.maximumLineLength = GetLineLengthConstraints(readabilityRules, context);
            }

            if (maximumLineLength <= 0)
            {
                return;
            }

            foreach (var line in fileText.Lines)
            {
                var difference = line.End - line.Start;

                if (difference > this.maximumLineLength)
                {
                    var location = root.SyntaxTree.GetLocation(line.Span);

                    var node = root.FindNode(location.SourceSpan);
                    var text = line.Text.ToString();

                    if (!node.IsKind(SyntaxKind.UsingDirective) &&
                        !node.IsKind(SyntaxKind.NamespaceDeclaration) &&
                        !node.IsKind(SyntaxKind.ClassDeclaration) &&
                        !node.IsKind(SyntaxKind.AnonymousMethodExpression) &&
                        !node.IsKind(SyntaxKind.EnumMemberDeclaration) &&
                        !node.IsKind(SyntaxKind.EnumDeclaration) &&
                        !node.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                        !node.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                    {
                        var diagnostic = Diagnostic.Create(
                        Rule, location,
                        $"Exceeds maximum line length of {maximumLineLength} characters.");

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private int GetLineLengthConstraints(
            ImmutableArray<AdditionalText> readabilityRules,
            SyntaxTreeAnalysisContext context)
        {
            foreach (var additionalText in readabilityRules)
            {
                if (IsAnalyzerSettingsFile(additionalText.Path))
                {
                    var text = additionalText.GetText(context.CancellationToken);
                    var analyzerSettings = this.Deserizalize(text.ToString());
                    return analyzerSettings?.settings?.readabilityRules?.maximumLineLength ?? default(int);
                }
            }

            return default(int);
        }

        // Deserialize a JSON stream to a Analyzer settings - Newtonsoft.Json not working for some reason.  
        private AnalyzerSettings Deserizalize(string json)
        {
            var deserializedSettings = new AnalyzerSettings();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedSettings.GetType());
                deserializedSettings = ser.ReadObject(ms) as AnalyzerSettings;
            }

            return deserializedSettings;
        }

        private static ImmutableArray<AdditionalText> GetReadabilityRules(SyntaxTreeAnalysisContext context)
        {
            return context.Options.GetStyleCopSettings(context.CancellationToken);
        }

        internal static bool IsAnalyzerSettingsFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var fileName = Path.GetFileName(path);

            return string.Equals(fileName, SettingsFileName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
