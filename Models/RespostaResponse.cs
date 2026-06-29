namespace InferenciaApi.Models;

public record RespostaResponse(
    string Resposta,
    int TokensEntrada,
    int TokensSaida,
    int TokensTotal,
    string Modelo,
    double SegundosResposta
);