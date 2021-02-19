import { Container, Renderer, autoDetectRenderer } from 'pixi.js';
import { Viewport } from './Viewport';

export class Scene extends Container {
    constructor(canvas: HTMLCanvasElement) {
        super();

        this.viewport = new Viewport(canvas);

        this.renderer = autoDetectRenderer({
            width: this.viewport.width,
            height: this.viewport.height,
            view: canvas,
            antialias: true,
            resolution: window.devicePixelRatio || 1
        });

        this.viewport.onUpdate = ({ width, height, offsetX, offsetY, zoom }) => {
            this.renderer.resize(width, height);
            this.x = offsetX;
            this.y = offsetY;
            this.scale.set(zoom);
            this.update();
        };
    }

    readonly renderer: Renderer;
    readonly viewport: Viewport;

    update() {
        this.renderer.render(this);
    }
}
