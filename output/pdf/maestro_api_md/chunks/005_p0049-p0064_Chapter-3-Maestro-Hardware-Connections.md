# Chapter 3 Maestro Hardware Connections

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 49-64
- Chunk: `005_p0049-p0064_Chapter-3-Maestro-Hardware-Connections.md`

## Active Outline At Chunk Start
- p. 49 - Chapter 3 Maestro Hardware Connections
  - p. 49 - 3.1 Maestro USB Connection

## Contained Bookmark Outline
- p. 49 - Chapter 3 Maestro Hardware Connections
  - p. 49 - 3.1 Maestro USB Connection
    - p. 50 - 3.1.1 Connection Procedure
    - p. 51 - 3.1.2 ipaddr - Maestro IP Address
    - p. 52 - 3.1.3 ipmask - Maestro IP Subnet Mask
    - p. 53 - 3.1.4 defgateway - Maestro Default Gateway
  - p. 53 - 3.2 Maestro Network Configuration
    - p. 54 - 3.2.1 Windows 7 and 10 Setup
  - p. 63 - 3.3 Maestro Master -Slave Hot Plug

## Extracted Text

### PDF page 49
<a id="pdf-page-49"></a>
#### Chapter 3 Maestro Hardware Connections
##### 3.1 Maestro USB Connection
Chapter 3 Maestro Hardware Connections
3.1 Maestro USB Connection
This section describes how to communicate with the Maestro when connected via a USB connection from a
host system to allow the Maestro ports to be configured and operate via the LAN. This connection imitates a
COM port connection at a COM Port. Therefore, if an RS232 Terminal connection is opened at the host system,
udev execution opens a terminal from which it is possible to perform the following basic operations.
- Change / Read IP Address.
- Change / Read Gateway.
- Change / Read Subnet mask.
- Change / Read Server IP.
- Change / Read download version path.

### PDF page 50
<a id="pdf-page-50"></a>
###### 3.1.1 Connection Procedure
3.1.1 Connection Procedure
The following procedure describes how to connect the host system to the Maestro via USB connection and
configure the communication parameters of the Maestro.
1. Make sure that the Maestro is powered on.
2. Connect the USB connection from the host system to the Maestro. The Maestro should emit a sound
signifying that a connection is made.
3. Open the Device Manager and locate the Ports section in the hierarchal structure. Verify which COM
port is defined for the Elmo Maestro.
4. At the host computer, open a communications Terminal to a COM port.
The prompt should display GMAS>.

5. 1. At the prompt, enter any command detailed in sections 3.1.2 - 3.1.4 to perform the
appropriate operation at the Maestro.
For example; To request the IP address enter ipaddr.
The Maestro IP address is returned.
Note: By default, the Maestro IP address is set to 192.168.1.3. However, the customer may prefer to
integrate the Maestro with his network system and therefore may wish to change the default value. Use this
procedure to perform this action.

### PDF page 51
<a id="pdf-page-51"></a>
###### 3.1.2 ipaddr - Maestro IP Address
3.1.2 ipaddr - Maestro IP Address
Purpose 1. Set a new IP address.
2. Request display of Maestro's IP address.
Syntax ipaddr
Parameters None or string
Attributes Type Source Default values Range
Parameter, string Interpreter N/A N/A
Examples
Input Output
ipaddr 10.10.10.1
Ipaddr 10.10.20.2 OK

### PDF page 52
<a id="pdf-page-52"></a>
###### 3.1.3 ipmask - Maestro IP Subnet Mask
3.1.3 ipmask - Maestro IP Subnet Mask
Purpose 1. Set a new IP subnet mask.
2. Request display of Maestro's IP subnet mask.
Syntax ipmask
Parameters None or string
Attributes Type Source Default values Range
Parameter, string Interpreter N/A N/A
Examples
Input Output
ipmask 255.255.255.0
ipmask 255.255.255.0 OK

### PDF page 53
<a id="pdf-page-53"></a>
###### 3.1.4 defgateway - Maestro Default Gateway
##### 3.2 Maestro Network Configuration
3.1.4 defgateway - Maestro Default Gateway
Purpose
1. Set a new default gateway.
2. Request display of Maestro's default gateway.
Syntax defgateway
Parameters None or string
Attributes Type Source Default values Range
Parameter, string Interpreter N/A N/A
Examples
Input Output
defgateway 10.10.10.1
Defgateway 10.10.10.2 OK
3.2 Maestro Network Configuration
This section introduces the procedure to connect between the Maestro and the PC for Windows 7 and
Windows 10 operating systems. The default Maestro IP settings are:
Setting IP Address
IP address 192.168.1.3
Subnet mask 255.255.255.0
Default Gateway 192.168.1.1

### PDF page 54
<a id="pdf-page-54"></a>
###### 3.2.1 Windows 7 and 10 Setup
3.2.1 Windows 7 and 10 Setup
To set the PC configuration in Windows 7 and Windows 10
6. Connect the USB connection from the Host system to the Maestro.
7. Perform the USB Connection procedure as described in the section above 3.1.1.
8. From the terminal window, check the IP Address, Default Gateway, and Subnet Mask, of the
Maestro.

9. Open the Network Connection window, and locate the Local Area Connection to the Maestro.

10. Right-click on the Connection, and select Status. Make sure that the IPv4 and IPv6 Connectivity show
No network access.

### PDF page 55
<a id="pdf-page-55"></a>
11. Click Properties, and select Internet Protocol Version 4 (TCP/IPv4).

12. Select Properties and enter the Default Gateway, and Subnet Mask obtained from the Telnet
window.

### PDF page 56
<a id="pdf-page-56"></a>
13. In the Internet Protocol Version 4 (TCP/IPv4 Properties window, Insert an IP Address similar to the
Maestro own address but different at the fourth set of digits, as shown e.g. 192.168.1.2, and then click
OK.
14. Check the connection to the Maestro by pinging it. Enter the following at the Windows prompt:
ping -t <Maestro IP Address> e.g.192.168.1.3

15. A Command Prompt should open with the reply results demonstrating a connection to the Maestro.

### PDF page 57
<a id="pdf-page-57"></a>
16. Open the EAS Application at the System Configuration window and right -click on the Workspace to
setup a new Maestro.

### PDF page 58
<a id="pdf-page-58"></a>
To perform a quick EtherCAT configuration:
1. Right-click the EtherCAT Maestro device in the workspace tree and select New EtherCAT
Configuration from the drop-down menu.
If you only want to edit the EtherCAT configuration that was already created, select Edit EtherCAT
Configuration from the drop-down menu.

Or, alternatively,
In the System Configuration activity, go to the System Configuration ribbon and click the
New
EtherCAT Configuration button to create a new EtherCAT configuration or click the
Edit EtherCAT
Configuration button to edit an EtherCAT configuration that was already created.
If you go into the EtherCAT Configuration tool using the Edit button, you must click the
Start and
Download button in order to create a new configuration for this Maestro.
Note: When editing an EtherCAT configuration, and EASII detects an inconsistency between the
Workspace Resource file and the Resource file located on the Maestro, the Maestro Resource window is
displayed. For details, see the EASII documentation.
EASII compares the number, type, and revision of devices in your EtherCAT network to those in the current
Maestro configuration. If a mismatch is found, the following dialog box appears displaying the mismatched
revision details:

### PDF page 59
<a id="pdf-page-59"></a>
- Click Keep Current Configuration to keep the current configuration
- Click Accept Scan Results to change the configuration.
Note: After clicking the New EtherCAT Configuration button EASII creates a new EtherCAT configuration
for this Maestro without asking the user. Hence, the Compare Configuration dialog does not appear and
the current configuration is overridden.
The EtherCAT Configuration window appears prompting you if you want to continue using the displayed
EoE IP address range.

2. Click Yes.
The EtherCAT Configuration window is displayed.

### PDF page 60
<a id="pdf-page-60"></a>
3. Select a Maestro EtherCAT slave.
4. Click the FMMU/SM tab.

5. Click the output name's corresponding checkbox in the left pane of the F MMU/SM tab to add it to the
Variables Output list in the right pane of the FMMU/SM tab.
6. Click the input name's corresponding checkbox in the left pane of the FMMU/SM tab to add it to the
Variables Input list in the right pane of the FMMU/SM tab.
7. Click the Add Template button in the lower part of the Variables pane.
The Add Template dialog box is displayed.

### PDF page 61
<a id="pdf-page-61"></a>
8. Enter the template name and then click Save to save the current configuration settings as a template
file (*.tmpx format).).
9. Click the Master (EtherCAT Maestro) device in the workspace tree.
10. Click the Quick Settings tab.

11. In the Quick Settings tab, select the slave(s) to implement the new configuration by clicking the
corresponding checkbox(es).
12. Select the template you want to add to the selected slave(s) by clicking the template name's
corresponding checkbox in the Templates pane at the right. The Apply Templates to selected Axes
button becomes active.
13. Click the Apply Templates to selected Axes button.
14. Click the
Start and Download button from the Master group in the EtherCAT Configuration
ribbon.
The new EtherCAT configuration is downloaded to the device(s).

### PDF page 62
<a id="pdf-page-62"></a>
15. In the EtherCAT Configuration ribbon, click the
To System Configuration button from the Main
group to go back to the system configuration.
Note: If you choose to go back to the configuration system without downloading the new EtherCAT
configuration, the following message appears:

- Click Download to download the configuration and exit the EtherCAT Configuration tool.
- Click Discard to exit the EtherCAT Configuration tool without downloading the new configuration.
- Click Cancel to go back to the EtherCAT configuration tool.

### PDF page 63
<a id="pdf-page-63"></a>
##### 3.3 Maestro Master -Slave Hot Plug
3.3 Maestro Master -Slave Hot Plug
Previously when a slave is disconnected from the EtherCAT network, the Maestro performed a scan on the
entire EtherCAT network to determine the topology. The active slaves which have not disconnected are
scanned and their EEPROMs read. In the EASII EtherCAT Diagnostics window, when a Slave is disconnected
from the Master, the Maestro EtherCAT master enters a mode called Wrong configuration and displays Default
input values.

Wrong configuration Physical configuration does not match the configuration created in the
EtherCAT configurator.
Default input values All inputs values are displayed as 0
When the configuration is "wrong" Maestro constantly checks the identity of the devices connected to the
EtherCAT network by reading the EtherCAT slaves EEPROMs containing the following information:
- Product code
- Vendor ID
- Serial number
- Revision number
The Maestro then compared this stored data in the EEPROM against the configur ation stage data. The reading
of the EEPROM content was performed for all slaves connected to the Maestro, and was performed
periodically until the configuration is no longer "wrong" i.e. the disconnected slave is reconnected.
This scan is redundant since slaves which have not disconnected were already identified and another
identification is unnecessary. The Hot Plug mechanism identifies only the reconnected slaves and thus spares
the redundant EEPROM reads.
If a slave was already validated (already connected and operative) as a configuration slave when the Maestro
was initialized, it is not necessary to read its EEPROM even in "Wrong configuration" mode. It is only necessary
to read a slave EEPROM when it connects to the network in order to validate that i t is the desired slave.
The Wrong configuration mode occurs when the number of slaves is smaller than the number of slaves in the
xml (slaves disconnected):

When in Wrong configuration mode, instead of reading all the EEPROMS and validating, the Maestro looks for
the disconnection point and waits for a connection at this point, and checks the slave by reading its EEPROM:

### PDF page 64
<a id="pdf-page-64"></a>
Maestro verified the slave's identity and adds it to the valid slaves. If further slaves are connected to this slave,
then they are also checked.

If a wrong slave or slaves are connected, the Maestro will continue scanning, and will only stop scanning the
network when a valid configuration is located.
