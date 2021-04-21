import { makeObservable, computed, action, observable } from 'mobx'
import { GameStore } from './game-store'
import { SkillInfo } from './game/skill-info'

interface KnownSkill {
    skill: SkillInfo
    level: number
    men: number
}

export interface SkillStats {
    skill: SkillInfo
    levels: number[]
    total: number
}

export type StatTabs = 'skills'

export class StatsStore {
    constructor(private game: GameStore) {
        makeObservable(this)
    }

    @observable tab: StatTabs = 'skills'
    @action setTab = (tab: StatTabs) => this.tab = tab

    @computed get skills() {
        const { world, factionNumber } = this.game

        if (!world) return []

        const faction = world.factions.get(factionNumber)

        const skills: { [code: string]: SkillStats } = { }
        faction.troops.units
            .filter(x => x.skills.length)
            .flatMap(x => x.skills.all.map(s => ({
                skill: s.info,
                level: s.level,
                men: x.inventory.items.all.filter(x => x.isMan).reduce((p, c) => p + c.amount, 0)
            } as KnownSkill) ))
            .forEach(({ skill, level, men }) => {
                const key = skill.code

                let s = skills[key]
                if (!s) {
                    s = { skill, levels: new Array(6), total: 0 }
                    skills[key] = s
                }

                s.levels[level] = (s.levels[level] ?? 0) + men
                s.total += men
            })

        const skillStats = Object.values(skills)
        skillStats.sort((a, b) => {
            const aMagic = a.skill.magic ? 1 : 0
            const bMagic = b.skill.magic ? 1 : 0

            if (aMagic !== bMagic) return aMagic - bMagic

            return a.skill.name.localeCompare(b.skill.name)
        })

        return skillStats
    }
}
