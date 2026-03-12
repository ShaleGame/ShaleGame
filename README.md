# Shale

[![2dog Tests](https://img.shields.io/github/actions/workflow/status/ShaleGame/ShaleGame/.github%2Fworkflows%2F2dog-tests.yml?style=for-the-badge&label=2dog%20Tests)](https://github.com/ShaleGame/ShaleGame/actions/workflows/2dog-tests.yml)

> A 2D search-action platformer — visit [shalegame.github.io](https://shalegame.github.io) for more.

---

## About

Shale is a 2D search-action platformer built in the Godot Engine, where you
play as a girl with mysterious cloning powers named Shale. Shale lives in the
Chasm, a cavern where monsters have started showing up as the air gets colder
and colder, disrupting life in the fragile society that has developed since
humanity reached the stars and all but forgot about their species' cradle.

Explore, gather equipment, solve puzzles, and find out who is causing this
mysterious phenomenon and why.

Even if Shale can't handle something alone, she's never alone when she's with
her clone. The clone mirrors her movements, and even the act of splitting into
two can provide some benefits. Place a clone on a switch to keep it open, split
mid-jump for extra jump height, double your firepower against enemies, and
explore multiple areas simultaneously.

Use tools you find in the labyrinth to help with solving puzzles. The Heart of
Siracus can freeze enemies in place, letting you create your own platforms. The
Rocket Launcher launches characters that its missiles hit, including yourself
or your clone.

Find logbooks and read the history of the Chasm, the Aether lingering in its
air, and the doomed world surrounding it. Even in a bleak world, the mistakes
of the past need not be the future.

---

## Attributions

- laser one-shot #3.wav by djfroyd -- https://freesound.org/s/348162/ --
  License: Attribution 3.0
- custom_insomiac_lizard_screeching_pain_sounds_06152025 by Artninja --
  https://freesound.org/s/812444/ -- License: Attribution 4.0
- Dash Sound Effect by ArTiX.0 -- https://freesound.org/s/742717/ -- License:
  Creative Commons 0
- MUSCChim_Atonal Crystal Chime Texture 1_EM by newlocknew --
  https://freesound.org/s/772279/ -- License: Creative Commons 0

---

## Requirements

| Tool | Version |
|------|---------|
| [Godot Engine](https://godotengine.org/download) (.NET build) | **4.5.1** |
| [.NET SDK](https://dotnet.microsoft.com/download) | **8.0** |

---

## Getting Started

```bash
git clone https://github.com/ShaleGame/ShaleGame.git
cd ShaleGame
dotnet build
```

Then open the project in Godot 4.5.1 (.NET build) to trigger the initial asset import, or do it headlessly:

```bash
godot --headless --import
```

> [!IMPORTANT]
> You must build the game before importing in Godot, otherwise the C#
> assemblies won't be generated and the editor will throw errors about missing scripts.
> This is required both for running the game and for running tests, since the
> test project uses 2dog to load the Godot project headlessly and run tests in it.

---

## Running the Game

### In the editor

1. Open Godot **4.5.1 (.NET build)** and import the project from the repository root (`project.godot`).
2. Press **F5** (or the ▶ Play button).

### Export builds

The [`build.yml`](.github/workflows/build.yml) workflow can be triggered manually from GitHub Actions to produce Windows and Linux release binaries.

---

## Running Tests

The C# test project lives at `CrossedDimensions.Tests/` and uses xUnit +
[2dog](https://github.com/outfox/2dog) for headless Godot testing.

> [!IMPORTANT]
> **The Godot project must be imported** before running tests (see [Getting
> Started](#getting-started)), and the test project must be built before
> importing.

```bash
dotnet test CrossedDimensions.Tests/CrossedDimensions.Tests.csproj --configuration Debug
```

To collect coverage (mirrors CI):

```bash
dotnet test CrossedDimensions.Tests/CrossedDimensions.Tests.csproj \
  --configuration Debug \
  --logger "trx;LogFileName=test_results.trx" \
  --results-directory ./TestResults \
  --collect:"XPlat Code Coverage" \
  --settings CrossedDimensions.Tests/coverlet.runsettings
```

---

## CI

| Workflow | Trigger | Description |
|----------|---------|-------------|
| [`2dog-tests.yml`](.github/workflows/2dog-tests.yml) | Push / PR → `main` | Builds, imports headlessly, runs `dotnet test`, and publishes a TRX report + code coverage. |
| [`build.yml`](.github/workflows/build.yml) | Manual | Exports Windows and Linux release binaries as GitHub Actions artifacts. |
| [`dotnet-format.yml`](.github/workflows/dotnet-format.yml) | Push / PR | Verifies C# code formatting. |

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for code style and branching conventions.
