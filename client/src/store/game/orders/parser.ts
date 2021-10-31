import * as P from 'parsimmon'

export class UnitNumber {
    constructor (public readonly num: number, public readonly isAlias = false, public readonly faction: number | null = null) { }
}

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

export class OrderDestroy {
    readonly order = 'destroy'
}

export class OrderAddress {
    constructor (public readonly address: string) { }
    readonly order = 'address'
}

export class OrderForm {
    constructor (public readonly alias: number) { }
    readonly order = 'form'
}

export class OrderEndForm {
    readonly order = 'end'
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

export class OrderName {
    constructor (public readonly target: string, public readonly name: string) { }
    readonly order = 'name'
}

export class OrderDescribe {
    constructor (public readonly target: string, public readonly description: string) { }
    readonly order = 'describe'
}

export class OrderMove {
    constructor (public readonly path: string[]) { }
    readonly order = 'move'
}

export class OrderAdvance {
    constructor (public readonly path: string[]) { }
    readonly order = 'advance'
}

export class OrderSail {
    constructor (public readonly path: string[]) { }
    readonly order = 'sail'
}

export class OrderPromote {
    constructor (public readonly unit: UnitNumber) { }
    readonly order = 'promote'
}

export class OrderEvict {
    constructor (public readonly unit: UnitNumber) { }
    readonly order = 'promote'
}

export class OrderEnter {
    constructor (public readonly building: number) { }
    readonly order = 'enter'
}

export class OrderAttack {
    constructor (public readonly unit: UnitNumber) { }
    readonly order = 'attack'
}

export class OrderAssassinate {
    constructor (public readonly unit: UnitNumber) { }
    readonly order = 'assassinate'
}

export class OrderSteal {
    constructor (public readonly unit: UnitNumber, public readonly item: string) { }
    readonly order = 'steal'
}

export type Order = OrderTurn | OrderEndTurn | OrderLeave | OrderPillage | OrderTax | OrderEntertain | OrderWork | OrderDestroy
    | OrderAddress | OrderForm | OrderEndForm | OrderArmor | OrderWeapon | OrderAutotax | OrderAvoid | OrderBehind | OrderHold | OrderNoAid
    | OrderShare | OrderNoCross | OrderGuard | OrderConsume | OrderReveal | OrderSpoils | OrderPrepare | OrderCombat | OrderName
    | OrderDescribe | OrderMove | OrderAdvance | OrderSail | OrderPromote | OrderEvict | OrderEnter | OrderAttack | OrderAssassinate
    | OrderSteal

export class RepeatOrder {
    constructor (public readonly other: Order) { }
    readonly order = '@'
}

export function createOrderParser() {
    const language = P.createLanguage({
        _:           () => P.regex(/\s*/).desc('whitepsace'),
        __:          () => P.regex(/\s+/).desc('whitepsace'),
        Quote:       () => P.string('"'),
        Direction:   () => P.regex(/nw|ne|sw|se|n|s/i).desc('move direction'),
        Num:         () => P.regex(/[0-9]+/).map(s => parseInt(s)).desc('number'),
        Repeat:      () => P.string('@').desc('@'),
        All:         () => P.regex(/all/i).desc('all'),
        ItemClass:   () => P.regex(/normal|advanced|trade|man|men|monster|monsters|magic|weapon|weapons|armor|mount|mounts|battle|special|tool|tools|food|ship|ships|item|items/i).desc('item class'),
        Bool:        () => P.regex(/0|1/).map(s => s === '1').desc('0 or 1'),
        Comment:     () => P.string(';').desc(';'),
        WordToken:   () => P.regex(/[\w]+/),
        NameTarget:  () => P.regex(/unit|faction|object|city/i).desc('unit, faction, object or city'),
        Flag:        () => P.regex(/autotax|avoid|behind|hold|noaid|share|nocross|guard/i).desc('autotax, avoid, behind, hold, noaid, share, nocross or guard'),
        New:         () => P.regex(/new/i),
        Except:      () => P.regex(/except/i),
        Attitude:    () => P.regex(/ally|friendly|neutral|unfriendly|hostile/i),
        FactionType: () => P.regex(/war|trade|magic/i),

        SpoilsType: r => P.alt(
            r.All,
            P.regex(/none|walk|ride|fly|swim|sail/i)
        ).desc('none, walk, ride, fly, swim, sailnone, walk, ride, fly, swim, sail or all'),

        // each command will end with the line or with a comment
        EndCommand: r => P.alt(
            P.end
                .desc('end of line'),
            r._
                .then(r.Comment)
                .then(P.takeWhile(() => true)).desc('comment')
        ),

        QuotedToken: r => r.Quote.then(P.takeWhile(x => r.Quote.parse(x).status).skip(r.Quote)),
        Token:       r => P.alt(r.WordToken, r.QuotedToken),

        TokenItem: r => P.seq(r.__, r.Token),
        TokenList: r => r.TokenItem.atLeast(1),

        NumItem: r => P.seq(r.__, r.Num),
        NumList: r => r.NumItem.atLeast(1),

        MoveItem: r => r.__.then(r.Direction),

        ContextFlag: r => P.alt(
            r.__.then(P.regex(/unit|faction/i)),
            P.succeed('')
        ).desc('unit, faction or nothing'),

        UnitAlias: r => r.New
            .then(r.__)
            .then(r.Num),

        FactionAlias: r => P.regex(/faction/i)
            .then(r.__)
            .then(r.Num),

        UnitNumber: r => P.alt(
            r.Num
                .map(num => new UnitNumber(num)),
            r.UnitAlias
                .map(num => new UnitNumber(num, true)),
            P.seq(
                r.FactionAlias.skip(r.__),
                r.UnitAlias
            ).map(([ faction, unit ]) => new UnitNumber(unit, true, faction))
        ),

        UnitListItem: r => r.__
            .then(r.UnitNumber),

        UnitList: r => r.UnitListItem.atLeast(1),

        FactionPointItem: r => r.__.then(P.seq(r.FactionType.skip(r.__), r.Num)),

        FactionPointList: r => r.FactionPointItem.atLeast(1),

        //////

        OSimpleOrders: r => P.regex(/turn|leave|pillage|tax|entertain|work|destroy/i)
            .skip(r.EndCommand)
            .map(order => {
                switch (order.toLowerCase()) {
                    case 'turn': return new OrderTurn()
                    case 'leave': return new OrderLeave()
                    case 'pillage': return new OrderPillage()
                    case 'tax': return new OrderTax()
                    case 'entertain': return new OrderEntertain()
                    case 'work': return new OrderWork()
                    case 'destroy': return new OrderDestroy()
                }
            }),

        OEndTurn: r => P.regex(/endturn/i)
            .skip(r.EndCommand)
            .map(() => new OrderEndTurn()),

        OAddress: r => P.regex(/address/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(address => new OrderAddress(address)),

        OForm: r => P.regex(/form/i)
            .then(r.__)
            .then(r.Num)
            .skip(r.EndCommand)
            .map(alias => new OrderForm(alias)),

        OEndForm: r => P.regex(/end/i)
            .skip(r.EndCommand)
            .map(() => new OrderEndForm()),

        OArmor: r => P.regex(/armor/i)
            .then(r.TokenList)
            .skip(r.EndCommand)
            .map(items => new OrderArmor(items)),

        OWeapon: r => P.regex(/weapon/i)
            .then(r.TokenList)
            .skip(r.EndCommand)
            .map(items => new OrderWeapon(items)),

        OFlag: r => r.Flag
            .chain(flag => r.__
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

        OConsume: r => P.regex(/consume/i)
            .then(r.ContextFlag)
            .skip(r.EndCommand)
            .map(flag => new OrderConsume(flag || 'silver')),

        OReveal: r => P.regex(/reveal/i)
            .then(r.ContextFlag)
            .skip(r.EndCommand)
            .map(flag => new OrderReveal(flag || 'none')),

        OSpoils: r => P.regex(/spoils/i)
            .then(P.alt(
                r.__.then(r.SpoilsType),
                P.succeed('all')
            ))
            .skip(r.EndCommand)
            .map(type => new OrderSpoils(type)),

        OPrepare: r => P.regex(/prepare/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(item => new OrderPrepare(item)),

        OCombat: r => P.regex(/combat/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(spell => new OrderCombat(spell)),

        OName: r => P.regex(/name/i)
            .then(r.__)
            .then(r.NameTarget)
            .chain(target => r.TokenItem
                .map(name => new OrderName(target, name)
            ))
            .skip(r.EndCommand),

        ODescribe: r => P.regex(/describe/i)
            .then(r.__)
            .then(r.NameTarget)
            .chain(target => r.TokenItem
                .map(description => new OrderDescribe(target, description)
            ))
            .skip(r.EndCommand),

        OMove: r => P.regex(/move/i)
            .then(r.MoveItem.atLeast(1))
            .skip(r.EndCommand)
            .map(path => new OrderMove(path)),

        OAdvance: r => P.regex(/advance/i)
            .then(r.MoveItem.atLeast(1))
            .skip(r.EndCommand)
            .map(path => new OrderAdvance(path)),

        OSail: r => P.regex(/sail/i)
            .then(r.MoveItem.many())
            .skip(r.EndCommand)
            .map(path => new OrderSail(path)),

        OPromote: r => P.regex(/promote/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.EndCommand)
            .map(unit => new OrderPromote(unit)),

        OEvict: r => P.regex(/evict/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.EndCommand)
            .map(unit => new OrderEvict(unit)),

        OEnter: r => P.regex(/enter/i)
            .then(r.__)
            .then(r.Num)
            .skip(r.EndCommand)
            .map(building => new OrderEnter(building)),

        OAttack: r => P.regex(/attack/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.EndCommand)
            .map(unit => new OrderAttack(unit)),

        OAssassinate: r => P.regex(/assassinate/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.EndCommand)
            .map(unit => new OrderAssassinate(unit)),

        OSteal: r => P.regex(/steal/i)
            .then(r.__)
            .then(P.seq(r.UnitNumber.skip(r._), r.Token))
            .map(([ unit, item ]) => new OrderSteal(unit, item)),

        // TEACH [unit] ...
        OTeach: r => P.regex(/teach/i)
            .then(r.__)
            .then(r.UnitList)
            .skip(r.EndCommand),

        // WITHDRAW [item]
        // WITHDRAW [quantity] [item]
        OWithdraw: r => P.regex(/withdraw/i)
            .then(r.__)
            .then(P.alt(
                r.Num
                    .chain(quantity => r.TokenItem),
                r.Token
            ))
            .skip(r.EndCommand),

        // CLAIM [amount]
        OClaim: r => P.regex(/claim/i)
            .then(r.__)
            .then(r.Num)
            .skip(r.EndCommand),

        // STUDY [skill]
        // STUDY [skill] [level]
        OStudy: r => P.regex(/study/i)
            .then(P.alt(
                r.TokenItem
                    .chain(skill => r.__.then(r.Num)),
                r.TokenItem
            ))
            .skip(r.EndCommand),

        // PRODUCE [item]
        // PRODUCE [number] [item]
        OProduce: r => P.regex(/produce/i)
            .then(r.__)
            .then(P.alt(
                r.Num.chain(number => r.TokenItem),
                r.Token
            ))
            .skip(r.EndCommand),

        // BUILD
        // BUILD [object type]
        // BUILD HELP [unit]
        OBuild: r => P.regex(/build/i)
            .then(r.__)
            .then(P.alt(
                P.regex(/help/i)
                    .then(r.__)
                    .then(r.UnitNumber),
                r.Token,
                P.succeed(null)
            ))
            .skip(r.EndCommand),

        // TRANSPORT [unit] [num] [item]
        // TRANSPORT [unit] ALL [item]
        // TRANSPORT [unit] ALL [item] EXCEPT [amount]
        OTransport: r => P.regex(/transport/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.__)
            .chain(unit => P.alt(
                P.seq(r.All, r.TokenItem, r.__.then(r.Except).then(r.__).then(r.Num)),
                P.seq(r.All, r.TokenItem),
                P.seq(r.Num, r.TokenItem),
            ))
            .skip(r.EndCommand),

        // DISTRIBUTE [unit] [num] [item]
        // DISTRIBUTE [unit] ALL [item]
        // DISTRIBUTE [unit] ALL [item] EXCEPT [amount]
        ODistribute: r => P.regex(/distribute/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.__)
            .chain(unit => P.alt(
                P.seq(r.All, r.TokenItem, r.__.then(r.Except).then(r.__).then(r.Num)),
                P.seq(r.All, r.TokenItem),
                P.seq(r.Num, r.TokenItem),
            ))
            .skip(r.EndCommand),

        // DECLARE [faction] [attitude]
        // DECLARE [faction]
        // DECLARE DEFAULT [attitude]
        ODeclare: r => P.regex(/declare/i)
            .then(r.__)
            .then(P.alt(
                P.regex(/default/i).then(r.__).then(r.Attitude),
                P.seq(r.Num, r.__.then(r.Attitude)),
                r.Num
            ))
            .skip(r.EndCommand),

        // FACTION [type] [points] ...
        OFaction: r => P.regex(/faction/i)
            .then(r.__)
            .then(r.FactionPointList)
            .skip(r.EndCommand),

        // GIVE [unit] [quantity] [item]
        // GIVE [unit] ALL [item]
        // GIVE [unit] ALL [item] EXCEPT [quantity]
        // GIVE [unit] ALL [item class]
        // GIVE [unit] UNIT
        OGive: r => P.regex(/give/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.__)
            .chain(unit => P.alt(
                P.regex(/unit/i),
                r.All
                    .then(r.__)
                    .then(P.alt(
                        P.seq(r.Token, r.__.then(r.Except).then(r.Num)),
                        r.ItemClass,
                        r.Token
                    )),
                P.seq(r.Num.skip(r.__), r.Token)
            ))
            .skip(r.EndCommand),

        // TAKE FROM [unit] [quantity] [item]
        // TAKE FROM [unit] ALL [item]
        // TAKE FROM [unit] ALL [item] EXCEPT [quantity]
        // TAKE FROM [unit] ALL [item class]
        OTake: r => P.regex(/take/i)
            .then(r.__)
            .then(P.regex(/from/i))
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.__)
            .chain(unit => P.alt(
                P.regex(/unit/i),
                r.All
                    .then(r.__)
                    .then(P.alt(
                        P.seq(r.Token, r.__.then(r.Except).then(r.Num)),
                        r.ItemClass,
                        r.Token
                    )),
                P.seq(r.Num.skip(r.__), r.Token)
            ))
            .skip(r.EndCommand),

        //JOIN [unit]
        //JOIN [unit] NOOVERLOAD
        //JOIN [unit] MERGE
        OJoin: r => P.regex(/join/i)
            .then(r.__)
            .then(r.UnitNumber)
            .skip(r.__)
            .chain(unit => P.alt(
                P.regex(/merge/i),
                P.regex(/nooverload/i),
                P.succeed(null)
            ))
            .skip(r.EndCommand),

        // CAST [skill] [arguments]
        OCast: r => P.regex(/cast/i)
            .then(P.seq(
                r.TokenItem,
                r.TokenItem.many()
            ))
            .skip(r.EndCommand),

        // SELL [quantity] [item]
        // SELL ALL [item]
        OSell: r => P.regex(/sell/i)
            .then(r.__)
            .then(P.alt(
                r.All.then(r.TokenItem),
                P.seq(r.Num, r.TokenItem)
            ))
            .skip(r.EndCommand),

        // BUY [quantity] [item]
        // BUY ALL [item]
        OBuy: r => P.regex(/buy/i)
            .then(r.__)
            .then(P.alt(
                r.All.then(r.TokenItem),
                P.seq(r.Num, r.TokenItem)
            ))
            .skip(r.EndCommand),

        // FORGET [skill]
        OForget: r => P.regex(/forget/i)
            .then(r.TokenItem)
            .skip(r.EndCommand),

        // OPTION TIMES
        // OPTION NOTIMES
        // OPTION SHOWATTITUDES
        // OPTION DONTSHOWATTITUDES
        // OPTION TEMPLATE OFF
        // OPTION TEMPLATE SHORT
        // OPTION TEMPLATE LONG
        // OPTION TEMPLATE MAP
        OOption: r => P.regex(/option/i)
            .then(r.__)
            .then(P.alt(
                P.regex(/times/i),
                P.regex(/notimes/i),
                P.regex(/showattitudes/i),
                P.regex(/dontshowattitudes/i),
                P.regex(/template/i).then(P.alt(
                    P.regex(/off/i),
                    P.regex(/short/i),
                    P.regex(/long/i),
                    P.regex(/map/i),
                )),
            ))
            .skip(r.EndCommand),

        // PASSWORD [password]
        // PASSWORD
        OPassword: r => P.regex(/password/i)
            .then(P.alt(
                r.TokenItem.skip(r.EndCommand),
                r.EndCommand
            )),

        // SHOW SKILL [skill] [level]
        // SHOW ITEM [item]
        // SHOW OBJECT [object]
        OShow: r => P.regex(/show/i)
            .then(r.__)
            .then(P.alt(
                P.regex(/skill/i).then(P.seq(r.TokenItem, r.TokenItem)),
                P.regex(/item/i).then(r.TokenItem),
                P.regex(/object/i).then(r.TokenItem)
            ))
            .skip(r.EndCommand),

        // EXCHANGE [unit] [quantity given] [item given] [quantity expected] [item expected]
        OExchange: r => P.regex(/exchange/i)
            .then(r.__)
            .then(P.seq(
                r.UnitNumber,
                r.__.then(r.Num),
                r.TokenItem,
                r.__.then(r.Num),
                r.TokenItem
            ))
            .skip(r.EndCommand),

        // QUIT [password]
        OQuit: r => P.regex(/quit/i)
            .then(r.TokenItem)
            .skip(r.EndCommand),

        // RESTART [password]
        ORestart: r => P.regex(/restart/i)
            .then(r.TokenItem)
            .skip(r.EndCommand),

        RepeatableOrder: r => P.alt(
            r.OSimpleOrders,
            r.OArmor,
            r.OWeapon,
            r.OFlag,
            r.OConsume,
            r.OReveal,
            r.OSpoils,
            r.OPrepare,
            r.OCombat,
            r.OName,
            r.ODescribe,
            r.OMove,
            r.OAdvance,
            r.OSail,
            r.OPromote,
            r.OEvict,
            r.OEnter,
            r.OAttack,
            r.OAssassinate,
            r.OSteal,
            r.OTeach,
            r.OWithdraw,
            r.OClaim,
            r.OStudy,
            r.OProduce,
            r.OBuild,
            r.OTransport,
            r.ODistribute,
            r.ODeclare,
            r.OFaction,
            r.OGive,
            r.OTake,
            r.OJoin,
            r.OCast,
            r.OSell,
            r.OBuy,
            r.OForget,
            r.OOption,
            r.OShow,
            r.OExchange
        ),

        Order: r => P.alt(
            r.EndCommand.result(null),
            P.alt(
                r.Repeat.then(r.RepeatableOrder).map(order => new RepeatOrder(order)),
                r.RepeatableOrder,
                r.OEndTurn,
                r.OAddress,
                r.OForm,
                r.OEndForm,
                r.OPassword,
                r.OQuit,
                r.ORestart,
            )
        )
    })

    return (line: string) => language.Order.parse(line) as P.Result<Order | RepeatOrder>
}
