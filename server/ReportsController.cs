namespace advisor {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Newtonsoft.Json;
    using MediatR;
    using HotChocolate.Types.Relay;
    using advisor.Features;
    using advisor.Persistence;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    [Route("api")]
    public class ReportsController : ControllerBase {
        public ReportsController(IAuthorizationService authorization,IIdSerializer relayId, IMediator mediator, Database database) {
            this.authorization = authorization;
            this.relayId = relayId;
            this.mediator = mediator;
            this.database = database;
        }

        private readonly IAuthorizationService authorization;
        private readonly IIdSerializer relayId;
        private readonly IMediator mediator;
        private readonly Database database;

        [HttpPost("{playerId}/report")]
        public async Task<IActionResult> UploadReports([Required, FromRoute] string playerId) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (Request.Form.Files.Count == 0) return UnprocessableEntity();

            var relayPlayerId = relayId.Deserialize(playerId);
            if (relayPlayerId.TypeName != "Player") return BadRequest();

            var playerIdValue = (long) relayPlayerId.Value;

            if (! await authorization.AuthorizeOwnPlayer(User, playerIdValue)) return Unauthorized();

            List<string> reports = new List<string>();
            foreach (var file in Request.Form.Files) {
                await using var stream = file.OpenReadStream();
                using var textReader = new StreamReader(stream);
                reports.Add(await textReader.ReadToEndAsync());
            }

            var earliestTurn = await mediator.Send(new UploadReports(playerIdValue, reports));
            await mediator.Send(new ProcessTurn(playerIdValue, earliestTurn));

            return Ok();
        }

        [HttpGet("{playerId}/report/{turnNumber}/{factionNumber}")]
        [Authorize(Policy = Policies.GameMasters)]
        public async Task<IActionResult> UploadRuleset([Required, FromRoute] string playerId, [Required, FromRoute] int turnNumber, [Required, FromRoute] int factionNumber) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var relayPlayerId = relayId.Deserialize(playerId);
            if (relayPlayerId.TypeName != "Player") return BadRequest();

            var playerIdValue = (long) relayPlayerId.Value;

            var report = await database.Reports
                .Include(x => x.Turn)
                .Where(x => x.PlayerId == playerIdValue && x.Turn.Number == turnNumber && x.FactionNumber == factionNumber)
                .Select(x => x.Source)
                .SingleOrDefaultAsync();

            if (report == null) return NotFound();

            return Content(report, "text");
        }

        [HttpPost("{playerId}/map")]
        public async Task<IActionResult> UploadMap([Required, FromRoute] string playerId) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (Request.Form.Files.Count == 0) return UnprocessableEntity();

            var relayPlayerId = relayId.Deserialize(playerId);
            if (relayPlayerId.TypeName != "Player") return BadRequest();

            var playerIdValue = (long) relayPlayerId.Value;

            if (! await authorization.AuthorizeOwnPlayer(User, playerIdValue)) return Unauthorized();

            await using var stream = Request.Form.Files[0].OpenReadStream();
            using var textReader = new StreamReader(stream);
            string map = await textReader.ReadToEndAsync();

            await mediator.Send(new ImportMap(playerIdValue, map));

            return Ok();
        }

        [HttpPost("{gameId}/ruleset")]
        [Authorize(Policy = Policies.GameMasters)]
        public async Task<IActionResult> UploadRuleset([Required, FromRoute] string gameId) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (Request.Form.Files.Count != 1) return UnprocessableEntity();

            var relayGameId = relayId.Deserialize(gameId);
            if (relayGameId.TypeName != "Game") return BadRequest();

            var gameIdValue = (long) relayGameId.Value;

            var game = await database.Games.FindAsync(gameIdValue);
            if (game == null) return NotFound();

            var file = Request.Form.Files[0];

            await using var stream = file.OpenReadStream();
            using var textReader = new StreamReader(stream);

            game.Ruleset = await textReader.ReadToEndAsync();

            await database.SaveChangesAsync();

            return Ok();
        }
    }
}
