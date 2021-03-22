import { UniqueItem } from "./unique-item";
import { AllTraits, Trait, Traits, TraitsMap } from './traits';
import { Item } from "./item";


export class ItemInfo implements UniqueItem {
    constructor(public readonly code: string, singular: string, plural: string, traits: AllTraits[]) {
        this.name = [singular, plural];

        for (const trait of traits) {
            if (!this.hasTrait(trait.type)) {
                this.traits[trait.type] = [];
            }

            this.traits[trait.type].push(trait);
        }
    }

    private readonly traits: TraitsMap = {};

    weight: number;
    name: [string, string];
    description: string;

    trait<T extends Trait>(trait: Traits): T[] | undefined {
        return this.traits[trait] as T[];
    }

    hasTrait(trait: Traits) {
        return !!this.traits[trait];
    }

    get isSilver() {
        return this.hasTrait("silver");
    }

    get isMan() {
        return this.hasTrait('man');
    }

    create(): Item {
        return new Item(this);
    }

    getName(count: number) {
        return this.name[count == 1 ? 0 : 1];
    }
}
