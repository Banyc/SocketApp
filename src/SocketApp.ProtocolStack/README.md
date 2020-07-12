# SocketApp.ProtocolStack

Protocol stack is the additional layers built on top of the Transport Layer. It helps socket connection add security layer easier and neater. In order words, those security logic could be departed from the business logic.

## Terms

The term "protocol" and "layer" refer to the same thing.

## Principle

Any protocol should be inherited from `IProtocol`.

The protocol stack supposes the higher the application layer and the lower the physical layer.

`DataContent` is the only data structure that passes through all additional layers.

`ProtocolStack` represent a pile of protocols. It could help links protocols together.

## TODO

- [ ] Build an ASP.NET Core-like dependency injection mechanism.
