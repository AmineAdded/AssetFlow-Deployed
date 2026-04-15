public enum CircuitState { Closed, Open, HalfOpen }

public class ScraperCircuitBreakerService
{
    private CircuitState _etat = CircuitState.Closed;
    private int _nbEchecs = 0;
    private DateTime? _ouvertDepuis = null;

    // ── Rendu public pour que le composant puisse initialiser le countdown ──
    public const int TimeoutSecondes = 20;    // après 20s → test
    private const int SeuilEchecs = 3;        // 3 échecs → OUVERT

    public CircuitState Etat => _etat;

    /// <summary>Secondes restantes avant la prochaine tentative (0 si non applicable).</summary>
    public int SecondesRestantes
    {
        get
        {
            if (_etat != CircuitState.Open || _ouvertDepuis == null) return 0;
            var restant = TimeoutSecondes - (int)(DateTime.UtcNow - _ouvertDepuis.Value).TotalSeconds;
            return restant > 0 ? restant : 0;
        }
    }

    public bool PeutEnvoyerRequete()
    {
        if (_etat == CircuitState.Open)
        {
            // Après timeout → passer en HalfOpen pour tester
            if (DateTime.UtcNow - _ouvertDepuis > TimeSpan.FromSeconds(TimeoutSecondes))
            {
                _etat = CircuitState.HalfOpen;
                return true;  // la banière HalfOpen sera rendue avant la requête (via le delay dans le composant)
            }
            return false; // Bloqué
        }
        return true; // Closed ou HalfOpen → on essaie
    }

    /// <summary>
    /// Vérifie uniquement si le timeout est expiré et transite Open → HalfOpen,
    /// SANS autoriser la requête. Permet au composant de re-render la banière ambre d'abord.
    /// Retourne true si une transition vient d'avoir lieu.
    /// </summary>
    public bool VerifierTransitionHalfOpen()
    {
        if (_etat == CircuitState.Open &&
            DateTime.UtcNow - _ouvertDepuis > TimeSpan.FromSeconds(TimeoutSecondes))
        {
            _etat = CircuitState.HalfOpen;
            return true;
        }
        return false;
    }

    public void EnregistrerSucces()
    {
        _nbEchecs = 0;
        _etat = CircuitState.Closed;
        _ouvertDepuis = null;
    }

    public void EnregistrerEchec()
    {
        _nbEchecs++;
        if (_etat == CircuitState.HalfOpen || _nbEchecs >= SeuilEchecs)
        {
            _etat = CircuitState.Open;
            _ouvertDepuis = DateTime.UtcNow;
            _nbEchecs = 0;
        }
    }

    public string MessageUtilisateur => _etat switch
    {
        CircuitState.Open =>
            $"Le service de recherche est temporairement indisponible. " +
            $"Nouvelle tentative dans {SecondesRestantes} secondes.",
        CircuitState.HalfOpen =>
            "Tentative de reconnexion au service en cours…",
        _ => string.Empty
    };
}