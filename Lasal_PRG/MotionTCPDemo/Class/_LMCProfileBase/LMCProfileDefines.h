// Hier werden alle globalen Definitionen des LMCProfilbausteins durchgeführt
#pragma once

#define  _LMC_AXIS_0      0
#define  _LMC_AXIS_1      1
#define  _LMC_AXIS_2      2
#define  _LMC_AXIS_3      3
#define  _LMC_AXIS_4      4
#define  _LMC_AXIS_5      5
#define  _LMC_AXIS_6      6
#define  _LMC_AXIS_7      7
#define  _LMC_AXIS_8      8

#define  _LMC_MAX_AXIS    9  //Anzahl der Achsen

#define CIRCLE_REMOTE_X       0
#define CIRCLE_REMOTE_Y       1
#define CIRCLE_REMOTE_Z       2

#define CHORD_APPROX        2

#define SCALE_ANGLE     1024000

// TempPosition, wenn gelockte Achse nicht fahren soll
#ifndef _VOID_POS
#define  _VOID_POS             16#7FFFFFFF
#endif
// Zeit nicht definiert
#ifndef _VOID_TIME
#define  _VOID_TIME           16#FFFFFFFF
#endif

#define MAX_LOG_BUFFER_INDEX  100   //log buffer size = MAX_LOG_BUFFER_INDEX + 1

#ifndef BIT_OPERATORS

  #define BIT_OPERATORS

  #define BIT_CHECK(BITFIELD,BITNR)   BITFIELD AND (1 SHL BITNR)
  #define BIT_SET(BITFIELD,BITNR)     BITFIELD:=BITFIELD OR (1 SHL BITNR)
  #define BIT_CLEAR(BITFIELD,BITNR)   BITFIELD:=BITFIELD AND (NOT(1 SHL BITNR))
  #define BIT_FLIP(BITFIELD,BITNR)    BITFIELD:=BITFIELD XOR (1 SHL BITNR)

#endif

// Nachkommafaktor
#define NK_FACTOR                       1000
// Ticks früher bremsen
#define PRE_CYCL                        1

#define SHIFT_MUL         3
#define SHIFT_DIV         2


#define MIN_TICKS_CUBIC_SPLINE    5
#define MIN_TICKS_BIQUAD_SPLINE   5

#ifndef _LMCPROF_EPSILON
#define _LMCPROF_EPSILON        0.000001
#endif

#define _LMCPROF_EPSILON_SQRD   _LMCPROF_EPSILON*_LMCPROF_EPSILON    // this define is _LMCPROF_EPSILON^2, only suitable for LREAL variable types

#define _LMCPROF_EPSILON_JERK        0.0
// #define _LMCPROF_EPSILON_JERK        0.000000000000000000001

#define SCALE_SPLINE_VT       10000
#define SCALE_SPLINE_AT       100000000

#define POLY_CONST           0
#define POLY_LINEAR          1
#define POLY_PARABEL         2
#define POLY_CUBIC           3


//*** MoveCurve - DEFINES ***
#define SHRINK_PATH         0.5
#define MAX_TRIMMCOUNTER    25
#define MAX_START_FACTOR    0.99
#define MIN_START_FACTOR    0.01
#define MAX_END_FACTOR      0.99
#define MIN_END_FACTOR      0.01

// **** only for debugging ****

// provide REAL data for Salamander OS and data analyzer
// #define DEBUG_REAL_DATA

// disable Mutex
// #define DEBUG_MODUS

// see spline-data
// #define DEBUG_SPLINE

// see enCmdType
// #define DEBUG_CMD_TYPE

// see second difference quotient of the TrapPosAct (actual acceleration)
// #define DEBUG_TRAP_DELTA_DIFFERENCE


// scale jerk for oszi
//#define SCALE_JERK

// #define DEBUG_HERMITE_SPLINE

#ifdef SCALE_JERK
  #define SCALE_JERKFACOR     1000000.0
#else
  #define SCALE_JERKFACOR     1.0
#endif
