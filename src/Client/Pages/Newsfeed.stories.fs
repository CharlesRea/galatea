module Newsfeed.Stories

open Fable.React
open Storybook
open Shared
open Fable.Core.JsInterop

importAll "@fortawesome/fontawesome-free/css/all.css"
importAll "../css/style.css"

let player: NewsfeedPlayer =
    { Id = 1
      Name = "Bob"
      Stars = 232
      Economy = 24
      Industry = 11
      Science = 7 }

let players = seq { 1..14 } |> Seq.map (fun i -> { player with Id = i; Name = "Player " + i.ToString(); }) |> List.ofSeq

storiesOf("Newsfeed", webpackModule)
    .add("Player card", (fun _ -> playerCard { Player = player }))
    .add("Player cards", (fun _ -> div [ Classes [ tw.bgGray700; tw.m4 ] ] [ playerCards { Players = players } ]))
    |> ignore
