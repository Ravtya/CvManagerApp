using CvManager.Application.Validation;
using CvManager.Domain;

namespace CvManager.Application.Dtos.Profile;

public class ProjectDto
{
    public int Id { get; set; }

    [LocalizedMaxLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [LocalizedMaxLength(FieldLengths.Text)]
    public string Description { get; set; } = string.Empty;

    [LocalizedMaxLength(FieldLengths.TagsString)]
    public string TagsString { get; set; } = string.Empty;
}
