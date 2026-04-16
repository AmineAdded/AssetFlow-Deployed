namespace AssetFlow.BlazorUI.DTOs
{
    public class MaterielResultDto
    {
        public bool   Succes    { get; set; }
        public string Message   { get; set; } = string.Empty;
        public int?   IdMateriel { get; set; }
    }
}