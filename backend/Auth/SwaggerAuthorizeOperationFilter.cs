using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameTracker.Api.Auth
{
    /// <summary>
    /// Yalnızca [Authorize] ile işaretli uçlarda Swagger'da Bearer zorunluluğu gösterir (login/register anonim kalır).
    /// </summary>
    public class SwaggerAuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var meta = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            if (!meta.Any(m => m is AuthorizeAttribute))
                return;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                },
            };
        }
    }
}
