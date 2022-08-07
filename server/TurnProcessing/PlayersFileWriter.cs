namespace advisor.TurnProcessing;

using System.IO;

public class PlayersFileWriter {
    public PlayersFileWriter(TextWriter writer) {
        this.writer = writer;
    }

    private readonly TextWriter writer;

    public void Write(FactionRecord rec) {
        WriteProp("Faction", rec.IsNew ? "new" : rec.Number.ToString());

        foreach (var (key, value) in rec.Props) {
            WriteProp(key, value);
        }
    }

    private void WriteProp(string name, string value) {
        writer.Write(name);
        writer.Write(':');

        if (!string.IsNullOrWhiteSpace(value)) {
            writer.Write(' ');
            writer.Write(value);
        }

        writer.WriteLine();
    }
}
