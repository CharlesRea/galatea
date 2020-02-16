module Api

open System
open System.Collections.Generic
open MongoDB.Driver
open Shared

let getResearchEvents (tech: Dictionary<string, Snapshot.Tech>) (prevTech: Dictionary<string, Snapshot.Tech>) =
    tech
    |> Seq.map (fun pair -> (pair.Key, pair.Value))
    |> Seq.map (fun (name, tech) -> (name, tech.level, prevTech.Item(name).level))
    |> Seq.filter (fun (_, level, prevLevel) -> not (level = prevLevel))
    |> Seq.map (fun (name, level, prevLevel) -> Research { Tech = name; Level = level; })

let getCounterEvent (getValue: Snapshot.Player -> int) counter player prevPlayer  =
    match getValue player = getValue prevPlayer with
    | true -> None
    | false -> Some (Counter { Counter = counter; OldValue = getValue prevPlayer; NewValue = getValue player })

let getCounterEvents player prevPlayer: NewsfeedEvent seq =
    [
        getCounterEvent (fun player -> player.TotalEconomy) Economy;
        getCounterEvent (fun player -> player.TotalIndustry) Industry;
        getCounterEvent (fun player -> player.TotalScience) Science;
        getCounterEvent (fun player -> player.TotalStars) Stars;
        getCounterEvent (fun player -> player.TotalStrength) Ships;
    ] |> Seq.map (fun getCounter -> getCounter player prevPlayer) |> Seq.choose id

let getEvents (player: Snapshot.Player) (prevPlayer: Snapshot.Player): NewsfeedEvent list =
    [
        yield! getCounterEvents player prevPlayer;
        yield! getResearchEvents player.Tech prevPlayer.Tech
    ]

let getNewsfeedTick (snapshot: Snapshot.Data, prevSnapshot: Snapshot.Data): NewsfeedTick =
    let players = snapshot.Players.Values
                  |> Seq.zip prevSnapshot.Players.Values
                  |> Seq.map (fun (player, prevPlayer) -> { Name = player.Alias; Events = getEvents player prevPlayer })
                  |> Seq.filter (fun (player) -> not (Seq.isEmpty player.Events))
                  |> Seq.toList
    { Tick = snapshot.Tick; Players = players; Time = DateTimeOffset.FromUnixTimeMilliseconds(snapshot.Now) }

let getNewsfeed (): Newsfeed =
    let snapshots = Snapshot.collection.Find(fun _ -> true).ToList()
    let ticks = snapshots
                |> Seq.distinctBy (fun snapshot -> snapshot.Tick)
                |> Seq.sortBy (fun snapshot -> snapshot.Tick)
                |> Seq.pairwise
                |> (Seq.map getNewsfeedTick)
                |> Seq.toList
    { Ticks = ticks }

let galateaApi: IGalateaApi = {
    newsfeed = fun () -> async { return getNewsfeed () }
}
