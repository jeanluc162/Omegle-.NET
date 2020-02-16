# Omegle-.NET
A simple .NET wrapper for Omegle

The goal of this project is to provide a simple .NET wrapper for omegle, the anonymous chat website, so people can use it for clients, bots or something else. check out omegles API here: https://gist.github.com/nucular/e19264af8d7fc8a26ece

This project is **targeted at .NET 3.5** and is being developed using **Visual Studio 2008**, opening it in a newer version and/or changing the Target-Framework should however not be a problem.

# What's working?

Just normal chatting, meaning you can

- Connect
- Disconnect
- Send a Message
- Set your Typing State
- Get Notified via Events
  - When the strangers typing state changes
  - When the stranger sends a message
  - When the stranger disconnects
  - When the stranger connects
  
# What's left to do?

Bugfixing, mainly. In principle, other functions such as

- Same interests
- Spy-Mode
- Spyee-Mode
  
are already implemented, however, they just don't seem to work. The client always get's connected to a normal chat instead.

# How can I use the API?

You can either just clone the whole Project or download a compiled DLL from the "Releases" Section. You also need a Json.NET DLL for .Net 3.5 in your application directory.

When you have referenced **OmegleAPI** in your project, just create a **OmegleAPI.Client**, attach some eventhandlers and do a `ConnectChat(null)`. Keep in Mind that you should always do a proper `Disconnect()` before reconnecting, even if the stranger disconnected first.

You can use the **OmegleClientWin** Project as an example for how to do things.

If you just want to see that it works, there is also a precompiled Client available under "Releases". It's not pretty, but it get's the job done.
