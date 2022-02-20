import { Pathfinder } from './algo/pathfinder'
import { MoveType } from './move-capacity'
import { Link } from './link'
import { Region } from './region'
import { Coord } from '../geometry'
import { Level } from './level'

export function distanceHeuristic(mapWidth: number, start: Region, goal: Region) {
    const s = start.coords
    const g = goal.coords

    const s0 = s.cube
    const g0 = g.cube

    const s1 = new Coord(s.x + mapWidth, s.y).toCube()
    const g1 = new Coord(g.x + mapWidth, g.y).toCube()

    const d0 = s0.distance(g0)
    const d1 = s0.distance(g1)
    const d2 = s1.distance(g0)

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
