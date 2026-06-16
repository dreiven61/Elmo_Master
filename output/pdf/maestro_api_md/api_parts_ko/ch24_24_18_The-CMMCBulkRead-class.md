# 24.18 The CMMCBulkRead class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 2323
- 원문 위치: [24.18 The CMMCBulkRead class](../chunks/086_p2323-p2352_24.18-The-CMMCBulkRead-class.md#pdf-page-2323)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.18` | 2323 | `MMC_ConfigBulkReadCmd` | 구성 벌크 읽기 값/상태를 조회하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.18 The CMMCBulkRead class

- PDF 페이지: 2323
- 원문 위치: [24.18 The CMMCBulkRead class](../chunks/086_p2323-p2352_24.18-The-CMMCBulkRead-class.md#pdf-page-2323)
- 기능 설명: 구성 벌크 읽기 값/상태를 조회하는 API입니다.

#### 시그니처

```c
def MMC_ConfigBulkReadCmd(hConn, pInParam, pOutParam):
return _mmcpp_lib.MMC_ConfigBulkReadCmd(hConn,
pInParam, pOutParam)
class MMC_CONFIGBULKREAD_IN(object):
uBulkReadParams =
property(_mmcpp_lib.MMC_CONFIGBULKREAD_IN_uBulkReadParams_g
et, _mmcpp_lib.MMC_CONFIGBULKREAD_IN_uBulkReadParams_set)
eConfiguration =
property(_mmcpp_lib.MMC_CONFIGBULKREAD_IN_eConfiguration_ge
t, _mmcpp_lib.MMC_CONFIGBULKREAD_IN_eConfiguration_set)
usAxisRefArray =
property(_mmcpp_lib.MMC_CONFIGBULKREAD_IN_usAxisRefArray_ge
t, _mmcpp_lib.MMC_CONFIGBULKREAD_IN_usAxisRefArray_set)
usNumberOfAxes =
property(_mmcpp_lib.MMC_CONFIGBULKREAD_IN_usNumberOfAxes_ge
t, _mmcpp_lib.MMC_CONFIGBULKREAD_IN_usNumberOfAxes_set)
ucIsPreset =
property(_mmcpp_lib.MMC_CONFIGBULKREAD_IN_ucIsPreset_get,
_mmcpp_lib.MMC_CONFIGBULKREAD_IN_ucIsPreset_set)
```
```c
void Config(
) throw (CMMCException);
```
```c
void Config(
MMC_CONFIGBULKREAD_IN stCfgBulkReadIn
) throw (CMMCException);
```
```c
void BulkRead(
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.
