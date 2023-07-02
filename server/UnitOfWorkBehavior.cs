namespace advisor;

using System;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {
    public UnitOfWorkBehavior(IUnitOfWork unitOfWork) {
        this.unitOfWork = unitOfWork;
    }

    private readonly IUnitOfWork unitOfWork;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next) {
        return await next();
        // unitOfWork.BeginTransaction(cancellationToken)
        //     .Bind(() => AsyncEffect(() => next()))
        //     .Bind(value => unitOfWork.CommitTransaction().Return(value))
        //     .Select()

        //     .Bind(value => unitOfWork.CommitTransaction(cancellation).Return(value))
        //     .Select(onSuccess)
        //     .OnFailure(() => unitOfWork.RollbackTransaction(cancellation))
        //     .Run()
        //     .Unwrap(onFailure);

        // try {

        // }
        // var triggerName = typeof(TRequest).Name;

        // logger.LogInformation($"Starting [{triggerName}]");
        // try {
        //     var response = await next();
        //     logger.LogInformation($"Executed [{triggerName}] --> [{typeof(TResponse).Name}]");

        //     return response;
        // }
        // catch (Exception ex) {
        //     logger.LogError(ex, $"Failed [{triggerName}] --> {ex.GetType().Name}");

        //     throw;
        // }
    }
}
