namespace Application.Common.Exceptions
{
    /// <summary>
    /// The caller is authenticated but lacks permission for the requested
    /// operation. Surfaces as HTTP 403 (not 401) so the SPA shows an
    /// "access denied" message instead of bouncing the user to sign-in.
    /// </summary>
    public sealed class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
