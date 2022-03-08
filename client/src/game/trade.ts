import { Hex } from '../geometry'
import { ItemInfo, Level, MovementPathfinder, Region } from './internal'


export interface TradeMarket {
    region: Region
    price: number
    amount: number
}

export interface TradeRoute {
    item: ItemInfo
    buy: TradeMarket
    sell: TradeMarket
    amount: number
    profit: number
    distance: number
    cost: number
}

export class Trade {
    constructor(public readonly item: ItemInfo, private readonly level: Level, private readonly pathfinder: MovementPathfinder) {

    }

    readonly sell = new Map<string, TradeMarket>()
    readonly buy = new Map<string, TradeMarket>()

    supply(region: Region, price: number, amount: number) {
        this.buy.set(region.coords.toString(), { region, price, amount })
    }

    demand(region: Region, price: number, amount: number) {
        this.sell.set(region.coords.toString(), { region, price, amount })
    }

    getRoutes(): TradeRoute[] {
        const routes = []
        for (const sell of this.sell.values()) {
            for (const buy of this.buy.values()) {
                const amount = Math.min(buy.amount, sell.amount)
                const price = sell.price - buy.price

                const tr: TradeRoute = {
                    item: this.item,
                    buy,
                    sell,
                    amount,
                    profit: amount * price,
                    distance: buy.region.coords.cube.distance(sell.region.coords.cube),
                    cost: this.pathfinder
                        .search(this.level, 'ride', buy.region, sell.region)
                        .map(x => x.cost)
                        .reduce((pv, cv) => pv + cv, 0)
                }

                routes.push(tr)
            }
        }

        routes.sort((a, b) => a.cost - b.cost)
        return routes
    }
}
