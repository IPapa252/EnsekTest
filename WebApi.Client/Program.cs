using System.Text.Json;

namespace WebApi.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using HttpClient httpClient = new HttpClient();

            //Wait for the web api
            await Task.Delay(4000);
            
            using var multipartContent = new MultipartFormDataContent();

            var filePath = @".\Meter_Reading.csv";
            using var fileStream = new FileStream(filePath, FileMode.Open);
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");

            multipartContent.Add(streamContent, "csvFile", Path.GetFileName(filePath));

            var response = await httpClient.PostAsync(new Uri("http://localhost:5298/EnergyReader/meter-reading-uploads"), multipartContent);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var responseResults =  JsonSerializer.Deserialize<ResponseResults>(responseBody);
        }
    }

    public record ResponseResults(int successes, int failures);
}