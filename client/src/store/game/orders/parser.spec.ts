import * as P from 'parsimmon'
import { Order, createOrderParser, RepeatOrder, OrderSail } from './parser'

function isOrder<T>(result: P.Result<Order | RepeatOrder>, type: Order['order']) {
    if (result.status) {
        if (result.value.order === type) {
            return result.value as unknown as T
        }
        else {
            fail('Order is of wrong type')
        }
    }
    else {
        fail('Parsing failed')
    }
}

describe('Order Parser', () => {
    const parser = createOrderParser()

    describe('Comment', () => {
        it(';comment', () => {
            expect(parser(';comment').status).toBe(true)
        })
    })

    describe('SAIL', () => {
        it('single direction', () => {
            expect(isOrder<OrderSail>(parser('sail nw'), 'sail').path).toEqual(['nw'])
        })

        it('multi direction', () => {
            expect(isOrder<OrderSail>(parser('sail s n nw'), 'sail').path).toEqual(['s', 'n', 'nw'])
        })

        it('with comment', () => {
            expect(isOrder<OrderSail>(parser('sail s n nw;comment'), 'sail').path).toEqual(['s', 'n', 'nw'])
            expect(isOrder<OrderSail>(parser('sail s n nw    ;comment'), 'sail').path).toEqual(['s', 'n', 'nw'])
            expect(isOrder<OrderSail>(parser('sail s n nw ;comment   '), 'sail').path).toEqual(['s', 'n', 'nw'])
        })
    })
})
