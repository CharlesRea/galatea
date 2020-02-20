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
    PlayerId: int
    Events: NewsfeedEvent list
}

type NewsfeedTick = {
    Players: NewsfeedPlayerEntry list
    Tick: int
    Time: DateTimeOffset
}

type NewsfeedPlayer = {
    Id: int
    Name: string
    Stars: int
    Ships: int
    Economy: int
    Industry: int
    Science: int
    Scanning: int
    HyperspaceRange: int
    Terraforming: int
    Experimentation: int
    Weapons: int
    Banking: int
    Manufacturing: int
}

type Newsfeed = {
    Players: Map<int, NewsfeedPlayer>
    Ticks: NewsfeedTick list
}

module Route =
    let builder typeName methodName =
        sprintf "/api/%s" methodName

type IGalateaApi = {
      newsfeed: unit -> Async<Newsfeed> }
