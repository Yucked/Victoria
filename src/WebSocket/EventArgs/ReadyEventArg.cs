namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
/// <param name="IsResumed"></param>
/// <param name="SessionId"></param>
public readonly record struct ReadyEventArg(bool IsResumed, string SessionId);