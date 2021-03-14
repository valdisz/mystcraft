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

export type User = Node & {
  email: Scalars['String'];
  id: Scalars['ID'];
  players?: Maybe<Array<Maybe<Player>>>;
  roles?: Maybe<Array<Maybe<UserRole>>>;
};

export type Game = Node & {
  engineVersion?: Maybe<Scalars['String']>;
  id: Scalars['ID'];
  myPlayer?: Maybe<Player>;
  myUniversity?: Maybe<University>;
  name: Scalars['String'];
  players?: Maybe<Array<Maybe<Player>>>;
  remoteGameOptions?: Maybe<Scalars['String']>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
  type: GameType;
  universities?: Maybe<Array<Maybe<University>>>;
};

export type Player = Node & {
  factionName?: Maybe<Scalars['String']>;
  factionNumber?: Maybe<Scalars['Int']>;
  game?: Maybe<Game>;
  id: Scalars['ID'];
  lastTurnNumber: Scalars['Int'];
  password?: Maybe<Scalars['String']>;
  reports?: Maybe<Array<Maybe<Report>>>;
  turnByNumber?: Maybe<Turn>;
  turns?: Maybe<Array<Maybe<Turn>>>;
  university?: Maybe<PlayerUniversity>;
};


export type PlayerReportsArgs = {
  turn?: Maybe<Scalars['Long']>;
};


export type PlayerTurnByNumberArgs = {
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


export type RegionUnitsArgs = {
  insideStructures?: Scalars['Boolean'];
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

export type University = Node & {
  id: Scalars['ID'];
  members?: Maybe<Array<Maybe<UniversityMember>>>;
  name: Scalars['String'];
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
  joinGame?: Maybe<Player>;
  joinUniversity?: Maybe<University>;
  openUniversity?: Maybe<University>;
  updateUserRoles?: Maybe<User>;
};


export type MutationCreateGameArgs = {
  name?: Maybe<Scalars['String']>;
};


export type MutationCreateUserArgs = {
  email?: Maybe<Scalars['String']>;
  password?: Maybe<Scalars['String']>;
};


export type MutationJoinGameArgs = {
  gameId: Scalars['ID'];
};


export type MutationJoinUniversityArgs = {
  playerId: Scalars['ID'];
  universityId: Scalars['ID'];
};


export type MutationOpenUniversityArgs = {
  name?: Maybe<Scalars['String']>;
  playerId: Scalars['ID'];
};


export type MutationUpdateUserRolesArgs = {
  add?: Maybe<Array<Maybe<Scalars['String']>>>;
  remove?: Maybe<Array<Maybe<Scalars['String']>>>;
  userId: Scalars['ID'];
};

export type UserRole = {
  role?: Maybe<Scalars['String']>;
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

export enum SettlementSize {
  Village = 'VILLAGE',
  Town = 'TOWN',
  City = 'CITY'
}

export type UniversityMember = {
  player?: Maybe<Player>;
  role: UniveristyMemberRole;
};

export type Event = {
  faction?: Maybe<Faction>;
  id: Scalars['Long'];
  message: Scalars['String'];
  type: EventType;
};

export type PlayerUniversity = {
  role: UniveristyMemberRole;
  university?: Maybe<University>;
  _Clone__: PlayerUniversity;
};


export enum EventType {
  Info = 'INFO',
  Battle = 'BATTLE',
  Error = 'ERROR'
}

export enum UniveristyMemberRole {
  Owner = 'OWNER',
  Teacher = 'TEACHER',
  Member = 'MEMBER'
}

export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Maybe<Array<Maybe<GameListItemFragment>>> };

export type GameListItemFragment = (
  Pick<Game, 'id' | 'name' | 'rulesetName' | 'rulesetVersion'>
  & { myPlayer?: Maybe<PlayerItemFragment>, myUniversity?: Maybe<UniverisyItemFragment> }
);

export type PlayerItemFragment = Pick<Player, 'id' | 'factionNumber' | 'factionName' | 'lastTurnNumber'>;

export type UniverisyItemFragment = Pick<University, 'id' | 'name'>;

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  cursor?: Maybe<Scalars['String']>;
  pageSize?: Scalars['PaginationAmount'];
}>;


export type GetRegionsQuery = { node?: Maybe<{ regions?: Maybe<(
      Pick<RegionConnection, 'totalCount'>
      & { pageInfo: Pick<PageInfo, 'hasNextPage' | 'endCursor'>, edges?: Maybe<Array<{ node?: Maybe<RegionFragment> }>> }
    )> }> };

export type RegionFragment = (
  Pick<Region, 'id' | 'updatedAtTurn' | 'x' | 'y' | 'z' | 'label' | 'terrain' | 'province' | 'race' | 'population' | 'tax' | 'wages' | 'totalWages' | 'entertainment'>
  & { settlement?: Maybe<SettlementFragment>, wanted?: Maybe<Array<Maybe<TradableItemFragment>>>, products?: Maybe<Array<Maybe<ItemFragment>>>, forSale?: Maybe<Array<Maybe<TradableItemFragment>>>, exits?: Maybe<Array<Maybe<ExitFragment>>>, units?: Maybe<Array<Maybe<UnitFragment>>>, structures?: Maybe<Array<Maybe<StructureFragment>>> }
);

export type StructureFragment = (
  Pick<Structure, 'description' | 'flags' | 'id' | 'name' | 'needs' | 'number' | 'sailDirections' | 'speed' | 'type'>
  & { contents?: Maybe<Array<Maybe<FleetContentFragment>>>, load?: Maybe<LoadFragment>, sailors?: Maybe<SailorsFragment>, units?: Maybe<Array<Maybe<UnitFragment>>> }
);

export type FleetContentFragment = Pick<DbFleetContent, 'type' | 'count'>;

export type LoadFragment = Pick<DbTransportationLoad, 'used' | 'max'>;

export type SailorsFragment = Pick<DbSailors, 'current' | 'required'>;

export type UnitFragment = (
  Pick<Unit, 'description' | 'flags' | 'id' | 'name' | 'number' | 'onGuard' | 'weight'>
  & { canStudy?: Maybe<Array<Maybe<SkillFragment>>>, capacity?: Maybe<CapacityFragment>, combatSpell?: Maybe<SkillFragment>, faction?: Maybe<FactionFragment>, items: Array<Maybe<ItemFragment>>, readyItem?: Maybe<ItemFragment>, skills?: Maybe<Array<Maybe<SkillFragment>>> }
);

export type FactionFragment = Pick<Faction, 'id' | 'name' | 'number'>;

export type CapacityFragment = Pick<Capacity, 'walking' | 'riding' | 'flying' | 'swimming'>;

export type SkillFragment = Pick<Skill, 'code' | 'level' | 'days'>;

export type ItemFragment = Pick<Item, 'code' | 'amount'>;

export type TradableItemFragment = Pick<TradableItem, 'code' | 'price' | 'amount'>;

export type SettlementFragment = Pick<Settlement, 'name' | 'size'>;

export type ExitFragment = (
  Pick<Exit, 'direction' | 'x' | 'y' | 'z' | 'label' | 'terrain' | 'province'>
  & { settlement?: Maybe<SettlementFragment> }
);

export type GetSingleGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetSingleGameQuery = { node?: Maybe<SingleGameFragment> };

export type SingleGameFragment = (
  Pick<Game, 'id' | 'name' | 'engineVersion' | 'rulesetName' | 'rulesetVersion'>
  & { myPlayer?: Maybe<(
    Pick<Player, 'factionName' | 'factionNumber' | 'lastTurnNumber'>
    & { turns?: Maybe<Array<Maybe<TurnSummaryFragment>>> }
  )>, myUniversity?: Maybe<UniversitySummaryFragment> }
);

export type UniversitySummaryFragment = (
  Pick<University, 'id' | 'name'>
  & { members?: Maybe<Array<Maybe<(
    Pick<UniversityMember, 'role'>
    & { player?: Maybe<PlayerSummaryFragment> }
  )>>> }
);

export type PlayerSummaryFragment = Pick<Player, 'id' | 'factionName' | 'factionNumber' | 'lastTurnNumber'>;

export type TurnSummaryFragment = (
  Pick<Turn, 'id' | 'number' | 'month' | 'year'>
  & { reports?: Maybe<Array<Maybe<ReportSummaryFragment>>> }
);

export type ReportSummaryFragment = Pick<Report, 'id' | 'factionName' | 'factionNumber'>;

export type CreateGameMutationVariables = Exact<{
  name: Scalars['String'];
}>;


export type CreateGameMutation = { createGame?: Maybe<GameListItemFragment> };

export type JoinGameMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type JoinGameMutation = { joinGame?: Maybe<PlayerItemFragment> };

export const PlayerItem = gql`
    fragment PlayerItem on Player {
  id
  factionNumber
  factionName
  lastTurnNumber
}
    `;
export const UniverisyItem = gql`
    fragment UniverisyItem on University {
  id
  name
}
    `;
export const GameListItem = gql`
    fragment GameListItem on Game {
  id
  name
  rulesetName
  rulesetVersion
  myPlayer {
    ...PlayerItem
  }
  myUniversity {
    ...UniverisyItem
  }
}
    ${PlayerItem}
${UniverisyItem}`;
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
export const Skill = gql`
    fragment Skill on Skill {
  code
  level
  days
}
    `;
export const Capacity = gql`
    fragment Capacity on Capacity {
  walking
  riding
  flying
  swimming
}
    `;
export const Faction = gql`
    fragment Faction on Faction {
  id
  name
  number
}
    `;
export const Unit = gql`
    fragment Unit on Unit {
  canStudy {
    ...Skill
  }
  capacity {
    ...Capacity
  }
  combatSpell {
    ...Skill
  }
  description
  faction {
    ...Faction
  }
  flags
  id
  items {
    ...Item
  }
  name
  number
  onGuard
  readyItem {
    ...Item
  }
  skills {
    ...Skill
  }
  weight
}
    ${Skill}
${Capacity}
${Faction}
${Item}`;
export const FleetContent = gql`
    fragment FleetContent on DbFleetContent {
  type
  count
}
    `;
export const Load = gql`
    fragment Load on DbTransportationLoad {
  used
  max
}
    `;
export const Sailors = gql`
    fragment Sailors on DbSailors {
  current
  required
}
    `;
export const Structure = gql`
    fragment Structure on Structure {
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
  units {
    ...Unit
  }
}
    ${FleetContent}
${Load}
${Sailors}
${Unit}`;
export const Region = gql`
    fragment Region on Region {
  id
  updatedAtTurn
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
  products {
    ...Item
  }
  forSale {
    ...TradableItem
  }
  exits {
    ...Exit
  }
  units {
    ...Unit
  }
  structures {
    ...Structure
  }
}
    ${Settlement}
${TradableItem}
${Item}
${Exit}
${Unit}
${Structure}`;
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
export const PlayerSummary = gql`
    fragment PlayerSummary on Player {
  id
  factionName
  factionNumber
  lastTurnNumber
}
    `;
export const UniversitySummary = gql`
    fragment UniversitySummary on University {
  id
  name
  members {
    role
    player {
      ...PlayerSummary
    }
  }
}
    ${PlayerSummary}`;
export const SingleGame = gql`
    fragment SingleGame on Game {
  id
  name
  engineVersion
  rulesetName
  rulesetVersion
  myPlayer {
    factionName
    factionNumber
    lastTurnNumber
    turns {
      ...TurnSummary
    }
  }
  myUniversity {
    ...UniversitySummary
  }
}
    ${TurnSummary}
${UniversitySummary}`;
export const GetGames = gql`
    query GetGames {
  games {
    ...GameListItem
  }
}
    ${GameListItem}`;
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
export const GetSingleGame = gql`
    query GetSingleGame($gameId: ID!) {
  node(id: $gameId) {
    ... on Game {
      ...SingleGame
    }
  }
}
    ${SingleGame}`;
export const CreateGame = gql`
    mutation CreateGame($name: String!) {
  createGame(name: $name) {
    ...GameListItem
  }
}
    ${GameListItem}`;
export const JoinGame = gql`
    mutation JoinGame($gameId: ID!) {
  joinGame(gameId: $gameId) {
    ...PlayerItem
  }
}
    ${PlayerItem}`;