using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using WebApi.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnergyReaderController : ControllerBase
    {
        IMeterReadingService _energyReaderService;
        private readonly ILogger<EnergyReaderController> _logger;

        public EnergyReaderController(IMeterReadingService energyReaderService, ILogger<EnergyReaderController> logger)
        {
            _energyReaderService = energyReaderService;
            _logger = logger;
        }

        [HttpPost("meter-reading-uploads")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UploadMeterReadings([Required]IFormFile csvFile)
        {
            try
            {
                var result = await _energyReaderService.ProcessMeterReadings(csvFile);
                return Ok(new { Successes = result.successes, Failures = result.failures});
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong while processing file {csvFile.FileName}", ex);
                return StatusCode(500, ex.Message);
            }
        }
    }
}