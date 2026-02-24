// ============================================================
// Pages/Achat/Materiel.razor.cs
// Code-behind de la page Matériel
// Gère : chargement, filtres, CRUD, thème, sidebar mobile
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AssetFlow.BlazorUI.Pages.Achat
{
    public partial class Materiel : ComponentBase
    {
        // ── Injection des dépendances ─────────────────────────────
        [Inject] private AssetFlow.BlazorUI.Services.MaterielService MaterielSvc { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        // ── VM interne (vue de la liste) ──────────────────────────
        private class MaterielVm
        {
            public int      Id            { get; set; }
            public string   Reference     { get; set; } = string.Empty;
            public string   Designation   { get; set; } = string.Empty;
            public string?  Description   { get; set; }
            public string   Categorie     { get; set; } = string.Empty;
            public int      QuantiteStock { get; set; }
            public int      QuantiteMin   { get; set; }
            public string   Unite         { get; set; } = "pièce";
            public string?  Emplacement   { get; set; }
            public string   Etat          { get; set; } = "Disponible";
        }

        // ── VM formulaire ─────────────────────────────────────────
        private class FormulaireVm
        {
            public int      Id            { get; set; }
            public string   Reference     { get; set; } = string.Empty;
            public string   Designation   { get; set; } = string.Empty;
            public string?  Description   { get; set; }
            public string   Categorie     { get; set; } = string.Empty;
            public int      QuantiteStock { get; set; }
            public int      QuantiteMin   { get; set; }
            public string   Unite         { get; set; } = "pièce";
            public string?  Emplacement   { get; set; }
            public string   Etat          { get; set; } = "Disponible";
        }

        // ── État de la page ───────────────────────────────────────
        private List<MaterielVm>           _materiels     = new();
        private MaterielStatsDto           _stats         = new();
        private List<string>               _categories    = new();
        private int                        _totalCount    = 0;
        private bool                       _chargement    = true;
        private string                     _erreur        = string.Empty;
        private string                     _termeRecherche = string.Empty;
        private string                     _categorieFiltre = "all";
        private string                     _etatFiltre    = "all";
        private string                     _theme         = "dark";
        private bool                       _sidebarOpen   = false;

        // Formulaire slide-in
        private bool                       _panneauOuvert = false;
        private bool                       _modeModif     = false;
        private bool                       _sauvegarde    = false;
        private FormulaireVm               _form          = new();
        private Dictionary<string, string> _erreurs       = new();

        // Modale suppression
        private MaterielVm?                _aSupprimer    = null;

        // Toast
        private string _toastMsg  = string.Empty;
        private string _toastType = "mat-toast-success";

        // ── Utilisateur courant ───────────────────────────────────
        private string _currentUserName = "Administrateur";
        private string _currentUserRole = "Admin système";

        private string CurrentUserInitials
        {
            get
            {
                var parts = _currentUserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
                return "AD";
            }
        }

        // ── Cycle de vie ──────────────────────────────────────────

        protected override async Task OnInitializedAsync()
        {
            // Détection du thème courant
            var isDark = await JS.InvokeAsync<bool>("eval",
                "document.documentElement.classList.contains('dark')");
            _theme = isDark ? "dark" : "light";

            await ChargerInfosUtilisateur();
            await ChargerDonnees();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            // Observe les changements de thème sur <html>
            await JS.InvokeVoidAsync("eval", @"
                window.__matSetThemeRef = function(ref) {
                    window.__matThemeObs && window.__matThemeObs.disconnect();
                    window.__matThemeObs = new MutationObserver(function() {
                        var isDark = document.documentElement.classList.contains('dark');
                        ref.invokeMethodAsync('OnThemeChanged', isDark);
                    });
                    window.__matThemeObs.observe(document.documentElement, {
                        attributes: true, attributeFilter: ['class']
                    });
                };
            ");
            var dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("__matSetThemeRef", dotNetRef);
        }

        // ── Chargement données ────────────────────────────────────

        private async Task ChargerDonnees()
        {
            _chargement = true; _erreur = string.Empty;
            try
            {
                // Chargement parallèle : stats + liste filtrée
                var statsTask    = MaterielSvc.GetStatsAsync();
                var materielsTask = MaterielSvc.GetAllAsync();
                await Task.WhenAll(statsTask, materielsTask);

                _stats     = statsTask.Result ?? new();
                _totalCount = materielsTask.Result.Count;

                // Extraire les catégories distinctes
                _categories = materielsTask.Result
                    .Select(m => m.Categorie)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                MapperVms(materielsTask.Result);
                AppliquerFiltresLocaux();
            }
            catch (Exception ex) { _erreur = $"Erreur de chargement : {ex.Message}"; }
            finally { _chargement = false; }
        }

        /// <summary>Convertit les DTOs en ViewModels internes</summary>
        private void MapperVms(List<MaterielDto> dtos)
        {
            _materiels = dtos.Select(d => new MaterielVm
            {
                Id            = d.Id,
                Reference     = d.Reference,
                Designation   = d.Designation,
                Description   = d.Description,
                Categorie     = d.Categorie,
                QuantiteStock = d.QuantiteStock,
                QuantiteMin   = d.QuantiteMin,
                Unite         = d.Unite,
                Emplacement   = d.Emplacement,
                Etat          = d.Etat
            }).ToList();
        }

        /// <summary>Filtre la liste en mémoire (recherche + catégorie + état)</summary>
        private void AppliquerFiltresLocaux()
        {
            // Note : le filtre serveur est dans SearchAsync, ici on peut re-filtrer
            // pour une UX plus réactive (sans aller-retour serveur à chaque frappe)
            var q = _materiels.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_termeRecherche))
            {
                var t = _termeRecherche.Trim().ToLower();
                q = q.Where(m =>
                    m.Designation.ToLower().Contains(t) ||
                    m.Reference.ToLower().Contains(t)   ||
                    (m.Description?.ToLower().Contains(t) ?? false));
            }
            if (_categorieFiltre != "all")
                q = q.Where(m => m.Categorie.Equals(_categorieFiltre, StringComparison.OrdinalIgnoreCase));
            if (_etatFiltre != "all")
                q = q.Where(m => m.Etat.Equals(_etatFiltre, StringComparison.OrdinalIgnoreCase));

            _materiels = q.ToList();
        }

        // ── Filtres & recherche ───────────────────────────────────

        private async void OnRecherche(ChangeEventArgs e)
        {
            _termeRecherche = e.Value?.ToString() ?? string.Empty;
            await ChargerDonnees();
        }

        private async void OnCategorieChange(ChangeEventArgs e)
        {
            _categorieFiltre = e.Value?.ToString() ?? "all";
            await ChargerDonnees();
        }

        private async void OnEtatChange(ChangeEventArgs e)
        {
            _etatFiltre = e.Value?.ToString() ?? "all";
            await ChargerDonnees();
        }

        // ── Formulaire ────────────────────────────────────────────

        private void OuvrirFormulaire(MaterielVm? vm)
        {
            _erreurs.Clear();
            _modeModif     = vm is not null;
            _panneauOuvert = true;
            _form = vm is not null
                ? new FormulaireVm
                {
                    Id            = vm.Id,
                    Reference     = vm.Reference,
                    Designation   = vm.Designation,
                    Description   = vm.Description,
                    Categorie     = vm.Categorie,
                    QuantiteStock = vm.QuantiteStock,
                    QuantiteMin   = vm.QuantiteMin,
                    Unite         = vm.Unite,
                    Emplacement   = vm.Emplacement,
                    Etat          = vm.Etat
                }
                : new FormulaireVm();
        }

        private void FermerFormulaire() { _panneauOuvert = false; _erreurs.Clear(); }

        private async Task Sauvegarder()
        {
            _erreurs.Clear();

            // Validations côté client
            if (string.IsNullOrWhiteSpace(_form.Reference))
                _erreurs["Reference"]   = "La référence est obligatoire.";
            if (string.IsNullOrWhiteSpace(_form.Designation))
                _erreurs["Designation"] = "La désignation est obligatoire.";
            if (string.IsNullOrWhiteSpace(_form.Categorie))
                _erreurs["Categorie"]   = "La catégorie est obligatoire.";
            if (_form.QuantiteStock < 0)
                _erreurs["QuantiteStock"] = "La quantité ne peut pas être négative.";
            if (_erreurs.Any()) return;

            _sauvegarde = true;
            try
            {
                MaterielResultDto result;
                if (_modeModif)
                {
                    result = await MaterielSvc.ModifierAsync(new ModifierMaterielDto
                    {
                        Id            = _form.Id,
                        Reference     = _form.Reference.Trim(),
                        Designation   = _form.Designation.Trim(),
                        Description   = Vide(_form.Description),
                        Categorie     = _form.Categorie.Trim(),
                        QuantiteStock = _form.QuantiteStock,
                        QuantiteMin   = _form.QuantiteMin,
                        Unite         = _form.Unite.Trim(),
                        Emplacement   = Vide(_form.Emplacement),
                        Etat          = _form.Etat
                    });
                }
                else
                {
                    result = await MaterielSvc.AjouterAsync(new CreerMaterielDto
                    {
                        Reference     = _form.Reference.Trim(),
                        Designation   = _form.Designation.Trim(),
                        Description   = Vide(_form.Description),
                        Categorie     = _form.Categorie.Trim(),
                        QuantiteStock = _form.QuantiteStock,
                        QuantiteMin   = _form.QuantiteMin,
                        Unite         = _form.Unite.Trim(),
                        Emplacement   = Vide(_form.Emplacement),
                        Etat          = _form.Etat
                    });
                }

                if (result.Succes)
                {
                    FermerFormulaire();
                    AfficherToast(
                        _modeModif ? $"« {_form.Designation} » mis à jour." : $"« {_form.Designation} » ajouté.",
                        "mat-toast-success");
                    await ChargerDonnees();
                }
                else
                {
                    _erreur = result.Message;
                }
            }
            catch (Exception ex) { _erreur = ex.Message; }
            finally { _sauvegarde = false; }
        }

        // ── Suppression ───────────────────────────────────────────

        private void DemanderSuppression(MaterielVm vm) => _aSupprimer = vm;
        private void AnnulerSuppression()                => _aSupprimer = null;

        private async Task ConfirmerSuppression()
        {
            if (_aSupprimer is null) return;
            var nom = _aSupprimer.Designation;
            var id  = _aSupprimer.Id;
            _aSupprimer = null;

            var result = await MaterielSvc.SupprimerAsync(id);
            if (result.Succes)
            {
                AfficherToast($"« {nom} » supprimé.", "mat-toast-success");
                await ChargerDonnees();
            }
            else _erreur = result.Message;
        }

        // ── Thème ─────────────────────────────────────────────────

        [JSInvokable("OnThemeChanged")]
        public void OnThemeChanged(bool isDark)
        {
            _theme = isDark ? "dark" : "light";
            InvokeAsync(StateHasChanged);
        }

        private void ToggleSidebar() => _sidebarOpen = !_sidebarOpen;

        // ── Helpers d'affichage ───────────────────────────────────

        /// <summary>Retourne la classe CSS du badge d'état</summary>
        private static string BadgeClass(string etat) => etat switch
        {
            "Disponible"  => "badge-disponible",
            "EnRupture"   => "badge-rupture",
            "EnCommande"  => "badge-commande",
            "HorsService" => "badge-hors-service",
            _             => "badge-disponible"
        };

        /// <summary>Retourne le libellé affichable de l'état</summary>
        private static string BadgeLabel(string etat) => etat switch
        {
            "Disponible"  => "Disponible",
            "EnRupture"   => "Rupture",
            "EnCommande"  => "En commande",
            "HorsService" => "Hors service",
            _             => etat
        };

        private static string? Vide(string? v) =>
            string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        private async void AfficherToast(string msg, string type)
        {
            _toastMsg = msg; _toastType = type; StateHasChanged();
            await Task.Delay(3500);
            _toastMsg = string.Empty; StateHasChanged();
        }

        // ── Infos utilisateur ─────────────────────────────────────

        private async Task ChargerInfosUtilisateur()
        {
            try
            {
                var nom = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_name') || localStorage.getItem('userFullName') || localStorage.getItem('currentUserName')");
                var role = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_role') || localStorage.getItem('currentUserRole')");

                if (!string.IsNullOrWhiteSpace(nom))
                    _currentUserName = SupprimerGuillemets(nom);
                if (!string.IsNullOrWhiteSpace(role))
                    _currentUserRole = SupprimerGuillemets(role);
            }
            catch { /* garde les valeurs par défaut */ }
        }

        private static string SupprimerGuillemets(string v)
        {
            v = v.Trim();
            if (v.Length >= 2 &&
                ((v.StartsWith('"') && v.EndsWith('"')) ||
                 (v.StartsWith('\'') && v.EndsWith('\''))))
                v = v[1..^1].Trim();
            return v;
        }
    }
}