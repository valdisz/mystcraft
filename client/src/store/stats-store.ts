import { makeObservable, computed, action, observable, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { GameStore } from './game-store'
import { SkillInfo } from '../game'
import { AnItem, Expenses, Income, PlayerTurnStatisticsFragment, StatisticsCategory } from '../schema'
import { GetTurnStats, GetTurnStatsQuery, GetTurnStatsQueryVariables } from '../schema'
import { ItemCategory } from '../game'
import { ItemInfo } from '../game'

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

interface TurnStats {
    turnNumber: number
    production: AnItem[]
    income: Income
    expenses: Expenses
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

    readonly stats = observable<TurnStats>([])

    readonly products = observable<ItemInfo>([])

    @computed get skills() {
        const { world, factionNumber } = this.game

        if (!world) return []

        const faction = world.factions.get(factionNumber)

        const skills: { [code: string]: SkillStats } = { }
        faction.troops
            .filter(x => !!x.skills.size)
            .flatMap(x => x.skills.map(s => ({
                skill: s.info,
                level: s.level,
                men: x.inventory.items.filter(x => x.isMan).reduce((p, c) => p + c.amount, 0)
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

            return (a.skill.name ?? a.skill.code).localeCompare(b.skill.name ?? b.skill.code)
        })

        console.log(skillStats)

        return skillStats
    }

    loadStats = async () => {
        if (this.stats.length) {
            return
        }

        const response = await CLIENT.query<GetTurnStatsQuery, GetTurnStatsQueryVariables>({
            query: GetTurnStats,
            variables: {
                playerId: this.game.playerId
            }
        })

        if (response.data.node.__typename !== 'Player') {
            return
        }

        const turns: TurnStats[] = response.data.node.turns.items.map(x => ({
            turnNumber: x.turnNumber,
            income: x.income,
            expenses: x.expenses,
            production: x.statistics.filter(i => i.category === StatisticsCategory.Produced).map(({ code, amount }) => ({ code, amount }))
        }))

        // all items produced during all turns
        const items: string[] = []
        for (const turn of turns) {
            for (const prod of turn.production) {
                if (items.includes(prod.code)) continue

                items.push(prod.code)
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

        for (const turn of turns) {
            const production  = turn.production

            for (const item of producedItems) {
                if (!production.some(x => x.code === item.code)) {
                    // item was not produced
                    production.push({
                        code: item.code,
                        amount: null
                    })
                }
            }

            production.sort((a, b) => index[a.code] - index[b.code])
        }

        runInAction(() => {
            this.stats.replace(turns)
            this.products.replace(producedItems)
        })
    }
}
