using System.ClientModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using InferenciaApi.Configuration;
using InferenciaApi.Models;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace InferenciaApi.Services;

public class InferenciaService : IInferenciaService
{
    private readonly ChatClient _chatClient;
    private readonly InferenciaOptions _options;
    private readonly ILogger<InferenciaService> _logger;

    public InferenciaService(
        IOptions<InferenciaOptions> options,
        ILogger<InferenciaService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(_options.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(_options.Endpoint)
            }
        );

        _chatClient = openAIClient.GetChatClient(_options.ModelName);
    }

    public async Task<RespostaResponse> PerguntarAsync(
        string pergunta,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        var mensagens = new List<ChatMessage>
        {
            new SystemChatMessage(_options.SystemPrompt),
            new UserChatMessage(pergunta)
        };

        var opcoes = new ChatCompletionOptions
        {
            Temperature = _options.Temperature,
            MaxOutputTokenCount = _options.MaxTokens
        };

        _logger.LogInformation("Iniciando inferência. Modelo: {Modelo}", _options.ModelName);

        ChatCompletion completion = await _chatClient.CompleteChatAsync(mensagens, opcoes, ct);

        stopwatch.Stop();

        var texto = completion.Content[0].Text;

        _logger.LogInformation(
            "Inferência concluída. Tokens: {Total}. Tempo: {Tempo}s",
            completion.Usage.TotalTokenCount,
            stopwatch.Elapsed.TotalSeconds
        );

        return new RespostaResponse(
            Resposta: texto,
            TokensEntrada: completion.Usage.InputTokenCount,
            TokensSaida: completion.Usage.OutputTokenCount,
            TokensTotal: completion.Usage.TotalTokenCount,
            Modelo: _options.ModelName,
            SegundosResposta: Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
        );
    }

    public async IAsyncEnumerable<string> PerguntarStreamAsync(
        string pergunta,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var mensagens = new List<ChatMessage>
        {
            new SystemChatMessage(_options.SystemPrompt),
            new UserChatMessage(pergunta)
        };

        var opcoes = new ChatCompletionOptions
        {
            Temperature = _options.Temperature,
            MaxOutputTokenCount = _options.MaxTokens
        };

        await foreach (var update in _chatClient.CompleteChatStreamingAsync(mensagens, opcoes, ct))
        {
            if (update.ContentUpdate.Count > 0)
            {
                yield return update.ContentUpdate[0].Text;
            }
        }
    }
}