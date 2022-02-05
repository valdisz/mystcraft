import { Direction } from "../schema";
import { Region } from "./types";

export class Link {
    constructor(public readonly source: Region, public readonly direction: Direction, public readonly target: Region) {

    }

    get cost() {
        return this.target.terrain.movement
    }

    toString() {
        return `${this.direction} : ${this.target}`;
    }
}

export interface LinkPredicate {
    (direction: Direction, link: Link): boolean
}

export interface LinkTransformation<T> {
    (direction: Direction, link: Link): T
}

export class Links implements Iterable<Link> {
    private readonly links: Map<Direction, Link> = new Map();

    [Symbol.iterator](): Iterator<Link, any, undefined> {
        return this.links.values()
    }

    get size() {
        return this.links.size
    }

    toArray(): Link[] {
        return Array.from(this.links.values())
    }

    some(p: LinkPredicate): boolean {
        for (const kv of this.links) {
            if (p(kv[0], kv[1])) {
                return true
            }
        }

        return false
    }

    all(p: LinkPredicate): boolean {
        for (const kv of this.links) {
            if (!p(kv[0], kv[1])) {
                return false
            }
        }

        return true
    }

    map<T>(p: LinkTransformation<T>): T[] {
        const items: T[] = [ ]
        for (const kv of this.links) {
            items.push(p(kv[0], kv[1]))
        }

        return items
    }

    get(direction: Direction) {
        return this.links.get(direction)
    }

    set(source: Region, direction: Direction, target: Region) {
        this.links.set(direction, new Link(source, direction, target))
    }

    find(predicate: (link: Link) => boolean) {
        const linksIterator = this.links.values()
        for (const l of linksIterator) {
            if (predicate(l)) {
                return l
            }
        }

        return null
    }
}
