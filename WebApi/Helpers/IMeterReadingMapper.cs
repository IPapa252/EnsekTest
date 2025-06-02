using WebApi.Entities;

namespace WebApi.Helpers
{
    public interface IMeterReadingMapper
    {
        Task<List<MeterReading>> Map(IFormFile formFile);
    }
}