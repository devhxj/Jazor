


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Jero.Plugin.SaveLife.Service;
namespace Jero.Plugin.SaveLife;

[AppStartup(120)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAGUIServer();
        services.AddHttpContextAccessor();
        services.TryAddSingleton<SaveLifeMafChatClient>();
        services.TryAddSingleton<SaveLifeMafRunMiddleware>();
        services.TryAddSingleton<SaveLifeMafContextProvider>();
        services.TryAddSingleton<MedicalInformationTool>();
        services.TryAddSingleton<ClinicalTrialTool>();
        services.TryAddSingleton<HealthProfileProposalTool>();
        services.TryAddSingleton<RoutineGuidanceTool>();
        services.AddScoped<SaveLifeMafRequestContext>();
        services.AddScoped<SaveLifeMafToolService>();

        services.AddAIAgent(
                SaveLifeMafAgent.Name,
                (serviceProvider, name) => SaveLifeMafAgent.Create(serviceProvider, name))
            .WithSessionStore(
                (serviceProvider, _) => new SaveLifeMafSessionStore(
                    serviceProvider.GetRequiredService<IHttpContextAccessor>()),
                withIsolation: false);

        services.AddHttpClient(SaveLifeConst.ClinicalTrialHttpClientName, client =>
        {
            client.BaseAddress = new Uri(SaveLifeConst.ClinicalTrialBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36 Edg/149.0.0.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        });
        services.AddHttpClient(SaveLifeConst.MaterialEmbeddingHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddHttpClient(SaveLifeConst.AgentChatHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(300);
        });
        services.AddHttpClient(SaveLifeConst.MiniAppHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(8);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<SaveLifeTraceIdMiddleware>();

        var db = app.ApplicationServices.GetRequiredService<ISqlSugarClient>()
            .AsTenant()
            .GetConnectionScope(SqlSugarConst.MainConfigId);
        var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
        SaveLifeSqlitePinnedColumnMigration.Apply(db);
        SaveLifeAccountDeletionMigration.EnsureSchema(db);
        SaveLifeEnrollmentAuditScopeMigration.EnsureSchema(db);
        SaveLifePatientProfileMigration.EnsureSchema(db);
        var conversationMigrationReport = SaveLifeAgentConversationMigration.Apply(db);
        logger.LogInformation(
            "SaveLife active conversation migration: tenantConflicts={TenantConflictCount}, accountConflicts={AccountConflictCount}, maxDuplicates={MaxDuplicateCount}",
            conversationMigrationReport.TenantConflictCount,
            conversationMigrationReport.AccountConflictCount,
            conversationMigrationReport.MaxDuplicateCount);
        SaveLifeAgentTurnMigration.Apply(db);
        SaveLifeAgentCommandMigration.Apply(db);
        SaveLifeMaterialUploadMigration.Apply(db);
        SaveLifeDataIntegrityMigration.Apply(db);
        var patientMigrationReport = SaveLifePatientProfileMigration.Apply(db);
        logger.LogInformation(
            "SaveLife patient data migration: enrollments={EnrollmentCount}, materials={MaterialCount}, privateIndexes={PrivateIndexCount}, conflicts={ConflictCount}",
            patientMigrationReport.EnrollmentBackfilledCount,
            patientMigrationReport.MaterialBackfilledCount,
            patientMigrationReport.PrivateIndexBackfilledCount,
            patientMigrationReport.EnrollmentConflicts.Count);
        var report = SaveLifeClinicalTrialDataMigration.Apply(db);
        logger.LogInformation(
            "SaveLife project data quality: total={TotalCount}, changed={ChangedCount}, indicationBackfilled={IndicationBackfilledCount}, diseaseCorrected={DiseaseAreaCorrectedCount}, conflicts={ConflictCount}, ambiguous={AmbiguousCount}, unresolved={UnresolvedCount}",
            report.TotalCount,
            report.ChangedCount,
            report.IndicationBackfilledCount,
            report.DiseaseAreaCorrectedCount,
            report.ConflictCount,
            report.AmbiguousCount,
            report.UnresolvedCount);
    }
}
