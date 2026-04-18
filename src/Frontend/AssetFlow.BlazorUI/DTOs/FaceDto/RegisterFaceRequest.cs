namespace AssetFlow.BlazorUI.DTOs
{
    public class RegisterFaceRequest
    {
        public string Email      { get; set; } = string.Empty;
        public float[][] Keypoints { get; set; } = Array.Empty<float[]>();
    }
}