namespace ScopeTrack.Application
{
  public sealed record Result<T>
  {
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
      if (isSuccess && value is null)
        throw new InvalidOperationException(
          "Success result must have a value."
        );

      if (!isSuccess && error is null)
        throw new InvalidOperationException(
          "Failure result must have an error."
        );

      IsSuccess = isSuccess;
      Value = value;
      Error = error;
    }

    public static Result<T> Success(T value)
      => new(true, value, null);

    public static Result<T> Failure(string error)
      => new(false, default, error);
  }
}
