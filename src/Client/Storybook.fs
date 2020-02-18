/// Fable bindings for @storybook/react
module rec Storybook

open Fable.Core
open Fable.React

type [<AllowNullLiteral>] IExports =
    abstract storiesOf : name:string * ``module``:obj -> Story

type Renderable = ReactElement

type RenderFunction = unit -> Renderable

type [<AllowNullLiteral>] DecoratorParameters =
    [<Emit "$0[$1]{{=$2}}">] abstract Item : key:string -> obj option with get, set

type [<AllowNullLiteral>] Story =
    abstract kind : string
    abstract add : storyName:string * callback:RenderFunction * ?parameters:DecoratorParameters -> Story

/// Access a reference to the Webpack 'module' global variable
let [<Emit("module")>] webpackModule<'T> : 'T = jsNative

[<Import("storiesOf", from="@storybook/react")>]
let storiesOf(name: string, ``module``: obj): Story = jsNative
