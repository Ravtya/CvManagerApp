namespace CvManager.Application.Common;

public class ServiceError
{
    public string Code { get; private init; } = string.Empty;
    public string? Label { get; private init; }
    public string? Field { get; private init; }

    public static ServiceError CodeOnly(string code) => new() { Code = code };
    public static ServiceError FieldError(string field, string code) => new() { Field = field, Code = code };
    public static ServiceError ItemError(string label, string code) => new() { Label = label, Code = code };
}