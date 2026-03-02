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
// LIAISON PYTHON :
//   La méthode LancerRecherche() appelle le service Python
//   via HttpClient sur http://localhost:5000/scrape?q=...
// ============================================================

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Pages.Achat
{
    public partial class WebScraping : ComponentBase
    {
        // ── Injection ───────────────────────────────────────────
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        // ── Modèle d'un résultat de scraping ────────────────────
        /// <summary>
        /// Représente un résultat renvoyé par le scraper Python.
        /// Sera mappé depuis le JSON de l'API Python.
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

        // ── Classes pour désérialiser la réponse JSON du script Python ──
        private class ReponseScraping
        {
            public bool succes { get; set; }
            public string article { get; set; } = string.Empty;
            public string date_recherche { get; set; } = string.Empty;
            public int nombre_resultats { get; set; }
            public List<ResultatPython> resultats { get; set; } = new();
            public MeilleurPrix? meilleur_prix { get; set; }
            public Recommandation? recommandation { get; set; }
        }

        private class ResultatPython
        {
            public string site { get; set; } = string.Empty;
            public string nom_produit { get; set; } = string.Empty;
            public decimal prix { get; set; }
            public string devise { get; set; } = string.Empty;
            public string stock { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string date_scraping { get; set; } = string.Empty;
        }

        private class MeilleurPrix
        {
            public string site { get; set; } = string.Empty;
            public string nom_produit { get; set; } = string.Empty;
            public decimal prix { get; set; }
            public string stock { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
        }

        private class Recommandation
        {
            public string? site { get; set; }
            public decimal? prix { get; set; }
            public string? url { get; set; }
            public string message { get; set; } = string.Empty;
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
        private string? _nomRecherche = null;     // dernier terme soumis
        private string? _derniereRecherche = null; // null = pas encore cherché
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
        /// Appelle le service Python via HTTP sur http://localhost:5000/scrape?q=...
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

            try
            {
                // Appel à l'API Python Flask
                var url = $"http://localhost:5000/scrape?q={Uri.EscapeDataString(_nomRecherche)}";
                var reponse = await Http.GetFromJsonAsync<ReponseScraping>(url);

                if (reponse?.succes == true && reponse.resultats != null && reponse.resultats.Any())
                {
                    _resultats = reponse.resultats.Select(r => new ResultatScraping
                    {
                        Site = r.site,
                        NomProduit = r.nom_produit,
                        Prix = r.prix,
                        // Adapter selon le texte exact de 'stock' (ex: "En stock", "Épuisé", "Non indiqué")
                        EnStock = r.stock?.Contains("stock", StringComparison.OrdinalIgnoreCase) == true ||
                                  r.stock?.Contains("En stock", StringComparison.OrdinalIgnoreCase) == true,
                        Livraison = "Non précisé", // Le script ne fournit pas encore ces infos
                        Garantie = "Non précisée", // Le script ne fournit pas encore ces infos
                        Url = r.url
                    }).ToList();

                    AfficherToast($"{_resultats.Count} résultat(s) trouvé(s)", "ws-toast-success");
                }
                else
                {
                    _resultats = new();
                    AfficherToast("Aucun résultat trouvé", "ws-toast-warning");
                }
            }
            catch (HttpRequestException)
            {
                AfficherToast("Impossible de contacter le service Python. Vérifiez que le serveur est lancé sur http://localhost:5000", "ws-toast-error");
            }
            catch (Exception ex)
            {
                AfficherToast($"Erreur : {ex.Message}", "ws-toast-error");
            }
            finally
            {
                _chargement = false;
                StateHasChanged();
            }
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