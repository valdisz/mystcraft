namespace advisor.Features;

using System.Threading.Tasks;
using MediatR;

public class ReconcileJob {
    public ReconcileJob(IMediator mediator) {
        this.mediator = mediator;
    }

    private readonly IMediator mediator;

    public Task RunAsync() => mediator.Send(new Reconcile());
}

