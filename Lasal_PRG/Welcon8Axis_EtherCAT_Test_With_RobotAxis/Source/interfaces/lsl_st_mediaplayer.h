(****************************************************************************************)
(*                                                                                      *)
(* lsl_st_mediaplayer.h                                                                 *)
(*                                                                                      *)
(* enthält die Definitionen für das Medienplayer Interface                              *)
(*                                                                                      *)
(*                                                                                      *)
(*                                                                                      *)
(*                                                                                      *)
(****************************************************************************************)
//HFILTER:1
#ifndef __LSL_ST_MEDIAPLAYER
//HFILTER:
//HFILTER:2
#pragma once
//HFILTER:
#define __LSL_ST_MEDIAPLAYER



//
// Defines
//
// Error Codes

// Media Player
#define LRTERROR_PLAYER_BASE				0x81600000
#define LRTERROR_PLAYER_NOMEDIAPLAYER		(LRTERROR_PLAYER_BASE + 0x0001)
#define LRTERROR_PLAYER_INIT_PLAYER			(LRTERROR_PLAYER_BASE + 0x0002)
#define LRTERROR_PLAYER_DISABLED_MEDIAPLAYER	(LRTERROR_PLAYER_BASE + 0x0003)
#define LRTERROR_PLAYER_UNKNOWN_CMD			(LRTERROR_PLAYER_BASE + 0x0004)
#define LRTERROR_PLAYER_UNSUPPORTED_MRL		(LRTERROR_PLAYER_BASE + 0x0005)
#define LRTERROR_PLAYER_INVALID_STATE		(LRTERROR_PLAYER_BASE + 0x0006)
#define LRTERROR_PLAYER_IS_PLAYING			(LRTERROR_PLAYER_BASE + 0x0007)
#define LRTERROR_PLAYER_IS_STOPPED			(LRTERROR_PLAYER_BASE + 0x0008)
#define LRTERROR_PLAYER_NO_MEDIA			(LRTERROR_PLAYER_BASE + 0x0009)
#define LRTERROR_PLAYER_NO_PLAYER			(LRTERROR_PLAYER_BASE + 0x000A)
#define LRTERROR_PLAYER_ERROR_MRL			(LRTERROR_PLAYER_BASE + 0x000B)
#define LRTERROR_PLAYER_ERROR_PLAYER		(LRTERROR_PLAYER_BASE + 0x000C)
#define LRTERROR_PLAYER_ALREADY_INIT		(LRTERROR_PLAYER_BASE + 0x000D)
#define LRTERROR_PLAYER_HANDLE				(LRTERROR_PLAYER_BASE + 0x000E)
#define LRTERROR_PLAYER_MEDIA_INFO			(LRTERROR_PLAYER_BASE + 0x000F)
#define LRTERROR_PLAYER_MEDIA_INFO_TRACK	(LRTERROR_PLAYER_BASE + 0x0010)
#define LRTERROR_PLAYER_LESS_MEMORY			(LRTERROR_PLAYER_BASE + 0x0011)
#define LRTERROR_PLAYER_UNKNOWN_TYPE		(LRTERROR_PLAYER_BASE + 0x0012)
#define LRTERROR_PLAYER_PARAM_SIZE			(LRTERROR_PLAYER_BASE + 0x0013)
#define LRTERROR_PLAYER_PARAM_UNKNOWN		(LRTERROR_PLAYER_BASE + 0x0014)
#define LRTERROR_PLAYER_PARAM_ERROR			(LRTERROR_PLAYER_BASE + 0x0015)
#define LRTERROR_PLAYER_LOAD_MP				(LRTERROR_PLAYER_BASE + 0x0016)

// Mediaplayer States:
#define MEDIAPLAYER_STATE_OPENING		1
#define MEDIAPLAYER_STATE_BUFFERING		2
#define MEDIAPLAYER_STATE_PLAYING		3
#define MEDIAPLAYER_STATE_PAUSED		4
#define MEDIAPLAYER_STATE_STOPPED		5
#define MEDIAPLAYER_STATE_ENDED			6
#define MEDIAPLAYER_STATE_ERROR			7

// Mediaplayer Info requests:
#define MEDIAPLAYER_INFO_STATE			0	// Same as calling mpGetState
#define MEDIAPLAYER_INFO_MEMNEED		1

// Interface Name
#define INTERFACE_MEDIAPLAYER                  "GFX_MEDIAPLAYER"

// Interface
#pragma pack (push, 1)
TYPE
	LSL_MEDIAPLAYER : STRUCT
		// Version
		udVersion : UDINT;

		mpCreate : pVoid;
		mpDestroy : pVoid;
		mpGetWindowHandle : pVoid;

		mpSetMedia : pVoid;
		mpPlay : pVoid;
		mpStop : pVoid;
		mpPause : pVoid;
		mpResume : pVoid;
		mpGetState : pVoid;
		mpGetInfo : pVoid;
		// since Version 2
		mpVolume : pVoid;
	END_STRUCT;
END_TYPE
#pragma pack (pop)


FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_CREATE
VAR_INPUT
    flags   : UDINT;
    wndHandle : pVoid;
    x       : INT;
    y       : INT;
    w       : INT;
    h       : INT;
    style   : UDINT;
END_VAR
VAR_OUTPUT
    retval      : pVoid;
END_VAR;
#define MEDIAPLAYER_Create(pMP,p1,p2,p3,p4,p5,p6,p7) pMP^.mpCreate $ P_GFX_MEDIAPLAYER_CREATE(p1,p2,p3,p4,p5,p6,p7)


FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_DESTROY
VAR_INPUT
    handle : pVoid;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Destroy(pMP,p1) pMP^.mpDestroy $ P_GFX_MEDIAPLAYER_DESTROY(p1)


FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_SETMEDIA
VAR_INPUT
    handle : pVoid;
    mrl : ^CHAR;
    flags : UDINT;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_SetMedia(pMP,p1,p2,p3) pMP^.mpSetMedia $ P_GFX_MEDIAPLAYER_SETMEDIA(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_PLAY
VAR_INPUT
    handle : pVoid;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Play(pMP,p1) pMP^.mpPlay $ P_GFX_MEDIAPLAYER_PLAY(p1)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_STOP
VAR_INPUT
    handle : pVoid;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Stop(pMP,p1) pMP^.mpStop $ P_GFX_MEDIAPLAYER_STOP(p1)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_PAUSE
VAR_INPUT
    handle : pVoid;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Pause(pMP,p1) pMP^.mpPause $ P_GFX_MEDIAPLAYER_PAUSE(p1)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_RESUME
VAR_INPUT
    handle : pVoid;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Resume(pMP,p1) pMP^.mpResume $ P_GFX_MEDIAPLAYER_RESUME(p1)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_GETSTATE
VAR_INPUT
    handle  : pVoid;
    state   : ^UDINT;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_GetState(pMP,p1,p2) pMP^.mpGetState $ P_GFX_MEDIAPLAYER_GETSTATE(p1,p2)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_GETINFO
VAR_INPUT
    handle  : pVoid;
    infoType   : UDINT;
	  pData	: pVoid;
	  size	: UDINT;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_GetInfo(pMP,p1,p2,p3,p4) pMP^.mpGetInfo $ P_GFX_MEDIAPLAYER_GETINFO(p1,p2,p3,p4)

FUNCTION __CDECL GLOBAL P_GFX_MEDIAPLAYER_VOLUME
VAR_INPUT
    handle  : pVoid;
    volume   : ^UDINT;
	  set	: UDINT;
END_VAR
VAR_OUTPUT
    retval      : DINT;
END_VAR;
#define MEDIAPLAYER_Volume(pMP,p1,p2,p3) pMP^.mpVolume $ P_GFX_MEDIAPLAYER_VOLUME(p1,p2,p3)



//HFILTER:1
#endif
//HFILTER:
