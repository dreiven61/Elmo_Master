#ifndef LSL_X_RAM_H
#pragma once
#define LSL_X_RAM_H

// Defines

// Interface
#define INTERFACE_X_RAM "IX_RAM"

// Ergebniscodes
// ok, kein Fehler
#define X_RAM_OK                        0

// Error defines
#define ERROR_WRONG_INDEX             -2    // -ENOENT
#define ERROR_BUSY                    -16   // -EBUSY
#define ERROR_MAX_LENGH_EXCEEDED      -34   // -ERANGE
// read/write geben Fehler als < 0 retour. 
// und sie verwenden die Linux errno

// Flag defines
#define XRAM_BUS_TYPE_I2C             16#0001
#define XRAM_BUS_TYPE_SPI             16#0002
#define XRAM_BUS_TYPE_MASK            16#00FF
#define XRAM_REAL_TIME_CAPABLE        16#0100

#define LSL_X_RAM_TYPE_VERSION        0x00000001

//////////////////////////////////////////////
// STRUCTS
//////////////////////////////////////////////
TYPE 
    #pragma pack (push, 1)
    LSL_X_RAM : STRUCT
        version                 : UDINT;

        x_ram_get_count     : pVoid;
        
        x_ram_info_by_nr    : pVoid;
        x_ram_read          : pVoid;
        x_ram_write         : pVoid;

        x_ram_status_asy    : pVoid;
        x_ram_read_asy      : pVoid;
        x_ram_write_asy     : pVoid;

    END_STRUCT;
    
    LSL_X_RAM_INFO_TYPE : STRUCT
        ram_type      : ARRAY [0..15] OF CHAR;
        status        : DINT;
        size          : UDINT;
        flags         : BDINT;
    END_STRUCT;    
    
    #pragma pack (pop)
END_TYPE

//////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////
FUNCTION __CDECL GLOBAL P_x_ram_get_count
    VAR_INPUT
        pRamCount : ^UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_info_by_nr
    VAR_INPUT
        index : UDINT;
        pRamInfo : pVoid; (* Struct: ram_info *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_read
    VAR_INPUT
        index : UDINT;
        offset : UDINT;
        len : UDINT;
        buff : ^hsint;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_write
    VAR_INPUT
        index : UDINT;
        offset : UDINT;
        len : UDINT;
        buff : ^hsint;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_status_asy
    VAR_INPUT
        index : UDINT;
        status : ^DINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_read_asy
    VAR_INPUT
        index : UDINT;
        offset : UDINT;
        len : UDINT;
        buff : ^hsint;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_x_ram_write_asy
    VAR_INPUT
        index : UDINT;
        offset : UDINT;
        len : UDINT;
        buff : ^hsint;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

// Since Version 0x00010001
#define OS_X_RAM_CHECK_VERSION(pX_RAM)                pX_RAM^.version               <> LSL_X_RAM_TYPE_VERSION
#define OS_x_ram_get_count(pX_RAM,p1)                 pX_RAM^.x_ram_get_count       $ P_x_ram_get_count(p1)
#define OS_x_ram_info_by_nr(pX_RAM,p1,p2)             pX_RAM^.x_ram_info_by_nr      $ P_x_ram_info_by_nr(p1,p2)
#define OS_x_ram_read(pX_RAM,p1,p2,p3,p4)             pX_RAM^.x_ram_read            $ P_x_ram_read(p1,p2,p3,p4)
#define OS_x_ram_write(pX_RAM,p1,p2,p3,p4)            pX_RAM^.x_ram_write           $ P_x_ram_write(p1,p2,p3,p4)
#define OS_x_ram_status_asy(pX_RAM,p1,p2)             pX_RAM^.x_ram_status_asy      $ P_x_ram_status_asy(p1,p2)
#define OS_x_ram_read_asy(pX_RAM,p1,p2,p3,p4)         pX_RAM^.x_ram_read_asy        $ P_x_ram_read_asy(p1,p2,p3,p4)
#define OS_x_ram_write_asy(pX_RAM,p1,p2,p3,p4)        pX_RAM^.x_ram_write_asy       $ P_x_ram_write_asy(p1,p2,p3,p4)

#endif