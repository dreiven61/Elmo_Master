# Maestro API 파트별 한국어 분석

이 디렉터리는 `Maestro Administrative and Motion API` Markdown 추출본을 기준으로 API 파트별 요약을 자동 생성한 결과입니다.

## 생성 기준

- 범위: Chapter 6-26의 API 관련 북마크
- 분할: 장 내부의 1차 절(예: `6.1`, `10.2`, `24.3`) 기준
- 내용: API 목록, 기능 요약, C/C++ 시그니처, 입력/출력 구조체 필드
- 주의: 함수명/구조체명/필드명은 원문 식별자라 영어 그대로 유지했습니다. 기능 설명과 필드 해석은 한국어입니다.
- 시그니처나 구조체가 추출되지 않은 항목은 원문 링크를 남겼습니다.

- 생성 파일 수: 88
- 분석 항목 수: 563

## 파일 목록

| 장 | 시작 페이지 | 항목 수 | 파일 |
|---:|---:|---:|---|
| 6 | 313 | 14 | [6.1 Single Axis Motion Control](ch06_6_1_Single-Axis-Motion-Control.md) |
| 6 | 393 | 40 | [6.2 Single Axis Administrative Control](ch06_6_2_Single-Axis-Administrative-Control.md) |
| 7 | 511 | 22 | [7.4 PCS - Product Coordinate System](ch07_7_4_PCS-Product-Coordinate-System.md) |
| 7 | 588 | 6 | [7.6 Multiple Axes Motion Control - Circular Modes](ch07_7_6_Multiple-Axes-Motion-Control-Circular-Modes.md) |
| 7 | 596 | 5 | [7.7 Multiple Axes Motion Control - Normalcy Modes](ch07_7_7_Multiple-Axes-Motion-Control-Normalcy-Modes.md) |
| 7 | 627 | 19 | [7.9 Multiple Axes Motion Control - Functions](ch07_7_9_Multiple-Axes-Motion-Control-Functions.md) |
| 7 | 726 | 39 | [7.10 Multiple Axes Administrative Control](ch07_7_10_Multiple-Axes-Administrative-Control.md) |
| 8 | 848 | 4 | [8.4 Polynomial Interpolation Functions](ch08_8_4_Polynomial-Interpolation-Functions.md) |
| 8 | 855 | 6 | [8.9 Table Functions](ch08_8_9_Table-Functions.md) |
| 8 | 864 | 7 | [8.10 PVT Functions](ch08_8_10_PVT-Functions.md) |
| 9 | 906 | 13 | [9.6 ECAM Functions](ch09_9_6_ECAM-Functions.md) |
| 10 | 959 | 55 | [10.2 Main Configuration Function Blocks](ch10_10_2_Main-Configuration-Function-Blocks.md) |
| 11 | 1095 | 1 | [11.3 PI User Functions](ch11_11_3_PI-User-Functions.md) |
| 11 | 1095 | 1 | [11.4 Read/Write RAW Data](ch11_11_4_Read-Write-RAW-Data.md) |
| 11 | 1099 | 1 | [11.6 PI Functions](ch11_11_6_PI-Functions.md) |
| 11 | 1194 | 2 | [11.7 PI Bulk Read User Functions](ch11_11_7_PI-Bulk-Read-User-Functions.md) |
| 11 | 1201 | 1 | [11.8 PI Functions and Implementation Examples](ch11_11_8_PI-Functions-and-Implementation-Examples.md) |
| 12 | 1222 | 5 | [12.5 Data Recording Functions](ch12_12_5_Data-Recording-Functions.md) |
| 13 | 1238 | 2 | [13.1 Bulk Reading Functions](ch13_13_1_Bulk-Reading-Functions.md) |
| 14 | 1253 | 1 | [14.1 Communication Byte Order](ch14_14_1_Communication-Byte-Order.md) |
| 14 | 1253 | 1 | [14.2 Communication ASYNC Replies (Events) From Drives](ch14_14_2_Communication-ASYNC-Replies-Events-From-Drives.md) |
| 14 | 1257 | 5 | [14.7 PDO Receive Event](ch14_14_7_PDO-Receive-Event.md) |
| 14 | 1259 | 2 | [14.8 Home Ended Event (C & C++)](ch14_14_8_Home-Ended-Event-C-C++.md) |
| 14 | 1260 | 1 | [14.9 Modbus Write Event](ch14_14_9_Modbus-Write-Event.md) |
| 14 | 1260 | 1 | [14.10 Touch Probe Ended Event](ch14_14_10_Touch-Probe-Ended-Event.md) |
| 14 | 1264 | 2 | [14.14 Stop On Limit Event (C & C++)](ch14_14_14_Stop-On-Limit-Event-C-C++.md) |
| 14 | 1273 | 1 | [14.19 Communication Event Mechanism](ch14_14_19_Communication-Event-Mechanism.md) |
| 14 | 1275 | 4 | [14.21 Asynchronous Events Callback (C & C++)](ch14_14_21_Asynchronous-Events-Callback-C-C++.md) |
| 14 | 1284 | 6 | [14.22 Notification and Event Function Blocks in C](ch14_14_22_Notification-and-Event-Function-Blocks-in-C.md) |
| 15 | 1311 | 5 | [15.4 Error Correction Functions](ch15_15_4_Error-Correction-Functions.md) |
| 16 | 1328 | 6 | [16.2 The MMCUserParams C++ Class](ch16_16_2_The-MMCUserParams-C++-Class.md) |
| 17 | 1344 | 14 | [17.1 Network Function Blocks](ch17_17_1_Network-Function-Blocks.md) |
| 18 | 1387 | 8 | [18.1 Modbus Communication Function Blocks](ch18_18_1_Modbus-Communication-Function-Blocks.md) |
| 19 | 1412 | 1 | [19.3 PDO Mapping](ch19_19_3_PDO-Mapping.md) |
| 19 | 1415 | 1 | [19.7 CAN Bulk Upload](ch19_19_7_CAN-Bulk-Upload.md) |
| 19 | 1416 | 1 | [19.8 CAN - PDO, SDO Configurator](ch19_19_8_CAN-PDO-SDO-Configurator.md) |
| 19 | 1416 | 31 | [19.9 CANbus Function Blocks](ch19_19_9_CANbus-Function-Blocks.md) |
| 20 | 1513 | 14 | [20.1 DS-401 Function Blocks](ch20_20_1_DS-401-Function-Blocks.md) |
| 21 | 1553 | 1 | [21.1 Elmo EtherCAT](ch21_21_1_Elmo-EtherCAT.md) |
| 21 | 1554 | 1 | [21.2 Elmo Slave Drives](ch21_21_2_Elmo-Slave-Drives.md) |
| 21 | 1555 | 1 | [21.3 EtherCAT with Maestro](ch21_21_3_EtherCAT-with-Maestro.md) |
| 21 | 1556 | 1 | [21.4 EtherCAT Gateway](ch21_21_4_EtherCAT-Gateway.md) |
| 21 | 1556 | 6 | [21.5 EtherCAT Redundancy in the Platinum Maestro](ch21_21_5_EtherCAT-Redundancy-in-the-Platinum-Maestro.md) |
| 21 | 1566 | 6 | [21.6 EtherCAT Aliasing support in the Platinum Maestro](ch21_21_6_EtherCAT-Aliasing-support-in-the-Platinum-Maestro.md) |
| 21 | 1571 | 15 | [21.7 EtherCAT Function Blocks](ch21_21_7_EtherCAT-Function-Blocks.md) |
| 22 | 1617 | 1 | [22.2 MMC_ElmoExecuteLabel](ch22_22_2_MMC_ElmoExecuteLabel.md) |
| 22 | 1620 | 1 | [22.3 MMC_ElmoSetParameter](ch22_22_3_MMC_ElmoSetParameter.md) |
| 22 | 1623 | 1 | [22.4 MMC_ElmoGetParameter](ch22_22_4_MMC_ElmoGetParameter.md) |
| 22 | 1626 | 1 | [22.5 MMC_ElmoGetAn array](ch22_22_5_MMC_ElmoGetAn-array.md) |
| 22 | 1629 | 1 | [22.6 MMC_ElmoGetAn arrayAndRetrieveData](ch22_22_6_MMC_ElmoGetAn-arrayAndRetrieveData.md) |
| 22 | 1632 | 1 | [22.7 MMC_ElmoGetParameterAndRetrieveData](ch22_22_7_MMC_ElmoGetParameterAndRetrieveData.md) |
| 22 | 1635 | 1 | [22.8 MMC_ElmoSetAn array](ch22_22_8_MMC_ElmoSetAn-array.md) |
| 22 | 1638 | 1 | [22.9 MMC_ElmoQueryOperationFIFOIndex](ch22_22_9_MMC_ElmoQueryOperationFIFOIndex.md) |
| 22 | 1640 | 1 | [22.10 MMC_ElmoQueryOperationFIFORetrieveData](ch22_22_10_MMC_ElmoQueryOperationFIFORetrieveData.md) |
| 22 | 1642 | 1 | [22.11 MMC_ElmoQueryOperationFIFOIndexReset](ch22_22_11_MMC_ElmoQueryOperationFIFOIndexReset.md) |
| 22 | 1644 | 1 | [22.12 MMC_ElmoCall](ch22_22_12_MMC_ElmoCall.md) |
| 23 | 1650 | 4 | [23.2 Configuring the Ethernet IP Device as Adapter](ch23_23_2_Configuring-the-Ethernet-IP-Device-as-Adapter.md) |
| 23 | 1661 | 17 | [23.4 EtherNetIP Functions](ch23_23_4_EtherNetIP-Functions.md) |
| 24 | 1707 | 27 | [24.2 The MMCPPGlobal class](ch24_24_2_The-MMCPPGlobal-class.md) |
| 24 | 1754 | 73 | [24.3 The MMCSingleAxis class](ch24_24_3_The-MMCSingleAxis-class.md) |
| 24 | 1925 | 25 | [24.4 The MMCNode class](ch24_24_4_The-MMCNode-class.md) |
| 24 | 1984 | 1 | [24.5 The MMCAxis class](ch24_24_5_The-MMCAxis-class.md) |
| 24 | 1993 | 1 | [24.6 The MMCMotionAxis class](ch24_24_6_The-MMCMotionAxis-class.md) |
| 24 | 2050 | 1 | [24.7 The DLLMMCPP_API MMC_MOTIONPARAMS_GROUP class](ch24_24_7_The-DLLMMCPP_API-MMC_MOTIONPARAMS_GROUP-class.md) |
| 24 | 2055 | 1 | [24.8 The MMCGroupAxis class](ch24_24_8_The-MMCGroupAxis-class.md) |
| 24 | 2190 | 1 | [24.9 The MMCDS401Axis class](ch24_24_9_The-MMCDS401Axis-class.md) |
| 24 | 2200 | 1 | [24.10 The MMCDS406Axis class](ch24_24_10_The-MMCDS406Axis-class.md) |
| 24 | 2203 | 1 | [24.11 The MMCECATIO class](ch24_24_11_The-MMCECATIO-class.md) |
| 24 | 2210 | 1 | [24.12 The MMCConnection class](ch24_24_12_The-MMCConnection-class.md) |
| 24 | 2269 | 1 | [24.13 The MMCNetwork class](ch24_24_13_The-MMCNetwork-class.md) |
| 24 | 2286 | 1 | [24.14 The MMCHostComm class](ch24_24_14_The-MMCHostComm-class.md) |
| 24 | 2313 | 1 | [24.15 The CMMCModbusBuffer class](ch24_24_15_The-CMMCModbusBuffer-class.md) |
| 24 | 2314 | 1 | [24.16 The CMMCModbusSwapBuffer class](ch24_24_16_The-CMMCModbusSwapBuffer-class.md) |
| 24 | 2315 | 1 | [24.17 The MMCErrorCorr class](ch24_24_17_The-MMCErrorCorr-class.md) |
| 24 | 2323 | 1 | [24.18 The CMMCBulkRead class](ch24_24_18_The-CMMCBulkRead-class.md) |
| 24 | 2329 | 1 | [24.19 The MMCUserParams class](ch24_24_19_The-MMCUserParams-class.md) |
| 24 | 2333 | 1 | [24.20 The MMCEIPSession class](ch24_24_20_The-MMCEIPSession-class.md) |
| 24 | 2338 | 1 | [24.21 The MMCEIPDataType class](ch24_24_21_The-MMCEIPDataType-class.md) |
| 24 | 2352 | 1 | [24.22 Functions TCP/IP and UDP/IP C++ User Libraries](ch24_24_22_Functions-TCP-IP-and-UDP-IP-C++-User-Libraries.md) |
| 24 | 2353 | 1 | [24.23 The MMCUDP class](ch24_24_23_The-MMCUDP-class.md) |
| 24 | 2383 | 1 | [24.24 The MMCTCP class](ch24_24_24_The-MMCTCP-class.md) |
| 24 | 2393 | 1 | [24.25 The MMCEoE Class](ch24_24_25_The-MMCEoE-Class.md) |
| 25 | 2423 | 1 | [25.1 ElmoIECLibVers](ch25_25_1_ElmoIECLibVers.md) |
| 25 | 2423 | 1 | [25.2 ElmoIECRTVers](ch25_25_2_ElmoIECRTVers.md) |
| 25 | 2424 | 1 | [25.3 Elmo_RetainLoad](ch25_25_3_Elmo_RetainLoad.md) |
| 25 | 2425 | 1 | [25.4 Elmo_RetainSave](ch25_25_4_Elmo_RetainSave.md) |
| 25 | 2426 | 1 | [25.5 MMC_SetImmediateExec](ch25_25_5_MMC_SetImmediateExec.md) |
| 26 | 2434 | 1 | [26.3 Python Functions](ch26_26_3_Python-Functions.md) |
