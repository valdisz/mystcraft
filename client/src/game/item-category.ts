export type ItemCategory =
      'man'
    | 'trade'
    | 'monster'
    | 'weapon'
    | 'armor'
    | 'shield'
    | 'special'
    | 'money'
    | 'ship'
    | 'mount'
    | 'battle'
    | 'tool'
    | 'food'
    | 'resource'
    | 'other'

const CATEGORY_ORDER = {
    'money': 0,
    'man': 10,
    'mount': 20,
    'weapon': 30,
    'armor': 40,
    'shield': 50,
    'tool': 60,
    'food': 70,
    'battle': 80,
    'resource': 90,
    'special': 100,
    'ship': 110,
    'monster': 120,
    'trade': 130,
    'other': 140,
}

export function getCategoryOrder(category: ItemCategory) {
    return CATEGORY_ORDER[category] || 0
}
