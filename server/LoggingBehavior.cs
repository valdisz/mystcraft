namespace advisor;

using System;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) {
        this.logger = logger;
    }

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next) {
        var triggerName = typeof(TRequest).Name;

        logger.LogInformation("Starting [{Trigger}]", triggerName);
        try {
            var response = await next();
            logger.LogInformation("Executed [{Trigger}] --> [{Output}]", triggerName, typeof(TResponse).Name);

            return response;
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed [{Trigger}] --> {Exception}", triggerName, ex.GetType().Name);

            throw;
        }
    }
}
