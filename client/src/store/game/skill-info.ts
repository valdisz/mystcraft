import { UniqueItem } from './unique-item';
import { Skill } from "./skill";


export class SkillInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    name: string;
    magic: boolean
    description: string[];

    create(): Skill {
        return new Skill(this);
    }
}
