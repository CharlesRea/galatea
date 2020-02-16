module Api

open System
open HttpFs.Logging
open System.Collections.Generic
open MongoDB.Driver

let getSnapshots (): Snapshot.Data seq =
    Snapshot.collection.Find(fun _ -> true).ToList() |> seq

type NewsfeedPlayerEntry = {
    name: string
    events: string list
}

type NewsfeedTick = {
    players: NewsfeedPlayerEntry list
    tick: int
    time: DateTimeOffset
}

type Newsfeed = {
    ticks: NewsfeedTick list
}

let getTechEvents (tech: Dictionary<string, Snapshot.Tech>) (prevTech: Dictionary<string, Snapshot.Tech>) =
    tech
    |> Seq.map (fun pair -> (pair.Key, pair.Value))
    |> Seq.map (fun (name, tech) -> (name, tech.level, prevTech.Item(name).level))
    |> Seq.filter (fun (_, level, prevLevel) -> not (level = prevLevel))
    |> Seq.map (fun (name, level, prevLevel) -> sprintf "%s increased from level %d to %d" name prevLevel level)


let getEvents (player: Snapshot.Player) (prevPlayer: Snapshot.Player): string list =
    [
        if not (player.TotalEconomy = prevPlayer.TotalEconomy) then yield sprintf "Economy from %d to %d" prevPlayer.TotalEconomy player.TotalEconomy
        if not (player.TotalIndustry = prevPlayer.TotalIndustry) then yield sprintf "Industry from %d to %d" prevPlayer.TotalIndustry player.TotalIndustry
        if not (player.TotalScience = prevPlayer.TotalScience) then yield sprintf "Science from %d to %d" prevPlayer.TotalScience player.TotalScience
        if not (player.TotalStars = prevPlayer.TotalStars) then yield sprintf "Stars from %d to %d" prevPlayer.TotalStars player.TotalStars
        if not (player.TotalStrength = prevPlayer.TotalStrength) then yield sprintf "Ships from %d to %d" prevPlayer.TotalStrength player.TotalStrength
        yield! getTechEvents player.Tech prevPlayer.Tech
    ]

let getNewsfeedTick (snapshot: Snapshot.Data, prevSnapshot: Snapshot.Data): NewsfeedTick =
    let players = snapshot.Players.Values
                  |> Seq.zip prevSnapshot.Players.Values
                  |> Seq.map (fun (player, prevPlayer) -> { name = player.Alias; events = getEvents player prevPlayer })
                  |> Seq.filter (fun (player) -> not (Seq.isEmpty player.events))
                  |> Seq.toList
    { tick = snapshot.Tick; players = players; time = DateTimeOffset.FromUnixTimeMilliseconds(snapshot.Now) }

let getNewsfeed (): Newsfeed =
    let snapshots = getSnapshots ()
    let ticks = snapshots |> Seq.distinctBy (fun snapshot -> snapshot.Tick) |> Seq.pairwise |> (Seq.map getNewsfeedTick) |> Seq.toList
    { ticks = ticks }

type IApi =
    { snapshots: unit -> Async<Snapshot.Data seq>
      newsfeed: unit -> Async<Newsfeed> }

let api: IApi = {
    snapshots = fun () -> async { return getSnapshots () }
    newsfeed = fun () -> async { return getNewsfeed () }
}
