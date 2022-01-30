function parent(i: number) {
    return ((i + 1) >>> 1) - 1
}

function left(i: number) {
    return (i << 1) + 1
}

function right(i: number) {
    return (i + 1) << 1
}

interface QueueItem<T> {
    priority: number
    value: T
}

const TOP = 0

export class PriorityQueue<T> {
    constructor(private readonly comparator = (a, b) => a > b) {
    }

    private readonly heap: QueueItem<T>[] = []

    size() {
        return this.heap.length
    }

    isEmpty() {
        return this.size() == 0
    }

    peek() {
        return this.heap[TOP]
    }

    push(value: T, priority: number) {
        this.heap.push({ priority, value })
        this.siftUp()
        return this.size()
    }

    pop(): T {
        const item = this.peek()

        const bottom = this.size() - 1
        if (bottom > TOP) {
            this.swap(TOP, bottom)
        }

        this.heap.pop()
        this.siftDown()

        return item.value
    }

    replace(value: T, priority: number) {
        const replacedItem = this.peek()

        this.heap[TOP] = { priority, value }
        this.siftDown()

        return replacedItem.value
    }

    private greater(i: number, j: number) {
        return this.comparator(this.heap[i].priority, this.heap[j].priority)
    }

    private swap(i, j) {
        [this.heap[i], this.heap[j]] = [this.heap[j], this.heap[i]]
    }

    private siftUp() {
        let node = this.size() - 1
        while (node > TOP && this.greater(node, parent(node))) {
            this.swap(node, parent(node));
            node = parent(node)
        }
    }

    private siftDown() {
        let node = TOP;
        while (
            (left(node) < this.size() && this.greater(left(node), node)) ||
            (right(node) < this.size() && this.greater(right(node), node))
        ) {
            const maxChild = (right(node) < this.size() && this.greater(right(node), left(node)))
                ? right(node)
                : left(node)

            this.swap(node, maxChild)
            node = maxChild
        }
    }
}
