using ChatApi.Models;
using ChatApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantController : ControllerBase
    {
        private readonly ILogger<ParticipantController> _logger;
        private readonly IMessageProducer _messageProducer;     

        //In memery Db
        private static readonly List<Participant> _participants = new List<Participant>();

        public ParticipantController(ILogger<ParticipantController> logger, IMessageProducer messageProducer)
        {
            _logger = logger;
            _messageProducer = messageProducer;
        }

        [HttpPost]
        public async Task<IActionResult> AddParticipant(Participant participant)
        {
            if (participant == null)
            {
                return BadRequest();
            }
            _participants.Add(participant);
            _messageProducer.SendMessage(participant);
            return Ok();
        } 

    }
}
