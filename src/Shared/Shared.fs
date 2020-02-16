namespace Shared

type Counter = { Value : int }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IGalateaApi =
    { initialCounter : unit -> Async<Counter> }

