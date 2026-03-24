// ============================================================
// AssetFlow.BlazorUI / Pages / Employe / MesEquipements.razor.cs
// MISE À JOUR : Groupement par matériel + gestion modal articles
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using static AssetFlow.BlazorUI.Services.EmployeService;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class MesEquipements
    {
        [Inject] private EmployeService    EmployeService { get; set; } = default!;
        [Inject] private NavigationManager Navigation     { get; set; } = default!;

        // ── Données ────────────────────────────────────────────
        private List<MaterielAffecteGroupeDto> MaterielsGroupes       { get; set; } = new();
        private List<MaterielAffecteGroupeDto> MaterielsGroupesFiltres { get; set; } = new();

        private bool   IsLoading     { get; set; } = true;
        private string ErrorMessage  { get; set; } = string.Empty;

        // ── Recherche principale ───────────────────────────────
        private string SearchQuery { get; set; } = string.Empty;

        // ── Modal ──────────────────────────────────────────────
        private bool                     ModalOuvert          { get; set; } = false;
        private MaterielAffecteGroupeDto? MaterielSelectionne  { get; set; } = null;
        private string                   ModalSearchQuery      { get; set; } = string.Empty;

        // ── Info utilisateur ───────────────────────────────────
        private string UserName { get; set; } = "Utilisateur";
        private string UserRole { get; set; } = "Employé";

        // ── Init ───────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            await LoadUserInfoAsync();
            await LoadMaterielsGroupesAsync();
        }

        private async Task LoadUserInfoAsync()
        {
            UserName = await EmployeService.GetCurrentUserNameAsync();
            UserRole = await EmployeService.GetCurrentUserRoleAsync();
        }

        private async Task LoadMaterielsGroupesAsync()
        {
            try
            {
                IsLoading    = true;
                ErrorMessage = string.Empty;

                MaterielsGroupes        = await EmployeService.GetMaterielsGroupesAsync();
                MaterielsGroupesFiltres = MaterielsGroupes;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur lors du chargement : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Recherche ──────────────────────────────────────────
        private void OnSearchInput(ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? string.Empty;
            FiltrerMateriels();
        }

        private void FiltrerMateriels()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                MaterielsGroupesFiltres = MaterielsGroupes;
                return;
            }

            var q = SearchQuery.Trim().ToLower();
            MaterielsGroupesFiltres = MaterielsGroupes
                .Where(m =>
                    m.Designation.ToLower().Contains(q) ||
                    m.Reference.ToLower().Contains(q)   ||
                    m.Categorie.ToLower().Contains(q))
                .ToList();
        }

        // ── Modal ──────────────────────────────────────────────
        private void OuvrirModal(MaterielAffecteGroupeDto materiel)
        {
            MaterielSelectionne = materiel;
            ModalSearchQuery    = string.Empty;
            ModalOuvert         = true;
        }

        private void FermerModal()
        {
            ModalOuvert         = false;
            MaterielSelectionne = null;
            ModalSearchQuery    = string.Empty;
        }

        private void OnModalSearchInput(ChangeEventArgs e)
        {
            ModalSearchQuery = e.Value?.ToString() ?? string.Empty;
            StateHasChanged();
        }

        // ── Navigation vers détail ─────────────────────────────
        private void NaviguerVersDetail(int affectationId, int articleId)
        {
            Navigation.NavigateTo($"/employe/equipement/{affectationId}/article/{articleId}");
        }

        // ── Helpers UI ─────────────────────────────────────────
        private string GetStatutLabel(string statut) => statut switch
        {
            "EnCours"   => "BON",
            "Retourne"  => "RETOURNÉ",
            "Perdu"     => "PERDU",
            "Endommage" => "ENDOMMAGÉ",
            _           => statut.ToUpper()
        };

        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "??";
        }
            // ============================================================
            // PATCH MesEquipements.razor.cs
            // Ajoutez ces propriétés et méthodes dans la classe partielle
            // MesEquipements existante
            // ============================================================

            // ── Propriétés état du modal commentaire ──────────────────────
            private bool              ModalCommentaireOuvert  { get; set; } = false;
            private int               CommentaireMaterielId   { get; set; } = 0;
            private string            CommentaireNumeroSerie  { get; set; } = string.Empty;
            private string            CommentaireContenu      { get; set; } = string.Empty;
            private string            CommentaireFeedback     { get; set; } = string.Empty;
            private bool              CommentaireSucces       { get; set; } = false;
            private bool              CommentaireEnvoi        { get; set; } = false;
            private List<CommentaireDto> CommentairesExistants { get; set; } = new();

            // ── Ouvre le modal commentaire ────────────────────────────────
            private async Task OuvrirModalCommentaire(int materielId, string numeroSerie)
            {
                CommentaireMaterielId  = materielId;
                CommentaireNumeroSerie = numeroSerie;
                CommentaireContenu     = string.Empty;
                CommentaireFeedback    = string.Empty;
                CommentaireSucces      = false;
                CommentaireEnvoi       = false;

                // Charger les commentaires existants
                CommentairesExistants = await EmployeService.GetCommentairesMaterielAsync(materielId);

                ModalCommentaireOuvert = true;
            }

            // ── Ferme le modal commentaire ────────────────────────────────
            private void FermerModalCommentaire()
            {
                ModalCommentaireOuvert = false;
                CommentaireFeedback    = string.Empty;
                CommentaireContenu     = string.Empty;
                CommentairesExistants  = new();
            }

            // ── Envoie le commentaire vers l'API ─────────────────────────
            private async Task EnvoyerCommentaire()
            {
                if (string.IsNullOrWhiteSpace(CommentaireContenu)) return;

                CommentaireEnvoi    = true;
                CommentaireFeedback = string.Empty;
                StateHasChanged();

                var result = await EmployeService.AjouterCommentaireAsync(
                    CommentaireMaterielId, CommentaireContenu);

                CommentaireSucces   = result.Succes;
                CommentaireEnvoi    = false;

                if (result.Succes)
                {
                    CommentaireFeedback = "Commentaire enregistré avec succès !";
                    CommentaireContenu  = string.Empty;
                    // Rafraîchir la liste
                    CommentairesExistants = await EmployeService.GetCommentairesMaterielAsync(CommentaireMaterielId);
                }
                else
                {
                    CommentaireFeedback = result.Message;
                }

                StateHasChanged();
            }
    }

}