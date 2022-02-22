import { UniqueItem, Skill } from "./internal";

export class SkillInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    name: string;
    magic: boolean
    description: string[];

    create(days: number, level?: number): Skill {
        const skill = new Skill(this);
        skill.days = days
        skill.level = level ? level : SkillInfo.level(days)

        return skill
    }

    static days(level: number) {
        return 15 * (level * level + level)
    }

    static level(days: number) {
        if (days === 0) {
            return 0
        }

        return Math.trunc((Math.sqrt(60 * days + 225) - 15) / 30)
    }
}
