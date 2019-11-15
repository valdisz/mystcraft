export interface Faction {
    name: string;
    number: number;
}

export interface Attribute {
    key: string;
    value: number;
}

export interface FactionInfo {
    faction: Faction;
    attributes: Attribute[];
}

export interface GameDate {
    month: string;
    year: number;
}

export type Attitude = 'hostile' | 'unfriendly' | 'neutral' | 'friendly' | 'ally';

export interface Attitudes {
    default: Attitude;
    hostile: Faction[];
    unfriendly: Faction[];
    neutral: Faction[];
    friendly: Faction[];
    ally: Faction[];
}

export interface Coords {
    x: number;
    y: number;
    z?: number;
    label?: string;
}

export interface Location {
    terrain: string;
    coords: Coords;
    province: string;
}

export interface Population {
    amount: number;
    race: string;
}

export type SettlementSize = 'village' | 'town' | 'city';

export interface Settlement {
    name: string;
    size: SettlementSize;
}

export interface RegionHeader {
    location: Location;
    population?: Population;
    tax?: number;
    settlement?: Settlement;
}

export interface Wages {
    salary: number;
    maxWages: number;
}

export interface ItemBase {
    name: string;
    code: string;
}

export interface Item extends ItemBase {
    amount: number;
    needs?: number;
}

export interface ItemWithPrice extends Item {
    price: number;
}

export interface Capacity {
    flying: number;
    riding: number;
    walking: number;
    swimming: number;
}

export interface SkillBase {
    name: string;
    code: string;
}

export interface Skill extends SkillBase {
    level: number;
    days: number;
}

export interface Unit {
    name: string;
    description: string | null;
    number: number;
    own: boolean;
    faction?: Faction;
    flags: string[];
    weight?: number;
    capacity?: Capacity;
    skills?: Skill[];
    items: Item[];
    canStudy?: SkillBase[];
    attributes?: Attribute[];
}

export interface Structure {
    name: string;
    description: string | null;
    number: number;
    type: string;
    flag: string;
    units: Unit[];
}

export interface Region {
    regionHeader: RegionHeader;
    wages?: Wages;
    wanted: ItemWithPrice[];
    forSale: ItemWithPrice[];
    products: Item[];
    entertainment?: number;
    units: Unit[];
    structures: Structure[];
    attributes: Attribute[];
}

export interface Report {
    factionInfo: FactionInfo;
    date: GameDate;
    attitudes: Attitudes;
    regions: Region[];
    ordersTemplate: string;
}
