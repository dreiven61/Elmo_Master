# Chapter 12 Data Recording

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1207-1237
- Chunk: `047_p1207-p1237_Chapter-12-Data-Recording.md`

## Active Outline At Chunk Start
- p. 1207 - Chapter 12 Data Recording
  - p. 1207 - 12.1 Triggering a Recording

## Contained Bookmark Outline
- p. 1207 - Chapter 12 Data Recording
  - p. 1207 - 12.1 Triggering a Recording
  - p. 1208 - 12.2 Active Range Support
  - p. 1208 - 12.3 Using Data Recording in the Maestro
    - p. 1208 - 12.3.1 Excluding Triggers
    - p. 1209 - 12.3.2 Including Triggers
    - p. 1210 - 12.4.1 Recording Data Signals Bitmask Definitions
    - p. 1210 - 12.4.2 Recording Parameters
    - p. 1211 - 12.4.3 Recording Signal Parameters
    - p. 1220 - 12.4.4 Trigger Modes
  - p. 1222 - 12.5 Data Recording Functions
    - p. 1222 - 12.5.1 MMC_BeginRecording
    - p. 1226 - 12.5.2 MMC_StopRecording
    - p. 1228 - 12.5.3 MMC_UploadData
    - p. 1231 - 12.5.4 MMC_RecStatus
    - p. 1234 - 12.5.5 MMC_UploadDataHeader

## Extracted Text

### PDF page 1207
<a id="pdf-page-1207"></a>
#### Chapter 12 Data Recording
##### 12.1 Triggering a Recording
Chapter 12 Data Recording
Data recording is a powerful feature of the Maestro that allows the user to record internal controller variables,
store them in local a temporary array, and upload them to a host computer using either one of the controller's
communication channels.
[PDF field-code object omitted] Caution: This is an advanced API option. Care
should be taken when using data recording. It should only be used when the Ma estro
parameters have been set and the servo drivers functioning correctly. Only use TCP/IP
communication to perform the data recording.
The Maestro has the following data recording capabilities:
- Simultaneous recording of up to a maximum of 32 bit internal controller signals/variables
- Up to one million data recorded points. The user can select to record up to a maximum of 32 bit vectors
with 31,250 sample points, or for single vectors, one million sample points, or any other combination
- Various recording vectors
- Advanced triggering options
In most situations where the variable is described as a char, short, int, float, etc., each variable can be recorded
as a single vector, and therefore a maximum 32 bit variable is recorded. However, where the variable is a
double, the vector memory space allocated requires two vectors, therefore only allowing a 16 bit variable to be
recorded.
12.1 Triggering a Recording
The Maestro supports advanced trigger options that further enhance the data logging capabilities of the
system, and provide a powerful tool for monitoring and debugging servo applications. In general, Trigger
support refers to the ability of the controller to start a data recording process, and to condition the actual
execution of the data logging based on a specific event. The user can select the event source, type, and
condition, as well as to perform pre-trigger data logging (logging data prior to the trigger event).
The Maestro support numerous trigger event types, generally divided into the following groups:
Group Explanation
Edge Type Triggers Where a change in signal level around some threshold is detected.
Level Type Triggers Where the signal level is detected (not necessarily a change).
Single Level or Window conditions The trigger condition can be defined as a single level or a window
(Min/Max).
Bitwise Masking of signals The trigger condition can be defined by a user defined Bitwise
mask of the requested signal.

### PDF page 1208
<a id="pdf-page-1208"></a>
##### 12.2 Active Range Support
##### 12.3 Using Data Recording in the Maestro
###### 12.3.1 Excluding Triggers
12.2 Active Range Support
The fast upload data rate supported via the TCP/IP link, and double buffering capability, allows the host
computer to display data recording in an active range, with all of the triggering options. There are four data
recording types:
Type Explanation
AUTO The data recording is performed with no triggering, but is cyclic (t he double buffering
recording mechanism is used).
SINGLE The data recording is performed with triggers, but is not cyclic. The trigger is searched for,
once only, and then the recording ends.
NOTRIGGER The data recording is performed with no triggering, and is not cyclic (The double buffering
recording mechanism is not used).
NORMAL The data recording is performed with triggering, and is cyclic (the double buffering recording
mechanism is used).
12.3 Using Data Recording in the Maestro
The Maestro supports Data Recording using the following keywords:
Keywords Variable Command
Begin / Stop Data Recording command MMC_BeginRecordingCmd
Data Recording Configuration Parameters: MMC_BEGIN_RECORDING_IN
- Select Recorded variables parameter uiRp
- Select Recording Length parameter uiRl
- Select Recording GAP parameter uiRg
Report Recording Status command
Rest Recording Index

uiRr
MMC_RecStatusCmd
Uploading the array
From/To/Buffered index
uiFrom
uiTo
uiBufIdx
MMC_UploadDataCmd
Triggering Options
Recorder Trigger Status

uiSr

Data Recording An array
12.3.1 Excluding Triggers
When normal data recording without triggers is used, the Recording Status variable uiRr is automatically
initialized by the Begin Recording command, with the user defined Recording Length uiRr =uiRl (number of
recorded points). During the recording process it is decremented, every gap (defined by uiRg) servo samples by
1 until it reaches 0 (Rr=0). At this point, the data recording is terminated and the data itself can be uploaded.

### PDF page 1209
<a id="pdf-page-1209"></a>
###### 12.3.2 Including Triggers
12.3.2 Including Triggers
The ability to perform data recording with Triggers Support general depends on the capability of the controller
to initiate a data recording process, and to condition its actual execution and progress bases on user -defined
events and conditions. For the Pre-Triggering phase, the controller supports data logging before the trigger
event occurred. The Maestro supports pre-trigger buffer length from 0% to 100%.
The Arming (using variable uiSr) state indicates that the controller is in the Pre -Trigger phase, collecting data,
and filling the recording buffer, to the size of pre-trigger size defined. When using triggers, the phase starts
immediately with the Start Recording command. The variable uiRr is initially set to uiRl, and is decremented
until the size of the Pre-Trigger is reached. For example, if uiRl =1000, and the Pre -Trigger is 25% - i.e. 250
points, uiRr will be decremented during the Pre-Trigger state to 750.
The condition whereby the system is waiting for an Inverse Trigger state is present only in Edge Trigger modes,
and indicates that the controller is waiting for the opposite trigger condition, to operate the trigger condition
itself. This is required for Edge or Change Detection. At this phase, data logging is continuously performed in
the internal data recording buffers, but the actual recording is paused. The Recording Status variable uiRr is
unchanged.
The delay for a Trigger Condition state is present in all Trigger modes, to indicate that the controller is waiting
for the trigger event condition. At this phase, data logging is continuously performed in the internal data
recording buffers, but the actual recording is paused. Similarly, the Recording Status variable uiRr is unchanged.
When the Triggered and Recording state is invoked, the trigger event is detected, and normal data recoding
continues. The End of Recording process always completes when the Recording Status Variable reach Zero
value uiRr =0.
When using normal data recording without triggers, the Start of Recording (Begin Recordi ng Command) is
referred to the 0 time-base in the output data buffer shown on the graphic plots. When operating with triggers,
the 0 time-base will depend on the trigger Event condition. The actual Start of Recording time is no longer
relevant, as the trigger event condition can occur long after the Start Recording command was issued. In this
case, the 0 time-base of the Output Buffers refers to the point that is the Pre-Trigger time before the Trigger
Event. For example, if a 1 second recording process is initiated, with a pre-trigger of 50%, the Pre-Trigger point
is always (by definition) at 0.5 second after start.

### PDF page 1210
<a id="pdf-page-1210"></a>
###### 12.4.1 Recording Data Signals Bitmask Definitions
###### 12.4.2 Recording Parameters
12.4 Recording Definitions and Parameters
The following is a list of recording enumerator definitions and parameters.
12.4.1 Recording Data Signals Bitmask Definitions
The Recording Data Signal Bitmask variable uiRc, and ulRc create a mask using the following memory buffers to
store the input parameters.
Bit Mask Parameter Memory used
MC_REC_TRIGGER_TYPE_MASK 0x0000ffff
MC_REC_TRIGGER_PARAM_MASK 0xffff0000
MC_REC_TRIGGERG_STATE_MASK 0x00ff
MC_REC_BUF_STATE_MASK 0xff00
MC_SCOPE_BITS_NONE_BUF_READY 0x000
MC_SCOPE_BITS_BUFFER1_READY 0x100
MC_SCOPE_BITS_BUFFER2_READY 0x200
MC_NORMAL_TRIGGER 0x10000
MC_AUTO_TRIGGER 0x20000
MC_SINGLE_TRIGGER 0x30000
12.4.2 Recording Parameters
The following table lists the Recording Parameters Rp(i).
Recording Parameters ID
TG_RECORDING_SPARE 0
TG_RECORDING_TRIGGER_VALUE 1
TG_RECORDING_PRE_TRIGGER_LENGTH 2
TG_RECORDING_TRIGGER_TYPE 3
TG_RECORDING_TRIGGER_LEVEL_1 4
TG_RECORDING_TRIGGER_LEVEL_2 5
TG_RECORDING_TRIGGER_POLARITY 6
TG_RECORDING_TRIGGER_IN_MASK 7

### PDF page 1211
<a id="pdf-page-1211"></a>
###### 12.4.3 Recording Signal Parameters
12.4.3 Recording Signal Parameters
The following parameters are recording signal variables and their IDs.
Recording Signal Variable ID
NC_REC_DESIRED_POS_LOW_PARAM 0
NC_REC_DESIRED_POS_HIGH_PARAM 1
NC_REC_DESIRED_VEL_LOW_PARAM 2
NC_REC_DESIRED_VEL_HIGH_PARAM 3
NC_REC_GROUP_VEL_LOW_PARAM 4
NC_REC_GROUP_VEL_HIGH_PARAM 5
NC_REC_GROUP_AC_LOW_PARAM 6
NC_REC_GROUP_AC_HIGH_PARAM 7
NC_REC_GROUP_DC_LOW_PARAM 8
NC_REC_GROUP_DC_HIGH_PARAM 9
NC_REC_GROUP_AC_DC_LOW_PARAM 10
NC_REC_GROUP_AC_DC_HIGH_PARAM 11
NC_REC_GROUP_JERK_LOW_PARAM 12
NC_REC_GROUP_JERK_HIGH_PARAM 13
NC_REC_SMOOTH_FACTOR_AC_LOW_PARAM 14
NC_REC_SMOOTH_FACTOR_AC_HIGH_PARAM 15
NC_REC_SMOOTH_FACTOR_DC_LOW_PARAM 16
NC_REC_SMOOTH_FACTOR_DC_HIGH_PARAM 17
NC_REC_POS_INCR_LOW_PARAM 18
NC_REC_POS_INCR_HIGH_PARAM 19
NC_REC_CYCLE_CNT_PARAM 20
NC_REC_TARGET_POS_PARAM 21
NC_REC_TARGET_VEL_PARAM 22
NC_REC_F_POS_PARAM 23
NC_REC_F_VEL_PARAM 24
NC_REC_ACTUAL_POS_PARAM 25
NC_REC_ACTUAL_VEL_PARAM 26
NC_REC_AXIS_STATUS_PARAM 27
NC_REC_MAX_NUM_PARAM 28

### PDF page 1212
<a id="pdf-page-1212"></a>
NC_REC_ACTUAL_POS_PARAM 30
NC_REC_ACTUAL_VEL_PARAM 31
NC_REC_AXIS_STATUS_PARAM 32
NC_REC_ACTUAL_TORQUE_PARAM 33
NC_REC_I_USER_1_PARAM 34
NC_REC_I_USER_AUX_1_PARAM 35
NC_REC_F_USER_1_PARAM 36
NC_REC_F_USER_AUX_1_PARAM 37
NC_REC_I_USER_2_PARAM 38
NC_REC_I_USER_AUX_2_PARAM 39
NC_REC_F_USER_2_PARAM 40
NC_REC_F_USER_AUX_2_PARAM 41
NC_REC_I_USER_3_PARAM 42
NC_REC_I_USER_AUX_3_PARAM 43
NC_REC_F_USER_3_PARAM 44
NC_REC_F_USER_AUX_3_PARAM 45
NC_REC_I_USER_4_PARAM 46
NC_REC_I_USER_AUX_4_PARAM 47
NC_REC_F_USER_4_PARAM 48
NC_REC_F_USER_AUX_4_PARAM 49
NC_REC_POS_FOLLOWING_ERR_PARAM 50
NC_REC_DIGITAL_INPUTS_PARAM 51
NC_REC_DIGITAL_OUTPUTS_PARAM 52
NC_REC_TRACKING_ERROR_LOW_PARAM 53
NC_REC_TRACKING_ERROR_HIGH_PARAM 54
NC_REC_ERROR_CORRECTION_POS_PARAM 55
NC_REC_ACTUAL_HW_POSITION_PARAM 56
NC_REC_CONTROL_WORD_PARAM 57
NC_REC_STATUS_WORD_PARAM 58
NC_REC_MOTION_MODE_PARAM 59
NC_REC_DI_LOW_PARAM 60
NC_REC_DI_HIGH_PARAM 61
NC_REC_DO_LOW_PARAM 62

### PDF page 1213
<a id="pdf-page-1213"></a>
NC_REC_DO_HIGH_PARAM 63
NC_REC_AXIS_COMM_ERROR_PARAM 64
NC_REC_AXIS_LAST_EMCY_CODE_PARAM 65
NC_STATUS_REGISTER 66
NC_MCS_LIMIT_REGISTER 67
NC_REC_DESIRED_PCS_X_POS_LOW_PARAM 68
NC_REC_DESIRED_PCS_X_POS_HIGH_PARAM 69
NC_REC_DESIRED_PCS_Y_POS_LOW_PARAM 70
NC_REC_DESIRED_PCS_Y_POS_HIGH_PARAM 71
NC_REC_DESIRED_PCS_Z_POS_LOW_PARAM 72
NC_REC_DESIRED_PCS_Z_POS_HIGH_PARAM 73
NC_REC_DESIRED_PCS_U_POS_LOW_PARAM 74
NC_REC_DESIRED_PCS_U_POS_HIGH_PARAM 75
NC_REC_DESIRED_PCS_V_POS_LOW_PARAM 76
NC_REC_DESIRED_PCS_V_POS_HIGH_PARAM 77
NC_REC_DESIRED_PCS_W_POS_LOW_PARAM 78
NC_REC_DESIRED_PCS_W_POS_HIGH_PARAM 79
NC_REC_DESIRED_MCS_N1_POS_LOW_PARAM 80
NC_REC_DESIRED_MCS_N1_POS_HIGH_PARAM 81
NC_REC_DESIRED_MCS_N2_POS_LOW_PARAM 82
NC_REC_DESIRED_MCS_N2_POS_HIGH_PARAM 83
NC_REC_DESIRED_MCS_N3_POS_LOW_PARAM 84
NC_REC_DESIRED_MCS_N3_POS_HIGH_PARAM 85
NC_REC_DESIRED_MCS_N4_POS_LOW_PARAM 86
NC_REC_DESIRED_MCS_N4_POS_HIGH_PARAM 87
NC_REC_DESIRED_MCS_N5_POS_LOW_PARAM 88
NC_REC_DESIRED_MCS_N5_POS_HIGH_PARAM 89
NC_REC_DESIRED_MCS_N6_POS_LOW_PARAM 90
NC_REC_DESIRED_MCS_N6_POS_HIGH_PARAM 91
NC_REC_DESIRED_MCS_N7_POS_LOW_PARAM 92
NC_REC_DESIRED_MCS_N7_POS_HIGH_PARAM 93
NC_REC_DESIRED_MCS_N8_POS_LOW_PARAM 94
NC_REC_DESIRED_MCS_N8_POS_HIGH_PARAM 95

### PDF page 1214
<a id="pdf-page-1214"></a>
NC_REC_DESIRED_MCS_N9_POS_LOW_PARAM 96
NC_REC_DESIRED_MCS_N9_POS_HIGH_PARAM 97
NC_REC_DESIRED_MCS_S_POS_LOW_PARAM 98
NC_REC_DESIRED_MCS_S_POS_HIGH_PARAM 99
NC_REC_DESIRED_PCS_X_VEL_LOW_PARAM 100
NC_REC_DESIRED_PCS_X_VEL_HIGH_PARAM 101
NC_REC_DESIRED_PCS_Y_VEL_LOW_PARAM 102
NC_REC_DESIRED_PCS_Y_VEL_HIGH_PARAM 103
NC_REC_DESIRED_PCS_Z_VEL_LOW_PARAM 104
NC_REC_DESIRED_PCS_Z_VEL_HIGH_PARAM 105
NC_REC_DESIRED_PCS_U_VEL_LOW_PARAM 106
NC_REC_DESIRED_PCS_U_VEL_HIGH_PARAM 107
NC_REC_DESIRED_PCS_V_VEL_LOW_PARAM 108
NC_REC_DESIRED_PCS_V_VEL_HIGH_PARAM 109
NC_REC_DESIRED_PCS_W_VEL_LOW_PARAM 110
NC_REC_DESIRED_PCS_W_VEL_HIGH_PARAM 111
NC_REC_DESIRED_MCS_N1_VEL_LOW_PARAM 112
NC_REC_DESIRED_MCS_N1_VEL_HIGH_PARAM 113
NC_REC_DESIRED_MCS_N2_VEL_LOW_PARAM 114
NC_REC_DESIRED_MCS_N2_VEL_HIGH_PARAM 115
NC_REC_DESIRED_MCS_N3_VEL_LOW_PARAM 116
NC_REC_DESIRED_MCS_N3_VEL_HIGH_PARAM 117
NC_REC_DESIRED_MCS_N4_VEL_LOW_PARAM 118
NC_REC_DESIRED_MCS_N4_VEL_HIGH_PARAM 119
NC_REC_DESIRED_MCS_N5_VEL_LOW_PARAM 120
NC_REC_DESIRED_MCS_N5_VEL_HIGH_PARAM 121
NC_REC_DESIRED_MCS_N6_VEL_LOW_PARAM 122
NC_REC_DESIRED_MCS_N6_VEL_HIGH_PARAM 123
NC_REC_DESIRED_MCS_N7_VEL_LOW_PARAM 124
NC_REC_DESIRED_MCS_N7_VEL_HIGH_PARAM 125
NC_REC_DESIRED_MCS_N8_VEL_LOW_PARAM 126
NC_REC_DESIRED_MCS_N8_VEL_HIGH_PARAM 127
NC_REC_DESIRED_MCS_N9_VEL_LOW_PARAM 128

### PDF page 1215
<a id="pdf-page-1215"></a>
NC_REC_DESIRED_MCS_N9_VEL_HIGH_PARAM 129
NC_REC_DESIRED_MCS_S_VEL_LOW_PARAM 130
NC_REC_DESIRED_MCS_S_VEL_HIGH_PARAM 131
NC_REC_DESIRED_PCS_X_AC_DC_LOW_PARAM 132
NC_REC_DESIRED_PCS_X_AC_DC_HIGH_PARAM 133
NC_REC_DESIRED_PCS_Y_AC_DC_LOW_PARAM 134
NC_REC_DESIRED_PCS_Y_AC_DC_HIGH_PARAM 135
NC_REC_DESIRED_PCS_Z_AC_DC_LOW_PARAM 136
NC_REC_DESIRED_PCS_Z_AC_DC_HIGH_PARAM 137
NC_REC_DESIRED_PCS_U_AC_DC_LOW_PARAM 138
NC_REC_DESIRED_PCS_U_AC_DC_HIGH_PARAM 139
NC_REC_DESIRED_PCS_V_AC_DC_LOW_PARAM 140
NC_REC_DESIRED_PCS_V_AC_DC_HIGH_PARAM 141
NC_REC_DESIRED_PCS_W_AC_DC_LOW_PARAM 142
NC_REC_DESIRED_PCS_W_AC_DC_HIGH_PARAM 143
NC_REC_DESIRED_MCS_N1_AC_DC_LOW_PARAM 144
NC_REC_DESIRED_MCS_N1_AC_DC_HIGH_PARAM 145
NC_REC_DESIRED_MCS_N2_AC_DC_LOW_PARAM 146
NC_REC_DESIRED_MCS_N2_AC_DC_HIGH_PARAM 147
NC_REC_DESIRED_MCS_N3_AC_DC_LOW_PARAM 148
NC_REC_DESIRED_MCS_N3_AC_DC_HIGH_PARAM 149
NC_REC_DESIRED_MCS_N4_AC_DC_LOW_PARAM 150
NC_REC_DESIRED_MCS_N4_AC_DC_HIGH_PARAM 151
NC_REC_DESIRED_MCS_N5_AC_DC_LOW_PARAM 152
NC_REC_DESIRED_MCS_N5_AC_DC_HIGH_PARAM 153
NC_REC_DESIRED_MCS_N6_AC_DC_LOW_PARAM 154
NC_REC_DESIRED_MCS_N6_AC_DC_HIGH_PARAM 155
NC_REC_DESIRED_MCS_N7_AC_DC_LOW_PARAM 156
NC_REC_DESIRED_MCS_N7_AC_DC_HIGH_PARAM 157
NC_REC_DESIRED_MCS_N8_AC_DC_LOW_PARAM 158
NC_REC_DESIRED_MCS_N8_AC_DC_HIGH_PARAM 159
NC_REC_DESIRED_MCS_N9_AC_DC_LOW_PARAM 160
NC_REC_DESIRED_MCS_N9_AC_DC_HIGH_PARAM 161

### PDF page 1216
<a id="pdf-page-1216"></a>
NC_REC_DESIRED_MCS_S_AC_DC_LOW_PARAM 162
NC_REC_DESIRED_MCS_S_AC_DC_HIGH_PARAM 163
NC_REC_END_MOTION_REASON_PARAM 164
NC_REC_ANALOG_INPUT_PARAM 165
NC_REC_DESIRED_MCS_X_POS_LOW_PARAM 166
NC_REC_DESIRED_MCS_X_POS_HIGH_PARAM 167
NC_REC_DESIRED_MCS_Y_POS_LOW_PARAM 168
NC_REC_DESIRED_MCS_Y_POS_HIGH_PARAM 169
NC_REC_DESIRED_MCS_Z_POS_LOW_PARAM 170
NC_REC_DESIRED_MCS_Z_POS_HIGH_PARAM 171
NC_REC_DESIRED_MCS_U_POS_LOW_PARAM 172
NC_REC_DESIRED_MCS_U_POS_HIGH_PARAM 173
NC_REC_DESIRED_MCS_V_POS_LOW_PARAM 174
NC_REC_DESIRED_MCS_V_POS_HIGH_PARAM 175
NC_REC_DESIRED_MCS_W_POS_LOW_PARAM 176
NC_REC_DESIRED_MCS_W_POS_HIGH_PARAM 177
NC_REC_DESIRED_ACS_A1_POS_LOW_PARAM 178
NC_REC_DESIRED_ACS_A1_POS_HIGH_PARAM 179
NC_REC_DESIRED_ACS_A2_POS_LOW_PARAM 180
NC_REC_DESIRED_ACS_A2_POS_HIGH_PARAM 181
NC_REC_DESIRED_ACS_A3_POS_LOW_PARAM 182
NC_REC_DESIRED_ACS_A3_POS_HIGH_PARAM 183
NC_REC_DESIRED_ACS_A4_POS_LOW_PARAM 184
NC_REC_DESIRED_ACS_A4_POS_HIGH_PARAM 185
NC_REC_DESIRED_ACS_A5_POS_LOW_PARAM 186
NC_REC_DESIRED_ACS_A5_POS_HIGH_PARAM 187
NC_REC_DESIRED_ACS_A6_POS_LOW_PARAM 188
NC_REC_DESIRED_ACS_A6_POS_HIGH_PARAM 189
NC_REC_DESIRED_MCS_X_VEL_LOW_PARAM 190
NC_REC_DESIRED_MCS_X_VEL_HIGH_PARAM 191
NC_REC_DESIRED_MCS_Y_VEL_LOW_PARAM 192
NC_REC_DESIRED_MCS_Y_VEL_HIGH_PARAM 193
NC_REC_DESIRED_MCS_Z_VEL_LOW_PARAM 194

### PDF page 1217
<a id="pdf-page-1217"></a>
NC_REC_DESIRED_MCS_Z_VEL_HIGH_PARAM 195
NC_REC_DESIRED_MCS_U_VEL_LOW_PARAM 196
NC_REC_DESIRED_MCS_U_VEL_HIGH_PARAM 197
NC_REC_DESIRED_MCS_V_VEL_LOW_PARAM 198
NC_REC_DESIRED_MCS_V_VEL_HIGH_PARAM 199
NC_REC_DESIRED_MCS_W_VEL_LOW_PARAM 200
NC_REC_DESIRED_MCS_W_VEL_HIGH_PARAM 201
NC_REC_DESIRED_ACS_A1_VEL_LOW_PARAM 202
NC_REC_DESIRED_ACS_A1_VEL_HIGH_PARAM 203
NC_REC_DESIRED_ACS_A2_VEL_LOW_PARAM 204
NC_REC_DESIRED_ACS_A2_VEL_HIGH_PARAM 205
NC_REC_DESIRED_ACS_A3_VEL_LOW_PARAM 206
NC_REC_DESIRED_ACS_A3_VEL_HIGH_PARAM 207
NC_REC_DESIRED_ACS_A4_VEL_LOW_PARAM 208
NC_REC_DESIRED_ACS_A4_VEL_HIGH_PARAM 209
NC_REC_DESIRED_ACS_A5_VEL_LOW_PARAM 210
NC_REC_DESIRED_ACS_A5_VEL_HIGH_PARAM 211
NC_REC_DESIRED_ACS_A6_VEL_LOW_PARAM 212
NC_REC_DESIRED_ACS_A6_VEL_HIGH_PARAM 213
NC_REC_DESIRED_MCS_X_AC_DC_LOW_PARAM 214
NC_REC_DESIRED_MCS_X_AC_DC_HIGH_PARAM 215
NC_REC_DESIRED_MCS_Y_AC_DC_LOW_PARAM 216
NC_REC_DESIRED_MCS_Y_AC_DC_HIGH_PARAM 217
NC_REC_DESIRED_MCS_Z_AC_DC_LOW_PARAM 218
NC_REC_DESIRED_MCS_Z_AC_DC_HIGH_PARAM 219
NC_REC_DESIRED_MCS_U_AC_DC_LOW_PARAM 220
NC_REC_DESIRED_MCS_U_AC_DC_HIGH_PARAM 221
NC_REC_DESIRED_MCS_V_AC_DC_LOW_PARAM 222
NC_REC_DESIRED_MCS_V_AC_DC_HIGH_PARAM 223
NC_REC_DESIRED_MCS_W_AC_DC_LOW_PARAM 224
NC_REC_DESIRED_MCS_W_AC_DC_HIGH_PARAM 225
NC_REC_DESIRED_ACS_A1_AC_DC_LOW_PARAM 226
NC_REC_DESIRED_ACS_A1_AC_DC_HIGH_PARAM 227

### PDF page 1218
<a id="pdf-page-1218"></a>
NC_REC_DESIRED_ACS_A2_AC_DC_LOW_PARAM 228
NC_REC_DESIRED_ACS_A2_AC_DC_HIGH_PARAM 229
NC_REC_DESIRED_ACS_A3_AC_DC_LOW_PARAM 230
NC_REC_DESIRED_ACS_A3_AC_DC_HIGH_PARAM 231
NC_REC_DESIRED_ACS_A4_AC_DC_LOW_PARAM 232
NC_REC_DESIRED_ACS_A4_AC_DC_HIGH_PARAM 233
NC_REC_DESIRED_ACS_A5_AC_DC_LOW_PARAM 234
NC_REC_DESIRED_ACS_A5_AC_DC_HIGH_PARAM 235
NC_REC_DESIRED_ACS_A6_AC_DC_LOW_PARAM 236
NC_REC_DESIRED_ACS_A6_AC_DC_HIGH_PARAM 237
NC_REC_SPEED_OVERRIDE_PARAM 238
NC_REC_TARGET_TORQUE_RC_PARAM 239
NC_REC_AUXILARY_POS_PARAM 240
NC_REC_TARGET_TORQUE_UU_LOW_PARAM 241
NC_REC_TARGET_TORQUE_UU_HIGH_PARAM 242
NC_REC_TORQUE_VELOCITY_UU_LOW_PARAM 243
NC_REC_TORQUE_VELOCITY_UU_HIGH_PARAM 244
NC_REC_TORQUE_ACCELERATION_UU_LOW_PARAM 245
NC_REC_TORQUE_ACCELERATION_UU_HIGH_PARAM 246
NC_REC_ACTUAL_POS_UU_LOW_PARAM 247
NC_REC_ACTUAL_POS_UU_HIGH_PARAM 248
NC_REC_D_ACTUAL_POS_CNT_LOW_PARAM 249
NC_REC_D_ACTUAL_POS_CNT_HIGH_PARAM 250
NC_REC_TARGET_POS_UU_LOW_PARAM 251
NC_REC_TARGET_POS_UU_HIGH_PARAM 252
NC_REC_D_TARGET_POS_CNT_LOW_PARAM 253
NC_REC_D_TARGET_POS_CNT_HIGH_PARAM 254
NC_REC_D_ACTUAL_HW_POS_CNT_LOW_PARAM 255
NC_REC_D_ACTUAL_HW_POS_CNT_HIGH_PARAM 256
NC_REC_ACTUAL_VEL_UU_LOW_PARAM 257
NC_REC_ACTUAL_VEL_UU_HIGH_PARAM 258
NC_REC_TARGET_VEL_UU_LOW_PARAM 259
NC_REC_TARGET_VEL_UU_HIGH_PARAM 260

### PDF page 1219
<a id="pdf-page-1219"></a>
NC_REC_D_TARGET_VEL_CNT_LOW_PARAM 261
NC_REC_D_TARGET_VEL_CNT_HIGH_PARAM 262
NC_REC_D_ACDC_CNT_LOW_PARAM 263
NC_REC_D_ACDC_CNT_HIGH_PARAM 264
NC_REC_TARGET_MOD_POS_UU_LOW_PARAM 265
NC_REC_TARGET_MOD_POS_UU_HIGH_PARAM 266
NC_REC_TARGET_POS_TOTAL_UU_LOW_PARAM 267
NC_REC_TARGET_POS_TOTAL_UU_HIGH_PARAM 268
NC_DBG6_LOW_PARAM 269
NC_DBG56_HIGH_PARAM 270
NC_REC_MAX_NUM_PARAM 271

### PDF page 1220
<a id="pdf-page-1220"></a>
###### 12.4.4 Trigger Modes
12.4.4 Trigger Modes
The parameter uiRp controls the trigger value and type. These trigger values consist of high and low bytes. The
uiRp[1] higher 2 bytes control the axis or node ID, wheras the uiRp[3] low 2 bytes may control the motion.
Under these conditions uiRp[1] low 2 bytes should be 0. The uiRp[3] high 2 bytes control the trigger type which
may be of three values as detailed in the section Recording Data Signals Bitmas k Definitions above:
Bit Mask Parameter Memory used
MC_NORMAL_TRIGGER 0x10000
MC_AUTO_TRIGGER 0x20000
MC_SINGLE_TRIGGER 0x30000

uiRp[1] Trigger
value
MS bytes LS bytes
Axis/Node ID Recording Signal
Parameters ID
uiRp ]3 [ Trigger
type
Trigger type
3 options:
MC_NORMAL_TRIGGER 0x10000
MC_AUTO_TRIGGER 0x20000
MC_SINGLE_TRIGGER 0x30000
Trigger modes as per
table of 13 options

The following Trigger modes and their IDs are used in the Recording Data functions.
Parameter ID Explanation
TG_RECORDING_TRIGGER_TYPE_NO_TRIGGER 0 Edge : Rising (Positive Slope Over: TRIGVAL >= Level#1)
TG_RECORDING_TRIGGER_TYPE_EDGE_Rise 1 Edge : Rising (Positive Slope Over: TRIGVAL >= Level#1)
TG_RECORDING_TRIGGER_TYPE_EDGE_Fall 2 Edge : Falling (Negative Slope over: TRIGVAL <= Level#1)
TG_RECORDING_TRIGGER_TYPE_EDGE_WindowIn 3 Edge : Window In (Into the Window defined by: Level#2
<= TRIGVAL <= LEVEL#1)
TG_RECORDING_TRIGGER_TYPE_EDGE_WindowOut 4 Edge : Window Out (Out Of the Window defined by:
Level#2 <= TRIGVAL <= LEVEL#1)
TG_RECORDING_TRIGGER_TYPE_LEVEL_GE 5 Level : >= (GreaterEqual The: TRIGVAL >= Level#1)
TG_RECORDING_TRIGGER_TYPE_LEVEL_SE 6 Level : <= (SmallerEqual Then: TRIGVAL <= Level#1)
TG_RECORDING_TRIGGER_TYPE_LEVEL_WindowInside 7 Level : Inside Window (Inside of Window defined by:
Level#2 <= TRIGVAL <= LEVEL#1)
TG_RECORDING_TRIGGER_TYPE_LEVEL_WindowOutside 8 Level : outside Window (Outside of Window defined by:

### PDF page 1221
<a id="pdf-page-1221"></a>
Level#2 <= TRIGVAL <= LEVEL#1)
TG_RECORDING_TRIGGER_TYPE_EDGE_Rise_Mask 9 Rising-Edge + MASK (Positive Slope Over: (TRIGVAL &
MASK) == MASK )
TG_RECORDING_TRIGGER_TYPE_EDGE_Fall_Mask 10 Falling-Edge + MASK (Negative Slope Over: (TRIGVAL &
MASK) != MASK )
TG_RECORDING_TRIGGER_TYPE_LEVEL_GE_Mask 11 Grater-Equal + Mask (Equal TO: (TRIGVAL & MASK) ==
MASK )
TG_RECORDING_TRIGGER_TYPE_LEVEL_SE_Mask 12 Smaller-Equal + Mask (Not Equal TO: (TRIGVAL & MASK)
!= MASK )
TG_RECORDING_TRIGGER_TYPE_BEGIN_MOTION_Mask 13 Begin Motion

### PDF page 1222
<a id="pdf-page-1222"></a>
##### 12.5 Data Recording Functions
###### 12.5.1 MMC_BeginRecording
12.5 Data Recording Functions
12.5.1 MMC_BeginRecording
Starts the recording of internal controller variables data from the Maestro server.
MMC_LIB_API int MMC_BeginRecordingCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_BEGIN_RECORDING_IN* pInParam,
OUT MMC_BEGIN_RECORDING_OUT* pOutParam
);
Motion Mode NC - N/A Distributed - N/A
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the Connection
Handle Type. It should be noted that this connection handle is common throughout all
Maestro functions. This connection handle is returned by the Init Connection command. If an
error occurs, the function returns -1 and a MMC_LIB_API error with more details.
pInParam
Points to the MMC_BEGIN_RECORDING input data structure using the MMC_BeginRecording
function.
pOutParam
Points to the MMC_BEGIN_RECORDING_OUT output structure receiving information, as a
result of calling the MMC_BeginRecording function.
Remarks
None
Scope
All
MMC_BEGIN_RECORDING_IN Structure
typedef struct{
unsigned long uiRg;
unsigned long uiRl;
unsigned long uiRc;
unsigned long uiRv[NC_MAX_REC_SIGNALS_NUM];
unsigned long uiRp[NC_MAX_REC_PARAMS_NUM];
}MMC_BEGIN_RECORDING_IN;
Parameters

### PDF page 1223
<a id="pdf-page-1223"></a>
uiRg
Recording Data Gap which specifies the sampling rate of the recorder. Any positive integer value
uiRl
Recording Data Length with a buffer size (default size 4MB). Any positive integer value.
uiRc
Recording Data Signals Bit mask according to the definitions described in section Recording Data
Signals Bitmask Definitions. Defines which of mapped signals should be recorded with up to 32
different synchronized signals. Any positive integer value.
uiRv
uiRv is the recording Signals ID mapping enumerator which maps the IDs of the recordable signals
to logical IDs that the recorder can reference. It is a 32 bit mask assembled from the AxisNumber
and the Signal parameter. The upper 16 bits are the axis reference, and the lower 16 bits the
signal parameter, e.g. 0x00020015, where the 0002 refers to axis 02, and the 0015 refers to the
signal ID 21 (in Hex). Refer to the ID definitions described in section Recording Parameters .
[NC_MAX_REC_SIGNALS_NUM] is an array value of between [1....22] and uiRv can have any
positive integer value.
uiRp
uiRp is the ID integer value of the Recording Parameters. Refer to the section Trigger Modes for
further details. The recorder parameters defines which event will trigger the recorder, and the
trigger position, according to the following definitions:
Recording Parameters - RP[N] ID Definition
TG_RECORDING_SPARE 0 Spare
TG_RECORDING_TRIGGER_VALUE 1 Defines which of mapped signals should
be recorded, but only 1 bit may be non-
zero. The trigger variable does not need
to be one of the recorded variables.
TG_RECORDING_PRE_TRIGGER_LENGTH 2 The percentage of the recorded signal
taken before the trigger event. (recorder
trigger delay*)
TG_RECORDING_TRIGGER_TYPE 3
TG_RECORDING_TRIGGER_LEVEL_1 4 Level for positive slope trigger, or high
side for window trigger.
TG_RECORDING_TRIGGER_LEVEL_2 5 Level for negative slope trigger, or low
side for window trigger.
TG_RECORDING_TRIGGER_POLARITY 6 Logic for bit field trigger- 0 positive logic,
1 - negative
TG_RECORDING_TRIGGER_IN_MASK 7 Mask for bit field trigger
[NC_MAX_REC_PARAMS_NUM] is an array value of [1....8].

### PDF page 1224
<a id="pdf-page-1224"></a>
MMC_BEGIN_RECORDING_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_BEGIN_RECORDING_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the func tion block. Refer
to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error IDs.
Figure 391 describes the function block for MMC_BeginRecording
[PDF field-code object omitted]
Figure 391: MMC_BeginRecording function block

### PDF page 1225
<a id="pdf-page-1225"></a>
12.5.1.1 Function Block Code Example
int rc;
MMC_BEGIN_RECORDING_IN stBeginRecording_in;
MMC_BEGIN_RECORDING_OUT stBeginRecording_out;
//
// Inserting the structure parameters:
stBeginRecording_in.uiRg = 1000; // Recording Data Gap
stBeginRecording_in.uiRl = 10; // Recording Data Length
stBeginRecording_in.uiRc = 0x10000; // Parameter array index
stBeginRecording_in.uiRv[1] = 0x0020015; // Recording axis number
and the Signals ID mapping
stBeginRecording_in.uiRv[2] = 0x0030015;
stBeginRecording_in.uiRv[3] = 0x0040015;
stBeginRecording_in.uiRp[1] = 1; //Recorder parameters defines which
event will trigger the recorder
stBeginRecording_in.uiRp[2] = 2;
stBeginRecording_in.uiRp[3] = 3;
//
rc = MMC_BeginRecordingCmd (hConn, &stBeginRecording_in,
&stBeginRecording_out);
if (rc != 0)
{
HandleError() ;
}

### PDF page 1226
<a id="pdf-page-1226"></a>
###### 12.5.2 MMC_StopRecording
12.5.2 MMC_StopRecording
Halts recording of the Maestro server data.
MMC_LIB_API int MMC_StopRecordingCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_STOP_RECORDING_IN* pInParam,
OUT MMC_STOP_RECORDING_OUT* pOutParam
);
Motion Mode NC - N/A Distributed - N/A
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_STOP_RECORDING input data structure using the
MMC_StopRecording function.
pOutParam
Points to the MMC_STOP_RECORDING_OUT output structure receiving information, as
a result of calling the MMC_StopRecording function.
Remarks
None
Scope
All
MMC_STOP_RECORDING_IN Structure
typedef struct{
unsigned char dummy;
}MMC_STOP_RECORDING_IN;
Parameters
dummy
Dummy values. Any positive character value.
MMC_STOP_RECORDING_OUT Structure
typedef struct{

### PDF page 1227
<a id="pdf-page-1227"></a>
unsigned short usStatus;
short sErrorID;
}MMC_STOP_RECORDING_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 392 describes the function block for MMC_StopRecording
[PDF field-code object omitted]
Figure 392: MMC_StopRecording function block
12.5.2.1 Function Block Code Example
int rc;
MMC_STOP_RECORDING_IN stStopRecording_in;
MMC_STOP_RECORDING_OUT stStopRecording_out;
//
// Inserting the structure parameters:
stStopRecording_in.dummy = 1; // dummy data
//
rc = MMC_StopRecordingCmd (hConn, &stStopRecording_in, &stStopRecording_out);
if (rc != 0)
{
HandleError() ;
}

### PDF page 1228
<a id="pdf-page-1228"></a>
###### 12.5.3 MMC_UploadData
12.5.3 MMC_UploadData
Uploads recording data to the Maestro.
MMC_LIB_API int MMC_UploadDataCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_UPLOAD_DATA_IN* pInParam,
OUT MMC_UPLOAD_DATA_OUT* pOutParam
);
Motion Mode NC - N/A Distributed - N/A
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_UPLOAD_DATA input data structure using the MMC_UploadData
function.
pOutParam
Points to the MMC_UPLOAD_DATA_OUT output structure receiving information, as a
result of calling the MMC_UploadData function.
Remarks
None
Scope
All
MMC_UPLOAD_DATA_IN Structure
typedef struct{
unsigned int uiFrom;
unsigned int uiTo;
unsigned int uiBufIdx;
}MMC_UPLOAD_DATA_IN;
Parameters
uiFrom
Upload from index. Any positive integer value.

### PDF page 1229
<a id="pdf-page-1229"></a>
uiTo
Upload to index. Any positive integer value.
uiBufIdx
Buffer Index. Any positive integer value.
MMC_UPLOAD_DATA_OUT Structure
typedef struct{
long ulUpdatData[NC_MAX_LONG];
unsigned short usStatus;
short sErrorID;
}MMC_UPLOAD_DATA_OUT;
Parameters
ulUpdatData
Update data status. Any positive or negative integer value, with [NC_MAX_LONG]
having array values [1 - ??]
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 393 describes the function block for MMC_UploadData
[PDF field-code object omitted]
Figure 393: MMC_UploadData function block
12.5.3.1 Function Block Code Example
int rc;
MMC_UPLOAD_DATA_IN stUploadData_in;
MMC_UPLOAD_DATA_OUT stUploadData_out;
//
// Inserting the structure parameters:
stUploadData_in.uiFrom = 1; // Upload from index
stUploadData_in.uiTo = 100; // Upload to index
stUploadData_in.uiBufIdx = 1000; // Buffer Index
//
rc = MMC_UploadDataCmd (hConn, &stUploadData_in, &stUploadData_out);
if (rc != 0)
{
HandleError() ;
}

### PDF page 1230
<a id="pdf-page-1230"></a>
[No extractable text on this page.]

### PDF page 1231
<a id="pdf-page-1231"></a>
###### 12.5.4 MMC_RecStatus
12.5.4 MMC_RecStatus
Requests the status of the recording.
MMC_LIB_API int MMC_RecStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_REC_STATUS_IN* pInParam,
OUT MMC_REC_STATUS_OUT* pOutParam
);
Motion Mode NC - N/A Distributed - N/A
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_REC_STATUS input data structure using the MMC_RecStatus
function.
pOutParam
Points to the MMC_REC_STATUS_OUT output structure receiving information, as a
result of calling the MMC_RecStatus function.
Remarks
None
Scope
All
MMC_REC_STATUS_IN Structure
typedef struct{
unsigned char dummy;
}MMC_REC_STATUS_IN;
Parameters
dummy
Dummy values. Any positive character value.
MMC_REC_STATUS_OUT Structure
typedef struct{

### PDF page 1232
<a id="pdf-page-1232"></a>
unsigned long uiRr;
unsigned long uiSr;
unsigned short usStatus;
short sErrorID;
}MMC_REC_STATUS_OUT;
Parameters
uiRr
Rest Recording Index. Reads back recorder status. Any positive integer value
uiSr
Recorder Trigger Status. Status register, which indicates the status of the recorder; idle,
armed, triggered and recording, or ready with data, where the lower 8 bits display the
following options to be entered:
0 - Arming
1 - Waiting Opposite trigger
2 - Waiting Trigger
3 - Trigger Detected
4 - No Trigger
The following 8 bits (bitwise) display the following options to be entered:
0 - No Buffer Ready
1 - Buffer 1 ready
2 - Buffer 2 ready
3 - Both Buffers ready
Values accepted are any positive integer value.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 394 describes the function block for MMC_RecStatus
[PDF field-code object omitted]
Figure 394: MMC_RecStatus function block
12.5.4.1 Function Block Code Example
int rc;
MMC_REC_STATUS_IN stRecStatus_in;
MMC_REC_STATUS_OUT stRecStatus_out;
//

### PDF page 1233
<a id="pdf-page-1233"></a>
// Inserting the structure parameters:
stRecStatus_in.dummy = 1; // dummy data
//
rc = MMC_RecStatusCmd (hConn, &stRecStatus_in, &stRecStatus_out);
if (rc != 0)
{
HandleError() ;
}

### PDF page 1234
<a id="pdf-page-1234"></a>
###### 12.5.5 MMC_UploadDataHeader
12.5.5 MMC_UploadDataHeader
Recorder upload data header.
MMC_LIB_API int MMC_UploadDataHeaderCmd(
IN MMC_CONNECT_HNDL hConn,
OUT NC_UPLOAD_REC_HEADER_STRUCT* pOutParam
);
Motion Mode NC - N/A Distributed - N/A
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the Connection
Handle Type. It should be noted that this connection handle is common throughout all
Maestro functions. This connection handle is returned by the Init Connection command. If
an error occurs, the function returns -1 and a MMC_LIB_API error with more details.
pOutParam
[OUT] Points to the MMC_POSITIONPROFILE_OUT output structure receiving information,
as a result of calling the MMC_UploadDataHeader function.
Remarks
None
Scope
All
NC_UPLOAD_REC_HEADER_STRUCT Output Structure
typedef struct {
unsigned long ulDummy;
unsigned long ulRc;
unsigned long ulRg;
unsigned long ulRl;
NC_REC_RV_STRUCT usRv[NC_MAX_REC_SIGNALS_NUM];
unsigned long ulRp[NC_MAX_REC_PARAMS_NUM];
unsigned long ulTi;
unsigned long ulTs;
unsigned long ulSpare[3];
unsigned short usStatus;
short sErrorID;
unsigned char dummy [952];
}NC_UPLOAD_REC_HEADER_STRUCT;
Parameters

### PDF page 1235
<a id="pdf-page-1235"></a>
ulDummy
Dummy data. To align and upload data field in a common message. Any positive character
value.
ulRc
Recording data signals bit mask according to the definitions described in section Recording
Data Signals Bitmask Definitions. Defines which of mapped signals should be recorded
with up to 32 different synchronized signals. Any positive integer value.
ulRg
Recording data gap, which specifies the sampling rate of the recorder. Any positive integer
value.
ulRl
Recording data length with a buffer size (default size 4MB). Any positive integer value.
usRv
usRv is the recording Signals ID mapping enumerator which maps the IDs of the recordable
signals to logical IDs that the recorder can reference. Refer to the I D definitions described
in section Recording Parameters and Trigger Modes.
[NC_MAX_REC_SIGNALS_NUM] is an array value of between [1....32] and usRv can have
any positive integer value.
NC_REC_RV_STRUCT is the recorder signal value structure with the following parameters:
ulValue
Signal value reference handle of the axis. Any positive integer value.
ulType
Signal value type. The Enumerator ID has the following variable
values, which describe the enumerator NC_RV_TYPE_ENUM, and are
the recorder supported data types.
Recorder Supported Data Types ID
NC_UCHAR_TYPE 0
NC_CHAR_TYPE 1
NC_USHORT_TYPE 2
NC_SHORT_TYPE 3
NC_UINT_TYPE 4
NC_INT_TYPE 5
NC_ULONG_TYPE 6
NC_LONG_TYPE 7
NC_FLOAT_TYPE 8

### PDF page 1236
<a id="pdf-page-1236"></a>
NC_DOUBLE_L_TYPE 9
NC_DOUBLE_H_TYPE 10

ulFactor
Signal value multiple factor of the cycle time. Any positive float value
ulRp
ulRp is the ID integer value of the Recording Parameters. Refer to the section Trigger
Modes for further details. The recorder parameters defines which event will trigger the
recorder, and the trigger position, according to the following definitions:
Recording Parameters - RP[N] ID Definition
TG_RECORDING_SPARE 0 Spare
TG_RECORDING_TRIGGER_VALUE 1 Defines which of mapped signals should be
recorded, but only 1 bit may be non-zero. T
trigger variable does not need to be one of
the recorded variables.
TG_RECORDING_PRE_TRIGGER_LENGTH 2 The percentage of the recorded signal taken
before the trigger event. (recorder trigger
delay*)
TG_RECORDING_TRIGGER_TYPE 3
TG_RECORDING_TRIGGER_LEVEL_1 4 Level for positive slope trigger, or high side
for window trigger.
TG_RECORDING_TRIGGER_LEVEL_2 5 Level for negative slope trigger, or low side
for window trigger.
TG_RECORDING_TRIGGER_POLARITY 6 Logic for bit field trigger- 0 positive logic, 1 -
negative
TG_RECORDING_TRIGGER_IN_MASK 7 Mask for bit field trigger
[NC_MAX_REC_PARAMS_NUM] is an array value of [1....8].
ulTi
Trigger Index. Any positive integer value.
ulTs
Recorder Update Time. Sampling time, the basic resolution of recorder, which is the basic
time of the NC process. Any positive integer value.
ulSpare[3]
Spare. For internal use only. Any positive integer value with a maximum of 3 integers.
dummy [952]
Dummy data. Any positive character value to a maximum of 952 characters.
usStatus
Bitwise returned command status with the following values:

### PDF page 1237
<a id="pdf-page-1237"></a>
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function block .
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 395 describes the function block for MMC_UploadDataHeader
[PDF field-code object omitted]
Figure 395: MMC_UploadDataHeader function block
12.5.5.1 Function Block Code Example
int rc;
NC_UPLOAD_REC_HEADER_STRUCT stUploadRecHeadStr;
//NC_REC_RV_STRUCT (ulValue, ulType, ulFactor);
//
//
rc = MMC_UploadDataHeaderCmd (hConn, &stUploadRecHeadStr);
if (rc != 0)
{
HandleError() ;
}
