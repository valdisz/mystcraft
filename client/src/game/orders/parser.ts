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
    readonly order = 'avoid'
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
    constructor (public readonly path: (string|number)[]) { }
    readonly order = 'move'
}

export class OrderAdvance {
    constructor (public readonly path: (string|number)[]) { }
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
    readonly order = 'evict'
}

export class OrderEnter {
    constructor (public readonly building: number) { }
    readonly order = 'enter'
}

export class OrderAttack {
    constructor (public readonly units: UnitNumber[]) { }
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

export class OrderTeach {
    constructor (public readonly units: UnitNumber[]) { }
    readonly order = 'teach'
}

export class OrderWithdraw {
    constructor (public readonly quantity: number, public readonly item: string) { }
    readonly order = 'withdraw'
}

export class OrderClaim {
    constructor (public readonly amount: number) { }
    readonly order = 'claim'
}

export class OrderStudy {
    constructor (public readonly skill: string, public readonly level: number = null) { }
    readonly order = 'study'
}

export class OrderProduce {
    constructor (public readonly item: string, public readonly quantity: number = null) { }
    readonly order = 'produce'
}

export class Build {
    constructor (public readonly type: string) { }
    public readonly variant = 'default'
}

export class BuildContinue  {
    public readonly variant = 'continue'
}

export class BuildHelp  {
    constructor (public readonly unit: UnitNumber) { }
    public readonly variant = 'help'
}

export type BuilVariants = Build | BuildContinue | BuildHelp

export class OrderBuild {
    constructor (public readonly variant: BuilVariants) { }
    readonly order = 'build'
}

export class OrderTransport {
    constructor (
        public readonly unit: UnitNumber,
        public readonly quantity: number | 'all',
        public readonly item: string,
        public readonly except: number = null
    ) { }

    readonly order = 'transport'
}

export class OrderDistribute {
    constructor (
        public readonly unit: UnitNumber,
        public readonly quantity: number | 'all',
        public readonly item: string,
        public readonly except: number = null
    ) { }

    readonly order = 'distribute'
}

export class DeclareFaction {
    constructor (public readonly faction: number, public readonly attitude: string = null) { }
    readonly variant = 'faction'
}

export class DeclareDefault {
    constructor (public readonly attitude: string) { }
    readonly variant = 'default'
}

export type DeclareVariants = DeclareDefault | DeclareFaction

export class OrderDeclare {
    constructor (public readonly variant: DeclareVariants) { }
    readonly order = 'declare'
}

export class FactionSetting {
    constructor (public readonly type: string, public readonly points: number) { }
}

export class OrderFaction {
    constructor (public readonly settings: FactionSetting[]) { }
    readonly order = 'faction'
}

export class OrderGive {
    constructor (
        public readonly toUnit: UnitNumber,
        public readonly quantity: number | 'all' | 'unit',
        public readonly itemOrClass: string = null,
        public readonly except: number = null
    ) { }

    readonly order = 'give'
}

export class OrderTake {
    constructor (
        public readonly fromUnit: UnitNumber,
        public readonly quantity: number | 'all',
        public readonly itemOrClass: string,
        public readonly except: number = null
    ) { }

    readonly order = 'take'
}

export class OrderJoin {
    constructor (public readonly withUnit: UnitNumber, public readonly mode: string = null) { }

    readonly order = 'join'
}

export class OrderCast {
    constructor (public readonly skill: string, public readonly args: string[] = []) { }

    readonly order = 'cast'
}

export class OrderBuy {
    constructor (public readonly quantity: number | 'all', public readonly item: string) { }

    readonly order = 'buy'
}

export class OrderSell {
    constructor (public readonly quantity: number | 'all', public readonly item: string) { }

    readonly order = 'sell'
}

export class OrderForget {
    constructor (public readonly skill: string) { }

    readonly order = 'forget'

}

export class OrderOption {
    constructor (public readonly setting: string) { }

    readonly order = 'option'
}

export class OrderPassword {
    constructor (public readonly password: string) { }

    readonly order = 'password'
}

export class OrderQuit {
    constructor (public readonly password: string) { }

    readonly order = 'quit'
}

export class OrderRestart {
    constructor (public readonly password: string) { }

    readonly order = 'restart'
}

export class OrderShow {
    constructor (
        public readonly type: string,
        public readonly itemSkillObject: string,
        public readonly level: number = null
    ) { }

    readonly order = 'show'
}

export class OrderExchange {
    constructor (
        public readonly unit: UnitNumber,
        public readonly quantityGiven: number,
        public readonly itemGiven: string,
        public readonly quantityExpected: number,
        public readonly itemExpected: string
    ) { }

    readonly order = 'exchange'
}

export class OrderComment {
    constructor (public readonly text: string) { }
    readonly order = 'comment'
}

export class OrderUnknown {
    constructor (public readonly command: string, public readonly args: string = null) { }
    readonly order = 'unknown'
}

export type Order = OrderTurn | OrderEndTurn | OrderLeave | OrderPillage | OrderTax | OrderEntertain | OrderWork | OrderDestroy
    | OrderAddress | OrderForm | OrderEndForm | OrderArmor | OrderWeapon | OrderAutotax | OrderAvoid | OrderBehind | OrderHold | OrderNoAid
    | OrderShare | OrderNoCross | OrderGuard | OrderConsume | OrderReveal | OrderSpoils | OrderPrepare | OrderCombat | OrderName
    | OrderDescribe | OrderMove | OrderAdvance | OrderSail | OrderPromote | OrderEvict | OrderEnter | OrderAttack | OrderAssassinate
    | OrderSteal | OrderTeach | OrderWithdraw | OrderClaim | OrderStudy | OrderProduce | OrderBuild | OrderTransport | OrderDistribute
    | OrderDeclare | OrderFaction | OrderGive | OrderTake | OrderJoin | OrderCast | OrderBuy | OrderSell | OrderForget | OrderOption
    | OrderPassword | OrderQuit | OrderRestart | OrderShow | OrderExchange | OrderComment | OrderUnknown

export class RepeatOrder {
    constructor (public readonly other: Order) { }
    readonly order = '@'
}

export type UnitOrder = Order | RepeatOrder

export function createOrderParser() {
    const language = P.createLanguage({
        _:              () => P.regex(/\s*/).desc('whitepsace'),
        __:             () => P.regex(/\s+/).desc('whitepsace'),
        Quote:          () => P.string('"'),
        N:              () => P.regex(/n/i),
        NW:             () => P.regex(/nw/i),
        NE:             () => P.regex(/ne/i),
        S:              () => P.regex(/s/i),
        SW:             () => P.regex(/sw/i),
        SE:             () => P.regex(/se/i),
        IN:             () => P.regex(/in/i),
        OUT:            () => P.regex(/out/i),
        Num:            () => P.regex(/[0-9]+/).map(s => parseInt(s)).desc('number'),
        Repeat:         () => P.string('@').desc('@'),
        All:            () => P.regex(/all/i).desc('all'),
        // ItemClass:      () => P.regex(/normal|advanced|trade|man|men|monster|monsters|magic|weapon|weapons|armor|mount|mounts|battle|special|tool|tools|food|ship|ships|item|items/i).desc('item class'),
        Bool:           () => P.regex(/0|1/).map(s => s === '1').desc('0 or 1'),
        Comment:        () => P.string(';').desc(';'),
        WordToken:      () => P.regex(/[\w@!\?:_\-\.,]+/),
        NameTarget:     () => P.regex(/unit|faction|object|city/i).desc('unit, faction, object or city'),
        DescribeTarget: () => P.regex(/unit|ship|building|object|structure/i).desc('unit, ship, building, object or structure'),
        Flag:           () => P.regex(/autotax|avoid|behind|hold|noaid|share|nocross|guard/i).desc('autotax, avoid, behind, hold, noaid, share, nocross or guard'),
        New:            () => P.regex(/new/i),
        Except:         () => P.regex(/except/i),
        Attitude:       () => P.regex(/ally|friendly|neutral|unfriendly|hostile/i),

        MoveDirection: r => P.alt(r.NW, r.NE, r.SW, r.SE, r.N, r.S, r.IN, r.OUT, r.Num),
        SailDirection: r => P.alt(r.NW, r.NE, r.SW, r.SE, r.N, r.S),

        MoveItem: r => r.__.then(r.MoveDirection),
        SailItem: r => r.__.then(r.SailDirection),

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

        QuotedToken: r => r.Quote.then(P.takeWhile(x => !r.Quote.parse(x).status).skip(r.Quote)),
        Token:       r => P.alt(r.WordToken, r.QuotedToken).map(s => s.replace(/_/g, ' ')),

        TokenItem: r => r.__.then(r.Token),
        TokenList: r => r.TokenItem.atLeast(1),

        NumItem: r => r.__.then(r.Num),
        NumList: r => r.NumItem.atLeast(1),


        ContextFlag: r => P.alt(
            r.__.then(P.regex(/unit|faction/i)),
            P.succeed('')
        ).desc('unit, faction or nothing'),

        UnitAlias: r => r.New.then(r.NumItem),

        FactionAlias: r => P.regex(/faction/i).then(r.NumItem),

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

        UnitItem: r => r.__.then(r.UnitNumber),
        UnitList: r => r.UnitItem.atLeast(1),

        FactionSettingItem: r => P.seq(r.TokenItem, r.NumItem)
            .map(([ type, points ]) => new FactionSetting(type, points)),

        FactionSettingList: r => r.FactionSettingItem.atLeast(1),

        ExceptNum: r => r.__.then(r.Except).then(r.NumItem),

        ItemClassToken: r => r.__.then(r.ItemClass),

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
            .then(r.NumItem)
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
            .then(P.seq(r.__.then(r.NameTarget), r.TokenItem))
            .skip(r.EndCommand)
            .map(([ target, name ]) => new OrderName(target, name)),

        ODescribe: r => P.regex(/describe/i)
            .then(r.__)
            .then(P.alt(
                P.seq(r.DescribeTarget, r.TokenItem),
                P.seq(r.DescribeTarget, P.succeed(null))
            ))
            .skip(r.EndCommand)
            .map(([target, description]) => new OrderDescribe(target, description)),

        OMove: r => P.regex(/move/i)
            .then(r.MoveItem.atLeast(1))
            .skip(r.EndCommand)
            .map(path => new OrderMove(path)),

        OAdvance: r => P.regex(/advance/i)
            .then(r.MoveItem.atLeast(1))
            .skip(r.EndCommand)
            .map(path => new OrderAdvance(path)),

        OSail: r => P.regex(/sail/i)
            .then(r.SailItem.many())
            .skip(r.EndCommand)
            .map(path => new OrderSail(path)),

        OPromote: r => P.regex(/promote/i)
            .then(r.UnitItem)
            .skip(r.EndCommand)
            .map(unit => new OrderPromote(unit)),

        OEvict: r => P.regex(/evict/i)
            .then(r.UnitItem)
            .skip(r.EndCommand)
            .map(unit => new OrderEvict(unit)),

        OEnter: r => P.regex(/enter/i)
            .then(r.NumItem)
            .skip(r.EndCommand)
            .map(building => new OrderEnter(building)),

        OAttack: r => P.regex(/attack/i)
            .then(r.UnitList)
            .skip(r.EndCommand)
            .map(units => new OrderAttack(units)),

        OAssassinate: r => P.regex(/assassinate/i)
            .then(r.UnitItem)
            .skip(r.EndCommand)
            .map(unit => new OrderAssassinate(unit)),

        OSteal: r => P.regex(/steal/i)
            .then(P.seq(r.UnitItem, r.TokenItem))
            .skip(r.EndCommand)
            .map(([ unit, item ]) => new OrderSteal(unit, item)),

        // TEACH [unit] ...
        OTeach: r => P.regex(/teach/i)
            .then(r.UnitList)
            .skip(r.EndCommand)
            .map(units => new OrderTeach(units)),

        // WITHDRAW [item]
        // WITHDRAW [quantity] [item]
        OWithdraw: r => P.regex(/withdraw/i)
            .then(r.__)
            .then(P.alt(
                P.seq(r.Num, r.TokenItem),
                P.seq(P.succeed(1), r.Token)
            ))
            .skip(r.EndCommand)
            .map(([ quantity, item ]) => new OrderWithdraw(quantity, item)),

        // CLAIM [amount]
        OClaim: r => P.regex(/claim/i)
            .then(r.NumItem)
            .skip(r.EndCommand)
            .map(amount => new OrderClaim(amount)),

        // STUDY [skill]
        // STUDY [skill] [level]
        OStudy: r => P.regex(/study/i)
            .then(P.alt(
                P.seq(r.TokenItem, r.NumItem),
                P.seq(r.TokenItem, P.succeed(null))
            ))
            .skip(r.EndCommand)
            .map(([ skill, level ]) => new OrderStudy(skill, level)),

        // PRODUCE [item]
        // PRODUCE [number] [item]
        OProduce: r => P.regex(/produce/i)
            .then(r.__)
            .then(P.alt(
                P.seq(r.Num, r.TokenItem),
                P.seq(P.succeed(null), r.Token)
            ))
            .skip(r.EndCommand)
            .map(([ quantity, item ]) => new OrderProduce(item, quantity)),

        // BUILD
        // BUILD [object type]
        // BUILD HELP [unit]
        OBuild: r => P.regex(/build/i)
            .then(P.alt(
                r.__
                    .then(P.regex(/help/i))
                    .then(r.UnitItem)
                    .map(unit => new OrderBuild(new BuildHelp(unit))),
                r.TokenItem
                    .map(type => new OrderBuild(new Build(type))),
                P.succeed(new OrderBuild(new BuildContinue()))
            ))
            .skip(r.EndCommand),

        // TRANSPORT [unit] [num] [item]
        // TRANSPORT [unit] ALL [item]
        // TRANSPORT [unit] ALL [item] EXCEPT [amount]
        OTransport: r => P.regex(/transport/i)
            .then(r.UnitItem)
            .skip(r.__)
            .chain(unit => P
                .alt(
                    P.seq(r.All, r.TokenItem, r.ExceptNum),
                    P.seq(r.All, r.TokenItem, P.succeed(null)),
                    P.seq(r.Num, r.TokenItem, P.succeed(null))
                )
                .map(([ quantity, item, except ]) => new OrderTransport(unit, quantity, item, except))
            )
            .skip(r.EndCommand),

        // DISTRIBUTE [unit] [num] [item]
        // DISTRIBUTE [unit] ALL [item]
        // DISTRIBUTE [unit] ALL [item] EXCEPT [amount]
        ODistribute: r => P.regex(/distribute/i)
            .then(r.UnitItem)
            .skip(r.__)
            .chain(unit => P
                .alt(
                    P.seq(r.All, r.TokenItem, r.ExceptNum),
                    P.seq(r.All, r.TokenItem, P.succeed(null)),
                    P.seq(r.Num, r.TokenItem, P.succeed(null))
                )
                .map(([ quantity, item, except ]) => new OrderDistribute(unit, quantity, item, except))
            )
            .skip(r.EndCommand),

        // DECLARE [faction] [attitude]
        // DECLARE [faction]
        // DECLARE DEFAULT [attitude]
        ODeclare: r => P.regex(/declare/i)
            .then(r.__)
            .then(P.alt(
                P.regex(/default/i)
                    .then(r.__)
                    .then(r.Attitude)
                    .map(attitude => new OrderDeclare(new DeclareDefault(attitude))),
                P.seq(r.Num, r.__.then(r.Attitude))
                    .map(([ faction, attitude ]) => new OrderDeclare(new DeclareFaction(faction, attitude))),
                r.Num
                    .map(faction => new OrderDeclare(new DeclareFaction(faction)))
            ))
            .skip(r.EndCommand),

        // FACTION [type] [points] ...
        OFaction: r => P.regex(/faction/i)
            .then(r.FactionSettingList)
            .skip(r.EndCommand)
            .map(type => new OrderFaction(type)),

        // GIVE [unit] UNIT
        // GIVE [unit] ALL [item] EXCEPT [quantity]
        // GIVE [unit] ALL [item class]
        // GIVE [unit] ALL [item]
        // GIVE [unit] [quantity] [item]
        OGive: r => P.regex(/give/i)
            .then(r.UnitItem)
            .chain(unit => P.alt(
                r.__.then(P.regex(/unit/i))
                    .map(() => new OrderGive(unit, 'unit')),
                r.__.then(P.seq(r.All, r.TokenItem, r.ExceptNum))
                    .map(([ quantity, item, except ]) => new OrderGive(unit, quantity, item, except)),
                r.__.then(P.alt(
                    P.seq(r.All, r.TokenItem),
                    P.seq(r.Num, r.TokenItem)
                ))
                    .map(([ quantity, item ]) => new OrderGive(unit, quantity, item))
            ))
            .skip(r.EndCommand),

        // TAKE FROM [unit] ALL [item] EXCEPT [quantity]
        // TAKE FROM [unit] ALL [item class]
        // TAKE FROM [unit] ALL [item]
        // TAKE FROM [unit] [quantity] [item]
        OTake: r => P.regex(/take/i).then(r.__).then(P.regex(/from/i))
            .then(r.UnitItem)
            .chain(unit => P.alt(
                r.__.then(P.seq(r.All, r.TokenItem, r.ExceptNum))
                    .map(([ quantity, item, except ]) => new OrderTake(unit, quantity, item, except)),
                r.__.then(P.alt(
                    P.seq(r.All, r.TokenItem),
                    P.seq(r.Num, r.TokenItem)
                ))
                    .map(([ quantity, item ]) => new OrderTake(unit, quantity, item))
            ))
            .skip(r.EndCommand),

        //JOIN [unit] NOOVERLOAD
        //JOIN [unit] MERGE
        //JOIN [unit]
        OJoin: r => P.regex(/join/i)
            .then(P.alt(
                P.seq(r.UnitItem, r.__.then(P.regex(/nooverload/i))),
                P.seq(r.UnitItem, r.__.then(P.regex(/merge/i))),
                P.seq(r.UnitItem, P.succeed(null))
            ))
            .skip(r.EndCommand)
            .map(([ unit, mode ]) => new OrderJoin(unit, mode)),

        // CAST [skill] [arguments]
        /*
        CAST Gate_Lore DETECT
        CAST Gate_Lore RANDOM [UNITS <unit> ...]
        CAST Gate_Lore RANDOM LEVEL [UNITS <unit> ...]
        CAST Gate_Lore GATE <number> [UNITS <unit> ...]
        CAST Farsight REGION <x> <y> [<z>]
        CAST Teleportation REGION <x> <y> [<z>]
        CAST Portal_Lore <target> UNITS <unit> ...
        CAST Mind_Reading <unit>
        CAST Weather_Lore REGION <x> <y> <z>
        CAST Summon_Wind
        CAST Clear_Skies
        CAST Earth_Lore
        CAST Wolf_Lore
        CAST Bird_Lore DIRECTION <dir>
        CAST Bird_Lore EAGLE
        CAST Dragon_Lore
        CAST Summon_Skeleton
        CAST Raise_Undead
        CAST Summon_Lich
        CAST Summon_Imps
        CAST Summon_Demon
        CAST Summon_Balrog
        CAST Phantasmal_Entertainment
        CAST Create_Phantasmal_Beasts WOLF <number>
        CAST Create_Phantasmal_Beasts EAGLE <number>
        CAST Create_Phantasmal_Beasts DRAGON
        CAST Create_Phantasmal_Undead SKELETON <number>
        CAST Create_Phantasmal_Undead UNDEAD <number>
        CAST Create_Phantasmal_Undead LICH
        CAST Create_Phantasmal_Demons IMP <number>
        CAST Create_Phantasmal_Demons DEMON <number>
        CAST Create_Phantasmal_Demons BALROG
        CAST Invisibility UNITS <unit> ...
        CAST Create_Ring_Of_Invisibility
        CAST Create_Cloak_Of_Invulnerability
        CAST Create_Staff_Of_Fire
        CAST Create_Amulet_Of_True_Seeing
        CAST Create_Amulet_Of_Protection
        CAST Create_Runesword
        CAST Create_Shieldstone
        CAST Create_Magic_Carpet
        CAST Engrave_Runes_of_Warding
        CAST Construct_Gate
        CAST Enchant_Swords
        CAST Enchant_Armor
        CAST Enchant_Shields
        CAST Construct_Portal
        CAST Create_Flaming_Sword
        CAST Create_Aegis
        CAST Create_Windchime
        CAST Create_Gate_Crystal
        CAST Create_Staff_Of_Healing
        CAST Create_Scrying_Orb
        CAST Create_Cornucopia
        CAST Create_Book_Of_Exorcism
        CAST Create_Holy_Symbol
        CAST Create_Censer_Of_Protection
        CAST Transmutation <material>
        CAST Transmutation [number] <material>
        */
        OCast: r => P.regex(/cast/i)
            .then(
                P.seq(r.TokenItem, r.TokenItem.many())
            )
            .skip(r.EndCommand)
            .map(([ skill, args ]) => new OrderCast(skill, args)),

        // SELL ALL [item]
        // SELL [quantity] [item]
        OSell: r => P.regex(/sell/i)
            .then(r.__)
            .then(P.alt(
                P.seq(r.All, r.TokenItem),
                P.seq(r.Num, r.TokenItem)
            ))
            .skip(r.EndCommand)
            .map(([ quantity, item ]) => new OrderSell(quantity, item)),

        // BUY [quantity] [item]
        // BUY ALL [item]
        OBuy: r => P.regex(/buy/i)
            .then(r.__)
            .then(P.alt(
                P.seq(r.All, r.TokenItem),
                P.seq(r.Num, r.TokenItem)
            ))
            .skip(r.EndCommand)
            .map(([ quantity, item ]) => new OrderBuy(quantity, item)),

        // FORGET [skill]
        OForget: r => P.regex(/forget/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(skill => new OrderForget(skill)),

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
                P.regex(/template/i)
                    .chain(template => r.__.then(P.alt(
                            P.regex(/off/i),
                            P.regex(/short/i),
                            P.regex(/long/i),
                            P.regex(/map/i),
                        ))
                        .map(mode => `${template} ${mode}`)
                    )
            ))
            .skip(r.EndCommand)
            .map(setting => new OrderOption(setting)),

        // PASSWORD [password]
        // PASSWORD
        OPassword: r => P.regex(/password/i)
            .then(P.alt(
                r.TokenItem,
                P.succeed(null)
            ))
            .skip(r.EndCommand)
            .map(passwrod => new OrderPassword(passwrod)),

        // QUIT [password]
        OQuit: r => P.regex(/quit/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(password => new OrderQuit(password)),

        // RESTART [password]
        ORestart: r => P.regex(/restart/i)
            .then(r.TokenItem)
            .skip(r.EndCommand)
            .map(password => new OrderRestart(password)),

        // SHOW SKILL [skill] [level]
        // SHOW ITEM [item]
        // SHOW OBJECT [object]
        OShow: r => P.regex(/show/i)
            .then(r.__)
            .then(P.alt(
                P.seq(P.regex(/skill/i), r.TokenItem, r.NumItem),
                P.seq(P.regex(/item/i), r.TokenItem, P.succeed(null)),
                P.seq(P.regex(/object/i), r.TokenItem, P.succeed(null))
            ))
            .skip(r.EndCommand)
            .map(([ type, token, level ]) => new OrderShow(type, token, level)),

        // EXCHANGE [unit] [quantity given] [item given] [quantity expected] [item expected]
        OExchange: r => P.regex(/exchange/i)
            .then(P.seq(
                r.UnitItem,
                r.NumItem,
                r.TokenItem,
                r.NumItem,
                r.TokenItem
            ))
            .skip(r.EndCommand)
            .map(([ unit, quantityGiven, itemGiven, quantityExpected, itemExpected ]) => new OrderExchange(unit, quantityGiven, itemGiven, quantityExpected, itemExpected)),

        OComment: r => r._
            .then(r.Comment)
            .then(P.takeWhile(() => true))
            .map(text => new OrderComment(text)),

        OUnknown: r => P.takeWhile(x => !r.__.parse(x).status)
            .chain(command => r.__
                .then(P.takeWhile(x => true))
                .fallback(null)
                .map(args => new OrderUnknown(command, args))
            ),

        RepeatableOrder: r => P.alt(
            r.OComment,
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
            r.Repeat.then(r.RepeatableOrder).map(order => new RepeatOrder(order)),
            r.RepeatableOrder,
            r.OEndTurn,
            r.OAddress,
            r.OForm,
            r.OEndForm,
            r.OPassword,
            r.OQuit,
            r.ORestart,
        ).or(r.OUnknown)
    })

    return (line: string) => language.Order.parse(line) as P.Result<UnitOrder>
}

export class OrderParser {
    private readonly parser = createOrderParser()

    parse(orders: string): UnitOrder[] {
        const items = []
        for (const line of orders.split(/\r?\n/g)) {
            const r = this.parser(line)
            if (r.status) {
                items.push(r.value)
            }
        }

        return items
    }
}

export const ORDER_PARSER = new OrderParser()
