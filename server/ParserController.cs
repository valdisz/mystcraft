namespace advisor;

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[AllowAnonymous]
[Route("api/parser")]
public class ParserController : ControllerBase {
    [HttpGet]
    public IActionResult Index() {
        return Content($@"<html>
<head>
    <title>Parser</title>
</head>
<body>

<form action=""{Url.Action(nameof(ParseAsync))}"" method=""post"" enctype=""multipart/form-data"">
    <!-- file -->
    <div>
        <label for=""file"">File</label>
        <input type=""file"" name=""file"" id=""file"" />
    </div>

    <!-- submit -->
    <div>
        <button type=""submit"">Parse</button>
    </div>
</form>

</body>
</html>
", "text/html");
    }

    [HttpPost]
    public async Task<IActionResult> ParseAsync([FromForm(Name = "file")] IFormFile file) {
        if (file == null || file.Length == 0) {
            return BadRequest("No file was uploaded.");
        }

        // Read the file contents
        using var reader = new StreamReader(file.OpenReadStream());

        using var converter = new AtlantisReportJsonConverter(reader);

        Response.ContentType = "application/json";
        using var writer = new JsonTextWriter(new StreamWriter(Response.Body)) {
            Formatting = Formatting.Indented
        };

        await converter.ReadAsJsonAsync(writer);

        return new EmptyResult();
    }
}

