using System.ComponentModel.DataAnnotations;

namespace InferenciaApi.Models;

public class PerguntaRequest
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Pergunta deve ter entre 1 e 2000 caracteres.")]
    public string Pergunta { get; set; } = string.Empty;
}