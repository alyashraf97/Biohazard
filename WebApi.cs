using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QuarantinedMailHandler
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuarantinedMailController : ControllerBase
    {
        private readonly QuarantinedMailDbContext _context;
        private readonly ILogger<QuarantinedMailController> _logger;

        public QuarantinedMailController(QuarantinedMailDbContext context, ILogger<QuarantinedMailController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/QuarantinedMail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuarantinedMail>>> GetQuarantinedMails()
        {
            return await _context.QuarantinedMails.ToListAsync();
        }

        // GET: api/QuarantinedMail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuarantinedMail>> GetQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QuarantinedMails.FindAsync(id);

            if (quarantinedMail == null)
            {
                return NotFound();
            }

            return quarantinedMail;
        }

        // PUT: api/QuarantinedMail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuarantinedMail(string id, QuarantinedMail quarantinedMail)
        {
            if (id != quarantinedMail.ID)
            {
                return BadRequest();
            }

            _context.Entry(quarantinedMail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuarantinedMailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/QuarantinedMail
        [HttpPost]
        public async Task<ActionResult<QuarantinedMail>> PostQuarantinedMail(QuarantinedMail quarantinedMail)
        {
            _context.QuarantinedMails.Add(quarantinedMail);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuarantinedMail), new { id = quarantinedMail.ID }, quarantinedMail);
        }

        // DELETE: api/QuarantinedMail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QuarantinedMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            _context.QuarantinedMails.Remove(quarantinedMail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/QuarantinedMail/Release/5
        [HttpGet("Release/{id}")]
        public async Task<IActionResult> ReleaseQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QuarantinedMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            // TODO: Implement the logic to release the quarantined mail from the messaging gateway

            return Ok();
        }

        // GET: api/QuarantinedMail/Retract/5
        [HttpGet("Retract/{id}")]
        public async Task<IActionResult> RetractQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QuarantinedMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            // TODO: Implement the logic to retract the quarantined mail from the messaging gateway

            return Ok();
        }

        private bool QuarantinedMailExists(string id)
        {
            return _context.QuarantinedMails.Any(e => e.ID == id);
        }
    }
}