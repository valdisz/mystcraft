import { UniqueItem, Traits, TraitsMap, Item, ItemCategory, SkillInfo } from './internal'

export class ItemInfo implements UniqueItem {
    constructor(public readonly code: string, public readonly category: ItemCategory, singular: string, plural: string) {
        this.name = [singular, plural]
    }

    readonly traits: TraitsMap = { }

    weight: number
    name: [string, string]
    description: string

    hasTrait(trait: Traits) {
        return !!this.traits[trait]
    }

    /**
     * Man-like items can act on its own and form a unit.
     */
    get isManLike() {
        return this.category === 'man' || this.category === 'monster'
    }

    maxSkillLevel(skill: string | SkillInfo) {
        if (!this.hasTrait('canLearn')) {
            return 0
        }

        const code = typeof skill === 'string' ? skill : skill.code
        const { canLearn } = this.traits

        return canLearn.skills.find(x => x.skill.code === code)?.level ?? canLearn.defaultLevel
    }

    create(amount: number = 1): Item {
        const item = new Item(this)
        item.amount = amount

        return item
    }

    getName(count: number) {
        return this.name[count == 1 ? 0 : 1];
    }
}
