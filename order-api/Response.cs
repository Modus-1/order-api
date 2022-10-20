namespace order_api;

public class Response<TData> where TData : class?
{
    
    public TData? Data { get; init; }
    public bool Successful { get; init; } = true;
    public string Message { get; init; } = string.Empty;

}

public class Response
{
    public bool Successful { get; init; } = true;
    public string Message { get; init; } = string.Empty;
}