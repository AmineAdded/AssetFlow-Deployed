namespace AssetFlow.Application.Interfaces
{
    public interface ITeamsDemandeAchatNotifier
    {
        Task NotifierNouvelleDemandeAsync(int demandeId);
    }
}