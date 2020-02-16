module Newsfeed

open System
open Shared
open Types
open Elmish
open Fable.React
open Api
open Client
open Fable.React.Props

type Filter =
    | All
    | ExcludeShips

type Model = { Newsfeed: Remote<Newsfeed>; Filter: Filter }

type Msg = NewsfeedRequest of RequestMsg<unit, Newsfeed>

let init() =
    { Newsfeed = Remote.Empty; Filter = ExcludeShips }, Cmd.ofMsg (NewsfeedRequest(Initiate()))

let filterEmptyTicks newsfeed =
    let ticks = newsfeed.Ticks |> List.filter (fun tick -> tick.Players |> List.isEmpty |> not)
    { newsfeed with Ticks = ticks }

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | NewsfeedRequest(Initiate _) ->
        { model with Newsfeed = Loading }, (Cmd.ofRequest api.newsfeed () NewsfeedRequest)
    | NewsfeedRequest(Fetched response) -> { model with Newsfeed = Body (filterEmptyTicks response) }, Cmd.none
    | NewsfeedRequest(FetchError error) -> { model with Newsfeed = Error error }, Cmd.none

let eventDisplay (event: NewsfeedEvent) =
    match event with
    | Research research ->
        p [] [ str (sprintf "Researched %s level %d!" research.Tech research.Level) ]
    | Counter counter  ->
        let hasIncreased = counter.OldValue < counter.NewValue
        match hasIncreased with
        | true -> p [] [ str (sprintf "%A increased from %d to %d!" counter.Counter counter.OldValue counter.NewValue) ]
        | false -> p [] [ str (sprintf "%A dropped from %d to %d :(" counter.Counter counter.OldValue counter.NewValue) ]

let playerEntry (player: NewsfeedPlayerEntry) =
    div [ Class "pb-3 last:pb-0" ] [
        p [ Class "text-grey-700 text-base text-2xl" ] [ str player.Name ]
        player.Events |> List.map eventDisplay |> ofList
    ]

type NewsfeedTickProps = {
    Tick: NewsfeedTick
}

let newsfeedTick = elmishView "NewsfeedEvent" (fun ({ Tick = entry }: NewsfeedTickProps) ->
    div [ Class "flex flex-col w-full bg-white rounded shadow-lg my-8" ] [
        div [ Class "flex flex-col w-full bg-white rounded shadow-lg" ] [
            div [ Class "flex flex-col w-full md:flex-row" ] [
                div [ Class "flex flex-row justify-around p-6 font-bold leading-none text-gray-800 uppercase bg-gray-400 rounded md:flex-col md:items-center md:justify-center md:w-1/5" ] [
                    div [ Class "md:text-5xl" ] [ str (entry.Time.ToString("HH:00")) ]
                ]
                div [ Class "p-6 font-normal text-gray-800 md:w-3/4" ] [
                    (entry.Players |> List.map playerEntry |> ofList)
                ]
            ]
        ]
    ]
)

type NewsfeedDayProps = { Date: DateTime; Ticks: NewsfeedTick list }
let newsfeedDay = elmishView "NewsfeedDay" (fun (props: NewsfeedDayProps) ->
    div [] [
        h2 [ Class "text-6xl text-gray-200 text-center" ] [ str (props.Date.ToString("dd/MM")) ]
        props.Ticks
            |> List.sortByDescending (fun tick -> tick.Tick)
            |> List.map (fun tick -> newsfeedTick { Tick = tick })
            |> ofList
    ]
)

type NewsfeedProps = {
    Model: Model
    Dispatch: Msg -> unit
}

let ticksByDay newsfeed =
    newsfeed.Ticks |> List.groupBy (fun tick -> tick.Time.Date)

let filterNewsfeed (newsfeed: Newsfeed) filter =
    let filterEvent filter (event: NewsfeedEvent): NewsfeedEvent option =
        match filter, event with
        | (All, _) -> Some event
        | (ExcludeShips, Research _) -> Some event
        | (ExcludeShips, Counter counter) ->
            match counter.Counter with
            | Ships -> None
            | _ -> Some event

    let filterPlayer filter (player: NewsfeedPlayerEntry) =
        let events = player.Events |> List.choose (filterEvent filter)
        match events with
        | [] -> None
        | _ -> Some { player with Events = events; }

    let filterTick filter (tick: NewsfeedTick) =
        let players = tick.Players |> List.choose (filterPlayer filter)
        match players with
        | [] -> None
        | _ -> Some { tick with Players = players; }

    let ticks = newsfeed.Ticks |> List.choose (filterTick filter)
    { newsfeed with Ticks = ticks }

let render = elmishView "Newsfeed" (fun { Model = model; Dispatch = dispatch } ->
    match model.Newsfeed with
    | Remote.Empty -> str "Loading"
    | Loading -> str "Loading"
    | Error error -> str "Error"
    | Body newsfeed ->
       let newsfeed = filterNewsfeed newsfeed model.Filter
       div [ Class "w-full" ] [
           newsfeed.Ticks
           |> List.sortByDescending (fun tick -> tick.Tick)
           |> List.groupBy (fun tick -> tick.Time.Date)
           |> List.map (fun (date, ticks) -> newsfeedDay { Date = date; Ticks = ticks }) |> ofList
       ]
)
