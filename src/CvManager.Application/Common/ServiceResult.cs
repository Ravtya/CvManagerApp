namespace CvManager.Application.Common;

public class ServiceResult<T>
{
    public T? Value { get; private init; }
    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyList<ServiceError> Errors { get; private init; } = [];

    public bool HasCode(string code) => Errors.Any(e => e.Code == code);

    public static ServiceResult<T> Ok(T value) => new() { Value = value };
    public static ServiceResult<T> Fail(ServiceError error) => new() { Errors = [error] };

    public static ServiceResult<T> FailCode(string code, string? field = null) =>
        Fail(field is null ? ServiceError.CodeOnly(code) : ServiceError.FieldError(field, code));
}