import * as PIXI from 'pixi.js'
import throttle from 'lodash.throttle'

declare const ResizeObserver: any

export class Viewport {
    constructor(
        private element: HTMLElement,
        public origin: PIXI.Point,
        private mapWidth: number,
        private mapHeight: number,
        private onUpdate: (event: Viewport) => void,
    ) {
        this.observer = new ResizeObserver(() =>
            this.updateBounds(this.origin.x, this.origin.y)
        )
        this.observer.observe(element)

        element.addEventListener('pointerdown', this.onPanStart)
        element.addEventListener('pointermove', this.onPan)
        element.addEventListener('pointerup', this.onPanEnd)
        element.addEventListener('pointercancel', this.onPanEnd)
        element.addEventListener('pointerleave', this.onPanEnd)
        element.addEventListener('pointerout', this.onPanEnd)
        element.addEventListener('contextmenu', this.onContextMenu)
    }

    private readonly observer;

    // todo: add unobserve
    free() {
        this.observer.unobserve()
    }

    pan: PIXI.Point
    paning = false;

    private updateBounds(x0: number, y0: number) {
        let x = x0
        if (Math.abs(x) > this.mapWidth) {
            if (x > 0) x -= this.mapWidth
            if (x < 0) x += this.mapWidth
        }

        const y = Math.max(-this.mapHeight + this.height, Math.min(y0, 100))

        if (x !== this.origin.x || y !== this.origin.y) {
            this.origin.x = x
            this.origin.y = y

            window.requestAnimationFrame(() => this.raiseOnUpdate());
        }
    }

    private onContextMenu = (e: MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()
    }

    private onPanStart = (e: MouseEvent) => {
        if (e.button !== 2) return

        this.pan = new PIXI.Point(e.clientX, e.clientY)
        this.paning = true;
    };

    private onPan = throttle((e: MouseEvent) => {
        if (!this.paning) return;

        const deltaX = e.clientX - this.pan.x
        const deltaY = e.clientY - this.pan.y

        this.pan = new PIXI.Point(e.clientX, e.clientY)

        this.updateBounds(
            Math.floor(this.origin.x + deltaX),
            Math.floor(this.origin.y + deltaY)
        )
    }, 20);

    private onPanEnd = (e: MouseEvent) => {
        this.paning = false;
    };

    private raiseOnUpdate = () => {
        this.onUpdate && this.onUpdate(this);
    }

    get width() { return this.element.clientWidth; }
    get height() { return this.element.clientHeight; }
}
