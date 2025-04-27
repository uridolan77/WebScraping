using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebScraperApi.Models;
using WebScraperApi.Services.Scheduling;
using WebScraperApi.Services.State;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly IScraperSchedulingService _schedulingService;
        private readonly IScraperStateService _stateService;
        
        public SchedulingController(
            IScraperSchedulingService schedulingService,
            IScraperStateService stateService)
        {
            _schedulingService = schedulingService;
            _stateService = stateService;
        }
        
        [HttpGet]
        public IActionResult GetAllSchedules()
        {
            var schedules = _schedulingService.GetAllScheduledItems();
            return Ok(new { Schedules = schedules });
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchedule(string id)
        {
            // Find which scraper this schedule belongs to
            foreach (var scraperId in _stateService.GetAllScraperIds())
            {
                var schedules = _schedulingService.GetScheduledItems(scraperId);
                foreach (var schedule in schedules)
                {
                    if (schedule.Id == id)
                    {
                        return Ok(schedule);
                    }
                }
            }
            
            return NotFound($"Schedule with ID {id} not found");
        }
        
        [HttpGet("scraper/{scraperId}")]
        public IActionResult GetScraperSchedules(string scraperId)
        {
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {scraperId} not found");
            }
            
            var schedules = _schedulingService.GetScheduledItems(scraperId);
            return Ok(new { Schedules = schedules });
        }
        
        [HttpPost("scraper/{scraperId}")]
        public async Task<IActionResult> CreateSchedule(string scraperId, [FromBody] ScheduleConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {scraperId} not found");
            }
            
            try
            {
                var schedule = await _schedulingService.ScheduleScraper(scraperId, config);
                
                return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(string id, [FromBody] ScheduleConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var success = await _schedulingService.UpdateSchedule(id, config);
                if (!success)
                {
                    return NotFound($"Schedule with ID {id} not found");
                }
                
                return Ok(new { Message = "Schedule updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(string id)
        {
            var success = await _schedulingService.RemoveSchedule(id);
            if (!success)
            {
                return NotFound($"Schedule with ID {id} not found");
            }
            
            return NoContent();
        }
        
        [HttpPost("validate-cron")]
        public IActionResult ValidateCronExpression([FromBody] CronValidationRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.CronExpression))
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var schedule = NCrontab.CrontabSchedule.Parse(request.CronExpression);
                
                // Calculate next occurrences
                var now = DateTime.Now;
                var nextOccurrences = new DateTime[5];
                
                var nextRun = schedule.GetNextOccurrence(now);
                nextOccurrences[0] = nextRun;
                
                for (int i = 1; i < 5; i++)
                {
                    nextRun = schedule.GetNextOccurrence(nextRun.AddSeconds(1));
                    nextOccurrences[i] = nextRun;
                }
                
                return Ok(new
                {
                    IsValid = true,
                    NextOccurrences = nextOccurrences
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
    
    public class CronValidationRequest
    {
        public string CronExpression { get; set; }
    }
}