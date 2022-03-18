import { IPointData, Point, Rectangle } from 'pixi.js'
import throttle from 'lodash.throttle'

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
// export declare const ResizeObserver: any
declare const ResizeObserver: any

export class Viewport {
    constructor(
        private element: HTMLElement,
        origin: IPointData,
        public mapWidth: number,
        public mapHeight: number,
        private onUpdate: (event: Viewport) => void,
        private onClick: (e: MouseEvent, vp: Viewport) => void,
    ) {
        this.origin.copyFrom(origin)

        this.observer = new ResizeObserver(() => {
            this.updateBounds(this.origin.x, this.origin.y)
        })
        this.observer.observe(element)

        element.addEventListener('pointerdown', this.onPanStart)
        element.addEventListener('pointermove', this.onPan)
        element.addEventListener('pointerup', this.onPanEnd)
        element.addEventListener('pointercancel', this.onPanEnd)
        element.addEventListener('pointerleave', this.onPanEnd)
        element.addEventListener('pointerout', this.onPanEnd)
        element.addEventListener('contextmenu', this.onContextMenu)
        element.addEventListener('wheel', this.onWheel)
    }

    private readonly observer;

    destroy() {
        this.element.removeEventListener('pointerdown', this.onPanStart)
        this.element.removeEventListener('pointermove', this.onPan)
        this.element.removeEventListener('pointerup', this.onPanEnd)
        this.element.removeEventListener('pointercancel', this.onPanEnd)
        this.element.removeEventListener('pointerleave', this.onPanEnd)
        this.element.removeEventListener('pointerout', this.onPanEnd)
        this.element.removeEventListener('contextmenu', this.onContextMenu)
        this.element.removeEventListener('wheel', this.onWheel)
        this.observer.unobserve(this.element)
    }

    readonly origin = new Point()
    readonly pan = new Point()
    paning: 'no' | 'pending' | 'yes' = 'no';

    public updateBounds(x0: number, y0: number) {
        let x = x0 % this.mapWidth
        if (x < 0) {
            x += this.mapWidth
        }

        let y = Math.min(100, y0)
        y = Math.max(y, this.height - this.mapHeight - 100 - this.height / 3)

        // const y = Math.max(-this.mapHeight + this.height, Math.min(y0, 100))

        if (x !== this.origin.x || y !== this.origin.y) {
            this.origin.set(x, y)
        }

        this.raiseOnUpdate()
    }

    private onWheel = (e: MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()
    }

    private onContextMenu = (e: MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()
    }

    private onPanStart = (e: MouseEvent) => {
        if (e.button !== 0) return

        this.pan.set(e.clientX, e.clientY)
        this.paning = 'pending';
    };

    private onPan = (e: MouseEvent) => {
        if (this.paning === 'no') return;

        const deltaX = e.clientX - this.pan.x
        const deltaY = e.clientY - this.pan.y

        const len = Math.sqrt(deltaX * deltaX + deltaY * deltaY)
        if (len > 4) {
            this.paning = 'yes'
        }

        if (this.paning === 'yes') {
            this.pan.set(e.clientX, e.clientY)
        }

        this.updateBounds(
            Math.floor(this.origin.x + deltaX),
            Math.floor(this.origin.y + deltaY)
        )
    }

    private onPanEnd = (e: MouseEvent) => {
        if (this.paning === 'pending') {
            this.onClick(e, this)
        }

        this.paning = 'no';
    };

    private raiseOnUpdate = () => {
        this.onUpdate && this.onUpdate(this);
    }

    get width() { return this.element.clientWidth }
    get height() { return this.element.clientHeight }
    get rect() { return new Rectangle(this.origin.x, this.origin.y, this.width, this.height) }
}
