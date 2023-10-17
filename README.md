Neuro-Credits™
-----------------

Neuro-Credits™ is a payment provider allowing users to buy eDaler® on credit. Bought credits will be invoiced to the corresponding buyer.

## Projects

The solution contains the following C# projects:

| Project                      | Framework         | Description |
|:-----------------------------|:------------------|:------------|
| `TAG.Payments.NeuroCredits`  | .NET Standard 2.0 | Service module for the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md), permitting authorized users to buy eDaler® on credit. |

## Nugets

The following nugets external are used. They faciliate common programming tasks, and
enables the libraries to be hosted on an [IoT Gateway](https://github.com/PeterWaher/IoTGateway).
This includes hosting the bridge on the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
They can also be used standalone.

| Nuget                                                                              | Description |
|:-----------------------------------------------------------------------------------|:------------|
| [Paiwise](https://www.nuget.org/packages/Paiwise)                                  | Contains services for integration of financial services into Neurons. |
| [Waher.Content](https://www.nuget.org/packages/Waher.Content/)                     | Pluggable architecture for accessing, encoding and decoding Internet Content. |
| [Waher.Events](https://www.nuget.org/packages/Waher.Events/)                       | An extensible architecture for event logging in the application. |
| [Waher.IoTGateway](https://www.nuget.org/packages/Waher.IoTGateway/)               | Contains the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) hosting environment. |
| [Waher.Runtime.Counters](https://www.nuget.org/packages/Waher.Runtime.Counters/)   | Framework for managing counters on the Neuron®. |
| [Waher.Runtime.Inventory](https://www.nuget.org/packages/Waher.Runtime.Inventory/) | Maintains an inventory of type definitions in the runtime environment, and permits easy instantiation of suitable classes, and inversion of control (IoC). |
| [Waher.Runtime.Settings](https://www.nuget.org/packages/Waher.Runtime.Settings/)   | Provides easy access to persistent settings. |
| [Waher.Runtime.Threading](https://www.nuget.org/packages/Waher.Runtime.Threading/) | Simplifies the creation of user-specific semaphores in a multi-threaded multi-user environment. |
| [Waher.Security](https://www.nuget.org/packages/Waher.Security/)                   | Contains basic cryptography functions. |
