namespace advisor;

using System;

public static class MystcraftExtensions
{
    public static Mystcraft<B> Bind<A, B>(this Mystcraft<A> ma, Func<A, Mystcraft<B>> f) => ma switch {
        Mystcraft<A>.Return rt                => f(rt.Value),

        Mystcraft<A>.Create cr                => new Mystcraft<B>.Create(cr.Name, cr.Engine, cr.Ruleset, cr.Map, cr.Schedule, cr.Period, n => cr.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyGames gm         => new Mystcraft<B>.ReadManyGames(gm.Predicate, n => gm.Next(n).Bind(f)),
        Mystcraft<A>.ReadOneGame og           => new Mystcraft<B>.ReadOneGame(og.Game, n => og.Next(n).Bind(f)),
        Mystcraft<A>.WriteOneGame wg          => new Mystcraft<B>.WriteOneGame(wg.Game, n => wg.Next(n).Bind(f)),
        Mystcraft<A>.Start st                 => new Mystcraft<B>.Start(st.Game, n => st.Next(n).Bind(f)),
        Mystcraft<A>.Pause ps                 => new Mystcraft<B>.Pause(ps.Game, n => ps.Next(n).Bind(f)),
        Mystcraft<A>.Lock lk                  => new Mystcraft<B>.Lock(lk.Game, n => lk.Next(n).Bind(f)),
        Mystcraft<A>.Stop sp                  => new Mystcraft<B>.Stop(sp.Game, n => sp.Next(n).Bind(f)),
        Mystcraft<A>.Delete dl                => new Mystcraft<B>.Delete(dl.Game, n => dl.Next(n).Bind(f)),
        Mystcraft<A>.ReadOptions ro           => new Mystcraft<B>.ReadOptions(ro.Game, n => ro.Next(n).Bind(f)),
        Mystcraft<A>.WriteSchedule ws         => new Mystcraft<B>.WriteSchedule(ws.Game, ws.Schedule, n => ws.Next(n).Bind(f)),
        Mystcraft<A>.WriteMap wm              => new Mystcraft<B>.WriteMap(wm.Game, wm.Map, n => wm.Next(n).Bind(f)),
        Mystcraft<A>.WritRuleset wr           => new Mystcraft<B>.WritRuleset(wr.Game, wr.Ruleset, n => wr.Next(n).Bind(f)),
        Mystcraft<A>.WritEngine we            => new Mystcraft<B>.WritEngine(we.Game, we.Engine, n => we.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyRegistrations rg => new Mystcraft<B>.ReadManyRegistrations(rg.Game, rg.Predicate, n => rg.Next(n).Bind(f)),
        Mystcraft<A>.ReadOneRegistration or   => new Mystcraft<B>.ReadOneRegistration(or.Registration, n => or.Next(n).Bind(f)),
        Mystcraft<A>.RegisterPlayer rp        => new Mystcraft<B>.RegisterPlayer(rp.Game, rp.Name, rp.Password, n => rp.Next(n).Bind(f)),
        Mystcraft<A>.RemoveRegistration rr    => new Mystcraft<B>.RemoveRegistration(rr.Registration, n => rr.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyPlayers pl       => new Mystcraft<B>.ReadManyPlayers(pl.Game, pl.IncludeQuit, pl.Predicate, n => pl.Next(n).Bind(f)),
        Mystcraft<A>.ReadOnePlayer op         => new Mystcraft<B>.ReadOnePlayer(op.Player, n => op.Next(n).Bind(f)),
        Mystcraft<A>.QuitPlayer qp            => new Mystcraft<B>.QuitPlayer(qp.Player, n => qp.Next(n).Bind(f)),
        Mystcraft<A>.RunTurn rt               => new Mystcraft<B>.RunTurn(rt.Game, n => rt.Next(n).Bind(f)),
        Mystcraft<A>.ParseReport pr           => new Mystcraft<B>.ParseReport(pr.Report, n => pr.Next(n).Bind(f)),

        _ => throw new NotSupportedException()
    };


    public static Mystcraft<B> Map<A, B>(this Mystcraft<A> ma, Func<A, B> f) =>
        ma.Bind(a => Mystcraft.Return(f(a)));

    public static Mystcraft<B> Select<A, B>(this Mystcraft<A> ma, Func<A, B> f) =>
        ma.Bind(a => Mystcraft.Return(f(a)));

    public static Mystcraft<C> SelectMany<A, B, C>(this Mystcraft<A> ma, Func<A, Mystcraft<B>> bind, Func<A, B, C> project) =>
        ma.Bind(a => bind(a).Select(b => project(a, b)));
}
