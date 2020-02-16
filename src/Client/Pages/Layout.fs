module Layout

open Fable.React
open Fable.React.Props

let layout children =
    div [ Class "bg-gray-100 font-sans leading-normal tracking-normal min-h-screen" ]
        [ div [ Class "container w-full md:max-w-3xl mx-auto pt-20" ]
            [ div [ Class "w-full px-4 md:px-6 text-xl text-gray-800 leading-normal"
                    Style [ FontFamily "Georgia,serif" ] ]
                children
               ]
            ]