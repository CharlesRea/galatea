module Newsfeed

open Shared
open Types
open Elmish
open Fable.React
open Api
open Client
open Fable.React.Props

type Model =
    { Newsfeed: Remote<Newsfeed> }
    static member Empty: Model = { Newsfeed = Remote.Empty }

type Msg = NewsfeedRequest of RequestMsg<unit, Newsfeed>

let init() =
    { Newsfeed = Remote.Empty }, Cmd.ofMsg (NewsfeedRequest(Initiate()))

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
    | Counter counter ->
        let hasIncreased = counter.OldValue < counter.NewValue
        match hasIncreased with
        | true -> p [] [ str (sprintf "%A increased from %d to %d!" counter.Counter counter.OldValue counter.NewValue) ]
        | false -> p [] [ str (sprintf "%A dropped from %d to %d :(" counter.Counter counter.OldValue counter.NewValue) ]

let playerEntry (player: NewsfeedPlayerEntry) =
    div [] [
        p [ Class "text-grey-700 text-base" ] [ str player.Name ]
        player.Events |> List.map eventDisplay |> ofList
    ]

type NewsfeedTickProps = {
    Tick: NewsfeedTick
}

let newsfeedTick = elmishView "NewsfeedEvent" (fun ({ Tick = entry }: NewsfeedTickProps) ->
    div [ Class "max-w-sm rounded overflow-hidden shadow-lg mb-10 bg-white" ]
            [ div [ Class "px-6 py-4" ]
                [ div [ Class "font-bold text-xl mb-2" ]
                    [ str (entry.Time.ToString("dd/MM HH:00")) ]
                  (entry.Players |> List.map playerEntry |> ofList)
                  p [ Class "text-gray-700 text-base" ]
                    [ str "Lorem ipsum dolor sit amet, consectetur adipisicing elit. Voluptatibus quia, nulla! Maiores et perferendis eaque, exercitationem praesentium nihil." ] ] ]
)

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
       newsfeed.Ticks |> List.map (fun tick -> newsfeedTick { Tick = tick }) |> ofList

)
