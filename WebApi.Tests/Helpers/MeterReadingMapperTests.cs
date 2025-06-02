using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WebApi.Helpers;

namespace WebApi.Tests.Helpers
{
    [TestFixture]
    public class MeterReadingMapperTests
    {
        private MeterReadingMapper _meterReadingMapper;

        [SetUp]
        public void SetUp()
        {
            _meterReadingMapper = new MeterReadingMapper();
        }

        [Test]
        public void Map_ShouldThrowABadHttpRequestException_WhenFormFileIsNull()
        {
            //Act
            var exception = Assert.ThrowsAsync<BadHttpRequestException>(() =>_meterReadingMapper.Map(null));

            //Assert
            Assert.That(exception.Message, Is.EqualTo("Input formFile is null"));
        }

        [TestCase(".cvs")]
        [TestCase(".txt")]
        [TestCase(".xls")]
        public void Map_ShouldThrowABadHttpRequestException_WhenFormFileIsDoesNotHaveACsvExtension(string fileExtension)
        {
            //Arrange
            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            var fileName = "Test_file_name" + fileExtension;
            formFileMock.Setup(x => x.FileName).Returns(fileName);

            //Act
            var exception = Assert.ThrowsAsync<BadHttpRequestException>(() => _meterReadingMapper.Map(formFileMock.Object));

            //Assert
            Assert.That(exception.Message, Is.EqualTo($"File {fileName} does not have a '.csv' extension"));
        }

        [Test]
        public void Map_ShouldThrowABadHttpRequestException_WhenFormFileLengthIsZero()
        {
            //Arrange
            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            var fileName = "Test_file_name.csv";
            formFileMock.Setup(x => x.FileName).Returns(fileName);
            formFileMock.Setup(x => x.Length).Returns(0);

            //Act
            var exception = Assert.ThrowsAsync<BadHttpRequestException>(() => _meterReadingMapper.Map(formFileMock.Object));

            //Assert
            Assert.That(exception.Message, Is.EqualTo($"File {fileName} is empty"));
        }

        [Test]
        public async Task Map_ShouldReturnTheExpectedMeterReadings_WhenInputDataIsValid()
        {
            //Arrange
            var fileName = "Test_file_name.csv";

            var fileContent = "AccountId,MeterReadingDateTime,MeterReadValue,\r\n2344,22/04/2019 09:24,1002,";

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(fileContent);
            streamWriter.Flush();
            memoryStream.Position = 0;

            IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "id_from_form", fileName);

            //Act
            var response = await _meterReadingMapper.Map(formFile);

            //Assert
            Assert.That(response.Count(), Is.EqualTo(1));

            Assert.That(response[0].AccountId, Is.EqualTo(2344));
            Assert.That(response[0].MeterReadingDateTime, Is.EqualTo(new DateTime(2019, 04, 22, 09, 24, 00)));
            Assert.That(response[0].MeterReadValue, Is.EqualTo("1002"));
        }
    }
}