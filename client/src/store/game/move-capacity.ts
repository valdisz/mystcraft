export class Capacity {
    walk: number = 0
    ride: number = 0
    fly: number = 0
    swim: number = 0

    get known() {
        return this.walk || this.ride || this.fly || this.swim
    }
}

export type MoveType = keyof Omit<Capacity, 'add' | 'known'>;
