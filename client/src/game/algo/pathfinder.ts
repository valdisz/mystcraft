import { PriorityQueue } from './priority-queue'

export interface PathfinderHeuristic<TNode, TEdge> {
    (start: TNode, goal: TNode): number
}

export interface PathfinderCost<TNode, TEdge> {
    (node: TNode, edge: TEdge): number
}

export interface PathfinderOptions<TNode, TEdge> {
    heuristic: PathfinderHeuristic<TNode, TEdge>
    cost: PathfinderCost<TNode, TEdge>
}

export interface MoveEstimate<TNode> {
    node: TNode
    cost: number
}

export abstract class Pathfinder<TNode, TEdge, TStep> {
    abstract neighbors(node: TNode): TEdge[];
    abstract getNode(edge: TEdge): TNode;
    abstract toStep(edge: TEdge, cost: number): TStep;

    search(start: TNode, goal: TNode, { cost, heuristic }: PathfinderOptions<TNode, TEdge>): TStep[] {
        if (start === goal) return []

        // priority queue where smaller priority number is on top
        const frontier = new PriorityQueue<TNode>((a, b) => a < b)
        frontier.push(start, 0)

        const cameFrom: Map<TNode, { node: TNode, edge: TEdge } | null> = new Map()
        const costSoFar: Map<TNode, number> = new Map()
        cameFrom.set(start, null)
        costSoFar.set(start, 0)

        while (!frontier.isEmpty()) {
            const current = frontier.pop()

            if (current === goal) {
                break
            }

            const neighbors = this.neighbors(current)
            for (const edge of neighbors) {
                const moveCost = cost(current, edge)
                if (moveCost <= 0) {
                    continue
                }

                const next = this.getNode(edge)
                const currentCost = costSoFar.get(current)
                const newCost = currentCost + moveCost

                const costForNext = costSoFar.get(next)
                if (costForNext === undefined || newCost < costForNext) {
                    costSoFar.set(next, newCost)
                    const priority = newCost + heuristic(next, goal)
                    frontier.push(next, priority)
                    cameFrom.set(next, { node: current, edge })
                }
            }
        }

        let current = cameFrom.get(goal)
        let currentCost = costSoFar.get(goal)

        if (!current) return []

        const path: TStep[] = []
        while (current.node !== start) {
            path.push(this.toStep(current.edge, currentCost))

            current = cameFrom.get(current.node)
            currentCost = costSoFar.get(current.node)
        }

        path.push(this.toStep(current.edge, currentCost))
        
        path.reverse()

        return path
    }

    estimate(start: TNode, maxCost: number, cost: PathfinderCost<TNode, TEdge>): MoveEstimate<TNode>[] {
        // priority queue where smaller priority number is on top
        const frontier = new PriorityQueue<TNode>((a, b) => a < b)
        frontier.push(start, 0)

        const cameFrom: Map<TNode, { node: TNode, edge: TEdge } | null> = new Map()
        const costSoFar: Map<TNode, number> = new Map()
        cameFrom.set(start, null)
        costSoFar.set(start, 0)

        while (!frontier.isEmpty()) {
            const current = frontier.pop()

            const neighbors = this.neighbors(current)
            for (const edge of neighbors) {
                const moveCost = cost(current, edge)
                if (moveCost <= 0) {
                    continue
                }

                const next = this.getNode(edge)
                const currentCost = costSoFar.get(current)
                const newCost = currentCost + moveCost

                if (newCost > maxCost) {
                    continue
                }

                const costForNext = costSoFar.get(next)
                if (costForNext === undefined || newCost < costForNext) {
                    costSoFar.set(next, newCost)
                    const priority = newCost
                    frontier.push(next, priority)
                    cameFrom.set(next, { node: current, edge })
                }
            }
        }

        const items: MoveEstimate<TNode>[] = []
        costSoFar.forEach((value, key) => items.push({ node: key, cost: value }))

        return items
    }
}
