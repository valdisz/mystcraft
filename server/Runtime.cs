namespace advisor;

using System;
using advisor.Persistence;
using LanguageExt.Effects.Traits;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

public readonly struct Runtime:
    HasCancel<Runtime>,
    HasDatabase<Runtime>,
    HasUnitOfWork<Runtime>
{
    readonly RuntimeEnv env;

    /// <summary>
    /// Constructor
    /// </summary>
    Runtime(RuntimeEnv env) => this.env = env;

    /// <summary>
    /// Configuration environment accessor
    /// </summary>
    public RuntimeEnv Env =>
        env ?? throw new InvalidOperationException("Runtime Env not set. Perhaps because of using default(Runtime) or new Runtime() rather than Runtime.New()");

    /// <summary>
    /// Constructor function
    /// </summary>
    public static Runtime New(Database database) =>
        new Runtime(new RuntimeEnv(database, new CancellationTokenSource()));

    /// <summary>
    /// Constructor function
    /// </summary>
    /// <param name="source">Cancellation token source</param>
    public static Runtime New(Database database, CancellationTokenSource source) =>
        new Runtime(new RuntimeEnv(database, source));

    public static Runtime New(Database database, CancellationToken linkedToken) =>
        new Runtime(new RuntimeEnv(database, CancellationTokenSource.CreateLinkedTokenSource(linkedToken)));

    /// <summary>
    /// Create a new Runtime with a fresh cancellation token
    /// </summary>
    /// <remarks>Used by localCancel to create new cancellation context for its sub-environment</remarks>
    /// <returns>New runtime</returns>
    public Runtime LocalCancel =>
        new Runtime(new RuntimeEnv(Env.Database, new CancellationTokenSource()));

    /// <summary>
    /// Direct access to cancellation token
    /// </summary>
    public CancellationToken CancellationToken =>
        Env.Token;

    /// <summary>
    /// Directly access the cancellation token source
    /// </summary>
    /// <returns>CancellationTokenSource</returns>
    public CancellationTokenSource CancellationTokenSource =>
        Env.Source;

    public Eff<Runtime, DatabaseIO> DatabaseEff =>
        SuccessEff<DatabaseIO>(Env.Database);

    public Eff<Runtime, UnitOfWorkIO> UnitOfWorkEff =>
        SuccessEff<UnitOfWorkIO>(Env.Database);
}

public sealed class RuntimeEnv {
    public RuntimeEnv(Database database, CancellationTokenSource source, CancellationToken token) {
        Source   = source;
        Token    = token;
        Database = database;
    }

    public RuntimeEnv(Database database, CancellationTokenSource source) : this(database, source, source.Token) {
    }

    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;
    public readonly Database Database;
}

public interface IRuntimeAccessor {
    Runtime Current { get; }
}

public sealed class ScopedRuntimeFactory : IRuntimeAccessor {
    public ScopedRuntimeFactory(Database database, IHttpContextAccessor httpAccessor) {
        runtime = new Lazy<Runtime>(() => Runtime.New(database, httpAccessor.HttpContext.RequestAborted));
    }

    private Lazy<Runtime> runtime;

    public Runtime Current => runtime.Value;
}

public static class RuntimeExtensions {
    public static IServiceCollection AddFunctionalRuntime(this IServiceCollection services) {
        services.AddScoped<IRuntimeAccessor, ScopedRuntimeFactory>();

        return services;
    }
}
