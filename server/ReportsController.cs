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

    [Authorize]
    [Route("report")]
    public class ReportsController : ControllerBase {
        public ReportsController(IAuthorizationService authorization,IIdSerializer relayId, IMediator mediator) {
            this.authorization = authorization;
            this.relayId = relayId;
            this.mediator = mediator;
        }

        private readonly IAuthorizationService authorization;
        private readonly IIdSerializer relayId;
        private readonly IMediator mediator;

        [HttpPost("{playerId}")]
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
            await mediator.Send(new ParseReports(playerIdValue, earliestTurn));
            await mediator.Send(new SetupStudyPlans(playerIdValue));

            return Ok();
        }
    }
}
