# SocketApp

A simple interactive socket. Cross-platform.

## Features

- Console app. Cross-platform.
- Build the host and the client at the same time
- Easy to setup a new Socket experiment
- Including additional network layers on top of the transport layer. Learn more from `SocketApp.ProtocolStack`.

## Build

- download dotnet SDK
- set pwd to the same directory as `.csproj` in
- run `dotnet run`

## Customization

Reaction for some events is defined in class `Responser`. Those Events are

- On data (byte[]) has passed down to the bottom of the protocol stack and is ready to send
- On data has been delivered up to the top of the protocol stack and is ready for APP to consume
- On Accept() has done
- On Connect() has done
- On Receiving new message (which has not been processed by the protocol stack yet)
- On Socket is about to shutdown

To Modify the protocol stack, goto method `GetDefaultStack` in `DefaultProtocolFactory` as an example.

To build a new layer of protocol, derive your class from `IProtocol`.

To register your protocols, build your own protocol factory deriving from `IProtocolFactory`, and pass it as a parameter to `BeginBuildTcp()` from class `SockController`

## TODO

- [ ] Deport ProtocolStack to an independent project.
- [ ] Support UDP
- [ ] SocketApp Hosts a Http Proxy Server
- [ ] Big file transportation protocol
- [ ] replace callback mechanism with async
