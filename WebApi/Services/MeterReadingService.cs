using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private IMeterReadingMapper _meterReadingMapper;
        private string _connectionString;
        EnsekDatabaseContext _dbContext;
        private ILogger<MeterReadingService> _logger;

        private Regex _meterReadingValueRegex = new Regex(@"^\d{1,5}$", RegexOptions.Compiled);

        public MeterReadingService(IMeterReadingMapper meterReadingMapper, EnsekDatabaseContext dbContext, ILogger<MeterReadingService> logger)
        {
            _meterReadingMapper = meterReadingMapper;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(int successes, int failures)> ProcessMeterReadings(IFormFile csvFile)
        {
            var meterReadings = await _meterReadingMapper.Map(csvFile);

            if (!meterReadings.Any())
                throw new BadHttpRequestException($"File {csvFile.FileName} contains no meter readings");

            using var sqlConnection = new SqlConnection(_connectionString);

            var successes = 0;
            foreach (var reading in meterReadings)
            {
                var isMatch = _meterReadingValueRegex.IsMatch(reading.MeterReadValue);

                if (!isMatch)
                    continue;

                var customerAccount = _dbContext.CustomerAccounts.AsNoTracking().FirstOrDefault(x => x.AccountId == reading.AccountId);

                if (customerAccount is null)
                    continue;

                var existingMeterReading = _dbContext.MeterReadings.FirstOrDefault(x => x.AccountId == reading.AccountId);

                if (existingMeterReading is null)
                {
                    var newMeterReading = new MeterReading { AccountId = reading.AccountId, MeterReadingDateTime = reading.MeterReadingDateTime, MeterReadValue = reading.MeterReadValue };
                    _dbContext.MeterReadings.Add(newMeterReading);
                    await _dbContext.SaveChangesAsync();
                    successes++;
                }
                else if (reading.MeterReadingDateTime > existingMeterReading.MeterReadingDateTime)
                {
                    existingMeterReading.MeterReadingDateTime = reading.MeterReadingDateTime;
                    existingMeterReading.MeterReadValue = reading.MeterReadValue;

                    await _dbContext.SaveChangesAsync();
                    successes++;
                }
            }

            return (successes, meterReadings.Count - successes);
        }
    }
}