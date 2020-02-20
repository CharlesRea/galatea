module Layout

open Fable.React
open Fable.React.Props

let layout (children: ReactElement list) =
    div [ Class "mx-auto bg-gray-700 min-h-screen flex items-center justify-center p-10" ] [
        div [ Class "flex items-center justify-center" ] children
    ]