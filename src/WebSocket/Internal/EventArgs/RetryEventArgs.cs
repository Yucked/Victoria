namespace Victoria.WebSocket.Internal.EventArgs; 

/// <summary>
/// 
/// </summary>
/// <param name="Count"></param>
/// <param name="IsLastRetry"></param>
public readonly record struct RetryEventArgs(int Count, bool IsLastRetry);