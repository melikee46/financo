using Finaco.Api.Application.Finfin;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Finaco.Api.Endpoints;

public static class FinfinEndpoints
{
    public static IEndpointRouteBuilder MapFinfinEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/finfin/chat", Chat)
            .WithSummary("Ask Finfin to explain Financo data")
            .WithDescription("Returns a transparent demo response until a backend-only LLM provider is configured.")
            .Produces<FinfinChatResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        return endpoints;
    }

    private static async Task<Results<Ok<FinfinChatResponse>, BadRequest<ProblemDetails>>> Chat(FinfinChatRequest request, FinfinService service, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return TypedResults.BadRequest(new ProblemDetails { Title = "Soru gerekli", Detail = "Finfin’e göndermek için bir soru yazın." });
        return TypedResults.Ok(await service.ChatAsync(request, cancellationToken));
    }
}
