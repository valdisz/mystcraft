import { Pathfinder } from './algo'
import { MoveType, Link, Region, Level } from './internal'
import { Coord } from '../geometry'

export function distanceHeuristic(mapWidth: number, start: Region, goal: Region) {
    const s = start.coords.toCube()
    const g = goal.coords

    const g0 = g.cube
    const g1 = new Coord(g.x + mapWidth, g.y).toCube()
    const g2 = new Coord(g.x - mapWidth, g.y).toCube()

    const d0 = s.distance(g0)
    const d1 = s.distance(g1)
    const d2 = s.distance(g2)

    return Math.min(d0, d1, d2)
}

export function terrainMoveCost(moveType: MoveType, node: Region, edge: Link) {
    let effectiveMoveType = moveType
    if (effectiveMoveType === 'swim' && node.terrain.isWater && !edge.target.terrain.isWater) {
        effectiveMoveType = 'walk'
    }

    const cost = edge.cost[effectiveMoveType]
    return cost
}

export interface PathStep {
    edge: Link
    cost: number
}

export class RegionPathfinder extends Pathfinder<Region, Link, PathStep> {
    neighbors(node: Region): Link[] {
        return node.neighbors.toArray()
    }

    getNode(edge: Link): Region {
        return edge.target
    }

    toStep(edge: Link, cost: number): PathStep {
        return { edge, cost }
    }
}

export class MovementPathfinder {
    private readonly pf = new RegionPathfinder()

    search(level: Level, moveType: MoveType, start: Region, goal: Region): PathStep[] {
        return this.pf.search(start, goal, {
            cost: (node, edge) => terrainMoveCost(moveType, node, edge),
            heuristic: (start, goal) => distanceHeuristic(level.width, start, goal)
        })
    }

    estimate(moveType: MoveType, start: Region, movePoints: number) {
        return this.pf.estimate(start, movePoints, (node, edge) => terrainMoveCost(moveType, node, edge))
    }
}
