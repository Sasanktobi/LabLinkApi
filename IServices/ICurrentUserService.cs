namespace Backend.IServices
{
    // Identity of the caller, read from the JWT claims of the current request.
    public interface ICurrentUserService
    {
        int? UserId{get;}
        string? Role{get;}
        bool IsInRole(params string[] roles);

        // UserId, or ForbiddenException when the request is anonymous.
        int RequireUserId();
    }
}
