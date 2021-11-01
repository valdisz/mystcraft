import { SkillInfo } from './skill-info';
import { UniqueItem } from './unique-item';


export class Skill implements UniqueItem {
    constructor(public readonly info: SkillInfo) {
    }

    get name() {
        return this.info.name;
    }

    get code() {
        return this.info.code;
    }

    level: number;
    days: number;
}
