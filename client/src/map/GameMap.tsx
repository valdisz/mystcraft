import { Region } from './Region';
import { Scene } from './Scene';

export class GameMap {
    constructor(canvas: HTMLCanvasElement) {
        this.scene = new Scene(canvas);
    }

    private scene: Scene;

    addRegion(x: number, y: number, terrain: string) {
        this.scene.addChild(new Region(x, y, terrain, 4));
    }

    finish() {
        this.scene.sortChildren();
        this.scene.update();
    }
}
