// ============================================================
// AssetFlow.WebAPI / Controllers / ProjectAffectationsController.cs
// GET api/projects/{id}/affectations — accessible IT + Admin
// ============================================================

using AssetFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize(Policy = "ITOrAdmin")]
    public class ProjectAffectationsController : ControllerBase
    {
        [HttpGet("{id:int}/affectations")]
        public async Task<IActionResult> GetAffectations(int id, [FromServices] AppDbContext db)
        {
            var affectations = await db.Affectations
                .AsNoTracking()
                .Include(a => a.Materiel)
                .Where(a => a.ProjetId == id)
                .OrderByDescending(a => a.DateAffectation)
                .Select(a => new
                {
                    AffectationId    = a.Id,
                    Designation      = a.Materiel.Designation,
                    Reference        = a.Materiel.Reference,
                    QuantiteAffectee = a.QuantiteAffectee,
                    DateAffectation  = a.DateAffectation,
                    DateRetourPrevue = a.DateRetour,
                    Etat             = a.Etat.ToString()
                })
                .ToListAsync();

            return Ok(affectations);
        }
    }
}