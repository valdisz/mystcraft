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
};



export type Item = {
  code?: Maybe<Scalars['String']>;
  amount: Scalars['Int'];
};

export type Query = {
  /** Fetches an object given its ID. */
  node?: Maybe<Node>;
  /** Lookup nodes by a list of IDs. */
  nodes: Array<Maybe<Node>>;
  games?: Maybe<Array<Maybe<Game>>>;
  users?: Maybe<UserCollectionSegment>;
  me?: Maybe<User>;
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

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type User = Node & {
  id: Scalars['ID'];
  email: Scalars['String'];
  roles?: Maybe<Array<Maybe<Scalars['String']>>>;
  players?: Maybe<Array<Maybe<Player>>>;
};

export type Game = Node & {
  id: Scalars['ID'];
  name: Scalars['String'];
  type: GameType;
  ruleset?: Maybe<Scalars['String']>;
  engineVersion?: Maybe<Scalars['String']>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
  options?: Maybe<GameOptions>;
  me?: Maybe<Player>;
  players?: Maybe<Array<Maybe<Player>>>;
};

export type Player = Node & {
  id: Scalars['ID'];
  number?: Maybe<Scalars['Int']>;
  name?: Maybe<Scalars['String']>;
  lastTurnNumber: Scalars['Int'];
  password?: Maybe<Scalars['String']>;
  isQuit: Scalars['Boolean'];
  game?: Maybe<Game>;
  lastTurnId?: Maybe<Scalars['String']>;
  reports?: Maybe<Array<Maybe<Report>>>;
  turns?: Maybe<Array<Maybe<Turn>>>;
  turn?: Maybe<Turn>;
};


export type PlayerReportsArgs = {
  turn?: Maybe<Scalars['Int']>;
};


export type PlayerTurnArgs = {
  number: Scalars['Int'];
};

export type Report = Node & {
  id: Scalars['ID'];
  factionNumber: Scalars['Int'];
  factionName: Scalars['String'];
  source: Scalars['String'];
  json?: Maybe<Scalars['String']>;
};

export type Turn = Node & {
  id: Scalars['ID'];
  number: Scalars['Int'];
  month: Scalars['Int'];
  year: Scalars['Int'];
  reports?: Maybe<Array<Maybe<Report>>>;
  regions?: Maybe<RegionCollectionSegment>;
  structures?: Maybe<StructureCollectionSegment>;
  units?: Maybe<UnitCollectionSegment>;
  events?: Maybe<EventCollectionSegment>;
  factions?: Maybe<Array<Maybe<Faction>>>;
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


export type TurnEventsArgs = {
  skip?: Maybe<Scalars['Int']>;
  take?: Maybe<Scalars['Int']>;
};

export type Region = Node & {
  id: Scalars['ID'];
  code?: Maybe<Scalars['String']>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
  explored: Scalars['Boolean'];
  lastVisitedAt?: Maybe<Scalars['Int']>;
  label: Scalars['String'];
  province: Scalars['String'];
  terrain: Scalars['String'];
  settlement?: Maybe<Settlement>;
  population: Scalars['Int'];
  race?: Maybe<Scalars['String']>;
  entertainment: Scalars['Int'];
  tax: Scalars['Int'];
  wages: Scalars['Float'];
  totalWages: Scalars['Int'];
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  wanted?: Maybe<Array<Maybe<TradableItem>>>;
  produces?: Maybe<Array<Maybe<Item>>>;
  exits?: Maybe<Array<Maybe<Exit>>>;
  structures?: Maybe<Array<Maybe<Structure>>>;
};

export type Unit = Node & {
  id: Scalars['ID'];
  regionCode?: Maybe<Scalars['String']>;
  structureNumber?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  strcutureNumber?: Maybe<Scalars['Int']>;
  factionNumber?: Maybe<Scalars['Int']>;
  sequence: Scalars['Int'];
  name: Scalars['String'];
  description?: Maybe<Scalars['String']>;
  onGuard: Scalars['Boolean'];
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  weight?: Maybe<Scalars['Int']>;
  items: Array<Maybe<Item>>;
  capacity?: Maybe<Capacity>;
  skills?: Maybe<Array<Maybe<Skill>>>;
  canStudy?: Maybe<Array<Maybe<Scalars['String']>>>;
  readyItem?: Maybe<Scalars['String']>;
  combatSpell?: Maybe<Scalars['String']>;
  orders?: Maybe<Scalars['String']>;
};

export type Structure = Node & {
  id: Scalars['ID'];
  code?: Maybe<Scalars['String']>;
  regionCode?: Maybe<Scalars['String']>;
  sequence: Scalars['Int'];
  number: Scalars['Int'];
  name: Scalars['String'];
  type: Scalars['String'];
  description?: Maybe<Scalars['String']>;
  contents?: Maybe<Array<Maybe<FleetContent>>>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  sailDirections?: Maybe<Array<Direction>>;
  speed?: Maybe<Scalars['Int']>;
  needs?: Maybe<Scalars['Int']>;
  load?: Maybe<TransportationLoad>;
  sailors?: Maybe<Sailors>;
};

export type Faction = Node & {
  id: Scalars['ID'];
  number: Scalars['Int'];
  name: Scalars['String'];
  defaultAttitude?: Maybe<Stance>;
  attitudes?: Maybe<Array<Maybe<Attitude>>>;
  events?: Maybe<Array<Maybe<Event>>>;
  stats?: Maybe<FactionStats>;
};

export type Event = {
  unitNumber?: Maybe<Scalars['Int']>;
  id: Scalars['Long'];
  factionNumber: Scalars['Int'];
  regionCode?: Maybe<Scalars['String']>;
  unitName?: Maybe<Scalars['String']>;
  type: EventType;
  category: EventCategory;
  message: Scalars['String'];
  amount?: Maybe<Scalars['Int']>;
  itemCode?: Maybe<Scalars['String']>;
  itemName?: Maybe<Scalars['String']>;
  itemPrice?: Maybe<Scalars['Int']>;
};

export type UserCollectionSegment = {
  items?: Maybe<Array<Maybe<User>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

/** Information about the offset pagination. */
export type CollectionSegmentInfo = {
  /** Indicates whether more items exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean'];
  /** Indicates whether more items exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean'];
};

export type MutationType = {
  createUser?: Maybe<User>;
  updateUserRoles?: Maybe<User>;
  createGame?: Maybe<Game>;
  joinGame?: Maybe<Player>;
  deleteGame?: Maybe<Array<Maybe<Game>>>;
  setOrders?: Maybe<MutationResultOfString>;
};


export type MutationTypeCreateUserArgs = {
  email?: Maybe<Scalars['String']>;
  password?: Maybe<Scalars['String']>;
};


export type MutationTypeUpdateUserRolesArgs = {
  userId: Scalars['ID'];
  add?: Maybe<Array<Maybe<Scalars['String']>>>;
  remove?: Maybe<Array<Maybe<Scalars['String']>>>;
};


export type MutationTypeCreateGameArgs = {
  name?: Maybe<Scalars['String']>;
};


export type MutationTypeJoinGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeDeleteGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationTypeSetOrdersArgs = {
  unitId?: Maybe<Scalars['ID']>;
  orders?: Maybe<Scalars['String']>;
};

export type MutationResultOfString = {
  isSuccess: Scalars['Boolean'];
  data?: Maybe<Scalars['String']>;
  error?: Maybe<Scalars['String']>;
};

export type AuthorizeDirective = {
  policy?: Maybe<Scalars['String']>;
  roles?: Maybe<Array<Scalars['String']>>;
  apply: ApplyPolicy;
};

export enum GameType {
  Local = 'LOCAL',
  Remote = 'REMOTE'
}

export type Settlement = {
  name?: Maybe<Scalars['String']>;
  size: SettlementSize;
};

export type TradableItem = {
  code: Scalars['String'];
  amount: Scalars['Int'];
  price: Scalars['Int'];
  market: Market;
};

export type Exit = {
  targetRegion: Scalars['String'];
  direction: Direction;
};

export type Capacity = {
  flying: Scalars['Int'];
  riding: Scalars['Int'];
  walking: Scalars['Int'];
  swimming: Scalars['Int'];
};

export type Skill = {
  code?: Maybe<Scalars['String']>;
  level?: Maybe<Scalars['Int']>;
  days?: Maybe<Scalars['Int']>;
};

export type FleetContent = {
  type?: Maybe<Scalars['String']>;
  count: Scalars['Int'];
};

export enum Direction {
  North = 'NORTH',
  Northeast = 'NORTHEAST',
  Southeast = 'SOUTHEAST',
  South = 'SOUTH',
  Southwest = 'SOUTHWEST',
  Northwest = 'NORTHWEST'
}

export type TransportationLoad = {
  used: Scalars['Int'];
  max: Scalars['Int'];
};

export type Sailors = {
  current: Scalars['Int'];
  required: Scalars['Int'];
};

export enum Stance {
  Hostile = 'HOSTILE',
  Unfriendly = 'UNFRIENDLY',
  Neutral = 'NEUTRAL',
  Friendly = 'FRIENDLY',
  Ally = 'ALLY'
}

export type Attitude = {
  factionNumber: Scalars['Int'];
  stance: Stance;
};


export enum EventType {
  Info = 'INFO',
  Battle = 'BATTLE',
  Error = 'ERROR'
}

export enum EventCategory {
  Unknown = 'UNKNOWN',
  Tax = 'TAX',
  Sell = 'SELL',
  Work = 'WORK',
  Produce = 'PRODUCE',
  Pillage = 'PILLAGE',
  Claim = 'CLAIM',
  Cast = 'CAST'
}

export type RegionCollectionSegment = {
  items?: Maybe<Array<Maybe<Region>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type StructureCollectionSegment = {
  items?: Maybe<Array<Maybe<Structure>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type UnitCollectionSegment = {
  items?: Maybe<Array<Maybe<Unit>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type EventCollectionSegment = {
  items?: Maybe<Array<Maybe<Event>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export enum Market {
  ForSale = 'FOR_SALE',
  Wanted = 'WANTED'
}

export enum SettlementSize {
  Village = 'VILLAGE',
  Town = 'TOWN',
  City = 'CITY'
}

export enum ApplyPolicy {
  BeforeResolver = 'BEFORE_RESOLVER',
  AfterResolver = 'AFTER_RESOLVER'
}

export type FactionStats = {
  factionNumber: Scalars['Int'];
  factionName?: Maybe<Scalars['String']>;
  income?: Maybe<IncomeStats>;
  production?: Maybe<Array<Maybe<Item>>>;
};

export type GameOptions = {
  map?: Maybe<Array<Maybe<MapLevel>>>;
};

export type MapLevel = {
  label?: Maybe<Scalars['String']>;
  level: Scalars['Int'];
  width: Scalars['Int'];
  height: Scalars['Int'];
};

export type IncomeStats = {
  work: Scalars['Int'];
  tax: Scalars['Int'];
  pillage: Scalars['Int'];
  trade: Scalars['Int'];
  total: Scalars['Int'];
};

export type AttitudeFragment = Pick<Attitude, 'factionNumber' | 'stance'>;

export type CapacityFragment = Pick<Capacity, 'walking' | 'riding' | 'flying' | 'swimming'>;

export type EventFragment = Pick<Event, 'type' | 'category' | 'message' | 'regionCode' | 'unitNumber' | 'unitName' | 'itemCode' | 'itemName' | 'itemPrice' | 'amount'>;

export type ExitFragment = Pick<Exit, 'direction' | 'targetRegion'>;

export type FactionFragment = (
  Pick<Faction, 'id' | 'name' | 'number' | 'defaultAttitude'>
  & { attitudes?: Maybe<Array<Maybe<AttitudeFragment>>>, events?: Maybe<Array<Maybe<EventFragment>>> }
);

export type FleetContentFragment = Pick<FleetContent, 'type' | 'count'>;

export type GameDetailsFragment = (
  Pick<Game, 'id' | 'name' | 'rulesetName' | 'rulesetVersion' | 'ruleset'>
  & { options?: Maybe<GameOptionsFragment>, me?: Maybe<PlayerHeaderFragment> }
);

export type GameHeaderFragment = (
  Pick<Game, 'id' | 'name' | 'rulesetName' | 'rulesetVersion'>
  & { me?: Maybe<PlayerHeaderFragment> }
);

export type GameOptionsFragment = { map?: Maybe<Array<Maybe<Pick<MapLevel, 'label' | 'level' | 'width' | 'height'>>>> };

export type ItemFragment = Pick<Item, 'code' | 'amount'>;

export type LoadFragment = Pick<TransportationLoad, 'used' | 'max'>;

export type PlayerHeaderFragment = Pick<Player, 'id' | 'number' | 'name' | 'lastTurnNumber' | 'lastTurnId'>;

export type RegionFragment = (
  Pick<Region, 'id' | 'code' | 'lastVisitedAt' | 'explored' | 'x' | 'y' | 'z' | 'label' | 'terrain' | 'province' | 'race' | 'population' | 'tax' | 'wages' | 'totalWages' | 'entertainment'>
  & { settlement?: Maybe<SettlementFragment>, wanted?: Maybe<Array<Maybe<TradableItemFragment>>>, produces?: Maybe<Array<Maybe<ItemFragment>>>, forSale?: Maybe<Array<Maybe<TradableItemFragment>>>, exits?: Maybe<Array<Maybe<ExitFragment>>>, structures?: Maybe<Array<Maybe<StructureFragment>>> }
);

export type SailorsFragment = Pick<Sailors, 'current' | 'required'>;

export type SetOrderResultFragment = Pick<MutationResultOfString, 'isSuccess' | 'error'>;

export type SettlementFragment = Pick<Settlement, 'name' | 'size'>;

export type SkillFragment = Pick<Skill, 'code' | 'level' | 'days'>;

export type StructureFragment = (
  Pick<Structure, 'id' | 'code' | 'regionCode' | 'sequence' | 'description' | 'flags' | 'name' | 'needs' | 'number' | 'sailDirections' | 'speed' | 'type'>
  & { contents?: Maybe<Array<Maybe<FleetContentFragment>>>, load?: Maybe<LoadFragment>, sailors?: Maybe<SailorsFragment> }
);

export type TradableItemFragment = Pick<TradableItem, 'code' | 'price' | 'amount'>;

export type TurnFragment = (
  Pick<Turn, 'id' | 'number' | 'month' | 'year'>
  & { factions?: Maybe<Array<Maybe<FactionFragment>>> }
);

export type UnitFragment = (
  Pick<Unit, 'id' | 'regionCode' | 'structureNumber' | 'sequence' | 'canStudy' | 'combatSpell' | 'description' | 'factionNumber' | 'flags' | 'name' | 'number' | 'onGuard' | 'readyItem' | 'weight' | 'orders'>
  & { capacity?: Maybe<CapacityFragment>, items: Array<Maybe<ItemFragment>>, skills?: Maybe<Array<Maybe<SkillFragment>>> }
);

export type CreateGameMutationVariables = Exact<{
  name: Scalars['String'];
}>;


export type CreateGameMutation = { createGame?: Maybe<GameHeaderFragment> };

export type DeleteGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type DeleteGameMutation = { deleteGame?: Maybe<Array<Maybe<GameHeaderFragment>>> };

export type JoinGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type JoinGameMutation = { joinGame?: Maybe<PlayerHeaderFragment> };

export type SetOrderMutationVariables = Exact<{
  unitId: Scalars['ID'];
  orders?: Maybe<Scalars['String']>;
}>;


export type SetOrderMutation = { setOrders?: Maybe<SetOrderResultFragment> };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { node?: Maybe<GameDetailsFragment> };

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Maybe<Array<Maybe<GameHeaderFragment>>> };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { node?: Maybe<{ regions?: Maybe<(
      Pick<RegionCollectionSegment, 'totalCount'>
      & { pageInfo: Pick<CollectionSegmentInfo, 'hasNextPage'>, items?: Maybe<Array<Maybe<RegionFragment>>> }
    )> }> };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { node?: Maybe<TurnFragment> };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { node?: Maybe<{ units?: Maybe<(
      Pick<UnitCollectionSegment, 'totalCount'>
      & { pageInfo: Pick<CollectionSegmentInfo, 'hasNextPage'>, items?: Maybe<Array<Maybe<UnitFragment>>> }
    )> }> };

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
export const CreateGame = gql`
    mutation CreateGame($name: String!) {
  createGame(name: $name) {
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