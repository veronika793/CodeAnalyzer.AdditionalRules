namespace CodeAnalyzers.AdditionalRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    ///  Use this rule or CR9000 - only one of the two should be enabled 
    ///  This has default line length of 120 characters without option of changing it from stylecop.json file
    ///  Setup according to visual studio characters count 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class CR9001LineLengthRestrictionWithDefaultValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CR9001";
        internal const string Title = "Line length is too long";
        internal const string MessageFormat = "{0}";
        internal const string Category = "Syntax";

        internal const string SettingsFileName = "stylecop.json";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        private readonly int maximumLineLength = 119;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        private void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);

            var fileText = context.Tree.GetText(context.CancellationToken);

            var startTrace = false;

            foreach (var line in fileText.Lines)
            {
                var difference = line.End - line.Start;

                var location = root.SyntaxTree.GetLocation(line.Span);

                var node = root.FindNode(location.SourceSpan);

                if (node.IsKind(SyntaxKind.NamespaceDeclaration))
                {
                    startTrace = true;
                }

                if (difference > this.maximumLineLength)
                {
                    if (!node.IsKind(SyntaxKind.UsingDirective) &&
                        !node.IsKind(SyntaxKind.NamespaceDeclaration) &&
                        !node.IsKind(SyntaxKind.ClassDeclaration) &&
                        !node.IsKind(SyntaxKind.EnumMemberDeclaration) &&
                        !node.IsKind(SyntaxKind.EnumDeclaration) &&
                        !node.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                        !node.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) &&
                        startTrace)
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule,
                            location,
                            $"Exceeds maximum line length of {maximumLineLength + 1} characters.");

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
