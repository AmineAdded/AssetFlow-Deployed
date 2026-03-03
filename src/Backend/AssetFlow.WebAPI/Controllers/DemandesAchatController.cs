// ============================================================
// COUCHE  : AssetFlow.WebApi
// FICHIER : Controllers/DemandesAchatController.cs
// RÔLE    : Endpoints REST pour la page Agent Achat.
//           Même pattern que FournisseursController.cs
//
// ENDPOINTS :
//   GET    /api/demandes                            → liste toutes les demandes
//   GET    /api/demandes/{id}                       → détail d'une demande
//   PUT    /api/demandes/{id}/statut                → changer statut (+ motif refus)
//   POST   /api/demandes/{id}/offres                → uploader un PDF (multipart)
//   DELETE /api/demandes/{id}/offres/{offreId}      → supprimer une offre
//   GET    /api/demandes/{id}/offres/{offreId}/pdf  → télécharger le PDF pour l'iframe
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AssetFlow.WebApi.Controllers
{
    [ApiController]
    [Route("api/demandes")]
    [Authorize(Policy = "EquipeAchatOnly")]
    public class DemandesAchatController : ControllerBase
    {
        private readonly IDemandeAchatService _service;

        public DemandesAchatController(IDemandeAchatService service)
        {
            _service = service;
        }

        // ────────────────────────────────────────────────────────
        // GET /api/demandes
        // Retourne toutes les demandes avec leurs offres (sans PDF)
        // ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<List<DemandeAchatDto>>> GetAll()
        {
            var liste = await _service.GetAllAsync();
            return Ok(liste.Select(MapToDto).ToList());
        }

        // ────────────────────────────────────────────────────────
        // GET /api/demandes/{id}
        // Retourne le détail complet d'une demande
        // ────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DemandeAchatDto>> GetById(int id)
        {
            var demande = await _service.GetByIdAsync(id);

            if (demande == null)
                return NotFound(new { Message = $"Demande ID {id} introuvable." });

            return Ok(MapToDto(demande));
        }

        // ────────────────────────────────────────────────────────
        // PUT /api/demandes/{id}/statut
        // Change le statut : en_attente → commande → traite | refuse
        // ────────────────────────────────────────────────────────

        [HttpPut("{id:int}/statut")]
        public async Task<ActionResult<DemandeAchatReponseDto>> ChangerStatut(
            int id,
            [FromBody] ChangerStatutDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Statut))
                return BadRequest(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = "Le statut est obligatoire."
                });

            try
            {
                await _service.ChangerStatutAsync(id, dto.Statut, dto.MotifRefus);

                return Ok(new DemandeAchatReponseDto
                {
                    Succes    = true,
                    Message   = $"Statut mis à jour : {dto.Statut}",
                    IdDemande = id
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = ex.Message
                });
            }
        }

        // ────────────────────────────────────────────────────────
        // POST /api/demandes/{id}/offres
        // Upload d'un PDF — multipart/form-data
        // Le champ du formulaire doit s'appeler "fichier"
        // ────────────────────────────────────────────────────────

        [HttpPost("{id:int}/offres")]
        [RequestSizeLimit(10 * 1024 * 1024)]   // max 10 Mo par fichier
        public async Task<ActionResult<DemandeAchatReponseDto>> AjouterOffre(
            int id,
            IFormFile fichier)
        {
            if (fichier == null || fichier.Length == 0)
                return BadRequest(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = "Aucun fichier reçu."
                });

            if (!fichier.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = "Seuls les fichiers PDF sont acceptés."
                });

            // Lire le binaire complet
            byte[] contenu;
            using (var ms = new MemoryStream())
            {
                await fichier.CopyToAsync(ms);
                contenu = ms.ToArray();
            }

            try
            {
                var offre = new OffreAchat
                {
                    NomFichier = fichier.FileName,
                    Taille     = fichier.Length,
                    ContenuPdf = contenu,
                    EstChoisie = false
                };

                await _service.AjouterOffreAsync(id, offre);

                return StatusCode(201, new DemandeAchatReponseDto
                {
                    Succes    = true,
                    Message   = $"Offre « {fichier.FileName} » ajoutée.",
                    IdDemande = id
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = ex.Message
                });
            }
        }

        // ────────────────────────────────────────────────────────
        // DELETE /api/demandes/{id}/offres/{offreId}
        // Supprime une offre PDF
        // ────────────────────────────────────────────────────────

        [HttpDelete("{id:int}/offres/{offreId:guid}")]
        public async Task<ActionResult<DemandeAchatReponseDto>> SupprimerOffre(
            int id, Guid offreId)
        {
            try
            {
                await _service.SupprimerOffreAsync(offreId);

                return Ok(new DemandeAchatReponseDto
                {
                    Succes    = true,
                    Message   = "Offre supprimée.",
                    IdDemande = id
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new DemandeAchatReponseDto
                {
                    Succes  = false,
                    Message = ex.Message
                });
            }
        }

        // ────────────────────────────────────────────────────────
        // GET /api/demandes/{id}/offres/{offreId}/pdf
        // Retourne le binaire PDF — utilisé par l'iframe dans le frontend
        // ────────────────────────────────────────────────────────

        [HttpGet("{id:int}/offres/{offreId:guid}/pdf")]
        public async Task<IActionResult> GetPdf(int id, Guid offreId)
        {
            var contenu = await _service.GetContenuPdfAsync(offreId);

            if (contenu == null || contenu.Length == 0)
                return NotFound(new { Message = "PDF introuvable." });

            return File(contenu, "application/pdf");
        }

        // ────────────────────────────────────────────────────────
        // MAPPER Entité → DTO
        // Les offres sont mappées SANS le ContenuPdf (binaire)
        // pour ne pas surcharger les réponses GET
        // ────────────────────────────────────────────────────────

        private static DemandeAchatDto MapToDto(DemandeAchat d) => new()
        {
            IdDemande    = d.IdDemande,
            Reference    = d.Reference,
            NomProduit   = d.NomProduit,
            Quantite     = d.Quantite,
            Description  = d.Description,
            Statut       = d.Statut,
            DateCreation = d.DateCreation,
            DemandeurNom = d.DemandeurNom,
            MotifRefus   = d.MotifRefus,
            Offres = d.Offres.Select(o => new OffreAchatDto
            {
                IdOffre    = o.IdOffre,
                NomFichier = o.NomFichier,
                Taille     = o.Taille,
                EstChoisie = o.EstChoisie
                // ContenuPdf volontairement absent
            }).ToList()
        };
    }
}
