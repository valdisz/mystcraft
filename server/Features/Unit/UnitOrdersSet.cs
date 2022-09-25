namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UnitOrdersSet(long PlayerId, int TurnNumber, int UnitNumber, string Orders): IRequest<UnitOrdersSetResult>;

public record UnitOrdersSetResult(bool IsSuccess, string Error = null) : MutationResult(IsSuccess, Error);


public class UnitOrdersSetHandler : IRequestHandler<UnitOrdersSet, UnitOrdersSetResult> {
    public UnitOrdersSetHandler(IUnitOfWork unit) {
        this.db = unit.Database;
    }

    private readonly Database db;

    public async Task<UnitOrdersSetResult> Handle(UnitOrdersSet request, CancellationToken cancellationToken) {
        var orders = await db.Orders
            .InTurn(request.PlayerId, request.TurnNumber)
            .SingleOrDefaultAsync(x => x.UnitNumber == request.UnitNumber);

        if (orders == null) {
            return new UnitOrdersSetResult(false, "Unit not found");
        }

        orders.Orders = request.Orders;

        await db.SaveChangesAsync();

        return new UnitOrdersSetResult(true);
    }
}
