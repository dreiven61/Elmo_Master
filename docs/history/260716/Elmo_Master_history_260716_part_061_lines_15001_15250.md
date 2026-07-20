>     outdir=Path(r'tmp/manual_qa_final')/(kind+'_sheets')
>     outdir.mkdir(parents=True,exist_ok=True)
>     for batch in range(0,len(files),4):
>         ims=[Image.open(p).convert('RGB') for p in files[batch:batch+4]]
>         thumb_w=600
>         thumbs=[]
>         for im in ims:
>             h=round(im.height*thumb_w/im.width)
>             thumbs.append(im.resize((thumb_w,h)))
>         cell_h=max(im.height for im in thumbs)+36
>         sheet=Image.new('RGB',(thumb_w*2,cell_h*2),'#d9dde2')
>         d=ImageDraw.Draw(sheet)
>         for i,(im,p) in enumerate(zip(thumbs,files[batch:batch+4])):
>             x=(i%2)*thumb_w; y=(i//2)*cell_h
>             sheet.paste(im,(x,y+24)); d.text((x+8,y+5),p.stem,fill='black')
>         sheet.save(outdir/f'sheet-{batch//4+1:02d}.jpg',quality=88)
>     print(kind,len(files),len(list(outdir.glob('*.jpg'))))
> '@ | & $py -`
>
> </details>
>
> Viewed 6 images
>
> Viewed 7 images
>
> 최종 PDF 24쪽과 편집용 Word 27쪽을 전 페이지 확인했습니다. 표·코드 시그니처·목차가 잘리지 않았고, 문서 안의 응용프로그램 조작 내용은 제거된 상태입니다. 이제 공개 타입 누락과 배포본 해시만 마지막으로 검증하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pathlib import Path
> from pypdf import PdfReader
> from docx import Document
> import hashlib, re, sys
> root=Path('.')
> md=(root/'LMC_Library/LMC_API/API_USER_MANUAL_KO.md').read_text(encoding='utf-8')
> pdf=root/'output/pdf/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> docx=root/'output/doc/LASAL_Motion_Control_API_User_Manual_KO.docx'
> dpdf=root/'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> ddocx=root/'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'
> reader=PdfReader(str(pdf)); text='\n'.join((p.extract_text() or '') for p in reader.pages)
> d=Document(str(docx))
> required_types=['LMCConnection','LMCConnectionOptions','LMCConnectionStateChangedEventArgs','LMC_Response','LMCCallbackEventArgs','LMCCallbackErrorEventArgs','LMCSingleAxis','LMCAxis','LMCGroupAxis','LMCGroup','LMCGroupMotionOptions','LMCReadStatusResult','LMCReadActualPositionResult','LMCGroupReadStatusResult','LMCGroupReadActualPositionResult','LMCGroupMemberInfo','LMCGroupMembersInfoResult','LMC_Units','LMC_DIRECTION','LMC_COORD_SYSTEM','LMC_BUFFER_MODE','LMC_GROUP_TRANSITION_MODE','LMCConnectionState']
> required_methods=['RpcInitConnectionAsync','CloseConnectionAsync','PowerOnAsync','PowerOffAsync','ResetAsync','StopAsync','ReadStatusResultAsync','GetActualPositionResultAsync','MoveAbsoluteExAsync','MoveRelativeExAsync','MoveVelocityExAsync','GetGroupMembersInfoResultAsync','GroupPowerOnAsync','GroupPowerOffAsync','GroupEnableAsync','GroupDisableAsync','GroupResetAsync','GroupStopAsync','GroupReadStatusResultAsync','GroupReadActualPositionAsync','SetKinTransformCartesian4AxisAsync','MoveLinearAbsoluteExAsync']
> for token in required_types+required_methods:
>     assert token in md, token
> for token in ['WPF','예제프로그램','ConnectAndLoadAsync','EnsureAccepted','EnsureReferencedAsync','_LMCAxis1','_LMCRobotBase1','ToDint(']:
>     assert token not in md, token
> assert len(reader.pages)==24
> assert '\ufffd' not in text
> assert 'LASAL Motion Control API 레퍼런스' in text
> assert d.core_properties.title=='LASAL Motion Control API 레퍼런스'
> assert sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')) >= 70
> assert len(d.tables) >= 75
> assert 'TOC' in docx.read_bytes().decode('latin1','ignore')
> def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
> assert sha(pdf)==sha(dpdf)
> assert sha(docx)==sha(ddocx)
> print('VALIDATION=PASS')
> print('PDF_PAGES',len(reader.pages))
> print('DOCX_HEADINGS',sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')))
> print('DOCX_TABLES',len(d.tables))
> print('PDF_SHA256',sha(pdf))
> print('DOCX_SHA256',sha(docx))
> '@ | & $py -; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --cached --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; Write-Output 'DIFF_CHECK=PASS'; git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API/Generate-ApiUserManual.py' 'LMC_Library/LMC_API/Generate-ApiUserManualDocx.py' 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' 'output/pdf/LASAL_Motion_Control_API_User_Manual_KO.pdf' 'output/doc/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pathlib import Path
> from pypdf import PdfReader
> from docx import Document
> import hashlib
> root=Path('.')
> md=(root/'LMC_Library/LMC_API/API_USER_MANUAL_KO.md').read_text(encoding='utf-8')
> pdf=root/'output/pdf/LASAL_Motion_Control_API_User_Manual_KO.pdf'; docx=root/'output/doc/LASAL_Motion_Control_API_User_Manual_KO.docx'
> r=PdfReader(str(pdf)); text='\n'.join((p.extract_text() or '') for p in r.pages); d=Document(str(docx))
> print('pages',len(r.pages),'replacement', '\ufffd' in text,'title_exact', 'LASAL Motion Control API 레퍼런스' in text,'core',repr(d.core_properties.title))
> print('headings',sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')),'tables',len(d.tables),'TOC_latin', 'TOC' in docx.read_bytes().decode('latin1','ignore'))
> for token in ['WPF','예제프로그램','ConnectAndLoadAsync','EnsureAccepted','EnsureReferencedAsync','_LMCAxis1','_LMCRobotBase1','ToDint(']: print(token,token in md)
> print(text[:200])
> '@ | & $py -`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pathlib import Path
> from pypdf import PdfReader
> from docx import Document
> import hashlib, zipfile
> root=Path('.')
> md=(root/'LMC_Library/LMC_API/API_USER_MANUAL_KO.md').read_text(encoding='utf-8')
> pdf=root/'output/pdf/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> docx=root/'output/doc/LASAL_Motion_Control_API_User_Manual_KO.docx'
> dpdf=root/'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> ddocx=root/'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'
> reader=PdfReader(str(pdf)); text='\n'.join((p.extract_text() or '') for p in reader.pages)
> d=Document(str(docx))
> required_types=['LMCConnection','LMCConnectionOptions','LMCConnectionStateChangedEventArgs','LMC_Response','LMCCallbackEventArgs','LMCCallbackErrorEventArgs','LMCSingleAxis','LMCAxis','LMCGroupAxis','LMCGroup','LMCGroupMotionOptions','LMCReadStatusResult','LMCReadActualPositionResult','LMCGroupReadStatusResult','LMCGroupReadActualPositionResult','LMCGroupMemberInfo','LMCGroupMembersInfoResult','LMC_Units','LMC_DIRECTION','LMC_COORD_SYSTEM','LMC_BUFFER_MODE','LMC_GROUP_TRANSITION_MODE','LMCConnectionState']
> required_methods=['RpcInitConnectionAsync','CloseConnectionAsync','PowerOnAsync','PowerOffAsync','ResetAsync','StopAsync','ReadStatusResultAsync','GetActualPositionResultAsync','MoveAbsoluteExAsync','MoveRelativeExAsync','MoveVelocityExAsync','GetGroupMembersInfoResultAsync','GroupPowerOnAsync','GroupPowerOffAsync','GroupEnableAsync','GroupDisableAsync','GroupResetAsync','GroupStopAsync','GroupReadStatusResultAsync','GroupReadActualPositionAsync','SetKinTransformCartesian4AxisAsync','MoveLinearAbsoluteExAsync']
> for token in required_types+required_methods: assert token in md, token
> for token in ['WPF','예제프로그램','ConnectAndLoadAsync','EnsureAccepted','EnsureReferencedAsync','_LMCAxis1','_LMCRobotBase1','ToDint(']: assert token not in md, token
> assert len(reader.pages)==24
> assert '\ufffd' not in text
> assert all(t in text for t in ['Motion Control API','레퍼런스','공개 API 빠른 색인'])
> assert d.core_properties.title=='LASAL Motion Control API 레퍼런스'
> assert sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')) >= 70
> assert len(d.tables) >= 75
> with zipfile.ZipFile(docx) as z: xml=z.read('word/document.xml').decode('utf-8')
> assert 'TOC \\o' in xml
> def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
> assert sha(pdf)==sha(dpdf)
> assert sha(docx)==sha(ddocx)
> print('VALIDATION=PASS')
> print('PDF_PAGES',len(reader.pages))
> print('DOCX_HEADINGS',sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')))
> print('DOCX_TABLES',len(d.tables))
> print('PDF_SHA256',sha(pdf))
> print('DOCX_SHA256',sha(docx))
> '@ | & $py -; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --cached --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; Write-Output 'DIFF_CHECK=PASS'; git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API/Generate-ApiUserManual.py' 'LMC_Library/LMC_API/Generate-ApiUserManualDocx.py' 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' 'output/pdf/LASAL_Motion_Control_API_User_Manual_KO.pdf' 'output/doc/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'`
>
> </details>
</details>

API만 설명하는 전용 레퍼런스로 다시 만들었습니다.

- [배포용 PDF](./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)
- [편집 가능한 Word 원본](./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx)
- [Markdown 원본](./LMC_Library/LMC_API/API_USER_MANUAL_KO.md)

주요 변경 사항:

- 예제 프로그램, 화면, 버튼, 운전 순서 관련 내용 제거
- 공개 API 23개 타입 기준으로 재작성
- 동기·비동기 메서드와 모든 overload 명시
- 매개변수, 반환값, 예외, 상태값 설명
- `LMC_Response`와 typed result 전체 설명
- Enum과 `LMC_Units` 전체 값 수록
- UNIT 자동 변환이 없다는 점과 PLC adapter 지원 범위 명시
- PDF 24쪽, Word 27쪽 전 페이지 레이아웃 검수 완료
- 배포본과 생성 원본의 해시 일치 확인

커밋은 요청하지 않아 진행하지 않았습니다.

> api 사용서 그렇게 복잡하게 만들필요없어.
>
> 이건 지금 다 제작되지 않은 문서를 만드는거기 때문에 간단하게만 설명해.
>
> 4.15.8 MMC_GetErrPolicy
> This function return the current error policies statues.
> int MMC_GetErrPolicy(
> MMC_CONNECT_HNDL hConn,
> MMC_GETERRPOLICY_IN* pInParam
> MMC_GETERRPOLICY_OUT* pOutParam
> );
> Source GMAS\includes\MMC_General_API.h
> Parameters
> hConn
> [IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the Connection
> Handle Type. It should be noted that this connection handle is common throughout all
> Maestro functions. This connection handle is returned by the Init Connection command. If an
> error occurs, the function returns -1 and a MMC_LIB_API error with more details.
> pInParam
> Points to the MMC_GETERRPOLICY input structure that receives the MMC_GetErrPolicy
> command.
> pOutParam
> Points to the MMC_GETERRPOLICY_OUT output structure receiving information as a result
> of calling the MMC_GetErrPolicy function.
> Remarks
> None
> Scope
> All
> MMC_GETERRPOLICY_IN Structure
> typedef struct{
> ERRORS pErrType[MAX_REG_POLICY];
> unsigned short usAxisRef;
> unsigned char ucNum;
> }MMC_GETERRPOLICY_IN;
> Parameters
> ERRORS pErrType[MAX_REG_POLICY]
> An array of errors that their policy should be returned, where [MAX_REG_POLICY] is the
> maximum number of policies that can be returned.
> Maestro Administrative and Motion
> MAN-MAESTRO-API (Ver. 2.012)
> |www.elmomc.com
> 188
> Error! Reference
> t f d
> usAxisRef
> Axis ref of the axis its policies (if axis related) should be returned. positive value.
> ucNum
> Number of policies to be returned. positive character.
> MMC_GETERRPOLICY_OUT Structure
> typedef struct{
> NC_GET_POLICY_ENTRY pPolicies[MAX_REG_POLICY];
> unsigned short usStatus;
> short sErrorID;
> unsigned char pSpare[64];
> } MMC_GETERRPOLICY_OUT;
> Parameters
> NC_GET_POLICY_ENTRY pPolicies[MAX_REG_POLICY]
> An array of returned policies data.
> NC_GET_POLICY_ENTRY
> typedef struct nc_get_policy_entry{
> unsigned char ucPolicy;
> unsigned char ucThreshold;
> unsigned char ucCurrentVal;
> } NC_GET_POLICY_ENTRY;
> ucPolicy
> Bitwise value that defines the policy options:
> 0 No reaction
> 0x1 Send notification by event
> 0x2 All drives perform stop function block
> 0x4 All drives enter SAFEOP state - Move
> Axis\System to SAFEOP
> 0x8 Perform EEPROM scan of the entire
> network
> 0x80 Apply policy to the entire system
> ucThreshold
> The current value of the registered threshold,
> after which the resolution will operate. Any
> positive value.
> ucCurrentVal
> Maestro Administrative and Motion
> MAN-MAESTRO-API (Ver. 2.012)
> |www.elmomc.com
> 189
> Error! Reference
> t f d
> Current value of the error counter. Any positive
> value.
> usStatus
> Bitwise returned command status with the following values:
> Aborted
> Done
> CommandError
> sErrorID
> Returned command error ID. Signals where an error has occurred within the function block.
> Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs. Displays an
> error code as negative or positive integers.
> pSpare[64]
> An entry of a spare policy as n unsigned character with a limit of 64 bits.
>
> 이런식으로 API 기능만 설명해서 사용할수 있게만해.
> 나머지에 대한 내용은 적지말고 UNIT이랑 Return값만 설명할 수 있게 만들어
>
> User context
>
> Attachments:
> - Maestro Administrative and Motion API_2022_12_v2.012.pdf: `./docs/Elmo_manual/Maestro Administrative and Motion API_2022_12_v2.012.pdf`

<details><summary>12 previous messages</summary>

> 요청하신 형식으로 다시 줄이겠습니다. 기존 24쪽 레퍼런스의 공통 규칙·예외·제약·상태 설명은 제거하고, Maestro 문서처럼 각 API마다 `기능 설명 → 시그니처 → 인자와 UNIT → 반환값`만 남기겠습니다. 편집용 Word와 배포용 PDF도 함께 다시 만들겠습니다.
>
> <details><summary>Ran 2 commands</summary>
