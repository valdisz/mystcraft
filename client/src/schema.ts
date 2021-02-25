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


/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type Game = Node & {
  engineVersion?: Maybe<Scalars['String']>;
  id: Scalars['ID'];
  lastTurnNumber: Scalars['Int'];
  name: Scalars['String'];
  password?: Maybe<Scalars['String']>;
  playerFactionName?: Maybe<Scalars['String']>;
  playerFactionNumber?: Maybe<Scalars['Int']>;
  reports?: Maybe<Array<Maybe<Report>>>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
  turnByNumber?: Maybe<Turn>;
  turns?: Maybe<Array<Maybe<Turn>>>;
};


export type GameReportsArgs = {
  turn?: Maybe<Scalars['Long']>;
};


export type GameTurnByNumberArgs = {
  turn: Scalars['Long'];
};

export type Report = Node & {
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json: Scalars['String'];
  source: Scalars['String'];
};

export type Turn = Node & {
  events?: Maybe<Array<Maybe<Event>>>;
  factions?: Maybe<Array<Maybe<Faction>>>;
  id: Scalars['ID'];
  month: Scalars['Int'];
  number: Scalars['Int'];
  regionByCoords?: Maybe<Region>;
  regions?: Maybe<RegionConnection>;
  reports?: Maybe<Array<Maybe<Report>>>;
  structures?: Maybe<StructureConnection>;
  unitByNumber?: Maybe<Unit>;
  units?: Maybe<UnitConnection>;
  year: Scalars['Int'];
};


export type TurnRegionByCoordsArgs = {
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
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


export type TurnUnitByNumberArgs = {
  number: Scalars['Int'];
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
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  id: Scalars['ID'];
  label: Scalars['String'];
  population: Scalars['Int'];
  products?: Maybe<Array<Maybe<Item>>>;
  province: Scalars['String'];
  race?: Maybe<Scalars['String']>;
  settlement?: Maybe<Settlement>;
  structureByNumber?: Maybe<Structure>;
  structures?: Maybe<Array<Maybe<Structure>>>;
  tax: Scalars['Int'];
  terrain: Scalars['String'];
  totalWages: Scalars['Int'];
  unitByNumber?: Maybe<Unit>;
  units?: Maybe<Array<Maybe<Unit>>>;
  updatedAtTurn: Scalars['Int'];
  wages: Scalars['Float'];
  wanted?: Maybe<Array<Maybe<TradableItem>>>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};


export type RegionStructureByNumberArgs = {
  number: Scalars['Int'];
};


export type RegionUnitByNumberArgs = {
  number: Scalars['Int'];
};

export type Unit = Node & {
  canStudy?: Maybe<Array<Maybe<Skill>>>;
  capacity?: Maybe<Capacity>;
  combatSpell?: Maybe<Skill>;
  description?: Maybe<Scalars['String']>;
  faction?: Maybe<Faction>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  items: Array<Maybe<Item>>;
  name: Scalars['String'];
  number: Scalars['Int'];
  onGuard: Scalars['Boolean'];
  readyItem?: Maybe<Item>;
  skills?: Maybe<Array<Maybe<Skill>>>;
  weight?: Maybe<Scalars['Int']>;
};

export type Structure = Node & {
  contents?: Maybe<Array<Maybe<DbFleetContent>>>;
  description?: Maybe<Scalars['String']>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  id: Scalars['ID'];
  load?: Maybe<DbTransportationLoad>;
  name: Scalars['String'];
  needs?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  sailDirections?: Maybe<Array<Direction>>;
  sailors?: Maybe<DbSailors>;
  speed?: Maybe<Scalars['Int']>;
  type: Scalars['String'];
  unitByNumber?: Maybe<Unit>;
  units?: Maybe<Array<Maybe<Unit>>>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};


export type StructureUnitByNumberArgs = {
  number: Scalars['Int'];
};

export type Faction = Node & {
  events?: Maybe<Array<Maybe<Event>>>;
  id: Scalars['ID'];
  name: Scalars['String'];
  number: Scalars['Int'];
  unitByNumber?: Maybe<Unit>;
};


export type FactionUnitByNumberArgs = {
  number: Scalars['Int'];
};

export type Query = {
  games?: Maybe<Array<Maybe<Game>>>;
  node?: Maybe<Node>;
};


export type QueryNodeArgs = {
  id: Scalars['ID'];
};


export type Mutation = {
  deleteGame?: Maybe<Scalars['String']>;
  newGame?: Maybe<Game>;
};


export type MutationDeleteGameArgs = {
  id: Scalars['Long'];
};


export type MutationNewGameArgs = {
  name?: Maybe<Scalars['String']>;
};

export type Settlement = {
  name?: Maybe<Scalars['String']>;
  size: SettlementSize;
};

export type TradableItem = {
  amount: Scalars['Int'];
  code: Scalars['String'];
  price: Scalars['Int'];
};

export type Item = {
  amount?: Maybe<Scalars['Int']>;
  code: Scalars['String'];
};

export type Exit = {
  direction: Direction;
  label: Scalars['String'];
  province: Scalars['String'];
  settlement?: Maybe<Settlement>;
  terrain: Scalars['String'];
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
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

export type DbFleetContent = {
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

export type DbTransportationLoad = {
  max: Scalars['Int'];
  used: Scalars['Int'];
};

export type DbSailors = {
  current: Scalars['Int'];
  required: Scalars['Int'];
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
export type UnitConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<UnitEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Unit>>>;
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
export type RegionEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Region>;
};

/** An edge in a connection. */
export type StructureEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Structure>;
};

/** An edge in a connection. */
export type UnitEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Unit>;
};

export enum SettlementSize {
  Village = 'VILLAGE',
  Town = 'TOWN',
  City = 'CITY'
}

export type Event = {
  faction?: Maybe<Faction>;
  id: Scalars['Long'];
  message: Scalars['String'];
  type: EventType;
};


export enum EventType {
  Info = 'INFO',
  Battle = 'BATTLE',
  Error = 'ERROR'
}

export type GameListItemFragment = Pick<Game, 'id' | 'name' | 'rulesetName' | 'rulesetVersion' | 'playerFactionNumber' | 'playerFactionName' | 'lastTurnNumber'>;

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Maybe<Array<Maybe<GameListItemFragment>>> };

export type GetSingleGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetSingleGameQuery = { node?: Maybe<SingleGameFragment> };

export type SingleGameFragment = (
  Pick<Game, 'id' | 'name' | 'engineVersion' | 'rulesetName' | 'rulesetVersion' | 'playerFactionName' | 'playerFactionNumber' | 'password' | 'lastTurnNumber'>
  & { turns?: Maybe<Array<Maybe<TurnSummaryFragment>>> }
);

export type TurnSummaryFragment = (
  Pick<Turn, 'id' | 'number' | 'month' | 'year'>
  & { reports?: Maybe<Array<Maybe<ReportSummaryFragment>>> }
);

export type ReportSummaryFragment = Pick<Report, 'id' | 'factionName' | 'factionNumber'>;

export type NewGameMutationVariables = Exact<{
  name: Scalars['String'];
}>;


export type NewGameMutation = { newGame?: Maybe<GameListItemFragment> };

export const GameListItem = gql`
    fragment GameListItem on Game {
  id
  name
  rulesetName
  rulesetVersion
  playerFactionNumber
  playerFactionName
  lastTurnNumber
}
    `;
export const ReportSummary = gql`
    fragment ReportSummary on Report {
  id
  factionName
  factionNumber
}
    `;
export const TurnSummary = gql`
    fragment TurnSummary on Turn {
  id
  number
  month
  year
  reports {
    ...ReportSummary
  }
}
    ${ReportSummary}`;
export const SingleGame = gql`
    fragment SingleGame on Game {
  id
  name
  engineVersion
  rulesetName
  rulesetVersion
  playerFactionName
  playerFactionNumber
  password
  lastTurnNumber
  turns {
    ...TurnSummary
  }
}
    ${TurnSummary}`;
export const GetGames = gql`
    query GetGames {
  games {
    ...GameListItem
  }
}
    ${GameListItem}`;
export const GetSingleGame = gql`
    query GetSingleGame($gameId: ID!) {
  node(id: $gameId) {
    ... on Game {
      ...SingleGame
    }
  }
}
    ${SingleGame}`;
export const NewGame = gql`
    mutation NewGame($name: String!) {
  newGame(name: $name) {
    ...GameListItem
  }
}
    ${GameListItem}`;