import { UniqueItem } from './unique-item';
import { Skill } from "./skill";


export class SkillInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    name: string;
    magic: boolean
    description: string[];

    create(days: number, level?: number): Skill {
        const skill = new Skill(this);
        skill.days = days
        skill.level = level ? level : SkillInfo.daysToLevel(days)

        return skill
    }

    static levelToDays(level: number) {
        if (level == 1) return 30;

        return SkillInfo.levelToDays(level - 1) + 30 * level
    }

    static daysToLevel(days: number) {
        let level = 1
        let levelDays = 30
        while (days > levelDays) {
            level++
            levelDays += 30 * level
        }

        return level - 1
    }
}
