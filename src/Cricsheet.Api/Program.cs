using Cricsheet.Api.Contracts;
using Cricsheet.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

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
