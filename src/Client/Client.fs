module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Thoth.Json

open Fable.React.ReactiveComponents
open Shared

type PageModel =
    | NewsfeedModel of Newsfeed.Model
    | NotFoundModel
type Model = { Page: PageModel }

type Msg =
    | NewsfeedMsg of Newsfeed.Msg

let init () : Model * Cmd<Msg> =
    let subModel, msg = Newsfeed.init()
    { Page = NewsfeedModel subModel }, Cmd.map NewsfeedMsg msg

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg, model.Page with
    | NewsfeedMsg msg, NewsfeedModel m ->
        let m, cmd = Newsfeed.update msg m
        { model with Page = NewsfeedModel m }, Cmd.map NewsfeedMsg cmd
    | _ -> model, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    div [] [
        match model.Page with
        | NewsfeedModel model ->
            yield Newsfeed.render { Model = model; Dispatch = (NewsfeedMsg >> dispatch) }
        | NotFoundModel ->
            yield div [] [ str "The page is not available." ]
    ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
