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
