import {DisplayObject, IPointData} from 'pixi.js'
import {Link} from '../game'
import {Layers} from './layers'
import {Resources} from './resources'
import {MapState} from './state'
import {Direction} from '../schema';
import {corners} from './tile';

export class Path {

    private _graphics: DisplayObject[] = [];
    private _positions: IPointData[] = [];

    public constructor(public readonly path: Link[], private layers: Layers, private res: Resources, readonly map: MapState) {
    }

    update(positions: IPointData[]) {
        const cornerCoords = corners(this.map.zoom);

        for (const index in this.path) {
            const link = this.path[index];

            if (this._positions.length && this._positions[index] === positions[index]) {
                continue;
            }

            if (this._graphics[index]) {
                this._graphics[index].destroy();
            }

            const asset = this.res.sprite(`sprites/map-arrow-${link.direction.toLowerCase()}`);
            asset.position.copyFrom(positions[index]);
            const scale = 1 / this.map.zoom;
            asset.scale.set(scale, scale);

            if (link.direction === Direction.South) {
                asset.position.set(asset.position.x, asset.position.y + cornerCoords[5].y);
            } else if (link.direction === Direction.North) {
                asset.position.set(asset.position.x, asset.position.y + cornerCoords[1].y);
            } else if (link.direction === Direction.Northeast) {
                const midPointOfCorners = cornerCoords[1].x + (cornerCoords[0].x - cornerCoords[1].x) / 2;
                asset.position.set(asset.position.x + midPointOfCorners, asset.position.y + cornerCoords[1].y / 2);
            } else if (link.direction === Direction.Northwest) {
                const midPointOfCorners = cornerCoords[2].x + (cornerCoords[3].x - cornerCoords[2].x) / 2;
                asset.position.set(asset.position.x + midPointOfCorners, asset.position.y + cornerCoords[2].y / 2);
            } else if (link.direction === Direction.Southeast) {
                const midPointOfCorners = cornerCoords[5].x + (cornerCoords[0].x - cornerCoords[5].x) / 2;
                asset.position.set(asset.position.x + midPointOfCorners, asset.position.y + cornerCoords[4].y / 2);
            } else if (link.direction === Direction.Southwest) {
                const midPointOfCorners = cornerCoords[2].x + (cornerCoords[3].x - cornerCoords[2].x) / 2;
                asset.position.set(asset.position.x + midPointOfCorners, asset.position.y + cornerCoords[4].y / 2);
            }

            this.layers.add('path', asset);
            this._graphics[index] = asset;
        }

        this._positions = positions;
    }

    destroy() {
        if (this._graphics.length) {
            for (const graphic of this._graphics) {
                graphic.destroy();
            }

            this._graphics = [];
        }
    }
}
