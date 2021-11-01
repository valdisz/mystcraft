import * as P from 'parsimmon'
import * as Parser from './parser'

function isOrder<T>(result: P.Result<Parser.Order | Parser.RepeatOrder>, type: Parser.Order['order']) {
    if (result.status === true) {
        expect(result.value.order).toBe(type)

        if (result.value.order === type) {
            return result.value as unknown as T
        }
    }

    if (result.status === false) {
        fail(result.expected.join('\n'))
    }
}

function isRepeatOrder(result: P.Result<Parser.Order | Parser.RepeatOrder>, type: Parser.Order['order']) {
    if (result.status === true) {
        expect(result.value.order).toBe('@')

        if (result.value.order === '@') {
            expect(result.value.other.order).toBe(type)
            return result.value as unknown as Parser.RepeatOrder
        }
    }

    if (result.status === false) {
        fail(result.expected.join('\n'))
    }
}

describe('Order Parser', () => {
    const parser = Parser.createOrderParser()

    function pass<T>(repeat: boolean, type: Parser.Order['order'], order: string, expectation: (order: T) => void) {
        const comments = [
            '',
            ';comment',
            '  ;comment',
            ';comment  ',
            '  ;comment  '
        ]

        for (const comment of comments) {
            let testOrder = `${order}${comment}`

            it(testOrder, () => expectation(isOrder<T>(parser(testOrder), type)))
            if (repeat) {
                it('@' + testOrder, () => expectation(isRepeatOrder(parser('@' + testOrder), type).other as unknown as T))
            }
        }

        if (!repeat) {
            it('@' + order, () => expect(parser('@' + order).status).toBe(false))
        }
    }

    it('comment', () => {
        expect(parser(';comment').status).toBe(true)
    })

    describe('turn', () => pass<Parser.OrderTurn>(true, 'turn', 'turn', o => expect(o).toEqual(new Parser.OrderTurn())))

    describe('endturn', () => pass<Parser.OrderEndTurn>(false, 'endturn', 'endturn', o => expect(o).toEqual(new Parser.OrderEndTurn())))

    describe('leave', () => pass<Parser.OrderLeave>(true, 'leave', 'leave', o => expect(o).toEqual(new Parser.OrderLeave())))

    describe('pillage', () => pass<Parser.OrderPillage>(true, 'pillage', 'pillage', o => expect(o).toEqual(new Parser.OrderPillage())))

    describe('tax', () => pass<Parser.OrderTax>(true, 'tax', 'tax', o => expect(o).toEqual(new Parser.OrderTax())))

    describe('entertain', () => pass<Parser.OrderEntertain>(true, 'entertain', 'entertain', o => expect(o).toEqual(new Parser.OrderEntertain())))

    describe('work', () => pass<Parser.OrderWork>(true, 'work', 'work', o => expect(o).toEqual(new Parser.OrderWork())))

    describe('destroy', () => pass<Parser.OrderDestroy>(true, 'destroy', 'destroy', o => expect(o).toEqual(new Parser.OrderDestroy())))

    describe('address', () => {
        pass<Parser.OrderAddress>(false, 'address', 'address name@example.com', o => expect(o).toEqual(new Parser.OrderAddress('name@example.com')))
        pass<Parser.OrderAddress>(false, 'address', 'address "name@example.com"', o => expect(o).toEqual(new Parser.OrderAddress('name@example.com')))
    })

    describe('form', () => pass<Parser.OrderForm>(false, 'form', 'form 1', o => expect(o).toEqual(new Parser.OrderForm(1))))

    describe('end', () => pass<Parser.OrderEndForm>(false, 'end', 'end', o => expect(o).toEqual(new Parser.OrderEndForm())))

    describe('armor', () => {
        pass<Parser.OrderArmor>(true, 'armor', 'armor parm', o => expect(o).toEqual(new Parser.OrderArmor([ 'parm' ])))
        pass<Parser.OrderArmor>(true, 'armor', 'armor parm marm', o => expect(o).toEqual(new Parser.OrderArmor([ 'parm','marm' ])))
        pass<Parser.OrderArmor>(true, 'armor', 'armor parm marm "admt armor"', o => expect(o).toEqual(new Parser.OrderArmor([ 'parm','marm', 'admt armor' ])))
    })

    describe('weapon', () => {
        pass<Parser.OrderWeapon>(true, 'weapon', 'weapon swor', o => expect(o).toEqual(new Parser.OrderWeapon([ 'swor' ])))
        pass<Parser.OrderWeapon>(true, 'weapon', 'weapon swor mswo', o => expect(o).toEqual(new Parser.OrderWeapon([ 'swor', 'mswo' ])))
        pass<Parser.OrderWeapon>(true, 'weapon', 'weapon swor mswo "admt sword"', o => expect(o).toEqual(new Parser.OrderWeapon([ 'swor', 'mswo', 'admt sword' ])))
    })

    describe('autotax', () => {
        pass<Parser.OrderAutotax>(true, 'autotax', 'autotax 0', o => expect(o).toEqual(new Parser.OrderAutotax(false)))
        pass<Parser.OrderAutotax>(true, 'autotax', 'autotax 1', o => expect(o).toEqual(new Parser.OrderAutotax(true)))
    })

    describe('avoid', () => {
        pass<Parser.OrderAvoid>(true, 'avoid', 'avoid 0', o => expect(o).toEqual(new Parser.OrderAvoid(false)))
        pass<Parser.OrderAvoid>(true, 'avoid', 'avoid 1', o => expect(o).toEqual(new Parser.OrderAvoid(true)))
    })

    describe('behind', () => {
        pass<Parser.OrderBehind>(true, 'behind', 'behind 0', o => expect(o).toEqual(new Parser.OrderBehind(false)))
        pass<Parser.OrderBehind>(true, 'behind', 'behind 1', o => expect(o).toEqual(new Parser.OrderBehind(true)))
    })

    describe('hold', () => {
        pass<Parser.OrderHold>(true, 'hold', 'hold 0', o => expect(o).toEqual(new Parser.OrderHold(false)))
        pass<Parser.OrderHold>(true, 'hold', 'hold 1', o => expect(o).toEqual(new Parser.OrderHold(true)))
    })

    describe('noaid', () => {
        pass<Parser.OrderNoAid>(true, 'noaid', 'noaid 0', o => expect(o).toEqual(new Parser.OrderNoAid(false)))
        pass<Parser.OrderNoAid>(true, 'noaid', 'noaid 1', o => expect(o).toEqual(new Parser.OrderNoAid(true)))
    })

    describe('share', () => {
        pass<Parser.OrderShare>(true, 'share', 'share 0', o => expect(o).toEqual(new Parser.OrderShare(false)))
        pass<Parser.OrderShare>(true, 'share', 'share 1', o => expect(o).toEqual(new Parser.OrderShare(true)))
    })

    describe('nocross', () => {
        pass<Parser.OrderNoCross>(true, 'nocross', 'nocross 0', o => expect(o).toEqual(new Parser.OrderNoCross(false)))
        pass<Parser.OrderNoCross>(true, 'nocross', 'nocross 1', o => expect(o).toEqual(new Parser.OrderNoCross(true)))
    })

    describe('guard', () => {
        pass<Parser.OrderGuard>(true, 'guard', 'guard 0', o => expect(o).toEqual(new Parser.OrderGuard(false)))
        pass<Parser.OrderGuard>(true, 'guard', 'guard 1', o => expect(o).toEqual(new Parser.OrderGuard(true)))
    })

    describe('consume', () => {
        pass<Parser.OrderConsume>(true, 'consume', 'consume', o => expect(o).toEqual(new Parser.OrderConsume('silver')))
        pass<Parser.OrderConsume>(true, 'consume', 'consume unit', o => expect(o).toEqual(new Parser.OrderConsume('unit')))
        pass<Parser.OrderConsume>(true, 'consume', 'consume faction', o => expect(o).toEqual(new Parser.OrderConsume('faction')))
    })

    describe('reveal', () => {
        pass<Parser.OrderReveal>(true, 'reveal', 'reveal', o => expect(o).toEqual(new Parser.OrderReveal('none')))
        pass<Parser.OrderReveal>(true, 'reveal', 'reveal unit', o => expect(o).toEqual(new Parser.OrderReveal('unit')))
        pass<Parser.OrderReveal>(true, 'reveal', 'reveal faction', o => expect(o).toEqual(new Parser.OrderReveal('faction')))
    })

    describe('spoils', () => {
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils', o => expect(o).toEqual(new Parser.OrderSpoils('all')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils all', o => expect(o).toEqual(new Parser.OrderSpoils('all')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils none', o => expect(o).toEqual(new Parser.OrderSpoils('none')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils walk', o => expect(o).toEqual(new Parser.OrderSpoils('walk')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils ride', o => expect(o).toEqual(new Parser.OrderSpoils('ride')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils fly', o => expect(o).toEqual(new Parser.OrderSpoils('fly')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils swim', o => expect(o).toEqual(new Parser.OrderSpoils('swim')))
        pass<Parser.OrderSpoils>(true, 'spoils', 'spoils sail', o => expect(o).toEqual(new Parser.OrderSpoils('sail')))
    })

    describe('prepare', () => {
        pass<Parser.OrderPrepare>(true, 'prepare', 'prepare item', o => expect(o).toEqual(new Parser.OrderPrepare('item')))
        pass<Parser.OrderPrepare>(true, 'prepare', 'prepare "item with space"', o => expect(o).toEqual(new Parser.OrderPrepare('item with space')))
        pass<Parser.OrderPrepare>(true, 'prepare', 'prepare item_with_space', o => expect(o).toEqual(new Parser.OrderPrepare('item with space')))
    })

    describe('combat', () => {
        pass<Parser.OrderCombat>(true, 'combat', 'combat item', o => expect(o).toEqual(new Parser.OrderCombat('item')))
        pass<Parser.OrderCombat>(true, 'combat', 'combat "item with space"', o => expect(o).toEqual(new Parser.OrderCombat('item with space')))
        pass<Parser.OrderCombat>(true, 'combat', 'combat item_with_space', o => expect(o).toEqual(new Parser.OrderCombat('item with space')))
    })

    describe('name', () => {
        pass<Parser.OrderName>(true, 'name', 'name unit name', o => expect(o).toEqual(new Parser.OrderName('unit', 'name')))
        pass<Parser.OrderName>(true, 'name', 'name faction name', o => expect(o).toEqual(new Parser.OrderName('faction', 'name')))
        pass<Parser.OrderName>(true, 'name', 'name object name', o => expect(o).toEqual(new Parser.OrderName('object', 'name')))
        pass<Parser.OrderName>(true, 'name', 'name city name', o => expect(o).toEqual(new Parser.OrderName('city', 'name')))

        pass<Parser.OrderName>(true, 'name', 'name unit "long name"', o => expect(o).toEqual(new Parser.OrderName('unit', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name faction "long name"', o => expect(o).toEqual(new Parser.OrderName('faction', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name object "long name"', o => expect(o).toEqual(new Parser.OrderName('object', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name city "long name"', o => expect(o).toEqual(new Parser.OrderName('city', 'long name')))

        pass<Parser.OrderName>(true, 'name', 'name unit long_name', o => expect(o).toEqual(new Parser.OrderName('unit', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name faction long_name', o => expect(o).toEqual(new Parser.OrderName('faction', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name object long_name', o => expect(o).toEqual(new Parser.OrderName('object', 'long name')))
        pass<Parser.OrderName>(true, 'name', 'name city long_name', o => expect(o).toEqual(new Parser.OrderName('city', 'long name')))
    })

    describe('describe', () => {
        pass<Parser.OrderDescribe>(true, 'describe', 'describe unit name', o => expect(o).toEqual(new Parser.OrderDescribe('unit', 'name')))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe ship name', o => expect(o).toEqual(new Parser.OrderDescribe('ship', 'name')))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe building name', o => expect(o).toEqual(new Parser.OrderDescribe('building', 'name')))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe object name', o => expect(o).toEqual(new Parser.OrderDescribe('object', 'name')))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe structure name', o => expect(o).toEqual(new Parser.OrderDescribe('structure', 'name')))

        pass<Parser.OrderDescribe>(true, 'describe', 'describe unit', o => expect(o).toEqual(new Parser.OrderDescribe('unit', null)))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe ship', o => expect(o).toEqual(new Parser.OrderDescribe('ship', null)))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe building', o => expect(o).toEqual(new Parser.OrderDescribe('building', null)))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe object', o => expect(o).toEqual(new Parser.OrderDescribe('object', null)))
        pass<Parser.OrderDescribe>(true, 'describe', 'describe structure', o => expect(o).toEqual(new Parser.OrderDescribe('structure', null)))
    })

    describe('move', () => {
        pass<Parser.OrderMove>(true, 'move', 'move n', o => expect(o).toEqual(new Parser.OrderMove([ 'n' ])))
        pass<Parser.OrderMove>(true, 'move', 'move n nw ne s sw se in out 1', o => expect(o).toEqual(new Parser.OrderMove([ 'n', 'nw', 'ne', 's', 'sw', 'se', 'in', 'out', 1 ])))
    })

    describe('advance', () => {
        pass<Parser.OrderAdvance>(true, 'advance', 'advance n', o => expect(o).toEqual(new Parser.OrderAdvance([ 'n' ])))
        pass<Parser.OrderAdvance>(true, 'advance', 'advance n nw ne s sw se in out 1', o => expect(o).toEqual(new Parser.OrderAdvance([ 'n', 'nw', 'ne', 's', 'sw', 'se', 'in', 'out', 1 ])))
    })

    describe('sail', () => {
        pass<Parser.OrderSail>(true, 'sail', 'sail', o => expect(o).toEqual(new Parser.OrderSail([])))
        pass<Parser.OrderSail>(true, 'sail', 'sail n', o => expect(o).toEqual(new Parser.OrderSail([ 'n' ])))
        pass<Parser.OrderSail>(true, 'sail', 'sail n nw ne s sw se', o => expect(o).toEqual(new Parser.OrderSail([ 'n', 'nw', 'ne', 's', 'sw', 'se' ])))
    })

    describe('promote', () => {
        pass<Parser.OrderPromote>(true, 'promote', 'promote 123', o => expect(o).toEqual(new Parser.OrderPromote(new Parser.UnitNumber(123))))
        pass<Parser.OrderPromote>(true, 'promote', 'promote new 1', o => expect(o).toEqual(new Parser.OrderPromote(new Parser.UnitNumber(1, true))))
        pass<Parser.OrderPromote>(true, 'promote', 'promote faction 10 new 5', o => expect(o).toEqual(new Parser.OrderPromote(new Parser.UnitNumber(5, true, 10))))
    })

    describe('evict', () => {
        pass<Parser.OrderEvict>(true, 'evict', 'evict 123', o => expect(o).toEqual(new Parser.OrderEvict(new Parser.UnitNumber(123))))
        pass<Parser.OrderEvict>(true, 'evict', 'evict new 1', o => expect(o).toEqual(new Parser.OrderEvict(new Parser.UnitNumber(1, true))))
        pass<Parser.OrderEvict>(true, 'evict', 'evict faction 10 new 5', o => expect(o).toEqual(new Parser.OrderEvict(new Parser.UnitNumber(5, true, 10))))
    })

    describe('enter', () => pass<Parser.OrderEnter>(true, 'enter', 'enter 3', o => expect(o).toEqual(new Parser.OrderEnter(3))))

    describe('attack', () => {
        pass<Parser.OrderAttack>(true, 'attack', 'attack 1', o => expect(o).toEqual(new Parser.OrderAttack([ new Parser.UnitNumber(1) ])))
        pass<Parser.OrderAttack>(true, 'attack', 'attack 1 2 345', o => expect(o).toEqual(new Parser.OrderAttack([
            new Parser.UnitNumber(1),
            new Parser.UnitNumber(2),
            new Parser.UnitNumber(345)
        ])))
    })

    describe('assassinate', () => pass<Parser.OrderAssassinate>(true, 'assassinate', 'assassinate 1', o => expect(o).toEqual(new Parser.OrderAssassinate(new Parser.UnitNumber(1)))))

    describe('steal', () => pass<Parser.OrderSteal>(true, 'steal', 'steal 1 silver', o => expect(o).toEqual(new Parser.OrderSteal(
        new Parser.UnitNumber(1),
        'silver'
    ))))

    describe('teach', () => {
        pass<Parser.OrderTeach>(true, 'teach', 'teach 1', o => expect(o).toEqual(new Parser.OrderTeach([
            new Parser.UnitNumber(1)
        ])))

        pass<Parser.OrderTeach>(true, 'teach', 'teach 1 2', o => expect(o).toEqual(new Parser.OrderTeach([
            new Parser.UnitNumber(1),
            new Parser.UnitNumber(2)
        ])))

        pass<Parser.OrderTeach>(true, 'teach', 'teach 1 2 new 1 new 2', o => expect(o).toEqual(new Parser.OrderTeach([
            new Parser.UnitNumber(1),
            new Parser.UnitNumber(2),
            new Parser.UnitNumber(1, true),
            new Parser.UnitNumber(2, true)
        ])))

        pass<Parser.OrderTeach>(true, 'teach', 'teach 1 2 new 1 faction 10 new 5 new 2 faction 11 new 6', o => expect(o).toEqual(new Parser.OrderTeach([
            new Parser.UnitNumber(1),
            new Parser.UnitNumber(2),
            new Parser.UnitNumber(1, true),
            new Parser.UnitNumber(5, true, 10),
            new Parser.UnitNumber(2, true),
            new Parser.UnitNumber(6, true, 11),
        ])))
    })

    describe('withdraw', () => {
        pass<Parser.OrderWithdraw>(true, 'withdraw', 'withdraw hors', o => expect(o).toEqual(new Parser.OrderWithdraw(1, 'hors')))
        pass<Parser.OrderWithdraw>(true, 'withdraw', 'withdraw 3 hors', o => expect(o).toEqual(new Parser.OrderWithdraw(3, 'hors')))
    })

    describe('claim', () => pass<Parser.OrderClaim>(true, 'claim', 'claim 100', o => expect(o).toEqual(new Parser.OrderClaim(100))))

    describe('study', () => {
        pass<Parser.OrderStudy>(true, 'study', 'study comb', o => expect(o).toEqual(new Parser.OrderStudy('comb')))
        pass<Parser.OrderStudy>(true, 'study', 'study comb 5', o => expect(o).toEqual(new Parser.OrderStudy('comb', 5)))
    })

    describe('produce', () => {
        pass<Parser.OrderProduce>(true, 'produce', 'produce swor', o => expect(o).toEqual(new Parser.OrderProduce('swor')))
        pass<Parser.OrderProduce>(true, 'produce', 'produce 10 swor', o => expect(o).toEqual(new Parser.OrderProduce('swor', 10)))
    })

    describe('build', () => {
        pass<Parser.OrderBuild>(true, 'build', 'build', o => expect(o).toEqual(new Parser.OrderBuild(
            new Parser.BuildContinue()
        )))
        pass<Parser.OrderBuild>(true, 'build', 'build help 123', o => expect(o).toEqual(new Parser.OrderBuild(
            new Parser.BuildHelp(new Parser.UnitNumber(123))
        )))
        pass<Parser.OrderBuild>(true, 'build', 'build Citadel', o => expect(o).toEqual(new Parser.OrderBuild(
            new Parser.Build('Citadel')
        )))
    })

    describe('transport', () => {
        pass<Parser.OrderTransport>(true, 'transport', 'transport 123 500 stone', o => expect(o).toEqual(new Parser.OrderTransport(
            new Parser.UnitNumber(123),
            500,
            'stone'
        )))

        pass<Parser.OrderTransport>(true, 'transport', 'transport 123 all silver', o => expect(o).toEqual(new Parser.OrderTransport(
            new Parser.UnitNumber(123),
            'all',
            'silver'
        )))

        pass<Parser.OrderTransport>(true, 'transport', 'transport 123 all silver except 1000', o => expect(o).toEqual(new Parser.OrderTransport(
            new Parser.UnitNumber(123),
            'all',
            'silver',
            1000
        )))
    })

    describe('distribute', () => {
        pass<Parser.OrderDistribute>(true, 'distribute', 'distribute 123 500 stone', o => expect(o).toEqual(new Parser.OrderDistribute(
            new Parser.UnitNumber(123),
            500,
            'stone'
        )))

        pass<Parser.OrderDistribute>(true, 'distribute', 'distribute 123 all silver', o => expect(o).toEqual(new Parser.OrderDistribute(
            new Parser.UnitNumber(123),
            'all',
            'silver'
        )))

        pass<Parser.OrderDistribute>(true, 'distribute', 'distribute 123 all silver except 1000', o => expect(o).toEqual(new Parser.OrderDistribute(
            new Parser.UnitNumber(123),
            'all',
            'silver',
            1000
        )))
    })

    describe('declare', () => {
        for (const attitude of ['ally', 'friendly', 'neutral', 'unfriendly', 'hostile']) {
            pass<Parser.OrderDeclare>(true, 'declare', `declare default ${attitude}`, o => expect(o).toEqual(new Parser.OrderDeclare(
                new Parser.DeclareDefault(attitude)
            )))

            pass<Parser.OrderDeclare>(true, 'declare', `declare 10 ${attitude}`, o => expect(o).toEqual(new Parser.OrderDeclare(
                new Parser.DeclareFaction(10, attitude)
            )))
        }

        pass<Parser.OrderDeclare>(true, 'declare', 'declare 11', o => expect(o).toEqual(new Parser.OrderDeclare(
            new Parser.DeclareFaction(11)
        )))
    })

    describe('faction', () => {
        pass<Parser.OrderFaction>(true, 'faction', 'faction war 10', o => expect(o).toEqual(new Parser.OrderFaction([
            new Parser.FactionSetting('war', 10)
        ])))

        pass<Parser.OrderFaction>(true, 'faction', 'faction war 10 trade 9', o => expect(o).toEqual(new Parser.OrderFaction([
            new Parser.FactionSetting('war', 10),
            new Parser.FactionSetting('trade', 9)
        ])))

        pass<Parser.OrderFaction>(true, 'faction', 'faction war 10 trade 9 magic 8', o => expect(o).toEqual(new Parser.OrderFaction([
            new Parser.FactionSetting('war', 10),
            new Parser.FactionSetting('trade', 9),
            new Parser.FactionSetting('magic', 8)
        ])))

        pass<Parser.OrderFaction>(true, 'faction', 'faction martial 7 magic 6', o => expect(o).toEqual(new Parser.OrderFaction([
            new Parser.FactionSetting('martial', 7),
            new Parser.FactionSetting('magic', 6)
        ])))
    })

    describe('give', () => {
        pass<Parser.OrderGive>(true, 'give', 'give 123 unit', o => expect(o).toEqual(new Parser.OrderGive(
            new Parser.UnitNumber(123), 'unit'
        )))

        pass<Parser.OrderGive>(true, 'give', 'give 123 all silv except 750', o => expect(o).toEqual(new Parser.OrderGive(
            new Parser.UnitNumber(123), 'all', 'silv', 750
        )))

        pass<Parser.OrderGive>(true, 'give', 'give 123 all tools', o => expect(o).toEqual(new Parser.OrderGive(
            new Parser.UnitNumber(123), 'all', 'tools'
        )))

        pass<Parser.OrderGive>(true, 'give', 'give 123 all parm', o => expect(o).toEqual(new Parser.OrderGive(
            new Parser.UnitNumber(123), 'all', 'parm'
        )))

        pass<Parser.OrderGive>(true, 'give', 'give 123 75 hors', o => expect(o).toEqual(new Parser.OrderGive(
            new Parser.UnitNumber(123), 75, 'hors'
        )))
    })

    describe('take', () => {
        pass<Parser.OrderTake>(true, 'take', 'take from 123 all live except 10', o => expect(o).toEqual(new Parser.OrderTake(
            new Parser.UnitNumber(123), 'all', 'live', 10
        )))

        pass<Parser.OrderTake>(true, 'take', 'take from 123 all monsters', o => expect(o).toEqual(new Parser.OrderTake(
            new Parser.UnitNumber(123), 'all', 'monsters'
        )))

        pass<Parser.OrderTake>(true, 'take', 'take from 123 all swor', o => expect(o).toEqual(new Parser.OrderTake(
            new Parser.UnitNumber(123), 'all', 'swor'
        )))

        pass<Parser.OrderTake>(true, 'take', 'take from 123 800 silv', o => expect(o).toEqual(new Parser.OrderTake(
            new Parser.UnitNumber(123), 800, 'silv'
        )))
    })

    describe('join', () => {
        pass<Parser.OrderJoin>(true, 'join', 'join 345', o => expect(o).toEqual(new Parser.OrderJoin(
            new Parser.UnitNumber(345)
        )))

        pass<Parser.OrderJoin>(true, 'join', 'join 345 merge', o => expect(o).toEqual(new Parser.OrderJoin(
            new Parser.UnitNumber(345), 'merge'
        )))

        pass<Parser.OrderJoin>(true, 'join', 'join 345 nooverload', o => expect(o).toEqual(new Parser.OrderJoin(
            new Parser.UnitNumber(345), 'nooverload'
        )))
    })

    describe('cast', () => {
        pass<Parser.OrderCast>(true, 'cast', 'cast eart', o => expect(o).toEqual(new Parser.OrderCast('eart')))
        pass<Parser.OrderCast>(true, 'cast', 'cast earth_lore', o => expect(o).toEqual(new Parser.OrderCast('earth lore')))
        pass<Parser.OrderCast>(true, 'cast', 'cast Gate_Lore GATE 123 UNITS 756', o => expect(o).toEqual(new Parser.OrderCast(
            'Gate Lore',
            [ 'GATE', '123', 'UNITS', '756' ]
        )))
    })

    describe('sell', () => {
        pass<Parser.OrderSell>(true, 'sell', 'sell all grai', o => expect(o).toEqual(new Parser.OrderSell('all', 'grai')))
        pass<Parser.OrderSell>(true, 'sell', 'sell 75 grai', o => expect(o).toEqual(new Parser.OrderSell(75, 'grai')))
    })

    describe('buy', () => {
        pass<Parser.OrderBuy>(true, 'buy', 'buy all grai', o => expect(o).toEqual(new Parser.OrderBuy('all', 'grai')))
        pass<Parser.OrderBuy>(true, 'buy', 'buy 75 grai', o => expect(o).toEqual(new Parser.OrderBuy(75, 'grai')))
    })

    describe('forget', () => pass<Parser.OrderForget>(true, 'forget', 'forget comb', o => expect(o).toEqual(new Parser.OrderForget('comb'))))

    describe('option', () => {
        for (const opt of [
            'TIMES',
            'NOTIMES',
            'SHOWATTITUDES',
            'DONTSHOWATTITUDES',
            'TEMPLATE OFF',
            'TEMPLATE SHORT',
            'TEMPLATE LONG',
            'TEMPLATE MAP'
        ]) {
            pass<Parser.OrderOption>(true, 'option', `option ${opt}`, o => expect(o).toEqual(new Parser.OrderOption(opt)))
        }
    })

    describe('password', () => {
        pass<Parser.OrderPassword>(false, 'password', 'password "new password"', o => expect(o).toEqual(new Parser.OrderPassword('new password')))
        pass<Parser.OrderPassword>(false, 'password', 'password', o => expect(o).toEqual(new Parser.OrderPassword(null)))
    })

    describe('quit', () => pass<Parser.OrderQuit>(false, 'quit', 'quit "password"', o => expect(o).toEqual(new Parser.OrderQuit('password'))))

    describe('restart', () => pass<Parser.OrderRestart>(false, 'restart', 'restart "password"', o => expect(o).toEqual(new Parser.OrderRestart('password'))))

    describe('show', () => {
        pass<Parser.OrderShow>(true, 'show', 'show skill armo 5', o => expect(o).toEqual(new Parser.OrderShow(
            'skill', 'armo', 5
        )))

        pass<Parser.OrderShow>(true, 'show', 'show item parm', o => expect(o).toEqual(new Parser.OrderShow(
            'item', 'parm'
        )))

        pass<Parser.OrderShow>(true, 'show', 'show object Citadel', o => expect(o).toEqual(new Parser.OrderShow(
            'object', 'Citadel'
        )))
    })

    describe('exchange', () => pass<Parser.OrderExchange>(true, 'exchange', 'exchange 456 100 silv 1 hors', o => expect(o).toEqual(new Parser.OrderExchange(
        new Parser.UnitNumber(456), 100, 'silv', 1, 'hors'
    ))))
})
