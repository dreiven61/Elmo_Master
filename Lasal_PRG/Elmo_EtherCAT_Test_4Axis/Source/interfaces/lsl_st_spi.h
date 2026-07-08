#ifndef _LSL_ST_SPI_H
#pragma once
#define _LSL_ST_SPI_H

// Interface
#define INTERFACE_SPI_COMMON                "SPI_COMMON"

//----------------------------------
// Errors-Codes
#define SPI_NO_ERROR                         0    // Kein Fehler
#define SPI_INVALID_PARAM                   -1    // Unzulässiger Parameter (Übergabe-, global -, ...)
#define SPI_NO_IF                           -2    // Es ist kein SPI Interface auf der Platform installiert/benutzbar
#define SPI_EXCEED_BUFFER_SIZE              -3    // Datenbuffer ist zu groß
#define SPI_NO_RESPONSE                     -4    // Es ist keine Antwort vom Slave verfügbar
#define SPI_INIT_FAILED                     -5    // Fehler trat während des Inits auf
#define SPI_BAD_STATE                       -6    // Es wurde ein Fehler im SPI festgestellt
#define SPI_TRANSFER_FAILED                 -7    // Es kann nichts gesendet werden (FIFO voll,..)
#define SPIIF_INVALIDHANDLE                 -101  // Das Handle ist unbekannt
#define SPIIF_NOTINITIALISED                -102  // Interface ist nicht initialisiert
                                       
//----------------------------------
// Data
#define SPI_MAX_DATA_SIZE                   500    // Maximale Anzahl zu übertragender Bytes --> muss mit OS übereinstimmen!!
// SPI-Nummern
#define SPINO_VORTEX_APP                    1      // SPI-Nummer für Kommunikation auf Vortex als Master mit Header
#define SPINO_EDGE2_RAW1                    0x10   // SPI-Nummer 1 für Kommunikation auf Edge2 als Master ohne Protokoll
#define SPINO_EDGE2_RAW2                    0x20   // SPI-Nummer 2 für Kommunikation auf Edge2 als Master ohne Protokoll
#define SPINO_EDGE2_RAW3                    0x30   // SPI-Nummer 3 für Kommunikation auf Edge2 als Master ohne Protokoll
#define SPINO_EDGE2_RAW4                    0x40   // SPI-Nummer 4 für Kommunikation auf Edge2 als Master ohne Protokoll

(*!
 * Initialisiert die SPI-Schnittstelle.
 * Die Nummer gibt eine Typbezeichnung und Schnittstelle an.
 * Der Parameter hängt von der jeweiligen benutzten SPI-
 * Schnittstelle ab (zB ulParam == ClockDivider). Wenn kein
 * Fehler auftritt, wird das Handle in #piHandle hinterlegt.
 *
 * \param ulSPINo Spezifische Nummer, welche SPI initialisiert werden soll
 * \param ulParam Parameter, abhängig vom Typ/SPINo
 * \param piHandle Pointer auf dem das Handle gespeichert werden soll
 * \return Error-Code (<0 bei Fehler)
 *
 * ulParam bei OS > 09.01.100 : ulClockNo
 * 		Bits 0..15		SPI-Speed
 * 						Wert < 15: zum RTK (ETVEdge) kompatibler Teilerwert für die SPI-Clock
 * 					   		(siehe Kommentar unten)
 * 						Wert >= 15: SPI-Clock in 10kHz Einheiten
 * 							z.B. 1200 = 12MHz
 * 		Bits 16..23		Bits per Word (0=Defaultwert 8BPW)
 * 		Bits 24..27		Chipselect (0..3)
 * 		Bits 28..31		Mode
 * 									CPOL	CPHA
 * 							Mode 0		0		0
 * 							Mode 1		0		1
 * 							Mode 2		1		0
 * 							Mode 3		1		1
 *
 * ulParam beim ETVEdge: ulClockNo
 * 	ulClockNo	SPI-clock[Hz]
 * 	  0 		   5533333
 * 	  1 		   5928571
 * 	  2 		   6384615
 * 	  3 		   6916666
 * 	  4 		   7545454
 * 	  5 		   8300000
 * 	  6 		   9222222
 * 	  7 		  10375000
 * 	  8 		  11857142
 * 	  9 		  13833333
 * 	 10 		  16600000
 * 	 11 		  20750000
 * 	 12 		  27666666
 * 	 13 		  41500000
 * 	 14 		  83000000
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_Init
VAR_INPUT
  ulSPINo : UDINT;
  ulParam : UDINT;
  piHandle : ^DINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;

(*!
 * Überprüft den Status der Schnittstelle.
 *
 * \return 0, kein Fehler
 *         negativer Wert gibt den Fehlercode an
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_GetState
VAR_INPUT
  iHandle : DINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;

(*!
 * Liest Daten vom SPI
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_ReadData
VAR_INPUT
  iHandle : DINT;
  pucData : ^USINT;
  ulSize : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;

(*!
 * Schreibt Daten übers SPI
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_WriteData
VAR_INPUT
  iHandle : DINT;
  pucData : ^USINT;
  ulSize : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;

(*!
 * Schreibt eine beliebige Anzahl von Daten aufs SPI und liest die Empfangsdatan.
 *
 * \param pucTxData Datenpuffer der zu schreibenden Daten
 * \param pucRxData Datenpuffer der zu lesenden Daten
 * \param ulSize Anzahl der zu übertragenden Bytes
 * \param burstMode Flag ist gesetzt, wenn während der gesamten Übertragung das Chipselect
 * 		aktiv sein soll.
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_TransferData
VAR_INPUT
  iHandle : DINT;
  pucTxData : ^USINT;
  pucRxData : ^USINT;
  ulSize : UDINT;
  burstMode : DINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;

(*!
 * Setzt die SPI-Schnittstelle zurück
 *)
FUNCTION __CDECL GLOBAL P_SpiIf_Release
VAR_INPUT
  iHandle : DINT;
END_VAR;


#define OS_SPIIF_INIT(itf,p1,p2,p3)  itf^.SPIIF_Init $ P_SpiIf_Init(p1,p2,p3)
#define OS_SPIIF_GETSTATE(itf,p1,p2,p3)  itf^.SPIIF_GetState $ P_SpiIf_GetState(p1,p2,p3)
#define OS_SPIIF_READDATA(itf,p1,p2,p3)  itf^.SPIIF_ReadData $ P_SpiIf_ReadData(p1,p2,p3)
#define OS_SPIIF_WRITEDATA(itf,p1,p2,p3)  itf^.SPIIF_WriteData $ P_SpiIf_WriteData(p1,p2,p3)
#define OS_SPIIF_TRANSFERDATA(itf,p1,p2,p3,p4,p5)  itf^.SPIIF_TransferData $ P_SpiIf_TransferData(p1,p2,p3,p4,p5)
#define OS_SPIIF_RELEASE(itf,p1,p2,p3)  itf^.SPIIF_Release $ P_SpiIf_Release(name,p1,p2,p3)

#pragma pack (push, 1)
TYPE
    OS_SPI_COMMON : STRUCT
      ulVersion : UDINT;
      SPIIF_Init : pVoid;
      SPIIF_GetState : pVoid;
      SPIIF_ReadData : pVoid;
      SPIIF_WriteData : pVoid;
      SPIIF_Release : pVoid;
      // since version 3:
      SPIIF_TransferData : pVoid;
    END_STRUCT;
END_TYPE
#pragma pack (pop)

#endif // _LSL_ST_SPI_H
