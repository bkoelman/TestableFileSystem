using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestableFileSystem.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileSystemUsageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FS01";
        private const string Title = "Usage of non-testable file system.";
        private const string InternMessageFormat = "{0} of '{1}' should be replaced by '{2}'.";
        private const string ExternMessageFormat = "{0} '{1}' should be passed a 'System.IO.Stream' instead of a file path.";
        private const string Category = "API Usage";
        private const string Description = "A file system API is being used for which a testable alternative exists.";
        private const string HelpLinkUri = "https://github.com/bkoelman/TestableFileSystem";

        [NotNull]
        private static readonly DiagnosticDescriptor InternRule = new DiagnosticDescriptor(DiagnosticId, Title,
            InternMessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

        [NotNull]
        private static readonly DiagnosticDescriptor ExternRule = new DiagnosticDescriptor(DiagnosticId, Title,
            ExternMessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(InternRule, ExternRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var typeRegistry = new TypeRegistry(startContext.Compilation);
                if (!typeRegistry.IsComplete)
                {
                    return;
                }

                var memberRegistry = new MemberRegistry(typeRegistry);

                startContext.RegisterSyntaxNodeAction(
                    syntaxContext => AnalyzeMemberAccessSyntax(syntaxContext, typeRegistry, memberRegistry),
                    SyntaxKind.SimpleMemberAccessExpression);

                startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeObjectCreationSyntax(syntaxContext, memberRegistry),
                    SyntaxKind.ObjectCreationExpression);

                startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeClassDeclarationSyntax(syntaxContext, typeRegistry),
                    SyntaxKind.ClassDeclaration);

                startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeParameterSyntax(syntaxContext, typeRegistry),
                    SyntaxKind.Parameter);

                startContext.RegisterSymbolAction(symbolContext => AnalyzeFieldSymbol(symbolContext, typeRegistry),
                    SymbolKind.Field);

                startContext.RegisterSymbolAction(symbolContext => AnalyzePropertySymbol(symbolContext, typeRegistry),
                    SymbolKind.Property);

                startContext.RegisterSymbolAction(symbolContext => AnalyzeMethodSymbol(symbolContext, typeRegistry),
                    SymbolKind.Method);
            });
        }

        private void AnalyzeMemberAccessSyntax(SyntaxNodeAnalysisContext context, [NotNull] TypeRegistry typeRegistry,
            [NotNull] MemberRegistry memberRegistry)
        {
            var memberAccessSyntax = (MemberAccessExpressionSyntax)context.Node;

            ISymbol symbol = context.SemanticModel.GetSymbolInfo(memberAccessSyntax).Symbol;
            if (symbol == null)
            {
                return;
            }

            string systemMemberName = symbol.GetCompleteMemberName();
            string testableMemberName = memberRegistry.TryResolveSystemMember(systemMemberName);

            if (testableMemberName != null)
            {
                Location location = GetMemberInvocationLocation(memberAccessSyntax);
                ReportInternDiagnosticAt(location, systemMemberName, testableMemberName, false, context.ReportDiagnostic);
            }
            else if (symbol is IMethodSymbol methodSymbol && !typeRegistry.SystemTypes.Contains(methodSymbol.ContainingType) &&
                ExternMemberHeuristic.HasFilePathParameter(methodSymbol))
            {
                Location location = GetMemberInvocationLocation(memberAccessSyntax);
                ReportExternDiagnosticAt(location, systemMemberName, false, context.ReportDiagnostic);
            }
        }

        [NotNull]
        private Location GetMemberInvocationLocation([NotNull] MemberAccessExpressionSyntax memberAccessSyntax)
        {
            SyntaxNode lastChild = memberAccessSyntax.ChildNodes().LastOrDefault();
            return lastChild != null ? lastChild.GetLocation() : memberAccessSyntax.GetLocation();
        }

        private void AnalyzeObjectCreationSyntax(SyntaxNodeAnalysisContext context, [NotNull] MemberRegistry memberRegistry)
        {
            var objectCreationSyntax = (ObjectCreationExpressionSyntax)context.Node;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

            if (methodSymbol?.MethodKind != MethodKind.Constructor)
            {
                return;
            }

            string systemTypeName = methodSymbol.ContainingType.GetCompleteTypeName();
            string testableMemberName = memberRegistry.TryResolveSystemConstructor(systemTypeName);
            if (testableMemberName != null)
            {
                ReportInternDiagnosticAt(objectCreationSyntax.Type.GetLocation(), systemTypeName, testableMemberName, true,
                    context.ReportDiagnostic);
            }
            else if (ExternMemberHeuristic.HasFilePathParameter(methodSymbol))
            {
                ReportExternDiagnosticAt(objectCreationSyntax.Type.GetLocation(), systemTypeName, true, context.ReportDiagnostic);
            }
        }

        private void AnalyzeClassDeclarationSyntax(SyntaxNodeAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (typeSymbol != null)
            {
                INamedTypeSymbol fileSystemInfoTypeSymbol =
                    typeRegistry.SystemTypes.Single(symbol => symbol.Name == "FileSystemInfo");

                if (fileSystemInfoTypeSymbol.Equals(typeSymbol.BaseType))
                {
                    INamedTypeSymbol testableTypeSymbol = typeRegistry.TryResolveSystemType(fileSystemInfoTypeSymbol);
                    if (testableTypeSymbol != null)
                    {
                        Location location = classDeclarationSyntax.Identifier.GetLocation();
                        string systemTypeName = fileSystemInfoTypeSymbol.GetCompleteTypeName();
                        string testableTypeName = testableTypeSymbol.GetCompleteTypeName();

                        ReportInternDiagnosticAt(location, systemTypeName, testableTypeName, false, context.ReportDiagnostic);
                    }
                }
            }
        }

        private void AnalyzeParameterSyntax(SyntaxNodeAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var parameterSyntax = (ParameterSyntax)context.Node;

            IParameterSymbol parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameterSyntax);
            if (parameterSymbol == null)
            {
                return;
            }

            AnalyzeMemberSymbol(parameterSymbol, parameterSymbol.Type, typeRegistry, context.ReportDiagnostic);
        }

        private void AnalyzeFieldSymbol(SymbolAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            AnalyzeMemberSymbol(fieldSymbol, fieldSymbol.Type, typeRegistry, context.ReportDiagnostic);
        }

        private void AnalyzePropertySymbol(SymbolAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            AnalyzeMemberSymbol(propertySymbol, propertySymbol.Type, typeRegistry, context.ReportDiagnostic);
        }

        private void AnalyzeMethodSymbol(SymbolAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (methodSymbol.IsAccessor())
            {
                return;
            }

            AnalyzeMemberSymbol(methodSymbol, methodSymbol.ReturnType, typeRegistry, context.ReportDiagnostic);
        }

        private void AnalyzeMemberSymbol([NotNull] ISymbol memberSymbol, [NotNull] ITypeSymbol memberTypeSymbol,
            [NotNull] TypeRegistry typeRegistry, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            INamedTypeSymbol testableTypeSymbol = typeRegistry.TryResolveSystemType(memberTypeSymbol);

            if (testableTypeSymbol != null)
            {
                ReportInternDiagnosticAt(memberSymbol.Locations.First(), memberTypeSymbol.GetCompleteTypeName(),
                    testableTypeSymbol.GetCompleteTypeName(), false, reportDiagnostic);
            }
        }

        private void ReportInternDiagnosticAt([NotNull] Location location, [NotNull] string typeOrMemberName,
            [NotNull] string replacementName, bool isConstruction, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            string text = isConstruction ? "Construction" : "Usage";
            reportDiagnostic(Diagnostic.Create(InternRule, location, text, typeOrMemberName, replacementName));
        }

        private void ReportExternDiagnosticAt([NotNull] Location location, [NotNull] string memberName, bool isConstruction,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            string text = isConstruction ? "Constructor of" : "Member";
            reportDiagnostic(Diagnostic.Create(ExternRule, location, text, memberName));
        }
    }
}
