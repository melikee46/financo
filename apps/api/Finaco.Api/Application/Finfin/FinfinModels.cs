namespace Finaco.Api.Application.Finfin;

public sealed record FinfinChatRequest(Guid? AccountId, string Question);

public sealed record FinfinChatResponse(
    string Mode,
    bool IsDemo,
    string Answer,
    IReadOnlyList<string> Context,
    IReadOnlyList<string> MissingData,
    string RelatedPath,
    IReadOnlyList<string> RequiredEnvironmentVariables);

public static class FinfinDemoResponder
{
    public static FinfinChatResponse Create(
        string? projectionError,
        string? method,
        IReadOnlyList<string> warnings,
        IReadOnlyList<string> missingData)
    {
        var context = new List<string>
        {
            "Bu yanıt demo modundadır; gerçek bir LLM bağlantısı kullanılmıyor."
        };
        if (method is not null) context.Add($"Financo hesaplama yöntemi: {method}");
        context.AddRange(warnings);
        var answer = projectionError is not null
            ? $"Finfin demo modunda. Bu soruyu açıklamak için önce Financo’daki eksik bilgileri tamamlamak gerekiyor: {projectionError}"
            : "Finfin demo modunda. Gerçek LLM bağlantısı kurulmadığı için serbest metin yanıtı üretmiyorum; aşağıda Financo’nun doğrulanmış hesaplama bağlamını gösteriyorum.";
        return new("demo", true, answer, context, missingData, "Nakit Akışı", ["FINFIN_LLM_API_KEY", "FINFIN_LLM_BASE_URL", "FINFIN_LLM_MODEL"]);
    }
}
