(****************************************************************************************)
(*                                                                                      *)
(* lsl_st_safetydll.h                                                                   *)
(*                                                                                      *)
(* enthält die Definitionen für das Safety DLL Interface                                *)
(*                                                                                      *)
(*                                                                                      *)
(****************************************************************************************)
(*
  Die Funktionen des 'Safety DLL Interface' werden verwendet, um von einer Standard-SPS 
  aus eine Kommunikationsverbindung zu einer Safe-CPU aufzubauen und folgende 
  Abläufe auszuführen:
  - Download einer Konfiguration
  - Setzen des Verified Flags
  - Ändern des Passworts
  - Starten der Safe-CPU (verlassen des Service-Modes)
  - Stoppen der Safe-CPU (herstellen des Service-Modes)
  - Statusabfrage
  - Neustart der Safety-Applikation
  - Firmware Download
  - Parameterlisten Download und auslesen
  
  Hinweis:
  Die Funktionen dürfen nicht dafür verwendet werden, um vollautomatisch einen Ablauf 
  durchzuführen. Es muss vom Programmersteller sichergestellt werden dass die Eingabedaten 
  vom Benutzer stammen und nicht im Programm hardcodiert werden.
  
  Das Interface ISAFETY_DLL2 hat den gleichen Funktionsumfan wie das Interface ISAFETY_DLL.
  Es können damit aber mehrere Abläufe zu verschiedenen Safety-CPUs gleichzeitig 
  ausgeführt werden.
  Beim ISAFETY_DLL Interface wird ein globaler State verwendet, der mit SAFETY_NEW_STATE 
  und SAFETY_DELETE_STATE erstellt bzw. wieder entfernt wird. 
  Beim ISAFETY_DLL2 Interface wird ein lokaler State mit SAFETY_ALLOC_STATE2 erstellt und 
  mit SAFETY_FREE_STATE2 wieder freigegeben. Dieser lokale State wird bei den einzelnen 
  Funkionen des ISAFETY_DLL2 als erster Parameter angegeben.
*)
//HFILTER:1
#ifndef __LSL_ST_SAFETY_DLL
//HFILTER:
//HFILTER:2
#pragma once
//HFILTER:
#define __LSL_ST_SAFETY_DLL

// Interfacename
#define INTERFACE_SAFETY_DLL            "ISAFETY_DLL"
#define INTERFACE_SAFETY_DLL2           "ISAFETY_DLL2"

//
//Fehlercodes
//
(* Es konnte kein Safety-State angelegt werden (mit SAFETY_NEW_STATE), 
   weil bereits einer existiert.
*)
#define SAFETY_E_IN_USE                   9001

(* Beim Öffnen des für den Download vorgesehenen Files ist ein 
   Fehler aufgetreten.
*)
#define SAFETY_E_OPEN                     9002

(* Beim Lesen des für den Download vorgesehenen Files ist ein 
   Fehler aufgetreten.
*)
#define SAFETY_E_IO                       9003

(* Beim Lesen des für den Download vorgesehenen Files wurden 
   ungültige Daten erkannt.
*)
#define SAFETY_E_FILEDATA                 9004      

(* Ein Funktionsaufruf, der innerhalb einer gewissen Zeitspanne seit dem 
   Aufruf von SAFETY_SET_USERPROMPT_TIME stattfinden muss, ist ausserhalb 
   der Zeitspanne durchgeführt worden.
*)
#define SAFETY_E_INVALID_USERPROMPT_TIME  9005

(* Die Safety-DLL konnte nicht geladen werden, weil die erforderliche 
   Schnittstelle im Lasal-Betriebssystem nicht existiert oder eine 
   falsche Version aufweist-
*)
#define SAFETY_E_INVALID_CIL_VERSION      9006

(* Bei einem internen Funktionsaufruf wurden zu wendig Daten zurückgegeben.
*)
#define SAFETY_E_BUFFER_UNDERFLOW         9007

(* Ein als Funktionsparameter übergegener Buffer ist zu klein.
*)
#define SAFETY_E_BUF_TOO_SMALL            9008

(* Es steht zu wenig Speicher zur Verfügung (Heap).
*)
#define SAFETY_E_OUT_OF_MEM               9009

(* Beim Verbindungsaufbau wurde festgestellt, dass die vom Benutzer eingegebene 
   Sicherheitsnummer nicht mit der Sicherheitsnummer der Ziel-Safe-CPU 
   übereinstimmt.
*)
#define SAFETY_E_INVALID_SAFETY_NBR       9010

(* Beim internen Aufruf einer Hardwaretree-Funktion ist ein Fehler aufgetreten.
*)
#define SAFETY_E_HWT_CMD                  9011

(* Es wurde ein ungültiger Wert eines Funktionsparameters angegeben.
*)
#define SAFETY_E_INVALID_PARAM            9012

(* Beim Versuch, die Verbindung aufzubauen, wurde das Ziel-Modul nicht gefunden.
*)
#define SAFETY_E_MODULE_NOT_FOUND         9013

(* Der State-Zeiger, der an eine eine Funktion aus der Schnittstelle ISAFETY_DLL2 
   übergeben wurde, ist ungültig *)
#define SAFETY_E_INVALID_STATE_PTR        9014

(* Die Safety-Nbr wurde bereits einem anderen Safety-State zugewiesen *)
#define SAFETY_E_SAFETY_NBR_IN_USE        9015

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist eine ungültige 
   Sequenznummer erkannt worden.
*)
#define SAFETY_E_INVALID_SN_RSP           9100

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist eine ungültige 
   Länge erkannt worden.
*)
#define SAFETY_E_INVALID_LEN_RSP          9101

(* In der Antwort, die von der Safe-CPU empfangen wurde, sind zu wenig Daten 
   vorhanden.
*)
#define SAFETY_E_TOO_LESS_DATA_RSP        9102

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist eine ungültige 
   CRC erkannt worden.
*)
#define SAFETY_E_INVALID_CRC_RSP          9103

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist ein ungültiger 
   Frametyp erkannt worden.
*)
#define SAFETY_E_INVALID_FRAMETYPE_RSP    9104

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist eine ungültige 
   Adresse erkannt worden.
*)
#define SAFETY_E_INVALID_ADDR_RSP         9105

(* Die Antwort, die von der Safe-CPU erwartet wird, ist innerhalb einer 
   gewissen Zeit nicht empfangen worden.
*)
#define SAFETY_E_TIMEOUT_RSP              9106

(* Bei der Antwort, die von der Safe-CPU empfangen wurde, ist eine ungültige 
   Session-ID erkannt worden.
*)
#define SAFETY_E_INVALID_SESSID_RSP       9107

(* Ein Funktionsaufruf ist in einem ungültigen Zustand durchgeführt worden.
   Die Funktion SAFETY_FILE_GET_PRJNAME muss beispielsweise nach der 
   Funktion SAFETY_SET_FILE aufgerufen werden.
*)
#define SAFETY_E_INVALID_STATE            9200
#define SAFETY_E_INVALID_STATE_1          9201
#define SAFETY_E_INVALID_STATE_2          9202
#define SAFETY_E_INVALID_STATE_3          9203
#define SAFETY_E_INVALID_STATE_4          9204

(* In der Antwort, die von der Safe-CPU empfangen wurde, wird im Feld Returncode 
   ein Fehler angezeigt. Die Fehlernummer ist der Offset zur Basis 10000.
   Z.b. 10123 bedeutet Fehler 123 wurde von der Safe-CPU zurückgeschickt.
   Bsp. Safe-CPU-Error (SCP111):
   - 230: Invalid LogIn-Level
   - 234: Invalid Runstate
   
*)
#define SAFETY_E_SSDO_RESULT             10000

//
// Interface
//

(*
  Erstellt eine neuen Safety-State, der für den Aufruf der folgenden Funktionen 
  erforderlich ist. Muss am Anfang einmal aufgerufen werden.
  Im Safety-State werden folgende Elemente gespeichert:
    File-State: Zustand des Download-Files
      FS_IDLE        .. wenn noch kein Download-File angegeben wurde
      FS_FILE        .. wenn ein Download-File angegeben wurde und gelesen werden konnte
      FS_DOWNLOAD    .. wenn ein Download-File zur Safe-CPU übertragen wurde
    Connection-State: Zustand der Kommunikationsverbindung zur Safe-CPU
      CS_IDLE        .. wenn noch keine Sicherheitsnummer angegeben wurde
      CS_SAFETY_NBR  .. wenn eine Sicherheitsnummer angegeben wurde
      CS_CONNECTION  .. wenn eine Verbindung aufgebaut wurde
      CS_LOGGED_IN   .. wenn ein Login durchgeführt wurde (Developer-Login)
    UserPromptTime: Zeitpunkt des Aufrufs von SetUserPromptTime
    Prj-Name: Name des im Download-File angegebenen Projekts
    Rev-Nbr: Revisions-Nummer des im Download-File angegebenen Projekts
    Scpu-Name: Name der im Download-File angegebenen Ziel Safe-CPU
    HW-Path: Hardware-Pfad zur Ziel Safe-CPU

  Setzt den File-State auf FS_IDLE und den Connection-State auf CS_IDLE.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_NewState
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_NEW_STATE(pCil)                pCil^.NewState $ P_Safety_NewState()

(*
  Entfernt den Safety-State.
  Muss am Ende aufgerufen werden. Eine aufrechte Kommunikationsverbindung zur Safe-CPU 
  wird geschlossen.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_DeleteState
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DELETE_STATE(pCil)             pCil^.DeleteState $ P_Safety_DeleteState()

FUNCTION __CDECL GLOBAL P_Safety_AllocState2
VAR_INPUT
  ppState : ^pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_ALLOC_STATE2(pCil,p1)          pCil^.AllocState2 $ P_Safety_AllocState2(p1)

FUNCTION __CDECL GLOBAL P_Safety_FreeState2
VAR_INPUT
  ppState : ^pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FREE_STATE2(pCil,p1)           pCil^.FreeState2 $ P_Safety_FreeState2(p1)

(*
  Gibt das File an, das vom Safety-Designer für den Download erstellt wurde.

  Setzt den File-State auf FS_FILE wenn das File geöffnet und gelesen werden kann.

  Voraussetzung: File-State == FS_IDLE

  \param Name des Files (0-terminierter String)
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetFile
VAR_INPUT
  filename : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_FILE(pCil,p1)              pCil^.SetFile $ P_Safety_SetFile(p1)

FUNCTION __CDECL GLOBAL P_Safety_SetFile2
VAR_INPUT
  pState : pVoid;
  filename : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_FILE2(pCil,p1,p2)          pCil^.SetFile2 $ P_Safety_SetFile2(p1,p2)

(*
  Liefert den im Download-File gespeicherten Projektnamen.

  Voraussetzung: File-State >= FS_FILE

  \param prjName Buffer in den der Projektname als 0-terminierter String geschrieben wird
  \param bufSize_prjName Größe des Buffers
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_FileGetPrjName
VAR_INPUT
  prjName : ^CHAR;
  bufsizePrjName : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_PRJNAME(pCil,p1,p2)   pCil^.FileGetPrjName $ P_Safety_FileGetPrjName(p1,p2)

FUNCTION __CDECL GLOBAL P_Safety_FileGetPrjName2
VAR_INPUT
  pState : pVoid;
  prjName : ^CHAR;
  bufsizePrjName : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_PRJNAME2(pCil,p1,p2,p3)   pCil^.FileGetPrjName2 $ P_Safety_FileGetPrjName2(p1,p2,p3)

(*
  Liefert die im Download-File gespeicherte Revisionsnummer.

  Voraussetzung: File-State >= FS_FILE

  \param revNbr Buffer in den die Revisionsnummer als 0-terminierter String geschrieben wird
  \param bufSize_revNbr Größe des Buffers
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_FileGetRevNbr
VAR_INPUT
  revNbr : ^CHAR;
  bufsizeRevNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_REVNBR(pCil,p1,p2)    pCil^.FileGetRevNbr $ P_Safety_FileGetRevNbr(p1,p2)

FUNCTION __CDECL GLOBAL P_Safety_FileGetRevNbr2
VAR_INPUT
  pState : pVoid;
  revNbr : ^CHAR;
  bufsizeRevNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_REVNBR2(pCil,p1,p2,p3)    pCil^.FileGetRevNbr2 $ P_Safety_FileGetRevNbr2(p1,p2,p3)

(*
  Liefert den im Download-File gespeicherten SCPU-Namen.

  Voraussetzung: File-State >= FS_FILE

  \param scpuName Buffer in den der SCPU-Name als 0-terminierter String geschrieben wird
  \param bufSize_scpuName Größe des Buffers
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_FileGetScpuName
VAR_INPUT
  scpuName : ^CHAR;
  bufsizeScpuName : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_SCPUNAME(pCil,p1,p2)  pCil^.FileGetScpuName $ P_Safety_FileGetScpuName(p1,p2)

FUNCTION __CDECL GLOBAL P_Safety_FileGetScpuName2
VAR_INPUT
  pState : pVoid;
  scpuName : ^CHAR;
  bufsizeScpuName : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_FILE_GET_SCPUNAME2(pCil,p1,p2,p3)  pCil^.FileGetScpuName2 $ P_Safety_FileGetScpuName2(p1,p2,p3)

(*
  Muss aufgerufen werden, wenn vom Benutzer eine Eingabe angefordert wird. 
  Der Zeitpunkt dieses Aufrufs wird gemerkt und wird dann später für eine 
  Überprüfung der Plausibilität der Eingabe verwendet (es darf z.B. SetSafetyNbr 
  nicht unmittelbar nach SetUserPromptTime aufgerufen werden).

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetUserPromptTime
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_USERPROMPT_TIME(pCil)      pCil^.SetUserPromptTime $ P_Safety_SetUserPromptTime()

FUNCTION __CDECL GLOBAL P_Safety_SetUserPromptTime2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_USERPROMPT_TIME2(pCil,p1)  pCil^.SetUserPromptTime2 $ P_Safety_SetUserPromptTime2(p1)

(*
  Deaktiviert die Prüfung der Aufrufzeitpunkte der Funktionen für die 
  Benutzereingaben. Wird verwendet, um einen Ablauf ohne Benutzereingabe 
  auszuführen. Darf nur für dafür vorgesehene Abläufe verwendet werden.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_DisableUserPromptCheck
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DISABLE_USERPROMPT_CHECK(pCil)      pCil^.DisableUserPromptCheck $ P_Safety_DisableUserPromptCheck()

FUNCTION __CDECL GLOBAL P_Safety_DisableUserPromptCheck2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DISABLE_USERPROMPT_CHECK2(pCil,p1)  pCil^.DisableUserPromptCheck2 $ P_Safety_DisableUserPromptCheck2(p1)

(*
  Übernimmt die vom Anwender eingegebene Sicherheitsnummer der Ziel-Safe-CPU.
  Beim Verbindungsaufbau zur Safe-CPU wird geprüft, ob diese Sicherheitsnummer 
  mit der Sicherheitsnummer in der Safe-CPU übereinstimmt.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und die 
  Sicherheitsnummer muss vom Benutzer angeforder werdem. Nach dem Aufruf von 
  SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 1 bis 60 Sekunden 
  stattfinden.
  
  Setzt Connection-State auf SAFETY_NBR

  Voraussetzung: Connection-State == IDLE

  \param safetyNbr die Sicherheitsnummer
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetSafetyNbr
VAR_INPUT
  safetyNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_SAFETY_NBR(pCil,p1)        pCil^.SetSafetyNbr $ P_Safety_SetSafetyNbr(p1)

FUNCTION __CDECL GLOBAL P_Safety_SetSafetyNbr2
VAR_INPUT
  pState : pVoid;
  safetyNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_SAFETY_NBR2(pCil,p1,p2)        pCil^.SetSafetyNbr2 $ P_Safety_SetSafetyNbr2(p1,p2)

(*
  Stellt eine Kommunikationsverbindung zur Ziel Safe-CPU mit der zuvor angegebenen 
  Sicherheitsnummer her.
  Wenn ein Download-File angegeben worden ist, dann wird für die nicht sichere 
  Netzwerkadresse der Hardware-Pfad aus dem Download-File verwendet, andernfalls 
  werden alle angeschlossenen Safe-CPUs aufgelistet und diejenige mit der 
  passenden Sicherheitsnummer herausgesucht.

  Zu beachten ist, dass bei einer aufrechten Kommunikationsverbindung regelmäßig 
  eine Kommunikation stattfinden muss, sonst wird diese nach einer Timeoutzeit 
  von 10 Sekunden von der Safe-CPU geschlossen.

  Setzt Connection-State auf CONNECTION

  Voraussetzung: Connection-State == SAFETY_NBR

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_OpenConnection
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_OPEN_CONNECTION(pCil)          pCil^.OpenConnection $ P_Safety_OpenConnection()

FUNCTION __CDECL GLOBAL P_Safety_OpenConnection2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_OPEN_CONNECTION2(pCil,p1)          pCil^.OpenConnection2 $ P_Safety_OpenConnection2(p1)

(*
  Stellt eine Kommunikationsverbindung zu der im File gespeicherten Safe-CPU 
  her und fragt die Sicherheitsnummer ab.

  Voraussetzung: File-State >= FS_FILE

  \param pSafetyNbr Zeiger auf die Variable, in der die abgefragte Sicherheitsnummer 
      geschrieben wird
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetSafetyNbr
VAR_INPUT
  pSafetyNbr : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_SAFETY_NBR(pCil,p1)        pCil^.GetSafetyNbr $ P_Safety_GetSafetyNbr(p1)

FUNCTION __CDECL GLOBAL P_Safety_GetSafetyNbr2
VAR_INPUT
  pState : pVoid;
  pSafetyNbr : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_SAFETY_NBR2(pCil,p1,p2)        pCil^.GetSafetyNbr2 $ P_Safety_GetSafetyNbr2(p1,p2)

(*
  Ein- oder Ausloggen auf der Ziel Safe-CPU.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und das  
  Passwort muss vom Benutzer angeforder werdem. Nach dem Aufruf von 
  SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 1 bis 60 Sekunden 
  stattfinden.
  
  Setzt Connection-State auf CS_LOGGED_IN

  Voraussetzung: Connection-State == CS_CONNECTION

  \param level der Login-Level (0=ausloggen, 1=Debug-Level, 2=Configuration-Level)
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_Login
VAR_INPUT
  level : USINT;
  password : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LOGIN(pCil,p1,p2)              pCil^.Login $ P_Safety_Login(p1,p2)

FUNCTION __CDECL GLOBAL P_Safety_Login2
VAR_INPUT
  pState : pVoid;
  level : USINT;
  password : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LOGIN2(pCil,p1,p2,p3)              pCil^.Login2 $ P_Safety_Login2(p1,p2,p3)

(*
  Weist die Safe-CPU an, in den Run-Status 'SERVICE' zu wechseln.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetServiceMode
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_SERVICE_MODE(pCil)         pCil^.SetServiceMode $ P_Safety_SetServiceMode()

FUNCTION __CDECL GLOBAL P_Safety_SetServiceMode2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_SERVICE_MODE2(pCil,p1)         pCil^.SetServiceMode2 $ P_Safety_SetServiceMode2(p1)

(*
  Weist die Safe-CPU an, vorübergehend in den Run-Status 'SERVICE' zu wechseln. 
  Die Applikation wird dadurch neu gestartet.

  Voraussetzung: Connection-State >= CS_CONNECTION

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetTempServiceMode
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_TEMP_SERVICE_MODE(pCil)    pCil^.SetTempServiceMode $ P_Safety_SetTempServiceMode()

FUNCTION __CDECL GLOBAL P_Safety_SetTempServiceMode2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_TEMP_SERVICE_MODE2(pCil,p1)    pCil^.SetTempServiceMode2 $ P_Safety_SetTempServiceMode2(p1)

(*
  Weist die Safe-CPU an, den Run-Status 'SERVICE' zu verlassen.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_LeaveServiceMode
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LEAVE_SERVICE_MODE(pCil)       pCil^.LeaveServiceMode $ P_Safety_LeaveServiceMode()

FUNCTION __CDECL GLOBAL P_Safety_LeaveServiceMode2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LEAVE_SERVICE_MODE2(pCil,p1)       pCil^.LeaveServiceMode2 $ P_Safety_LeaveServiceMode2(p1)

(*
  Überträgt einen Teil des zuvor angegebenen Download-File zur Safe-CPU.
  Vor dem ersten Aufruf muss die Anzahl der bereits übertragenen Bytes 
  (pBytesTransferred) auf 0 gesetzt werden. Die Übertragung ist beendet, 
  sobald die Anzahl der bereits übertragenen Bytes der Anzahl der insgesamt 
  zu übertragenden Bytes (pTransferSize) entspricht.

  Voraussetzung: Connection-State == CS_LOGGED_IN und File-State == FS_FILE

  \param pBytesTransferred [in,out] Zeiger auf die Variable in der die bereits 
      übertragenen Bytes gespeichert sind. Der Wert dieser Variablen muss 
      vor dem ersten Aufruf auf 0 gesetzt werden. Nach dem Aufruf dieser Funktion 
      ist in dieser Variablen die Anzahl der bisher übertragenen Bytes 
      gespeichert.
  \param pBytesTransferred [out] Zeiger auf die Variable in die die Anzahl 
      der insgesamt zu übertragenden Bytes geschrieben wird.
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_DownloadFile
VAR_INPUT
  pBytesTransferred : ^UDINT;
  pTransferSize : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DOWNLOAD_FILE(pCil,p1,p2)      pCil^.DownloadFile $ P_Safety_DownloadFile(p1,p2)

FUNCTION __CDECL GLOBAL P_Safety_DownloadFile2
VAR_INPUT
  pState : pVoid;
  pBytesTransferred : ^UDINT;
  pTransferSize : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DOWNLOAD_FILE2(pCil,p1,p2,p3)      pCil^.DownloadFile2 $ P_Safety_DownloadFile2(p1,p2,p3)

(*
  Bestätigt eine erfolgreiche Übertragung des Download-Files indem der Konfigurations-Status 
  in der Safe-CPU auf 'konfiguriert + nicht-verifiziert + nicht-verteilt' gesetzt wird.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und vom 
  Benutzer muss eine Bestätigung über die erfolgreiche Übertragung angefordert werden. 
  Dabei muss der Projektname, die Revisionsnummer und der Safe-CPU-Name angezeigt werden.
  Nach dem Aufruf von SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 
  1 bis 60 Sekunden stattfinden.
  
  Voraussetzung: Connection-State == CS_LOGGED_IN und File-State == FS_DOWNLOAD

  \param prjName der Name des Projekts, der im Downloadfile gespeichert ist
  \param revNbr die Revisionsnummer des Projekts, die im Downloadfile gespeichert ist
  \param scpuName der Name des Safe-CPU, der im Downloadfile gespeichert ist
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetConfigured
VAR_INPUT
  prjName : ^CHAR;
  revNbr : ^CHAR;
  scpuName : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_CONFIGURED(pCil,p1,p2,p3)  pCil^.SetConfigured $ P_Safety_SetConfigured(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_SetConfigured2
VAR_INPUT
  pState : pVoid;
  prjName : ^CHAR;
  revNbr : ^CHAR;
  scpuName : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_CONFIGURED2(pCil,p1,p2,p3,p4)  pCil^.SetConfigured2 $ P_Safety_SetConfigured2(p1,p2,p3,p4)

(*
  Setzt den Konfigurations-Status in der Safe-CPU auf 'verifiziert'.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und vom 
  Benutzer muss eine Bestätigung über den Verifiziervorgang angefordert werden. 
  Dabei muss der Projektname, die Revisionsnummer und der Safe-CPU-Name angezeigt werden.
  Nach dem Aufruf von SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 
  1 bis 60 Sekunden stattfinden.
  
  Voraussetzung: Connection-State == CS_LOGGED_IN und File-State == FS_FILE

  \param prjName der Name des Projekts, der im Downloadfile gespeichert ist
  \param revNbr die Revisionsnummer des Projekts, die im Downloadfile gespeichert ist
  \param scpuName der Name des Safe-CPU, der im Downloadfile gespeichert ist
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetVerified
VAR_INPUT
  prjName : ^CHAR;
  revNbr : ^CHAR;
  scpuName : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_VERIFIED(pCil,p1,p2,p3)    pCil^.SetVerified $ P_Safety_SetVerified(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_SetVerified2
VAR_INPUT
  pState : pVoid;
  prjName : ^CHAR;
  revNbr : ^CHAR;
  scpuName : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_VERIFIED2(pCil,p1,p2,p3,p4)    pCil^.SetVerified2 $ P_Safety_SetVerified2(p1,p2,p3,p4)

(*
  Setzt den Konfigurations-Status in der Safe-CPU auf 'verifiziert'.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und vom
  Benutzer muss eine Bestätigung über den Verifiziervorgang angefordert werden.
  Dabei muss die Konfigurations-CRC angezeigt werden.
  Nach dem Aufruf von SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von
  1 bis 60 Sekunden stattfinden.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \param cfgCrc Konfigurations-CRC des Projekts, das verifiziert werden soll
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetVerifiedCRC
VAR_INPUT
  cfgCrc : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_VERIFIED_CRC(pCil,p1)    pCil^.SetVerifiedCRC $ P_Safety_SetVerifiedCRC(p1)

FUNCTION __CDECL GLOBAL P_Safety_SetVerifiedCRC2
VAR_INPUT
  pState : pVoid;
  cfgCrc : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_VERIFIED_CRC2(pCil,p1,p2)    pCil^.SetVerifiedCRC2 $ P_Safety_SetVerifiedCRC2(p1,p2)

(*
  Ändert das Passwort auf der Ziel Safe-CPU.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und vom 
  Benutzer muss der Login-Level, das alte und das neue Passwort angefordert werden. 
  Nach dem Aufruf von SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 
  1 bis 60 Sekunden stattfinden.
  
  Voraussetzung: Connection-State >= CS_CONNECTION und File-State >= FS_FILE

  \param level der Login-Level (1=Debug-Level, 2=Configuration-Level)
  \param oldPassword das alte Passwort. Größe 8 Bytes. Wenn das Passwort kleiner 
      als 8 Zeichen ist, muss der Rest mit Leerzeichn aufgefüllt werden
  \param newPassword das neue Passwort. Größe 8 Bytes. Wenn das Passwort kleiner 
      als 8 Zeichen ist, muss der Rest mit Leerzeichn aufgefüllt werden
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_ChangePassword
VAR_INPUT
  level : USINT;
  oldPassword : ^CHAR;
  newPassword : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHANGE_PASSWORD(pCil,p1,p2,p3) pCil^.ChangePassword $ P_Safety_ChangePassword(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_ChangePassword2
VAR_INPUT
  pState : pVoid;
  level : USINT;
  oldPassword : ^CHAR;
  newPassword : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHANGE_PASSWORD2(pCil,p1,p2,p3,p4) pCil^.ChangePassword2 $ P_Safety_ChangePassword2(p1,p2,p3,p4)

(*
  Ändert das Passwort auf der Ziel Safe-CPU.

  Vor dem Aufruf dieser Funktion muss SetUserPromptTime aufgerufen werden und vom 
  Benutzer muss der Login-Level, das alte und das neue Passwort angefordert werden. 
  Nach dem Aufruf von SetUserPromptTime muss der Aufruf dieser Funktion innerhalb von 
  1 bis 60 Sekunden stattfinden.
  
  Voraussetzung: Connection-State >= CS_CONNECTION
  
  Anm.: 
  Der Unterschied zwischen P_Safety_ChangePassword und P_Safety_ChangePasswordNoFile 
  besteht darin, dass bei P_Safety_ChangePassword der File-State >= FS_FILE sein muss, 
  bei P_Safety_ChangePasswordNoFile nicht. D.h. P_Safety_ChangePasswordNoFile 
  benötigt kein vom Safety-Designer exportiertes Projektfile.

  \param level der Login-Level (1=Debug-Level, 2=Configuration-Level)
  \param oldPassword das alte Passwort. Größe 8 Bytes. Wenn das Passwort kleiner 
      als 8 Zeichen ist, muss der Rest mit Leerzeichn aufgefüllt werden
  \param newPassword das neue Passwort. Größe 8 Bytes. Wenn das Passwort kleiner 
      als 8 Zeichen ist, muss der Rest mit Leerzeichn aufgefüllt werden
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_ChangePasswordNoFile
VAR_INPUT
  level : USINT;
  oldPassword : ^CHAR;
  newPassword : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHANGE_PASSWORD_NOFILE(pCil,p1,p2,p3) pCil^.ChangePasswordNoFile $ P_Safety_ChangePasswordNoFile(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_ChangePasswordNoFile2
VAR_INPUT
  pState : pVoid;
  level : USINT;
  oldPassword : ^CHAR;
  newPassword : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHANGE_PASSWORD_NOFILE2(pCil,p1,p2,p3,p4) pCil^.ChangePasswordNoFile2 $ P_Safety_ChangePasswordNoFile2(p1,p2,p3,p4)

(*
  Liefert den Run-Status der Safe-CPU.

  Voraussetzung: Connection-State >= CS_CONNECTION

  \param pRunState Zeiger auf eine Variable, in die der Run-Status geschrieben wird.
    Werte:
       1 = POST
       2 = SERVICE
       4 = ERROR
       8 = IDLE
      16 = CHK_CFG
      32 = OP_TEMP
      64 = OP
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetRunState
VAR_INPUT
  pRunState : ^USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_RUNSTATE(pCil,p1)          pCil^.GetRunState $ P_Safety_GetRunState(p1)

FUNCTION __CDECL GLOBAL P_Safety_GetRunState2
VAR_INPUT
  pState : pVoid;
  pRunState : ^USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_RUNSTATE2(pCil,p1,p2)          pCil^.GetRunState2 $ P_Safety_GetRunState2(p1,p2)

(*
  Liefert den Konfigurations-Status der Safe-CPU.

  Voraussetzung: Connection-State >= CS_CONNECTION

  \param pCfgState Zeiger auf eine Variable, in die der Konfigurations-Status geschrieben wird.
    Werte:
       1 = INVALID
       2 = NOT_CONFIGURED
       4 = CONFIGURED_NOT_DEPLOYED_NOT_VERIFIED
       8 = CONFIGURED_AND_VERIFIED
      16 = CONFIGURED_DEPLOYED_NOT_VERIFIED
      36 = CONFIGURED_NOT_DEPLOYED_NOT_VERIFIED_DEV
      48 = CONFIGURED_DEPLOYED_NOT_VERIFIED_DEV
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetCfgState
VAR_INPUT
  pCfgState : ^USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_CFGSTATE(pCil,p1)          pCil^.GetCfgState $ P_Safety_GetCfgState(p1)

FUNCTION __CDECL GLOBAL P_Safety_GetCfgState2
VAR_INPUT
  pState : pVoid;
  pCfgState : ^USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_CFGSTATE2(pCil,p1,p2)          pCil^.GetCfgState2 $ P_Safety_GetCfgState2(p1,p2)

(*
  Schickt einen Quit-Error Befehl an die Safe-CPU.

  Voraussetzung: Connection-State >= CS_CONNECTION

  \param quitRemoteModules 
      0=es wird nur der Fehler im angesprochenen Modul quittiert
    <>0=die Safe-CPU schickt zusätzlich an die entfernten Module einen Quit-Error Befehl
      Anm.: Dieser Parameter wird in der Safe-CPU erst ab Version 337 berücksichtigt
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_QuitError
VAR_INPUT
  quitRemoteModules : USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_QUIT_ERROR(pCil,p1)    pCil^.QuitError $ P_Safety_QuitError(p1)

FUNCTION __CDECL GLOBAL P_Safety_QuitError2
VAR_INPUT
  pState : pVoid;
  quitRemoteModules : USINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_QUIT_ERROR2(pCil,p1,p2)    pCil^.QuitError2 $ P_Safety_QuitError2(p1,p2)

(*
 * Setzt den Pfad für das Image.
 * \param imagepath Dateipfad des Images
 * \ return 0 wenn kein Fehler, sonst Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Set_Image_Fw
VAR_INPUT
  pImagePath : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_IMAGE_FW(pCil,p1)    pCil^.SetImageFw $ P_Set_Image_Fw(p1)

FUNCTION __CDECL GLOBAL P_Set_Image_Fw2
VAR_INPUT
  pState : pVoid;
  pImagePath : ^CHAR;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_IMAGE_FW2(pCil,p1,p2)    pCil^.SetImageFw2 $ P_Set_Image_Fw2(p1,p2)

(*
  Prüft, ob eine SCPU mit Dongle im System enthalten ist.
  \param  safeyNbr Sicherheitsnummer des Moduls mit dem Dongle
  \return 0 wenn kein Fehler und Dongle gefunden, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Check_Dongle_FW
VAR_INPUT
  safetyNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHECK_DONGLE_FW(pCil,p1)    pCil^.CheckDongleFW $ P_Check_Dongle_FW(p1)

FUNCTION __CDECL GLOBAL P_Check_Dongle_FW2
VAR_INPUT
  pState : pVoid;
  safetyNbr : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHECK_DONGLE_FW2(pCil,p1,p2)    pCil^.CheckDongleFW2 $ P_Check_Dongle_FW2(p1,p2)

(*
  Firmware Download.
  Überträgt einen Teil des zuvor angegebenen FW-Image zur Safe-CPU.
  Vor dem ersten Aufruf muss die Anzahl der bereits übertragenen Bytes 
  (pBytesTransferred) auf 0 gesetzt werden. Die Übertragung ist beendet, 
  sobald die Anzahl der bereits übertragenen Bytes der Anzahl der insgesamt 
  zu übertragenden Bytes (pTransferSize) entspricht.

  Voraussetzung: Connection-State == CS_LOGGED_IN und File-State == FW_FILE

  \param pBytesTransferred [in,out] Zeiger auf die Variable in der die bereits 
      übertragenen Bytes gespeichert sind. Der Wert dieser Variablen muss 
      vor dem ersten Aufruf auf 0 gesetzt werden. Nach dem Aufruf dieser Funktion 
      ist in dieser Variablen die Anzahl der bisher übertragenen Bytes 
      gespeichert.
  \param pTransferSize [out] Zeiger auf die Variable in die die Anzahl 
      der insgesamt zu übertragenden Bytes geschrieben wird.
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Download_FW
VAR_INPUT
  pBytesTransferred : ^UDINT;
  pTransferSize : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DOWNLOAD_FW(pCil,p1,p2)    pCil^.DownloadFW $ P_Download_FW(p1,p2)

FUNCTION __CDECL GLOBAL P_Download_FW2
VAR_INPUT
  pState : pVoid;
  pBytesTransferred : ^UDINT;
  pTransferSize : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_DOWNLOAD_FW2(pCil,p1,p2,p3)    pCil^.DownloadFW2 $ P_Download_FW2(p1,p2,p3)

(*
  Liefert die im Image-File gespeicherte Modul-ID

  Voraussetzung: File-State >= FW_FILE

  \param pModID [out] Buffer in den die Modul-ID geschrieben wird
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Get_Image_Modul_ID_Fw
VAR_INPUT
  pModID : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_IMAGE_MOD_ID_FW(pCil,p1)    pCil^.GetImageModulIdFW $ P_Get_Image_Modul_ID_Fw(p1)

FUNCTION __CDECL GLOBAL P_Get_Image_Modul_ID_Fw2
VAR_INPUT
  pState : pVoid;
  pModID : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_IMAGE_MOD_ID_FW2(pCil,p1,p2)    pCil^.GetImageModulIdFW2 $ P_Get_Image_Modul_ID_Fw2(p1,p2)

(*
  Liefert die im Image-File gespeicherte Minor Version

  Voraussetzung: File-State >= FW_FILE

  \param pVersion [out] Buffer in den die Minor Version geschrieben wird
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Get_Image_Version_Fw
VAR_INPUT
  pVersion : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_IMAGE_VERSION_FW(pCil,p1)    pCil^.GetImageVersionFW $ P_Get_Image_Version_Fw(p1)

FUNCTION __CDECL GLOBAL P_Get_Image_Version_Fw2
VAR_INPUT
  pState : pVoid;
  pVersion : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_IMAGE_VERSION_FW2(pCil,p1,p2)    pCil^.GetImageVersionFW2 $ P_Get_Image_Version_Fw2(p1,p2)

#pragma pack (push, 1)
TYPE
  SAFETY_MODUL_VERSION : STRUCT
    minorVersion : UDINT;
    majorVersion : UINT;
    modType      : UINT;
    bcVersion    : UDINT;
  END_STRUCT;
END_TYPE
#pragma pack (pop)

(*
  Liefert die Version des Safety-Moduls

  Voraussetzung: File-State >= FW_FILE

  \param pVersion [out] Buffer in den die Minor Version geschrieben wird
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Get_Modul_Version_Fw
VAR_INPUT
  pVersion : ^SAFETY_MODUL_VERSION;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_MODUL_VERSION_FW(pCil,p1)    pCil^.GetModulVersionFW $ P_Get_Modul_Version_Fw(p1)

FUNCTION __CDECL GLOBAL P_Get_Modul_Version_Fw2
VAR_INPUT
  pState : pVoid;
  pVersion : ^SAFETY_MODUL_VERSION;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_MODUL_VERSION_FW2(pCil,p1,p2)    pCil^.GetModulVersionFW2 $ P_Get_Modul_Version_Fw2(p1,p2)

(*
  Löscht die Konfiguration von einem Safety-Modul.

  Man muss mindestens mit dem it dem Konfig-Level eingeloggt sein.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_ClearConfig
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CLEAR_CONFIG(pCil)    pCil^.ClearConfig $ P_Safety_ClearConfig()

FUNCTION __CDECL GLOBAL P_Safety_ClearConfig2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CLEAR_CONFIG2(pCil,p1)    pCil^.ClearConfig2 $ P_Safety_ClearConfig2(p1)

(*
  Wechselt in den Parametrier-Modus.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetParaMode
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_PARA_MODE(pCil)    pCil^.SetParaMode $ P_Safety_SetParaMode()

FUNCTION __CDECL GLOBAL P_Safety_SetParaMode2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SET_PARA_MODE2(pCil,p1)    pCil^.SetParaMode2 $ P_Safety_SetParaMode2(p1)

(*
  Liefert die Info, ob sich das Modul im Parametriermodus befindet und wie lange dieser noch
  aktiv ist

  \param remainingTime_sec Wird beschrieben mit der verbleibenden Zeit im Parametriermodus
  	  in Sekunden (0 = Parametriermodus nicht oder nicht mehr aktiv)
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetParaMode
VAR_INPUT
  remainingTime_sec : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_PARA_MODE(pCil,p1)    pCil^.GetParaMode $ P_Safety_GetParaMode(p1)

FUNCTION __CDECL GLOBAL P_Safety_GetParaMode2
VAR_INPUT
  pState : pVoid;
  remainingTime_sec : ^UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_GET_PARA_MODE2(pCil,p1,p2)    pCil^.GetParaMode2 $ P_Safety_GetParaMode2(p1,p2)

(*
  Verläßt in den Parametrier-Modus.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_LeaveParaMode
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LEAVE_PARA_MODE(pCil)    pCil^.LeaveParaMode $ P_Safety_LeaveParaMode()

FUNCTION __CDECL GLOBAL P_Safety_LeaveParaMode2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_LEAVE_PARA_MODE2(pCil,p1)    pCil^.LeaveParaMode2 $ P_Safety_LeaveParaMode2(p1)

(*
  Stoßt eine Überprüfung der temporären Parameterliste an. Als Parameter muss die CRC der
  gesamten Parameterliste übertragen werden, welche anschließend von der Safety-FW überprüft
  wird.

  \param crc CRC der gesamten Parameterliste
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_CheckPara
VAR_INPUT
  crc : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHECK_PARA(pCil, p1)    pCil^.CheckPara $ P_Safety_CheckPara(p1)

FUNCTION __CDECL GLOBAL P_Safety_CheckPara2
VAR_INPUT
  pState : pVoid;
  crc : UDINT;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_CHECK_PARA2(pCil, p1,p2)    pCil^.CheckPara2 $ P_Safety_CheckPara2(p1,p2)

(*
  Mit diesem Befehl können die Daten aus der temporären Parameterliste in die aktive Parameterliste
  umkopiert werden. Das Umkopieren funktioniert allerdings nur dann, wenn die Daten
  in der temporären Parameterliste zuvor überprüft bzw. wiederhergestellt worden sind.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SwitchPara
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SWITCH_PARA(pCil)    pCil^.SwitchPara $ P_Safety_SwitchPara()

FUNCTION __CDECL GLOBAL P_Safety_SwitchPara2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SWITCH_PARA2(pCil,p1)    pCil^.SwitchPara2 $ P_Safety_SwitchPara2(p1)

(*
  Löst ein Zurücksetzen der temporären Parameterliste aus. Diese wird mit den Daten aus
  dem externen Flash überschrieben und anschließend als „geprüft“ markiert.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_RestorePara
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_RESTORE_PARA(pCil)    pCil^.RestorePara $ P_Safety_RestorePara()

FUNCTION __CDECL GLOBAL P_Safety_RestorePara2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_RESTORE_PARA2(pCil,p1)    pCil^.RestorePara2 $ P_Safety_RestorePara2(p1)

(*
  Beim Absetzen dieses Befehls wird die aktive Parameterliste ins externe Flash geschrieben
  wodurch die Parameterliste dauerhaft gespeichert ist.

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SavePara
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SAVE_PARA(pCil)    pCil^.SavePara $ P_Safety_SavePara()

FUNCTION __CDECL GLOBAL P_Safety_SavePara2
VAR_INPUT
  pState : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_SAVE_PARA2(pCil,p1)    pCil^.SavePara2 $ P_Safety_SavePara2(p1)

(*
  Befehl zum Schreiben von Daten in den virtuellen Speicherbereich eines Safety-Moduls

  \param addr virtuelle Adresse des Speicherbereichs
  \param len Anzahl der zu schreibenden Bytes
  \param pData Zeiger auf die zu schreibenden Daten
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_Write
VAR_INPUT
  addr : UDINT;
  len : USINT;
  pData : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_WRITE(pCil,p1,p2,p3)    pCil^.Write $ P_Safety_Write(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_Write2
VAR_INPUT
  pState : pVoid;
  addr : UDINT;
  len : USINT;
  pData : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_WRITE2(pCil,p1,p2,p3,p4)    pCil^.Write2 $ P_Safety_Write2(p1,p2,p3,p4)

(*
  Befehl zum Lesen von Daten aus dem virtuellen Speicherbereich eines Safety-Moduls

  \param addr virtuelle Adresse des Speicherbereichs
  \param len Anzahl der zu lesenden Bytes
  \param pData Zeiger auf den Buffer, in den die gelesenen Daten geschrieben werden
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_Read
VAR_INPUT
  addr : UDINT;
  len : USINT;
  pData : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_READ(pCil,p1,p2,p3)    pCil^.Read $ P_Safety_Read(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_Read2
VAR_INPUT
  pState : pVoid;
  addr : UDINT;
  len : USINT;
  pData : pVoid;
END_VAR
VAR_OUTPUT
  retval : DINT;
END_VAR;
#define SAFETY_READ2(pCil,p1,p2,p3,p4)    pCil^.Read2 $ P_Safety_Read2(p1,p2,p3,p4)

(*
  Überträgt einen Teil der Parameterliste zur Safe-CPU.
  Vor dem ersten Aufruf muss die Anzahl der bereits übertragenen Bytes
  (pBytesTransferred) auf 0 gesetzt werden.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \param pData Zeiger auf Anfang der Parameterliste
  \param Offset wird zur virtuellen Adresse (Zieladresse) addiert
  \param pBytesTransferred [in,out] Zeiger auf die Variable in der die bereits
      übertragenen Bytes gespeichert sind. Der Wert dieser Variablen muss
      vor dem ersten Aufruf auf 0 gesetzt werden. Nach dem Aufruf dieser Funktion
      ist in dieser Variablen die Anzahl der bisher übertragenen Bytes
      gespeichert.
  \param transferSize Anzahl der insgesamt zu übertragenden Bytes
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_DownloadParaList
VAR_INPUT
    pData 	: ^void;
    Offset 	: UDINT;
    pBytesTransferred 	: ^UDINT;
    transferSize 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_DOWNLOAD_PARA_LIST(pCil,p1,p2,p3,p4)      pCil^.DownloadParaList $ P_Safety_DownloadParaList(p1,p2,p3,p4)

FUNCTION __CDECL GLOBAL P_Safety_DownloadParaList2
VAR_INPUT
    pState : pVoid;
    pData 	: ^void;
    Offset 	: UDINT;
    pBytesTransferred 	: ^UDINT;
    transferSize 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_DOWNLOAD_PARA_LIST2(pCil,p1,p2,p3,p4,p5)      pCil^.DownloadParaList2 $ P_Safety_DownloadParaList2(p1,p2,p3,p4,p5)

(*
  Liest einen Teil der Parameterliste von der Safe-CPU.
  Vor dem ersten Aufruf muss die Anzahl der bereits übertragenen Bytes
  (pBytesTransferred) auf 0 gesetzt werden.

  \param pData Zeiger auf Anfang der Parameterliste
  \param Offset wird zur virtuellen Adresse (Zieladresse) addiert
  \param pBytesTransferred [in,out] Zeiger auf die Variable in der die bereits
      übertragenen Bytes gespeichert sind. Der Wert dieser Variablen muss
      vor dem ersten Aufruf auf 0 gesetzt werden. Nach dem Aufruf dieser Funktion
      ist in dieser Variablen die Anzahl der bisher übertragenen Bytes
      gespeichert.
  \param transferSize Anzahl der insgesamt zu übertragenden Bytes
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_UploadParaList
VAR_INPUT
    pData 	: ^void;
    Offset 	: UDINT;
    pBytesTransferred 	: ^UDINT;
    transferSize 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_UPLOAD_PARA_LIST(pCil,p1,p2,p3,p4)      pCil^.UploadParaList $ P_Safety_UploadParaList(p1,p2,p3,p4)

FUNCTION __CDECL GLOBAL P_Safety_UploadParaList2
VAR_INPUT
    pState : pVoid;
    pData 	: ^void;
    Offset 	: UDINT;
    pBytesTransferred 	: ^UDINT;
    transferSize 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_UPLOAD_PARA_LIST2(pCil,p1,p2,p3,p4,p5)      pCil^.UploadParaList2 $ P_Safety_UploadParaList2(p1,p2,p3,p4,p5)

(*
  Liest den Header der Parameterliste von der Safe-CPU.

  \param pCRC [out] wird mit der CRC aus dem Header beschrieben
  \param pSize [out] wird mit der Größe aus dem Header beschrieben
  \param pID [out] wird mit der ID aus dem Header beschrieben
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetParaListHeader
VAR_INPUT
    pCRC 	: ^UDINT;
    pSize 	: ^UDINT;
    pID 	: ^UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_GET_PARA_LIST_HEADER(pCil,p1,p2,p3)      pCil^.GetParaListHeader $ P_Safety_GetParaListHeader(p1,p2,p3)

FUNCTION __CDECL GLOBAL P_Safety_GetParaListHeader2
VAR_INPUT
    pState : pVoid;
    pCRC 	: ^UDINT;
    pSize 	: ^UDINT;
    pID 	: ^UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_GET_PARA_LIST_HEADER2(pCil,p1,p2,p3,p4)      pCil^.GetParaListHeader2 $ P_Safety_GetParaListHeader2(p1,p2,p3,p4)

(*
  Überträgt die CRC der Parameterliste zur Safe-CPU.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \param CRC die zu übertragende CRC
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_DownloadParaListCRC
VAR_INPUT
    CRC 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_DOWNLOAD_PARA_LIST_CRC(pCil,p1)      pCil^.DownloadParaListCRC $ P_Safety_DownloadParaListCRC(p1)
    
FUNCTION __CDECL GLOBAL P_Safety_DownloadParaListCRC2
VAR_INPUT
    pState : pVoid;
    CRC 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_DOWNLOAD_PARA_LIST_CRC2(pCil,p1,p2)      pCil^.DownloadParaListCRC2 $ P_Safety_DownloadParaListCRC2(p1,p2)
    
(*
  Schickt einen Rescan-All Befehl an die Safe-CPU.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_RescanAll
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_RESCANALL(pCil)                   pCil^.RescanAll $ P_Safety_RescanAll()
    
FUNCTION __CDECL GLOBAL P_Safety_RescanAll2
VAR_INPUT
    pState : pVoid;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_RESCANALL2(pCil,p1)                   pCil^.RescanAll2 $ P_Safety_RescanAll2(p1)
    
(*
  Schickt einen Rescan-One Befehl an die Safe-CPU.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \param extModsafetyNbr Safety-ID des ausgewählten Erweiterungsmoduls
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_RescanOne
VAR_INPUT
    extModsafetyNbr 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_RESCANONE(pCil,p1)                pCil^.RescanOne $ P_Safety_RescanOne(p1)
    
FUNCTION __CDECL GLOBAL P_Safety_RescanOne2
VAR_INPUT
    pState : pVoid;
    extModsafetyNbr 	: UDINT;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_RESCANONE2(pCil,p1,p2)                pCil^.RescanOne2 $ P_Safety_RescanOne2(p1,p2)
    
(*
  Schickt einen SetVerified-Rescan Befehl an die Safe-CPU.

  Voraussetzung: Connection-State == CS_LOGGED_IN

  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_SetVerifiedRescan
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_SET_VERIFIED_RESCAN(pCil)         pCil^.SetVerifiedRescan $ P_Safety_SetVerifiedRescan()
    
FUNCTION __CDECL GLOBAL P_Safety_SetVerifiedRescan2
VAR_INPUT
    pState : pVoid;
END_VAR
VAR_OUTPUT
    Retcode 	: DINT;
END_VAR;
#define SAFETY_SET_VERIFIED_RESCAN2(pCil,p1)         pCil^.SetVerifiedRescan2 $ P_Safety_SetVerifiedRescan2(p1)
    
(*
  Liest die Anzahl der im Infoflash gespeicherten entfernten Module von der Safe-CPU.

  \param pCRC [out] wird mit Anzahl der entfernten Module beschrieben
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetRemoteModulNo
VAR_INPUT
    pNo : ^UDINT;
END_VAR
VAR_OUTPUT
    Retcode : DINT;
END_VAR;
#define SAFETY_GET_REMOTE_MODUL_NO(pCil,p1)      pCil^.GetRemoteModulNo $ P_Safety_GetRemoteModulNo(p1)

FUNCTION __CDECL GLOBAL P_Safety_GetRemoteModulNo2
VAR_INPUT
    pState : pVoid;
    pNo : ^UDINT;
END_VAR
VAR_OUTPUT
    Retcode : DINT;
END_VAR;
#define SAFETY_GET_REMOTE_MODUL_NO2(pCil,p1,p2)      pCil^.GetRemoteModulNo2 $ P_Safety_GetRemoteModulNo2(p1,p2)

(*
  Liest einen Teil der Liste mit den Informationen der entfernten Module von der Safe-CPU.
  Vor dem ersten Aufruf muss die Anzahl der bereits übertragenen Bytes
  (pBytesTransferred) auf 0 gesetzt werden.

  \param pData Zeiger auf Anfang der Liste der entfernten Module
  \param Offset wird zur virtuellen Adresse (Zieladresse) addiert
  \param pBytesTransferred [in,out] Zeiger auf die Variable in der die bereits
      übertragenen Bytes gespeichert sind. Der Wert dieser Variablen muss
      vor dem ersten Aufruf auf 0 gesetzt werden. Nach dem Aufruf dieser Funktion
      ist in dieser Variablen die Anzahl der bisher übertragenen Bytes
      gespeichert.
  \param transferSize Anzahl der insgesamt zu übertragenden Bytes
  \return 0 wenn kein Fehler, sonst der Fehlercode
*)
FUNCTION __CDECL GLOBAL P_Safety_GetRemoteModulList
VAR_INPUT
    pData : ^void;
    Offset : UDINT;
    pBytesTransferred : ^UDINT;
    transferSize : UDINT;
END_VAR
VAR_OUTPUT
    Retcode : DINT;
END_VAR;
#define SAFETY_GET_REMOTE_MODUL_LIST(pCil,p1,p2,p3,p4)      pCil^.GetRemoteModulList $ P_Safety_GetRemoteModulList(p1,p2,p3,p4)

FUNCTION __CDECL GLOBAL P_Safety_GetRemoteModulList2
VAR_INPUT
    pState : pVoid;
    pData : ^void;
    Offset : UDINT;
    pBytesTransferred : ^UDINT;
    transferSize : UDINT;
END_VAR
VAR_OUTPUT
    Retcode : DINT;
END_VAR;
#define SAFETY_GET_REMOTE_MODUL_LIST2(pCil,p1,p2,p3,p4,p5)      pCil^.GetRemoteModulList2 $ P_Safety_GetRemoteModulList2(p1,p2,p3,p4,p5)

    
#pragma pack (push, 1)
TYPE
  OS_SAFETY_DLL : STRUCT
    version : UDINT;
    NewState : pVoid;
    DeleteState : pVoid;
    SetFile : pVoid;
    FileGetPrjName : pVoid;
    FileGetRevNbr : pVoid;
    FileGetScpuName : pVoid;
    SetUserPromptTime : pVoid;
    SetSafetyNbr : pVoid;
    OpenConnection : pVoid;
    Login : pVoid;
    SetServiceMode : pVoid;
    LeaveServiceMode : pVoid;
    DownloadFile : pVoid;
    SetConfigured : pVoid;
    SetVerified : pVoid;
    ChangePassword : pVoid;
    GetRunState : pVoid;
    GetCfgState : pVoid;
    // since version 2:
    GetSafetyNbr : pVoid;
    // since version 3:
    SetTempServiceMode : pVoid;
    // since version 4:
    QuitError : pVoid;
    // since version 5:
    DisableUserPromptCheck : pVoid;
    // since version 7
    SetImageFw : pVoid;
    CheckDongleFW : pVoid;
    DownloadFW : pVoid;
    GetImageModulIdFW : pVoid;
    GetImageVersionFW : pVoid;
    GetModulVersionFW : pVoid;
    // since version 8
    ClearConfig : pVoid;
    // since version 9
    SetParaMode : pVoid;
    GetParaMode : pVoid;
    LeaveParaMode : pVoid;
    CheckPara : pVoid;
    SwitchPara : pVoid;
    RestorePara : pVoid;
    SavePara : pVoid;
    Write : pVoid;
    Read : pVoid;
    DownloadParaList : pVoid;
    UploadParaList : pVoid;
    GetParaListHeader : pVoid;
    DownloadParaListCRC : pVoid;
    // since version 10
    RescanAll : pVoid;
    RescanOne : pVoid;
    SetVerifiedRescan : pVoid;
    // since version 11
    GetRemoteModulNo : pVoid;
    GetRemoteModulList : pVoid;
    // since version 13
    SetVerifiedCRC : pVoid;
    ChangePasswordNoFile : pVoid;
  END_STRUCT;
END_TYPE
TYPE
  OS_SAFETY_DLL2 : STRUCT
    version : UDINT;
    AllocState2 : pVoid;
    FreeState2 : pVoid;
    SetFile2 : pVoid;
    FileGetPrjName2 : pVoid;
    FileGetRevNbr2 : pVoid;
    FileGetScpuName2 : pVoid;
    SetUserPromptTime2 : pVoid;
    SetSafetyNbr2 : pVoid;
    OpenConnection2 : pVoid;
    Login2 : pVoid;
    SetServiceMode2 : pVoid;
    LeaveServiceMode2 : pVoid;
    DownloadFile2 : pVoid;
    SetConfigured2 : pVoid;
    SetVerified2 : pVoid;
    ChangePassword2 : pVoid;
    GetRunState2 : pVoid;
    GetCfgState2 : pVoid;
    // since version 2:
    GetSafetyNbr2 : pVoid;
    // since version 3:
    SetTempServiceMode2 : pVoid;
    // since version 4:
    QuitError2 : pVoid;
    // since version 5:
    DisableUserPromptCheck2 : pVoid;
    // since version 7
    SetImageFw2 : pVoid;
    CheckDongleFW2 : pVoid;
    DownloadFW2 : pVoid;
    GetImageModulIdFW2 : pVoid;
    GetImageVersionFW2 : pVoid;
    GetModulVersionFW2 : pVoid;
    // since version 8
    ClearConfig2 : pVoid;
    // since version 9
    SetParaMode2 : pVoid;
    GetParaMode2 : pVoid;
    LeaveParaMode2 : pVoid;
    CheckPara2 : pVoid;
    SwitchPara2 : pVoid;
    RestorePara2 : pVoid;
    SavePara2 : pVoid;
    Write2 : pVoid;
    Read2 : pVoid;
    DownloadParaList2 : pVoid;
    UploadParaList2 : pVoid;
    GetParaListHeader2 : pVoid;
    DownloadParaListCRC2 : pVoid;
    // since version 10
    RescanAll2 : pVoid;
    RescanOne2 : pVoid;
    SetVerifiedRescan2 : pVoid;
    // since version 11
    GetRemoteModulNo2 : pVoid;
    GetRemoteModulList2 : pVoid;
    // since version 13
    SetVerifiedCRC2 : pVoid;
    ChangePasswordNoFile2 : pVoid;
  END_STRUCT;
END_TYPE
#pragma pack (pop)

//HFILTER:1
#endif
//HFILTER:
