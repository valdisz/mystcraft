import * as PIXI from 'pixi.js'
import throttle from 'lodash.throttle'

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
// export declare const ResizeObserver: any
declare const ResizeObserver: any

export class Viewport {
    constructor(
        private element: HTMLElement,
        public origin: PIXI.Point,
        private mapWidth: number,
        private mapHeight: number,
        private onUpdate: (event: Viewport) => void,
        private onClick: (e: MouseEvent, vp: Viewport) => void,
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

    pan: PIXI.Point
    paning: 'no' | 'peding' | 'yes' = 'no';
    scale = 1

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

    private onWheel = (e: WheelEvent) => {
        e.preventDefault()
        e.stopPropagation()

        // const s = Math.min(Math.max(.125, this.scale + e.deltaY * -0.001), 1)
        // const w = this.width
        // const h = this.height

        // const sW = w > this.mapWidth ? this.element.clientWidth / w : s
        // const sH = h > this.mapWidth ? this.element.clientHeight / h : s

        // this.scale = Math.min(Math.max(sW, sH), 1)

        // this.raiseOnUpdate();
    }

    private onPanStart = (e: MouseEvent) => {
        if (e.button !== 0) return

        this.pan = new PIXI.Point(e.clientX, e.clientY)
        this.paning = 'peding';
    };

    private onPan = throttle((e: MouseEvent) => {
        if (this.paning === 'no') return;

        const deltaX = e.clientX - this.pan.x
        const deltaY = e.clientY - this.pan.y

        const len = Math.sqrt(deltaX * deltaX + deltaY * deltaY)
        if (len > 4) {
            this.paning = 'yes'
        }

        if (this.paning === 'yes') {
            this.pan = new PIXI.Point(e.clientX, e.clientY)
        }

        this.updateBounds(
            Math.floor(this.origin.x + deltaX),
            Math.floor(this.origin.y + deltaY)
        )
    }, 20);

    private onPanEnd = (e: MouseEvent) => {
        if (this.paning === 'peding') {
            this.onClick(e, this)
        }

        this.paning = 'no';
    };

    private raiseOnUpdate = () => {
        this.onUpdate && this.onUpdate(this);
    }

    get width() { return this.element.clientWidth / this.scale; }
    get height() { return this.element.clientHeight / this.scale; }

    setOffset(p: PIXI.Point) {
        this.origin.copyFrom(p)
        this.raiseOnUpdate()
    }
}
