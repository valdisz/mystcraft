import { makeObservable, computed, action, observable, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { TurnStatsFragment } from '../schema'
import { GameStore } from './game-store'
import { SkillInfo } from './game/skill-info'
import { GetAllianceStats, GetAllianceStatsQuery, GetAllianceStatsQueryVariables } from '../schema'
import { ItemCategory } from './game/item-category'
import { ItemInfo } from './game/item-info'

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

const CATEGORY_ORDER = {
    'food': 0,
    'mount': 1,
    'tool': 2,
    'resource': 3,
    'weapon': 4,
    'armor': 5,
    'battle': 6,
    'man': 7,
    'trade': 8,
    'monster': 9,
    'special': 10,
    'money': 11,
    'ship': 12,
    'other': 13
}

export class StatsStore {
    constructor(private game: GameStore) {
        makeObservable(this)
    }

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

    readonly allianceStats = observable<TurnStatsFragment>([])

    loadAllianceStats = async () => {
        if (this.allianceStats.length) return

        const response = await CLIENT.query<GetAllianceStatsQuery, GetAllianceStatsQueryVariables>({
            query: GetAllianceStats,
            variables: {
                gameId: this.game.gameId
            }
        })

        const stats = response.data?.node?.myUniversity?.stats ?? []

        const items: string[] = []

        for (const turn of stats) {
            for (const fac of turn.factions) {
                for (const prod of fac.production) {
                    if (items.includes(prod.code)) continue

                    items.push(prod.code)
                }
            }
        }

        const producedItems = items
        .map(x => this.game.world.ruleset.getItem(x))
        .sort((a, b) => {
            if (a.category === b.category) {
                const aAdv = a.hasTrait('advanced') ? 1 : 0
                const bAdv = b.hasTrait('advanced') ? 1 : 0

                return aAdv === bAdv
                ? a.code.localeCompare(b.code)
                : aAdv - bAdv
            }

            return CATEGORY_ORDER[a.category] - CATEGORY_ORDER[b.category]
        })

        const index = {}
        for (let i = 0; i < producedItems.length; i++) {
            index[producedItems[i].code] = i
        }

        for (const turn of stats) {
            for (const fac of turn.factions) {

                for (const prod of producedItems) {
                    if (fac.production.some(x => x.code === prod.code)) continue

                    fac.production.push({
                        code: prod.code
                    })
                }

                fac.production.sort((a, b) => index[a.code] - index[b.code])
            }
        }

        runInAction(() => {
            this.allianceStats.replace(stats)
            this.allianceProducts.replace(producedItems.map(x => x.code))
        })
    }

    readonly allianceProducts = observable<string>([])
}
