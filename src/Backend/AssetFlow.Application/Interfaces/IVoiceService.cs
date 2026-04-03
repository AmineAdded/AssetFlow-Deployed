using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface IVoiceService
    {
        /// <summary>Transcrit un audio base64 via Voxtral Mini</summary>
        Task<string> TranscrireAsync(string audioBase64, string mimeType);

        /// <summary>Analyse l'intention via Mistral Small</summary>
        Task<ParseIntentResponse> ParseIntentAsync(string transcript, string role);

        /// <summary>Pipeline complet : transcription + NLU</summary>
        Task<VoiceCommandResponse> ProcessAsync(VoiceCommandRequest request);
    }
}