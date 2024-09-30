namespace InventarioPaldaca.Models
{
    public class ErrorViewModel
    {
        // Identificador de la solicitud en la que ocurri� el error
        public string? RequestId { get; set; }

        // Mensaje de error detallado para mostrar al usuario o en el registro de errores
        public string? Message { get; set; }

        // Mensaje gen�rico de error para mostrar al usuario en caso de no querer mostrar detalles espec�ficos
        public string? UserFriendlyMessage { get; set; } = "Ha ocurrido un error inesperado. Por favor, int�ntelo nuevamente o contacte al soporte.";

        // Detalles t�cnicos adicionales del error (por ejemplo, pila de llamadas)
        public string? ExceptionDetails { get; set; }

        // Indica si debe mostrarse el RequestId
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Indica si se debe mostrar un mensaje espec�fico o solo el gen�rico
        public bool ShowDetailedMessage => !string.IsNullOrEmpty(Message);
    }
}
