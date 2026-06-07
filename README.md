# MLagent2026

Unity 6.3 LTS project for a minimal ML-Agents training loop.

## Environment

- Unity: 6000.3.10f1
- Render pipeline: URP 17.3.0
- Unity package: `com.unity.ml-agents` 4.0.0
- Python trainer: `mlagents==1.1.0`

## Scene

`Assets/MLAgentsDemo/RollerTraining.unity`

The scene contains a simple rolling-ball agent, a target, floor, camera, light,
and ML-Agents `BehaviorParameters`/`DecisionRequester` setup for Unity 6.
If the scene needs to be rebuilt, use:

`MLAgent2026/Create Roller Training Scene`

## Training

Install the Python trainer in a virtual environment:

```bash
python3 -m venv .venv
.venv/bin/python -m pip install --upgrade pip
.venv/bin/python -m pip install mlagents==1.1.0
```

Start training:

```bash
.venv/bin/mlagents-learn config/roller.yaml --run-id RollerAgent-Unity6
```

Then press Play in Unity.

## 외부 에셋 / 라이선스 (Assets & License)

이 프로젝트는 **외부 서드파티 에셋을 사용하지 않습니다.**
씬의 모든 오브젝트는 Unity 기본 프리미티브(Sphere/Plane/Cube)와 직접 작성한
스크립트로만 구성됩니다. 별도 출처 표기가 필요한 에셋은 없습니다.

- Unity ML-Agents (`com.unity.ml-agents`): Unity 공식 패키지 — Unity Companion License
- `Assets/TutorialInfo/`: Unity 프로젝트 템플릿 기본 포함 파일

외부 모델·스프라이트·오디오 등을 추가할 경우, 이 섹션에
`에셋명 / 작성자 / 출처 URL / 라이선스` 형식으로 기록하세요.
