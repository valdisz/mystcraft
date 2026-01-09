namespace advisor.IO;

public interface HasUnix<RT>
    where RT : struct, HasUnix<RT> {

    Eff<RT, Traits.UnixIO> UnixEff { get; }
}
