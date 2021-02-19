import throttle from 'lodash.throttle';
import { IViewport } from './IViewport';
import { ResizeObserver } from './game-page';

export class Viewport implements IViewport {
    constructor(private element: HTMLElement) {
        this.observer = new ResizeObserver(this.raiseOnUpdate);
        this.observer.observe(element);

        element.addEventListener('pointerdown', this.onPanStart);

        element.addEventListener('pointermove', this.onPan);

        element.addEventListener('pointerup', this.onPanEnd);
        element.addEventListener('pointercancel', this.onPanEnd);
        element.addEventListener('pointerleave', this.onPanEnd);
        element.addEventListener('pointerout', this.onPanEnd);

        element.addEventListener('wheel', this.onZoom);
    }

    private _offsetX = 0;
    private _offsetY = 0;

    private _zoom = 100;

    private observer;

    private paning = false;
    private panOffsetX = 0;
    private panOffsetY = 0;
    private panX = 0;
    private panY = 0;

    private onPanStart = (e: MouseEvent) => {
        this.panOffsetX = this._offsetX;
        this.panOffsetY = this._offsetY;

        this.panX = e.x;
        this.panY = e.y;

        this.paning = true;
    };

    private onPan = throttle((e: MouseEvent) => {
        if (!this.paning)
            return;

        const deltaX = e.x - this.panX;
        const deltaY = e.y - this.panY;

        this._offsetX = Math.floor(this.panOffsetX + deltaX);
        this._offsetY = Math.floor(this.panOffsetY + deltaY);

        window.requestAnimationFrame(() => this.raiseOnUpdate());
    }, 30);

    private onPanEnd = (e: MouseEvent) => {
        if (!this.paning)
            return;

        this.paning = false;
        this.raiseOnUpdate();
    };

    private onZoom = (e: WheelEvent) => {
        const z = Math.floor(e.deltaY * 0.1) + this._zoom;
        this._zoom = Math.min(Math.max(z, 10), 400);

        this.raiseOnUpdate();
    };

    private raiseOnUpdate = () => this.onUpdate && this.onUpdate(this);

    get width() { return this.element.clientWidth; }
    get height() { return this.element.clientHeight; }

    get offsetX() { return this._offsetX; }
    get offsetY() { return this._offsetY; }

    get zoom() { return this._zoom / 100; }

    onUpdate?: (event: IViewport) => void;
}
