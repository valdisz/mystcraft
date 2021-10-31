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

    private readonly _heap: QueueItem<T>[] = []

    size() {
        return this._heap.length;
    }

    isEmpty() {
        return this.size() == 0;
    }

    peek() {
        return this._heap[TOP];
    }

    push(value: T, priority: number) {
        this._heap.push({ priority, value });
        this._siftUp();
        return this.size();
    }

    pop(): T {
        const item = this.peek();

        const bottom = this.size() - 1;
        if (bottom > TOP) {
            this._swap(TOP, bottom);
        }
        this._heap.pop();
        this._siftDown();

        return item.value;
    }

    replace(value: T, priority: number) {
        const replacedItem = this.peek();

        this._heap[TOP] = { priority, value };
        this._siftDown();

        return replacedItem.value;
    }

    private _greater(i: number, j: number) {
        return this.comparator(this._heap[i].priority, this._heap[j].priority);
    }

    private _swap(i, j) {
        [this._heap[i], this._heap[j]] = [this._heap[j], this._heap[i]];
    }

    private _siftUp() {
        let node = this.size() - 1;
        while (node > TOP && this._greater(node, parent(node))) {
            this._swap(node, parent(node));
            node = parent(node);
        }
    }

    private _siftDown() {
        let node = TOP;
        while (
            (left(node) < this.size() && this._greater(left(node), node)) ||
            (right(node) < this.size() && this._greater(right(node), node))
        ) {
            let maxChild = (right(node) < this.size() && this._greater(right(node), left(node))) ? right(node) : left(node);
            this._swap(node, maxChild);
            node = maxChild;
        }
    }
}
