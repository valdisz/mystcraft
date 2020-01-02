namespace atlantis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public interface IReportSectionParser {
        Task<bool> CanParseAsync(Cursor<TextParser> cursor);
        Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer);
    }
}
