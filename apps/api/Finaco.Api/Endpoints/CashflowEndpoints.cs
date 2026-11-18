using Finaco.Api.Application.Cashflow;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Finaco.Api.Endpoints;

public static class CashflowEndpoints
{
    public static IEndpointRouteBuilder MapCashflowEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/cashflow/accounts", ListAccounts).WithSummary("List accounts available for cash flow projections").Produces<IReadOnlyList<CashflowAccountResponse>>();
        endpoints.MapPut("/api/cashflow/accounts/{accountId:guid}/settings", SaveSettings).WithSummary("Save cash flow settings").Produces<CashflowAccountResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound);
        endpoints.MapPost("/api/cashflow/accounts/{accountId:guid}/projection", Projection).WithSummary("Calculate a read-only cash flow projection").Produces<CashflowResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound);
        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<CashflowAccountResponse>>> ListAccounts(CashflowService service, CancellationToken cancellationToken) => TypedResults.Ok(await service.ListAccountsAsync(cancellationToken));

    private static async Task<Results<Ok<CashflowAccountResponse>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> SaveSettings(Guid accountId, CashflowSettingsRequest request, CashflowService service, CancellationToken cancellationToken)
    {
        if (CashflowValidation.Settings(request) is { } settingsError)
            return TypedResults.BadRequest(new ProblemDetails { Title = "Geçersiz nakit akışı ayarı", Detail = settingsError });
        var result = await service.SaveSettingsAsync(accountId, request, cancellationToken);
        return result is null ? TypedResults.NotFound(new ProblemDetails { Title = "Hesap bulunamadı" }) : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CashflowResponse>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> Projection(Guid accountId, CashflowScenarioRequest? scenario, CashflowService service, string? asOfDate, CancellationToken cancellationToken)
    {
        if (scenario is { UnexpectedExpenseAmount: null, UnexpectedExpenseDate: null, DelayedIncomeDays: 0, AdditionalIncomeAmount: null, AdditionalIncomeDate: null }) scenario = null;
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        if (asOfDate is not null && (!DateOnly.TryParseExact(asOfDate, "yyyy-MM-dd", out date)))
            return TypedResults.BadRequest(new ProblemDetails { Title = "Geçersiz veri kesim tarihi", Detail = "asOfDate YYYY-MM-DD biçiminde olmalıdır." });
        if (scenario is not null && CashflowValidation.Scenario(scenario) is { } scenarioError)
            return TypedResults.BadRequest(new ProblemDetails { Title = "Geçersiz senaryo", Detail = scenarioError });
        var (response, error) = await service.GetAsync(accountId, date, scenario, cancellationToken);
        if (response is null) return TypedResults.NotFound(new ProblemDetails { Title = "Hesap bulunamadı", Detail = error });
        if (error is not null) return TypedResults.BadRequest(new ProblemDetails { Title = "Eksik nakit akışı bilgisi", Detail = error });
        return TypedResults.Ok(response);
    }
}
