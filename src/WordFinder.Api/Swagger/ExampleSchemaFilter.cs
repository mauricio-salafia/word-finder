using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WordFinder.Application.DTOs;

namespace WordFinder.Api.Swagger;

/// <summary>
/// Schema filter to add example values to DTOs in Swagger documentation
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(WordSearchRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["matrix"] = new OpenApiArray
                {
                    new OpenApiString("abcdc"),
                    new OpenApiString("fgwio"),
                    new OpenApiString("chill"),
                    new OpenApiString("pqnsd"),
                    new OpenApiString("uvdxy")
                },
                ["wordStream"] = new OpenApiArray
                {
                    new OpenApiString("chill"),
                    new OpenApiString("cold"),
                    new OpenApiString("wind"),
                    new OpenApiString("snow")
                }
            };
        }
        else if (context.Type == typeof(WordSearchResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["foundWords"] = new OpenApiArray
                {
                    new OpenApiString("chill"),
                    new OpenApiString("cold"),
                    new OpenApiString("wind")
                },
                ["totalWordsSearched"] = new OpenApiInteger(4),
                ["processingTimeMs"] = new OpenApiInteger(15)
            };
        }
    }
}
