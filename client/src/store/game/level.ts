import { Link, Region } from './region'
import { TypedMap } from './typed-map'
import { Pathfinder } from './algo/pathfinder'
import { MoveType } from './move-capacity'
import { DoubledCoord } from '../../geometry'

class RegionAStar extends Pathfinder<Region, Link> {
    neighbors(node: Region): Link[] {
        return node.neighbors.all()
    }

    getNode(edge: Link): Region {
        return edge.target
    }
}

function distanceHeuristic(mapWidth: number, start: Region, goal: Region) {
    const s = start.coords.toDouble()
    const g = goal.coords.toDouble()

    const s0 = s.toCube()
    const g0 = g.toCube()

    const s1 = new DoubledCoord(s.col + mapWidth, s.row).toCube()
    const g1 = new DoubledCoord(g.col + mapWidth, g.row).toCube()

    const d0 = s0.distance(g0)
    const d1 = s0.distance(g1)
    const d2 = s1.distance(g0)

    return Math.min(d0, d1, d2)
}

function terrainMoveCost(moveType: MoveType, node: Region, edge: Link) {
    let effectiveMoveType = moveType
    if (effectiveMoveType === 'swim' && node.terrain.isWater && !edge.target.terrain.isWater) {
        effectiveMoveType = 'walk'
    }

    const cost = edge.cost[effectiveMoveType]
    return cost
}

export class Level {
    constructor(public width: number, public readonly index: number, public readonly label: string) {
    }

    private readonly pathfinder = new RegionAStar()

    readonly regions: Region[] = [];
    readonly regionMap: TypedMap<Region> = {};
    readonly regionIdMap: TypedMap<Region> = {};

    add(region: Region) {
        this.regions.push(region);
        this.regionMap[region.code] = region;
        this.regionIdMap[region.id] = region;
    }

    get(x: number, y: number) {
        return this.regionMap[`${x},${y},${this.index}`];
    }

    getById(id: string) {
        return this.regionIdMap[id]
    }

    getByCode(code: string) {
        return this.regionMap[code]
    }

    search(moveType: MoveType, start: Region, goal: Region) {
        return this.pathfinder.search(start, goal, {
            cost: (node, edge) => terrainMoveCost(moveType, node, edge),
            heuristic: (start, goal) => distanceHeuristic(this.width, start, goal)
        })
    }

    estimate(moveType: MoveType, start: Region, movePoints: number) {
        return this.pathfinder.estimate(start, movePoints, (node, edge) => terrainMoveCost(moveType, node, edge))
    }
}
