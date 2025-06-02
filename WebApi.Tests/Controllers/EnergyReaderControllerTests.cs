using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebApi.Services;
using WebApplication1.Controllers;

namespace WebApi.Tests.Controllers
{
    [TestFixture]
    public class EnergyReaderControllerTests
    {
        EnergyReaderController _controller;

        Mock<IMeterReadingService> _meterReadingServiceMock;
        Mock<ILogger<EnergyReaderController>> _loggerMock;

        Mock<IFormFile> _formFileMock;

        [SetUp]
        public void SetUp()
        {
            _meterReadingServiceMock = new Mock<IMeterReadingService>();
            _loggerMock = new Mock<ILogger<EnergyReaderController>>();

            _formFileMock = new Mock<IFormFile>();

            _controller = new EnergyReaderController(_meterReadingServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task UploadMeterReadings_ShouldReturnOK_WithTheExpectedResult_WhenTheServiceIsSuccessful()
        {
            //Arrange
            var result = (5, 9);
            _meterReadingServiceMock.Setup(x => x.ProcessMeterReadings(It.IsAny<IFormFile>())).ReturnsAsync(result);

            //Act
            var response = await _controller.UploadMeterReadings(_formFileMock.Object);

            //Assert
            Assert.That(response is OkObjectResult, Is.True);

            var okObjectResult = (OkObjectResult)response;

            Assert.That(okObjectResult.Value.ToString(), Is.EqualTo("The file was uploaded successfully. Readings processed: 5. Readings failed: 9"));
        }

        [Test]
        public async Task UploadMeterReadings_ShouldReturnBadRequest_WhenTheService_ThrowsABadHttpRequestException()
        {
            //Arrange           
            var exception = new BadHttpRequestException("Bad http request exception");
            _meterReadingServiceMock.Setup(x => x.ProcessMeterReadings(It.IsAny<IFormFile>())).ThrowsAsync(exception);

            //Act
            var response = await _controller.UploadMeterReadings(_formFileMock.Object);

            //Assert
            Assert.That(response is BadRequestObjectResult, Is.True);

            var badRequestObjectResult = (BadRequestObjectResult)response;

            Assert.That(badRequestObjectResult.Value.ToString(), Is.EqualTo(exception.Message));
        }

        [Test]
        public async Task UploadMeterReadings_ShouldReturnStatus500_WhenTheService_ThrowsAGenericException()
        {
            //Arrange
            var fileName = "Test file name";
            _formFileMock.Setup(x => x.FileName).Returns(fileName);

            var exception = new Exception("Generic exception");
            _meterReadingServiceMock.Setup(x => x.ProcessMeterReadings(It.IsAny<IFormFile>())).ThrowsAsync(exception);

            //Act
            var response = await _controller.UploadMeterReadings(_formFileMock.Object);

            //Assert
            Assert.That(response is ObjectResult, Is.True);

            var objectResult = (ObjectResult)response;

            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value.ToString(), Is.EqualTo(exception.Message));
        }
    }
}