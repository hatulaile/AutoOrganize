using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ViewModelViewModelRegistration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ViewModelRegistrationDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string REGISTRATION_ATTRIBUTE_NAME =
        "ViewModelRegistrationGenerator.ViewModelRegistrationAttribute";

    private const string RESULT_VIEW_MODEL_INTERFACE_NAME = "AutoOrganize.ViewModels.Abstractions.IResultViewModel`1";

    private const int TRANSIENT_LIFETIME = 1;

    private const string WINDOW_SERVICE_INTERFACE_NAMESPACE = "AutoOrganize.Services.WindowManagers.IWindowService";

    private const string SHOW_DIALOG_METHOD_NAME = "ShowDialog";

    private static readonly DiagnosticDescriptor ResultViewModelLifetimeRule = new(
        "VMRG001",
        "带有 Result ViewModel 必须为瞬态",
        "实现 IResultViewModel 的 ViewModel '{0}' 必须注册为 Transient",
        "ViewModelRegistration",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor NonTransientDialogRule = new(
        "VMRG002",
        "模态窗口 ViewModel 必须为瞬态",
        "模态窗口 ViewModel '{0}' 注册为 {1}，必须注册为 Transient",
        "ViewModelRegistration",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [ResultViewModelLifetimeRule, NonTransientDialogRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(csct =>
        {
            INamedTypeSymbol? resultInterfaceSymbol =
                csct.Compilation.GetTypeByMetadataName(RESULT_VIEW_MODEL_INTERFACE_NAME);
            INamedTypeSymbol? attributeSymbol =
                csct.Compilation.GetTypeByMetadataName(REGISTRATION_ATTRIBUTE_NAME);
            if (resultInterfaceSymbol is null || attributeSymbol is null) return;

            csct.RegisterSymbolAction(ctx =>
            {
                if (ctx.Symbol is not INamedTypeSymbol namedTypeSymbol) return;

                AttributeData? attribute =
                    namedTypeSymbol.GetAttributes().FirstOrDefault(x =>
                        SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeSymbol));
                if (attribute is null) return;

                int lifetime = GetLifetime(attribute);
                if (lifetime == TRANSIENT_LIFETIME) return;
                if (!namedTypeSymbol.AllInterfaces.Any(x =>
                        SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, resultInterfaceSymbol)))
                {
                    return;
                }

                SyntaxReference? reference = attribute.ApplicationSyntaxReference;
                if (reference is not null)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        ResultViewModelLifetimeRule,
                        Location.Create(reference.SyntaxTree, reference.Span),
                        namedTypeSymbol));
                }
            }, SymbolKind.NamedType);
        });

        context.RegisterCompilationStartAction(csct =>
        {
            INamedTypeSymbol? resultInterfaceSymbol =
                csct.Compilation.GetTypeByMetadataName(RESULT_VIEW_MODEL_INTERFACE_NAME);
            INamedTypeSymbol? attributeSymbol =
                csct.Compilation.GetTypeByMetadataName(REGISTRATION_ATTRIBUTE_NAME);
            INamedTypeSymbol? serviceSymbol =
                csct.Compilation.GetTypeByMetadataName(WINDOW_SERVICE_INTERFACE_NAMESPACE);
            if (resultInterfaceSymbol is null || attributeSymbol is null || serviceSymbol is null)
                return;

            csct.RegisterOperationAction(ctx =>
            {
                if (ctx.Operation is not IInvocationOperation invocation) return;
                if (!invocation.TargetMethod.Name.Equals(SHOW_DIALOG_METHOD_NAME, StringComparison.Ordinal) ||
                    !SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, serviceSymbol) &&
                    !invocation.TargetMethod.ContainingType.AllInterfaces.Any(x =>
                        SymbolEqualityComparer.Default.Equals(x, serviceSymbol)))
                    return;

                if (invocation.TargetMethod.TypeArguments.Length == 0) return;
                ITypeSymbol typeSymbol = invocation.TargetMethod.TypeArguments.First();
                AttributeData? attributeData = typeSymbol.GetAttributes().FirstOrDefault(x =>
                    SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeSymbol));
                if (attributeData is null) return;
                int lifetime = GetLifetime(attributeData);
                if (lifetime == TRANSIENT_LIFETIME) return;
                ctx.ReportDiagnostic(Diagnostic.Create(
                    NonTransientDialogRule,
                    invocation.Syntax.GetLocation(),
                    typeSymbol,
                    GetLifetimeName(lifetime)));
            }, OperationKind.Invocation);
        });
    }

    private static int GetLifetime(AttributeData registration) =>
        registration.ConstructorArguments.Length > 0 &&
        registration.ConstructorArguments[0].Value is int lifetime ? lifetime : TRANSIENT_LIFETIME;

    private static string GetLifetimeName(int lifetime) => lifetime switch
    {
        0 => "None",
        2 => "Singleton",
        3 => "Scoped",
        _ => "Unknown"
    };
}