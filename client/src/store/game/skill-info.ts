import { UniqueItem } from './unique-item';
import { Skill } from "./skill";


export class SkillInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    name: string;
    description: string;

    create(): Skill {
        return new Skill(this);
    }
}
