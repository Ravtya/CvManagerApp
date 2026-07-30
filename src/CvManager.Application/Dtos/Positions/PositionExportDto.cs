namespace CvManager.Application.Dtos.Positions;

public record PositionTokenDto(int PositionId, string PositionTitle, string ApiToken);

public record PositionExportDto(
    int PositionId,
    string PositionTitle,
    int CvCount,
    IReadOnlyList<PositionAttributeExportDto> Attributes);

public record PositionAttributeExportDto(
    int AttributeDefinitionId,
    string Title,
    string Type,
    int FilledCount,
    IReadOnlyDictionary<string, string> Stats);
