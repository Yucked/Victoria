namespace Victoria.Enums;

/// <summary>
/// severity of the exception
/// </summary>
public enum ExceptionSeverity {
    /// <summary>
    /// Cause is known and expected, indicates that there is nothing wrong with the library itself
    /// </summary>
    COMMON,

    /// <summary>
    /// Cause might not be exactly known, but is possibly caused by outside factors.
    /// For example when an outside service responds in a format that we do not expect
    /// </summary>
    SUSPICIOUS,

    /// <summary>
    /// If the probable cause is an issue with the library or when there is no way to tell what the cause might be.
    /// This is the default level and other levels are used in cases where the thrower has more in-depth knowledge about the error
    /// </summary>
    FATAL
}