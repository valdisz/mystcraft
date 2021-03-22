namespace advisor {
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    public class DefaultFilesMiddleware {
        public DefaultFilesMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv) {
            this.next = next;
            this.hostingEnv = hostingEnv;
        }

        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment hostingEnv;

        private readonly Regex filePattern = new Regex(@"\.\w+$");

        public Task Invoke(HttpContext context) {
            var method = context.Request.Method.ToLowerInvariant();
            if (method != "get" && method != "head") return next(context);

            var path = context.Request.Path;
            if (path.StartsWithSegments("/graphql")) return next(context);
            if (path.StartsWithSegments("/report")) return next(context);
            if (path.StartsWithSegments("/login")) return next(context);
            if (path.StartsWithSegments("/register")) return next(context);
            if (path.StartsWithSegments("/hangfire")) return next(context);

            if (!path.HasValue || !IsFile(path)) {
                context.Request.Path = new PathString("/index.html");
                return next(context);
            }

            return next(context);
        }

        public bool IsFile(string path) {
            var lastSlash = path.LastIndexOf("/");
            var match = lastSlash >= 0
                ? filePattern.IsMatch(path.Substring(lastSlash))
                : filePattern.IsMatch(path);

            return match;
        }
    }
}
