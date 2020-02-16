module Types

open Elmish

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