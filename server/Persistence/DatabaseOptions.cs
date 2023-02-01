namespace advisor.Persistence;

public class DatabaseOptions {
    public string ConnectionString { get; set; }
    public DatabaseProvider Provider { get; set; }
    public bool IsProduction { get; set; }
}
