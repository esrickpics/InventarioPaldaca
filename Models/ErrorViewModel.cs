namespace InventarioPaldaca.Models
{
    public class ErrorViewModel
    {
        // Identificador de la solicitud en la que ocurrió el error
        public string? RequestId { get; set; }

        // Mensaje de error detallado para mostrar al usuario o en el registro de errores
        public string? Message { get; set; }

        // Mensaje genérico de error para mostrar al usuario en caso de no querer mostrar detalles específicos
        public string? UserFriendlyMessage { get; set; } = "Ha ocurrido un error inesperado. Por favor, inténtelo nuevamente o contacte al soporte.";

        // Detalles técnicos adicionales del error (por ejemplo, pila de llamadas)
        public string? ExceptionDetails { get; set; }

        // Indica si debe mostrarse el RequestId
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Indica si se debe mostrar un mensaje específico o solo el genérico
        public bool ShowDetailedMessage => !string.IsNullOrEmpty(Message);
    }
}
