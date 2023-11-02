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
  Byte: any;
  DateTime: any;
  Long: any;
  Upload: any;
};

export type AdditionalReport = {
  __typename?: 'AdditionalReport';
  error?: Maybe<Scalars['String']>;
  json?: Maybe<Array<Scalars['Byte']>>;
  name: Scalars['String'];
  source: Array<Scalars['Byte']>;
  type: ReportType;
};

export type AdditionalReportCollectionSegment = {
  __typename?: 'AdditionalReportCollectionSegment';
  items?: Maybe<Array<Maybe<AdditionalReport>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type Alliance = Node & {
  __typename?: 'Alliance';
  id: Scalars['ID'];
  members?: Maybe<Array<Maybe<AllianceMember>>>;
  name: Scalars['String'];
};

export type AllianceCreateResult = MutationResult & {
  __typename?: 'AllianceCreateResult';
  alliance?: Maybe<Alliance>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type AllianceJoinResult = MutationResult & {
  __typename?: 'AllianceJoinResult';
  alliance?: Maybe<Alliance>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  membership?: Maybe<AllianceMember>;
};

export type AllianceMember = {
  __typename?: 'AllianceMember';
  acceptedAt?: Maybe<Scalars['DateTime']>;
  canInvite: Scalars['Boolean'];
  createdAt: Scalars['DateTime'];
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
  units?: Maybe<UnitCollectionSegment>;
};


export type AllianceMemberTurnUnitsArgs = {
  filter?: InputMaybe<UnitsFilterInput>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};

export type AnItem = {
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
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

export type BackgroundJob = {
  __typename?: 'BackgroundJob';
  id?: Maybe<Scalars['String']>;
  reason?: Maybe<Scalars['String']>;
  state: JobState;
};

export type Battle = {
  __typename?: 'Battle';
  attacker?: Maybe<Participant>;
  attackers?: Maybe<Array<Maybe<BattleUnit>>>;
  casualties?: Maybe<Array<Maybe<Casualties>>>;
  defender?: Maybe<Participant>;
  defenders?: Maybe<Array<Maybe<BattleUnit>>>;
  location?: Maybe<Location>;
  rose?: Maybe<Array<Maybe<BattleItem>>>;
  rounds?: Maybe<Array<Maybe<BattleRound>>>;
  spoils?: Maybe<Array<Maybe<BattleItem>>>;
  statistics?: Maybe<Scalars['String']>;
};

export type BattleFaction = {
  __typename?: 'BattleFaction';
  name?: Maybe<Scalars['String']>;
  number: Scalars['Int'];
};

export type BattleItem = AnItem & {
  __typename?: 'BattleItem';
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
};

export type BattleRound = {
  __typename?: 'BattleRound';
  log?: Maybe<Scalars['String']>;
  statistics?: Maybe<Scalars['String']>;
};

export type BattleSkill = {
  __typename?: 'BattleSkill';
  level: Scalars['Int'];
  name?: Maybe<Scalars['String']>;
};

export type BattleUnit = {
  __typename?: 'BattleUnit';
  description?: Maybe<Scalars['String']>;
  faction?: Maybe<BattleFaction>;
  flags?: Maybe<Array<Maybe<Scalars['String']>>>;
  items?: Maybe<Array<Maybe<BattleItem>>>;
  name?: Maybe<Scalars['String']>;
  number: Scalars['Int'];
  skills?: Maybe<Array<Maybe<BattleSkill>>>;
};

export type Capacity = {
  __typename?: 'Capacity';
  flying: Scalars['Int'];
  riding: Scalars['Int'];
  swimming: Scalars['Int'];
  walking: Scalars['Int'];
};

export type Casualties = {
  __typename?: 'Casualties';
  army?: Maybe<Participant>;
  damagedUnits?: Maybe<Array<Scalars['Int']>>;
  lost: Scalars['Int'];
};

/** Information about the offset pagination. */
export type CollectionSegmentInfo = {
  __typename?: 'CollectionSegmentInfo';
  /** Indicates whether more items exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean'];
  /** Indicates whether more items exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean'];
};

export type Coords = {
  __typename?: 'Coords';
  label?: Maybe<Scalars['String']>;
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
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
  Buy = 'BUY',
  Cast = 'CAST',
  Claim = 'CLAIM',
  Consume = 'CONSUME',
  Entertain = 'ENTERTAIN',
  Give = 'GIVE',
  Pillage = 'PILLAGE',
  Produce = 'PRODUCE',
  Sell = 'SELL',
  Study = 'STUDY',
  Tax = 'TAX',
  Unknown = 'UNKNOWN',
  Withdraw = 'WITHDRAW',
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
  targetRegion: Scalars['ID'];
  terrain: Scalars['String'];
  x: Scalars['Int'];
  y: Scalars['Int'];
  z: Scalars['Int'];
};

export type Expenses = {
  __typename?: 'Expenses';
  consume: Scalars['Int'];
  study: Scalars['Int'];
  total: Scalars['Int'];
  trade: Scalars['Int'];
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
  createdAt: Scalars['DateTime'];
  createdBy?: Maybe<User>;
  createdByUserId?: Maybe<Scalars['Long']>;
  id: Scalars['ID'];
  lastTurnNumber?: Maybe<Scalars['Int']>;
  me?: Maybe<Player>;
  name: Scalars['String'];
  nextTurnNumber?: Maybe<Scalars['Int']>;
  options: GameOptions;
  players?: Maybe<PlayerCollectionSegment>;
  status: GameStatus;
  turns?: Maybe<Array<Maybe<Turn>>>;
  type: GameType;
  updatedAt: Scalars['DateTime'];
  updatedBy?: Maybe<User>;
  updatedByUserId?: Maybe<Scalars['Long']>;
};


export type GamePlayersArgs = {
  quit?: Scalars['Boolean'];
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};

export type GameCollectionSegment = {
  __typename?: 'GameCollectionSegment';
  items?: Maybe<Array<Maybe<Game>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type GameCreateLocalResult = MutationResult & {
  __typename?: 'GameCreateLocalResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameCreateRemoteResult = MutationResult & {
  __typename?: 'GameCreateRemoteResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameDeleteResult = MutationResult & {
  __typename?: 'GameDeleteResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type GameEngine = Node & {
  __typename?: 'GameEngine';
  createdAt: Scalars['DateTime'];
  createdBy?: Maybe<User>;
  createdByUserId?: Maybe<Scalars['Long']>;
  description?: Maybe<Scalars['String']>;
  id: Scalars['ID'];
  name: Scalars['String'];
  remote: Scalars['Boolean'];
  remoteApi?: Maybe<Scalars['String']>;
  remoteOptions?: Maybe<Scalars['String']>;
  remoteUrl?: Maybe<Scalars['String']>;
  updatedAt: Scalars['DateTime'];
  updatedBy?: Maybe<User>;
  updatedByUserId?: Maybe<Scalars['Long']>;
};

export type GameEngineCollectionSegment = {
  __typename?: 'GameEngineCollectionSegment';
  items?: Maybe<Array<Maybe<GameEngine>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type GameEngineCreateLocalResult = MutationResult & {
  __typename?: 'GameEngineCreateLocalResult';
  engine?: Maybe<GameEngine>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type GameEngineCreateRemoteResult = MutationResult & {
  __typename?: 'GameEngineCreateRemoteResult';
  engine?: Maybe<GameEngine>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type GameEngineDeleteResult = MutationResult & {
  __typename?: 'GameEngineDeleteResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type GameJoinLocalResult = MutationResult & {
  __typename?: 'GameJoinLocalResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  registration?: Maybe<Registration>;
};

export type GameJoinRemoteResult = MutationResult & {
  __typename?: 'GameJoinRemoteResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  player?: Maybe<Player>;
};

export type GameNextTurnForceInput = {
  merge: Scalars['Boolean'];
  parse: Scalars['Boolean'];
  process: Scalars['Boolean'];
};

export type GameNextTurnResult = MutationResult & {
  __typename?: 'GameNextTurnResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
  jobId?: Maybe<Scalars['String']>;
};

export type GameOptions = {
  __typename?: 'GameOptions';
  finishAt?: Maybe<Scalars['DateTime']>;
  map?: Maybe<Array<Maybe<MapLevel>>>;
  schedule?: Maybe<Scalars['String']>;
  serverAddress?: Maybe<Scalars['String']>;
  startAt?: Maybe<Scalars['DateTime']>;
  timeZone?: Maybe<Scalars['String']>;
};

export type GameOptionsInput = {
  finishAt?: InputMaybe<Scalars['DateTime']>;
  map?: InputMaybe<Array<InputMaybe<MapLevelInput>>>;
  schedule?: InputMaybe<Scalars['String']>;
  serverAddress?: InputMaybe<Scalars['String']>;
  startAt?: InputMaybe<Scalars['DateTime']>;
  timeZone?: InputMaybe<Scalars['String']>;
};

export type GameOptionsSetResult = MutationResult & {
  __typename?: 'GameOptionsSetResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GamePauseResult = MutationResult & {
  __typename?: 'GamePauseResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameScheduleSetResult = MutationResult & {
  __typename?: 'GameScheduleSetResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameStartResult = MutationResult & {
  __typename?: 'GameStartResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export enum GameStatus {
  Locked = 'LOCKED',
  New = 'NEW',
  Paused = 'PAUSED',
  Running = 'RUNNING',
  Stoped = 'STOPED'
}

export type GameStopResult = MutationResult & {
  __typename?: 'GameStopResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export enum GameType {
  Local = 'LOCAL',
  Remote = 'REMOTE'
}

export type Income = {
  __typename?: 'Income';
  claim: Scalars['Int'];
  entertain: Scalars['Int'];
  pillage: Scalars['Int'];
  tax: Scalars['Int'];
  total: Scalars['Int'];
  trade: Scalars['Int'];
  work: Scalars['Int'];
};

export type Item = AnItem & {
  __typename?: 'Item';
  amount: Scalars['Int'];
  code: Scalars['String'];
};

export enum JobState {
  Deleted = 'DELETED',
  Failed = 'FAILED',
  Pending = 'PENDING',
  Running = 'RUNNING',
  Succeeded = 'SUCCEEDED',
  Unknown = 'UNKNOWN'
}

export type Location = {
  __typename?: 'Location';
  coords?: Maybe<Coords>;
  province?: Maybe<Scalars['String']>;
  terrain?: Maybe<Scalars['String']>;
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

export type Mutation = {
  __typename?: 'Mutation';
  allianceCreate?: Maybe<AllianceCreateResult>;
  allianceJoin?: Maybe<AllianceJoinResult>;
  deleteTurn: Scalars['Int'];
  gameComplete?: Maybe<GameStopResult>;
  gameCreateLocal?: Maybe<GameCreateLocalResult>;
  gameCreateRemote?: Maybe<GameCreateRemoteResult>;
  gameDelete?: Maybe<GameDeleteResult>;
  gameEngineCreateLocal?: Maybe<GameEngineCreateLocalResult>;
  gameEngineCreateRemote?: Maybe<GameEngineCreateRemoteResult>;
  gameEngineDelete?: Maybe<GameEngineDeleteResult>;
  gameJoinLocal?: Maybe<GameJoinLocalResult>;
  gameJoinRemote?: Maybe<GameJoinRemoteResult>;
  gameNextTurn?: Maybe<GameNextTurnResult>;
  gameOptionsSet?: Maybe<GameOptionsSetResult>;
  gamePause?: Maybe<GamePauseResult>;
  gameScheduleSet?: Maybe<GameScheduleSetResult>;
  gameStart?: Maybe<GameStartResult>;
  playerQuit?: Maybe<PlayerQuitResult>;
  setOrders?: Maybe<UnitOrdersSetResult>;
  setRuleset?: Maybe<Game>;
  studyPlanStudy?: Maybe<StudyPlanResult>;
  studyPlanTarget?: Maybe<StudyPlanResult>;
  studyPlanTeach?: Maybe<StudyPlanResult>;
  userCreate?: Maybe<User>;
  userRolesUpdate?: Maybe<User>;
};


export type MutationAllianceCreateArgs = {
  name?: InputMaybe<Scalars['String']>;
  playerId: Scalars['ID'];
};


export type MutationAllianceJoinArgs = {
  allianceId: Scalars['ID'];
  playerId: Scalars['ID'];
};


export type MutationDeleteTurnArgs = {
  turnId?: InputMaybe<Scalars['ID']>;
};


export type MutationGameCompleteArgs = {
  gameId: Scalars['ID'];
};


export type MutationGameCreateLocalArgs = {
  finishAt?: InputMaybe<Scalars['DateTime']>;
  gameEngineId: Scalars['ID'];
  gameIn?: InputMaybe<Scalars['Upload']>;
  levels?: InputMaybe<Array<InputMaybe<MapLevelInput>>>;
  name?: InputMaybe<Scalars['String']>;
  playersIn?: InputMaybe<Scalars['Upload']>;
  schedule?: InputMaybe<Scalars['String']>;
  startAt?: InputMaybe<Scalars['DateTime']>;
  timeZone?: InputMaybe<Scalars['String']>;
};


export type MutationGameCreateRemoteArgs = {
  finishAt?: InputMaybe<Scalars['DateTime']>;
  gameEngineId: Scalars['ID'];
  levels?: InputMaybe<Array<InputMaybe<MapLevelInput>>>;
  name?: InputMaybe<Scalars['String']>;
  schedule?: InputMaybe<Scalars['String']>;
  startAt?: InputMaybe<Scalars['DateTime']>;
  timeZone?: InputMaybe<Scalars['String']>;
};


export type MutationGameDeleteArgs = {
  gameId: Scalars['ID'];
};


export type MutationGameEngineCreateLocalArgs = {
  description?: InputMaybe<Scalars['String']>;
  engine?: InputMaybe<Scalars['Upload']>;
  name?: InputMaybe<Scalars['String']>;
  ruleset?: InputMaybe<Scalars['Upload']>;
};


export type MutationGameEngineCreateRemoteArgs = {
  api?: InputMaybe<Scalars['String']>;
  description?: InputMaybe<Scalars['String']>;
  name?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<Scalars['String']>;
  url?: InputMaybe<Scalars['String']>;
};


export type MutationGameEngineDeleteArgs = {
  gameEngineId: Scalars['ID'];
};


export type MutationGameJoinLocalArgs = {
  gameId: Scalars['ID'];
  name?: InputMaybe<Scalars['String']>;
};


export type MutationGameJoinRemoteArgs = {
  gameId: Scalars['ID'];
  password?: InputMaybe<Scalars['String']>;
  playerId: Scalars['ID'];
};


export type MutationGameNextTurnArgs = {
  force?: InputMaybe<GameNextTurnForceInput>;
  gameId: Scalars['ID'];
  turnNumber?: InputMaybe<Scalars['Int']>;
};


export type MutationGameOptionsSetArgs = {
  gameId: Scalars['ID'];
  options?: InputMaybe<GameOptionsInput>;
};


export type MutationGamePauseArgs = {
  gameId: Scalars['ID'];
};


export type MutationGameScheduleSetArgs = {
  gameId: Scalars['ID'];
  schedule?: InputMaybe<Scalars['String']>;
};


export type MutationGameStartArgs = {
  gameId: Scalars['ID'];
};


export type MutationPlayerQuitArgs = {
  playerId: Scalars['ID'];
};


export type MutationSetOrdersArgs = {
  orders?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationSetRulesetArgs = {
  gameId: Scalars['ID'];
  ruleset?: InputMaybe<Scalars['String']>;
};


export type MutationStudyPlanStudyArgs = {
  skill?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationStudyPlanTargetArgs = {
  level: Scalars['Int'];
  skill?: InputMaybe<Scalars['String']>;
  unitId?: InputMaybe<Scalars['ID']>;
};


export type MutationStudyPlanTeachArgs = {
  unitId?: InputMaybe<Scalars['ID']>;
  units?: InputMaybe<Array<Scalars['Int']>>;
};


export type MutationUserCreateArgs = {
  email?: InputMaybe<Scalars['String']>;
  name?: InputMaybe<Scalars['String']>;
  password?: InputMaybe<Scalars['String']>;
};


export type MutationUserRolesUpdateArgs = {
  add?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  remove?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  userId: Scalars['ID'];
};

export type MutationResult = {
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type Orders = {
  __typename?: 'Orders';
  orders?: Maybe<Scalars['String']>;
  turnNumber: Scalars['Int'];
  unitNumber: Scalars['Int'];
};

export type OrdersCollectionSegment = {
  __typename?: 'OrdersCollectionSegment';
  items?: Maybe<Array<Maybe<Orders>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type Participant = {
  __typename?: 'Participant';
  name?: Maybe<Scalars['String']>;
  number: Scalars['Int'];
};

export type Player = Node & {
  __typename?: 'Player';
  alliance?: Maybe<Alliance>;
  createdAt: Scalars['DateTime'];
  game?: Maybe<Game>;
  id: Scalars['ID'];
  isClaimed: Scalars['Boolean'];
  isQuit: Scalars['Boolean'];
  lastTurn?: Maybe<PlayerTurn>;
  lastTurnNumber?: Maybe<Scalars['Int']>;
  name?: Maybe<Scalars['String']>;
  nextTurn?: Maybe<PlayerTurn>;
  nextTurnNumber?: Maybe<Scalars['Int']>;
  number: Scalars['Int'];
  password?: Maybe<Scalars['String']>;
  reports?: Maybe<Array<Maybe<Report>>>;
  turn?: Maybe<PlayerTurn>;
  turns?: Maybe<PlayerTurnCollectionSegment>;
  updatedAt: Scalars['DateTime'];
};


export type PlayerTurnArgs = {
  number: Scalars['Int'];
};


export type PlayerTurnsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};

export type PlayerCollectionSegment = {
  __typename?: 'PlayerCollectionSegment';
  items?: Maybe<Array<Maybe<Player>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type PlayerQuitResult = MutationResult & {
  __typename?: 'PlayerQuitResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  player?: Maybe<Player>;
};

export type PlayerTurn = Node & {
  __typename?: 'PlayerTurn';
  additionalReports?: Maybe<AdditionalReportCollectionSegment>;
  battles?: Maybe<Array<Maybe<Battle>>>;
  events?: Maybe<EventCollectionSegment>;
  expenses?: Maybe<Expenses>;
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  factions?: Maybe<Array<Maybe<Faction>>>;
  id: Scalars['ID'];
  income?: Maybe<Income>;
  isOrdersSubmitted: Scalars['Boolean'];
  isProcessed: Scalars['Boolean'];
  isReady: Scalars['Boolean'];
  isTimesSubmitted: Scalars['Boolean'];
  orders?: Maybe<OrdersCollectionSegment>;
  ordersSubmittedAt?: Maybe<Scalars['DateTime']>;
  readyAt?: Maybe<Scalars['DateTime']>;
  regions?: Maybe<RegionCollectionSegment>;
  statistics?: Maybe<Array<Maybe<TurnStatisticsItem>>>;
  structures?: Maybe<StructureCollectionSegment>;
  studyPlans?: Maybe<Array<Maybe<StudyPlan>>>;
  timesSubmittedAt?: Maybe<Scalars['DateTime']>;
  treasury?: Maybe<Array<Maybe<TreasuryItem>>>;
  turnNumber: Scalars['Int'];
  unclaimed: Scalars['Int'];
  units?: Maybe<UnitCollectionSegment>;
};


export type PlayerTurnAdditionalReportsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type PlayerTurnEventsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type PlayerTurnOrdersArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
  unitNumber?: InputMaybe<Scalars['Int']>;
};


export type PlayerTurnRegionsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
  withStructures?: Scalars['Boolean'];
};


export type PlayerTurnStructuresArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type PlayerTurnUnitsArgs = {
  filter?: InputMaybe<UnitsFilterInput>;
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};

export type PlayerTurnCollectionSegment = {
  __typename?: 'PlayerTurnCollectionSegment';
  items?: Maybe<Array<Maybe<PlayerTurn>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type Query = {
  __typename?: 'Query';
  gameEngines?: Maybe<GameEngineCollectionSegment>;
  games?: Maybe<GameCollectionSegment>;
  job?: Maybe<BackgroundJob>;
  me?: Maybe<User>;
  /** Fetches an object given its ID. */
  node?: Maybe<Node>;
  /** Lookup nodes by a list of IDs. */
  nodes: Array<Maybe<Node>>;
  users?: Maybe<UserCollectionSegment>;
};


export type QueryGameEnginesArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type QueryGamesArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
};


export type QueryJobArgs = {
  jobId?: InputMaybe<Scalars['String']>;
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
  expenses?: Maybe<Expenses>;
  explored: Scalars['Boolean'];
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  gate?: Maybe<Scalars['Int']>;
  id: Scalars['ID'];
  income?: Maybe<Income>;
  label: Scalars['String'];
  lastVisitedAt?: Maybe<Scalars['Int']>;
  population: Scalars['Int'];
  produces?: Maybe<Array<Maybe<Item>>>;
  province: Scalars['String'];
  race?: Maybe<Scalars['String']>;
  settlement?: Maybe<Settlement>;
  statistics?: Maybe<Array<Maybe<RegionStatisticsItem>>>;
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

export type RegionStatisticsItem = AnItem & {
  __typename?: 'RegionStatisticsItem';
  amount: Scalars['Int'];
  category: StatisticsCategory;
  code: Scalars['String'];
};

export type Registration = {
  __typename?: 'Registration';
  createdAt: Scalars['DateTime'];
  id: Scalars['Long'];
  name?: Maybe<Scalars['String']>;
  password?: Maybe<Scalars['String']>;
};

export type Report = {
  __typename?: 'Report';
  error?: Maybe<Scalars['String']>;
  factionNumber: Scalars['Int'];
  isMerged: Scalars['Boolean'];
  isParsed: Scalars['Boolean'];
};

export enum ReportType {
  Map = 'MAP',
  Report = 'REPORT'
}

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

export enum StatisticsCategory {
  Bought = 'BOUGHT',
  Consumed = 'CONSUMED',
  Produced = 'PRODUCED',
  Sold = 'SOLD'
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

export type StudyPlan = {
  __typename?: 'StudyPlan';
  study?: Maybe<Scalars['String']>;
  target?: Maybe<Skill>;
  teach?: Maybe<Array<Scalars['Int']>>;
  unitNumber: Scalars['Int'];
};

export type StudyPlanResult = MutationResult & {
  __typename?: 'StudyPlanResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  studyPlan?: Maybe<StudyPlan>;
};

export type TradableItem = AnItem & {
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

export type TreasuryItem = AnItem & {
  __typename?: 'TreasuryItem';
  amount: Scalars['Int'];
  code: Scalars['String'];
  max: Scalars['Int'];
  rank: Scalars['Int'];
  total: Scalars['Int'];
};

export type Turn = Node & {
  __typename?: 'Turn';
  id: Scalars['ID'];
  number: Scalars['Int'];
  reports?: Maybe<Array<Maybe<Report>>>;
  state: TurnState;
};

export enum TurnState {
  Executed = 'EXECUTED',
  Merged = 'MERGED',
  Parsed = 'PARSED',
  Pending = 'PENDING',
  Processed = 'PROCESSED',
  Ready = 'READY'
}

export type TurnStatisticsItem = AnItem & {
  __typename?: 'TurnStatisticsItem';
  amount: Scalars['Int'];
  category: StatisticsCategory;
  code: Scalars['String'];
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
  isMage: Scalars['Boolean'];
  items: Array<Maybe<UnitItem>>;
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

export type UnitItem = AnItem & {
  __typename?: 'UnitItem';
  amount: Scalars['Int'];
  code: Scalars['String'];
  illusion: Scalars['Boolean'];
  props?: Maybe<Scalars['String']>;
  unfinished: Scalars['Boolean'];
};

export type UnitOrdersSetResult = MutationResult & {
  __typename?: 'UnitOrdersSetResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type UnitsFilterInput = {
  mages?: InputMaybe<Scalars['Boolean']>;
  own?: InputMaybe<Scalars['Boolean']>;
};

export type User = Node & {
  __typename?: 'User';
  createdAt: Scalars['DateTime'];
  emails?: Maybe<Array<Maybe<UserEmail>>>;
  id: Scalars['ID'];
  lastVisitAt: Scalars['DateTime'];
  name: Scalars['String'];
  players?: Maybe<Array<Maybe<Player>>>;
  roles?: Maybe<Array<Maybe<Scalars['String']>>>;
  updatedAt: Scalars['DateTime'];
};

export type UserCollectionSegment = {
  __typename?: 'UserCollectionSegment';
  items?: Maybe<Array<Maybe<User>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type UserEmail = {
  __typename?: 'UserEmail';
  createdAt: Scalars['DateTime'];
  deletedAt?: Maybe<Scalars['DateTime']>;
  disabled: Scalars['Boolean'];
  email: Scalars['String'];
  emailVerifiedAt?: Maybe<Scalars['DateTime']>;
  id: Scalars['Long'];
  primary: Scalars['Boolean'];
  updatedAt: Scalars['DateTime'];
  user?: Maybe<User>;
  userId: Scalars['Long'];
  verificationCode?: Maybe<Scalars['String']>;
  verificationCodeExpiresAt?: Maybe<Scalars['DateTime']>;
};

export type AttitudeFragment = { __typename?: 'Attitude', factionNumber: number, stance: Stance };

export type BattleFactionFragment = { __typename?: 'BattleFaction', number: number, name?: string | null };

export type BattleRoundFragment = { __typename?: 'BattleRound', log?: string | null, statistics?: string | null };

export type BattleSkillFragment = { __typename?: 'BattleSkill', name?: string | null, level: number };

export type BattleUnitFragment = { __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null };

export type BattleFragment = { __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null };

export type CapacityFragment = { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number };

export type CasualtiesFragment = { __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null };

export type CoordsFragment = { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null };

export type EventFragment = { __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null };

export type ExitFragment = { __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null };

export type FactionFragment = { __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null };

export type FleetContentFragment = { __typename?: 'FleetContent', type?: string | null, count: number };

export type GameDetailsFragment = { __typename?: 'Game', id: string, name: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null };

export type GameEngineFragment = { __typename?: 'GameEngine', id: string, name: string, remote: boolean, description?: string | null, createdAt: any, createdBy?: { __typename?: 'User', name: string } | null };

export type GameHeaderFragment = { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, type: GameType, name: string, createdBy?: { __typename?: 'User', name: string } | null, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null };

export type GameOptionsFragment = { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null };

export type LoadFragment = { __typename?: 'TransportationLoad', used: number, max: number };

export type LocationFragment = { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null };

export type MageFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null };

export type ParticipantFragment = { __typename?: 'Participant', number: number, name?: string | null };

export type PlayerHeaderFragment = { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null };

export type PlayerTurnStatisticsFragment = { __typename?: 'PlayerTurn', unclaimed: number, income?: { __typename?: 'Income', work: number, entertain: number, tax: number, pillage: number, trade: number, claim: number, total: number } | null, expenses?: { __typename?: 'Expenses', trade: number, study: number, consume: number, total: number } | null, statistics?: Array<{ __typename?: 'TurnStatisticsItem', category: StatisticsCategory, code: string, amount: number } | null> | null, treasury?: Array<{ __typename?: 'TreasuryItem', rank: number, max: number, total: number, code: string, amount: number } | null> | null };

export type RegionFragment = { __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, gate?: number | null, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code: string, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null };

export type SailorsFragment = { __typename?: 'Sailors', current: number, required: number };

export type SetOrderResultFragment = { __typename?: 'UnitOrdersSetResult', isSuccess: boolean, error?: string | null };

export type SettlementFragment = { __typename?: 'Settlement', name?: string | null, size: SettlementSize };

export type SkillFragment = { __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null };

export type StructureFragment = { __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null };

export type StudyPlanFragment = { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null };

export type TradableItemFragment = { __typename?: 'TradableItem', code: string, price: number, amount: number };

export type TurnFragment = { __typename?: 'PlayerTurn', id: string, turnNumber: number, unclaimed: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null, battles?: Array<{ __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null } | null> | null };

export type UnitFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'UnitItem', code: string, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null };

export type FactionClaimMutationVariables = Exact<{
  gameId: Scalars['ID'];
  playerId: Scalars['ID'];
  password: Scalars['String'];
}>;


export type FactionClaimMutation = { __typename?: 'Mutation', gameJoinRemote?: { __typename?: 'GameJoinRemoteResult', isSuccess: boolean, error?: string | null, player?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null } | null };

export type GameCreateLocalMutationVariables = Exact<{
  name: Scalars['String'];
  gameEngineId: Scalars['ID'];
  levels: Array<MapLevelInput> | MapLevelInput;
  schedule: Scalars['String'];
  timeZone: Scalars['String'];
  gameIn: Scalars['Upload'];
  playersIn: Scalars['Upload'];
}>;


export type GameCreateLocalMutation = { __typename?: 'Mutation', gameCreateLocal?: { __typename?: 'GameCreateLocalResult', isSuccess: boolean, error?: string | null, game?: { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, type: GameType, name: string, createdBy?: { __typename?: 'User', name: string } | null, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null } | null };

export type GameCreateRemoteMutationVariables = Exact<{
  name: Scalars['String'];
  gameEngineId: Scalars['ID'];
  levels: Array<MapLevelInput> | MapLevelInput;
  schedule: Scalars['String'];
  timeZone: Scalars['String'];
}>;


export type GameCreateRemoteMutation = { __typename?: 'Mutation', gameCreateRemote?: { __typename?: 'GameCreateRemoteResult', isSuccess: boolean, error?: string | null, game?: { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, type: GameType, name: string, createdBy?: { __typename?: 'User', name: string } | null, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null } | null };

export type GameDeleteMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GameDeleteMutation = { __typename?: 'Mutation', gameDelete?: { __typename?: 'GameDeleteResult', isSuccess: boolean, error?: string | null } | null };

export type GameEngineCreateLocalMutationVariables = Exact<{
  name: Scalars['String'];
  description?: InputMaybe<Scalars['String']>;
  engine: Scalars['Upload'];
  ruleset?: InputMaybe<Scalars['Upload']>;
}>;


export type GameEngineCreateLocalMutation = { __typename?: 'Mutation', gameEngineCreateLocal?: { __typename?: 'GameEngineCreateLocalResult', isSuccess: boolean, error?: string | null, engine?: { __typename?: 'GameEngine', id: string, name: string, remote: boolean, description?: string | null, createdAt: any, createdBy?: { __typename?: 'User', name: string } | null } | null } | null };

export type GameEngineCreateRemoteMutationVariables = Exact<{
  name: Scalars['String'];
  description?: InputMaybe<Scalars['String']>;
  api?: InputMaybe<Scalars['String']>;
  url?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<Scalars['String']>;
}>;


export type GameEngineCreateRemoteMutation = { __typename?: 'Mutation', gameEngineCreateRemote?: { __typename?: 'GameEngineCreateRemoteResult', isSuccess: boolean, error?: string | null, engine?: { __typename?: 'GameEngine', id: string, name: string, remote: boolean, description?: string | null, createdAt: any, createdBy?: { __typename?: 'User', name: string } | null } | null } | null };

export type GameEngineDeleteMutationVariables = Exact<{
  gameEngineId: Scalars['ID'];
}>;


export type GameEngineDeleteMutation = { __typename?: 'Mutation', gameEngineDelete?: { __typename?: 'GameEngineDeleteResult', isSuccess: boolean, error?: string | null } | null };

export type GameJoinLocalMutationVariables = Exact<{
  gameId: Scalars['ID'];
  name: Scalars['String'];
}>;


export type GameJoinLocalMutation = { __typename?: 'Mutation', gameJoinLocal?: { __typename?: 'GameJoinLocalResult', isSuccess: boolean, error?: string | null, registration?: { __typename?: 'Registration', id: any, name?: string | null } | null } | null };

export type GameStartMutationVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GameStartMutation = { __typename?: 'Mutation', gameStart?: { __typename?: 'GameStartResult', isSuccess: boolean, error?: string | null } | null };

export type GetJobQueryVariables = Exact<{
  jobId: Scalars['String'];
}>;


export type GetJobQuery = { __typename?: 'Query', job?: { __typename?: 'BackgroundJob', state: JobState } | null };

export type SetOrderMutationVariables = Exact<{
  unitId: Scalars['ID'];
  orders?: InputMaybe<Scalars['String']>;
}>;


export type SetOrderMutation = { __typename?: 'Mutation', setOrders?: { __typename?: 'UnitOrdersSetResult', isSuccess: boolean, error?: string | null } | null };

export type StudyPlanStudyMutationVariables = Exact<{
  unitId: Scalars['ID'];
  skill: Scalars['String'];
}>;


export type StudyPlanStudyMutation = { __typename?: 'Mutation', studyPlanStudy?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type StudyPlanTargetMutationVariables = Exact<{
  unitId: Scalars['ID'];
  skill: Scalars['String'];
  level: Scalars['Int'];
}>;


export type StudyPlanTargetMutation = { __typename?: 'Mutation', studyPlanTarget?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type StudyPlanTeachMutationVariables = Exact<{
  unitId: Scalars['ID'];
  units: Array<Scalars['Int']> | Scalars['Int'];
}>;


export type StudyPlanTeachMutation = { __typename?: 'Mutation', studyPlanTeach?: { __typename?: 'StudyPlanResult', isSuccess: boolean, error?: string | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null };

export type GetAllianceMagesQueryVariables = Exact<{
  gameId: Scalars['ID'];
  turn: Scalars['Int'];
}>;


export type GetAllianceMagesQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', me?: { __typename?: 'Player', alliance?: { __typename?: 'Alliance', id: string, name: string, members?: Array<{ __typename?: 'AllianceMember', number?: number | null, name?: string | null, turn?: { __typename?: 'AllianceMemberTurn', number: number, units?: { __typename?: 'UnitCollectionSegment', items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null> | null } | null } | null } | null> | null } | null } | null } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGameDetailsQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameDetailsQuery = { __typename?: 'Query', node?: { __typename: 'Alliance' } | { __typename: 'Faction' } | { __typename: 'Game', id: string, name: string, status: GameStatus, lastTurnNumber?: number | null, me?: { __typename?: 'Player', id: string } | null, players?: { __typename?: 'PlayerCollectionSegment', items?: Array<{ __typename?: 'Player', id: string, name?: string | null, number: number, isClaimed: boolean, turns?: { __typename?: 'PlayerTurnCollectionSegment', items?: Array<{ __typename?: 'PlayerTurn', turnNumber: number, isOrdersSubmitted: boolean, isTimesSubmitted: boolean } | null> | null } | null } | null> | null } | null } | { __typename: 'GameEngine' } | { __typename: 'Player' } | { __typename: 'PlayerTurn' } | { __typename: 'Region' } | { __typename: 'Structure' } | { __typename: 'Turn' } | { __typename: 'Unit' } | { __typename: 'User' } | null };

export type GetGameEnginesQueryVariables = Exact<{
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
}>;


export type GetGameEnginesQuery = { __typename?: 'Query', gameEngines?: { __typename?: 'GameEngineCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'GameEngine', id: string, name: string, remote: boolean, description?: string | null, createdAt: any, createdBy?: { __typename?: 'User', name: string } | null } | null> | null, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean, hasPreviousPage: boolean } } | null };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', id: string, name: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGamesQueryVariables = Exact<{
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
}>;


export type GetGamesQuery = { __typename?: 'Query', games?: { __typename?: 'GameCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Game', id: string, status: GameStatus, createdAt: any, type: GameType, name: string, createdBy?: { __typename?: 'User', name: string } | null, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null> | null, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean, hasPreviousPage: boolean } } | null };

export type GetMeQueryVariables = Exact<{ [key: string]: never; }>;


export type GetMeQuery = { __typename?: 'Query', me?: { __typename?: 'User', id: string, roles?: Array<string | null> | null } | null };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', id: string, regions?: { __typename?: 'RegionCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, gate?: number | null, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code: string, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null } | null> | null } | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnStatsQueryVariables = Exact<{
  playerId: Scalars['ID'];
}>;


export type GetTurnStatsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player', turns?: { __typename?: 'PlayerTurnCollectionSegment', items?: Array<{ __typename?: 'PlayerTurn', id: string, turnNumber: number, unclaimed: number, income?: { __typename?: 'Income', work: number, entertain: number, tax: number, pillage: number, trade: number, claim: number, total: number } | null, expenses?: { __typename?: 'Expenses', trade: number, study: number, consume: number, total: number } | null, statistics?: Array<{ __typename?: 'TurnStatisticsItem', category: StatisticsCategory, code: string, amount: number } | null> | null, treasury?: Array<{ __typename?: 'TreasuryItem', rank: number, max: number, total: number, code: string, amount: number } | null> | null } | null> | null } | null } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', id: string, turnNumber: number, unclaimed: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null, battles?: Array<{ __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'BattleItem', code?: string | null, amount: number } | null> | null } | null> | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', id: string, units?: { __typename?: 'UnitCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'UnitItem', code: string, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null } | null> | null } | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type OrdersGetQueryVariables = Exact<{
  turnId: Scalars['ID'];
  take: Scalars['Int'];
  skip: Scalars['Int'];
}>;


export type OrdersGetQuery = { __typename?: 'Query', node?: { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', orders?: { __typename?: 'OrdersCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Orders', unitNumber: number, orders?: string | null } | null> | null } | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

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
  lastTurn {
    id
  }
}
    `;
export const GameDetails = gql`
    fragment GameDetails on Game {
  id
  name
  options {
    ...GameOptions
  }
  me {
    ...PlayerHeader
  }
}
    ${GameOptions}
${PlayerHeader}`;
export const GameEngine = gql`
    fragment GameEngine on GameEngine {
  id
  name
  remote
  description
  createdAt
  createdBy {
    name
  }
}
    `;
export const GameHeader = gql`
    fragment GameHeader on Game {
  id
  status
  createdAt
  createdBy {
    name
  }
  type
  name
  options {
    schedule
    timeZone
    startAt
    finishAt
  }
  me {
    ...PlayerHeader
  }
  players {
    items {
      ...PlayerHeader
    }
    totalCount
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
export const PlayerTurnStatistics = gql`
    fragment PlayerTurnStatistics on PlayerTurn {
  unclaimed
  income {
    work
    entertain
    tax
    pillage
    trade
    claim
    total
  }
  expenses {
    trade
    study
    consume
    total
  }
  statistics {
    category
    code
    amount
  }
  treasury {
    rank
    max
    total
    code
    amount
  }
}
    `;
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
  gate
  wanted {
    ...TradableItem
  }
  entertainment
  produces {
    code
    amount
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
${Exit}
${Structure}`;
export const SetOrderResult = gql`
    fragment SetOrderResult on UnitOrdersSetResult {
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
export const Coords = gql`
    fragment Coords on Coords {
  x
  y
  z
  label
}
    `;
export const Location = gql`
    fragment Location on Location {
  terrain
  province
  coords {
    ...Coords
  }
}
    ${Coords}`;
export const Participant = gql`
    fragment Participant on Participant {
  number
  name
}
    `;
export const BattleFaction = gql`
    fragment BattleFaction on BattleFaction {
  number
  name
}
    `;
export const BattleSkill = gql`
    fragment BattleSkill on BattleSkill {
  name
  level
}
    `;
export const BattleUnit = gql`
    fragment BattleUnit on BattleUnit {
  number
  name
  faction {
    ...BattleFaction
  }
  description
  flags
  items {
    code
    amount
  }
  skills {
    ...BattleSkill
  }
}
    ${BattleFaction}
${BattleSkill}`;
export const BattleRound = gql`
    fragment BattleRound on BattleRound {
  log
  statistics
}
    `;
export const Casualties = gql`
    fragment Casualties on Casualties {
  army {
    ...Participant
  }
  lost
  damagedUnits
}
    ${Participant}`;
export const Battle = gql`
    fragment Battle on Battle {
  location {
    ...Location
  }
  attacker {
    ...Participant
  }
  defender {
    ...Participant
  }
  attackers {
    ...BattleUnit
  }
  defenders {
    ...BattleUnit
  }
  rounds {
    ...BattleRound
  }
  casualties {
    ...Casualties
  }
  spoils {
    code
    amount
  }
  rose {
    code
    amount
  }
  statistics
}
    ${Location}
${Participant}
${BattleUnit}
${BattleRound}
${Casualties}`;
export const Turn = gql`
    fragment Turn on PlayerTurn {
  id
  turnNumber
  unclaimed
  factions {
    ...Faction
  }
  battles {
    ...Battle
  }
}
    ${Faction}
${Battle}`;
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
    code
    amount
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
${Skill}`;
export const FactionClaim = gql`
    mutation FactionClaim($gameId: ID!, $playerId: ID!, $password: String!) {
  gameJoinRemote(gameId: $gameId, playerId: $playerId, password: $password) {
    isSuccess
    error
    player {
      ...PlayerHeader
    }
  }
}
    ${PlayerHeader}`;
export const GameCreateLocal = gql`
    mutation GameCreateLocal($name: String!, $gameEngineId: ID!, $levels: [MapLevelInput!]!, $schedule: String!, $timeZone: String!, $gameIn: Upload!, $playersIn: Upload!) {
  gameCreateLocal(
    name: $name
    gameEngineId: $gameEngineId
    levels: $levels
    schedule: $schedule
    timeZone: $timeZone
    gameIn: $gameIn
    playersIn: $playersIn
  ) {
    isSuccess
    error
    game {
      ...GameHeader
    }
  }
}
    ${GameHeader}`;
export const GameCreateRemote = gql`
    mutation GameCreateRemote($name: String!, $gameEngineId: ID!, $levels: [MapLevelInput!]!, $schedule: String!, $timeZone: String!) {
  gameCreateRemote(
    name: $name
    gameEngineId: $gameEngineId
    levels: $levels
    schedule: $schedule
    timeZone: $timeZone
  ) {
    isSuccess
    error
    game {
      ...GameHeader
    }
  }
}
    ${GameHeader}`;
export const GameDelete = gql`
    mutation GameDelete($gameId: ID!) {
  gameDelete(gameId: $gameId) {
    isSuccess
    error
  }
}
    `;
export const GameEngineCreateLocal = gql`
    mutation GameEngineCreateLocal($name: String!, $description: String, $engine: Upload!, $ruleset: Upload) {
  gameEngineCreateLocal(
    name: $name
    description: $description
    engine: $engine
    ruleset: $ruleset
  ) {
    isSuccess
    error
    engine {
      ...GameEngine
    }
  }
}
    ${GameEngine}`;
export const GameEngineCreateRemote = gql`
    mutation GameEngineCreateRemote($name: String!, $description: String, $api: String, $url: String, $options: String) {
  gameEngineCreateRemote(
    name: $name
    description: $description
    api: $api
    url: $url
    options: $options
  ) {
    isSuccess
    error
    engine {
      ...GameEngine
    }
  }
}
    ${GameEngine}`;
export const GameEngineDelete = gql`
    mutation GameEngineDelete($gameEngineId: ID!) {
  gameEngineDelete(gameEngineId: $gameEngineId) {
    isSuccess
    error
  }
}
    `;
export const GameJoinLocal = gql`
    mutation GameJoinLocal($gameId: ID!, $name: String!) {
  gameJoinLocal(gameId: $gameId, name: $name) {
    isSuccess
    error
    registration {
      id
      name
    }
  }
}
    `;
export const GameStart = gql`
    mutation GameStart($gameId: ID!) {
  gameStart(gameId: $gameId) {
    isSuccess
    error
  }
}
    `;
export const GetJob = gql`
    query GetJob($jobId: String!) {
  job(jobId: $jobId) {
    state
  }
}
    `;
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
export const GetGameDetails = gql`
    query GetGameDetails($gameId: ID!) {
  node(id: $gameId) {
    __typename
    ... on Game {
      id
      name
      status
      lastTurnNumber
      me {
        id
      }
      players(quit: false) {
        items {
          id
          name
          number
          isClaimed
          turns(take: 10) {
            items {
              turnNumber
              isOrdersSubmitted
              isTimesSubmitted
            }
          }
        }
      }
    }
  }
}
    `;
export const GetGameEngines = gql`
    query GetGameEngines($skip: Int = 0, $take: Int = 100) {
  gameEngines(skip: $skip, take: $take) {
    items {
      ...GameEngine
    }
    totalCount
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
    ${GameEngine}`;
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
    query GetGames($skip: Int = 0, $take: Int = 100) {
  games(skip: $skip, take: $take) {
    items {
      ...GameHeader
    }
    totalCount
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
    ${GameHeader}`;
export const GetMe = gql`
    query GetMe {
  me {
    id
    roles
  }
}
    `;
export const GetRegions = gql`
    query GetRegions($turnId: ID!, $skip: Int! = 0, $pageSize: Int! = 1000) {
  node(id: $turnId) {
    ... on PlayerTurn {
      id
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
      turns(take: 10, skip: 1) {
        items {
          id
          turnNumber
          ...PlayerTurnStatistics
        }
      }
    }
  }
}
    ${PlayerTurnStatistics}`;
export const GetTurn = gql`
    query GetTurn($turnId: ID!) {
  node(id: $turnId) {
    ... on PlayerTurn {
      ...Turn
    }
  }
}
    ${Turn}`;
export const GetUnits = gql`
    query GetUnits($turnId: ID!, $skip: Int! = 0, $pageSize: Int! = 1000) {
  node(id: $turnId) {
    ... on PlayerTurn {
      id
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
export const OrdersGet = gql`
    query OrdersGet($turnId: ID!, $take: Int!, $skip: Int!) {
  node(id: $turnId) {
    ... on PlayerTurn {
      orders(take: $take, skip: $skip) {
        totalCount
        pageInfo {
          hasNextPage
        }
        items {
          unitNumber
          orders
        }
      }
    }
  }
}
    `;