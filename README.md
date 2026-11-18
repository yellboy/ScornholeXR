# ScornholeXR

A two-player XR bag-toss game for Meta VR Glasses and Meta Quest, inspired by the Scornhole format from *Good Mythical Morning*. Choose an answer, throw its numbered bag, and combine a good guess with a good shot.

Each round presents an AI-generated question with five answers ranked from least to most popular. Less popular answers are worth more points, and hitting the hole earns a bonus. The player with the highest total after the final round wins.

**[Read the game rules](RULES.md)** for scoring, turns, and match settings.

For information about data handling and privacy requests, see the [Privacy Policy](PRIVACY.md).

## Features

- Two-player online matches using Photon Fusion Shared Mode.
- Networked, grabbable bags with synchronized physics and throw results.
- AI-generated ranking questions through Meta XR's LLM building block and OpenAI.
- Configurable match length and throws per player.
- Shared scores, round-end answer reveals, and final results.
- An in-game runtime log panel for troubleshooting.

## Requirements

- **Unity 6000.2.15f1**, the editor version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
- Unity Hub with **Android Build Support**, including the Android SDK, NDK, and OpenJDK, for standalone Quest builds.
- A Meta Quest headset and an XR interaction setup supported by the scene. A full match requires two running clients.
- Internet access, a Photon Fusion application, and OpenAI API access with available quota.

The project uses the Meta XR All-in-One SDK, OpenXR, and the Universal Render Pipeline. Unity Package Manager resolves the dependencies in [manifest.json](Packages/manifest.json). Photon Fusion and its physics addon are included under [Assets/Photon](Assets/Photon).

## Getting started

### 1. Open the project

Clone the repository:

```powershell
git clone https://github.com/yellboy/ScornholeXR.git
Set-Location ScornholeXR
```

In Unity Hub, add the cloned project and open it with the editor version above. Allow package resolution, asset imports, and script compilation to finish.

Open [Main.unity](Assets/Scenes/Main.unity), the scene enabled in the project's build settings.

### 2. Configure multiplayer

Configure your Photon Fusion App ID in [PhotonAppSettings.asset](Assets/Photon/Fusion/Resources/PhotonAppSettings.asset) through Unity's Inspector. Both clients must use the same Photon application.

The current implementation uses a fixed room name, `scornhole-main-v2`, with a maximum of two players. The host connects in its best region; the joining client searches its best region and then a list of other regions. There is no room browser or invitation-code UI.

### 3. Configure question generation

The scene's **[BuildingBlock] Large Language Models** object uses [LargeLanguageModels_OpenAI_ProviderProfile.asset](Assets/MetaXR/LargeLanguageModels_OpenAI_ProviderProfile.asset). Configure the OpenAI provider in Unity's Inspector with a model your account can access and your own development credentials. Its Inspector includes a connection test.

The LLM response event is already connected to [TaskFetcher](Assets/Scripts/TaskFetcher.cs). Only the authoritative client requests a question; the resulting task is shared with both players.

**Keep credentials out of Git and distributed builds.**

- The provider profile is a tracked Unity asset. Entering a key can serialize it into an asset; Meta XR's shared credential storage is also an asset, not a guarantee of secret protection.
- Keep credential-bearing assets and local configuration changes out of commits. Inspect staged changes before committing; the Unity-generated-directory ignore rules do not protect keys stored in tracked assets.
- Never publish a build containing a private OpenAI key. A trusted backend for AI requests would be needed for safe public distribution; this repository does not implement one.
- Revoke any exposed key. Removing it from the latest file alone does not remove it from Git history.

### 4. Run on Quest

1. Enable developer mode on your headset and connect it to your development machine.
2. In Unity's **File > Build Profiles**, select or create an Android profile and switch to it.
3. Ensure [Main.unity](Assets/Scenes/Main.unity) is included in the scene list.
4. Review the project's XR/OpenXR validation messages, then use **Build And Run**.
5. Run a second client using the same Photon configuration.

The checked-in Android settings target ARM64 and a minimum Android API level of 32. For Editor testing, use a configured XR runtime and input setup; a keyboard-and-mouse gameplay mode is not implemented.

### 5. Start a match

1. Player 1 selects **Start Game**, chooses the number of rounds and throws, then selects **Start**.
2. Player 2 selects **Join Game** after the host has started.
3. Once both players are connected and the question is ready, Player 1 takes the first throw.
4. Follow the display's turn indicator and choose bags by their answer numbers.

The defaults are **5 rounds** and **5 throws per player per round**. Both settings can be adjusted from **1 to 10**. See [RULES.md](RULES.md) for the full scoring table.

## Project layout

| Location | Purpose |
| --- | --- |
| [Assets/Scenes](Assets/Scenes) | Main game scene. |
| [Assets/Scripts](Assets/Scripts) | Match flow, scoring, menus, question generation, and networking. |
| [Assets/Prefabs](Assets/Prefabs) | Bag prefabs and other reusable scene objects. |
| [Assets/MetaXR](Assets/MetaXR) | OpenAI provider configuration. |
| [Assets/Photon](Assets/Photon) | Photon Fusion SDK, physics addon, and network configuration. |
| [Assets/XR](Assets/XR) | XR loader and OpenXR settings. |
| [ProjectSettings](ProjectSettings) | Unity project, platform, and build settings. |

### Gameplay code

- [GameController.cs](Assets/Scripts/GameController.cs): authoritative turn order, rounds, and cumulative scores.
- [Round.cs](Assets/Scripts/Round.cs): per-throw scoring.
- [MultiplayerSession.cs](Assets/Scripts/MultiplayerSession.cs): connection flow, player assignment, throw messages, and state snapshots.
- [Cornbag.cs](Assets/Scripts/Cornbag.cs), [CornbagsController.cs](Assets/Scripts/CornbagsController.cs), and [NetworkCornbag.cs](Assets/Scripts/NetworkCornbag.cs): collision results, bag replacement, and network authority.
- [TaskFetcher.cs](Assets/Scripts/TaskFetcher.cs): question prompt and response parsing.
- [GameMenu.cs](Assets/Scripts/GameMenu.cs) and [MainDisplay.cs](Assets/Scripts/MainDisplay.cs): match configuration and player-facing UI.

## Troubleshooting and current limitations

- **No game found:** start the host first and check that both clients use the same Photon application and room name. Searching multiple regions can take time.
- **Game already running/full:** the configured room supports only two players. There is no UI for selecting a different room.
- **Round remains on "Loading":** check the Unity Console or the in-game log panel for provider, quota, network, or JSON parsing errors. There is no automatic replacement question when parsing fails.
- **Network bags are not configured:** use **Tools > Scornhole > Configure Network Cornbags**. The editor helper also checks their configuration on load.
- **Questions and rankings:** the prompt requests source-based rankings, but generated answers and source claims are not independently verified. Treat them as game content rather than established facts.
- **Disconnects and rematches:** reconnect/host migration and an in-game rematch flow are not fully implemented. Restart both clients for a fresh match.

The rules describe this project's implementation, not official cornhole rules or a claim of affiliation with *Good Mythical Morning*.
