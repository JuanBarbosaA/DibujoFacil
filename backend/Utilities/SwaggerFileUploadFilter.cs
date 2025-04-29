using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileUploadFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.HttpMethod == "POST" &&
            context.ApiDescription.ActionDescriptor.Parameters.Any(p => p.Name == "tutorialDto"))
        {
            var uploadSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["Title"] = new OpenApiSchema { Type = "string" },
                    ["Description"] = new OpenApiSchema { Type = "string" },
                    ["Difficulty"] = new OpenApiSchema { Type = "string" },
                    ["EstimatedDuration"] = new OpenApiSchema { Type = "integer" },
                    ["CategoryIds"] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "integer" }
                    },
                    ["Contents[0].File"] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    ["Contents[0].Order"] = new OpenApiSchema { Type = "integer" },
                    ["Contents[0].Title"] = new OpenApiSchema { Type = "string" },
                    ["Contents[0].Description"] = new OpenApiSchema { Type = "string" }
                }
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = uploadSchema
                    }
                }
            };
        }
    }
}