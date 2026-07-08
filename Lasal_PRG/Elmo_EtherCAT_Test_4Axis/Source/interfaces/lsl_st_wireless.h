#ifndef  __WIRELESS_H
#pragma once
#define  __WIRELESS_H

// Interface
#define INTERFACE_WIFI                       "LSL_WIRELESS"


#define LSL_WIRELESS_TYPE_VERSION    0x00010006

#define LSL_WIRELESS_ERR_IO			-5		// IO Error (executing request failed)
#define LSL_WIRELESS_ERR_NO_DATA	-13		// No data available (scan)
#define LSL_WIRELESS_ERR_BUSY		    -16		// Device is currently busy
#define LSL_WIRELESS_ERR_NO_DEVICE	-19		// Wireless Device not available
#define LSL_WIRELESS_ERR_PARAM		 	-22		// Invalid argument for function
#define LSL_WIRELESS_ERR_UNSUPPORTED_CMD  -1001 // Unknown command at OS_WIRELESSAP_{GETCONFIG|SETCONFIG}[_MULTI]

#define LSL_WIRELESS_FLG_BG_SCAN	0x00
#define LSL_WIRELESS_FLG_FG_SCAN	0x01
#define LSL_WIRELESS_FLG_NET_ONLY	0x02

// Flags for Security configuration of a Wireless Network
#define LSL_WIRELESS_NET_KEYMGM_NO				0x0000	// No Key(management)
#define LSL_WIRELESS_NET_KEYMGM_WPAPSK		0x0001	// WPA/WPA2 pre-shared key
#define LSL_WIRELESS_NET_KEYMGM_WPAEAP		0x0002	// WPA/WPA2 with EAP authentication
#define LSL_WIRELESS_NET_PROTO_WPA				0x0004	// WPA as protocol
#define LSL_WIRELESS_NET_PROTO_WPA2				0x0008	// RSN(WPA2) as protocol

#define LSL_WIRELESS_NET_CIPH_WPA_GRP_CCMP		0x0010	// CCMP(AES) as group Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA_GRP_TKIP		0x0020	// TKIP as group Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA_PAIR_CCMP		0x0040	// CCMP(AES) as pairwise Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA_PAIR_TKIP		0x0080	// TKIP as pairwise Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA_ALL			0x00F0	// All Ciphers supported

#define LSL_WIRELESS_NET_CIPH_WPA2_GRP_CCMP		0x0100	// CCMP(AES) as group Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA2_GRP_TKIP		0x0200	// TKIP as group Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA2_PAIR_CCMP	0x0400	// CCMP(AES) as pairwise Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA2_PAIR_TKIP	0x0800	// TKIP as pairwise Cipher for WPA Network
#define LSL_WIRELESS_NET_CIPH_WPA2_ALL			0x0F00	// All Ciphers supported

#define LSL_WIRELESS_NET_ENABLED				0x0001	// Wireless Net is enabled
#define LSL_WIRELESS_NET_CURRENT				0x0002	// Wireless Net is the current one
#define LSL_WIRELESS_NET_SCANSSID				0x0004	// scan with SSID-specific Probe Request frames


//Flags for Change Network method (since version 0x00010007)

// Old implementations set the BSSID incorrectly. This has been corrected in the OS 
// However, to remain compatible, the BSSID is only set if this is explicitly requested
// requested (with the LSL_WIRELESS_FLG_USE_BSSID flag).
#define LSL_WIRELESS_FLG_USE_BSSID     0x01

// This flag is used if you want to change the BSSID only.
#define LSL_WIRELESS_FLG_ONLY_BSSID    0x02

// This flag is used if you don't want a reassociate after calling the network change method
#define LSL_WIRELESS_FLG_NO_REASSOC    0x04

//Defines for Debug IDs (since version 0x00010006)
#define LSL_WIRELESS_DEBUG_ID_RING_COUNT  0x00

#define MAX_APS	100			  // Max Number of APs that can be stored inside the Arrays
#define MAX_NETWORKS 	32	// Max Number of WIFI Networks that are handled by the OS
#define MAX_CLIENTS_PER_AP  20  //Max Number of clients that can be connected to an AP

// Commands to read/write the configuration file
// string value
#define LSL_WIRELESSAP_CMD_SSID				0x0001
// string value
//	country code (e.g. AT, US)
#define LSL_WIRELESSAP_CMD_COUNTRY_CODE		0x0002
// integer value:
//	bytes 0,1: channel number
//	bytes 2,3: reserved, must be set to 0
#define LSL_WIRELESSAP_CMD_CHANNEL			0x0003
// integer value
#define LSL_WIRELESSAP_CMD_MAX_STATIONS		0x0004
// integer value:
//	0..visible
//	1..invisible
#define LSL_WIRELESSAP_CMD_SSID_INVISIBLE	0x0005
// if string is empty:
// 	integer value:
//		0..disabled
//		1..WPA
//		2..WPA2
//		3..WPA and WPA2
// else:
// 	string value:
//		name of protocol
#define LSL_WIRELESSAP_CMD_WPA				0x0006
// string value
#define LSL_WIRELESSAP_CMD_WPA_PASSPHRASE	0x0007
// string value
#define LSL_WIRELESSAP_CMD_WPA_KEY_MGMT		0x0008
// string value
#define LSL_WIRELESSAP_CMD_WPA_PAIRWISE		0x0009
// string value
#define LSL_WIRELESSAP_CMD_HW_MODE			0x000A
// integer value:
//	>=0: Bandwidth in MHz, -1: comment out entry in config-file
#define LSL_WIRELESSAP_CMD_BANDWIDTH		0x000D

TYPE
  wifi_roam_mode : (
    ROAM_RSSI  := 1,
    ROAM_HOST_BIAS,
    ROAM_LOCK_SSID
  ) $UDINT;
END_TYPE

TYPE
  wifi_network_control : (
	NWCTRL_DISABLE,			// disable a network
	NWCTRL_ENABLE,			// enable a network
	NWCTRL_SELECT,			// select a network (disable others)
	NWCTRL_DELETE,			// remove a network
	NWCTRL_SAVECFG,			// save the current configuration
	NWCTRL_RECONFIG, 		// force wpa_supplicant to re-read its configuration file
	NWCTRL_REASSOCIATE	// force the client to reassociate to a network
  ) $UDINT;
END_TYPE

TYPE
 #pragma pack (push, 1)
	LSL_WIRELESS_AP_INFO :	STRUCT
		frequency : UDINT;					// frequency in kHz
		signal    : DINT;					// Signal quality in dBm
		encConfig : UDINT;					// Encryption supported by the AP
		flags     : UDINT;
		bssid     : ARRAY [0..5] OF HSINT;	// MAC address of the AP
		bias      : INT;                    // BIAS value of the AP
		ssid      : ARRAY [0..35] OF CHAR;	// SSID has a max length of 32 characters, to avoid troubles with 0 termination, I use 36 Bytes
	END_STRUCT;
 #pragma pack (pop)
END_TYPE

TYPE
 #pragma pack (push, 1)
	LSL_WIRELESS_CONN_INFO : STRUCT
		apInfo		: LSL_WIRELESS_AP_INFO;
	  linkQuality : UDINT;    // Quality of the Link in %
	  bitrate     : UDINT;    // Bitrate of the Connection
	  txpower     : UDINT;    // TX-power in dBm
	  roamMode    : wifi_roam_mode;	  // Current Roaming mode
    wpastate    : ARRAY [0..35] OF CHAR;  //Possible values are DISCONNECTED, INTERFACE_DISABLED, INACTIVE, SCANNING, AUTHENTICATING, ASSOCIATING, ASSOCIATED, 4WAY_HANDSHAKE, GROUP_HANDSHAKE, COMPLETED
	END_STRUCT;
 #pragma pack (pop)
END_TYPE

TYPE
 #pragma pack (push, 1)
	LSL_WIRELESS_SCAN_INFO : STRUCT
		timestamp	 : UDINT;		// Timestamp of the Results
		scanActive : UDINT;		// Scan is currently active
		cnt				 : UDINT;		// Nr of APs found during the Scan
		aps				 : ARRAY [0..MAX_APS-1] OF LSL_WIRELESS_AP_INFO;		// Information about the APs
	END_STRUCT;
 #pragma pack (pop)
END_TYPE

TYPE
  #pragma pack (push, 1)
	LSL_WIRELESS_NET_INFO : STRUCT
		netID			: UINT;		// Network ID or -1 if Invalid
		status		: UINT;		// Either enabled/disabled/current
		ssid      : ARRAY [0..35] OF CHAR;	// SSID has a max length of 32 characters, to avoid troubles with 0 termination, I use 36 Bytes
		bssid     : ARRAY [0..5] OF CHAR;
		priority	: UINT;		// Priority of the Network
		encConfig	: UDINT;	// Encoding/Authentication Config
		psk				: ARRAY [0..255] OF CHAR;
		flags	    : UDINT;
	END_STRUCT;
  #pragma pack (pop)
END_TYPE


TYPE
  #pragma pack (push, 1)
	LSL_WIRELESS_AP_CLIENT_INFO : STRUCT    aMACAddr : ARRAY [0..5] OF HSINT;    //MAC Address of client
	END_STRUCT;
  #pragma pack (pop)
END_TYPE

TYPE
  #pragma pack (push, 1)
	LSL_WIRELESS_AP_CONNECTED_CLIENTS : STRUCT
    udNumberMaxConnectedClients : UDINT;  //Number of maximum connected clients for all interfaces of the access point
    udNumberConnectedClients : UDINT;   //Number of connected clients for the selected interface of the access point
    aClientMACAddresses : ARRAY [0..MAX_CLIENTS_PER_AP-1] OF LSL_WIRELESS_AP_CLIENT_INFO; //Save place for 20 MAC-adresses
	END_STRUCT;
  #pragma pack (pop)
END_TYPE

TYPE
  #pragma pack (push, 1)
  LSL_WIRELESS_TYPE : STRUCT
    version	        	: UDINT;
    WIFI_GetConnInfo  	: pVoid;
    WIFI_TriggerFGScan  : pVoid;
    WIFI_SetBGScan		  : pVoid;
    WIFI_GetScanResult	: pVoid;
    WIFI_SetRoamMode    : pVoid;
    WIFI_TriggerRoam    : pVoid;
    WIFI_SetLRSSIroam_Params  : pVoid;
    WIFI_SetBIASroam_Params   : pVoid;
    WIFI_NetworksGet    : pVoid;
    WIFI_NetworkAdd     : pVoid;
    WIFI_NetworkChange  : pVoid;
    WIFI_NetworkControl : pVoid;
    WIFIAP_Start        : pVoid;
    WIFIAP_Stop         : pVoid;
    WIFIAP_Restart      : pVoid;
    WIFIAP_Installed	: pVoid;
    WIFIAP_IsRunning	: pVoid;
    WIFIAP_GetConfig    : pVoid;
    WIFIAP_SetConfig    : pVoid;
    WIFIAP_Start_Multi  : pVoid;
    WIFIAP_Stop_Multi   : pVoid;
    WIFIAP_Restart_Multi	: pVoid;
    WIFIAP_Installed_Multi	: pVoid;
    WIFIAP_IsRunning_Multi	: pVoid;
    WIFIAP_GetConfig_Multi	: pVoid;
    WIFIAP_SetConfig_Multi	: pVoid;
    WIFI_SetRadio   : pVoid;
    WIFI_GetRadio   : pVoid;
    WIFI_SetTxPower : pVoid;
    WIFI_GetTxPower : pVoid;
    // since version 0x00010006
    WIFI_ClientStart: pVoid;
    WIFI_ClientStop : pVoid;
    WIFI_ClientIsRunning : pVoid;
    // since version 0x00010007
    WIFIAP_GetConnectedClients: pVoid;
    WIFIAP_ActivateDebugValues: pVoid;
    WIFIAP_GetDebugValues: pVoid;
    // since version 0x00010008
    WIFI_WpaCtrlEventsOnoff: pVoid;
    WIFI_WpaCtrlEventsGet: pVoid;
  END_STRUCT;
  #pragma pack (pop)
END_TYPE


FUNCTION __CDECL GLOBAL P_WIFI_GetConnInfo
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    pBuf        : ^LSL_WIRELESS_CONN_INFO;
    len         : UDINT;
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_TriggerFGScan
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    frequency	  : UDINT;
    ssid		    : ^CHAR;
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_SetBGScan
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    period      : UDINT;
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_GetScanResult
  VAR_INPUT
    interface : UDINT;
    flags     : UDINT;
    pBuf		  : ^LSL_WIRELESS_SCAN_INFO;
    len			  : UDINT;
  END_VAR
  VAR_OUTPUT
    retcode   : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_SetRoamMode
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    roam_mode   : wifi_roam_mode;
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_TriggerRoam
  VAR_INPUT
    interface : UDINT;
    flags     : UDINT;
    bssid   	: ^CHAR;
  END_VAR
  VAR_OUTPUT
    retcode   : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_SetLRSSIroam_Params
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    scan_period     : INT;		// default: 5000ms
    scan_threshold  : INT;		// default: -74dBm
    roam_threshold  : INT;		// default: -76dBm
    roam_floor      : INT;		// default: -56dBm
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_SetBIASroam_Params
  VAR_INPUT
    interface   : UDINT;
    flags       : UDINT;
    bias        : DINT;
    bssid       : ^CHAR;
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

// Since Version 0x00010001
FUNCTION __CDECL GLOBAL P_WIFI_NetworksGet
	VAR_INPUT
	  interface   : UDINT;
	  flags       : UDINT;
	  pBuf        : ^LSL_WIRELESS_NET_INFO;
	  len         : UDINT;
	END_VAR
	VAR_OUTPUT
	  retcode     : DINT;
	END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_NetworkAdd
  VAR_INPUT
	  interface   : UDINT;
	  flags       : UDINT;
	  pConf       : ^LSL_WIRELESS_NET_INFO;
  END_VAR
  VAR_OUTPUT
	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_NetworkChange
  VAR_INPUT
    interface   : UDINT;
	  flags       : UDINT;
    pConf       : ^LSL_WIRELESS_NET_INFO;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_NetworkControl
  VAR_INPUT
    interface   : UDINT;
  	flags       : UDINT;
  	netID       : UDINT;
  	control     : wifi_network_control;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

// Since Version 0x00010002
FUNCTION __CDECL GLOBAL P_WIFIAP_Start
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;
  
FUNCTION __CDECL GLOBAL P_WIFIAP_Stop
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_Restart
 VAR_OUTPUT
 	retcode     : DINT;
 END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_Installed
  VAR_OUTPUT
   	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_IsRunning
 VAR_OUTPUT
   	retcode     : DINT;
 END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_GetConfig
  VAR_INPUT
    cmd       	: UDINT;	// see define LSL_WIRELESSAP_CMD_...
    stringVal 	: ^CHAR;
    intVal		: ^DINT;
    strLen		: UDINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_SetConfig
  VAR_INPUT
    cmd       	: UDINT;	// see define LSL_WIRELESSAP_CMD_...
    stringVal 	: ^CHAR;
    intVal		: DINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

// Since version 0x00010003
FUNCTION __CDECL GLOBAL P_WIFIAP_Start_Multi
  VAR_INPUT
    interface	: UDINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;
  
FUNCTION __CDECL GLOBAL P_WIFIAP_Stop_Multi
  VAR_INPUT
    interface	: UDINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_Restart_Multi
  VAR_INPUT
    interface	: UDINT;
  END_VAR
 VAR_OUTPUT
 	retcode     : DINT;
 END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_Installed_Multi
  VAR_INPUT
    interface	: UDINT;
  END_VAR
  VAR_OUTPUT
   	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_IsRunning_Multi
  VAR_INPUT
    interface	: UDINT;
  END_VAR
 VAR_OUTPUT
   	retcode     : DINT;
 END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_GetConfig_Multi
  VAR_INPUT
    interface	: UDINT;	// interfacenumber (10=wlan0)
    cmd       	: UDINT;	// see define LSL_WIRELESSAP_CMD_...
    strval     	: ^CHAR;
    intval 	: ^DINT;
    strlen	: UDINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFIAP_SetConfig_Multi
  VAR_INPUT
    interface	: UDINT;	// interfacenumber (10=wlan0)
    cmd       	: UDINT;	// see define LSL_WIRELESSAP_CMD_...
    strval     	: ^CHAR;
    intval 	: DINT;
  END_VAR
  VAR_OUTPUT
  	retcode     : DINT;
  END_VAR;

// Since version 0x00010004
(* Switch radio on or off *)
FUNCTION __CDECL GLOBAL P_WIFI_SetRadio
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
    onOff       : DINT;		// >0:radio-on, 0:radio-off, -1:comment out entry in config-file
    flags       : DINT;		// reserved for future use, must be set to 0
    save		: DINT;		// <>0: save remanent in config file, 0..do not save in config file
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

(* Get saved radio on/off value *)
FUNCTION __CDECL GLOBAL P_WIFI_GetRadio
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
    onOff       : ^DINT;	// >0:radio-on, 0:radio-off
    flags       : DINT;		// reserved for future use, must be set to 0
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

(* Set tx power *)
FUNCTION __CDECL GLOBAL P_WIFI_SetTxPower
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
    txPower     : DINT;		// tx power in dBm, -1:comment out entry in config-file
    flags       : DINT;		// reserved for future use, must be set to 0
    save		: DINT;		// <>0: save remanent in config file, 0..do not save in config file
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

(* Set saved tx power *)
FUNCTION __CDECL GLOBAL P_WIFI_GetTxPower
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
    txPower     : ^DINT;	// tx power in dBm
    flags       : DINT;		// reserved for future use, must be set to 0
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;
  
// Since version 0x00010006  
FUNCTION __CDECL GLOBAL P_WIFI_ClientStart
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_ClientStop
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_ClientIsRunning
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) or 0xFFFFffff for all interfaces
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;
  END_VAR;

// Since version 0x00010007  
FUNCTION __CDECL GLOBAL P_WIFIAP_GetConnectedClients
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) 
    flags       : DINT;		// reserved for future use, must be set to 0
    udBufSize   : UDINT;  // size of pbuffer memory
    pBuffer     : ^VOID;  // pointer to buffer where data will be copied (type LSL_WIRELESS_AP_CONNECTED_CLIENTS)
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

// Since version 0x00010007  
FUNCTION __CDECL GLOBAL P_WIFIAP_ActivateDebugValues
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) 
    flags       : DINT;		// Bit 0        0=aktivieren, 1=deaktivieren
    pBuffer     : ^VOID;  // pointer to buffer which debug values should be activated (4Byte number of IDs to activate, ID0,ID1,ID2,ID3....)
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

// Since version 0x00010007  
FUNCTION __CDECL GLOBAL P_WIFIAP_GetDebugValues
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) 
    flags       : DINT;		// reserved for future use, must be set to 0
    udBufSize   : UDINT;  // size of pbuffer memory
    pBuffer     : ^VOID;  // pointer to buffer where the debug values get copied from OS (4 Byte amount of data (how many IDs´), then 4 Byte Typ of ID0, then 4 Byte value of ID0, then 4 Byte Typ of ID1 and so on
    END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

// Since version 0x00010008
FUNCTION __CDECL GLOBAL P_WIFI_WPA_CTRL_EVENTS_ONOFF
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) 
    flags       : DINT;		// reserved for future use, must be set to 0
    onOff       : DINT;		// 1..activate receiving control events, 0..deactivate
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 0 if o.k., else a neg. errorcode
  END_VAR;

FUNCTION __CDECL GLOBAL P_WIFI_WPA_CTRL_EVENTS_GET
  VAR_INPUT
    interface   : UDINT;	// interfacenumber (10=wlan0) 
    flags       : DINT;		// reserved for future use, must be set to 0
    eventBuf    : ^CHAR;  // filled with 0-terminated string with event message (maybe truncated when bufSize is too small)
    bufSize     : UDINT;  // size of eventBuf
  END_VAR
  VAR_OUTPUT
    retcode     : DINT;		// 1..event received, 0 no event available, <0 neg. errorcode
  END_VAR;


// Since Version 0x00010002
#define OS_WIRELESS_CHECK_VERSION(pWIFI)				pWIFI^.version <> LSL_WIRELESS_TYPE_VERSION
#define OS_WIRELESS_GET_CONN_INFO(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_GetConnInfo $ P_WIFI_GetConnInfo(p1,p2,p3,p4)
#define OS_WIRELESS_TRIGGER_FG_SCAN(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_TriggerFGScan $ P_WIFI_TriggerFGScan(p1,p2,p3,p4)
#define OS_WIRELESS_SET_BG_SCAN(pWIFI,p1,p2,p3)				pWIFI^.WIFI_SetBGScan $ P_WIFI_SetBGScan(p1,p2,p3)
#define OS_WIRELESS_GET_SCAN_RESULT(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_GetScanResult $ P_WIFI_GetScanResult(p1,p2,p3,p4)
#define OS_WIRELESS_SET_ROAM_MODE(pWIFI,p1,p2,p3)		pWIFI^.WIFI_SetRoamMode $ P_WIFI_SetRoamMode(p1,p2,p3)
#define OS_WIRELESS_TRIGGER_ROAM(pWIFI,p1,p2,p3)		pWIFI^.WIFI_TriggerRoam $ P_WIFI_TriggerRoam(p1,p2,p3)
#define OS_WIRELESS_SET_LRSSI_ROAM_PARAM(pWIFI,p1,p2,p3,p4,p5,p6)		pWIFI^.WIFI_SetLRSSIroam_Params $ P_WIFI_SetLRSSIroam_Params(p1,p2,p3,p4,p5,p6)
#define OS_WIRELESS_SET_BIAS_ROAM_PARAM(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_SetBIASroam_Params $ P_WIFI_SetBIASroam_Params(p1,p2,p3,p4)
// Since Version 0x00010001
#define OS_WIRELESS_NETWORKS_GET(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_NetworksGet $ P_WIFI_NetworksGet(p1,p2,p3,p4)
#define OS_WIRELESS_NETWORK_ADD(pWIFI,p1,p2,p3)		pWIFI^.WIFI_NetworkAdd $ P_WIFI_NetworkAdd(p1,p2,p3)
#define OS_WIRELESS_NETWORK_CHANGE(pWIFI,p1,p2,p3)		pWIFI^.WIFI_NetworkChange $ P_WIFI_NetworkChange(p1,p2,p3)
#define OS_WIRELESS_NETWORK_CONTROL(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_NetworkControl $ P_WIFI_NetworkControl(p1,p2,p3,p4)
// Since Version 0x00010002
#define OS_WIRELESSAP_START(pWIFI)              		pWIFI^.WIFIAP_Start 			$ P_WIFIAP_Start()
#define OS_WIRELESSAP_STOP(pWIFI)              			pWIFI^.WIFIAP_Stop 				$ P_WIFIAP_Stop()
#define OS_WIRELESSAP_RESTART(pWIFI)              		pWIFI^.WIFIAP_Restart			$ P_WIFIAP_Restart()
#define OS_WIRELESSAP_INSTALLED(pWIFI)					pWIFI^.WIFIAP_Installed			$ P_WIFIAP_Installed()
#define OS_WIRELESSAP_ISRUNNING(pWIFI)					pWIFI^.WIFIAP_IsRunning			$ P_WIFIAP_IsRunning()
#define OS_WIRELESSAP_GETCONFIG(pWIFI,p1,p2,p3,p4)     pWIFI^.WIFIAP_GetConfig 			$ P_WIFIAP_GetConfig(p1,p2,p3,p4)
#define OS_WIRELESSAP_SETCONFIG(pWIFI,p1,p2,p3)        pWIFI^.WIFIAP_SetConfig 			$ P_WIFIAP_SetConfig(p1,p2,p3)
// Since version 0x00010003
#define OS_WIRELESSAP_START_MULTI(pWIFI,p1)              	pWIFI^.WIFIAP_Start_Multi 	$ P_WIFIAP_Start_Multi(p1)
#define OS_WIRELESSAP_STOP_MULTI(pWIFI,p1)              	pWIFI^.WIFIAP_Stop_Multi 	$ P_WIFIAP_Stop_Multi(p1)
#define OS_WIRELESSAP_RESTART_MULTI(pWIFI,p1)             	pWIFI^.WIFIAP_Restart_Multi	$ P_WIFIAP_Restart_Multi(p1)
#define OS_WIRELESSAP_INSTALLED_MULTI(pWIFI,p1)			pWIFI^.WIFIAP_Installed_Multi	$ P_WIFIAP_Installed_Multi(p1)
#define OS_WIRELESSAP_ISRUNNING_MULTI(pWIFI,p1)			pWIFI^.WIFIAP_IsRunning_Multi	$ P_WIFIAP_IsRunning_Multi(p1)
#define OS_WIRELESSAP_GETCONFIG_MULTI(pWIFI,p1,p2,p3,p4,p5)	pWIFI^.WIFIAP_GetConfig_Multi 	$ P_WIFIAP_GetConfig_Multi(p1,p2,p3,p4,p5)
#define OS_WIRELESSAP_SETCONFIG_MULTI(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFIAP_SetConfig_Multi 	$ P_WIFIAP_SetConfig_Multi(p1,p2,p3,p4)
// Since version 0x00010004
#define OS_WIRELESS_SET_RADIO(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_SetRadio $ P_WIFI_SetRadio(p1,p2,p3,p4)
#define OS_WIRELESS_GET_RADIO(pWIFI,p1,p2,p3)			pWIFI^.WIFI_GetRadio $ P_WIFI_GetRadio(p1,p2,p3)
#define OS_WIRELESS_SET_TX_POWER(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFI_SetTxPower $ P_WIFI_SetTxPower(p1,p2,p3,p4)
#define OS_WIRELESS_GET_TX_POWER(pWIFI,p1,p2,p3)		pWIFI^.WIFI_GetTxPower $ P_WIFI_GetTxPower(p1,p2,p3)
// Since version 0x00010006
#define OS_WIRELESS_CLIENT_START(pWIFI,p1)		pWIFI^.WIFI_ClientStart $ P_WIFI_ClientStart(p1)
#define OS_WIRELESS_CLIENT_STOP(pWIFI,p1)		pWIFI^.WIFI_ClientStop $ P_WIFI_ClientStop(p1)
#define OS_WIRELESS_CLIENT_ISRUNNING(pWIFI,p1)		pWIFI^.WIFI_ClientIsRunning $ P_WIFI_ClientIsRunning(p1)
// Since version 0x00010007
#define OS_WIRELESSAP_GET_CONNECTED_CLIENTS(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFIAP_GetConnectedClients $ P_WIFIAP_GetConnectedClients(p1,p2,p3,p4)
#define OS_WIRELESSAP_ACTIVATE_DEBUG_VALUES(pWIFI,p1,p2,p3)		pWIFI^.WIFIAP_ActivateDebugValues $ P_WIFIAP_ActivateDebugValues(p1,p2,p3)
#define OS_WIRELESSAP_GET_DEBUG_VALUES(pWIFI,p1,p2,p3,p4)		pWIFI^.WIFIAP_GetDebugValues $ P_WIFIAP_GetDebugValues(p1,p2,p3,p4)
// Since version 0x00010008
#define OS_WIRELESS_WPA_CTRL_EVENTS_ONOFF(pWIFI,p1,p2,p3)		pWIFI^.WIFI_WpaCtrlEventsOnoff $ P_WIFI_WPA_CTRL_EVENTS_ONOFF(p1,p2,p3)
#define OS_WIRELESS_WPA_CTRL_EVENTS_GET(pWIFI,p1,p2,p3,p4)	    pWIFI^.WIFI_WpaCtrlEventsGet $ P_WIFI_WPA_CTRL_EVENTS_GET(p1,p2,p3,p4)


#endif
