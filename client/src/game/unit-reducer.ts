import { Link, Region, Unit } from "./internal"

export class UnitChanges {
    path: Link[]
    remainingPath: Link[]
    destination: Region
}

function unitAsOn(unit: Unit, stage: 'initial' | 'give'): Unit {
    return new Proxy(unit, {
        get: (target, prop) => {

        }
    })
}


// function reduceUnit(unit: Unit, order: Order): UnitChanges {
//     switch (order.order) {
//         case 'move':
//         case 'advance': {

//             break
//         }
//     }
// }
