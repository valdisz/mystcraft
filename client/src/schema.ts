import gql from 'graphql-tag';
export type Maybe<T> = T | null;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: string;
  String: string;
  Boolean: boolean;
  Int: number;
  Float: number;
  /** The `Long` scalar type represents non-fractional signed whole 64-bit numeric values. Long can represent values between -(2^63) and 2^63 - 1. */
  Long: any;
  /** The `Upload` scalar type represents a file upload. */
  Upload: any;
};

export enum ApplyPolicy {
  AfterResolver = 'AFTER_RESOLVER',
  BeforeResolver = 'BEFORE_RESOLVER'
}

export type Attitude = {
  factionNumber: Scalars['Int'];
  stance: Stance;
};

export type AuthorizeDirective = {
  apply: ApplyPolicy;
  policy?: Maybe<Scalars['String']>;
  roles?: Maybe<Array<Scalars['String']>>;
};

export type Capacity = {
  flying: Scalars['Int'];
  riding: Scalars['Int'];
  swimming: Scalars['Int'];
  walking: Scalars['Int'];
};

/** Information about the offset pagination. */
export type CollectionSegmentInfo = {
  /** Indicates whether more items exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean'];
  /** Indicates whether more items exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean'];
};

export enum Direction {
  North = 'NORTH',
  Northeast = 'NORTHEAST',
  Northwest = 'NORTHWEST',
  South = 'SOUTH',
  Southeast = 'SOUTHEAST',
  Southwest = 'SOUTHWEST'
}

export type Event = {
  amount?: Maybe<Scalars['Int']>;
  category: EventCategory;
  factionNumber: Scalars['Int'];
  id: Scalars['Long'];
  itemCode?: Maybe<Scalars['String']>;
  itemName?: Maybe<Scalars['String']>;
  itemPrice?: Maybe<Scalars['Int']>;
  message: Scalars['String'];
  regionCode?: Maybe<Scalars['String']>;
  type: EventType;
  unitName?: Maybe<Scalars['String']>;
  unitNumber?: Maybe<Scalars['Int']>;
};

export enum EventCategory {
  Cast = 'CAST',
  Claim = 'CLAIM',
  Pillage = 'PILLAGE',
  Produce = 'PRODUCE',
  Sell = 'SELL',
  Tax = 'TAX',
  Unknown = 'UNKNOWN',
  Work = 'WORK'
}

export type EventCollectionSegment = {
  items?: Maybe<Array<Maybe<Event>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export enum EventType {
  Battle = 'BATTLE',
  Error = 'ERROR',
  Info = 'INFO'
}

export type Exit = {
  direction: Direction;
  targetRegion: Scalars['String'];
};

export type Faction = Node & {
  attitudes?: Maybe<Array<Maybe<Attitude>>>;
  defaultAttitude?: Maybe<Stance>;
  events?: Maybe<Array<Maybe<Event>>>;
  id: Scalars['ID'];
  name: Scalars['String'];
  number: Scalars['Int'];
  stats?: Maybe<FactionStats>;
};

export type FactionStats = {
  factionName?: Maybe<Scalars['String']>;
  factionNumber: Scalars['Int'];
  income?: Maybe<IncomeStats>;
  production?: Maybe<Array<Maybe<Item>>>;
};

export type FleetContent = {
  count: Scalars['Int'];
  type?: Maybe<Scalars['String']>;
};

export type Game = Node & {
  engineVersion: Scalars['String'];
  id: Scalars['ID'];
  me?: Maybe<Player>;
  name: Scalars['String'];
  options: GameOptions;
  players?: Maybe<Array<Maybe<Player>>>;
  ruleset: Scalars['String'];
  rulesetName: Scalars['String'];
  rulesetVersion: Scalars['String'];
  type: GameType;
};

export type GameOptions = {
  map?: Maybe<Array<Maybe<MapLevel>>>;
  schedule?: Maybe<Scalars['String']>;
};

export type GameOptionsInput = {
  map?: Maybe<Array<Maybe<MapLevelInput>>>;
  schedule?: Maybe<Scalars['String']>;
};

export enum GameType {
  Local = 'LOCAL',
  Remote = 'REMOTE'
}

export type IncomeStats = {
  pillage: Scalars['Int'];
  tax: Scalars['Int'];
  total: Scalars['Int'];
  trade: Scalars['Int'];
  work: Scalars['Int'];
};

export type Item = {
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
};

export type MapLevel = {
  height: Scalars['Int'];
  label?: Maybe<Scalars['String']>;
  level: Scalars['Int'];
  width: Scalars['Int'];
};

export type MapLevelInput = {
  height: Scalars['Int'];
  label?: Maybe<Scalars['String']>;
  level: Scalars['Int'];
  width: Scalars['Int'];
};

export enum Market {
  ForSale = 'FOR_SALE',
  Wanted = 'WANTED'
}

export type MutationResultOfString = {
  data?: Maybe<Scalars['String']>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type MutationType = {
  createLocalGame?: Maybe<Game>;
  createUser?: Maybe<User>;
  deleteGame?: Maybe<Array<Maybe<Game>>>;
  joinGame?: Maybe<Player>;
  setOrders?: Maybe<MutationResultOfString>;
  updateUserRoles?: Maybe<User>;
};


export type MutationTypeCreateLocalGameArgs = {
  engine?: Maybe<Scalars['Upload']>;
  gameData?: Maybe<Scalars['Upload']>;
  name?: Maybe<Scalars['String']>;
  options?: Maybe<GameOptionsInput>;
  playerData?: Maybe<Scalars['Upload']>;
};


export type MutationTypeCreateUserArgs = {
  email?: Maybe<Scalars['String']>;
  password?: Maybe<Scalars['String']>;
};


export type MutationTypeDeleteGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeJoinGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeSetOrdersArgs = {
  orders?: Maybe<Scalars['String']>;
  unitId?: Maybe<Scalars['ID']>;
};


export type MutationTypeUpdateUserRolesArgs = {
  add?: Maybe<Array<Maybe<Scalars['String']>>>;
  remove?: Maybe<Array<Maybe<Scalars['String']>>>;
  userId: Scalars['ID'];
};

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type Player = Node & {
  game?: Maybe<Game>;
  id: Scalars['ID'];
  isQuit: Scalars['Boolean'];
  lastTurnId?: Maybe<Scalars['String']>;
  lastTurnNumber: Scalars['Int'];
  name?: Maybe<Scalars['String']>;
  number?: Maybe<Scalars['Int']>;
  password?: Maybe<Scalars['String']>;
  reports?: Maybe<Array<Maybe<Report>>>;
  turn?: Maybe<Turn>;
  turns?: Maybe<Array<Maybe<Turn>>>;
};


export type PlayerReportsArgs = {
  turn?: Maybe<Scalars['Int']>;
};


export type PlayerTurnArgs = {
  number: Scalars['Int'];
};

export type Query = {
  games?: Maybe<Array<Maybe<Game>>>;
  me?: Maybe<User>;
  /** Fetches an object given its ID. */
  node?: Maybe<Node>;
  /** Lookup nodes by a list of IDs. */
  nodes: Array<Maybe<Node>>;
  users?: Maybe<UserCollectionSegment>;
};


export type QueryNodeArgs = {
  id: Scalars['ID'];
};


export type QueryNodesArgs = {
  ids: Array<Scalars['ID']>;
};


export type QueryUsersArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
};

export type Region = Node & {
  code?: Maybe<Scalars['String']>;
  entertainment: Scalars['Int'];
  exits?: Maybe<Array<Maybe<Exit>>>;
  explored: Scalars['Boolean'];
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  id: Scalars['ID'];
  label: Scalars['String'];
  lastVisitedAt?: Maybe<Scalars['Int']>;
  population: Scalars['Int'];
  produces?: Maybe<Array<Maybe<Item>>>;
  province: Scalars['String'];
  race?: Maybe<Scalars['String']>;
  settlement?: Maybe<Settlement>;
  structures?: Maybe<Array<Maybe<Structure>>>;
  tax: Scalars['Int'];
  terrain: Scalars['String'];
  totalWages: Scalars['Int'];
  wages: Scalars['Float'];
  wanted?: Maybe<Array<Maybe<TradableItem>>>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type RegionCollectionSegment = {
  items?: Maybe<Array<Maybe<Region>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type Report = Node & {
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json?: Maybe<Scalars['String']>;
  source: Scalars['String'];
};

export type Sailors = {
  current: Scalars['Int'];
  required: Scalars['Int'];
};

export type Settlement = {
  name?: Maybe<Scalars['String']>;
  size: SettlementSize;
};

export enum SettlementSize {
  City = 'CITY',
  Town = 'TOWN',
  Village = 'VILLAGE'
}

export type Skill = {
  code?: Maybe<Scalars['String']>;
  days?: Maybe<Scalars['Int']>;
  level?: Maybe<Scalars['Int']>;
};

export enum Stance {
  Ally = 'ALLY',
  Friendly = 'FRIENDLY',
  Hostile = 'HOSTILE',
  Neutral = 'NEUTRAL',
  Unfriendly = 'UNFRIENDLY'
}

export type Structure = Node & {
  code?: Maybe<Scalars['String']>;
  contents?: Maybe<Array<Maybe<FleetContent>>>;
  description?: Maybe<Scalars['String']>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  load?: Maybe<TransportationLoad>;
  name: Scalars['String'];
  needs?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  regionCode?: Maybe<Scalars['String']>;
  sailDirections?: Maybe<Array<Direction>>;
  sailors?: Maybe<Sailors>;
  sequence: Scalars['Int'];
  speed?: Maybe<Scalars['Int']>;
  type: Scalars['String'];
};

export type StructureCollectionSegment = {
  items?: Maybe<Array<Maybe<Structure>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type TradableItem = {
  amount: Scalars['Int'];
  code: Scalars['String'];
  market: Market;
  price: Scalars['Int'];
};

export type TransportationLoad = {
  max: Scalars['Int'];
  used: Scalars['Int'];
};

export type Turn = Node & {
  events?: Maybe<EventCollectionSegment>;
  factions?: Maybe<Array<Maybe<Faction>>>;
  id: Scalars['ID'];
  month: Scalars['Int'];
  number: Scalars['Int'];
  ready: Scalars['Boolean'];
  regions?: Maybe<RegionCollectionSegment>;
  reports?: Maybe<Array<Maybe<Report>>>;
  structures?: Maybe<StructureCollectionSegment>;
  units?: Maybe<UnitCollectionSegment>;
  year: Scalars['Int'];
};


export type TurnEventsArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
};


export type TurnRegionsArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
  withStructures?: Scalars['Boolean'];
};


export type TurnStructuresArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
};


export type TurnUnitsArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
};

export type Unit = Node & {
  canStudy?: Maybe<Array<Maybe<Scalars['String']>>>;
  capacity?: Maybe<Capacity>;
  combatSpell?: Maybe<Scalars['String']>;
  description?: Maybe<Scalars['String']>;
  factionNumber?: Maybe<Scalars['Int']>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  items: Array<Maybe<Item>>;
  name: Scalars['String'];
  number: Scalars['Int'];
  onGuard: Scalars['Boolean'];
  orders?: Maybe<Scalars['String']>;
  readyItem?: Maybe<Scalars['String']>;
  regionCode?: Maybe<Scalars['String']>;
  sequence: Scalars['Int'];
  skills?: Maybe<Array<Maybe<Skill>>>;
  strcutureNumber?: Maybe<Scalars['Int']>;
  structureNumber?: Maybe<Scalars['Int']>;
  weight?: Maybe<Scalars['Int']>;
};

export type UnitCollectionSegment = {
  items?: Maybe<Array<Maybe<Unit>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type User = Node & {
  email: Scalars['String'];
  id: Scalars['ID'];
  players?: Maybe<Array<Maybe<Player>>>;
  roles?: Maybe<Array<Maybe<Scalars['String']>>>;
};

export type UserCollectionSegment = {
  items?: Maybe<Array<Maybe<User>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type AttitudeFragment = { factionNumber: number, stance: Stance };

export type CapacityFragment = { walking: number, riding: number, flying: number, swimming: number };

export type EventFragment = { type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined };

export type ExitFragment = { direction: Direction, targetRegion: string };

export type FactionFragment = { id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined };

export type FleetContentFragment = { type?: string | null | undefined, count: number };

export type GameDetailsFragment = { id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { map?: Array<{ label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined }, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type GameHeaderFragment = { id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type GameOptionsFragment = { map?: Array<{ label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined };

export type ItemFragment = { code?: string | null | undefined, amount: number };

export type LoadFragment = { used: number, max: number };

export type PlayerHeaderFragment = { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined };

export type RegionFragment = { id: string, code?: string | null | undefined, lastVisitedAt?: number | null | undefined, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null | undefined, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { name?: string | null | undefined, size: SettlementSize } | null | undefined, wanted?: Array<{ code: string, price: number, amount: number } | null | undefined> | null | undefined, produces?: Array<{ code?: string | null | undefined, amount: number } | null | undefined> | null | undefined, forSale?: Array<{ code: string, price: number, amount: number } | null | undefined> | null | undefined, exits?: Array<{ direction: Direction, targetRegion: string } | null | undefined> | null | undefined, structures?: Array<{ id: string, code?: string | null | undefined, regionCode?: string | null | undefined, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { used: number, max: number } | null | undefined, sailors?: { current: number, required: number } | null | undefined } | null | undefined> | null | undefined };

export type SailorsFragment = { current: number, required: number };

export type SetOrderResultFragment = { isSuccess: boolean, error?: string | null | undefined };

export type SettlementFragment = { name?: string | null | undefined, size: SettlementSize };

export type SkillFragment = { code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined };

export type StructureFragment = { id: string, code?: string | null | undefined, regionCode?: string | null | undefined, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { used: number, max: number } | null | undefined, sailors?: { current: number, required: number } | null | undefined };

export type TradableItemFragment = { code: string, price: number, amount: number };

export type TurnFragment = { id: string, number: number, month: number, year: number, factions?: Array<{ id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined };

export type UnitFragment = { id: string, regionCode?: string | null | undefined, structureNumber?: number | null | undefined, sequence: number, canStudy?: Array<string | null | undefined> | null | undefined, combatSpell?: string | null | undefined, description?: string | null | undefined, factionNumber?: number | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, number: number, onGuard: boolean, readyItem?: string | null | undefined, weight?: number | null | undefined, orders?: string | null | undefined, capacity?: { walking: number, riding: number, flying: number, swimming: number } | null | undefined, items: Array<{ code?: string | null | undefined, amount: number } | null | undefined>, skills?: Array<{ code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined } | null | undefined> | null | undefined };

export type CreateLocalGameMutationVariables = Exact<{
  name: Scalars['String'];
  options: GameOptionsInput;
  engine: Scalars['Upload'];
  playerData: Scalars['Upload'];
  gameData: Scalars['Upload'];
}>;


export type CreateLocalGameMutation = { createLocalGame?: { id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined };

export type DeleteGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type DeleteGameMutation = { deleteGame?: Array<{ id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined> | null | undefined };

export type JoinGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type JoinGameMutation = { joinGame?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type SetOrderMutationVariables = Exact<{
  unitId: Scalars['ID'];
  orders?: Maybe<Scalars['String']>;
}>;


export type SetOrderMutation = { setOrders?: { isSuccess: boolean, error?: string | null | undefined } | null | undefined };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { node?: { id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { map?: Array<{ label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined }, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | {} | null | undefined };

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Array<{ id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined> | null | undefined };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { node?: { regions?: { totalCount: number, pageInfo: { hasNextPage: boolean }, items?: Array<{ id: string, code?: string | null | undefined, lastVisitedAt?: number | null | undefined, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null | undefined, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { name?: string | null | undefined, size: SettlementSize } | null | undefined, wanted?: Array<{ code: string, price: number, amount: number } | null | undefined> | null | undefined, produces?: Array<{ code?: string | null | undefined, amount: number } | null | undefined> | null | undefined, forSale?: Array<{ code: string, price: number, amount: number } | null | undefined> | null | undefined, exits?: Array<{ direction: Direction, targetRegion: string } | null | undefined> | null | undefined, structures?: Array<{ id: string, code?: string | null | undefined, regionCode?: string | null | undefined, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { used: number, max: number } | null | undefined, sailors?: { current: number, required: number } | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | null | undefined } | {} | null | undefined };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { node?: { id: string, number: number, month: number, year: number, factions?: Array<{ id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | {} | null | undefined };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { node?: { units?: { totalCount: number, pageInfo: { hasNextPage: boolean }, items?: Array<{ id: string, regionCode?: string | null | undefined, structureNumber?: number | null | undefined, sequence: number, canStudy?: Array<string | null | undefined> | null | undefined, combatSpell?: string | null | undefined, description?: string | null | undefined, factionNumber?: number | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, number: number, onGuard: boolean, readyItem?: string | null | undefined, weight?: number | null | undefined, orders?: string | null | undefined, capacity?: { walking: number, riding: number, flying: number, swimming: number } | null | undefined, items: Array<{ code?: string | null | undefined, amount: number } | null | undefined>, skills?: Array<{ code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | null | undefined } | {} | null | undefined };

export const GameOptions = gql`
    fragment GameOptions on GameOptions {
  map {
    label
    level
    width
    height
  }
}
    `;
export const PlayerHeader = gql`
    fragment PlayerHeader on Player {
  id
  number
  name
  lastTurnNumber
  lastTurnId
}
    `;
export const GameDetails = gql`
    fragment GameDetails on Game {
  id
  name
  rulesetName
  rulesetVersion
  options {
    ...GameOptions
  }
  ruleset
  me {
    ...PlayerHeader
  }
}
    ${GameOptions}
${PlayerHeader}`;
export const GameHeader = gql`
    fragment GameHeader on Game {
  id
  name
  rulesetName
  rulesetVersion
  me {
    ...PlayerHeader
  }
}
    ${PlayerHeader}`;
export const Settlement = gql`
    fragment Settlement on Settlement {
  name
  size
}
    `;
export const TradableItem = gql`
    fragment TradableItem on TradableItem {
  code
  price
  amount
}
    `;
export const Item = gql`
    fragment Item on Item {
  code
  amount
}
    `;
export const Exit = gql`
    fragment Exit on Exit {
  direction
  targetRegion
}
    `;
export const FleetContent = gql`
    fragment FleetContent on FleetContent {
  type
  count
}
    `;
export const Load = gql`
    fragment Load on TransportationLoad {
  used
  max
}
    `;
export const Sailors = gql`
    fragment Sailors on Sailors {
  current
  required
}
    `;
export const Structure = gql`
    fragment Structure on Structure {
  id
  code
  regionCode
  sequence
  contents {
    ...FleetContent
  }
  description
  flags
  id
  load {
    ...Load
  }
  name
  needs
  number
  sailDirections
  sailors {
    ...Sailors
  }
  speed
  type
}
    ${FleetContent}
${Load}
${Sailors}`;
export const Region = gql`
    fragment Region on Region {
  id
  code
  lastVisitedAt
  explored
  x
  y
  z
  label
  terrain
  province
  settlement {
    ...Settlement
  }
  race
  population
  tax
  wages
  totalWages
  wanted {
    ...TradableItem
  }
  entertainment
  produces {
    ...Item
  }
  forSale {
    ...TradableItem
  }
  exits {
    ...Exit
  }
  structures {
    ...Structure
  }
}
    ${Settlement}
${TradableItem}
${Item}
${Exit}
${Structure}`;
export const SetOrderResult = gql`
    fragment SetOrderResult on MutationResultOfString {
  isSuccess
  error
}
    `;
export const Attitude = gql`
    fragment Attitude on Attitude {
  factionNumber
  stance
}
    `;
export const Event = gql`
    fragment Event on Event {
  type
  category
  message
  regionCode
  unitNumber
  unitName
  itemCode
  itemName
  itemPrice
  amount
}
    `;
export const Faction = gql`
    fragment Faction on Faction {
  id
  name
  number
  defaultAttitude
  attitudes {
    ...Attitude
  }
  events {
    ...Event
  }
}
    ${Attitude}
${Event}`;
export const Turn = gql`
    fragment Turn on Turn {
  id
  number
  month
  year
  factions {
    ...Faction
  }
}
    ${Faction}`;
export const Capacity = gql`
    fragment Capacity on Capacity {
  walking
  riding
  flying
  swimming
}
    `;
export const Skill = gql`
    fragment Skill on Skill {
  code
  level
  days
}
    `;
export const Unit = gql`
    fragment Unit on Unit {
  id
  regionCode
  structureNumber
  sequence
  canStudy
  capacity {
    ...Capacity
  }
  combatSpell
  description
  factionNumber
  flags
  items {
    ...Item
  }
  name
  number
  onGuard
  readyItem
  skills {
    ...Skill
  }
  weight
  orders
}
    ${Capacity}
${Item}
${Skill}`;
export const CreateLocalGame = gql`
    mutation CreateLocalGame($name: String!, $options: GameOptionsInput!, $engine: Upload!, $playerData: Upload!, $gameData: Upload!) {
  createLocalGame(
    name: $name
    options: $options
    engine: $engine
    playerData: $playerData
    gameData: $gameData
  ) {
    ...GameHeader
  }
}
    ${GameHeader}`;
export const DeleteGame = gql`
    mutation DeleteGame($gameId: ID!) {
  deleteGame(gameId: $gameId) {
    ...GameHeader
  }
}
    ${GameHeader}`;
export const JoinGame = gql`
    mutation JoinGame($gameId: ID!) {
  joinGame(gameId: $gameId) {
    ...PlayerHeader
  }
}
    ${PlayerHeader}`;
export const SetOrder = gql`
    mutation SetOrder($unitId: ID!, $orders: String) {
  setOrders(unitId: $unitId, orders: $orders) {
    ...SetOrderResult
  }
}
    ${SetOrderResult}`;
export const GetGame = gql`
    query GetGame($gameId: ID!) {
  node(id: $gameId) {
    ... on Game {
      ...GameDetails
    }
  }
}
    ${GameDetails}`;
export const GetGames = gql`
    query GetGames {
  games {
    ...GameHeader
  }
}
    ${GameHeader}`;
export const GetRegions = gql`
    query GetRegions($turnId: ID!, $skip: Int! = 0, $pageSize: Int! = 1000) {
  node(id: $turnId) {
    ... on Turn {
      regions(skip: $skip, take: $pageSize, withStructures: true) {
        totalCount
        pageInfo {
          hasNextPage
        }
        items {
          ...Region
        }
      }
    }
  }
}
    ${Region}`;
export const GetTurn = gql`
    query GetTurn($turnId: ID!) {
  node(id: $turnId) {
    ... on Turn {
      ...Turn
    }
  }
}
    ${Turn}`;
export const GetUnits = gql`
    query GetUnits($turnId: ID!, $skip: Int! = 0, $pageSize: Int! = 1000) {
  node(id: $turnId) {
    ... on Turn {
      units(skip: $skip, take: $pageSize) {
        totalCount
        pageInfo {
          hasNextPage
        }
        items {
          ...Unit
        }
      }
    }
  }
}
    ${Unit}`;