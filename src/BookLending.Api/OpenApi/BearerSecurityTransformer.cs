using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BookLending.Api.OpenApi;

internal sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http, // the standard "Authorization: <scheme> <token>" header
            Scheme = "bearer",              // Swagger adds the "Bearer " prefix: paste the raw token
            BearerFormat = "JWT",
            Description = "Call POST /login, copy the token, and paste it here (without 'Bearer ')."
        };
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuth = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
        if (!requiresAuth) return Task.CompletedTask;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, context.Document)] = []
        });

        // Document the two auth failures, so they show up in Swagger's "Responses" list.
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, expired or invalid token" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Valid token, but missing the required role" });
        return Task.CompletedTask;
    }
}
