namespace InferenciaApi.Configuration;

public class InferenciaOptions
{
    public const string SectionName = "Inferencia";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = "Você é um assistente útil e responde em Pt-BR.";
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 800;
}