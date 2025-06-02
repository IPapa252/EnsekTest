using CsvHelper.Configuration.Attributes;

namespace WebApi.Entities;

public partial class MeterReading
{
    public int AccountId { get; set; }

    [Format("dd/MM/yyyy HH:mm")]
    public DateTime MeterReadingDateTime { get; set; }

    public string MeterReadValue { get; set; }
}
