namespace advisor
{
    using System.Threading;
    using System.Threading.Tasks;
    using HotChocolate.AspNetCore;
    using HotChocolate.Execution;
    using Microsoft.AspNetCore.Http;

    public class GraphQLHttpRequestInterceptor : DefaultHttpRequestInterceptor
    {
        public override ValueTask OnCreateAsync(HttpContext context,
            IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder,
            CancellationToken cancellationToken)
        {
            if (!context.User.Identity.IsAuthenticated) {
                return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
            }

            var userId = context.User.FindFirst(WellKnownClaimTypes.UserId)?.Value;
            if (userId == null) {
                return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
            }

            requestBuilder.SetProperty("currentUserId", long.Parse(userId));
            requestBuilder.SetProperty("currentUserEmail", context.User.FindFirst(WellKnownClaimTypes.Email)?.Value);

            return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
        }
    }
}
