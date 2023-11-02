namespace advisor.Model;

using System.IO;

public record struct FactionFile(FactionNumber Number, FileInfo Contents);
