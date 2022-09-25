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
  DateTime: any;
  Long: any;
  Upload: any;
};

export type AditionalReport = Node & {
  __typename?: 'AditionalReport';
  factionName: Scalars['String'];
  factionNumber: Scalars['Int'];
  id: Scalars['ID'];
  json?: Maybe<Scalars['String']>;
  source: Scalars['String'];
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
  rose?: Maybe<Array<Maybe<Item>>>;
  rounds?: Maybe<Array<Maybe<BattleRound>>>;
  spoils?: Maybe<Array<Maybe<Item>>>;
  statistics?: Maybe<Scalars['String']>;
};

export type BattleFaction = {
  __typename?: 'BattleFaction';
  name?: Maybe<Scalars['String']>;
  number: Scalars['Int'];
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
  items?: Maybe<Array<Maybe<Item>>>;
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
  createdAt: Scalars['DateTime'];
  id: Scalars['ID'];
  lastTurnNumber?: Maybe<Scalars['Int']>;
  me?: Maybe<Player>;
  name: Scalars['String'];
  nextTurnNumber?: Maybe<Scalars['Int']>;
  options: GameOptions;
  players?: Maybe<PlayerCollectionSegment>;
  ruleset: Scalars['String'];
  status: GameStatus;
  turns?: Maybe<Array<Maybe<Turn>>>;
  type: GameType;
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

export type GameCompleteResult = {
  __typename?: 'GameCompleteResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameCreateLocalResult = {
  __typename?: 'GameCreateLocalResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameCreateRemoteResult = {
  __typename?: 'GameCreateRemoteResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameEngine = Node & {
  __typename?: 'GameEngine';
  createdAt: Scalars['DateTime'];
  id: Scalars['ID'];
  name: Scalars['String'];
};

export type GameEngineCollectionSegment = {
  __typename?: 'GameEngineCollectionSegment';
  items?: Maybe<Array<Maybe<GameEngine>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type GameEngineCreateResult = {
  __typename?: 'GameEngineCreateResult';
  engine?: Maybe<GameEngine>;
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
};

export type GameJoinLocalResult = {
  __typename?: 'GameJoinLocalResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  registration?: Maybe<Registration>;
};

export type GameJoinRemoteResult = {
  __typename?: 'GameJoinRemoteResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  player?: Maybe<Player>;
};

export type GameNextTurnResult = {
  __typename?: 'GameNextTurnResult';
  error?: Maybe<Scalars['String']>;
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

export type GameOptionsSetResult = {
  __typename?: 'GameOptionsSetResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GamePauseResult = {
  __typename?: 'GamePauseResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameScheduleSetResult = {
  __typename?: 'GameScheduleSetResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
};

export type GameStartResult = {
  __typename?: 'GameStartResult';
  error?: Maybe<Scalars['String']>;
  game?: Maybe<Game>;
  isSuccess: Scalars['Boolean'];
  jobId?: Maybe<Scalars['String']>;
};

export enum GameStatus {
  Compleated = 'COMPLEATED',
  Locked = 'LOCKED',
  New = 'NEW',
  Paused = 'PAUSED',
  Running = 'RUNNING'
}

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

export type Item = {
  __typename?: 'Item';
  amount: Scalars['Int'];
  code?: Maybe<Scalars['String']>;
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
  createUser?: Maybe<User>;
  deleteTurn: Scalars['Int'];
  gameComplete?: Maybe<GameCompleteResult>;
  gameCreateLocal?: Maybe<GameCreateLocalResult>;
  gameCreateRemote?: Maybe<GameCreateRemoteResult>;
  gameEngineCreate?: Maybe<GameEngineCreateResult>;
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
  updateUserRoles?: Maybe<User>;
};


export type MutationAllianceCreateArgs = {
  name?: InputMaybe<Scalars['String']>;
  playerId: Scalars['ID'];
};


export type MutationAllianceJoinArgs = {
  allianceId: Scalars['ID'];
  playerId: Scalars['ID'];
};


export type MutationCreateUserArgs = {
  email?: InputMaybe<Scalars['String']>;
  password?: InputMaybe<Scalars['String']>;
};


export type MutationDeleteTurnArgs = {
  turnId?: InputMaybe<Scalars['ID']>;
};


export type MutationGameCompleteArgs = {
  gameId: Scalars['ID'];
};


export type MutationGameCreateLocalArgs = {
  gameData?: InputMaybe<Scalars['Upload']>;
  gameEngineId: Scalars['ID'];
  name?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<GameOptionsInput>;
  playerData?: InputMaybe<Scalars['Upload']>;
};


export type MutationGameCreateRemoteArgs = {
  name?: InputMaybe<Scalars['String']>;
  options?: InputMaybe<GameOptionsInput>;
};


export type MutationGameEngineCreateArgs = {
  file?: InputMaybe<Scalars['Upload']>;
  name?: InputMaybe<Scalars['String']>;
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
  gameId: Scalars['ID'];
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


export type MutationUpdateUserRolesArgs = {
  add?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  remove?: InputMaybe<Array<InputMaybe<Scalars['String']>>>;
  userId: Scalars['ID'];
};

/** The node interface is implemented by entities that have a global unique identifier. */
export type Node = {
  id: Scalars['ID'];
};

export type Participant = {
  __typename?: 'Participant';
  name?: Maybe<Scalars['String']>;
  number: Scalars['Int'];
};

export type Player = Node & {
  __typename?: 'Player';
  alliance?: Maybe<Alliance>;
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
  reports?: Maybe<Array<Maybe<AditionalReport>>>;
  turn?: Maybe<PlayerTurn>;
  turns?: Maybe<Array<Maybe<PlayerTurn>>>;
};


export type PlayerReportsArgs = {
  turn?: InputMaybe<Scalars['Int']>;
};


export type PlayerTurnArgs = {
  number: Scalars['Int'];
};

export type PlayerCollectionSegment = {
  __typename?: 'PlayerCollectionSegment';
  items?: Maybe<Array<Maybe<Player>>>;
  /** Information to aid in pagination. */
  pageInfo: CollectionSegmentInfo;
  totalCount: Scalars['Int'];
};

export type PlayerQuitResult = {
  __typename?: 'PlayerQuitResult';
  error?: Maybe<Scalars['String']>;
  isSuccess: Scalars['Boolean'];
  player?: Maybe<Player>;
};

export type PlayerTurn = Node & {
  __typename?: 'PlayerTurn';
  battles?: Maybe<Array<Maybe<Battle>>>;
  events?: Maybe<EventCollectionSegment>;
  factions?: Maybe<Array<Maybe<Faction>>>;
  id: Scalars['ID'];
  isOrdersSubmitted: Scalars['Boolean'];
  isProcessed: Scalars['Boolean'];
  isReady: Scalars['Boolean'];
  isTimesSubmitted: Scalars['Boolean'];
  name: Scalars['String'];
  ordersSubmittedAt?: Maybe<Scalars['DateTime']>;
  readyAt?: Maybe<Scalars['DateTime']>;
  regions?: Maybe<RegionCollectionSegment>;
  reports?: Maybe<Array<Maybe<AditionalReport>>>;
  stats?: Maybe<Statistics>;
  structures?: Maybe<StructureCollectionSegment>;
  studyPlans?: Maybe<Array<Maybe<StudyPlan>>>;
  timesSubmittedAt?: Maybe<Scalars['DateTime']>;
  turnNumber: Scalars['Int'];
  units?: Maybe<UnitCollectionSegment>;
};


export type PlayerTurnEventsArgs = {
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
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
  explored: Scalars['Boolean'];
  forSale?: Maybe<Array<Maybe<TradableItem>>>;
  gate?: Maybe<Scalars['Int']>;
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

export type Registration = {
  __typename?: 'Registration';
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
  income?: Maybe<Income>;
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
  id: Scalars['ID'];
  number: Scalars['Int'];
  reports?: Maybe<Array<Maybe<Report>>>;
  state: TurnState;
};

export enum TurnState {
  Pending = 'PENDING',
  Ready = 'READY'
}

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

export type UnitOrdersSetResult = {
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

export type BattleFactionFragment = { __typename?: 'BattleFaction', number: number, name?: string | null };

export type BattleRoundFragment = { __typename?: 'BattleRound', log?: string | null, statistics?: string | null };

export type BattleSkillFragment = { __typename?: 'BattleSkill', name?: string | null, level: number };

export type BattleUnitFragment = { __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null };

export type BattleFragment = { __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null };

export type CapacityFragment = { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number };

export type CasualtiesFragment = { __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null };

export type CoordsFragment = { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null };

export type EventFragment = { __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null };

export type ExitFragment = { __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null };

export type FactionFragment = { __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null };

export type FleetContentFragment = { __typename?: 'FleetContent', type?: string | null, count: number };

export type GameDetailsFragment = { __typename?: 'Game', id: string, name: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null };

export type GameEngineFragment = { __typename?: 'GameEngine', id: string, name: string, createdAt: any };

export type GameHeaderFragment = { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, name: string, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null };

export type GameOptionsFragment = { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null };

export type ItemFragment = { __typename?: 'Item', code?: string | null, amount: number };

export type LoadFragment = { __typename?: 'TransportationLoad', used: number, max: number };

export type LocationFragment = { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null };

export type MageFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null };

export type ParticipantFragment = { __typename?: 'Participant', number: number, name?: string | null };

export type PlayerHeaderFragment = { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null };

export type RegionFragment = { __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, gate?: number | null, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null };

export type SailorsFragment = { __typename?: 'Sailors', current: number, required: number };

export type SetOrderResultFragment = { __typename?: 'UnitOrdersSetResult', isSuccess: boolean, error?: string | null };

export type SettlementFragment = { __typename?: 'Settlement', name?: string | null, size: SettlementSize };

export type SkillFragment = { __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null };

export type StaisticsFragment = { __typename?: 'Statistics', income?: { __typename?: 'Income', work: number, entertain: number, tax: number, pillage: number, trade: number, claim: number, total: number } | null, production?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null };

export type StructureFragment = { __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null };

export type StudyPlanFragment = { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null };

export type TradableItemFragment = { __typename?: 'TradableItem', code: string, price: number, amount: number };

export type TurnFragment = { __typename?: 'PlayerTurn', id: string, turnNumber: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null, battles?: Array<{ __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null } | null> | null };

export type UnitFragment = { __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null };

export type GameCreateLocalMutationVariables = Exact<{
  name: Scalars['String'];
  gameEngineId: Scalars['ID'];
  options: GameOptionsInput;
  playerData: Scalars['Upload'];
  gameData: Scalars['Upload'];
}>;


export type GameCreateLocalMutation = { __typename?: 'Mutation', gameCreateLocal?: { __typename?: 'GameCreateLocalResult', isSuccess: boolean, error?: string | null, game?: { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, name: string, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null } | null };

export type GameCreateRemoteMutationVariables = Exact<{
  name: Scalars['String'];
  options: GameOptionsInput;
}>;


export type GameCreateRemoteMutation = { __typename?: 'Mutation', gameCreateRemote?: { __typename?: 'GameCreateRemoteResult', isSuccess: boolean, error?: string | null, game?: { __typename?: 'Game', id: string, status: GameStatus, createdAt: any, name: string, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null } | null };

export type GameEngineCreateMutationVariables = Exact<{
  name: Scalars['String'];
  file: Scalars['Upload'];
}>;


export type GameEngineCreateMutation = { __typename?: 'Mutation', gameEngineCreate?: { __typename?: 'GameEngineCreateResult', isSuccess: boolean, error?: string | null, engine?: { __typename?: 'GameEngine', id: string, name: string, createdAt: any } | null } | null };

export type GameJoinLocalMutationVariables = Exact<{
  gameId: Scalars['ID'];
  name: Scalars['String'];
}>;


export type GameJoinLocalMutation = { __typename?: 'Mutation', gameJoinLocal?: { __typename?: 'GameJoinLocalResult', isSuccess: boolean, error?: string | null, registration?: { __typename?: 'Registration', id: any, name?: string | null } | null } | null };

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


export type GetAllianceMagesQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', me?: { __typename?: 'Player', alliance?: { __typename?: 'Alliance', id: string, name: string, members?: Array<{ __typename?: 'AllianceMember', number?: number | null, name?: string | null, turn?: { __typename?: 'AllianceMemberTurn', number: number, units?: { __typename?: 'UnitCollectionSegment', items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, number: number, name: string, factionNumber?: number | null, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null, studyPlan?: { __typename?: 'StudyPlan', study?: string | null, teach?: Array<number> | null, target?: { __typename?: 'Skill', code?: string | null, level?: number | null } | null } | null } | null> | null } | null } | null } | null> | null } | null } | null } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGameDetailsQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameDetailsQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', id: string, name: string, players?: { __typename?: 'PlayerCollectionSegment', items?: Array<{ __typename?: 'Player', id: string, name?: string | null, number: number, isClaimed: boolean, nextTurn?: { __typename?: 'PlayerTurn', isReady: boolean, isOrdersSubmitted: boolean, isTimesSubmitted: boolean } | null } | null> | null } | null } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGameEnginesQueryVariables = Exact<{
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
}>;


export type GetGameEnginesQuery = { __typename?: 'Query', gameEngines?: { __typename?: 'GameEngineCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'GameEngine', id: string, name: string, createdAt: any } | null> | null, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean, hasPreviousPage: boolean } } | null };

export type GetGameQueryVariables = Exact<{
  gameId: Scalars['ID'];
}>;


export type GetGameQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game', id: string, name: string, ruleset: string, options: { __typename?: 'GameOptions', map?: Array<{ __typename?: 'MapLevel', label?: string | null, level: number, width: number, height: number } | null> | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetGamesQueryVariables = Exact<{
  skip?: InputMaybe<Scalars['Int']>;
  take?: InputMaybe<Scalars['Int']>;
}>;


export type GetGamesQuery = { __typename?: 'Query', games?: { __typename?: 'GameCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Game', id: string, status: GameStatus, createdAt: any, name: string, options: { __typename?: 'GameOptions', schedule?: string | null, timeZone?: string | null, startAt?: any | null, finishAt?: any | null }, me?: { __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null, players?: { __typename?: 'PlayerCollectionSegment', totalCount: number, items?: Array<{ __typename?: 'Player', id: string, number: number, name?: string | null, lastTurnNumber?: number | null, lastTurn?: { __typename?: 'PlayerTurn', id: string } | null } | null> | null } | null } | null> | null, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean, hasPreviousPage: boolean } } | null };

export type GetMeQueryVariables = Exact<{ [key: string]: never; }>;


export type GetMeQuery = { __typename?: 'Query', me?: { __typename?: 'User', id: string, email: string, roles?: Array<string | null> | null } | null };

export type GetRegionsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetRegionsQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', regions?: { __typename?: 'RegionCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Region', id: string, lastVisitedAt?: number | null, explored: boolean, x: number, y: number, z: number, label: string, terrain: string, province: string, race?: string | null, population: number, tax: number, wages: number, totalWages: number, gate?: number | null, entertainment: number, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null, wanted?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, produces?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, forSale?: Array<{ __typename?: 'TradableItem', code: string, price: number, amount: number } | null> | null, exits?: Array<{ __typename?: 'Exit', direction: Direction, x: number, y: number, z: number, label: string, terrain: string, province: string, settlement?: { __typename?: 'Settlement', name?: string | null, size: SettlementSize } | null } | null> | null, structures?: Array<{ __typename?: 'Structure', id: string, x: number, y: number, z: number, sequence: number, description?: string | null, flags?: Array<string | null> | null, name: string, needs?: number | null, number: number, sailDirections?: Array<Direction> | null, speed?: number | null, type: string, contents?: Array<{ __typename?: 'FleetContent', type?: string | null, count: number } | null> | null, load?: { __typename?: 'TransportationLoad', used: number, max: number } | null, sailors?: { __typename?: 'Sailors', current: number, required: number } | null } | null> | null } | null> | null } | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnStatsQueryVariables = Exact<{
  playerId: Scalars['ID'];
}>;


export type GetTurnStatsQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player', turns?: Array<{ __typename?: 'PlayerTurn', turnNumber: number, stats?: { __typename?: 'Statistics', income?: { __typename?: 'Income', work: number, entertain: number, tax: number, pillage: number, trade: number, claim: number, total: number } | null, production?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null } | null } | null> | null } | { __typename?: 'PlayerTurn' } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetTurnQueryVariables = Exact<{
  turnId: Scalars['ID'];
}>;


export type GetTurnQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', id: string, turnNumber: number, factions?: Array<{ __typename?: 'Faction', id: string, name: string, number: number, defaultAttitude?: Stance | null, attitudes?: Array<{ __typename?: 'Attitude', factionNumber: number, stance: Stance } | null> | null, events?: Array<{ __typename?: 'Event', type: EventType, category: EventCategory, message: string, regionCode?: string | null, unitNumber?: number | null, unitName?: string | null, itemCode?: string | null, itemName?: string | null, itemPrice?: number | null, amount?: number | null } | null> | null } | null> | null, battles?: Array<{ __typename?: 'Battle', statistics?: string | null, location?: { __typename?: 'Location', terrain?: string | null, province?: string | null, coords?: { __typename?: 'Coords', x: number, y: number, z: number, label?: string | null } | null } | null, attacker?: { __typename?: 'Participant', number: number, name?: string | null } | null, defender?: { __typename?: 'Participant', number: number, name?: string | null } | null, attackers?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, defenders?: Array<{ __typename?: 'BattleUnit', number: number, name?: string | null, description?: string | null, flags?: Array<string | null> | null, faction?: { __typename?: 'BattleFaction', number: number, name?: string | null } | null, items?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, skills?: Array<{ __typename?: 'BattleSkill', name?: string | null, level: number } | null> | null } | null> | null, rounds?: Array<{ __typename?: 'BattleRound', log?: string | null, statistics?: string | null } | null> | null, casualties?: Array<{ __typename?: 'Casualties', lost: number, damagedUnits?: Array<number> | null, army?: { __typename?: 'Participant', number: number, name?: string | null } | null } | null> | null, spoils?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null, rose?: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null> | null } | null> | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

export type GetUnitsQueryVariables = Exact<{
  turnId: Scalars['ID'];
  skip?: Scalars['Int'];
  pageSize?: Scalars['Int'];
}>;


export type GetUnitsQuery = { __typename?: 'Query', node?: { __typename?: 'AditionalReport' } | { __typename?: 'Alliance' } | { __typename?: 'Faction' } | { __typename?: 'Game' } | { __typename?: 'GameEngine' } | { __typename?: 'Player' } | { __typename?: 'PlayerTurn', units?: { __typename?: 'UnitCollectionSegment', totalCount: number, pageInfo: { __typename?: 'CollectionSegmentInfo', hasNextPage: boolean }, items?: Array<{ __typename?: 'Unit', id: string, x: number, y: number, z: number, structureNumber?: number | null, sequence: number, canStudy?: Array<string | null> | null, combatSpell?: string | null, description?: string | null, factionNumber?: number | null, flags?: Array<string | null> | null, name: string, number: number, onGuard: boolean, readyItem?: string | null, weight?: number | null, orders?: string | null, capacity?: { __typename?: 'Capacity', walking: number, riding: number, flying: number, swimming: number } | null, items: Array<{ __typename?: 'Item', code?: string | null, amount: number } | null>, skills?: Array<{ __typename?: 'Skill', code?: string | null, level?: number | null, days?: number | null } | null> | null } | null> | null } | null } | { __typename?: 'Region' } | { __typename?: 'Structure' } | { __typename?: 'Turn' } | { __typename?: 'Unit' } | { __typename?: 'User' } | null };

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
  ruleset
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
  createdAt
}
    `;
export const GameHeader = gql`
    fragment GameHeader on Game {
  id
  status
  createdAt
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
  gate
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
    fragment SetOrderResult on UnitOrdersSetResult {
  isSuccess
  error
}
    `;
export const Staistics = gql`
    fragment Staistics on Statistics {
  income {
    work
    entertain
    tax
    pillage
    trade
    claim
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
    ...Item
  }
  skills {
    ...BattleSkill
  }
}
    ${BattleFaction}
${Item}
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
    ...Item
  }
  rose {
    ...Item
  }
  statistics
}
    ${Location}
${Participant}
${BattleUnit}
${BattleRound}
${Casualties}
${Item}`;
export const Turn = gql`
    fragment Turn on PlayerTurn {
  id
  turnNumber
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
export const GameCreateLocal = gql`
    mutation GameCreateLocal($name: String!, $gameEngineId: ID!, $options: GameOptionsInput!, $playerData: Upload!, $gameData: Upload!) {
  gameCreateLocal(
    name: $name
    options: $options
    gameEngineId: $gameEngineId
    playerData: $playerData
    gameData: $gameData
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
    mutation GameCreateRemote($name: String!, $options: GameOptionsInput!) {
  gameCreateRemote(name: $name, options: $options) {
    isSuccess
    error
    game {
      ...GameHeader
    }
  }
}
    ${GameHeader}`;
export const GameEngineCreate = gql`
    mutation GameEngineCreate($name: String!, $file: Upload!) {
  gameEngineCreate(name: $name, file: $file) {
    isSuccess
    error
    engine {
      ...GameEngine
    }
  }
}
    ${GameEngine}`;
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
    ... on Game {
      id
      name
      players(quit: false) {
        items {
          id
          name
          number
          isClaimed
          nextTurn {
            isReady
            isOrdersSubmitted
            isTimesSubmitted
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
    email
    roles
  }
}
    `;
export const GetRegions = gql`
    query GetRegions($turnId: ID!, $skip: Int! = 0, $pageSize: Int! = 1000) {
  node(id: $turnId) {
    ... on PlayerTurn {
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
        turnNumber
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