using backend.Dtos;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace backend.Utilities
{
    public class SwaggerRatingExampleFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(RatingDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["score"] = new OpenApiInteger(5)
                };
            }
        }
    }
}
