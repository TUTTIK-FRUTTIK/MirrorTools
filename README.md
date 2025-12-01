# MirrorTools

<figure><img src=".gitbook/assets/MirrorToolsBanner.png" alt=""><figcaption></figcaption></figure>

### Mirror Tools

A set of tools for games based on Unity + Mirror for **Unity 2021 and newer**.

### Features

Mirror Tools has 5 modules! Each module is a separate tool that allows you to get certain information or perform logic on the server side.

* General module - allows you to see the current status of the server and other general data.
* Players module - the list of active players and their current data.
* NetIdentities module - the list of Network Identities on the scene.
* Log module - server-side user logging and log history.
* Command Console module - a command console that allows you to execute any code on the server side.

### Getting Started

Get Unity, [download Mirror](https://assetstore.unity.com/packages/tools/network/mirror-129321), import our unitypackage & press play!&#x20;

By pressing the "\~" key You will be able to open the control panel. Enjoy!



### How To Use

#### Password

You can force the server to require a password from users before opening the control panel to them. To set a password, open the Assets/Resources/MirrorTools/Config/MainConfig file and change the password in the inspector.

After the client enters the correct password, it will be saved in the client's local storage, so that when the control panel is reopened, the client does not have to enter the password again.

#### General module

All the indicators are described here and what they mean.

* Connection count - the number of currently connected users out of the maximum available number (which is specified in NetworkManager.maxConnections).
* Player Count - The number of connected users who currently have an active network object.
* Network object count - the number of Network Identities on the scene.
* Main object count - number of objects on the stage (not counting children).
* Memory usage - the amount of RAM that Unity Application uses out of all available RAM on the computer.
* Current FPS - the current number of FPS on the server.

#### Players module

You can display other information about the players that you would like to see on this panel (for example, nicknames, number of coins, etc.).\
To do this, it is enough to create an instance of the `ConnectionData` class and assign its `displayedInfo` field the desired value. After that, you must assign the newly created instance of the class to the client's `conn.authenticatedData` field.

#### Log module

Use the `MTools.Log()` method to write a log in the logging panel. This method can only be called on the server side!

#### Command Console module

Command Console has command history and autocomplete.&#x20;

You can add your command to the console. To do this, create a method with the `[ConsoleCommand("NameYourCommand")]` attribute. For example:

```csharp
[ConsoleCommand("my_command")]
private void MyCommand()
{
    Debug.log("Hello World");
}
```

Now let's create a command with a description and parameters!

```csharp
[ConsoleCommand("test", "this is test command")]
private static void TestCommand(NetworkConnectionToClient sender, int count)
{
    MTools.ConsoleWrite(sender, count.ToString());
}
```

As we can see, if we add a parameter like NetworkConnectionToClient and name it as "sender", the console will automatically transmit the connection that triggered this command to the server in the parameter.

If we name this parameter differently, we can specify another connection in the console by specifying the connId of the player. You can also specify other connections in the console using your own connection names. In order to name a connection, you need to create an instance of the `ConnectionData` class, assign the `name` field to this instance of the class, and assign this instance of the class to the `conn.authenticatedData` field.

Limitations:

You can create methods with the \[ConsoleCommand] attribute only in NetworkBehaviour classes, or create static methods with this attribute anywhere.

List of supported parameter types for methods with the \[ConsoleCommand] attribute:

* string
* bool
* int
* float
* NetworkIdentity (name of gameobject)
* NetworkConnectionToClient (connId or ConnectionData.name)
