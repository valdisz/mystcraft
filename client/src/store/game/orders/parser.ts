import * as P from 'parsimmon'
import { Direction } from '../../../schema'

export class OrderTurn {
    readonly order = 'turn'
}

export class OrderEndTurn {
    readonly order = 'endturn'
}

export class OrderLeave {
    readonly order = 'leave'
}

export class OrderPillage {
    readonly order = 'pillage'
}

export class OrderTax {
    readonly order = 'tax'
}

export class OrderEntertain {
    readonly order = 'entertain'
}

export class OrderWork {
    readonly order = 'work'
}

export class OrderAddress {
    constructor (public readonly address: string) { }
    readonly order = 'address'
}

export class OrderForm {
    constructor (public readonly alias: number) { }
    readonly order = 'form'
}

export class OrderArmor {
    constructor (public readonly items: string[]) { }
    readonly order = 'armor'
}

export class OrderWeapon {
    constructor (public readonly items: string[]) { }
    readonly order = 'weapon'
}

export class OrderAutotax {
    constructor (public readonly set: boolean) { }
    readonly order = 'autotax'
}

export class OrderAvoid {
    constructor (public readonly set: boolean) { }
    readonly order = 'autotax'
}

export class OrderBehind {
    constructor (public readonly set: boolean) { }
    readonly order = 'behind'
}

export class OrderHold {
    constructor (public readonly set: boolean) { }
    readonly order = 'hold'
}

export class OrderNoAid {
    constructor (public readonly set: boolean) { }
    readonly order = 'noaid'
}

export class OrderShare {
    constructor (public readonly set: boolean) { }
    readonly order = 'share'
}

export class OrderNoCross {
    constructor (public readonly set: boolean) { }
    readonly order = 'nocross'
}

export class OrderGuard {
    constructor (public readonly set: boolean) { }
    readonly order = 'guard'
}

export class OrderConsume {
    constructor (public readonly scope: string) { }
    readonly order = 'consume'
}

export class OrderReveal {
    constructor (public readonly scope: string) { }
    readonly order = 'reveal'
}

export class OrderSpoils {
    constructor (public readonly type: string) { }
    readonly order = 'spoils'
}

export class OrderPrepare {
    constructor (public readonly item: string) { }
    readonly order = 'prepare'
}

export class OrderCombat {
    constructor (public readonly spell: string) { }
    readonly order = 'combat'
}

export type Order = OrderTurn | OrderEndTurn | OrderLeave | OrderPillage | OrderTax | OrderEntertain | OrderWork | OrderAddress | OrderForm
    | OrderArmor | OrderWeapon | OrderAutotax | OrderAvoid | OrderBehind | OrderHold | OrderNoAid | OrderShare | OrderNoCross | OrderGuard
    | OrderConsume | OrderReveal | OrderSpoils | OrderPrepare | OrderCombat

export function createOrders() {
    return P.createLanguage({
        _:         () => P.regex(/\s*/).desc('whitepsace'),
        __:        () => P.regex(/\s+/).desc('whitepsace'),
        Quote:     () => P.string('"'),
        Direction: () => P.regex(/nw|ne|sw|se|n|s/i).desc('move direction'),
        Num:       () => P.regex(/[0-9]+/).map(s => parseInt(s)).desc('number'),
        Repeat:    () => P.string('@').desc('@'),
        All:       () => P.regex(/all/i).desc('all'),
        ItemClass: () => P.regex(/normal|advanced|trade|man|men|monster|monsters|magic|weapon|weapons|armor|mount|mounts|battle|special|tool|tools|food|ship|ships|item|items/i).desc('item class'),
        Bool:      () => P.regex(/0|1/).map(s => s === '1').desc('0 or 1'),
        Comment:   () => P.string(';').desc(';'),

        // each command will end with the line or with a comment
        EndCommand: r => P.alt(
            P.end
                .desc('end of line'),
            r._
                .then(r.Comment)
                .then(P.takeWhile(() => true)).desc('comment')
        ),

        QuotedToken: r => r.Quote.then(P.takeWhile(x => r.Quote.parse(x).status).skip(r.Quote)),
        WordToken:  () => P.regex(/[\w]+/),
        Token:       r => P.alt(r.WordToken, r.QuotedToken),

        TokenItem: r => P.seq(r.__, r.Token),
        TokenList: r => r.TokenItem.atLeast(1),

        NumItem: r => P.seq(r.__, r.Num),
        NumList: r => r.NumItem.atLeast(1),

        SimpleOrder: r => P.regex(/turn|leave|pillage|tax|entertain|work/i)
            .skip(r.EndCommand)
            .map(order => {
                switch (order.toLowerCase()) {
                    case 'turn': return new OrderTurn()
                    case 'leave': return new OrderLeave()
                    case 'pillage': return new OrderPillage()
                    case 'tax': return new OrderTax()
                    case 'entertain': return new OrderEntertain()
                    case 'work': return new OrderWork()
                }
            }),

        OEndTurn: r => P.regex(/endturn/i)
            .skip(r.EndCommand)
            .map(() => new OrderEndTurn()),

        OAddress: r => P.regex(/address/i)
            .then(r.__)
            .then(r.Token)
            .skip(r.EndCommand)
            .map(address => new OrderAddress(address)),

        OForm: r => P.regex(/form/i)
            .then(r.__)
            .then(r.Num)
            .skip(r.EndCommand)
            .map(alias => new OrderForm(alias)),

        OArmor: r => P.regex(/armor/i)
            .then(r.TokenList)
            .skip(r.EndCommand)
            .map(items => new OrderArmor(items)),

        OWeapon: r => P.regex(/weapon/i)
            .then(r.TokenList)
            .skip(r.EndCommand)
            .map(items => new OrderWeapon(items)),

        OSimpleFlags: r => P.regex(/autotax|avoid|behind|hold|noaid|share|nocross|guard/i)
            .chain(flag => r
                .__
                .then(r.Bool)
                .skip(r.EndCommand)
                .map(set => {
                    switch (flag.toLowerCase()) {
                        case 'autotax': return new OrderAutotax(set)
                        case 'avoid': return new OrderAvoid(set)
                        case 'behind': return new OrderBehind(set)
                        case 'hold': return new OrderHold(set)
                        case 'noaid': return new OrderNoAid(set)
                        case 'share': return new OrderShare(set)
                        case 'nocross': return new OrderNoCross(set)
                        case 'guard': return new OrderGuard(set)
                    }
                })
            ),

        ContextFlag: r => P.alt(
            r.__.then(P.regex(/unit|faction/i)),
            P.succeed('')
        ).desc('unit, faction or nothing'),

        OConsume: r => P.regex(/consume/i)
            .then(r.ContextFlag)
            .skip(r.EndCommand)
            .map(flag => new OrderConsume(flag || 'silver')),

        OReveal: r => P.regex(/reveal/i)
            .then(r.ContextFlag)
            .skip(r.EndCommand)
            .map(flag => new OrderReveal(flag || 'none')),

        SpoilsType: r => P.alt(
            r.__.then(r.All),
            r.__.then(P.regex(/none|walk|ride|fly|swim|sail/i)),
            P.succeed('all')
        ).desc('spoils type'),

        OSpoils: r => P.regex(/spoils/i)
            .then(r.SpoilsType)
            .skip(r.EndCommand)
            .map(type => new OrderSpoils(type)),

        OPrepare: r => P.regex(/prepare/i)
            .then(r.__)
            .then(r.Token)
            .skip(r.EndCommand)
            .map(item => new OrderPrepare(item)),

        OCombat: r => P.regex(/combat/i)
            .then(r.__)
            .then(r.Token)
            .skip(r.EndCommand)
            .map(spell => new OrderCombat(spell)),

        OName: r => P.regex(/name/i),
        ODescribe: r => P.regex(/describe/i),

        OAdvance: r => P.regex(/advance/i),
        OMove: r => P.regex(/move/i),

        OSail: r => P.regex(/sail/i),

        OPromote: r => P.regex(/promote/i),
        OEvict: r => P.regex(/evict/i),
        OEnter: r => P.regex(/enter/i),
        OAttack: r => P.regex(/attack/i),
        OSteal: r => P.regex(/steal/i),
        OAssassinate: r => P.regex(/assassinate/i),

        OWithdraw: r => P.regex(/withdraw/i),
        OTeach: r => P.regex(/teach/i),
        OStudy: r => P.regex(/study/i),
        OProduce: r => P.regex(/produce/i),
        OBuild: r => P.regex(/build/i),
        OTransport: r => P.regex(/transport/i),
        ODistribute: r => P.regex(/distribute/i),
        OClaim: r => P.regex(/claim/i),
        ODeclare: r => P.regex(/declare/i),
        OFaction: r => P.regex(/faction/i),
        OGive: r => P.regex(/give/i),
        OTake: r => P.regex(/take/i),
        OJoin: r => P.regex(/join/i),
        ODestroy: r => P.regex(/destroy/i),
        OCast: r => P.regex(/cast/i),
        OSell: r => P.regex(/sell/i),
        OBuy: r => P.regex(/buy/i),
        OForget: r => P.regex(/forget/i),

        OOption: r => P.regex(/option/i),
        OPassword: r => P.regex(/password/i),
        OShow: r => P.regex(/show/i),
        OFind: r => P.regex(/find/i),
        OExchange: r => P.regex(/exchange/i),
        OQuit: r => P.regex(/quit/i),
        ORestart: r => P.regex(/restart/i),
    })
}
