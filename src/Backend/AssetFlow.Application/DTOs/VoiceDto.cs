namespace AssetFlow.Application.DTOs
{
    // ── Requête transcription ──────────────────────────────────
    public record TranscribeRequest(string AudioBase64, string MimeType);

    // ── Requête NLU ────────────────────────────────────────────
    public record ParseIntentRequest(string Transcript, string Role);

    // ── Réponse NLU ────────────────────────────────────────────
    public record ParseIntentResponse(
        string  Intent,
        string? NavigateTo,
        string? Reference,
        string? Designation
    );

    // ── Requête combinée (transcription + NLU en une passe) ────
    public record VoiceCommandRequest(string AudioBase64, string MimeType, string Role);

    // ── Réponse combinée ───────────────────────────────────────
    public record VoiceCommandResponse(
        string  Transcript,
        string  Intent,
        string? NavigateTo,
        string? Reference,
        string? Designation,
        string? Error
    );
}