using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleBiographieController : ControllerBase
    {
        private readonly IArticleBiographieService _service;

        public ArticleBiographieController(IArticleBiographieService service)
        {
            _service = service;
        }

        /// <summary>Retourne tous les matériels avec leurs articles (pour le sélecteur)</summary>
        [HttpGet("materiels")]
        public async Task<IActionResult> GetMateriels()
        {
            var result = await _service.GetMaterielsAvecArticlesAsync();
            return Ok(result);
        }

        /// <summary>Retourne la biographie complète d'un article</summary>
        [HttpGet("{articleId:int}")]
        public async Task<IActionResult> GetBiographie(int articleId)
        {
            var result = await _service.GetBiographieAsync(articleId);
            if (result == null) return NotFound($"Article {articleId} introuvable.");
            return Ok(result);
        }
    }
}
