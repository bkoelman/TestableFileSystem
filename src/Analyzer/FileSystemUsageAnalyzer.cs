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
        private const string MessageFormat = "Usage of '{0}' should be replaced by '{1}'.";
        private const string Description = "A file system API is being used for which a testable alternative exists.";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            "API Usage", DiagnosticSeverity.Warning, true, Description, "https://github.com/bkoelman/TestableFileSystem");

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

                startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeMemberAccessSyntax(syntaxContext, memberRegistry),
                    SyntaxKind.SimpleMemberAccessExpression);

                startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeObjectCreationSyntax(syntaxContext, typeRegistry),
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

        private void AnalyzeMemberAccessSyntax(SyntaxNodeAnalysisContext context, [NotNull] MemberRegistry memberRegistry)
        {
            ISymbol symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
            if (symbol == null)
            {
                return;
            }

            string systemMemberName = symbol.GetCompleteMemberName();
            string testableMemberName = memberRegistry.TryResolveSystemMember(systemMemberName);

            if (testableMemberName != null)
            {
                ReportDiagnosticAt(context.Node.GetLocation(), systemMemberName, testableMemberName, context.ReportDiagnostic);
            }
        }

        private void AnalyzeObjectCreationSyntax(SyntaxNodeAnalysisContext context, [NotNull] TypeRegistry typeRegistry)
        {
            var objectCreationSyntax = (ObjectCreationExpressionSyntax)context.Node;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

            if (methodSymbol?.MethodKind != MethodKind.Constructor)
            {
                return;
            }

            INamedTypeSymbol testableTypeSmbol = typeRegistry.TryResolveSystemType(methodSymbol.ContainingType);
            if (testableTypeSmbol != null)
            {
                ReportDiagnosticAt(objectCreationSyntax.Type.GetLocation(), methodSymbol.ContainingType.GetCompleteTypeName(),
                    testableTypeSmbol.GetCompleteTypeName(), context.ReportDiagnostic);
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

                        ReportDiagnosticAt(location, systemTypeName, testableTypeName, context.ReportDiagnostic);
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
                ReportDiagnosticAt(memberSymbol.Locations.First(), memberTypeSymbol.GetCompleteTypeName(),
                    testableTypeSymbol.GetCompleteTypeName(), reportDiagnostic);
            }
        }

        private void ReportDiagnosticAt([NotNull] Location location, [NotNull] string typeOrMemberName,
            [NotNull] string replacementName, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            reportDiagnostic(Diagnostic.Create(Rule, location, typeOrMemberName, replacementName));
        }
    }
}
