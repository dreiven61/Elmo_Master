# Elmo Controller API 공개 문서 번역 정리 (표 형식)

작성 기준일: 2026-04-02  
대상: **Elmo Multi-Axis Controller API / 개발 환경**  
범위: **공개된 공식 웹페이지, FAQ, 제품 페이지, 공개 문서 목록** 기준

> 주의  
> 이 문서는 **공개 접근 가능한 공식 페이지**를 읽고 한국어로 옮긴 요약본입니다.  
> Resource Center 로그인이 필요한 **상세 라이브러리 문서, ZIP 내부 도움말, 헤더/클래스/메서드 전체 레퍼런스**는 원문 직접 열람이 불가하여 여기에는 포함하지 않았습니다.  
> 따라서 아래 표는 **API 구조와 기능 범위**를 이해하기 위한 번역/정리본이며, **함수 시그니처 전체 번역본**은 아닙니다.

---

## 1. 범위와 해석 기준

| 항목 | 정리 |
|---|---|
| 주 대상 컨트롤러 | Gold Maestro (G-MAS), Platinum Maestro (P-MAS / PMAS) |
| 왜 G-MAS 문서가 중심인가 | Elmo의 공개 API 설명 페이지는 주로 **G-MAS** 명칭으로 제공됨 |
| Platinum Maestro와의 관계 | Platinum Maestro는 Gold Maestro의 **자연스러운 진화형**으로 설명되며, 제품 페이지에서 **Full Compatibility with the GOLD Maestro**를 명시 |
| 라이브러리 표기 | Resource Center의 라이브러리 항목도 **“Gold and Platinum Maestro .NET Library”**로 표기 |
| 포함한 내용 | 개발 방식(IPC/RPC), .NET/Win32/C/C++/IEC/GSM 특징, 이벤트 콜백 이름, 다축 기능 범위, 배포 패키지 |
| 제외한 내용 | 로그인 필요 자료에만 있는 상세 클래스 구조, 함수 파라미터, 헤더 파일 정의, 샘플 코드 본문 |

---

## 2. 컨트롤러 계열 요약

| 항목 | Gold Maestro (G-MAS) | Platinum Maestro (PMAS) | 해석 |
|---|---|---|---|
| 세대 | 1세대 Maestro | 2세대 Maestro | FAQ 기준 |
| 성격 | 네트워크 기반 멀티축 머신 모션 컨트롤러 | Gold Maestro의 진화형 고성능 멀티축 컨트롤러 | 제품/FAQ 기준 |
| 네트워크 | EtherCAT, CANopen | EtherCAT 중심, CANopen 지원 | 공개 제품 페이지 기준 |
| 모션 표준 | DS-301, DS-402, DS-406 등 | EtherCAT / CANopen / PLCopen 기반 확장 | 제품 페이지 기준 |
| 프로그래밍/API 표준 | IEC 61131-3, .NET, Win32, Native C/C++, PLCopen for Motion API, macro language | Gold와의 호환성을 유지하면서 IEC/C++ 기반 실시간 멀티태스킹 강화 | Gold/Platinum 제품 페이지 기준 |
| 다축 구조 | **16개 독립 그룹**, 각 그룹당 **최대 16축** | 제품 페이지에 **최대 256축** 명시 | 그룹 구조와 제품 스펙을 합치면 큰 틀에서는 256축급 구조로 이해 가능 |
| 동기 그룹 한계 | 동기화 그룹은 최대 16축 | FAQ상 Maestro의 동기 그룹 최대 16축 | 그룹 동작 한계와 전체 축 수는 다른 개념 |
| 최소 사이클 타임 관련 공개 수치 | Gold Maestro: 250 µs / 4 drives, 1 ms / 16 axes (FAQ) | Platinum Maestro: **down to 250 µs** | FAQ와 제품 페이지 기준 |
| 특징 | 점대점, 그룹 모션, 블렌딩, PVT, spline, Flying Vision, 오류 보정, 호스트 통신 | Gold 호환 + 더 빠른 외부 통신 + 멀티코어 + 더 큰 메모리 + 확장 HW/I/O | 제품 페이지 기준 |

---

## 3. 개발/실행 방식 번역표

| 방식 | 실행 위치 | 대표 도구/API | 사용 언어 | 핵심 의미 | 언제 적합한가 |
|---|---|---|---|---|---|
| IPC | Maestro 내부 사용자 공간 | MDS / GDS / Codesys | C/C++, IEC 계열 | 컨트롤러 내부에서 직접 실행되는 사용자 프로그램 | 통신 지연을 줄이고 최종 장비 로직을 컨트롤러 안에서 돌릴 때 |
| RPC | 호스트 PC | Win32 Library, .NET API, 멀티플랫폼 라이브러리 | C, C++, C#(Windows) | 호스트 프로그램이 TCP/IP로 Maestro를 원격 호출 | PC 애플리케이션/HMI/상위 소프트웨어와 연동할 때 |
| IEC / PLCopen | EAS 또는 Codesys 계열 환경 | IEC 툴, Codesys package | IEC 언어 계열 | PLCopen 표준 모션 함수 기반 개발 | PLC 스타일 개발, 재사용 라이브러리 구축 |
| Script Manager (GSM) | EAS 내장 | GSM | 코드 없이 FB 기반 | 실행 블록을 연결해 시퀀스/궤적 구성 | 빠른 데모, 시험, 시퀀스 검증 |
| Native C/C++ | G-MAS/PMAS 내부 또는 전용 IDE | GDS/MDS | C/C++ | Elmo 라이브러리와 직접 연결되는 정통 개발 방식 | 성능 우선, 정밀 디버깅, 최종 애플리케이션 |

---

## 4. 공개 페이지 기준 “무엇을 선택해야 하는가” 해석

| 상황 | 권장 방식 | 이유 |
|---|---|---|
| 빨리 데모를 만들어야 함 | GSM | 코드 없이 시퀀스와 궤적을 빠르게 구성 가능 |
| 최종 장비용 애플리케이션을 컨트롤러 내부에서 안정적으로 돌리고 싶음 | IPC + C/C++ (GDS/MDS) | 통신 시간 부담을 줄이고 컨트롤러 내부에서 직접 실행 가능 |
| Windows 기반 PC 프로그램에서 제어하고 싶음 | .NET API 또는 Win32 C/C++ Library | Visual Studio 친화적 |
| PLC 스타일로 구현하고 싶음 | IEC / PLCopen / Codesys | 표준 함수 블록, 샘플 코드, 라이브러리 재사용 가능 |
| 비 Windows OS와 연동하고 싶음 | RPC 멀티플랫폼 라이브러리 | FAQ 기준 Linux / VxWorks / RTX 계열도 언급됨 |

---

## 5. FAQ 기준 아키텍처 번역

| 원문 개념 | 한국어 번역 | 설명 |
|---|---|---|
| Maestro Kernel | Maestro 커널 | 일반 Linux 커널에 서보 제어를 가능하게 하는 수정이 들어간 커널 |
| Multi Axis Firmware | 다축 펌웨어 | Maestro 커널 수정본의 명칭 |
| IPC | 프로세스 간 통신 기반 내부 실행 | 컨트롤러 내부 사용자 공간 프로그램이 펌웨어와 상호작용 |
| RPC | 원격 프로시저 호출 | 외부 호스트 PC 프로그램이 TCP/IP로 Maestro와 상호작용 |
| ELMO Library | ELMO 라이브러리 | 사용자 프로그램과 Maestro 펌웨어 간 데이터 교환/동작 호출을 담당 |
| DS402 | DS402 프로토콜 | PLCopen 표준 방식에 맞춰 서보 제어를 수행하는 데이터 전송 프로토콜 |
| EtherCAT / CANopen | EtherCAT / CANopen | Maestro가 주로 지원하는 필드버스 |

---

## 6. G-MAS / Maestro 공개 API 라인업 번역표

| API/도구 | 원문 포지셔닝 | 한국어 번역 | 핵심 포인트 |
|---|---|---|---|
| G-MAS API for .NET | Full-featured programming with Microsoft’s .NET Framework | Microsoft .NET 기반의 완전한 모션 제어 API | Visual Studio + RPC + PLCopen 모션 함수 |
| G-MAS API – Win32 C/C++ Lib | Communicate with G-MAS using standard Win32 or MFC applications | Win32/MFC 응용프로그램용 C/C++ DLL | IP만 정의하면 즉시 통신 가능 |
| G-MAS Developer Studio (GDS) | Complete development and debugging environment | G-MAS용 통합 개발/디버깅 환경 | Native C/C++ 프로젝트 작성/다운로드/디버깅 |
| G-MAS PLCOpen IEC 61131-3 Programming | High-end software development using IEC and other languages | IEC 표준 기반 고급 소프트웨어 개발 환경 | PLCopen 함수, 샘플, 라이브러리, 템플릿 제공 |
| G-MAS Script Manager (GSM) | Construct machine sequences without code | 코드 없이 기계 시퀀스와 모션 궤적 작성 | FB 블록 연결 방식 |

---

## 7. .NET API 기능 항목 번역표

| 원문 항목 | 번역 | 의미 |
|---|---|---|
| Connection APIs | 연결 API | 컨트롤러/네트워크 연결 생성 및 관리 |
| Network Diagnostics | 네트워크 진단 | 네트워크 상태 확인 및 진단 |
| Single Axis and Group motion APIs | 단축 및 그룹 모션 API | 단일 축, 그룹 축 모션 제어 |
| Host Communication | 호스트 통신 | 외부 호스트와의 데이터 통신 |
| Modbus | Modbus 통신 | 표준 Modbus 프로토콜 |
| UDP / TCP | UDP / TCP 통신 | 일반 IP 소켓 기반 통신 |
| TFTP | TFTP | 파일 전송 계열 기능 |
| Ethercat IO | EtherCAT I/O | EtherCAT I/O 접근 |
| DS401 IO | DS401 I/O | CANopen DS401 계열 I/O |
| DS406 Can Encoder | DS406 CAN 엔코더 | DS406 계열 엔코더 처리 |
| Bulk Read | 일괄 읽기 | 여러 데이터를 묶어서 읽기 |
| Error Correction | 오류 보정/정정 | 오류 보정 또는 처리 관련 기능 |
| Messaging | 메시징 | 메시지 교환 기능 |
| PVT | PVT | Position / Velocity / Time 테이블 기반 모션 |

---

## 8. .NET API 이벤트 콜백 번역표

> 주의: 공개 페이지는 **콜백 이름 목록**을 제공하지만, 대부분은 세부 파라미터 설명을 제공하지 않습니다.  
> 따라서 아래 표는 **이름 그대로의 번역 + 공개 페이지에서 드러난 수준의 의미**만 적었습니다.

| 콜백 이름 | 한국어 번역 | 공개 페이지에서 확인 가능한 수준 |
|---|---|---|
| PDORCV* | PDO 수신 이벤트 | PDO 이벤트 발생 시 사용자 알림 구성 |
| HBEAT | 하트비트 이벤트 | 상세 설명 없음 |
| MOTIONENDED* | 모션 종료 이벤트 | MotionEndedEvent 활성화 |
| EMCY | 비상 이벤트 | 상세 설명 없음 |
| ASYNC_REPLY | 비동기 응답 이벤트 | 상세 설명 없음 |
| HOME_ENDED | 원점복귀 종료 이벤트 | 상세 설명 없음 |
| MODBUS_WRITE | Modbus 쓰기 이벤트 | 상세 설명 없음 |
| TOUCH_PROBE_ENDED | 터치 프로브 종료 이벤트 | 상세 설명 없음 |
| NODE_ERROR / STOP_ON_LIMIT | 노드 오류 / 리미트 정지 이벤트 | 페이지 표기상 연속 기재, 세부 설명 없음 |
| TABLE_UNDERFLOW | 테이블 언더플로 이벤트 | 상세 설명 없음 |
| NODE_CONNECTED | 노드 연결 이벤트 | 상세 설명 없음 |

---

## 9. Win32 C/C++ Library 번역표

| 원문 | 한국어 번역 |
|---|---|
| full-featured Win32/MFC DLL | 완전한 기능을 갖춘 Win32/MFC용 DLL |
| define target IP address and host IP addresses | 타깃 IP와 호스트 IP만 정의하면 됨 |
| communicate instantly with the G-MAS | 즉시 G-MAS와 통신 가능 |
| fully supports multiple NICS | 여러 NIC를 완전히 지원 |
| identical communications protocol used in GSM/GDS | GSM, GDS와 동일한 통신 프로토콜 사용 |

### Win32 라이브러리 해석
| 항목 | 의미 |
|---|---|
| 장점 | 기존 Win32/MFC 애플리케이션에 붙이기 쉬움 |
| 전형적인 사용처 | 기존 C/C++ Windows 프로그램, 장비용 GUI, 상위 제어 SW |
| 제한 | 공개 페이지에는 함수 목록이 아니라 접근 방식 설명만 있음 |

---

## 10. GDS (Native C/C++) 번역표

| 원문 기능 | 번역 |
|---|---|
| integrated development and debugging environment | 통합 개발 및 디버깅 환경 |
| create C/C++ projects | C/C++ 프로젝트 생성 |
| edit, debug and compile source code | 소스 편집, 디버깅, 컴파일 |
| run software | 소프트웨어 실행 |
| run C/C++ programs within a G-MAS target CPU | G-MAS 타깃 CPU 내부에서 C/C++ 프로그램 실행 |
| react to triggers from HMI and PC controllers | HMI/PC 컨트롤러 트리거에 반응 가능 |
| example templates | 예제 템플릿 포함 |
| input / output / host communications / error handling | 입력/출력/호스트 통신/오류 처리 예제 제공 |

### GDS 기능 요약
| 항목 | 설명 |
|---|---|
| 편집/디버깅 | 일반 IDE 수준의 편집/디버깅 기능 제공 |
| 통신 | G-MAS와 연결 설정 가능 |
| 다운로드 | 프로그램 다운로드 가능 |
| 실행 | 실행/정지/디버그 가능 |
| 터미널 | 내장 터미널 사용 가능 |
| Attach debug | 실행 중인 프로그램에 attach해서 디버깅 가능 |

---

## 11. IEC / PLCopen 개발 환경 번역표

| 원문 항목 | 번역 | 해석 |
|---|---|---|
| built into Elmo Application Studio (EAS) | Elmo Application Studio에 내장 | 별도 외부 툴이 아니라 EAS 내부 기능 |
| full accordance with the IEC standard | IEC 표준 완전 준수 | 표준 PLC 스타일 개발 가능 |
| supports all five IEC languages | 5개 IEC 언어 지원 | 공개 IEC 페이지 기준 |
| standard PLC-Open motion functions | 표준 PLCopen 모션 함수 지원 | 표준 함수 블록 기반 |
| administrative functions | 관리 기능 지원 | 모션 외 관리 기능 포함 |
| abundant sample code, motion libraries and templates | 풍부한 샘플 코드/모션 라이브러리/템플릿 | 초기 개발 단축 |
| reusable libraries | 재사용 라이브러리 가능 | 프로젝트 간 재활용 |
| simple, intuitive graphical user interface | 단순하고 직관적인 GUI | 비전문 프로그래머도 접근 가능 |

### IEC 언어 목록(공개 IEC 페이지 기준)
| 언어 | 번역 | 형태 |
|---|---|---|
| LD | 래더 다이어그램 | 그래픽 |
| FBD | 함수 블록 다이어그램 | 그래픽 |
| ST | 구조화 텍스트 | 텍스트 |
| IL | 명령어 리스트 | 텍스트 |
| SFC | 순차 기능 차트 | 순차/병렬 제어 구성 요소 포함 |

### 참고: Codesys FAQ의 언어 표기
| FAQ 표기 | 비고 |
|---|---|
| ST, LD, FBD, CFC, SFC | FAQ는 Codesys 관점의 언어 표기를 제시 |
| 공개 IEC 페이지와 차이 | 공개 IEC 페이지는 IL을 포함, FAQ는 CFC를 포함 |
| 해석 | 문맥상 **EAS 내 IEC 도구**와 **Codesys 패키지**가 동일 문서가 아니기 때문에 표기 차이가 있을 수 있음 |

---

## 12. GSM (Script Manager) 번역표

| 원문 항목 | 번역 |
|---|---|
| built into EAS | EAS에 내장 |
| manage and control all axes motions | 모든 축 모션 관리/제어 |
| EtherCAT / CANopen fieldbus Network | EtherCAT / CANopen 필드버스 네트워크 |
| executional function blocks | 실행형 함수 블록 |
| PLC-Open standard | PLCopen 표준 기반 |
| administrative, iterations and conditional FBs | 관리/반복/조건형 FB 지원 |
| construct, execute, load, save, and debug machine sequences | 시퀀스 구성/실행/로드/저장/디버깅 가능 |
| without writing a single line of code | 코드 한 줄 없이 가능 |

### GSM 설계 기능 번역
| 기능 | 번역 |
|---|---|
| creating both simple and complex machine motion sequences | 단순/복잡한 기계 모션 시퀀스 생성 |
| complex motion trajectories with special transitions | 특수 전환을 가진 복합 궤적 구성 |
| drag-and-drop insertion of PLC motion functions | PLC 모션 함수를 드래그 앤 드롭으로 삽입 |
| Set the kinematics for MCS coordinated motions | MCS 협조 모션용 기구학 설정 |
| Write DS402 Homing scripts | DS402 Homing 스크립트 작성 |
| Use administrative function blocks | 관리형 함수 블록 활용 |
| conditional and time based state modules | 조건/시간 기반 상태 모듈 활용 |
| Execute user program functions | 사용자 프로그램 함수 실행 |

---

## 13. 다축 모션 기능 범위 번역표

| 항목 | 공개 페이지 내용 | 한국어 정리 |
|---|---|---|
| 그룹 구조 | 16 independent groups, each group up to 16 single axes | 16개 독립 그룹, 각 그룹 최대 16축 |
| 그룹 보장 | ensures 100 percent synchronization | 그룹 내 축은 100% 동기화 보장 |
| 모션 타입 1 | Linear segment | 선형 세그먼트 |
| 모션 타입 2 | Circular segment | 원호 세그먼트 |
| 모션 타입 3 | Polynomial segment | 다항 세그먼트 |
| 모션 타입 4 | Spline interpolation of pre-defined position table | 미리 정의된 위치 테이블 기반 스플라인 보간 |
| 모션 타입 5 | PVT table | PVT(Position/Velocity/Time) 테이블 |
| 기구학 1 | Cartesian kinematic | 직교 좌표계 기구학 |
| 기구학 2 | Delta robot kinematic | 델타 로봇 기구학 |
| 기구학 3 | User-defined mechanics | 사용자 정의 메커니즘 |
| 변환 1 | Linear transformation for each axis | 축별 선형 변환 |
| 변환 2 | Cartesian Frame transformation, rotation and displacement | Cartesian 프레임 변환, 회전, 변위 |

---

## 14. 호스트 통신 방식 정리

| 통신 방식 | 공개 출처에서의 위치 | 의미 |
|---|---|---|
| TCP/IP | FAQ, Gold Maestro 제품 페이지, Getting Started 페이지 | 가장 일반적인 호스트-컨트롤러 통신 |
| UDP | FAQ, Gold Maestro 제품 페이지, .NET API 페이지 | 빠른 소켓 통신 |
| Modbus over TCP | FAQ, Getting Started, .NET API, Gold 제품 페이지 | 표준 산업용 통신 |
| Ethernet/IP | FAQ, Getting Started, Gold 제품 페이지 | 산업용 상위 통신 프로토콜 |
| USB Device / USB 3 Host | Platinum Maestro 제품 페이지 | PMAS의 확장 외부 통신 옵션 |
| HDMI | Platinum Maestro 제품 페이지 | HMI/화면 출력용 하드웨어 비디오 지원 |

---

## 15. 공개 문서 기준 “API로 할 수 있는 것” 요약

| 기능군 | 가능한 내용 |
|---|---|
| 기본 연결 | 컨트롤러 연결, 대상/호스트 IP 설정, 다중 NIC 지원 |
| 모션 제어 | 단축/그룹 축 모션, PVT, spline, polynomial, coordinated motion |
| 네트워크 | EtherCAT master, CANopen master, 네트워크 진단, 네트워크 건강 관리 |
| I/O | EtherCAT IO, DS401 IO |
| 엔코더 | DS406 CAN encoder |
| 궤적/기구학 | Cartesian, Delta, 사용자 정의 기구학, 좌표 변환 |
| 시퀀스 | PLCopen FB, Script Manager, DS402 Homing |
| 호스트 연동 | Modbus, TCP/IP, UDP, Ethernet/IP |
| 디버깅 | GDS 디버깅, attach debug, Script Manager 시퀀스 디버깅 |
| 고급 기능 | 실시간 목표값 업데이트, 오류 맵 보정, motion blending, superimposed motion |

---

## 16. 현재 공개된 관련 배포 패키지 / 문서 목록

> 아래 표는 Resource Center 및 공개 제품 페이지의 **현재 공개 표기명**을 정리한 것입니다.  
> Resource Center 항목은 실제 다운로드 시 **로그인 요구**가 걸릴 수 있습니다.

| 항목명 | 성격 | 대상 | 공개 표기상 설명 | 접근성 |
|---|---|---|---|---|
| Gold Maestro MMCLibDotNET Libs V3.0.0.7.zip | 라이브러리 | Gold/Platinum Maestro | Gold and Platinum Maestro .NET Library version V3.0.0.7 | Resource Center 로그인 필요 |
| Elmo_1.0.0.6.package | Codesys 패키지 | Platinum Maestro | Latest Codesys package for Platinum Maestro | Resource Center 로그인 필요 |
| Python and MWA for PMAS Rev. 1.0.12 | Python/Web 패키지 | Platinum Maestro | Python and Web Application for PMAS | Resource Center 로그인 필요 |
| MDS 6.0 User Manual | 사용자 매뉴얼 | Maestro 개발 환경 | MDS 6.0 User Manual | Resource Center 로그인 필요 |
| MDS 6.0 release notes.pdf | 릴리스 노트 | Maestro 개발 환경 | MDS 6.0 release notes | Resource Center 로그인 필요 |
| Release Notes - Platinum Maestro Rev 2 2 1 1 Ver 2.pdf | 릴리스 노트 | Platinum Maestro | Platinum Maestro 2.2.1.1 Release Note | Resource Center 로그인 필요 |
| Platinum Maestro Installation Guide_V.2.001_April 2024 | 설치 가이드 | Platinum Maestro | 공개 제품 문서 | 공개 접근 가능 |
| Platinum Maestro with Integrated I/O Functionality Network Motion Controller - Installation Guide_V.2.002_December 2024 | 설치 가이드 | PMAS IO | 공개 제품 문서 | 공개 접근 가능 |
| Gold Maestro Installation Manual_V.1.501_April 2024 | 설치 가이드 | Gold Maestro | 공개 제품 문서 | 공개 접근 가능 |

---

## 17. 실무 해석

| 질문 | 실무적으로 보면 |
|---|---|
| “Elmo controller API”는 무엇인가? | 단일 라이브러리 하나가 아니라 **Maestro용 개발 방식 전체(.NET, Win32, Native C/C++, IEC/PLCopen, GSM)**를 묶어 부르는 개념에 가깝다 |
| Windows PC에서 붙이려면? | .NET API 또는 Win32 C/C++ Library가 정석 |
| 컨트롤러 내부에서 돌리려면? | IPC 방식 + GDS/MDS 또는 Codesys/IEC |
| 코드 없이 빠르게 검증하려면? | GSM |
| PMAS도 G-MAS API 문서를 봐도 되나? | 공개 정보 기준으로는 **대체로 yes**. PMAS는 Gold와의 호환성을 명시하고, 라이브러리 항목도 Gold/Platinum 공용으로 표기됨 |
| 상세 함수 번역이 없는 이유는? | 상세 함수 레퍼런스는 로그인 필요 자료 또는 배포 ZIP 내부 문서에 있을 가능성이 높고, 공개 웹페이지에는 구조적 개요만 제공되기 때문 |

---

## 18. 공개 출처 목록

1. [Host Programming Environment](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/)
2. [Getting Started with G-MAS Programming](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/getting-started-with-g-mas-programming/)
3. [G-MAS API for .NET](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-api-for-net/)
4. [G-MAS API – Win32 C/C++ Library](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/getting-started-with-g-mas-programming-2/)
5. [G-MAS Developer Studio GDS](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-developer-studio-gds/)
6. [G-MAS PLCOpen IEC 61131-3 Programming](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-plcopen-iec-61131-3-programming/)
7. [G-MAS Script Manager](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-script-manager/)
8. [Getting Started with G-MAS Multi Axis Motion](https://www.elmomc.com/capabilities/motion-control/multi-axis-motion/getting-started-with-g-mas-multi-axis-motion/)
9. [Gold Maestro Product Page](https://www.elmomc.com/product/gold-maestro/)
10. [Platinum Maestro Product Page](https://www.elmomc.com/product/platinum-maestro/)
11. [Elmo FAQ](https://www.elmomc.com/support/faq/)
12. [Resource Center](https://www.elmomc.com/products/application-studio/download-resource-center/)

---

## 19. 한 줄 결론

Elmo의 컨트롤러 API는 **“호스트에서 원격 제어하는 API(.NET / Win32)” + “컨트롤러 내부에서 실행하는 C/C++ / IEC 환경” + “무코드 시퀀스 도구(GSM)”**의 조합으로 이해하는 것이 가장 정확합니다.
