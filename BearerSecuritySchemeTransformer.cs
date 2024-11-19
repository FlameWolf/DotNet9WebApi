using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace DotNet9WebApi;

public class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
	public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		if
		(
			(await authenticationSchemeProvider.GetAllSchemesAsync())
			.Any(authScheme => authScheme.Name == "Bearer")
		)
		{
			document.Components ??= new();
			document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
			{
				["Bearer"] = new()
				{
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					In = ParameterLocation.Header,
					BearerFormat = "JWT"
				}
			};
			foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
			{
				operation.Value.Security.Add(new()
				{
					[
						new()
						{
							Reference = new()
							{
								Id = "Bearer",
								Type = ReferenceType.SecurityScheme
							}
						}
					] = Array.Empty<string>()
				});
			}
		}
	}
}