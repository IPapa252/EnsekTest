using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    [TestFixture]
    public class MeterReadingServiceTests
    {
        private MeterReadingService _meterReadingService;
        private Mock<IMeterReadingMapper> _meterReadingMapperMock;
        private Mock<ILogger<MeterReadingService>> _meterReadingServiceMock;

        private Mock<IFormFile> _formFileMock;
        private const string TestFileName = "TestFile.csv";

        private DbContextOptions<EnsekDatabaseContext> _dbContextOptions;
        private EnsekDatabaseContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            //Arrange
            _meterReadingMapperMock = new Mock<IMeterReadingMapper>();
            _meterReadingServiceMock = new Mock<ILogger<MeterReadingService>>();

            _formFileMock = new Mock<IFormFile>();
            _formFileMock.Setup(x => x.FileName).Returns(TestFileName);

            _dbContextOptions = new DbContextOptionsBuilder<EnsekDatabaseContext>()
                .UseInMemoryDatabase("EnsekDatabaseTest")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new EnsekDatabaseContext(_dbContextOptions);
            
            _dbContext.Database.EnsureDeleted();

            _meterReadingService = new MeterReadingService(_meterReadingMapperMock.Object, _dbContext, _meterReadingServiceMock.Object);
        }

        [Test]
        public void ProcessMeterReadings_ShouldThrowABadHttpRequestException_WhenTheMapperReturnsAnEmptyList()
        {
            //Arrange
            _meterReadingMapperMock.Setup(x => x.Map(_formFileMock.Object)).ReturnsAsync(new List<MeterReading> { });

            //Act
            var exception = Assert.ThrowsAsync<BadHttpRequestException>(() => _meterReadingService.ProcessMeterReadings(_formFileMock.Object));

            //Assert
            Assert.That(exception.Message, Is.EqualTo($"File {TestFileName} contains no meter readings"));
        }

        [Test]
        public async Task ProcessMeterReadings_ShouldReturnTheExpectedMeterReadings()
        {
            //Arrange
            var existingMeterReading1 = new MeterReading { AccountId = 4, MeterReadingDateTime = DateTime.UtcNow.AddDays(-15), MeterReadValue = "54321" };
            var existingMeterReading2 = new MeterReading { AccountId = 5, MeterReadingDateTime = DateTime.UtcNow, MeterReadValue = "555" };

            _dbContext.MeterReadings.Add(existingMeterReading1);
            _dbContext.MeterReadings.Add(existingMeterReading2);
            await _dbContext.SaveChangesAsync();

            var meterReading1 = new MeterReading { AccountId = 1, MeterReadingDateTime = DateTime.UtcNow, MeterReadValue = "12345" }; //Valid and new

            var meterReading2 = new MeterReading { AccountId = 2, MeterReadingDateTime = DateTime.UtcNow, MeterReadValue = "ABCDEF" }; //Invalid MeterReadValue

            var meterReading3 = new MeterReading { AccountId = 3, MeterReadingDateTime = DateTime.UtcNow, MeterReadValue = "12345" }; //Invalid AccountId

            var meterReading4 = new MeterReading { AccountId = 4, MeterReadingDateTime = DateTime.UtcNow, MeterReadValue = "1" };//Valid and more recent

            var meterReading5 = new MeterReading { AccountId = 5, MeterReadingDateTime = DateTime.UtcNow.AddDays(-1), MeterReadValue = "12" };//Valid, but outdated

            _meterReadingMapperMock
                .Setup(x => x.Map(_formFileMock.Object))
                .ReturnsAsync(new List<MeterReading> { meterReading1, meterReading2, meterReading3, meterReading4, meterReading5 });

            var accountId1 = new CustomerAccount { AccountId = 1, FirstName = "A", LastName = "O" };
            var accountId2 = new CustomerAccount { AccountId = 2, FirstName = "B", LastName = "P" };
            var accountId3 = new CustomerAccount { AccountId = 4, FirstName = "C", LastName = "R" };
            var accountId4 = new CustomerAccount { AccountId = 5, FirstName = "D", LastName = "S" };
            var accountId5 = new CustomerAccount { AccountId = 6, FirstName = "E", LastName = "T" };

            _dbContext.CustomerAccounts.Add(accountId1);
            _dbContext.CustomerAccounts.Add(accountId2);
            _dbContext.CustomerAccounts.Add(accountId3);

            await _dbContext.SaveChangesAsync();

            //Act
            await _meterReadingService.ProcessMeterReadings(_formFileMock.Object);

            //Assert
            var meterReadings = _dbContext.MeterReadings.OrderBy(x => x.AccountId).ToList();

            Assert.That(meterReadings.Count, Is.EqualTo(3));

            Assert.That(meterReadings[0].AccountId, Is.EqualTo(meterReading1.AccountId));
            Assert.That(meterReadings[0].MeterReadingDateTime, Is.EqualTo(meterReading1.MeterReadingDateTime));                       
            Assert.That(meterReadings[0].MeterReadValue, Is.EqualTo(meterReading1.MeterReadValue));
            
            Assert.That(meterReadings[1].AccountId, Is.EqualTo(meterReading4.AccountId));
            Assert.That(meterReadings[1].MeterReadingDateTime, Is.EqualTo(meterReading4.MeterReadingDateTime));
            Assert.That(meterReadings[1].MeterReadValue, Is.EqualTo(meterReading4.MeterReadValue));

            Assert.That(meterReadings[2].AccountId, Is.EqualTo(existingMeterReading2.AccountId));
            Assert.That(meterReadings[2].MeterReadingDateTime, Is.EqualTo(existingMeterReading2.MeterReadingDateTime));
            Assert.That(meterReadings[2].MeterReadValue, Is.EqualTo(existingMeterReading2.MeterReadValue));
        }
    }
}