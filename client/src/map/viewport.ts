import * as PIXI from 'pixi.js'
import throttle from 'lodash.throttle'

declare const ResizeObserver: any

export class Viewport {
    constructor(
        private element: HTMLElement,
        origin: PIXI.Point,
        private mapWidth: number,
        private mapHeight: number,
        private onUpdate: (event: Viewport) => void,
    ) {
        this.observer = new ResizeObserver(() =>
            this.updateBounds(this.viewRect.x, this.viewRect.y)
        )
        this.observer.observe(element)

        element.addEventListener('pointerdown', this.onPanStart)
        element.addEventListener('pointermove', this.onPan)

        element.addEventListener('pointerup', this.onPanEnd)
        element.addEventListener('pointercancel', this.onPanEnd)
        element.addEventListener('pointerleave', this.onPanEnd)
        element.addEventListener('pointerout', this.onPanEnd)

        this.viewRect = new PIXI.Rectangle(origin.x, origin.y, this.width, this.height)
    }

    private readonly observer;

    // todo: add unobserve
    free() {
        this.observer.unobserve()
    }

    viewRect: PIXI.Rectangle

    get origin() {
        return new PIXI.Point(this.viewRect.x, this.viewRect.y)
    }

    get extent() {
        return new PIXI.Point(this.viewRect.right, this.viewRect.bottom)
    }

    pan = new PIXI.Point(0, 0)
    paning = false;
    zoom = 1;

    private updateBounds(x0: number, y0: number) {
        // const x = this.mapWidth > this.width
        //     ? x0
        //     : (this.width - this.mapWidth) / 2

        // const maxY = this.mapHeight - this.height
        // const y = maxY > 0
        //     ? Math.max(0, Math.min(y0, maxY))
        //     : (this.height - this.mapHeight) / 2

        let x = x0
        if (Math.abs(x) > this.mapWidth) {
            if (x > 0) x -= this.mapWidth
            if (x < 0) x += this.mapWidth
        }

        const y = Math.max(-this.mapHeight + this.height, Math.min(y0, 100))

        if (x !== this.viewRect.x || y !== this.viewRect.y) {
            this.viewRect.x = x
            this.viewRect.y = y

            window.requestAnimationFrame(() => this.raiseOnUpdate());
        }
    }

    private onPanStart = (e: MouseEvent) => {
        this.pan = new PIXI.Point(e.x, e.y)
        this.paning = true;
    };

    private onPan = throttle((e: MouseEvent) => {
        if (!this.paning) return;

        const deltaX = e.x - this.pan.x
        const deltaY = e.y - this.pan.y

        this.pan = new PIXI.Point(e.x, e.y)

        const origin = this.origin
        this.updateBounds(
            Math.floor(origin.x + deltaX),
            Math.floor(origin.y + deltaY)
        )
    }, 20);

    private onPanEnd = (e: MouseEvent) => {
        if (!this.paning) return;

        this.paning = false;
    };

    private raiseOnUpdate = () => {
        this.onUpdate && this.onUpdate(this);
    }

    get width() { return this.element.clientWidth; }
    get height() { return this.element.clientHeight; }
}
