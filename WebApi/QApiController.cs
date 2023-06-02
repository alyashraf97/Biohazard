using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Biohazard.Data;
using Biohazard.Listener;

namespace Biohazard.WebApi
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QApiController : ControllerBase
    {
        private readonly QMailDbContext _context;
        private Serilog.ILogger _log = QLogger.GetLogger<QApiController>();

        public QApiController(QMailDbContext context)
        {
            _context = context;
        }

        // GET: api/QMail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QMail>>> GetQuarantinedMails()
        {
            return await _context.QMails.ToListAsync();
        }

        [Authorize(Policy ="AdminOnly")]
        // GET: api/QMail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QMail>> GetQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QMails.FindAsync(id);

            if (quarantinedMail == null)
            {
                return NotFound();
            }

            return quarantinedMail;
        }

        // PUT: api/QMail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuarantinedMail(string id, QMail quarantinedMail)
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

        // POST: api/QMail
        [HttpPost]
        public async Task<ActionResult<QMail>> PostQuarantinedMail(QMail quarantinedMail)
        {
            _context.QMails.Add(quarantinedMail);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuarantinedMail), new { id = quarantinedMail.ID }, quarantinedMail);
        }

        // DELETE: api/QMail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            _context.QMails.Remove(quarantinedMail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/QMail/Release/5
        [HttpGet("Release/{id}")]
        public async Task<IActionResult> ReleaseQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            // TODO: Implement the logic to release the quarantined mail from the messaging gateway

            return Ok();
        }

        // GET: api/QMail/Retract/5
        [HttpGet("Retract/{id}")]
        public async Task<IActionResult> RetractQuarantinedMail(long id)
        {
            var quarantinedMail = await _context.QMails.FindAsync(id);
            if (quarantinedMail == null)
            {
                return NotFound();
            }

            // TODO: Implement the logic to retract the quarantined mail from the messaging gateway

            return Ok();
        }

        private bool QuarantinedMailExists(string id)
        {
            return _context.QMails.Any(e => e.ID == id);
        }
    }
}