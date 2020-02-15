# Galatea - A Neptune's Pride dashboard

F# Application to track and visualise the state of a game of Neptune's Pride.
Makes use of the [SAFE Stack](https://safe-stack.github.io/) for full-stack F# development.

Components used include:
* [Saturn](https://saturnframework.org/docs/) - F# web server on top of ASP.NET Core
* [Fable](https://fable.io/docs/) - F# to JS compiler
* [Elmish](https://elmish.github.io/elmish/) - Model-View-Update architecture for state management
* [Feliz](https://zaid-ajaj.github.io/Feliz/) - React bindings for F#
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/) - Type safe RPC style HTTP API calls

## Development setup

### Pre-requisites required
* The [.NET Core SDK 3.1+](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* [Yarn](https://yarnpkg.com/lang/en/docs/install/)
* [Node LTS](https://nodejs.org/en/download/)
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

### Work with the application


To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```
