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
  __typename?: 'Attitude';
  factionNumber: Scalars['Int'];
  stance: Stance;
};

export type AuthorizeDirective = {
  __typename?: 'AuthorizeDirective';
  apply: ApplyPolicy;
  policy?: Maybe<Scalars['String']>;
  roles?: Maybe<Array<Scalars['String']>>;
};

export type Capacity = {
  __typename?: 'Capacity';
  flying: Scalars['Int'];
  riding: Scalars['Int'];
  swimming: Scalars['Int'];
  walking: Scalars['Int'];
};

/** Information about the offset pagination. */
export type CollectionSegmentInfo = {
  __typename?: 'CollectionSegmentInfo';
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
  __typename?: 'Event';
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
  __typename?: 'EventCollectionSegment';
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
  __typename?: 'Exit';
  direction: Direction;
  label: Scalars['String'];
  province: Scalars['String'];
  settlement?: Maybe<Settlement>;
  targetRegion: Scalars['String'];
  terrain: Scalars['String'];
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type Faction = Node & {
  __typename?: 'Faction';
  attitudes?: Maybe<Array<Maybe<Attitude>>>;
  defaultAttitude?: Maybe<Stance>;
  events?: Maybe<Array<Maybe<Event>>>;
  id: Scalars['ID'];
  name: Scalars['String'];
  number: Scalars['Int'];
  stats?: Maybe<FactionStats>;
};

export type FactionStats = {
  __typename?: 'FactionStats';
  factionName?: Maybe<Scalars['String']>;
  factionNumber: Scalars['Int'];
  income?: Maybe<IncomeStats>;
  production?: Maybe<Array<Maybe<Item>>>;
};

export type FleetContent = {
  __typename?: 'FleetContent';
  count: Scalars['Int'];
  type?: Maybe<Scalars['String']>;
};

export type Game = Node & {
  __typename?: 'Game';
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

export type GameCreateRemoteResult = {
  __typename?: 'GameCreateRemoteResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameOptions = {
  __typename?: 'GameOptions';
  map?: Maybe<Array<Maybe<MapLevel>>>;
  schedule?: Maybe<Scalars['String']>;
  serverAddress?: Maybe<Scalars['String']>;
};

export type GameOptionsInput = {
  map?: Maybe<Array<Maybe<MapLevelInput>>>;
  schedule?: Maybe<Scalars['String']>;
  serverAddress?: Maybe<Scalars['String']>;
};

export enum GameType {
  Local = 'LOCAL',
  Remote = 'REMOTE'
}

export type IncomeStats = {
  __typename?: 'IncomeStats';
  pillage: Scalars['Int'];
  tax: Scalars['Int'];
  total: Scalars['Int'];
  trade: Scalars['Int'];
  work: Scalars['Int'];
};

export type Item = {
  __typename?: 'Item';
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
};

export type MapLevel = {
  __typename?: 'MapLevel';
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
  __typename?: 'MutationResultOfString';
  data?: Maybe<Scalars['String']>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type MutationType = {
  __typename?: 'MutationType';
  createLocalGame?: Maybe<Game>;
  createUser?: Maybe<User>;
  deleteGame?: Maybe<Array<Maybe<Game>>>;
  gameCreateRemote?: Maybe<GameCreateRemoteResult>;
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


export type MutationTypeGameCreateRemoteArgs = {
  engineVersion?: Maybe<Scalars['String']>;
  name?: Maybe<Scalars['String']>;
  options?: Maybe<GameOptionsInput>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
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
  __typename?: 'Player';
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
  __typename?: 'Query';
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
  __typename?: 'Region';
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
  __typename?: 'RegionCollectionSegment';
  items?: Maybe<Array<Maybe<Region>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type Report = Node & {
  __typename?: 'Report';
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json?: Maybe<Scalars['String']>;
  source: Scalars['String'];
};

export type Sailors = {
  __typename?: 'Sailors';
  current: Scalars['Int'];
  required: Scalars['Int'];
};

export type Settlement = {
  __typename?: 'Settlement';
  name?: Maybe<Scalars['String']>;
  size: SettlementSize;
};

export enum SettlementSize {
  City = 'CITY',
  Town = 'TOWN',
  Village = 'VILLAGE'
}

export type Skill = {
  __typename?: 'Skill';
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
  __typename?: 'Structure';
  contents?: Maybe<Array<Maybe<FleetContent>>>;
  description?: Maybe<Scalars['String']>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  load?: Maybe<TransportationLoad>;
  name: Scalars['String'];
  needs?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  sailDirections?: Maybe<Array<Direction>>;
  sailors?: Maybe<Sailors>;
  sequence: Scalars['Int'];
  speed?: Maybe<Scalars['Int']>;
  type: Scalars['String'];
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type StructureCollectionSegment = {
  __typename?: 'StructureCollectionSegment';
  items?: Maybe<Array<Maybe<Structure>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type TradableItem = {
  __typename?: 'TradableItem';
  amount: Scalars['Int'];
  code: Scalars['String'];
  market: Market;
  price: Scalars['Int'];
};

export type TransportationLoad = {
  __typename?: 'TransportationLoad';
  max: Scalars['Int'];
  used: Scalars['Int'];
};

export type Turn = Node & {
  __typename?: 'Turn';
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
  __typename?: 'Unit';
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
  sequence: Scalars['Int'];
  skills?: Maybe<Array<Maybe<Skill>>>;
  structureNumber?: Maybe<Scalars['Int']>;
  weight?: Maybe<Scalars['Int']>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type UnitCollectionSegment = {
  __typename?: 'UnitCollectionSegment';
  items?: Maybe<Array<Maybe<Unit>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type User = Node & {
  __typename?: 'User';
  email: Scalars['String'];
  id: Scalars['ID'];
  players?: Maybe<Array<Maybe<Player>>>;
  roles?: Maybe<Array<Maybe<Scalars['String']>>>;
};

export type UserCollectionSegment = {
  __typename?: 'UserCollectionSegment';
  items?: Maybe<Array<Maybe<User>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type AttitudeFragment = { __typename?: 'Attitude', factionNumber: number, stance: Stance };

export type CapacityFragment = { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number };

export type EventFragment = { __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined };

export type ExitFragment = { __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize } | null | undefined };

export type FactionFragment = { __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined };

export type FleetContentFragment = { __typename?: 'FleetContent', type?: string | null | undefined, count: number };

export type GameDetailsFragment = { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined }, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type GameHeaderFragment = { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type GameOptionsFragment = { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined };

export type ItemFragment = { __typename?: 'Item', code?: string | null | undefined, amount: number };

export type LoadFragment = { __typename?: 'TransportationLoad', used: number, max: number };

export type PlayerHeaderFragment = { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined };

export type RegionFragment = { __typename?: 'Region', id: string, lastVisitedAt?: number | null | undefined, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null | undefined, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize } | null | undefined, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null | undefined> | null | undefined, produces?: Array<{ __typename?: 'Item', code?: string | null | undefined, amount: number } | null | undefined> | null | undefined, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null | undefined> | null | undefined, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize } | null | undefined } | null | undefined> | null | undefined, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null | undefined, sailors?: { __typename?: 'Sailors', current: number, required: number } | null | undefined } | null | undefined> | null | undefined };

export type SailorsFragment = { __typename?: 'Sailors', current: number, required: number };

export type SetOrderResultFragment = { __typename?: 'MutationResultOfString', isSuccess: boolean, error?: string | null | undefined };

export type SettlementFragment = { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize };

export type SkillFragment = { __typename?: 'Skill', code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined };

export type StructureFragment = { __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null | undefined, sailors?: { __typename?: 'Sailors', current: number, required: number } | null | undefined };

export type TradableItemFragment = { __typename?: 'TradableItem', code: string, price: number, amount: number };

export type TurnFragment = { __typename?: 'Turn', id: string, number: number, month: number, year: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined };

export type UnitFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null | undefined, sequence: number, canStudy?: Array<string | null | undefined> | null | undefined, combatSpell?: string | null | undefined, description?: string | null | undefined, factionNumber?: number | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, number: number, onGuard: boolean, readyItem?: string | null | undefined, weight?: number | null | undefined, orders?: string | null | undefined, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null | undefined, items: Array<{ __typename?: 'Item', code?: string | null | undefined, amount: number } | null | undefined>, skills?: Array<{ __typename?: 'Skill', code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined } | null | undefined> | null | undefined };

export type CreateLocalGameMutationVariables = Exact<{
  name: Scalars['String'];
  options: GameOptionsInput;
  engine: Scalars['Upload'];
  playerData: Scalars['Upload'];
  gameData: Scalars['Upload'];
}>;


export type CreateLocalGameMutation = { __typename?: 'MutationType', createLocalGame?: { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined };

export type DeleteGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type DeleteGameMutation = { __typename?: 'MutationType', deleteGame?: Array<{ __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined> | null | undefined };

export type GameCreateRemoteMutationVariables = Exact<{
  name: Scalars['String'];
  engineVersion: Scalars['String'];
  rulesetName: Scalars['String'];
  rulesetVersion: Scalars['String'];
  options: GameOptionsInput;
}>;


export type GameCreateRemoteMutation = { __typename?: 'MutationType', gameCreateRemote?: { __typename?: 'GameCreateRemoteResult', isSuccess: boolean, error?: string | null | undefined, game?: { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined } | null | undefined };

export type JoinGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type JoinGameMutation = { __typename?: 'MutationType', joinGame?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined };

export type SetOrderMutationVariables = Exact<{
  unitId: Scalars['ID'];
  orders?: Maybe<Scalars['String']>;
}>;


export type SetOrderMutation = { __typename?: 'MutationType', setOrders?: { __typename?: 'MutationResultOfString', isSuccess: boolean, error?: string | null | undefined } | null | undefined };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { __typename?: 'Query', node?: { __typename?: 'Faction' } | { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null | undefined, level: number, width: number, height: number } | null | undefined> | null | undefined }, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null | undefined };

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { __typename?: 'Query', games?: Array<{ __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null | undefined, name?: string | null | undefined, lastTurnNumber: number, lastTurnId?: string | null | undefined } | null | undefined } | null | undefined> | null | undefined };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { __typename?: 'Query', node?: { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', regions?: { __typename?: 'RegionCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Region', id: string, lastVisitedAt?: number | null | undefined, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null | undefined, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize } | null | undefined, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null | undefined> | null | undefined, produces?: Array<{ __typename?: 'Item', code?: string | null | undefined, amount: number } | null | undefined> | null | undefined, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null | undefined> | null | undefined, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null | undefined, size: SettlementSize } | null | undefined } | null | undefined> | null | undefined, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, needs?: number | null | undefined, number: number, sailDirections?: Array<Direction> | null | undefined, speed?: number | null | undefined, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null | undefined, count: number } | null | undefined> | null | undefined, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null | undefined, sailors?: { __typename?: 'Sailors', current: number, required: number } | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | null | undefined } | { __typename?: 'Unit' } | { __typename?: 'User' } | null | undefined };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { __typename?: 'Query', node?: { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', id: string, number: number, month: number, year: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null | undefined, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null | undefined> | null | undefined, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null | undefined, unitNumber?: number | null | undefined, unitName?: string | null | undefined, itemCode?: string | null | undefined, itemName?: string | null | undefined, itemPrice?: number | null | undefined, amount?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | { __typename?: 'Unit' } | { __typename?: 'User' } | null | undefined };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { __typename?: 'Query', node?: { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', units?: { __typename?: 'UnitCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null | undefined, sequence: number, canStudy?: Array<string | null | undefined> | null | undefined, combatSpell?: string | null | undefined, description?: string | null | undefined, factionNumber?: number | null | undefined, flags?: Array<string | null | undefined> | null | undefined, name: string, number: number, onGuard: boolean, readyItem?: string | null | undefined, weight?: number | null | undefined, orders?: string | null | undefined, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null | undefined, items: Array<{ __typename?: 'Item', code?: string | null | undefined, amount: number } | null | undefined>, skills?: Array<{ __typename?: 'Skill', code?: string | null | undefined, level?: number | null | undefined, days?: number | null | undefined } | null | undefined> | null | undefined } | null | undefined> | null | undefined } | null | undefined } | { __typename?: 'Unit' } | { __typename?: 'User' } | null | undefined };

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
  x
  y
  z
  label
  terrain
  province
  settlement {
    ...Settlement
  }
}
    ${Settlement}`;
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
  x
  y
  z
  sequence
  contents {
    ...FleetContent
  }
  description
  flags
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
  x
  y
  z
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
export const GameCreateRemote = gql`
    mutation GameCreateRemote($name: String!, $engineVersion: String!, $rulesetName: String!, $rulesetVersion: String!, $options: GameOptionsInput!) {
  gameCreateRemote(
    name: $name
    engineVersion: $engineVersion
    rulesetName: $rulesetName
    rulesetVersion: $rulesetVersion
    options: $options
  ) {
    isSuccess
    error
    game {
      ...GameHeader
    }
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