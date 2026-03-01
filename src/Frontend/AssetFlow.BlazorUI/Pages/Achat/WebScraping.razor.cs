// ============================================================
// FICHIER  : Pages/Achat/WebScraping.razor.cs
// RÔLE     : Code-behind de la page Scraping Marché.
//
// COMPORTEMENT :
//   · Si la page est ouverte via /achat/web-scraping?q=Souris
//     (depuis le bouton "Recherche de prix" de DemandesAchat),
//     le champ de recherche est pré-rempli et la recherche se
//     lance automatiquement.
//   · Si la page est ouverte sans paramètre (depuis le sidebar),
//     l'utilisateur saisit lui-même le produit.
//
// LIAISON PYTHON (à venir) :
//   La méthode LancerRecherche() appellera le service Python
//   via HttpClient. Pour l'instant elle simule des résultats mock.
// ============================================================

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AssetFlow.BlazorUI.Pages.Achat
{
    public partial class WebScraping : ComponentBase
    {
        // ── Injection ───────────────────────────────────────────
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        // ── Modèle d'un résultat de scraping ────────────────────
        /// <summary>
        /// Représente un résultat renvoyé par le scraper Python.
        /// Sera mappé depuis le JSON de l'API Python plus tard.
        /// </summary>
        private class ResultatScraping
        {
            public string Site { get; set; } = string.Empty;
            public string NomProduit { get; set; } = string.Empty;
            public decimal Prix { get; set; }
            public bool EnStock { get; set; }
            public string Livraison { get; set; } = "Non précisé";
            public string Garantie { get; set; } = "Non précisée";
            public string Url { get; set; } = string.Empty;
        }

        // ── État ────────────────────────────────────────────────
        private string _theme = "dark";
        private bool _sidebarOpen = false;

        // Profil utilisateur (lu depuis localStorage)
        private string _nomUtilisateur = "Adem Added";
        private string _roleUtilisateur = "EquipeAchat";
        private string _initiales = "AA";

        // Recherche
        private string _recherche = string.Empty; // valeur du champ
        private string? _nomRecherche = null;         // dernier terme soumis
        private string? _derniereRecherche = null;        // null = pas encore cherché
        private bool _chargement = false;

        // Résultats et filtre
        private List<ResultatScraping> _resultats = new();
        private string _filtreActif = "prix"; // "prix" | "dispo"

        // Toast
        private string _toastMsg = string.Empty;
        private string _toastType = "ws-toast-success";

        // ── Lifecycle ───────────────────────────────────────────

        protected override async Task OnInitializedAsync()
        {
            // Lire le thème depuis le DOM
            try
            {
                var isDark = await JS.InvokeAsync<bool>("eval",
                    "document.documentElement.classList.contains('dark')");
                _theme = isDark ? "dark" : "light";
            }
            catch { }

            // Lire les infos utilisateur depuis localStorage
            await ChargerInfosUtilisateur();

            // Lire le query string ?q= transmis par DemandesAchat
            LireQueryString();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            // Abonnement aux changements de thème (bouton lune)
            try
            {
                await JS.InvokeVoidAsync("eval", @"
                    window.__wsThemeRef = null;
                    window.__wsSetTheme = function(ref) {
                        window.__wsThemeRef = ref;
                        if (window.__wsThemeObs) window.__wsThemeObs.disconnect();
                        window.__wsThemeObs = new MutationObserver(function() {
                            var dark = document.documentElement.classList.contains('dark');
                            window.__wsThemeRef &&
                                window.__wsThemeRef.invokeMethodAsync('OnThemeChanged', dark);
                        });
                        window.__wsThemeObs.observe(document.documentElement, {
                            attributes: true, attributeFilter: ['class']
                        });
                    };
                ");
                var dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("__wsSetTheme", dotNetRef);
            }
            catch { }

            // Si un terme était dans le query string → lancer la recherche auto
            if (!string.IsNullOrWhiteSpace(_recherche))
                await LancerRecherche();
        }

        /// <summary>Appelé par JS quand le thème change.</summary>
        [JSInvokable("OnThemeChanged")]
        public void OnThemeChanged(bool isDark)
        {
            _theme = isDark ? "dark" : "light";
            InvokeAsync(StateHasChanged);
        }

        // ── Lecture du query string ─────────────────────────────

        /// <summary>
        /// Extrait ?q= de l'URL pour pré-remplir le champ
        /// quand l'utilisateur arrive depuis "Recherche de prix".
        /// </summary>
        private void LireQueryString()
        {
            var uri = new Uri(Nav.Uri);
            var query = uri.Query.TrimStart('?');

            foreach (var param in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = param.Split('=', 2);
                if (parts.Length == 2 &&
                    Uri.UnescapeDataString(parts[0]) == "q" &&
                    !string.IsNullOrWhiteSpace(parts[1]))
                {
                    _recherche = Uri.UnescapeDataString(parts[1]).Trim();
                    break;
                }
            }
        }

        // ── Infos utilisateur ───────────────────────────────────

        private async Task ChargerInfosUtilisateur()
        {
            try
            {
                var nom = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_name') || " +
                    "localStorage.getItem('userFullName') || " +
                    "localStorage.getItem('currentUserName')");

                var role = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_role') || " +
                    "localStorage.getItem('currentUserRole')");

                if (!string.IsNullOrWhiteSpace(nom))
                {
                    _nomUtilisateur = Nettoyer(nom);
                    var parts = _nomUtilisateur.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    _initiales = parts.Length >= 2
                        ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                        : _nomUtilisateur[..Math.Min(2, _nomUtilisateur.Length)].ToUpper();
                }
                if (!string.IsNullOrWhiteSpace(role))
                    _roleUtilisateur = Nettoyer(role);
            }
            catch { }
        }

        // ── Actions ─────────────────────────────────────────────

        private void ToggleSidebar() => _sidebarOpen = !_sidebarOpen;

        private void ViderRecherche()
        {
            _recherche = string.Empty;
            _nomRecherche = null;
            _derniereRecherche = null;
            _resultats = new();
        }

        /// <summary>Lancer la recherche quand l'utilisateur appuie sur Entrée.</summary>
        private async Task OnKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(_recherche))
                await LancerRecherche();
        }

        /// <summary>
        /// Lance la recherche de prix.
        /// Pour l'instant : données mock simulant un appel Python.
        /// Plus tard : appel HTTP vers le service Python de scraping.
        /// </summary>
        private async Task LancerRecherche()
        {
            if (string.IsNullOrWhiteSpace(_recherche)) return;

            _nomRecherche = _recherche.Trim();
            _derniereRecherche = _nomRecherche;
            _chargement = true;
            _resultats = new();
            _filtreActif = "prix";
            StateHasChanged();

            // ──────────────────────────────────────────────────────
            // TODO : Remplacer ce bloc mock par l'appel à l'API Python
            //
            // Exemple d'intégration future :
            //   var response = await Http.GetFromJsonAsync<List<ResultatScraping>>(
            //       $"http://localhost:8000/scrape?q={Uri.EscapeDataString(_nomRecherche)}");
            //   _resultats = response ?? new();
            //
            // ──────────────────────────────────────────────────────
            await Task.Delay(1800); // Simule la latence du scraping

            _resultats = GenererResultatsMock(_nomRecherche);

            _chargement = false;
            StateHasChanged();
        }

        /// <summary>
        /// Génère des données mock réalistes pour tester l'UI
        /// avant que l'API Python soit branchée.
        /// </summary>
        private static List<ResultatScraping> GenererResultatsMock(string produit)
        {
            return new List<ResultatScraping>
            {
                new() {
                    Site       = "Mytek",
                    NomProduit = produit,
                    Prix       = 299.000m,
                    EnStock    = true,
                    Livraison  = "Livraison en 24/48h",
                    Garantie   = "Garantie 1 an Mytek",
                    Url        = "https://www.mytek.tn"
                },
                new() {
                    Site       = "Tunisianet",
                    NomProduit = produit,
                    Prix       = 315.000m,
                    EnStock    = false,
                    Livraison  = "Indisponible",
                    Garantie   = "Garantie 1 an Tunisianet",
                    Url        = "https://www.tunisianet.com.tn"
                },
                new() {
                    Site       = "Electrotounes",
                    NomProduit = produit,
                    Prix       = 289.000m,
                    EnStock    = true,
                    Livraison  = "Livraison express (24h)",
                    Garantie   = "Garantie 1 an ET",
                    Url        = "https://www.electrotounes.tn"
                },
                new() {
                    Site       = "Wiki",
                    NomProduit = produit,
                    Prix       = 305.000m,
                    EnStock    = true,
                    Livraison  = "Livraison 3-5 jours",
                    Garantie   = "Garantie 6 mois",
                    Url        = "https://www.wiki.tn"
                },
                new() {
                    Site       = "SpaceNet",
                    NomProduit = produit,
                    Prix       = 320.000m,
                    EnStock    = false,
                    Livraison  = "Indisponible",
                    Garantie   = "Garantie 1 an SpaceNet",
                    Url        = "https://www.spacenet.tn"
                }
            };
        }

        // ── Export CSV ──────────────────────────────────────────

        /// <summary>
        /// Exporte les résultats en CSV téléchargeable (sans frais de port).
        /// </summary>
        private async Task ExporterCsv()
        {
            if (!_resultats.Any()) return;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Site;Produit;Prix (DT);Disponibilité;Lien");

                foreach (var r in _resultats)
                {
                    sb.AppendLine(
                        $"{r.Site};" +
                        $"{r.NomProduit.Replace(";", ",")};" +
                        $"{r.Prix:N3};" +
                        $"{(r.EnStock ? "En stock" : "Rupture")};" +
                        $"{r.Url}");
                }

                var bytes = System.Text.Encoding.UTF8.GetPreamble()
                             .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                             .ToArray();
                var base64 = Convert.ToBase64String(bytes);
                var nom = $"scraping-{_nomRecherche?.Replace(" ", "-")}-{DateTime.Now:yyyyMMdd}.csv";

                await JS.InvokeVoidAsync("eval", $@"
                    (function(){{
                        var a = document.createElement('a');
                        a.href = 'data:text/csv;base64,{base64}';
                        a.download = '{nom}';
                        document.body.appendChild(a);
                        a.click();
                        document.body.removeChild(a);
                    }})();
                ");
                AfficherToast("Export CSV téléchargé.", "ws-toast-success");
            }
            catch (Exception ex)
            {
                AfficherToast($"Erreur export : {ex.Message}", "ws-toast-error");
            }
        }

        // ── Helpers ─────────────────────────────────────────────

        private static string Nettoyer(string v)
        {
            v = v.Trim();
            if (v.Length >= 2 &&
                ((v.StartsWith('"') && v.EndsWith('"')) ||
                 (v.StartsWith('\'') && v.EndsWith('\''))))
                v = v[1..^1].Trim();
            return v;
        }

        private async void AfficherToast(string msg, string type)
        {
            _toastMsg = msg;
            _toastType = type;
            StateHasChanged();
            await Task.Delay(3500);
            _toastMsg = string.Empty;
            StateHasChanged();
        }
    }
}