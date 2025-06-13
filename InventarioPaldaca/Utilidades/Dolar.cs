using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
namespace InventarioPaldaca.Utilidades
{
    public class Dolar
    {
        private readonly HttpClient _httpClient;

        public Dolar(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> ObtenerPrecioDolarAsync()
        {
            try
            {
                var url = "https://ve.dolarapi.com/v1/dolares/oficial";
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (jsonDoc.RootElement.TryGetProperty("promedio", out var promedioElement))
                {
                    return promedioElement.GetDecimal();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

