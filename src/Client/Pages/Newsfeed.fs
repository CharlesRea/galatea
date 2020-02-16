module Newsfeed

open Shared
open Types
open Elmish
open Fable.React
open Api
open Client
open Utils

type Model =
    { Newsfeed: Remote<Newsfeed> }
    static member Empty: Model = { Newsfeed = Remote.Empty }

type Msg = NewsfeedRequest of RequestMsg<unit, Newsfeed>

let init() =
    { Newsfeed = Remote.Empty }, Cmd.ofMsg (NewsfeedRequest(Initiate()))

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | NewsfeedRequest(Initiate _) ->
        { model with Newsfeed = Loading }, (Cmd.ofRequest api.newsfeed () NewsfeedRequest)
    | NewsfeedRequest(Fetched response) -> { model with Newsfeed = Body response }, Cmd.none
    | NewsfeedRequest(FetchError error) -> { model with Newsfeed = Error error }, Cmd.none

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
        div [] [ str "Loaded" ]
)
