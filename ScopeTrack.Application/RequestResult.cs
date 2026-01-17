namespace ScopeTrack.Application
{
  public sealed record RequestResult<T>
  {
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private RequestResult(bool isSuccess, T? value, string? error)
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

    public static RequestResult<T> Success(T value)
      => new(true, value, null);

    public static RequestResult<T> Failure(string error)
      => new(false, default, error);
  }
}
