namespace WebApi.Services
{
    public interface IMeterReadingService
    {
        Task<(int successes, int failures)> ProcessMeterReadings(IFormFile csvFile);
    }
}