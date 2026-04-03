namespace AssetFlow.BlazorUI.DTOs
{
   public record VoiceCommandResponse(
        string  Transcript,
        string  Intent,
        string? NavigateTo,
        string? Reference,
        string? Designation,
        string? Error
    );
}