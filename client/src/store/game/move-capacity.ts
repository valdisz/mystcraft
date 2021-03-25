export class Capacity {
    walk?: number
    ride?: number
    fly?: number
    swim?: number
}

export type MoveType = keyof Capacity;
