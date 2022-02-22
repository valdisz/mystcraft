import gql from 'graphql-tag';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
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

export type Alliance = Node & {
  __typename?: 'Alliance';
  id: Scalars['ID'];
  members?: Maybe<Array<Maybe<AllianceMember>>>;
  name: Scalars['String'];
};

export type AllianceCreateResult = {
  __typename?: 'AllianceCreateResult';
  alliance?: Maybe<Alliance>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type AllianceJoinResult = {
  __typename?: 'AllianceJoinResult';
  alliance?: Maybe<Alliance>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  membership?: Maybe<AllianceMember>;
};

export type AllianceMember = {
  __typename?: 'AllianceMember';
  canInvite: Scalars['Boolean'];
  name?: Maybe<Scalars['String']>;
  number?: Maybe<Scalars['Int']>;
  owner: Scalars['Boolean'];
  shareMap: Scalars['Boolean'];
  teachMages: Scalars['Boolean'];
  turn?: Maybe<AllianceMemberTurn>;
  turns?: Maybe<Array<Maybe<AllianceMemberTurn>>>;
};


export type AllianceMemberTurnArgs = {
  number: Scalars['Int'];
};

export type AllianceMemberTurn = {
  __typename?: 'AllianceMemberTurn';
  number: Scalars['Int'];
  stats?: Maybe<Statistics>;
  units?: Maybe<UnitCollectionSegment>;
};


export type AllianceMemberTurnUnitsArgs = {
  filter?: InputMaybe<UnitsFilterInput>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
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
  map?: InputMaybe<Array<InputMaybe<MapLevelInput>>>;
  schedule?: InputMaybe<Scalars['String']>;
  serverAddress?: InputMaybe<Scalars['String']>;
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
  label?: InputMaybe<Scalars['String']>;
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
  allianceCreate?: Maybe<AllianceCreateResult>;
  allianceJoin?: Maybe<AllianceJoinResult>;
  createLocalGame?: Maybe<Game>;
  createUser?: Maybe<User>;
  deleteGame?: Maybe<Array<Maybe<Game>>>;
  gameCreateRemote?: Maybe<GameCreateRemoteResult>;
  joinGame?: Maybe<Player>;
  setOrders?: Maybe<MutationResultOfString>;
  studyPlanStudy?: Maybe<StudyPlanResult>;
  studyPlanTarget?: Maybe<StudyPlanResult>;
  studyPlanTeach?: Maybe<StudyPlanResult>;
  updateUserRoles?: Maybe<User>;
};


export type MutationTypeAllianceCreateArgs = {
  name?: InputMaybe<Scalars['String']>;
  playerId: Scalars['ID'];
};


export type MutationTypeAllianceJoinArgs = {
  allianceId: Scalars['ID'];
  playerId: Scalars['ID'];
};


export type MutationTypeCreateLocalGameArgs = {
  engine?: InputMaybe<Scalars['Upload']>;
  gameData?: InputMaybe<Scalars['Upload']>;
  name?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<GameOptionsInput>;
  playerData?: InputMaybe<Scalars['Upload']>;
};


export type MutationTypeCreateUserArgs = {
  email?: InputMaybe<Scalars['String']>;
  password?: InputMaybe<Scalars['String']>;
};


export type MutationTypeDeleteGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeGameCreateRemoteArgs = {
  engineVersion?: InputMaybe<Scalars['String']>;
  name?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<GameOptionsInput>;
  rulesetName?: InputMaybe<Scalars['String']>;
  rulesetVersion?: InputMaybe<Scalars['String']>;
};


export type MutationTypeJoinGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeSetOrdersArgs = {
  orders?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationTypeStudyPlanStudyArgs = {
  skill?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationTypeStudyPlanTargetArgs = {
  level: Scalars['Int'];
  skill?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationTypeStudyPlanTeachArgs = {
  unitId?: InputMaybe<Scalars['ID']>;
  units?: InputMaybe<Array<Scalars['Int']>>;
};


export type MutationTypeUpdateUserRolesArgs = {
  add?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  remove?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  userId: Scalars['ID'];
};

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type Player = Node & {
  __typename?: 'Player';
  alliance?: Maybe<Alliance>;
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
  turn?: InputMaybe<Scalars['Int']>;
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
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
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

export type Statistics = {
  __typename?: 'Statistics';
  income?: Maybe<IncomeStats>;
  production?: Maybe<Array<Maybe<Item>>>;
};

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

export type StudyPlan = {
  __typename?: 'StudyPlan';
  study?: Maybe<Scalars['String']>;
  target?: Maybe<Skill>;
  teach?: Maybe<Array<Scalars['Int']>>;
  unitNumber: Scalars['Int'];
};

export type StudyPlanResult = {
  __typename?: 'StudyPlanResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  studyPlan?: Maybe<StudyPlan>;
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
  stats?: Maybe<Statistics>;
  structures?: Maybe<StructureCollectionSegment>;
  studyPlans?: Maybe<Array<Maybe<StudyPlan>>>;
  units?: Maybe<UnitCollectionSegment>;
  year: Scalars['Int'];
};


export type TurnEventsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type TurnRegionsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
  withStructures?: Scalars['Boolean'];
};


export type TurnStructuresArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type TurnUnitsArgs = {
  filter?: InputMaybe<UnitsFilterInput>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
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
  studyPlan?: Maybe<StudyPlan>;
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

export type UnitsFilterInput = {
  mages?: InputMaybe<Scalars['Boolean']>;
  own?: InputMaybe<Scalars['Boolean']>;
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

export type EventFragment = { __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null };

export type ExitFragment = { __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null };

export type FactionFragment = { __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null };

export type FleetContentFragment = { __typename?: 'FleetContent', type?: string | null, count: number };

export type GameDetailsFragment = { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null };

export type GameHeaderFragment = { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null };

export type GameOptionsFragment = { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null };

export type ItemFragment = { __typename?: 'Item', code?: string | null, amount: number };

export type LoadFragment = { __typename?: 'TransportationLoad', used: number, max: number };

export type MageFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null };

export type PlayerHeaderFragment = { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null };

export type RegionFragment = { __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null };

export type SailorsFragment = { __typename?: 'Sailors', current: number, required: number };

export type SetOrderResultFragment = { __typename?: 'MutationResultOfString', isSuccess: boolean, error?: string | null };

export type SettlementFragment = { __typename?: 'Settlement', name?: string | null, size: SettlementSize };

export type SkillFragment = { __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null };

export type StaisticsFragment = { __typename?: 'Statistics', income?: { __typename?: 'IncomeStats', work: number, tax: number, pillage: number, trade: number, total: number } | null, production?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null };

export type StructureFragment = { __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null };

export type StudyPlanFragment = { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null };

export type TradableItemFragment = { __typename?: 'TradableItem', code: string, price: number, amount: number };

export type TurnFragment = { __typename?: 'Turn', id: string, number: number, month: number, year: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null };

export type UnitFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null };

export type CreateLocalGameMutationVariables = Exact<{
  name: Scalars['String'];
  options: GameOptionsInput;
  engine: Scalars['Upload'];
  playerData: Scalars['Upload'];
  gameData: Scalars['Upload'];
}>;


export type CreateLocalGameMutation = { __typename?: 'MutationType', createLocalGame?: { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null } | null };

export type DeleteGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type DeleteGameMutation = { __typename?: 'MutationType', deleteGame?: Array<{ __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null } | null> | null };

export type GameCreateRemoteMutationVariables = Exact<{
  name: Scalars['String'];
  engineVersion: Scalars['String'];
  rulesetName: Scalars['String'];
  rulesetVersion: Scalars['String'];
  options: GameOptionsInput;
}>;


export type GameCreateRemoteMutation = { __typename?: 'MutationType', gameCreateRemote?: { __typename?: 'GameCreateRemoteResult', isSuccess: boolean, error?: string | null, game?: { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null } | null } | null };

export type JoinGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type JoinGameMutation = { __typename?: 'MutationType', joinGame?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null };

export type SetOrderMutationVariables = Exact<{
  unitId: Scalars['ID'];
  orders?: InputMaybe<Scalars['String']>;
}>;


export type SetOrderMutation = { __typename?: 'MutationType', setOrders?: { __typename?: 'MutationResultOfString', isSuccess: boolean, error?: string | null } | null };

export type StudyPlanStudyMutationVariables = Exact<{
  unitId: Scalars['ID'];
  skill: Scalars['String'];
}>;


export type StudyPlanStudyMutation = { __typename?: 'MutationType', studyPlanStudy?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type StudyPlanTargetMutationVariables = Exact<{
  unitId: Scalars['ID'];
  skill: Scalars['String'];
  level: Scalars['Int'];
}>;


export type StudyPlanTargetMutation = { __typename?: 'MutationType', studyPlanTarget?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type StudyPlanTeachMutationVariables = Exact<{
  unitId: Scalars['ID'];
  units: Array<Scalars['Int']> | Scalars['Int'];
}>;


export type StudyPlanTeachMutation = { __typename?: 'MutationType', studyPlanTeach?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type GetAllianceMagesQueryVariables = Exact<{
  gameId: Scalars['ID'];
  turn: Scalars['Int'];
}>;


export type GetAllianceMagesQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', me?: { __typename?: 'Player', alliance?: { __typename?: 'Alliance', id: string, name: string, members?: Array<{ __typename?: 'AllianceMember', number?: number | null, name?: string | null, turn?: { __typename?: 'AllianceMemberTurn', number: number, units?: { __typename?: 'UnitCollectionSegment', items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null> | null } | null } | null } | null> | null } | null } | null } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { __typename?: 'Query', games?: Array<{ __typename?: 'Game', id: string, name: string, rulesetName: string, rulesetVersion: string, me?: { __typename?: 'Player', id: string, number?: number | null, name?: string | null, lastTurnNumber: number, lastTurnId?: string | null } | null } | null> | null };

export type GetMeQueryVariables = Exact<{ [key: string]: never; }>;


export type GetMeQuery = { __typename?: 'Query', me?: { __typename?: 'User', id: string } | null };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', regions?: { __typename?: 'RegionCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null } | null> | null } | null } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnStatsQueryVariables = Exact<{
  playerId: Scalars['ID'];
}>;


export type GetTurnStatsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player', turns?: Array<{ __typename?: 'Turn', number: number, stats?: { __typename?: 'Statistics', income?: { __typename?: 'IncomeStats', work: number, tax: number, pillage: number, trade: number, total: number } | null, production?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null } | null } | null> | null } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', id: string, number: number, month: number, year: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'Player' } | { __typename?: 'Region' } | { __typename?: 'Report' } | { __typename?: 'Structure' } | { __typename?: 'Turn', units?: { __typename?: 'UnitCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null } | null> | null } | null } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

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
export const Skill = gql`
    fragment Skill on Skill {
  code
  level
  days
}
    `;
export const StudyPlan = gql`
    fragment StudyPlan on StudyPlan {
  target {
    code
    level
  }
  study
  teach
}
    `;
export const Mage = gql`
    fragment Mage on Unit {
  id
  x
  y
  z
  number
  name
  factionNumber
  skills {
    ...Skill
  }
  studyPlan {
    ...StudyPlan
  }
}
    ${Skill}
${StudyPlan}`;
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
export const Staistics = gql`
    fragment Staistics on Statistics {
  income {
    work
    tax
    pillage
    trade
    total
  }
  production {
    code
    amount
  }
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
export const StudyPlanStudy = gql`
    mutation StudyPlanStudy($unitId: ID!, $skill: String!) {
  studyPlanStudy(unitId: $unitId, skill: $skill) {
    isSuccess
    error
    studyPlan {
      ...StudyPlan
    }
  }
}
    ${StudyPlan}`;
export const StudyPlanTarget = gql`
    mutation StudyPlanTarget($unitId: ID!, $skill: String!, $level: Int!) {
  studyPlanTarget(unitId: $unitId, skill: $skill, level: $level) {
    isSuccess
    error
    studyPlan {
      ...StudyPlan
    }
  }
}
    ${StudyPlan}`;
export const StudyPlanTeach = gql`
    mutation StudyPlanTeach($unitId: ID!, $units: [Int!]!) {
  studyPlanTeach(unitId: $unitId, units: $units) {
    isSuccess
    error
    studyPlan {
      ...StudyPlan
    }
  }
}
    ${StudyPlan}`;
export const GetAllianceMages = gql`
    query GetAllianceMages($gameId: ID!, $turn: Int!) {
  node(id: $gameId) {
    ... on Game {
      me {
        alliance {
          id
          name
          members {
            number
            name
            turn(number: $turn) {
              number
              units(skip: 0, take: 100, filter: {own: true, mages: true}) {
                items {
                  ...Mage
                }
              }
            }
          }
        }
      }
    }
  }
}
    ${Mage}`;
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
export const GetMe = gql`
    query GetMe {
  me {
    id
  }
}
    `;
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
export const GetTurnStats = gql`
    query GetTurnStats($playerId: ID!) {
  node(id: $playerId) {
    ... on Player {
      turns {
        number
        stats {
          ...Staistics
        }
      }
    }
  }
}
    ${Staistics}`;
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