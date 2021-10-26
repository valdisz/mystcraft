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
  /** The multiplier path scalar represents a valid GraphQL multiplier path string. */
  MultiplierPath: any;
  PaginationAmount: any;
  /** The `Long` scalar type represents non-fractional signed whole 64-bit numeric values. Long can represent values between -(2^63) and 2^63 - 1. */
  Long: any;
};



export type Item = {
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
};

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type User = Node & {
  email: Scalars['String'];
  id: Scalars['ID'];
  players?: Maybe<Array<Maybe<Player>>>;
  roles?: Maybe<Array<Maybe<Scalars['String']>>>;
};

export type Game = Node & {
  engineVersion?: Maybe<Scalars['String']>;
  id: Scalars['ID'];
  me?: Maybe<Player>;
  name: Scalars['String'];
  options?: Maybe<GameOptions>;
  players?: Maybe<Array<Maybe<Player>>>;
  ruleset?: Maybe<Scalars['String']>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
  type: GameType;
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

export type Report = Node & {
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json?: Maybe<Scalars['String']>;
  source: Scalars['String'];
};

export type Turn = Node & {
  factions?: Maybe<Array<Maybe<Faction>>>;
  id: Scalars['ID'];
  month: Scalars['Int'];
  number: Scalars['Int'];
  regions?: Maybe<RegionConnection>;
  reports?: Maybe<Array<Maybe<Report>>>;
  structures?: Maybe<StructureConnection>;
  units?: Maybe<UnitConnection>;
  year: Scalars['Int'];
};


export type TurnRegionsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};


export type TurnStructuresArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};


export type TurnUnitsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};

export type Region = Node & {
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
  tax: Scalars['Int'];
  terrain: Scalars['String'];
  totalWages: Scalars['Int'];
  wages: Scalars['Float'];
  wanted?: Maybe<Array<Maybe<TradableItem>>>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
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
  region?: Maybe<Scalars['String']>;
  sequence: Scalars['Int'];
  skills?: Maybe<Array<Maybe<Skill>>>;
  strcutureNumber?: Maybe<Scalars['Int']>;
  structure?: Maybe<Scalars['Int']>;
  weight?: Maybe<Scalars['Int']>;
};

export type Structure = Node & {
  contents?: Maybe<Array<Maybe<FleetContent>>>;
  description?: Maybe<Scalars['String']>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  load?: Maybe<TransportationLoad>;
  name: Scalars['String'];
  needs?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  region?: Maybe<Scalars['String']>;
  sailDirections?: Maybe<Array<Direction>>;
  sailors?: Maybe<Sailors>;
  sequence: Scalars['Int'];
  speed?: Maybe<Scalars['Int']>;
  type: Scalars['String'];
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

export type Query = {
  games?: Maybe<Array<Maybe<Game>>>;
  me?: Maybe<User>;
  node?: Maybe<Node>;
  users?: Maybe<UserConnection>;
};


export type QueryNodeArgs = {
  id: Scalars['ID'];
};


export type QueryUsersArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};



/** A connection to a list of items. */
export type UserConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<UserEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<User>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** A connection to a list of items. */
export type UnitConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<UnitEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Unit>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** Information about pagination in a connection. */
export type PageInfo = {
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']>;
};

/** An edge in a connection. */
export type UserEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<User>;
};

/** An edge in a connection. */
export type StructureEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Structure>;
};

export type Mutation = {
  createGame?: Maybe<Game>;
  createUser?: Maybe<User>;
  deleteGame?: Maybe<Array<Maybe<Game>>>;
  joinGame?: Maybe<Player>;
  updateUserRoles?: Maybe<User>;
};


export type MutationCreateGameArgs = {
  name?: Maybe<Scalars['String']>;
};


export type MutationCreateUserArgs = {
  email?: Maybe<Scalars['String']>;
  password?: Maybe<Scalars['String']>;
};


export type MutationDeleteGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationJoinGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationUpdateUserRolesArgs = {
  add?: Maybe<Array<Maybe<Scalars['String']>>>;
  remove?: Maybe<Array<Maybe<Scalars['String']>>>;
  userId: Scalars['ID'];
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
  amount: Scalars['Int'];
  code: Scalars['String'];
  market: Market;
  price: Scalars['Int'];
};

export type Exit = {
  direction: Direction;
  targetRegion: Scalars['String'];
};

export type Capacity = {
  flying: Scalars['Int'];
  riding: Scalars['Int'];
  swimming: Scalars['Int'];
  walking: Scalars['Int'];
};

export type Skill = {
  code?: Maybe<Scalars['String']>;
  days?: Maybe<Scalars['Int']>;
  level?: Maybe<Scalars['Int']>;
};

export type FleetContent = {
  count: Scalars['Int'];
  type?: Maybe<Scalars['String']>;
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
  max: Scalars['Int'];
  used: Scalars['Int'];
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

/** A connection to a list of items. */
export type RegionConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<RegionEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Region>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** A connection to a list of items. */
export type StructureConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<StructureEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Structure>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** An edge in a connection. */
export type UnitEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Unit>;
};

/** An edge in a connection. */
export type RegionEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Region>;
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

export type FactionStats = {
  factionName?: Maybe<Scalars['String']>;
  factionNumber: Scalars['Int'];
  income?: Maybe<IncomeStats>;
  production?: Maybe<Array<Maybe<Item>>>;
};

export type Event = {
  amount?: Maybe<Scalars['Int']>;
  category: EventCategory;
  id: Scalars['Long'];
  itemCode?: Maybe<Scalars['String']>;
  itemName?: Maybe<Scalars['String']>;
  itemPrice?: Maybe<Scalars['Int']>;
  message: Scalars['String'];
  type: EventType;
  unitName?: Maybe<Scalars['String']>;
};

export type GameOptions = {
  map?: Maybe<Array<Maybe<MapLevel>>>;
};

export type MapLevel = {
  height: Scalars['Int'];
  label?: Maybe<Scalars['String']>;
  level: Scalars['Int'];
  width: Scalars['Int'];
  _Clone__: MapLevel;
};

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

export enum EventType {
  Info = 'INFO',
  Battle = 'BATTLE',
  Error = 'ERROR'
}


export type IncomeStats = {
  pillage: Scalars['Int'];
  tax: Scalars['Int'];
  total: Scalars['Int'];
  trade: Scalars['Int'];
  work: Scalars['Int'];
};

export type AttitudeFragment = Pick<Attitude, 'factionNumber' | 'stance'>;

export type CapacityFragment = Pick<Capacity, 'walking' | 'riding' | 'flying' | 'swimming'>;

export type ExitFragment = Pick<Exit, 'direction' | 'targetRegion'>;

export type FactionFragment = (
  Pick<Faction, 'id' | 'name' | 'number' | 'defaultAttitude'>
  & { attitudes?: Maybe<Array<Maybe<AttitudeFragment>>> }
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
  Pick<Region, 'id' | 'lastVisitedAt' | 'explored' | 'x' | 'y' | 'z' | 'label' | 'terrain' | 'province' | 'race' | 'population' | 'tax' | 'wages' | 'totalWages' | 'entertainment'>
  & { settlement?: Maybe<SettlementFragment>, wanted?: Maybe<Array<Maybe<TradableItemFragment>>>, produces?: Maybe<Array<Maybe<ItemFragment>>>, forSale?: Maybe<Array<Maybe<TradableItemFragment>>>, exits?: Maybe<Array<Maybe<ExitFragment>>> }
);

export type SailorsFragment = Pick<Sailors, 'current' | 'required'>;

export type SettlementFragment = Pick<Settlement, 'name' | 'size'>;

export type SkillFragment = Pick<Skill, 'code' | 'level' | 'days'>;

export type StructureFragment = (
  Pick<Structure, 'id' | 'region' | 'sequence' | 'description' | 'flags' | 'name' | 'needs' | 'number' | 'sailDirections' | 'speed' | 'type'>
  & { contents?: Maybe<Array<Maybe<FleetContentFragment>>>, load?: Maybe<LoadFragment>, sailors?: Maybe<SailorsFragment> }
);

export type TradableItemFragment = Pick<TradableItem, 'code' | 'price' | 'amount'>;

export type TurnFragment = (
  Pick<Turn, 'id' | 'number' | 'month' | 'year'>
  & { factions?: Maybe<Array<Maybe<FactionFragment>>> }
);

export type UnitFragment = (
  Pick<Unit, 'id' | 'region' | 'structure' | 'sequence' | 'canStudy' | 'combatSpell' | 'description' | 'factionNumber' | 'flags' | 'name' | 'number' | 'onGuard' | 'readyItem' | 'weight' | 'orders'>
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

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { node?: Maybe<GameDetailsFragment> };

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Maybe<Array<Maybe<GameHeaderFragment>>> };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  cursor?: Maybe<Scalars['String']>;
  pageSize?: Scalars['PaginationAmount'];
}>;


export type GetRegionsQuery = { node?: Maybe<{ regions?: Maybe<(
      Pick<RegionConnection, 'totalCount'>
      & { pageInfo: Pick<PageInfo, 'hasNextPage' | 'endCursor'>, edges?: Maybe<Array<{ node?: Maybe<RegionFragment> }>> }
    )> }> };

export type GetStructuresQueryVariables = Exact<{
  turnId: Scalars['ID'];
  cursor?: Maybe<Scalars['String']>;
  pageSize?: Scalars['PaginationAmount'];
}>;


export type GetStructuresQuery = { node?: Maybe<{ structures?: Maybe<(
      Pick<StructureConnection, 'totalCount'>
      & { pageInfo: Pick<PageInfo, 'hasNextPage' | 'endCursor'>, edges?: Maybe<Array<{ node?: Maybe<StructureFragment> }>> }
    )> }> };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { node?: Maybe<TurnFragment> };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  cursor?: Maybe<Scalars['String']>;
  pageSize?: Scalars['PaginationAmount'];
}>;


export type GetUnitsQuery = { node?: Maybe<{ units?: Maybe<(
      Pick<UnitConnection, 'totalCount'>
      & { pageInfo: Pick<PageInfo, 'hasNextPage' | 'endCursor'>, edges?: Maybe<Array<{ node?: Maybe<UnitFragment> }>> }
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
}
    ${Settlement}
${TradableItem}
${Item}
${Exit}`;
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
  region
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
export const Attitude = gql`
    fragment Attitude on Attitude {
  factionNumber
  stance
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
}
    ${Attitude}`;
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
  region
  structure
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
    query GetRegions($turnId: ID!, $cursor: String, $pageSize: PaginationAmount! = 100) {
  node(id: $turnId) {
    ... on Turn {
      regions(after: $cursor, first: $pageSize) {
        totalCount
        pageInfo {
          hasNextPage
          endCursor
        }
        edges {
          node {
            ...Region
          }
        }
      }
    }
  }
}
    ${Region}`;
export const GetStructures = gql`
    query GetStructures($turnId: ID!, $cursor: String, $pageSize: PaginationAmount! = 100) {
  node(id: $turnId) {
    ... on Turn {
      structures(after: $cursor, first: $pageSize) {
        totalCount
        pageInfo {
          hasNextPage
          endCursor
        }
        edges {
          node {
            ...Structure
          }
        }
      }
    }
  }
}
    ${Structure}`;
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
    query GetUnits($turnId: ID!, $cursor: String, $pageSize: PaginationAmount! = 100) {
  node(id: $turnId) {
    ... on Turn {
      units(after: $cursor, first: $pageSize) {
        totalCount
        pageInfo {
          hasNextPage
          endCursor
        }
        edges {
          node {
            ...Unit
          }
        }
      }
    }
  }
}
    ${Unit}`;