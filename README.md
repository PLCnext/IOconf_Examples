# IOconf_Examples

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Web](https://img.shields.io/badge/PLCnext-Website-blue.svg)](https://www.phoenixcontact.com/plcnext)
[![Community](https://img.shields.io/badge/PLCnext-Community-blue.svg)](https://www.plcnext-community.net)

| Date       | Version | Authors                     |
|------------|---------|-----------------------------|
| 01.10.2021 | 1.0.0   | Eduard Muenz                |

## Description

PLCnext IOconf is a .NET Class Library that can be used to generate valid Axioline and Profinet I/O configuration files for PLCnext Control devices without the need to using PLCnext Engineer.

Software developers who want to build the functionality of PLCnext IOconf Interface into their own projects are free to use this application as a reference.

## Contributing

Contributions to this project are welcome.

## Alternatives

The project "PLCnext Technology - Bus Conductor" is useful in applications where the precise I/O configuration is not known at design-time, or must be changed dynamically at run-time. You can find this solution in the following repository:
https://github.com/PLCnext/BusConductor


## Quick Start

This procedure describes the use of "axioconf" application to configure the Axioline I/O bus based on AXC F 2152 STARTERKIT.

Hardware requirements:

Starter kit- AXC F 2152 STARTERKIT - (Order number 1046568), consisting of:

1. I/O module AXL F DI8/1 DO8/1 1H (Order number 2701916).
1. I/O module AXL F AI2 AO2 1H (Order number 2702072).
1. Interface module UM 45-IB-DI/SIM8 (Order number 2962997).
1. Patch cable FL CAT5 PATCH 2,0 (Order number 2832289).

Host machine software requirements:

1. Windows 10 or Linux operating system.
1. WinSCP tool (optional). 

Procedure:

- Verify/Install the .NET Framework 4.8 or higher e.g. via [Using Regedit](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed#use-registry-editor).

- Clone the Github repository and open via Windows CMD the directory "IOconf_Examples\src\axioconf\bin\Release".

- Execute the "axioconf.exe" application and enter the path to the not existing file for converting all possible axioline controllers and devices in user readable format, e.g.

   ```text
   C:\IOconf_Examples\src\axioconf\bin\Release>axioconf.exe C:\IOconf_Examples\AxioConfig.txt
   ```

- Open the created configuration file "AxioConfig.txt" and delete the needless controllers, except one that should be used. Then edit the rows with AXL F devices according to the required bus configuration by deleting, adding or duplicating. The consecutive row numbering is not importand for application. The scheme below shows an example of a axioline bus configuration based on AXC F 2152 STARTERKIT. Please note, the maximum number of connectable AXC F devices is limited to 63!

   ```text
   0;AXC F 2152;00/2020.0.0
   1;AXL F DI8/1 DO8/1 1H;
   2;AXL F AI2 AO2 1H;02/1.00
   ``` 
 
- Execute the "axioconf.exe" application again and enter the edited configuration file as a parameter, e.g.

   ```text
   C:\IOconf_Examples\src\axioconf\bin\Release>axioconf.exe C:\IOconf_Examples\AxioConfig.txt
   ``` 

- After successful execution, find the folder of interest with a long, cryptic name (e.g. fb34da19-270d-41f8-a871-41ab78110a5f) in e.g. ```C:\IOconf_Examples\src\axioconf\bin\Release``` directory.  This folder contains the Axioline bus configuration including a valid set of TIC files in subfolder "\PLCnext\Arp.Io.AxlC" e.g.: 

   ```text
   C:\IOconf_Examples\src\axioconf\bin\Release\fb34da19-270d-41f8-a871-41ab78110a5f\PLCnext\Arp.Io.AxlC
   ``` 
   
## Usage

- Create the subdirectory ```IOconf/Io``` in folder ```/opt/plcnext/projects``` on the PLC and deploy the axioline bus configuration into this folder via tool "WinSCP" or following commands in a terminal window on the host:

   ```text
   ssh admin@192.168.1.10 mkdir -p /opt/plcnext/projects/IOconf/Io
   ```
   ```text
   scp -r C:/IOconf_Examples/src/axioconf/bin/Release/fb34da19-270d-41f8-a871-41ab78110a5f/PLCnext/Arp.Io.AxlC admin@192.168.1.10:~/projects/IOconf/Io
  ```
- To automatically transfer the DI-Input-Signals to DO-Output-Signals via [GDS](https://www.plcnext.help/te/PLCnext_Runtime/GDS_Global_Data_Space.htm) (Global Data Space):
  - Create the subdirectory ```GDS``` in folder ```/opt/plcnext/projects/IOconf/```via tool WinSCP or command line:
  ```text
   ssh admin@192.168.1.10 mkdir -p /opt/plcnext/projects/IOconf/GDS
   ```
  - Copy the ``IOconf.gds.config`` configuration located in the ``data`` folder of this repository to the PLC via tool WinSCP or command line e.g.:
   ```text
   scp C:\IOconf_Examples\data\IOconf.gds.config admin@192.168.1.10:~/projects/IOconf/GDS/
   ```
  or if you allready configure a automatically transfer data between GDS variables in PLCnext Engineer:

   - Copy the GDS folder located in the ```/opt/plcnext/projects/PCWE/Plc``` directory to ```/opt/plcnext/projects/IOconf``` directory via tool "WinSCP" or following command in a terminal window on the host:
   ```text
    ssh admin@192.168.1.10 cp -r /opt/plcnext/projects/PCWE/Plc/Gds/ /opt/plcnext/projects/IOconf/GDS/
   ```
   - Open the file ```/opt/plcnext/projects/IOconf/GDS/PCWE.gds.config``` via tool "vi" or "WinSCP" and add the connector ```<Connector startPort="Arp.Io.AxlC/1.~DI8" endPort="Arp.Io.AxlC/1.~DO8" />``` into connectors list e.g.:
   
   ```text
     <Connectors>
      <Connector startPort="Arp.Io.AxlC/1.~DI8" endPort="Arp.Io.AxlC/1.~DO8" />
      .
      .
    </Connectors>
   ```
- Update the current symbolic link to point to your own project:

  ```text
  ln -sfn IOconf current
  ```
   Now, all of the configuration files in the PCWE directory will be ignored. If you want to use any of these files in your own project, copy the relevant files (including the complete directory structure under PCWE) into ```IOconf``` project directory.
   
- Some configurations that you might want to use include:
   ```text
   •	Profinet Controller configuration, in the PCWE/Io/Arp.Io.PnC directory.
   •	Ethernet/IP configuration, in the PCWE/Io/Arp.Io.EthernetIP directory.
   •	Interbus configuration, in the PCWE/Io/Arp.Io.IbM directory.
   ```
   
   If your project includes real-time C++ programs:
   ```text
   •	PLM configuration, in the PCWE/Plc/Plm directory.
   •	ESM configuration, in the PCWE/Plc/Esm directory 
   ```
   
   - All the configuration files in folder ```IOconf``` will be automatically picked up by the PLCnext runtime via the current symlink. There is no need to add or edit any files in the Default project directory. The entire Default project should all be left in its factory-default state.
   
   - You can now delete the PCWE directory, if you want.
   
   - To test the axioline bus configuration and the DI/DO process data connector ```<Connector startPort="Arp.Io.AxlC/1.~DI8" endPort="Arp.Io.AxlC/1.~DO8" />```, you can use the Interface module UM 45-IB-DI/SIM8 (by switch On/Off of DI-Signals, the DO-Signals will be set/unset).

 ## Installing a development environment
 
You can use a Microsoft Visual Studio 2019 development environment to develop your own applications. Development is supported only on Microsoft Windows OS.

 ### Prerequisite

1. Microsoft Visual Studio 2019.
2. .NET Framework 4.8 or higher.
3. NuGet package "PLCnIOconf-net48.2021.3.10.nupkg" (included in this repository).
4. CommandLineParser 2.6.0 or higher.
5. Microsoft.Bcl.AsyncInterfaces 5.0.0 or higher.

### Installation and Building

1. Open "IOconf_Examples/src/axioconf/axioconf.sln" solution in MS VS2019.
2. Verify/Install the .NET Framework 4.8 or higher
3. In Solution Explorer, right-click ```References``` and choose ```Manage NuGet``` Packages.
4. Add a Package source via ```+``` Button and select the directory contains the data ```PLCnIOconf-net48.2021.3.10.nupkg```. As Package name set e.g. "IOConf" and accept the settings via ```OK``` Button.
5. Accept any license prompts.
6. If prompted to review changes, select OK.
7. Select the ```Tools > NuGet Package Manager > Package Manager Console``` menu command.
8. If the console opens, install CommandLineParser 2.6.0 or higher via command ```Install-Package CommandLineParser -Version 2.6.0```.
9. Install Microsoft.Bcl.AsyncInterfaces 5.0.0 or higher via command ```Install-Package Microsoft.Bcl.AsyncInterfaces -Version 5.0.0-rc.1.20451.14```.
10. In Solution Explorer, right-click ```References``` and choose ```Manage NuGet``` Packages.
11. Select the Browse tab, search for ```PLCnIOconf-net48```, select that package in the list and install.
12. Save the ```axioconf``` solution.
13. Restart Microsoft Visual Studio 2019 and reopen ```axioconf``` solution.
14. Recompile the ```axioconf``` solution.

### Configuration settings

The compilation process overrides the configuration settings. It may be necessary to edit the configuration to disable the task for current consumption check. This is only required if the current consumption exceeds 2000mA.

1. Open the file ```IOconf_Examples\src\axioconf\bin\Release\Profiles\1.0.0\Build.xml``` and entcomment the configuration line 64: 

```<!-- TaskRef name="MaxCurrentConsumption"></TaskRef -->```

2. Open the file ```IOconf_Examples\src\axioconf\bin\Release\Profiles\1.0.0\Protocols\Agnostic\Build.Agnostic.xml``` and entcomment the configuration lines 42-61: 

```text
    <!-- Task id="MaxCurrentConsumption" fullName="Ade.Network.Protocols.Agnostic.AgnosticMaxCurrentConsumptionCheckTask">
      <RequiresScope>
        <ScopeRef id="BuildEngineScope">
          <BindTo>
            <ProtocolBinding protocol="Agnostic" layer="Layer7">
            </ProtocolBinding>
          </BindTo>
        </ScopeRef>
      </RequiresScope>
      <Condition>
        <And>
          <Match Property="Type">//Device[exists(./Layer7[@type='AXLF:Master'])]</Match>
        </And>
      </Condition>
      <Parameter id="Protocol" value="AXLF"></Parameter>
      <Parameter id="MaxValue" value="PCEP:LogicCurrent"></Parameter>
      <Parameter id="ConsumedValue" value="AXLF:LogicCurrent"></Parameter>
      <Parameter id="MessageId" value="AXIO0004"></Parameter>
      <Parameter id="ResourceId" value="AXIO0004MaxCurrentConsumption"></Parameter>
    </Task -->
```    

## Known issues

PLCnIOconf Interface numbers the NodeID's of the Axioline devices in the TIC file from number ```1```. PLCnext Engineer starts from number ```0```.

When the Axioline bus configuration generated in PLCnext Enginner will be replaced by the Aioline bus configuration generated in PLCnIOconf, errors can occur in linked process data with GDS ports. These errors can be corrected manually by adjusting the configuration file ```/opt/plcnext/projects/PCWE/Plc/Gds/PCWE.gds.config```

### Example Scenario

The Axioline device ```AXL F DI8/1 DO8/1 1H``` is configured in PLCnext Engineer and the process data ```~DI8``` and ```~DO8``` are connected. The connector is defined in the file "/opt/plcnext/projects/PCWE/Plc/Gds/PCWE.gds.config" as follows:
```text
<Connector startPort="Arp.Io.AxlC/0.~DI8" endPort="Arp.Io.AxlC/0.~DO8" />
```

The number ```/0.``` is the NodeID of the first Axioline device configured in PLCnext Engineer.

PLCnIOconf starts numbering from NodeID = 1 and by replacing of TIC files an error will be generated because the NodeID = 0 isn't persists. The error can be corrected by adjusting the NodeID as follows:

```text
<Connector startPort="Arp.Io.AxlC/1.~DI8" endPort="Arp.Io.AxlC/1.~DO8" />
```

## PLCnIOconf API documentation

Extract the file [PLCnIOconf-doc](https://github.com/PLCnext/IOconf_Examples/tree/main/doc/) and find in the modules, namespaces, classes and files for C# programming.

## Troubleshooting

If the axioline bus configuration appears not to be working, check the contents of the file `/opt/plcnext/logs/Output.log` for error messages.

## How to get support

This solution is supported in the forum of the [PLCnext Community](https://www.plcnext-community.net/index.php?option=com_easydiscuss&view=categories&Itemid=221&lang=en).
Please raise an issue with a detailed error description and always provide a copy of the Output.log file.

## Design notes

This solution is designed to demonstate how it's possible to implement the application for axioline bus configuration by using the API "PLCnext IOconf Interface".

-----------

Copyright © 2019-2020 Phoenix Contact Electronics GmbH

All rights reserved. This program and the accompanying materials are made available under the terms of the [MIT License](http://opensource.org/licenses/MIT) which accompanies this distribution.


