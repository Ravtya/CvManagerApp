namespace CvManager.Domain.Entities;

public class CvLike
{
    public int CvId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public Cv Cv { get; set; } = null!;
}
