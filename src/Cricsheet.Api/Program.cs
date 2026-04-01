using Cricsheet.Api.Contracts;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Configuration;
using Cricsheet.Api.Infrastructure.Cosmos;
using Cricsheet.Api.Infrastructure.Providers;
using Cricsheet.Api.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<BrowseFilterRequestValidator>();
builder.Services.AddSingleton<ICosmosClientFactory, ManagedIdentityCosmosClientFactory>();
builder.Services.AddScoped<IMatchBrowseProvider, CosmosMatchBrowseProvider>();
builder.Services.AddScoped<IMatchDetailProvider, CosmosMatchDetailProvider>();

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

app.MapGet("/", () => "Hello World!");

app.Run();

// Expose Program for WebApplicationFactory in integration/contract tests
// CA1515 suppressed: public partial class is required for WebApplicationFactory access in test assemblies.
#pragma warning disable CA1515
public partial class Program { }
#pragma warning restore CA1515
