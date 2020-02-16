namespace Shared

open System

type ResearchEvent = {
    Tech: string
    Level: int
}

type Counter =
    | Stars
    | Ships
    | Economy
    | Industry
    | Science

type CounterEvent = {
    Counter: Counter
    OldValue: int
    NewValue: int
}

type NewsfeedEvent = Research of ResearchEvent | Counter of CounterEvent

type NewsfeedPlayerEntry = {
    Name: string
    Events: NewsfeedEvent list
}

type NewsfeedTick = {
    Players: NewsfeedPlayerEntry list
    Tick: int
    Time: DateTimeOffset
}

type Newsfeed = {
    Ticks: NewsfeedTick list
}

module Route =
    let builder typeName methodName =
        sprintf "/api/%s" methodName

type IGalateaApi = {
      newsfeed: unit -> Async<Newsfeed> }
