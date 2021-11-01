import { Direction } from "../../schema";
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

export class Links {
    private readonly links: Map<Direction, Link> = new Map()

    all(): Link[] {
        return Array.from(this.links.values())
    }

    get(direction: Direction) {
        return this.links.get(direction)
    }

    set(source: Region, direction: Direction, target: Region) {
        this.links.set(direction, new Link(source, direction, target))
    }
}
