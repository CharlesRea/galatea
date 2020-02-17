open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open DataFetcher.ExistingParser

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

let configureLogging (builder : ILoggingBuilder) =
    builder
           .AddConsole()
           .AddDebug()
    |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseDefaultFiles () |> ignore
    app.UseStaticFiles () |> ignore

let configureServices (services : IServiceCollection) =
    services.AddHostedService<NpData.SnapshotFetcher>() |> ignore

[<EntryPoint>]
let main args =
//    importExistingSnapshots ()

    Host.CreateDefaultBuilder(args)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
