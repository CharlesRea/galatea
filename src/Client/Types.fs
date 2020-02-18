[<AutoOpen>]
module Types

open Elmish
open Zanaptak.TypedCssClasses

type tw = CssClasses<"./css/tailwind-generated.css", Naming.CamelCase>

type Remote<'response> =
    | Empty
    | Loading
    | Error of exn
    | Body of 'response

type RequestMsg<'input, 'output> =
    | Initiate of 'input
    | Fetched of 'output
    | FetchError of exn

module Cmd =
    let ofRequest (task: 'a -> Async<_>) (arg: 'a) (mapMsg: RequestMsg<'a, _> -> 'msg): Cmd<'msg> =
        Cmd.OfAsync.either task arg Fetched FetchError |> Cmd.map mapMsg
