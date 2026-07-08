/////////////////////////////////////////////////////////////////////
//
// filename: lsl_st_sysinfo.h
// 
// category: osinterface
//
// about file: read system information
//
// department: ...
//
// contact person:
//
// created: 
//
// history:
//    v1.0 obejoh  initiator
//    v1.1 obejoh  enhancde group sysinfo
//    v1.2 obejoh  merge diff with OS (2024-08-02)
//
/////////////////////////////////////////////////////////////////////

#ifndef  __LSL_SYSINFO_H
#pragma once
#define  __LSL_SYSINFO_H

(*
 * LSL_SYSINFO_TYPE_VERSION 0x00010000
 *  0x00010000
 *  	Create first version
 *  0x00020000
 *  	add SYSinfo_GetPropertyEx with index & subindex		Bsp: diskid & partid
 *  0x00030000
 *  	add SYSinfo_SetPropertyEx with index & subindex
 *)

// Interface
#define INTERFACE_LSL_SYSINFO            "LSL_SYSINFO"

// defines for possible Error Codes
#define LSL_SYSINFO_ERR_NONE      0   ///< no error
#define LSL_SYSINFO_ERR_ID        -1  ///< Unknown ID for SYSINFO
#define LSL_SYSINFO_ERR_NOT_AVAIL -2  ///< Information for given ID not available on this CPU
#define LSL_SYSINFO_ERR_BUF       -3  ///< Invalid Buffer or Buffer length
#define LSL_SYSINFO_ERR_DATA      -4  ///< error while get data
#define LSL_SYSINFO_NOT_RUNNING   -5  ///< service not running
#define LSL_SYSINFO_ERR_CIL       -99 ///< CIL not available

// this values are used as input for the idx variable of the
// SYSinfo_GetPropertyEx function to specify the service you
// want to get info from
#define LSL_SYSINFOEX_SERVICE_BROWSER        0 ///< define for choosing the service (browser)
#define LSL_SYSINFOEX_SERVICE_DATASERVICE    1 ///< define for choosing the service (dataservice)
#define LSL_SYSINFOEX_SERVICE_WEBSERVER      2 ///< define for choosing the service (webserver)
#define LSL_SYSINFOEX_SERVICE_NODEJS         3 ///< define for choosing the service (nodejs)
#define LSL_SYSINFOEX_SERVICE_SAFEPLCHANDLER 4 ///< define for choosing the service (safeplchandler)
#define LSL_SYSINFOEX_SERVICE_PORTFORWARD    5 ///< define for choosing the service (portforward)


TYPE
  LSL_SYSINFO_ID :
  (
	// cpu 1..19
	LSL_SYSINFO_ID_CPU_PLATFORM_STR := 1,	// STRING: (ARM,X86,..)
	LSL_SYSINFO_ID_CPU_PLATFORM,			// DINT: ECpuPlatformX86=0, ECpuPlatformArm
	LSL_SYSINFO_ID_CPU_TYPE,				// DINT: ECpuTypeX86=0, ECpuTypeVortex, ECpuTypeArm, ECpuTypeArmV5, ECpuTypeArmV7
	LSL_SYSINFO_ID_CPU_COUNT,				// DINT: count of cpu core(s)
	LSL_SYSINFO_ID_CPU_CLOCK,				// DINT: clock of cpu-core (kHz)

	// platform 20..29
	LSL_SYSINFO_ID_PLATFORM_NAME_STR := 20,	// STRING: eeprom name of the CPU
	LSL_SYSINFO_ID_PLATFORM_TYPE,			// DINT: DESTPLC_...
	LSL_SYSINFO_ID_PLATFORM_BOARD,			// STRING: alias

	// Board info for X86 only 30..49
	LSL_SYSINFO_DMI_BIOS_DATE		 := 30,	// string:
	LSL_SYSINFO_DMI_BIOS_VENDOR,			// string:
	LSL_SYSINFO_DMI_BIOS_VERSION,			// string:
	LSL_SYSINFO_DMI_BOARD_NAME,				// string:
	LSL_SYSINFO_DMI_BOARD_SERIAL,			// string:
	LSL_SYSINFO_DMI_BOARD_VENDOR,			// string:
	LSL_SYSINFO_DMI_BOARD_VERSION,			// string:
	LSL_SYSINFO_DMI_CHASSIS_SERIAL,			// string:
	LSL_SYSINFO_DMI_CHASSIS_TYPE,			// string:
	LSL_SYSINFO_DMI_CHASSIS_VENDOR,			// string:
	LSL_SYSINFO_DMI_CHASSIS_VERSION,		// string:
	LSL_SYSINFO_DMI_PRODUCT_NAME,			// string:
	LSL_SYSINFO_DMI_PRODUCT_SERIAL,			// string:
	LSL_SYSINFO_DMI_PRODUCT_UUID,			// string:
	LSL_SYSINFO_DMI_PRODUCT_VERSION,		// string:
	LSL_SYSINFO_DMI_SYS_VENDOR,				// string:
	
	LSL_SYSINFO_ID_TEMPSENS0 := 50,			// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_BOARD0 := 50,	// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_BOARD1,			// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_BOARD2,			// DINT: temperature in milli Celsius
	
	LSL_SYSINFO_ID_TEMPSENS_CPU0 := 55,		// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_CPU1,			// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_CPU2,			// DINT: temperature in milli Celsius
	LSL_SYSINFO_ID_TEMPSENS_CPU3,			// DINT: temperature in milli Celsius

	// 60..69 Memory info
	LSL_SYSINFO_MEM_RAM_TOTAL := 60,		// uint32: kbyte
	LSL_SYSINFO_MEM_RAM_FREE,				// uint32: kbyte
	LSL_SYSINFO_MEM_OSHEAP_TOTAL,			// uint32: kbyte
	LSL_SYSINFO_MEM_OSHEAP_FREE,			// uint32: kbyte
	LSL_SYSINFO_MEM_APP_CODE,				// uint32: kbyte
	LSL_SYSINFO_MEM_APP_HEAP,				// uint32: kbyte
	LSL_SYSINFO_MEM_PREHEAP,				// uint32: kbyte
	LSL_SYSINFO_MEM_LAST,					// last 67

	// 70..79 framebuffer
	LSL_SYSINFO_FB_ADDR := 70,				// void*:
	LSL_SYSINFO_FB_WIDTH,					// uint32:
	LSL_SYSINFO_FB_HEIGHT,					// uint32:
	LSL_SYSINFO_FB_BITS_PER_PIXEL,			// uint32:
	LSL_SYSINFO_FB_BYTES_PER_LINE,			// uint32:
	LSL_SYSINFO_FB_ROTATE,					// uint32: 0 / 90 / 180 / 270

	// 90..99 SD-Card (SWISSBIT) Information
	LSL_SYSINFO_SDCARD_VENDOR			:= 90,	// STRING: vendor name
	LSL_SYSINFO_SDCARD_MODEL,					// STRING: model name
	LSL_SYSINFO_SDCARD_FIRMWARE,				// STRING: firmware version string
	LSL_SYSINFO_SDCARD_MAX_AVERAGE_ERASE_CNT,	// UDINT: max limit of average erase count
	LSL_SYSINFO_SDCARD_AVERAGE_ERASE_CNT,		// UDINT: current average erase count
	LSL_SYSINFO_SDCARD_POWER_ON_CNT,			// UDINT: power on counter
	LSL_SYSINFO_SDCARD_TYPE,					// STRING: firmware version string
	LSL_SYSINFO_SDCARD_ISBOOTDEVICE,			// int32_t: is boot device 0 / 1

	// 100..119 OS Version Information
	LSL_SYSINFO_OS_VERSION := 100,				// string: OS
	LSL_SYSINFO_OS_VERSION_MAJOR_MAJOR,
	LSL_SYSINFO_OS_VERSION_MAJOR,
	LSL_SYSINFO_OS_VERSION_MINOR,
	LSL_SYSINFO_OS_TYPE,						// return type as uppercase string "SALAMANDER", "GECKO"
	LSL_SYSINFO_OS_TYPE_INT,					// return 2 = Salamander, 3 = Gecko (CpuEquipment Platform)
	LSL_SYSINFO_OS_TYPE_VERSION,				// Salamander2, Salamander3, Salamander4, Gecko1, ...
	LSL_SYSINFO_OS_BUILDNRSTR,					///< string:

	// 120..139 SRAM Info
	LSL_SYSINFO_SRAM_RESTART_REASON := 120,		// int32_t: restart reason code or -1 if it unknown
	LSL_SYSINFO_SRAM_TYPE,						// int32_t:
	LSL_SYSINFO_SRAM_DISKTYPE,					// int32_t:
	LSL_SYSINFO_SRAM_ADDR,						// void*: physical address of SRAM
	LSL_SYSINFO_SRAM_SIZE,						// uint32_t: size of SRAM
	LSL_SYSINFO_SRAM_OBJ_ADDR,					// void*: physical address of obj-sram
	LSL_SYSINFO_SRAM_OBJ_SIZE,					// uint32_t: size of obj-sram
	LSL_SYSINFO_SRAM_RETAIN_ADDR,				// void*: physical address of retain
	LSL_SYSINFO_SRAM_RETAIN_SIZE,				// uint32_t: size of retain
	LSL_SYSINFO_SRAM_RETAINRAM_ADDR,			// void*: physical address of retain-ram
	LSL_SYSINFO_SRAM_RETAINRAM_SIZE,			// uint32_t: size of retain-ram
	LSL_SYSINFO_SRAM_SYS_ADDR,					// void*: physical address of sys-sram
	LSL_SYSINFO_SRAM_SYS_SIZE,					// uint32_t: size of sys-sram
	LSL_SYSINFO_SRAM_PHYSICAL_ADDR,				// uint64_t: physical address of SRAM
	LSL_SYSINFO_SRAM_PHYSICAL_PTR,				///< void*:  physical address of SRAM

	LSL_SYSINFO_ID_LAST						// not used, id not importe
	)$UDINT;
END_TYPE

TYPE
  LSL_SYSINFOEX_ID :
  (
	LSL_SYSINFOEX_FAN_COUNT				:= 0x00010000,	// uint32_t count of FAN's no idx, idxSub
	LSL_SYSINFOEX_FAN_PWM,						// int32_t pwm of [idx], no idxSub
	LSL_SYSINFOEX_FAN_RPM,						// int32_t RPM of [idx], no idxSub

	LSL_SYSINFOEX_SDCARD_COUNT			:= 0x00020000,  // uint32_t count of SDCARD's
	LSL_SYSINFOEX_SDCARD_VENDOR,				// string: vendor name
	LSL_SYSINFOEX_SDCARD_MODEL,					// string: model name
	LSL_SYSINFOEX_SDCARD_FIRMWARE,				// string: firmware version string
	LSL_SYSINFOEX_SDCARD_MAX_AVERAGE_ERASE_CNT,	// uint32: max limit of average erase count
	LSL_SYSINFOEX_SDCARD_AVERAGE_ERASE_CNT,		// uint32: current average erase count
	LSL_SYSINFOEX_SDCARD_POWER_ON_CNT,			// uint32: power on counter
	LSL_SYSINFOEX_SDCARD_TYPE,					// string: type (SD,MMC,...)
  LSL_SYSINFOEX_SDCARD_ISBOOT,					// int32_t: is bootdevice

	LSL_SYSINFOEX_SENSORS_COUNT                := 0x00050000, ///< uint32: get count of sensors values
	LSL_SYSINFOEX_SENSORS_TYPE_COUNT,                         ///< uint32:
	LSL_SYSINFOEX_SENSORS_TYPE_LIST,                          ///< uint32[..]:
	LSL_SYSINFOEX_SENSORS_INFO_GET,                           ///< struct:
	LSL_SYSINFOEX_SENSORS_VALUE_GET,                          ///< struct:
	LSL_SYSINFOEX_SENSORS_VALUE_SET,                          ///< struct: "SYSinfo_SetPropertyEx"

  LSL_SYSINFOEX_FPGA_COUNT                   := 0x00060000, ///< uint32: get count of FPGA on PCI
	LSL_SYSINFOEX_FPGA_SPI_COUNT,			                        ///< uint32: get count of SPI-Master (idx = fpga_nbr)
	LSL_SYSINFOEX_FPGA_SPI_LIST,  			                      ///< uint32: get count of SPI-Master (idx = fpga_nbr) 
	LSL_SYSINFOEX_SPI_VENDOR_ID                := 0x00060100, ///< uint32: get VendorID via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_DEVICE_ID,                              ///< uint32: get DeviceID via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_PLATFORM_ID,                            ///< uint32: get PlatformId via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_HW_VERSION,                             ///< uint32: get HWVersion via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_FLASH_SIZE,                             ///< uint32: get Flash Size via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_PROG_SIZE,                              ///< uint32: get Program Size via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_VENDOR_NAME,                            ///< string: get Vendor Name via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_DEVICE_NAME,                            ///< string: get Device Name via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_SERIAL_STR,                             ///< string: get Serial String via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_SPI_FLASH_FORMAT_FEATURE,                   ///< uint32: get Flash Format Feature via SPI Interface (idx = spi handle)
	LSL_SYSINFOEX_FPGA_VERSION,   		  	

  LSL_SYSINFOEX_CONTROLLER_COUNT             := 0x00070000, ///< uint32: get count of stek-controller
	LSL_SYSINFOEX_CONTROLLER_NAME,                            ///< string:
	LSL_SYSINFOEX_CONTROLLER_BUS,                             ///< string:
	LSL_SYSINFOEX_CONTROLLER_VERSION_FW,                      ///< uint32:
	LSL_SYSINFOEX_CONTROLLER_VERSION_HW,                      ///< uint32:
	LSL_SYSINFOEX_CONTROLLER_VERSION_SW,                      ///< uint32:
	LSL_SYSINFOEX_CONTROLLER_VERSION_BL,                      ///< uint32:
	LSL_SYSINFOEX_CONTROLLER_SERIALNR,                        ///< uint32:
	LSL_SYSINFOEX_CONTROLLER_BOARDINFO,                       ///< uint32:

  LSL_SYSINFOEX_SERVICE_MASK                  := 0x00080000, ///< only to define the service group
	LSL_SYSINFOEX_SERVICE_RUN                   := 0x00080000, ///< int32: 1 if service is running, otherwise 0
	LSL_SYSINFOEX_SERVICE_MEM_MAX               := 0x00080001, ///< uint32: maximal memory the service can use
	LSL_SYSINFOEX_SERVICE_MEM_CURR              := 0x00080002, ///< uint32: current memory the service is using

  LSL_SYSINFOEX_ID_LAST
	)$UDINT;
END_TYPE


TYPE
	#pragma pack (push, 1)
	LSL_SYSINFO_TYPE : STRUCT
		// version 0x00010000
		version	: UDINT;
		SYSINFO_GetProperty : pVoid;
		// version 0x00020000
		SYSINFO_GetPropertyEx : pVoid;
		// version 0x00030000
		SYSINFO_SetPropertyEx : pVoid;
	END_STRUCT;
	#pragma pack (pop)
END_TYPE

////////////////////////////////////////////////////////////////////////////////
// LASAL Function Prototypes
////////////////////////////////////////////////////////////////////////////////
FUNCTION __CDECL GLOBAL P_SYSINFO_GetProperty
	VAR_INPUT
		id	: LSL_SYSINFO_ID;
		pBuf : pVoid;
		len : UDINT;
	END_VAR
	VAR_OUTPUT
		retval	: DINT;
	END_VAR;

FUNCTION __CDECL GLOBAL P_SYSINFO_GetPropertyEx
	VAR_INPUT
		id	: LSL_SYSINFOEX_ID;
    index : UDINT;
    indexSub : UDINT;
		pBuf : pVoid;
		len : UDINT;
	END_VAR
	VAR_OUTPUT
		retval	: DINT;
	END_VAR;

FUNCTION __CDECL GLOBAL P_SYSINFO_SetPropertyEx
	VAR_INPUT
		id	: LSL_SYSINFOEX_ID;
    index : UDINT;
    indexSub : UDINT;
		pBuf : pVoid;
		len : UDINT;
	END_VAR
	VAR_OUTPUT
		retval	: DINT;
	END_VAR;

////////////////////////////////////////////////////////////////////////////////
// LASAL MACROS
////////////////////////////////////////////////////////////////////////////////
#define OS_SYSINFO_GET_PROP(pSYSINFO,p1,p2,p3) pSYSINFO^.SYSINFO_GetProperty $ P_SYSINFO_GetProperty(p1,p2,p3)
#define OS_SYSINFO_GET_PROPEX(pSYSINFO,p1,p2,p3,p4,p5) pSYSINFO^.SYSINFO_GetPropertyEx $ P_SYSINFO_GetPropertyEx(p1,p2,p3,p4,p5)
#define OS_SYSINFO_SET_PROPEX(pSYSINFO,p1,p2,p3,p4,p5) pSYSINFO^.SYSINFO_SetPropertyEx $ P_SYSINFO_SetPropertyEx(p1,p2,p3,p4,p5)

#endif
