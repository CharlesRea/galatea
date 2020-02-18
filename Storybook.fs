// ts2fable 0.7.0
module rec Storybook
open System
open Fable.Core
open Fable.Core.JS

type ClientStoryApi = @storybook_addons.ClientStoryApi
type Loadable = @storybook_addons.Loadable
type IStorybookSection = __types.IStorybookSection
type StoryFnReactReturnType = __types.StoryFnReactReturnType
let [<Import("storiesOf","@storybook/react/dist/client/preview")>] storiesOf: ClientApi = jsNative
let [<Import("configure","@storybook/react/dist/client/preview")>] configure: ClientApi = jsNative
let [<Import("addDecorator","@storybook/react/dist/client/preview")>] addDecorator: ClientApi = jsNative
let [<Import("addParameters","@storybook/react/dist/client/preview")>] addParameters: ClientApi = jsNative
let [<Import("clearDecorators","@storybook/react/dist/client/preview")>] clearDecorators: ClientApi = jsNative
let [<Import("setAddon","@storybook/react/dist/client/preview")>] setAddon: ClientApi = jsNative
let [<Import("forceReRender","@storybook/react/dist/client/preview")>] forceReRender: ClientApi = jsNative
let [<Import("getStorybook","@storybook/react/dist/client/preview")>] getStorybook: ClientApi = jsNative
let [<Import("raw","@storybook/react/dist/client/preview")>] raw: ClientApi = jsNative

type [<AllowNullLiteral>] ClientApi =
    inherit ClientStoryApi<StoryFnReactReturnType>
    abstract setAddon: addon: obj option -> unit
    abstract configure: loader: Loadable * ``module``: NodeModule -> unit
    abstract getStorybook: unit -> ResizeArray<IStorybookSection>
    abstract clearDecorators: unit -> unit
    abstract forceReRender: unit -> unit
    abstract raw: (unit -> obj option) with get, set

type DecoratorFn =
    Parameters<obj>
