module Api

open Elmish
open System.Threading.Tasks
open Shared
open Fable.Remoting.Client
open Types

let api : IGalateaApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Route.builder
  |> Remoting.buildProxy<IGalateaApi>
