
#ifndef ECAT_MASTER_BASE_IF_H
#define  ECAT_MASTER_BASE_IF_H

//*****************************************************************************
//** DEFINES                                                                 **
//*****************************************************************************


//*****************************************************************************
//** TYPES                                                                   **
//*****************************************************************************

  TYPE

    t_e_ECAT_STATE :
    (
      ECAT_STATE_NONE:=0x00,
      ECAT_STATE_INIT:=0x01,
      ECAT_STATE_PREOP:=0x02,
      ECAT_STATE_BOOT:=0x03,
      ECAT_STATE_SAFEOP:=0x04,
      ECAT_STATE_OP:=0x08
    )$UDINT;

  END_TYPE

#endif
