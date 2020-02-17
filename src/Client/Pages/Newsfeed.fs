module Newsfeed

open System
open Shared
open Types
open Elmish
open Fable.React
open Api
open Client
open Fable.React.Props
open Fable.FontAwesome

type EventFilter =
    | All
    | ResearchOnly
    | InfrastructureOnly
    | MetricsOnly

type Model = { Newsfeed: Remote<Newsfeed>; EventFilter: EventFilter }

type Msg =
    | NewsfeedRequest of RequestMsg<unit, Newsfeed>
    | SetEventFilter of EventFilter

let init() =
    { Newsfeed = Remote.Empty; EventFilter = All }, Cmd.ofMsg (NewsfeedRequest(Initiate()))

let filterEmptyTicks newsfeed =
    let ticks = newsfeed.Ticks |> List.filter (fun tick -> tick.Players |> List.isEmpty |> not)
    { newsfeed with Ticks = ticks }

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | NewsfeedRequest(Initiate _) ->
        { model with Newsfeed = Loading }, (Cmd.ofRequest api.newsfeed () NewsfeedRequest)
    | NewsfeedRequest(Fetched response) -> { model with Newsfeed = Body (filterEmptyTicks response) }, Cmd.none
    | NewsfeedRequest(FetchError error) -> { model with Newsfeed = Error error }, Cmd.none
    | SetEventFilter filter -> { model with EventFilter = filter }, Cmd.none


let filterNewsfeed (newsfeed: Newsfeed) filter =
    let filterShipChanges (event: NewsfeedEvent): NewsfeedEvent option =
        match event with
        | Counter counter ->
            match counter.Counter with
            | Ships ->
                match counter.NewValue > counter.OldValue with
                | true when ((counter.NewValue / 100) > (counter.OldValue / 100)) -> Some event
                | false when counter.OldValue - counter.NewValue > 10 -> Some event
                | _ -> None
            | _ -> Some event
        | _ -> Some event

    let filterEvent filter (event: NewsfeedEvent): NewsfeedEvent option =
        match filter, event with
        | (All, _) -> Some event
        | (ResearchOnly, Research _) -> Some event
        | (InfrastructureOnly, Counter counter) ->
             match counter.Counter with
             | Economy -> Some event
             | Industry -> Some event
             | Science -> Some event
             | _ -> None
        | (MetricsOnly, Counter counter) ->
            match counter.Counter with
            | Ships -> Some event
            | Stars -> Some event
            | _ -> None
        | _, _ -> None

    let filterPlayer filter (player: NewsfeedPlayerEntry) =
        let events = player.Events |> List.choose (filterEvent filter) |> List.choose filterShipChanges
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

let filterButton dispatch (selectedFilter: EventFilter) (filter: EventFilter) =
    let label = match filter with
                | All -> "All"
                | ResearchOnly -> "Research"
                | InfrastructureOnly -> "Infrastructure"
                | MetricsOnly -> "Metrics"

    let icon = match filter with
                | All -> Fa.Solid.Globe
                | ResearchOnly -> Fa.Solid.GraduationCap
                | InfrastructureOnly -> Fa.Solid.Building
                | MetricsOnly -> Fa.Solid.Signal

    let colour = match filter with
                 | EventFilter.All -> "green"
                 | ResearchOnly -> "blue"
                 | InfrastructureOnly -> "red"
                 | MetricsOnly -> "purple"

    let activeClasses =
        match filter = selectedFilter with
        | true -> sprintf "border-%s-600 bg-%s-500 text-white" colour colour
        | false -> sprintf "border-%s-500 bg-white text-grey-800" colour

    button [
        Class (String.Format("bg-white text-2xl border-b-4 hover:bg-{0}-600 hover:text-white
                             font-bold py-4 px-6 shadow-md " + activeClasses, colour))
        OnClick (fun _ -> dispatch (SetEventFilter filter))
    ] [
        Fa.i [ icon ] [ ]
        span [ Class "pl-2" ] [ str label ]
    ]

let filters (model: Model) dispatch =
    div [ Class "flex items-center justify-center mb-4" ] [
        [ All; ResearchOnly; InfrastructureOnly; MetricsOnly ] |> List.map (filterButton dispatch model.EventFilter) |> ofList
    ]

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

let ticksByDay newsfeed =
    newsfeed.Ticks |> List.groupBy (fun tick -> tick.Time.Date)

type NewsfeedProps = {
    Model: Model
    Dispatch: Msg -> unit
}

let render = elmishView "Newsfeed" (fun { Model = model; Dispatch = dispatch } ->
    match model.Newsfeed with
    | Remote.Empty -> str "Loading"
    | Loading -> str "Loading"
    | Error error -> str "Error"
    | Body newsfeed ->
       let newsfeed = filterNewsfeed newsfeed model.EventFilter
       div [ Class "w-full" ] [
           filters model dispatch
           newsfeed.Ticks
           |> List.sortByDescending (fun tick -> tick.Tick)
           |> List.groupBy (fun tick -> tick.Time.Date)
           |> List.map (fun (date, ticks) -> newsfeedDay { Date = date; Ticks = ticks }) |> ofList
       ]
)
