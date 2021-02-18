import gql from 'graphql-tag';
export type Maybe<T> = T | null;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
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
  reports?: Maybe<ReportConnection>;
  rulesetName?: Maybe<Scalars['String']>;
  rulesetVersion?: Maybe<Scalars['String']>;
  turn?: Maybe<Turn>;
  turns?: Maybe<TurnConnection>;
};


export type GameReportsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
  turn?: Maybe<Scalars['Long']>;
};


export type GameTurnArgs = {
  turn: Scalars['Long'];
};


export type GameTurnsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};

export type Report = Node & {
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json: Scalars['String'];
  source: Scalars['String'];
};

export type Turn = Node & {
  id: Scalars['ID'];
  month: Scalars['Int'];
  number: Scalars['Int'];
  regions?: Maybe<RegionConnection>;
  reports?: Maybe<ReportConnection>;
  year: Scalars['Int'];
};


export type TurnRegionsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};


export type TurnReportsArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};

export type Region = Node & {
  entertainment: Scalars['Int'];
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  id: Scalars['ID'];
  label: Scalars['String'];
  population: Scalars['Int'];
  products?: Maybe<Array<Maybe<Item>>>;
  province: Scalars['String'];
  race?: Maybe<Scalars['String']>;
  tax: Scalars['Int'];
  terrain: Scalars['String'];
  totalWages: Scalars['Int'];
  updatedAtTurn: Scalars['Int'];
  wages: Scalars['Float'];
  wanted?: Maybe<Array<Maybe<TradableItem>>>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type Query = {
  games?: Maybe<GameConnection>;
  node?: Maybe<Node>;
};


export type QueryGamesArgs = {
  after?: Maybe<Scalars['String']>;
  before?: Maybe<Scalars['String']>;
  first?: Maybe<Scalars['PaginationAmount']>;
  last?: Maybe<Scalars['PaginationAmount']>;
};


export type QueryNodeArgs = {
  id: Scalars['ID'];
};


/** A connection to a list of items. */
export type GameConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<GameEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Game>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
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
export type GameEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Game>;
};

/** An edge in a connection. */
export type TurnEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Turn>;
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

export type TradableItem = {
  code?: Maybe<Scalars['String']>;
  count: Scalars['Int'];
  name: Scalars['String'];
  price: Scalars['Int'];
};

export type Item = {
  code?: Maybe<Scalars['String']>;
  count: Scalars['Int'];
  name: Scalars['String'];
};

/** A connection to a list of items. */
export type ReportConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<ReportEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Report>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** A connection to a list of items. */
export type TurnConnection = {
  /** A list of edges. */
  edges?: Maybe<Array<TurnEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Maybe<Turn>>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  totalCount: Scalars['Int'];
};

/** An edge in a connection. */
export type RegionEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Region>;
};

/** An edge in a connection. */
export type ReportEdge = {
  /** A cursor for use in pagination. */
  cursor: Scalars['String'];
  /** The item at the end of the edge. */
  node?: Maybe<Report>;
};


export type GetGamesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetGamesQuery = { games?: Maybe<{ nodes?: Maybe<Array<Maybe<GameListItemFragment>>> }> };

export type NewGameMutationVariables = Exact<{
  name: Scalars['String'];
}>;


export type NewGameMutation = { newGame?: Maybe<GameListItemFragment> };

export type GameListItemFragment = (
  Pick<Game, 'id' | 'name' | 'rulesetName' | 'rulesetVersion' | 'playerFactionNumber' | 'playerFactionName' | 'lastTurnNumber'>
  & { turns?: Maybe<{ edges?: Maybe<Array<{ node?: Maybe<Pick<Turn, 'id' | 'number' | 'month' | 'year'>> }>> }> }
);

export type GetLastTurnMapQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetLastTurnMapQuery = { node?: Maybe<{ turns?: Maybe<{ nodes?: Maybe<Array<Maybe<Pick<Turn, 'id'>>>> }> }> };

export type GetMapQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetMapQuery = { node?: Maybe<{ regions?: Maybe<{ nodes?: Maybe<Array<Maybe<Pick<Region, 'id' | 'x' | 'y' | 'z' | 'label' | 'terrain' | 'province'>>>> }> }> };

export const GameListItem = gql`
    fragment GameListItem on Game {
  id
  name
  rulesetName
  rulesetVersion
  playerFactionNumber
  playerFactionName
  lastTurnNumber
  turns(last: 1) {
    edges {
      node {
        id
        number
        month
        year
      }
    }
  }
}
    `;
export const GetGames = gql`
    query GetGames {
  games {
    nodes {
      ...GameListItem
    }
  }
}
    ${GameListItem}`;
export const NewGame = gql`
    mutation NewGame($name: String!) {
  newGame(name: $name) {
    ...GameListItem
  }
}
    ${GameListItem}`;
export const GetLastTurnMap = gql`
    query GetLastTurnMap($gameId: ID!) {
  node(id: $gameId) {
    ... on Game {
      turns(last: 1) {
        nodes {
          id
        }
      }
    }
  }
}
    `;
export const GetMap = gql`
    query GetMap($turnId: ID!) {
  node(id: $turnId) {
    ... on Turn {
      regions {
        nodes {
          id
          x
          y
          z
          label
          terrain
          province
        }
      }
    }
  }
}
    `;