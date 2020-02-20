module Newsfeed

open System
open Shared
open Types
open Elmish
open Fable.React
open Api
open Fable.React.Props
open Fable.FontAwesome

type EventFilter =
    | All
    | ResearchOnly
    | InfrastructureOnly
    | MetricsOnly

type Model = { Newsfeed: Remote<Newsfeed>; EventFilter: EventFilter; SelectedPlayers: PlayerId list }

type Msg =
    | NewsfeedRequest of RequestMsg<unit, Newsfeed>
    | SetEventFilter of EventFilter
    | SelectPlayer of PlayerId

let init() =
    { Newsfeed = Remote.Empty; EventFilter = All; SelectedPlayers = [] }, Cmd.ofMsg (NewsfeedRequest(Initiate()))

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

    | SelectPlayer playerId ->
        let players =
            match (model.SelectedPlayers |> List.contains playerId) with
            | true -> model.SelectedPlayers |> List.filter (fun p -> not (p = playerId))
            | false -> playerId :: model.SelectedPlayers
        { model with SelectedPlayers = players }, Cmd.none

let filterPlayers (selectedPlayers: PlayerId list) (newsfeed: Newsfeed)  =
    match selectedPlayers with
    | [] -> newsfeed
    | selectedPlayers ->
        let filterPlayer (player: NewsfeedPlayerEntry) =
            match selectedPlayers |> List.contains player.PlayerId with
            | true -> Some player
            | false -> None

        let filterTick (tick: NewsfeedTick) =
            let players = tick.Players |> List.choose filterPlayer
            match players with
            | [] -> None
            | _ -> Some { tick with Players = players; }

        let ticks = newsfeed.Ticks |> List.choose filterTick
        { newsfeed with Ticks = ticks }

let filterNewsfeed filter (newsfeed: Newsfeed) =
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

let colours = [| "#0333ff"; "#18a3c1"; "#4bbb02"; "#ffbe0e"; "#e16100"; "#c11900"; "#c12ebf"; "#6e28c3"; |]
let playerColour (player: NewsfeedPlayer): (string * Fa.IconOption) =
    let (PlayerId playerId) = player.Id
    let colour = colours.[playerId % 8]
    let shape = match (playerId / 8) with
                | 0 -> Fa.Regular.Circle
                | _ -> Fa.Regular.Square

    (colour, shape)


type PlayerCardProps = { Player: NewsfeedPlayer; IsSelected: bool; Dispatch: Msg -> unit }
let playerCard = elmishView "PlayerCard" (fun ({ Player = player; IsSelected = isSelected; Dispatch = dispatch; }: PlayerCardProps) ->
    let (colour, shape) = playerColour player
    let selectedClass = if isSelected then "border-4" else ""

    div [
        Class ("w-1/4 m-4 bg-white text-2xl border-b-4 py-4 px-6 shadow-md cursor-pointer " + selectedClass)
        Style [ BorderColor colour ]
        OnClick (fun _ -> dispatch (SelectPlayer player.Id) )
    ] [
        div [ Class "flex flex-row justify-between" ] [
            div [] [
                span [ Class "mr-2 align-text-top"; Style [ Color colour ] ] [ Fa.i [ shape ] [ ] ]
                span [ Class "font-bold" ] [ str player.Name ]
            ]
            div [] [
                str (string player.Stars)
                span [ Class "ml-2 align-text-bottom text-yellow-600" ] [ Fa.i [ Fa.Solid.Star ] [ ] ]
                span [ Class "ml-4" ] [ str (string player.Ships) ]
                span [ Class "ml-2 align-text-bottom text-blue-500" ] [ Fa.i [ Fa.Solid.Rocket ] [ ] ]
            ]
        ]
        div [ Class "flex flex-row justify-between text-sm" ] [
            div [] [
                span [ Class "" ] [ str (string player.Economy) ]
                span [ Class "ml-2 align-text-top"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.MoneyBill ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Industry) ]
                span [ Class "ml-2 align-text-top"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Industry ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Science) ]
                span [ Class "ml-2 align-text-top"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.GraduationCap ] [ ] ]
            ]
            div [] [
                span [ Class "" ] [ str (string player.Scanning) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.SatelliteDish; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.HyperspaceRange) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Ruler; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Terraforming) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Tractor; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Experimentation) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Flask; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Weapons) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Utensils; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Banking) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.PiggyBank; Fa.Size Fa.FaSmall ] [ ] ]
                span [ Class "ml-3" ] [ str (string player.Manufacturing) ]
                span [ Class "align-text-top ml-1"; Style [ Color colour ] ] [ Fa.i [ Fa.Solid.Pallet; Fa.Size Fa.FaSmall ] [ ] ]

            ]
        ]
    ]
)

type PlayerCardsProps = { Players: NewsfeedPlayer list; SelectedPlayers: PlayerId list; Dispatch: Msg -> unit }
let playerCards = elmishView "PlayerCard" (fun ({ Players = players; SelectedPlayers = selectedPlayers; Dispatch = dispatch }: PlayerCardsProps) ->
    div [ Class "flex flex-row flex-wrap justify-center w-full -m-4 mb-8" ] [
        players |> List.map (fun player -> playerCard { Player = player; IsSelected = selectedPlayers |> List.contains player.Id; Dispatch = dispatch }) |> ofList
    ]
)

let playerEntry (newsfeed: Newsfeed) (playerEntry: NewsfeedPlayerEntry) =
    let player = newsfeed.Players.[playerEntry.PlayerId]
    let (colour, shape) = playerColour player
    div [ Class "pb-3 last:pb-0" ] [
        p [ Class "text-gray-800 text-base text-2xl" ] [
            span [ Class "mr-2 align-text-top"; Style [ Color colour ] ] [ Fa.i [ shape ] [ ] ]
            str player.Name
        ]
        playerEntry.Events |> List.map eventDisplay |> ofList
    ]

type NewsfeedTickProps = { Newsfeed: Newsfeed; Tick: NewsfeedTick }
let newsfeedTick = elmishView "NewsfeedEvent" (fun ({ Tick = tick; Newsfeed = newsfeed }: NewsfeedTickProps) ->
    div [ Class "flex flex-col w-full bg-white rounded shadow-lg my-8" ] [
        div [ Class "flex flex-col w-full bg-white rounded shadow-lg" ] [
            div [ Class "flex flex-col w-full md:flex-row" ] [
                div [ Class "flex flex-row justify-around p-6 font-bold leading-none text-gray-800 uppercase bg-gray-400 rounded md:flex-col md:items-center md:justify-center md:w-1/5" ] [
                    div [ Class "md:text-5xl" ] [ str (tick.Time.ToString("HH:00")) ]
                ]
                div [ Class "p-6 font-normal text-gray-800 md:w-3/4" ] [
                    (tick.Players |> List.map (playerEntry newsfeed) |> ofList)
                ]
            ]
        ]
    ]
)

type NewsfeedDayProps = { Newsfeed: Newsfeed; Date: DateTime; Ticks: NewsfeedTick list }
let newsfeedDay = elmishView "NewsfeedDay" (fun (props: NewsfeedDayProps) ->
    div [] [
        h2 [ Class "text-6xl text-gray-200 text-center" ] [ str (props.Date.ToString("dd/MM")) ]
        props.Ticks
            |> List.sortByDescending (fun tick -> tick.Tick)
            |> List.map (fun tick -> newsfeedTick { Newsfeed = props.Newsfeed; Tick = tick })
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
       let newsfeed = newsfeed |> filterPlayers model.SelectedPlayers |> filterNewsfeed model.EventFilter
       div [ Class "w-full flex flex-col items-center justify-center" ] [
           playerCards { Players = newsfeed.Players
                                   |> Map.toSeq
                                   |> Seq.map snd
                                   |> Seq.sortByDescending (fun player -> (player.Stars, player.Ships))
                                   |> Seq.toList;
                        SelectedPlayers = model.SelectedPlayers;
                        Dispatch = dispatch }

           div [ Class "w-full container flex flex-col items-center justify-center" ] [
               div [ Class "w-full" ] [
                   filters model dispatch
                   newsfeed.Ticks
                   |> List.sortByDescending (fun tick -> tick.Tick)
                   |> List.groupBy (fun tick -> tick.Time.Date)
                   |> List.map (fun (date, ticks) -> newsfeedDay { Newsfeed = newsfeed; Date = date; Ticks = ticks }) |> ofList
               ]
           ]
       ]
)
