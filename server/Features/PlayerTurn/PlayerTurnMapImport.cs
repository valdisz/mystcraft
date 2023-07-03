// FIXME

namespace advisor.Features;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Model;
using advisor.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record PlayerTurnMapImport(long PlayerId, Stream Source) : IRequest;

// public class PlayerTurnMapImportHandler : IRequestHandler<PlayerTurnMapImport> {
//     public PlayerTurnMapImportHandler(IUnitOfWork unit, IMediator mediator) {
//         this.unit = unit;
//         this.mediator = mediator;
//         this.db = unit.Database;
//     }

//     private readonly IUnitOfWork unit;
//     private readonly IMediator mediator;
//     private readonly Database db;

//     public async Task<MediatR.Unit> Handle(PlayerTurnMapImport request, CancellationToken cancellationToken) {
//         DbPlayer player = await db.Players
//             .AsNoTracking()
//             .Include(x => x.Game)
//             .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

//         if (player == null) return Unit.Value;

//         using var buffer = new MemoryStream();
//         await request.Source.CopyToAsync(buffer);
//         buffer.Seek(0, SeekOrigin.Begin);

//         // using var textReader = new StreamReader(buffer);
//         // using var atlantisReader = new AtlantisReportJsonConverter(textReader,
//         //     new RegionsSection()
//         // );
//         // var json = await atlantisReader.ReadAsJsonAsync();

//         var rep = new DbAdditionalReport {
//             PlayerId = request.PlayerId,
//             TurnNumber = player.LastTurnNumber.Value,
//             Type = ReportType.Map,
//             // Json = Encoding.UTF8.GetBytes(json.ToString()),
//             Source = buffer.ToArray()
//         };

//         await db.AddAsync(rep);

//         return Unit.Value;
//     }
// }
