using Grow.Domain.Commons;

namespace Grow.Infrastructure.Auth;

public class AuthUserSessionProvider(UserSessionProvider sessionProvider) : IAuthUserSessionProvider
{
    public AuthUser Get() => sessionProvider.GetSession().ToAuthUser();
}
