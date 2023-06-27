using System;
using System.Threading.Tasks;

namespace advisor;

public enum FutureState {
    Pending,
    Completed,
    Faulted
}

public abstract class Future<T> {
    public abstract Task<Result<T>> Result { get; }

    public abstract FutureState State { get; }
    public abstract Exception Fault { get; }

    public bool IsCompleted => State == FutureState.Completed;
    public bool IsPending => State == FutureState.Pending;
    public bool IsFaulted => State == FutureState.Faulted;

    public class Sync : Future<T> {
        public Sync(Func<Result<T>> func) {
            this.func = func;
        }

        private FutureState state = FutureState.Pending;
        private Exception fault;
        private Task<Result<T>> value;

        private readonly Func<Result<T>> func;

        public override FutureState State => state;

        public override Exception Fault => fault;

        public override Task<Result<T>> Result {
            get {
                switch (State) {
                    case FutureState.Pending:
                        try {
                            value = Task.FromResult(func());
                        }
                        catch (Exception ex) {
                            fault = ex;
                        }

                        return value;

                    case FutureState.Completed:
                        return value;

                    default:
                        return Task.FromResult(Failure<T>(Fault));
                }
            }
        }
    }

    public class Async : Future<T> {
        public Async(Func<Task<Result<T>>> func) {
            this.func = func;
        }

        private FutureState state = FutureState.Pending;

        private readonly Func<Task<Result<T>>> func;

        public override FutureState State => state;

        public override Task<Result<T>> Result => throw new NotImplementedException();

        public override Exception Fault => throw new NotImplementedException();

        public Task<Result<T>> Run() => func();
    }
}
