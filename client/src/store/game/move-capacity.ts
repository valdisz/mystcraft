export class Capacity {
    walk: number = 0
    ride: number = 0
    fly: number = 0
    swim: number = 0

    add(other: Capacity) {
        this.walk += other.walk
        this.swim += other.swim
        this.ride += other.ride
        this.fly += other.fly
    }
}

export type MoveType = keyof Capacity;
