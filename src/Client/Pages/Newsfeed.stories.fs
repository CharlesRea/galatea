module Newsfeed.Stories

open Fable.React
open Storybook
open Shared
open Fable.Core.JsInterop

importAll "@fortawesome/fontawesome-free/css/all.css"
importAll "../css/style.css"

let player: NewsfeedPlayer =
    { Id = PlayerId 1
      Name = "Bob"
      Stars = 232
      Ships = 523
      Economy = 24
      Industry = 11
      Science = 7
      Scanning = 2
      HyperspaceRange = 3
      Terraforming = 5
      Experimentation = 3
      Weapons = 11
      Banking = 3
      Manufacturing = 2 }

let players = seq { 1..14 } |> Seq.map (fun i -> { player with Id = PlayerId i; Name = "Player " + i.ToString(); }) |> List.ofSeq

let dispatch msg = ()

storiesOf("Newsfeed", webpackModule)
    .add("Player card", (fun _ -> playerCard { Player = player; IsSelected = false; Dispatch = dispatch }))
    .add("Player cards", (fun _ -> div [ Classes [ tw.bgGray700; tw.m4 ] ] [ playerCards { Players = players; SelectedPlayers = [PlayerId 2; PlayerId 4;]; Dispatch = dispatch } ]))
    |> ignore
