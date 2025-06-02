using CsvHelper;
using System.Globalization;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class MeterReadingMapper : IMeterReadingMapper
    {
        public async Task<List<MeterReading>> Map(IFormFile formFile)
        {
            if (formFile is null)
                throw new BadHttpRequestException("Input formFile is null");

            var hasCsvExtension = Path.GetExtension(formFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase);

            if (!hasCsvExtension)
                throw new BadHttpRequestException($"File {formFile.FileName} does not have a '.csv' extension");

            //Commented-out, as it does not work with the given file
            //var hasCsvContent = file.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase);

            //if (!hasCsvContent)
            //throw new BadHttpRequestException("The file does not contain csv content");

            if (formFile.Length == 0L)
                throw new BadHttpRequestException($"File {formFile.FileName} is empty");

            using var fileReader = new StreamReader(formFile.OpenReadStream());
            using var csvReader = new CsvReader(fileReader, CultureInfo.InvariantCulture);

            await csvReader.ReadAsync();

            csvReader.ReadHeader();
            csvReader.ValidateHeader<MeterReading>();

            return csvReader
                .GetRecords<MeterReading>()
                .ToList();
        }
    }
}