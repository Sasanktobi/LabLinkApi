using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Backend.Configuration
{
    // Adds the JWT bearer scheme to the OpenAPI document so the API explorer UI can send tokens.
    public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"]=new OpenApiSecurityScheme
            {
                Type=SecuritySchemeType.Http,
                Scheme="bearer",
                BearerFormat="JWT",
                In=ParameterLocation.Header,
                Description="Paste the token returned by POST /api/auth/login."
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)]=new List<string>()
            });

            return Task.CompletedTask;
        }
    }
}
