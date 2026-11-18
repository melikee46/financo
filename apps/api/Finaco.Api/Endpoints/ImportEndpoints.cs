using Finaco.Api.Application.Imports;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Finaco.Api.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/imports/preview", Preview)
            .WithName("PreviewImport")
            .WithSummary("Preview and validate a CSV statement")
            .WithDescription("Parses supported Turkish date and amount formats without creating transactions.")
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<ImportPreviewResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        endpoints.MapPost("/api/imports/{id:guid}/confirm", Confirm)
            .WithName("ConfirmImport")
            .WithSummary("Confirm valid rows from an import")
            .WithDescription("Creates transactions only for rows without validation errors.")
            .Produces<ImportConfirmResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<Results<Ok<ImportPreviewResponse>, BadRequest<ProblemDetails>>> Preview(
        IFormFile? file, ImportPreviewService service, CancellationToken cancellationToken)
    {
        if (file is null) return TypedResults.BadRequest(new ProblemDetails { Title = "Dosya gerekli", Detail = "Bir CSV dosyası yükleyin." });
        try
        {
            return TypedResults.Ok(await service.PreviewAsync(file, cancellationToken));
        }
        catch (CsvImportException exception)
        {
            return TypedResults.BadRequest(new ProblemDetails { Title = "CSV doğrulama hatası", Detail = exception.Message });
        }
    }

    private static async Task<Results<Ok<ImportConfirmResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> Confirm(
        Guid id, ImportPreviewService service, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ConfirmAsync(id, cancellationToken);
            return result is null
                ? TypedResults.NotFound(new ProblemDetails { Title = "İçe aktarma bulunamadı", Detail = $"{id} kimlikli içe aktarma bulunamadı." })
                : TypedResults.Ok(result);
        }
        catch (CsvImportException exception)
        {
            return TypedResults.BadRequest(new ProblemDetails { Title = "İçe aktarma onaylanamadı", Detail = exception.Message });
        }
    }
}
