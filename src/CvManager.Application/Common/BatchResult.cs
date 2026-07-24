namespace CvManager.Application.Common;

public class BatchResult
{
    public int TotalCount { get; private init; }
    public int SuccessCount { get; private init; }
    public bool HasSuccess => SuccessCount > 0;
    public bool IsFullSuccess => SuccessCount == TotalCount && Errors.Count == 0;
    public IReadOnlyList<ServiceError> Errors { get; private init; } = [];

    public static BatchResult FromCounts(int total, int success, IReadOnlyList<ServiceError> errors) =>
        new() { TotalCount = total, SuccessCount = success, Errors = errors };

    public static BatchResult FailCode(string code) => new() { Errors = [ServiceError.CodeOnly(code)] };
}