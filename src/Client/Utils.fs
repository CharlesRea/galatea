[<AutoOpen>]
module Client.Utils

open Fable.React
open Fable.React.Props

let inline elmishView name render = FunctionComponent.Of(render, name, equalsButFunctions)

let ClassNames classes = ClassName (classes |> String.concat " ")
