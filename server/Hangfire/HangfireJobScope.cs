namespace advisor
{
    using System;
    using Hangfire;
    using Microsoft.Extensions.DependencyInjection;

    public class HangfireJobScope : JobActivatorScope {
        public HangfireJobScope(IServiceScope scope) {
            this.scope = scope;
        }

        private readonly IServiceScope scope;

        public override object Resolve(Type type) {
            return scope.ServiceProvider.GetRequiredService(type);
        }

        public override void DisposeScope() {
            scope.Dispose();
        }
    }
}
