using InventarioPaldaca.Models.Inventario;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Utilidades
{
    public class LimpiezaReportesService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public LimpiezaReportesService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<InventarioPaldacaContext>();
                    var fechaLimite = DateTime.Now.AddDays(-60);

                    var reportesFinalizadosAntiguos = db.Reportes
                        .Where(r => r.FechaGeneracion < fechaLimite && r.Estado == "Finalizado")
                        .ToList();

                    if (reportesFinalizadosAntiguos.Any())
                    {
                        db.Reportes.RemoveRange(reportesFinalizadosAntiguos);
                        await db.SaveChangesAsync();
                    }
                }

                // Espera 24 horas antes de volver a ejecutar
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

