# CoQKorean
Caves of Qud 한국어화 시도해보기

## 방식
* 스크립트 모드를 이용해 패치
  * 한국어 폰트로 교체
    * TMPro 폰트 fallback에 D2Coding으로 만든 폰트 추가
    * 큰 폰트들은 TMPro가 아닌지 출력이 안 됨 (TODO)
  * `.xml` 파일 번역
    * 대화, 퀘스트, 개체 세부 정보 등
  * 소스코드 번역
    * Harmony Transpiler로 소스코드 내의 문자열 번역
      * 처음에는 Prefix로 메서드를 재정의하는 방식으로 처리했으나 너무 비효율적이고 구조상의 한계가 있었음
      * IL 코드를 통해 문자열만 갈아끼우는 Transpiler
