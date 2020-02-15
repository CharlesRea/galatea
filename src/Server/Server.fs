open System.IO

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Shared

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let galateaApi: IGalateaApi = {
    initialCounter = fun () -> async { return { Value = 42 } }
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue galateaApi
    |> Remoting.buildHttpHandler

let configureLogging (builder : ILoggingBuilder) =
    builder
           .AddConsole()
           .AddDebug()
    |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles () |> ignore
    app.UseStaticFiles () |> ignore
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddHostedService<NpData.DataFetchingService>() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
