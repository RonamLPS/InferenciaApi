using InferenciaApi.Models;

namespace InferenciaApi.Services;

public interface IInferenciaService
{
    Task<RespostaResponse> PerguntarAsync(string pergunta, CancellationToken ct);
    IAsyncEnumerable<string> PerguntarStreamAsync(string pergunta, CancellationToken ct);
}