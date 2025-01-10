Neuro-Credits™
-----------------

Neuro-Credits™ is a payment provider allowing users to buy eDaler® on credit. Bought credits will be invoiced to the corresponding buyer.

## Projects

The solution contains the following C# projects:

| Project                      | Framework         | Description |
|:-----------------------------|:------------------|:------------|
| `TAG.Payments.NeuroCredits`  | .NET Standard 2.1 | Service module for the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md), permitting authorized users to buy eDaler® on credit. |

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
| [Waher.Security.SHA3](https://www.nuget.org/packages/Waher.Security.SHA3/)         | Provides an implementation of the SHA3 algorithm. |

## Installable Package

The `TAG.Payments.NeuroCredits` project has been made into a package that can be downloaded and installed on any 
[TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
To create a package, that can be distributed or installed, you begin by creating a *manifest file*. The
`TAG.Payments.NeuroCredits` project has a manifest file called `TAG.Payments.NeuroCredits.manifest`. It defines the
assemblies and content files included in the package. You then use the `Waher.Utility.Install` and `Waher.Utility.Sign` command-line
tools in the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository, to create a package file and cryptographically
sign it for secure distribution across the Neuron network.

The Neuro-Credits™ service is published as a package on TAG Neurons. If your neuron is connected to this network, you can install the
package using the following information:

| Package information                                                                                                              ||
|:-----------------|:---------------------------------------------------------------------------------------------------------------|
| Package          | `TAG.NeuroCredits.package`                                                                                     |
| Installation key | `qHgVDhIcD7PlhOFLY5fM0qMM5dqG6Lk6mmFTUk40Ap2muhOwtZ9OjE++idf4PxjAWCRnxxWi3PKA193514394afa737059b1995f7270000e` |
| More Information | TBD                                                                                                            |

## Building, Compiling & Debugging

The repository assumes you have the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository cloned in a folder called
`C:\My Projects\IoT Gateway`, and that this repository is placed in `C:\My Projects\NeuronCredits`. You can place the
repositories in different folders, but you need to update the build events accordingly. To run the application, you select the
`TAG.Payments.NeuroCredits` project as your startup project. It will execute the console version of the
[IoT Gateway](https://github.com/PeterWaher/IoTGateway), and make sure the compiled files of the `NeuronCredits` solution
is run with it.

### Configuring service

You configure the service via the browser, by navigating to the `/NeuroCredits/Settings.md` resource, and entering the requested
information. Following parameters are available:

| Parameter                                    | Description                                                        |
|:---------------------------------------------|:-------------------------------------------------------------------|
| TBD                                          | TBD                                                                |

### Gateway.config

To simplify development, once the project is cloned, add a `FileFolder` reference
to your repository folder in your [gateway.config file](https://lab.tagroot.io/Documentation/IoTGateway/GatewayConfig.md). 
This allows you to test and run your changes to Markdown and Javascript immediately, 
without having to synchronize the folder contents with an external 
host, or recompile or go through the trouble of generating a distributable software 
package just for testing purposes. Changes you make in .NET can be applied in runtime
if you the *Hot Reload* permits, otherwise you need to recompile and re-run the
application again.

Example of how to point a web folder to your project folder:

```
<FileFolders>
  <FileFolder webFolder="/NeuroCredits" folderPath="C:\My Projects\NeuroCredits\TAG.Payments.NeuroCredits\Root\NeuroCredits"/>
</FileFolders>
```

**Note**: Once the file folder reference is added, you need to restart the IoT Gateway service for the change to take effect.

**Note 2**:  Once the gateway is restarted, the source for the files is in the new location. Any changes you make in the corresponding
`ProgramData` subfolder will have no effect on what you see via the browser.

**Note 3**: This file folder is only necessary on your developer machine, to give you real-time updates as you edit the files in your
developer folder. It is not necessary in a production environment, as the files are copied into the correct folders when the package 
is installed.

