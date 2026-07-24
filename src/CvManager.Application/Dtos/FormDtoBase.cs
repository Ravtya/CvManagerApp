namespace CvManager.Application.Dtos;

public abstract class FormDtoBase
{
    public int? Id { get; set; }
    public uint RowVersion { get; set; }
    public bool IsNew => !Id.HasValue;
}
