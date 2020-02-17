module Snapshot

open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open System.Collections.Generic
open MongoDB.Driver

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Tech = {
    value: decimal
    level: int
}

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Player = {
    Uid: int
    Huid: int
    Alias: string
    TotalEconomy: int
    TotalIndustry: int
    TotalScience: int
    TotalStars: int
    TotalFleets: int
    TotalStrength: int
    Regard: int
    Ai: int
    Tech: Dictionary<string, Tech>
    Avatar: int
    Conceded: int
    Ready: int
    MissedTurns: int
    KarmaToGive: int
}

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Snapshot = {
    Id: ObjectId
    FleetSpeed: decimal
    Paused: bool
    Productions: int
    TickFragment: decimal
    Now: int64
    ProductionRate: int
    StarsForVictory: int
    GameOver: int
    Started: bool
    StartTime: int64
    TotalStars: int
    ProductionCounter: int
    TradeScanned: int
    Tick: int
    TradeCost: int
    Name: string
    Admin: int
    TurnBased: int
    War: int
    Players: Dictionary<string, Player>
}

let mongoClient = MongoClient("mongodb://root:supersecure@localhost:27017")
let mongoDb = mongoClient.GetDatabase("galatea")
let collection = mongoDb.GetCollection<Snapshot>("snapshots")
