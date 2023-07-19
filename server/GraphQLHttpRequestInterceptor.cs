namespace advisor;

using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

public class GraphQLHttpRequestInterceptor : DefaultHttpRequestInterceptor {
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        IQueryRequestBuilder requestBuilder,
        CancellationToken cancellationToken) {

        var userId = context.User.Identity.IsAuthenticated
            ? context.User.FindFirst(WellKnownClaimTypes.USER_ID)?.Value
            : null;

        if (userId != null) {
            requestBuilder.SetProperty("currentUserId", long.Parse(userId));
            requestBuilder.SetProperty("currentUserEmail", context.User.FindFirst(WellKnownClaimTypes.EMAIL)?.Value);
        }

        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
