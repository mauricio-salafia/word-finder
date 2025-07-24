using Serilog;
using WordFinder.Api.Swagger;
using WordFinder.Application.Services;
using WordFinder.Application.Validators;
using WordFinder.Domain.Factories;
using WordFinder.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WordFinder API",
        Version = "v1",
        Description = "API for finding words in character matrices with high performance search capabilities"
    });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Add schema filter for example values
    c.SchemaFilter<ExampleSchemaFilter>();
});

// Register application services
builder.Services.AddScoped<IWordFinderApplicationService, WordFinderApplicationService>();
builder.Services.AddScoped<IWordSearchAlgorithmService, WordSearchSimpleAlgorithmService>();
builder.Services.AddScoped<IWordFinderFactory, WordFinderFactory>();
builder.Services.AddScoped<IWordSearchRequestValidator, WordSearchRequestValidator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WordFinder API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at app root
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseSerilogRequestLogging();

app.MapControllers();

try
{
    Log.Information("Starting WordFinder API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class public for testing
public partial class Program { }
