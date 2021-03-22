import { ItemInfo } from "./item-info";
import { List } from './list';
import { TerrainInfo } from "./terrain-info";


export class Ruleset {
    readonly items: List<ItemInfo> = new List();
    readonly terrain: List<TerrainInfo> = new List();

    getItem(nameOrCode: string) {
        let item = this.items.get(nameOrCode);

        if (!item) {
            for (const i of this.items.all) {
                if (item)
                    break;

                for (const name of i.name) {
                    if (nameOrCode === name) {
                        item = i;
                        break;
                    }
                }
            }
        }

        // we must ensure that even missing items from the ruleset does not crash the client
        if (!item) {
            item = new ItemInfo(nameOrCode, nameOrCode, nameOrCode, []);
            this.items.set(item);
        }

        return item;
    }

    getTerrain(name: string) {
        let t = this.terrain.get(name);

        // we must ensure that even missing terrain from the ruleset does not crash the client
        if (!t) {
            t = new TerrainInfo(name);
            this.terrain.set(t);
        }

        return t;
    }
}
