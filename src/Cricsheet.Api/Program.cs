using System.Globalization;
using System.Text.Json;
using Cricsheet.Api.Contracts;
using Cricsheet.Api.Endpoints;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Application.Services;
using Cricsheet.Api.Configuration;
using Cricsheet.Api.Infrastructure.Cosmos;
using Cricsheet.Api.Infrastructure.Providers;
using Cricsheet.Api.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
	.ReadFrom.Configuration(context.Configuration)
	.ReadFrom.Services(services)
	.Enrich.FromLogContext()
	.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.AddValidatorsFromAssemblyContaining<BrowseFilterRequestValidator>(includeInternalTypes: true);
builder.Services.AddSingleton<ICosmosClientFactory, ManagedIdentityCosmosClientFactory>();
builder.Services.AddScoped<IMatchBrowseProvider, CosmosMatchBrowseProvider>();
builder.Services.AddScoped<IMatchDetailProvider, CosmosMatchDetailProvider>();
builder.Services.AddScoped<ISummaryMapper, SummaryMapper>();
builder.Services.AddScoped<IBrowseService, BrowseService>();
builder.Services.AddScoped<IDetailService, DetailService>();
builder.Services.AddScoped<IErrorTranslator, ErrorTranslator>();
builder.Services
	.AddHealthChecks()
	.AddCheck(
		"self",
		() => HealthCheckResult.Healthy("The API process is running."),
		tags: ["live", "ready"])
	.AddCheck(
		"cosmos-configuration",
		() => HasCosmosConfiguration(builder.Configuration)
			? HealthCheckResult.Healthy("Cosmos configuration is available.")
			: HealthCheckResult.Unhealthy("Cosmos configuration is incomplete."),
		tags: ["ready"]);

builder.Services
	.AddOptions<CosmosSettings>()
	.Bind(builder.Configuration.GetSection(CosmosSettings.SectionName))
	.ValidateDataAnnotations()
	.ValidateOnStart();

var app = builder.Build();

app.Use(async (context, next) =>
{
	var incomingCorrelationId = context.Request.Headers[CorrelationConstants.HeaderName].FirstOrDefault();
	var correlationId = string.IsNullOrWhiteSpace(incomingCorrelationId)
		? context.TraceIdentifier
		: incomingCorrelationId.Trim();

	context.TraceIdentifier = correlationId;
	context.Items[CorrelationConstants.ItemKey] = correlationId;

	context.Response.OnStarting(() =>
	{
		context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;
		return Task.CompletedTask;
	});

	await next().ConfigureAwait(false);
});

app.UseSerilogRequestLogging(options =>
{
	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("CorrelationId", httpContext.GetCorrelationId());
		diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
		diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? "/");
		diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
	};
});

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/health", new HealthCheckOptions
{
	Predicate = registration => registration.Tags.Contains("live"),
	ResponseWriter = WriteHealthResponseAsync
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
	Predicate = registration => registration.Tags.Contains("ready"),
	ResponseWriter = WriteHealthResponseAsync
});
app.MapBrowseEndpoints();
app.MapDetailEndpoints();

app.Run();

static bool HasCosmosConfiguration(IConfiguration configuration)
{
	var cosmosSection = configuration.GetSection(CosmosSettings.SectionName);
	return !string.IsNullOrWhiteSpace(cosmosSection[nameof(CosmosSettings.AccountEndpoint)])
		&& !string.IsNullOrWhiteSpace(cosmosSection[nameof(CosmosSettings.DatabaseName)])
		&& !string.IsNullOrWhiteSpace(cosmosSection[nameof(CosmosSettings.ContainerName)]);
}

static async Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
	context.Response.ContentType = "application/json";

	var payload = new
	{
		status = report.Status.ToString(),
		checks = report.Entries.Select(entry => new
		{
			name = entry.Key,
			status = entry.Value.Status.ToString(),
			description = entry.Value.Description
		})
	};

	await context.Response.WriteAsync(JsonSerializer.Serialize(payload)).ConfigureAwait(false);
}

// Expose Program for WebApplicationFactory in integration/contract tests
// CA1515 suppressed: public partial class is required for WebApplicationFactory access in test assemblies.
#pragma warning disable CA1515
public partial class Program { }
#pragma warning restore CA1515
