namespace PixApiRest.Exceptions;

/// <summary>
/// Resposta de erro da API
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Timestamp do erro
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Código de status HTTP
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Mensagem de erro
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem detalhada
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Caminho da requisição
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Lista de erros de validação
    /// </summary>
    public List<FieldError>? FieldErrors { get; set; }

    public class FieldError
    {
        /// <summary>
        /// Nome do campo
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro do campo
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
