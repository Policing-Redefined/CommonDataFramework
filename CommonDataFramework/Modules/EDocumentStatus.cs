namespace CommonDataFramework.Modules;

/// <summary>
/// Specifies the different states of documents.
/// </summary>
public enum EDocumentStatus
{
    /// <summary>
    /// Document has no status.
    /// </summary>
    None,
    
    /// <summary>
    /// Document has been revoked.
    /// </summary>
    Revoked,
    
    /// <summary>
    /// Document is expired.
    /// </summary>
    Expired,
    
    /// <summary>
    /// Document is valid. 
    /// </summary>
    Valid
}