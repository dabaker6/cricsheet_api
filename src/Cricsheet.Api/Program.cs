using Cricsheet.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddOptions<CosmosSettings>()
	.Bind(builder.Configuration.GetSection(CosmosSettings.SectionName))
	.ValidateDataAnnotations()
	.ValidateOnStart();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
