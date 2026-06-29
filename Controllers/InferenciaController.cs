using InferenciaApi.Models;
using InferenciaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace InferenciaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InferenciaController : ControllerBase
{
    private readonly IInferenciaService _inferenciaService;
    private readonly ILogger<InferenciaController> _logger;

    public InferenciaController(
        IInferenciaService inferenciaService,
        ILogger<InferenciaController> logger)
    {
        _inferenciaService = inferenciaService;
        _logger = logger;
    }

    [HttpPost("perguntar")]
    [ProducesResponseType(typeof(RespostaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Perguntar(
        [FromBody] PerguntaRequest request,
        CancellationToken ct)
    {
        try
        {
            var resposta = await _inferenciaService.PerguntarAsync(request.Pergunta, ct);
            return Ok(resposta);
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogWarning("Rate limit atingido");
            return StatusCode(429, new { erro = "Muitas requisições. Tente novamente em alguns segundos." });
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "Erro do provedor de IA. Status: {Status}", ex.Status);
            return StatusCode(502, new { erro = "Erro ao consultar o modelo de IA." });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Requisição cancelada pelo cliente");
            return StatusCode(499, new { erro = "Requisição cancelada." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na inferência");
            return StatusCode(500, new { erro = "Erro interno ao processar." });
        }
    }

    [HttpPost("perguntar/stream")]
    public async IAsyncEnumerable<string> PerguntarStream(
        [FromBody] PerguntaRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");

        await foreach (var pedaco in _inferenciaService.PerguntarStreamAsync(request.Pergunta, ct))
        {
            yield return pedaco;
        }
    }
}