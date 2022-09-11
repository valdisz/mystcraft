namespace advisor
{
    using System;
    using Hangfire;
    using Microsoft.Extensions.DependencyInjection;

    public class HangfireContainerActivator : JobActivator {
        public HangfireContainerActivator(IServiceProvider services) {
            this.services = services;
        }

        private readonly IServiceProvider services;

        public override JobActivatorScope BeginScope(JobActivatorContext context) {
            var scope = services.CreateScope();
            return new HangfireJobScope(scope);
        }
    }
}
