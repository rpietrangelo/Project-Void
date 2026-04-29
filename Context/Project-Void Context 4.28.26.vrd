# ELDRITCH DOMINION — VERSIONED REQUIREMENTS DOCUMENT (VRD)
## For: Claude Coding Agent
## Version: 1.1.0 — Vibe Coding Edition
## Last Updated: 2026-04-28

---

## HOW TO READ THIS DOCUMENT

You are a Claude coding agent tasked with building **Eldritch Dominion**, a mobile-first 4X strategy game in Unity 6 LTS. This VRD is divided into sequential phases. **Complete every task in a phase before starting the next phase.** Each task has an acceptance criteria block — do not mark a task done until all criteria pass.

> **For the human reading this:** Feed this document to your coding agent one phase at a time. Paste Phase 0 first. When the agent confirms the phase git tag is created and all acceptance criteria pass, paste the next phase. Never feed Phase N+1 before Phase N's git tag exists. If the agent gets stuck on the same problem three times in a row, open a **fresh Claude.ai conversation** (not your IDE agent), paste the full error and the full script, and ask for root cause analysis before asking for a fix. IDE agents optimize for writing code fast; Claude in a clean conversation is better at understanding what went wrong.

---

## VIBE CODING ENVIRONMENT SETUP
### Read this before your first session. Configure your environment exactly as described.

### IDE
Use **Cursor** or **Windsurf** as your code editor. Both are built for AI-assisted coding and have Unity C# support. Do not use VS Code alone — you lose the inline agent capability that makes this workflow function.

- Cursor: cursor.com
- Windsurf: codeium.com/windsurf

Open your Unity project folder directly in Cursor/Windsurf (not through Unity's built-in editor shortcut). This gives the agent full project context.

### Git Setup
Initialize git in your project root before writing any code:
```bash
git init
echo "Library/" >> .gitignore
echo "Temp/" >> .gitignore
echo "Logs/" >> .gitignore
echo "UserSettings/" >> .gitignore
echo "*.csproj" >> .gitignore
echo "*.sln" >> .gitignore
echo "*.suo" >> .gitignore
echo ".DS_Store" >> .gitignore
git add .
git commit -m "[INIT] Project created"
```

Commit cadence rule: **commit after every working feature, not after every session.** A session where you write 5 features and only commit once means one bad feature can break all 5 with no safe rollback point.

Commit message format for the agent to follow:
```
[PHASE-X][TASK-Y] short description of what was completed
```

### Session Discipline Rules
These rules prevent the most common vibe coding failures:

1. **One system per session.** Never ask the agent to touch two unrelated systems in the same prompt. "Build the building upgrade system" is one system. "Build the building upgrade system and the alliance chat" is two — split it.

2. **Document as you go.** After every completed script, add a `// SUMMARY:` comment block at the top of the file explaining what the class does in plain English. Future sessions depend on this — without it, the agent re-reads all code to rebuild context.

3. **Paste the session opener at the start of every new chat.** See the master prompts section below. If you skip this, the agent generates code for the wrong Unity version or wrong package APIs.

4. **Never accept code that compiles but has TODO placeholders in critical paths.** Placeholders in UI labels are fine. Placeholders in CloudScript validation or timer logic are bugs waiting to happen.

5. **If it doesn't compile, fix it before continuing.** Never ask the agent to "move on and fix later." Technical debt in core systems compounds.

---

## MASTER PROMPTS
### Copy these exactly. Paste the relevant one at the start of every new coding session.

---

### MASTER PROMPT — SESSION OPENER
> Paste this at the top of EVERY new Cursor/Windsurf session before any other prompt.

```
I am building a mobile 4X strategy game called Eldritch Dominion in Unity 6 LTS
using Universal Render Pipeline (URP). My stack is:

ENGINE & RENDERING:
- Unity 6000.0 LTS, Universal Render Pipeline (URP) — NOT Built-in, NOT HDRP
- IL2CPP scripting backend, .NET Standard 2.1
- Target: iOS 15+ and Android API 26+

PACKAGES (all installed):
- TextMeshPro (all UI text — never legacy Text)
- New Input System (never legacy Input.GetKey)
- Cinemachine 3.x
- Addressables (never Resources.Load)
- DOTween Pro (all UI animations and tweens)
- Lean Touch (all mobile touch gestures)
- UniTask 2.x (all async — never Coroutines for network)
- NaughtyAttributes 2.x
- PlayFab Unity SDK (economy, CloudScript, leaderboards)
- Firebase Unity SDK — Auth, Firestore, Remote Config, Crashlytics

ARCHITECTURE:
- Service Locator pattern: ServiceLocator.cs handles all cross-system access
- EventBus.cs for all decoupled communication between systems
- All events defined in GameEvents.cs as C# records
- ScriptableObjects for all game data (BuildingData, MonsterData, etc.)

SECURITY RULES (never violate these):
- Client REQUESTS, server VALIDATES and EXECUTES
- Resources/currencies only modified by PlayFab CloudScript
- Timer completion validated server-side (server checks finishTime <= Date.now())
- Never store economy state in PlayerPrefs or on the client

FOLDER STRUCTURE:
Assets/_Game/Scripts/Core/         ← ServiceLocator, EventBus, GameManager
Assets/_Game/Scripts/Network/      ← FirebaseService, PlayFabService, EconomyService
Assets/_Game/Scripts/Player/       ← LocalCityState, PlayerProfile
Assets/_Game/Scripts/Gameplay/     ← Buildings, Resources, Combat, Research, Monsters
Assets/_Game/Scripts/World/        ← WorldMapController, TileManager, CorruptionSystem
Assets/_Game/Scripts/UI/           ← Screens/, Components/, Popups/
Assets/_Game/Scripts/Horror/       ← AmbientHorrorManager, HorrorAudioManager
Assets/_Game/Scripts/Data/         ← ScriptableObject class definitions

COMMIT FORMAT: [PHASE-X][TASK-Y] description

Current task: [PASTE THE CURRENT TASK FROM THE VRD HERE]
```

---

### MASTER PROMPT — NEW UI SCREEN
> Use this when starting any new UI screen or panel.

```
Create a new UI screen called [SCREEN NAME] in Unity 6 URP.
Tech: uGUI (Canvas), TextMeshPro, DOTween Pro for all animations.

This screen displays: [DESCRIBE DATA SHOWN]
Player interactions: [LIST BUTTONS AND WHAT THEY DO]
On confirm: [WHAT HAPPENS]
On cancel/back: [WHAT HAPPENS]

Requirements:
- Extends UIScreenBase (create this base class if it doesn't exist)
- Registers with ServiceLocator<[ScreenName]> on Awake
- Subscribes to EventBus events: [LIST RELEVANT EVENTS FROM GameEvents.cs]
- Unsubscribes in OnDestroy — no memory leaks
- Animates in: DOTween fade (alpha 0→1) + scale (0.95→1.0), duration 0.25s, Ease.OutQuad
- Animates out: DOTween fade (1→0), duration 0.2s, then SetActive(false)
- Loading state: disable all buttons + show TMP "..." animated dots while any async op runs
- Error state: show error text in Color.red below the relevant element
- Mobile safe area: all layout within Screen.safeArea bounds
- No hardcoded user-facing strings — all go in LocalizationConstants.cs

Do NOT hardcode any game balance values — read from RemoteConfig via
ServiceLocator.Instance.Get<FirebaseService>().GetFloat(GameConstants.RC_*)
```

---

### MASTER PROMPT — PLAYFAB CLOUDSCRIPT FUNCTION
> Use this when writing any new CloudScript function.

```
Write a PlayFab CloudScript handler in JavaScript for PlayFab's CloudScript editor.
Function name: [FUNCTION NAME]
Triggered when: [DESCRIBE THE GAME ACTION THAT CALLS THIS]
Client sends these args: [LIST args AND THEIR TYPES]

The function MUST:
1. Log start: log.info("[FunctionName]: player=" + currentPlayerId + " args=" + JSON.stringify(args))
2. Retrieve authoritative player state from server.GetUserInternalData (never trust client args for state)
3. Validate ALL of these rules server-side: [LIST VALIDATION RULES]
4. On any validation failure: return { success: false, error: "descriptive human-readable message" }
5. Execute the change using PlayFab server.* APIs only (SubtractUserVirtualCurrency, AddUserVirtualCurrency, UpdateUserInternalData, etc.)
6. Log completion: log.info("[FunctionName]: complete, result=" + JSON.stringify(result))
7. Return { success: true, [result fields] }

Rate limiting: reject if called more than [N] times per minute by the same player.
Track call count in UserInternalData key "rateLimit_[functionName]" with a reset timestamp.

Security: never accept a resource amount, power value, or level from client args as
authoritative. Always read current state from server.GetUserInternalData first.
```

---

### MASTER PROMPT — HORROR EFFECT
> Use this when building any atmospheric/horror system.

```
Create a Unity 6 URP component for a horror effect called [EFFECT NAME].
This is part of the AmbientHorrorManager system.

Trigger: [DESCRIBE WHAT EventBus EVENT OR CONDITION TRIGGERS THIS]
Effect: [DESCRIBE WHAT THE PLAYER SEES/HEARS]
Duration: [HOW LONG IT LASTS]
Recovery: [HOW IT RETURNS TO NORMAL — DOTween back to original values]

Requirements:
- All visual changes use DOTween — never directly set values without tweening
- Post-processing effects modified via URP Volume.profile with DOTween on float parameters
- TextMeshPro distortion: use DOTween to animate TMP.text through random characters, restore after duration
- All effects toggled off if PlayerPrefs.GetInt("HorrorEffectsEnabled", 1) == 0
- No frame rate drops — profile before committing (target 60fps on iPhone SE 2020)
- Audio triggered via ServiceLocator.Instance.Get<HorrorAudioManager>().PlayHorrorClip(clipName)
- Effect must be interruptible: if the same effect triggers again mid-play, restart cleanly

Accessibility: horror effects OFF must be fully respected — no visual distortion,
no screen glitches. Ambient audio may still play at 30% volume when effects are off.
```

---

### MASTER PROMPT — STUCK (use in fresh Claude.ai conversation, not IDE)
> When the agent has failed to fix the same issue 3+ times, open a clean Claude.ai browser conversation and paste this.

```
I am debugging a Unity 6 URP project. The following error has occurred and three
different fix attempts have not resolved it. I need root cause analysis, not another fix attempt.

ERROR:
[PASTE FULL ERROR WITH STACK TRACE]

SCRIPT THAT CONTAINS THE ERROR (full file):
[PASTE FULL SCRIPT]

WHAT I TRIED:
1. [describe attempt 1]
2. [describe attempt 2]
3. [describe attempt 3]

Please identify the ROOT CAUSE of this error before suggesting any fix.
Explain why my three attempts failed. Then suggest one correct fix.
```

---

## KNOWN WALLS — READ BEFORE THEY HIT YOU
### These are the exact points where vibe coding a Unity multiplayer game gets painful. This section is embedded here so you encounter the warning before you hit the problem.

---

### WALL 1 — Firebase SDK / Android Gradle Conflict
**When it hits:** Phase 1, Task 1.2 (Firebase installation), when you first try to build for Android.

**Symptom:** Android build fails with `Duplicate class com.google.android.gms...` or `More than one file was found with OS independent path 'META-INF/...'`

**Fix:**
1. Do NOT manually edit `mainTemplate.gradle` or `launcherTemplate.gradle`
2. Go to: `Assets → External Dependency Manager → Android → Force Resolve`
3. If error persists: delete `Assets/Plugins/Android/` entirely and run Force Resolve again
4. If still failing: `Assets → External Dependency Manager → Android → Delete Resolved Libraries` then Force Resolve

**Prevention prompt for agent:**
```
I'm installing Firebase Unity SDK on Android with Unity 6. Use only the External 
Dependency Manager (EDM4U) that ships with Firebase for all Android dependency 
resolution. Never manually edit mainTemplate.gradle or launcherTemplate.gradle. 
After Firebase installation, always run: Assets → External Dependency Manager 
→ Android → Force Resolve.
```

---

### WALL 2 — Apple Sign-In Entitlements (iOS)
**When it hits:** Phase 1, Task 1.3, when you first build for a real iOS device.

**Symptom:** Build succeeds but Apple Sign-In button does nothing, or Xcode throws `Missing required entitlement com.apple.developer.applesignin`

**Fix sequence (do this in order, every step required):**
1. Apple Developer Portal → Identifiers → your App ID → enable "Sign In with Apple"
2. Create a new provisioning profile after enabling (old profiles don't have the entitlement)
3. In Unity: `Edit → Project Settings → Player → iOS → Other Settings → Automatically Sign` = true (for development)
4. In Xcode: Signing & Capabilities → + Capability → Sign In with Apple
5. Firebase Console → Authentication → Sign-in method → Apple → enter your Services ID and Key
6. The Firebase Apple Sign-In flow requires a real device — it CANNOT be tested in simulator

**Set aside a full day for this.** Do not attempt it 2 hours before a deadline.

---

### WALL 3 — Firestore Real-Time Map Performance
**When it hits:** Phase 4, Task 4.2, when you have 50+ city markers loading on the world map.

**Symptom:** World map FPS drops when scrolling, or Firestore rate limit errors in console (`RESOURCE_EXHAUSTED`)

**Root cause:** Listening to individual city documents triggers one network call per city. At 200 cities, that's 200 simultaneous listeners.

**Fix:** Use Firestore bounds queries with a single listener, updated on camera movement:
```csharp
// Only query cities within camera viewport + 20 tile buffer
// Cancel listener when camera moves > 10 tiles
// Re-subscribe with new bounds
// This one change reduces active listeners from 200 to ~20
```

**Prevention prompt for agent:**
```
When querying Firestore for world map city markers in Unity, never open 
individual document listeners per city. Use a single CollectionReference 
.WhereGreaterThanOrEqualTo("worldX", minX).WhereLessThanOrEqualTo("worldX", maxX)
bounds query. Cancel and re-subscribe the listener when the camera pans more than 
10 tiles from the last query position. This is mandatory for performance.
```

---

### WALL 4 — PlayFab CloudScript Logging (Invisible Failures)
**When it hits:** Phase 3, Task 3.1 — the moment you deploy your first CloudScript.

**Symptom:** CloudScript returns `{ success: false }` and you have no idea why. Or it seems to work but resources aren't changing.

**Fix:** Before deploying any CloudScript function, set up logging:
1. PlayFab Dashboard → Automation → CloudScript → check "Enable logging"
2. In every CloudScript function, add `log.info()` calls at start, after each validation, and at end
3. View logs: PlayFab Dashboard → Analytics → Event History → filter by `com.playfab.cloudscript.execute_function`
4. For local testing, use PlayFab's "Run Function" button in the CloudScript editor before calling from Unity

**Do this before writing any CloudScript or you will debug blind.**

---

### WALL 5 — Addressables Build Pipeline
**When it hits:** Phase 0, Task 0.2 — when you first try to make a device build after setting up Addressables.

**Symptom:** Build succeeds but Addressable assets throw `InvalidKeyException` at runtime — the assets can't be found.

**Root cause:** Addressables asset bundles must be built separately from the Unity player build. Unity's normal Build button does NOT build Addressables.

**Fix:** Create this Editor script immediately after installing Addressables. Run it before every device build.

**File: `Assets/_Game/Scripts/Editor/BuildAddressablesPreBuild.cs`**
```csharp
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public class BuildAddressablesPreBuild
{
    [MenuItem("ElDom/Build Addressables")]
    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
        UnityEngine.Debug.Log("[Addressables] Build complete.");
    }

    // Auto-runs before any player build
    [InitializeOnLoadMethod]
    static void SetupPreBuildHook()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(buildOptions =>
        {
            AddressableAssetSettings.BuildPlayerContent();
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildOptions);
        });
    }
}
```

---

### WALL 6 — UniTask and Firebase SDK Conflict
**When it hits:** Phase 1, Task 1.3 — when you first write async Firebase code using UniTask.

**Symptom:** `await` on Firebase tasks throws at runtime, or tasks complete but callbacks never fire.

**Root cause:** Firebase SDK returns `System.Threading.Tasks.Task`, not `UniTask`. They need a bridge.

**Fix:** Always wrap Firebase calls:
```csharp
// WRONG — will cause issues:
await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, pass);

// CORRECT — convert Firebase Task to UniTask:
await FirebaseAuth.DefaultInstance
    .SignInWithEmailAndPasswordAsync(email, pass)
    .AsUniTask();

// Add to every file that uses both:
using Cysharp.Threading.Tasks;
// The .AsUniTask() extension is in the UniTask.Firebase bridge package
// Install: openupm add com.cysharp.unitask.firebase (or manually add bridge)
```

**Prevention prompt for agent:**
```
In this project, Firebase SDK returns System.Threading.Tasks.Task objects.
Always convert them to UniTask using the .AsUniTask() extension method before
awaiting. Never await a raw Firebase Task directly. Import 
using Cysharp.Threading.Tasks; in every script that uses Firebase async calls.
```

---

### WALL 7 — IL2CPP Stripping Breaks PlayFab SDK
**When it hits:** Phase 3, when you make your first Release build (not Development build).

**Symptom:** Debug builds work fine. Release builds throw `MissingMethodException` or `EntryPointNotFoundException` for PlayFab calls.

**Root cause:** IL2CPP's code stripping removes PlayFab's reflection-based serialization code because the stripper doesn't know it's used at runtime.

**Fix:** Create a `link.xml` file in Assets/ to preserve PlayFab assemblies:
```xml
<!-- Assets/link.xml -->
<linker>
  <assembly fullname="PlayFabSDK" preserve="all"/>
  <assembly fullname="PlayFabUnitySDK" preserve="all"/>
  <assembly fullname="Firebase.Auth" preserve="all"/>
  <assembly fullname="Firebase.Firestore" preserve="all"/>
  <assembly fullname="Firebase.RemoteConfig" preserve="all"/>
  <assembly fullname="Newtonsoft.Json" preserve="all"/>
</linker>
```

---

## AGENT RULES
### These rules apply to every task in every phase. The agent must follow them without exception.

- Engine: Unity 6000.0 LTS, Universal Render Pipeline (URP)
- Language: C# (.NET Standard 2.1, IL2CPP scripting backend)
- Backend: Firebase (Auth + Firestore + Remote Config + Crashlytics) + PlayFab SDK
- Animation: DOTween Pro only — no Unity Animator for UI transitions
- Text: TextMeshPro only — never legacy UI Text
- Input: New Input System only — never `Input.GetKey` or `Input.GetAxis`
- Asset Loading: Addressables — never `Resources.Load`
- Async: UniTask with `.AsUniTask()` bridge for Firebase — never raw Coroutines for network operations
- Architecture: Service Locator + EventBus — no direct MonoBehaviour references between systems
- Security: Client REQUESTS, server VALIDATES and EXECUTES — never trust client-provided amounts or completion states
- Comments: Every completed script must have a `// SUMMARY:` block at the top in plain English
- Commits: `[PHASE-X][TASK-Y] description` after every working feature

---

## STACK REFERENCE
```
Unity:        6000.0.x LTS
Render:       Universal Render Pipeline (URP)
IDE:          Cursor or Windsurf
Backend A:    Firebase Unity SDK (Auth, Firestore, Remote Config, Crashlytics)
Backend B:    Microsoft PlayFab Unity SDK (Economy V2, CloudScript, Leaderboards)
Animation:    DOTween Pro 1.2.x
Touch:        Lean Touch (Asset Store)
Async:        UniTask 2.x + Firebase bridge
Inspector:    NaughtyAttributes 2.x
Tilemap:      Unity 2D Tilemap Extras
Camera:       Cinemachine 3.x
```

---
---

## PHASE 0 — PROJECT SCAFFOLD
### Goal: Empty but correctly structured Unity project with all packages installed, vibe coding environment configured, and core architecture files written. No gameplay logic.

---

### TASK 0.0 — Vibe Coding Environment
**Action:** Configure your development environment before opening Unity.

Step 1 — Install Cursor or Windsurf:
```
Cursor:   cursor.com → Download → install
Windsurf: codeium.com/windsurf → Download → install
```

Step 2 — Install Unity Hub + Unity 6000.0 LTS:
```
unityhub://download → install Unity 6000.0 LTS
Add modules: Android Build Support, iOS Build Support
```

Step 3 — Initialize the project repository:
```bash
# After Unity creates the project folder:
cd /path/to/your/project
git init
cat > .gitignore << 'EOF'
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.DS_Store
.AppleDouble
.LSOverride
Thumbs.db
ehthumbs.db
Desktop.ini
EOF
git add .
git commit -m "[INIT] Project scaffold initialized"
git branch -M main
```

Step 4 — Open the project in Cursor/Windsurf:
```
File → Open Folder → select your Unity project root
(The root with Assets/, ProjectSettings/, Packages/ visible)
```

Step 5 — Configure Cursor/Windsurf AI context:
Create a file at your project root named `.cursorrules` (Cursor) or `.windsurfrules` (Windsurf):
```
# Eldritch Dominion — Agent Rules

## Stack
Unity 6000.0 LTS, URP, C#, .NET Standard 2.1, IL2CPP

## Non-negotiable rules
- TextMeshPro only (never legacy Text)
- New Input System only (never Input.GetKey)
- UniTask for all async (never Coroutines for network, use .AsUniTask() for Firebase)
- Addressables for all assets (never Resources.Load)
- DOTween Pro for all UI animation
- Service Locator pattern for cross-system access
- EventBus for all system-to-system communication
- Client never modifies economy state — always via PlayFab CloudScript
- Never await a Firebase Task directly — wrap with .AsUniTask()

## Commit format
[PHASE-X][TASK-Y] description

## Every completed script must include at the top:
// SUMMARY: [plain English description of what this class does]
```

**Acceptance Criteria:**
- [ ] Cursor or Windsurf installed and opens project folder
- [ ] Git repository initialized with `.gitignore`
- [ ] `.cursorrules` or `.windsurfrules` file created at project root
- [ ] Unity 6000.0 LTS installed with Android and iOS modules

---

### TASK 0.1 — Unity Project Creation
**Action:** Create Unity 6000.0 LTS project. Paste the **Session Opener** master prompt into your IDE agent before starting this task.

Settings to apply immediately after creation:
```
Edit → Project Settings → Player:
  iOS Bundle Identifier:   com.yourstudio.eldritchdominion
  Android Package Name:    com.yourstudio.eldritchdominion
  Minimum iOS Version:     15.0
  Minimum Android API:     26
  Scripting Backend:       IL2CPP
  Api Compatibility Level: .NET Standard 2.1
  Active Input Handling:   Input System Package (New)

Edit → Project Settings → Quality:
  Delete all quality levels. Create two: "Mobile Low" and "Mobile High"
  Both levels:
    Anti Aliasing:    Disabled
    Shadow Distance:  50
    Shadow Cascades:  2

URP Asset (Assets/Settings/UniversalRenderPipelineAsset.asset):
  Rendering Path:    Forward
  HDR:               Disabled
  MSAA:              2x
  Render Scale:      1.0
  Post Processing:   Enabled
  Depth Texture:     Enabled
  Opaque Texture:    Disabled
```

**Acceptance Criteria:**
- [ ] Project opens without errors in Unity 6000.0 LTS
- [ ] URP is active (no pink/magenta materials in default scene)
- [ ] New Input System is active (legacy Input disabled)
- [ ] Bundle IDs set for both platforms

---

### TASK 0.2 — Package Installation
**Action:** Install all required packages in this exact order. Resolve compilation errors before proceeding to the next package.

> ⚠️ **Wall Warning — Addressables:** Install Addressables now. After installation, immediately create the `BuildAddressablesPreBuild.cs` Editor script from the Wall 5 section above. Without it, your first device build will fail with `InvalidKeyException` and you won't know why.

> ⚠️ **Wall Warning — Firebase:** After importing Firebase, immediately run `Assets → External Dependency Manager → Android → Force Resolve`. If you see "Duplicate class" errors on Android build, refer to Wall 1 above before spending time debugging.

**Unity Registry (Window → Package Manager → Unity Registry):**
```
1. TextMeshPro                    (install + import TMP Essentials when prompted)
2. Input System                   (confirm switching to new Input System)
3. Cinemachine                    (version 3.x)
4. 2D Tilemap Extras
5. Addressables                   → immediately create BuildAddressablesPreBuild.cs after this
6. Burst
7. Collections
```

**Asset Store / Manual Import:**
```
8.  DOTween Pro             → Asset Store → import all → run DOTween Setup Wizard
9.  Lean Touch              → Asset Store → import all
10. NaughtyAttributes       → OpenUPM: com.dbrizov.naughtyattributes
11. UniTask                 → OpenUPM: com.cysharp.unitask
12. UniTask Firebase Bridge → OpenUPM: com.cysharp.unitask  (includes Firebase adapter)
13. PlayFab Unity SDK       → Asset Store OR GitHub: PlayFab/UnitySDK
14. Firebase Unity SDK      → firebase.google.com/docs/unity/setup
                              Import ONLY:
                              - FirebaseAuth.unitypackage
                              - FirebaseFirestore.unitypackage
                              - FirebaseRemoteConfig.unitypackage
                              - FirebaseAnalytics.unitypackage
                              - FirebaseCrashlytics.unitypackage
                              → Run: Assets → External Dependency Manager → Android → Force Resolve
```

**Immediately after Firebase install — create link.xml:**
Create `Assets/link.xml` with the content from Wall 7 above. Do this now before it causes a Release build failure later.

**Acceptance Criteria:**
- [ ] Zero compilation errors after all packages installed
- [ ] DOTween setup wizard completed
- [ ] TMP Essentials imported
- [ ] Firebase EDM4U has resolved Android dependencies (no red warnings)
- [ ] `BuildAddressablesPreBuild.cs` exists in `Assets/_Game/Scripts/Editor/`
- [ ] `Assets/link.xml` exists with PlayFab and Firebase assembly preservation entries

---

### TASK 0.3 — Folder Structure
**Action:** Create this exact folder structure. Create a `.gitkeep` in empty leaf folders.

```
Assets/
├── _Game/
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Network/
│   │   ├── Player/
│   │   ├── Gameplay/
│   │   │   ├── Buildings/
│   │   │   ├── Resources/
│   │   │   ├── Combat/
│   │   │   ├── Research/
│   │   │   └── Monsters/
│   │   ├── World/
│   │   ├── UI/
│   │   │   ├── Screens/
│   │   │   ├── Components/
│   │   │   └── Popups/
│   │   ├── Horror/
│   │   ├── Data/
│   │   └── Editor/
│   ├── Prefabs/
│   │   ├── Buildings/
│   │   │   ├── Order/
│   │   │   └── Cult/
│   │   ├── Units/
│   │   ├── Monsters/
│   │   ├── UI/
│   │   ├── FX/
│   │   └── World/
│   ├── Scenes/
│   ├── Art/
│   │   ├── Tiles/
│   │   ├── Buildings/
│   │   ├── Units/
│   │   ├── UI/
│   │   ├── Monsters/
│   │   └── FX/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Horror/
│   ├── Data/
│   │   ├── Buildings/
│   │   ├── Units/
│   │   ├── Monsters/
│   │   ├── Patrons/
│   │   └── Research/
│   ├── Fonts/
│   └── Settings/
│       ├── Input/
│       └── Addressables/
├── Plugins/
└── StreamingAssets/
    └── Firebase/
```

**Acceptance Criteria:**
- [ ] Folder structure matches exactly
- [ ] No scripts in any folder yet (only `.gitkeep`)
- [ ] `Assets/link.xml` exists at root level

---

### TASK 0.4 — Core Architecture Scripts
**Action:** Create these 6 files. These are the foundation. Do not modify them after this task completes — any changes require creating a new version and migrating.

> **Session tip:** Use the **Session Opener** master prompt at the top of your IDE agent session. Then ask it to create each file one at a time. Do not ask it to create all 6 in one prompt — the output will be truncated or rushed.

**File: `Assets/_Game/Scripts/Core/ServiceLocator.cs`**
```csharp
// SUMMARY: Central registry that any script can use to get access to any major system.
// Prevents spaghetti dependencies. Register services on Awake, Get them anywhere.
// Usage: ServiceLocator.Instance.Register<MyService>(this);
//        ServiceLocator.Instance.Get<MyService>().DoThing();

using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[ServiceLocator]");
                _instance = go.AddComponent<ServiceLocator>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
        Debug.Log($"[ServiceLocator] Registered: {typeof(T).Name}");
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;
        Debug.LogError($"[ServiceLocator] Service not found: {typeof(T).Name}");
        return null;
    }

    public bool IsRegistered<T>() where T : class => _services.ContainsKey(typeof(T));
    public void Unregister<T>() where T : class => _services.Remove(typeof(T));
}
```

**File: `Assets/_Game/Scripts/Core/EventBus.cs`**
```csharp
// SUMMARY: Decoupled pub/sub event system. Systems publish events without knowing who listens.
// Listeners subscribe without knowing who publishes. Eliminates direct MonoBehaviour references.
// Usage: EventBus.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
//        EventBus.Publish(new BuildingUpgradedEvent("city_hall", 2));
//        EventBus.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded); // always in OnDestroy

using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing))
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        else
            _handlers[typeof(T)] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (_handlers.TryGetValue(typeof(T), out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) _handlers.Remove(typeof(T));
            else _handlers[typeof(T)] = updated;
        }
    }

    public static void Publish<T>(T evt)
    {
        if (_handlers.TryGetValue(typeof(T), out var handler))
            ((Action<T>)handler)?.Invoke(evt);
    }

    public static void Clear() => _handlers.Clear();
}
```

**File: `Assets/_Game/Scripts/Core/GameEvents.cs`**
```csharp
// SUMMARY: All EventBus event types live here. Add new events to this file only.
// Events are C# records — immutable, value-equality, concise syntax.

public record AuthStateChangedEvent(bool IsLoggedIn, string PlayFabId, string FirebaseUid);
public record FactionChosenEvent(FactionType Faction);
public record ResourceChangedEvent(string CurrencyCode, int NewAmount);
public record BuildingUpgradeStartedEvent(string BuildingId, int NewLevel, long FinishTimeUnix);
public record BuildingUpgradeCompleteEvent(string BuildingId, int NewLevel);
public record TileOccupiedEvent(int X, int Y, string OwnerId, string AllianceId);
public record CorruptionChangedEvent(int TileX, int TileY, float NewLevel, float ServerGlobalLevel);
public record PatronProgressChangedEvent(string PatronId, float ProgressPercent);
public record PatronAwakenedEvent(string PatronId);
public record AllianceJoinedEvent(string AllianceId, string AllianceName);
public record AllianceLeftEvent(string AllianceId);
public record MarchStartedEvent(string MarchId, int DestX, int DestY, long ArrivalTimeUnix);
public record MarchCompletedEvent(string MarchId, bool WasAttack, bool Victory);
public record HorrorEventTriggeredEvent(HorrorEventType EventType, float Intensity);
public record PvEMonsterSpawnedEvent(int TileX, int TileY, string MonsterId, int MonsterLevel);
public record PlayerPowerChangedEvent(long NewPower);
public record KvKStartedEvent(string OpponentServerId);
public record SeasonEndedEvent(int SeasonNumber, FactionType WinningFaction);
```

**File: `Assets/_Game/Scripts/Core/GameEnums.cs`**
```csharp
// SUMMARY: All game-wide enum types. Never define enums in individual scripts.

public enum FactionType   { Unset = 0, Order = 1, Cult = 2, Wanderer = 3 }
public enum HorrorEventType { UIGlitch, CorruptionSpread, PatronWhisper, PatronAwakening, VoidRiftOpened }
public enum BuildingState  { Locked, Available, Building, Upgrading, Ready }
public enum TileType       { Plains, Forest, Mountain, Ruins, VoidRift,
                             ResourceNode_Iron, ResourceNode_Stone,
                             ResourceNode_VoidStone, ResourceNode_AncientText }
public enum MarchType      { Gather, AttackPlayer, AttackMonster, Reinforce, Scout }
public enum ResearchCategory { Military, Architecture, Arcane, ForbiddenRituals,
                               VoidArchitecture, Survival }
```

**File: `Assets/_Game/Scripts/Core/GameConstants.cs`**
```csharp
// SUMMARY: All magic strings and numbers live here. Never hardcode currency codes,
// collection names, or Remote Config keys anywhere else.

public static class GameConstants
{
    public const string CURRENCY_PALE_GOLD       = "PG";
    public const string CURRENCY_VOID_CRYSTALS   = "VC";
    public const string CURRENCY_ELDRITCH_ESSENCE = "EE";
    public const string CURRENCY_CORRUPTION_PTS  = "CP";
    public const string CURRENCY_FLAME_TOKENS    = "FT";
    public const string CURRENCY_WANDERER_MARKS  = "WM";

    public const string COL_PLAYERS     = "players";
    public const string COL_CITIES      = "cities";
    public const string COL_ALLIANCES   = "alliances";
    public const string COL_WORLD_TILES = "worldTiles";
    public const string COL_SERVERS     = "servers";
    public const string COL_CHAT        = "allianceChat";

    public const string RC_RESOURCE_RATE_MULT  = "resource_gen_rate_multiplier";
    public const string RC_BUILD_TIME_MULT      = "building_time_multiplier";
    public const string RC_SHIELD_DURATION_H    = "pvp_shield_duration_hours";
    public const string RC_CORRUPTION_RATE      = "corruption_spread_rate";
    public const string RC_PATRON_THRESHOLD     = "patron_awakening_threshold";
    public const string RC_HORROR_EVENT_FREQ_H  = "horror_event_frequency_hours";

    public const int    MAX_ALLIANCE_SIZE        = 50;
    public const int    CITY_BUILDING_SLOTS      = 25;
    public const int    MAX_MARCH_SLOTS          = 5;
    public const int    DAILY_LOGIN_CHAIN_LENGTH = 30;
    public const int    VOID_CHEST_PITY_COUNTER  = 60;
    public const float  HORROR_TRIGGER_THRESHOLD = 0.8f;
    public const int    MAP_WIDTH                = 500;
    public const int    MAP_HEIGHT               = 500;
}
```

**File: `Assets/_Game/Scripts/Core/GameManager.cs`**
```csharp
// SUMMARY: Central game state holder and scene router. Persists across all scenes.
// Initializes all services on startup. Routes player to correct scene based on auth state.

using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsInitialized  { get; private set; }
    public string CurrentPlayFabId  { get; private set; }
    public string CurrentFirebaseUid { get; private set; }
    public FactionType CurrentFaction { get; private set; }
    public string CurrentServerId { get; private set; }
    public long CurrentPower { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start() => await InitializeServicesAsync();

    private async UniTask InitializeServicesAsync()
    {
        Debug.Log("[GameManager] Initializing...");
        await UniTask.WaitUntil(() => ServiceLocator.Instance.IsRegistered<FirebaseService>());
        await UniTask.WaitUntil(() => ServiceLocator.Instance.IsRegistered<PlayFabService>());
        IsInitialized = true;
        Debug.Log("[GameManager] Ready.");

        var auth = ServiceLocator.Instance.Get<FirebaseService>();
        await LoadSceneAsync(auth.IsLoggedIn ? "WorldMap" : "Login");
    }

    public void SetPlayerState(string playfabId, string firebaseUid,
                               FactionType faction, string serverId, long power = 0)
    {
        CurrentPlayFabId  = playfabId;
        CurrentFirebaseUid = firebaseUid;
        CurrentFaction    = faction;
        CurrentServerId   = serverId;
        CurrentPower      = power;
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[GameManager] → {sceneName}");
        await SceneManager.LoadSceneAsync(sceneName);
    }
}
```

**Acceptance Criteria:**
- [ ] All 6 files compile without errors
- [ ] Every file has a `// SUMMARY:` block at the top
- [ ] No gameplay logic in any Core file
- [ ] Git commit: `[PHASE-0][TASK-0.4] Core architecture scripts`

---

### TASK 0.5 — Scene Setup
**Action:** Create 4 scenes. Add to Build Settings index 0–3.

```
Bootstrap.unity   (index 0) — GameManager + ServiceLocator only
Login.unity       (index 1) — Camera + Canvas (placeholder)
WorldMap.unity    (index 2) — Cinemachine Camera + Canvas
City.unity        (index 3) — Camera + Canvas
```

**Acceptance Criteria:**
- [ ] 4 scenes in Build Settings
- [ ] Bootstrap loads, finds no auth session, transitions to Login
- [ ] Zero null reference exceptions on startup
- [ ] Git commit: `[PHASE-0][TASK-0.5] Scene setup`

---

### PHASE 0 COMPLETE CRITERIA
- [ ] Project runs on device/simulator without crashes
- [ ] All packages installed, zero console errors
- [ ] Core scripts compile and function
- [ ] `BuildAddressablesPreBuild.cs` and `link.xml` exist
- [ ] `.cursorrules` or `.windsurfrules` at project root
- [ ] Git tag: `v0.0.1-scaffold`

---
---

## PHASE 1 — AUTHENTICATION
### Goal: Players can create accounts, log in, choose faction. State persists. No gameplay yet.

> **Session setup:** Paste the **Session Opener** master prompt at the start of your IDE session. For Firebase-specific tasks, append: *"Remember: always wrap Firebase Task returns with .AsUniTask() before awaiting. Never await a raw Firebase Task."* (Wall 6)

---

### TASK 1.1 — Firebase Project Configuration
**Action:** Configure Firebase in the Firebase Console, then import config files.

```
1. console.firebase.google.com → Create Project → "eldritch-dominion-dev"
   (Also create "eldritch-dominion-prod" — never develop against prod)
2. Authentication → Sign-in method → Enable: Email/Password, Google, Apple
3. Firestore → Create database → Production mode → us-central1
4. Remote Config → Add all keys (see Appendix D for full list with defaults)
5. Project Settings → Add iOS app → Bundle ID: com.yourstudio.eldritchdominion
   Download GoogleService-Info.plist → Assets/StreamingAssets/Firebase/
6. Project Settings → Add Android app → Package: com.yourstudio.eldritchdominion
   Download google-services.json → Assets/StreamingAssets/Firebase/
```

Firestore Security Rules:
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /players/{playerId} {
      allow read: if request.auth != null;
      allow create: if request.auth.uid == playerId;
      allow update: if request.auth.uid == playerId
                   && !request.resource.data.diff(resource.data)
                     .affectedKeys()
                     .hasAny(['power', 'resources', 'allianceId', 'serverId']);
    }
    match /cities/{cityId} {
      allow read: if request.auth != null;
      allow write: if false;
    }
    match /alliances/{allianceId} {
      allow read: if request.auth != null;
      allow update: if request.auth.uid == resource.data.leaderId
                   && !request.resource.data.diff(resource.data)
                     .affectedKeys().hasAny(['score', 'territory', 'totalPower']);
    }
    match /worldTiles/{tileId} {
      allow read: if request.auth != null;
      allow write: if false;
    }
    match /servers/{serverId} {
      allow read: if request.auth != null;
      allow write: if false;
    }
  }
}
```

> ⚠️ **Wall Warning — Apple Sign-In:** If you test on iOS and Apple Sign-In does nothing, see Wall 2 above. Set aside a full day for that integration. Do not attempt it late in a sprint.

**Acceptance Criteria:**
- [ ] Config files in StreamingAssets/Firebase/
- [ ] Firebase initializes without errors
- [ ] Remote Config fetches defaults
- [ ] Security rules deployed

---

### TASK 1.2 — PlayFab Project Configuration

> ⚠️ **Wall Warning — CloudScript Logging:** Before you write a single CloudScript function, enable logging in PlayFab Dashboard → Automation → CloudScript → Enable logging. Without this, you debug blind. See Wall 4 above.

```
1. playfab.com → Create Title → "Eldritch Dominion Dev"
2. Economy → Currencies → Create all 6 (see Appendix E for full list)
3. Automation → CloudScript → Enable logging  ← DO THIS NOW
4. Settings → API → copy your Title ID
5. Unity: Assets → PlayFab → Editor Extensions → paste Title ID
```

**Acceptance Criteria:**
- [ ] Title ID set in Unity
- [ ] All 6 currencies created
- [ ] CloudScript logging enabled
- [ ] PlayFab SDK initializes without errors

---

### TASK 1.3 — Firebase Service

> **Prompt tip:** Use the **Session Opener** then ask for this one file at a time. Append: *"All Firebase async calls must use .AsUniTask() — never await a raw Firebase Task."*

**File: `Assets/_Game/Scripts/Network/FirebaseService.cs`**

Required public interface:
```csharp
// SUMMARY: Handles all Firebase Auth and Firestore operations.
// Registers with ServiceLocator on Awake. All methods async via UniTask.
// IMPORTANT: All Firebase Task returns wrapped with .AsUniTask() — never awaited raw.

public class FirebaseService : MonoBehaviour
{
    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn    { get; private set; }
    public string UserId      { get; private set; }
    public string Email       { get; private set; }

    public async UniTask<(bool success, string error)> SignUpWithEmailAsync(string email, string password, string displayName);
    public async UniTask<(bool success, string error)> SignInWithEmailAsync(string email, string password);
    public async UniTask<(bool success, string error)> SignInWithGoogleAsync();
    public async UniTask<(bool success, string error)> SignInWithAppleAsync();
    public async UniTask SignOutAsync();
    public async UniTask<bool> CreatePlayerDocumentAsync(string displayName, FactionType faction);
    public async UniTask<PlayerDocument> GetPlayerDocumentAsync(string playerId = null);
    public async UniTask<bool> UpdatePlayerFactionAsync(FactionType faction);
    public async UniTask FetchRemoteConfigAsync();
    public float GetFloat(string key, float defaultValue);
    public int   GetInt(string key, int defaultValue);
}
```

**Acceptance Criteria:**
- [ ] Registers with ServiceLocator on Awake
- [ ] Email sign-up creates Firebase Auth user AND Firestore player document
- [ ] Sign-in retrieves player document and caches it
- [ ] All Firebase calls use `.AsUniTask()` bridge
- [ ] Publishes `AuthStateChangedEvent` on sign-in and sign-out
- [ ] All error paths return descriptive strings, no unhandled exceptions
- [ ] `// SUMMARY:` at top of file

---

### TASK 1.4 — PlayFab Service
**File: `Assets/_Game/Scripts/Network/PlayFabService.cs`**

```csharp
// SUMMARY: Handles PlayFab login (linked to Firebase UID), currency retrieval,
// and CloudScript invocation. All economy state lives here, never in Unity.

public class PlayFabService : MonoBehaviour
{
    public bool IsLoggedIn  { get; private set; }
    public string PlayFabId { get; private set; }

    public async UniTask<(bool success, string error)> LoginWithFirebaseAsync(string firebaseToken);
    public async UniTask<Dictionary<string, int>> GetAllCurrenciesAsync();
    public async UniTask<int> GetCurrencyAsync(string currencyCode);
    public async UniTask<(bool success, string error)> CallCloudScriptAsync(string functionName, object args);
    public async UniTask<bool> SetDisplayNameAsync(string name);
    public async UniTask UpdatePlayerStatisticAsync(string statisticName, int value);
}
```

**Acceptance Criteria:**
- [ ] PlayFab login always called after Firebase login using Firebase token
- [ ] Currency retrieval caches values locally
- [ ] `ResourceChangedEvent` fires after any currency fetch
- [ ] Registers with ServiceLocator on Awake
- [ ] `// SUMMARY:` at top of file

---

### TASK 1.5 — Auth UI

> **Prompt:** Use the **New UI Screen** master prompt. Screen name: "Login Screen". List the 4 panels and their interactions.

Screens (as separate GameObjects, toggled active):
```
Panel_Welcome       — logo, Sign In button, Create Account button
Panel_SignIn        — email, password, Sign In button, Back button
Panel_CreateAccount — display name, email, password, confirm password, Create button
Panel_FactionChoice — Order card + Cult card + Wanderer card, each with Choose button
                      Shown ONLY when PlayerDocument.faction == "unset"
```

**Acceptance Criteria:**
- [ ] Account creation → faction choice → WorldMap loads
- [ ] Returning user → WorldMap loads directly (no faction choice)
- [ ] Faction written to Firestore and never shown again
- [ ] Errors display for: invalid email, weak password, wrong credentials, network failure
- [ ] All panels animate with DOTween (0.25s fade + slide)
- [ ] All user-facing strings in `LocalizationConstants.cs`
- [ ] `// SUMMARY:` at top of script

---

### PHASE 1 COMPLETE CRITERIA
- [ ] Full auth flow: create → faction → game
- [ ] Returning user: sign in → game (no faction choice)
- [ ] PlayFab linked and currencies initialized
- [ ] Remote Config fetching
- [ ] Git tag: `v0.1.0-auth`

---
---

## PHASE 2 — CITY BUILDING (LOCAL)
### Goal: Building placement and upgrade works locally. No backend calls yet. Proves the core loop.

> **Session setup:** Paste the **Session Opener** master prompt. Append: *"This phase is local only — no backend calls. LocalCityState uses PlayerPrefs for persistence. Economy calls will be replaced in Phase 3."*

> **One system per session rule:** Do Task 2.1 (ScriptableObjects) in one session. Do Task 2.2 (LocalCityState) in a separate session. Do Task 2.3 (UI) in a third session. Never combine.

---

### TASK 2.1 — Building Data ScriptableObjects
**File: `Assets/_Game/Scripts/Data/BuildingData.cs`**

```csharp
// SUMMARY: ScriptableObject definition for all building types and their per-level stats.
// Create instances in Assets/_Game/Data/Buildings/ — one asset per building type.
// Agent: create the class definition, then create 8 starter building assets as listed below.

[CreateAssetMenu(fileName = "NewBuilding", menuName = "ElDom/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Identity")]
    public string buildingId;
    public string displayName;
    [TextArea(2,4)] public string loreText;
    public FactionType faction;
    public int cityHallLevelRequired;
    public bool isUniquePerCity;

    [Header("Levels")]
    public List<BuildingLevel> levels;

    [Header("Visuals")]
    public Sprite[] levelSprites;
    public GameObject worldPrefab;
}

[System.Serializable]
public class BuildingLevel
{
    public int level;
    public float buildTimeSeconds;
    public List<ResourceCost> buildCost;
    public List<ResourceProduction> hourlyProduction;
    public float defenseBonus;
    public int powerContribution;
    public int troopCapacityBonus;
}

[System.Serializable]
public class ResourceCost       { public string currencyCode; public long amount; }
[System.Serializable]
public class ResourceProduction { public string currencyCode; public long amountPerHour; }
```

Create 8 starter building assets (4 Order, 4 Cult) as listed in the original technical guide.

**Acceptance Criteria:**
- [ ] `BuildingData.cs` compiles
- [ ] 8 assets exist in `Assets/_Game/Data/Buildings/`
- [ ] All assets serialized with no missing field warnings

---

### TASK 2.2 — Local City State

> **Prompt tip:** Use the **Session Opener** + append: *"This is local-only. No network calls. Persistence via PlayerPrefs as JSON. All timers via UniTask.Delay. Will be replaced with backend calls in Phase 3."*

**File: `Assets/_Game/Scripts/Player/LocalCityState.cs`**

```csharp
// SUMMARY: Manages local building slot state and fake local resources for Phase 2.
// Will be partially replaced by EconomyService (Phase 3) for all resource operations.
// Timers use UniTask.Delay. State persists via PlayerPrefs as JSON.
```

Required operations:
- `PlaceBuilding(string buildingId, int slotIndex)`
- `StartUpgrade(int slotIndex)` — deducts local resources, starts timer
- `CollectUpgrade(int slotIndex)` — called when timer ends, increments level
- `GetBuildingAt(int slotIndex) → BuildingInstance`
- `GetAllBuildings() → List<BuildingInstance>`
- `CanAfford(BuildingLevel level) → bool`

**Acceptance Criteria:**
- [ ] Build, upgrade, timer, completion cycle works
- [ ] `BuildingUpgradeStartedEvent` fires on start
- [ ] `BuildingUpgradeCompleteEvent` fires on completion
- [ ] State survives scene reload (PlayerPrefs JSON)

---

### TASK 2.3 — City Scene UI

> **Prompt:** Use the **New UI Screen** master prompt. Screen name: "City Screen". List the slot grid, HUD elements, and both panels (empty slot / occupied slot).

City scene layout:
```
CinemachineCamera (orthographic, top-down)
CityGrid (25 building slots — placeholder colored squares)
Canvas
├── HUD_Top (ResourceBar showing PG + VC — local values)
├── HUD_Bottom
│   ├── Button_WorldMap
│   └── Button_Alliance (placeholder)
├── Panel_BuildingSlot (tap empty slot)
│   ├── ScrollView with available buildings for player's faction
│   └── BuildingCard (name, cost, time, Build button)
└── Panel_BuildingDetail (tap occupied slot)
    ├── Name, Level, Upgrade cost, Upgrade time
    ├── ProgressBar (countdown while upgrading)
    ├── Button_Upgrade (hidden while upgrading)
    └── Button_SpeedUp (placeholder)
```

**Acceptance Criteria:**
- [ ] 25 slots in grid
- [ ] Panels open/close with DOTween (0.3s)
- [ ] Building placed and upgrade starts on button tap
- [ ] Progress bar counts down correctly
- [ ] On completion, slot visual updates
- [ ] Works on 375×812 mobile safe area

---

### PHASE 2 COMPLETE CRITERIA
- [ ] Full local building loop works
- [ ] 8 building types, faction-filtered
- [ ] Timers complete correctly
- [ ] No backend calls
- [ ] Git tag: `v0.2.0-city-local`

---
---

## PHASE 3 — BACKEND ECONOMY INTEGRATION
### Goal: All economy server-authoritative via PlayFab CloudScript. Building state in PlayFab. Local fake economy removed.

> **Session setup:** Paste **Session Opener** + **CloudScript** master prompts together at the session start. The CloudScript prompt is critical for this entire phase.

> ⚠️ **Wall Warning — CloudScript Debugging:** If CloudScript returns `{ success: false }` and you don't know why, check PlayFab Dashboard → Analytics → Event History first. If you haven't enabled logging (Task 1.2), do it now before writing any CloudScript.

---

### TASK 3.1 — PlayFab CloudScript: Building Upgrade

> **Prompt:** Paste the **CloudScript** master prompt. Function name: `StartBuildingUpgrade`. Include the complete validation rules below.

Deploy to PlayFab → Automation → CloudScript:

```javascript
// SUMMARY: Validates and starts a building upgrade. Deducts cost server-side.
// Stores upgrade finish timestamp in UserInternalData (never in Unity client).
// Client cannot fake completion — server checks finishTime in CompleteUpgrade.

var BUILDING_COSTS = {
  "city_hall_order_2":    { PG: 500,  buildSeconds: 60 },
  "barracks_order_1":     { PG: 200,  buildSeconds: 30 },
  "farm_order_1":         { PG: 150,  buildSeconds: 20 },
  "watchtower_order_1":   { PG: 300,  buildSeconds: 45 },
  "sanctum_cult_2":       { PG: 500,  buildSeconds: 60 },
  "ritual_circle_cult_1": { PG: 200,  buildSeconds: 30 },
  "void_font_cult_1":     { PG: 150,  buildSeconds: 20 },
  "monster_lair_cult_1":  { PG: 400,  buildSeconds: 60 }
};

handlers.StartBuildingUpgrade = function(args, context) {
    var playerId = currentPlayerId;
    log.info("StartBuildingUpgrade start: player=" + playerId + " args=" + JSON.stringify(args));

    var costKey = args.buildingId + "_" + args.targetLevel;
    var costEntry = BUILDING_COSTS[costKey];
    if (!costEntry) return { success: false, error: "Unknown building/level: " + costKey };

    var data = server.GetUserInternalData({ PlayFabId: playerId, Keys: ["buildings"] });
    var buildings = data.Data["buildings"] ? JSON.parse(data.Data["buildings"].Value) : {};
    var current = buildings[args.slotIndex] || { level: 0, state: "empty" };

    if (args.targetLevel !== current.level + 1) return { success: false, error: "Invalid level jump" };
    if (current.state === "upgrading") return { success: false, error: "Already upgrading" };

    if (costEntry.PG && costEntry.PG > 0) {
        try {
            server.SubtractUserVirtualCurrency({ PlayFabId: playerId, VirtualCurrency: "PG", Amount: costEntry.PG });
        } catch(e) { return { success: false, error: "Insufficient Pale Gold" }; }
    }

    var finishTime = Date.now() + (costEntry.buildSeconds * 1000);
    buildings[args.slotIndex] = {
        buildingId: args.buildingId, level: current.level,
        state: "upgrading", targetLevel: args.targetLevel, upgradeFinishTime: finishTime
    };
    server.UpdateUserInternalData({ PlayFabId: playerId, Data: { buildings: JSON.stringify(buildings) } });

    log.info("StartBuildingUpgrade complete: finishTime=" + finishTime);
    return { success: true, finishTime: finishTime };
};

handlers.CompleteUpgrade = function(args, context) {
    var playerId = currentPlayerId;
    log.info("CompleteUpgrade: player=" + playerId + " slot=" + args.slotIndex);

    var data = server.GetUserInternalData({ PlayFabId: playerId, Keys: ["buildings"] });
    var buildings = JSON.parse(data.Data["buildings"].Value || "{}");
    var building = buildings[args.slotIndex];

    if (!building || building.state !== "upgrading") return { success: false, error: "No upgrade in progress" };
    if (Date.now() < building.upgradeFinishTime) return { success: false, error: "Timer not complete",
        remainingMs: building.upgradeFinishTime - Date.now() };

    building.level = building.targetLevel;
    building.state = "ready";
    delete building.upgradeFinishTime;
    delete building.targetLevel;
    buildings[args.slotIndex] = building;
    server.UpdateUserInternalData({ PlayFabId: playerId, Data: { buildings: JSON.stringify(buildings) } });

    log.info("CompleteUpgrade complete: level=" + building.level);
    return { success: true, buildingId: building.buildingId, newLevel: building.level };
};

handlers.GetPlayerBuildings = function(args, context) {
    var data = server.GetUserInternalData({ PlayFabId: currentPlayerId, Keys: ["buildings"] });
    var buildings = data.Data["buildings"] ? JSON.parse(data.Data["buildings"].Value) : {};
    return { success: true, buildings: buildings };
};
```

**Acceptance Criteria:**
- [ ] Deployed without errors
- [ ] `StartBuildingUpgrade` deducts PG, returns finishTime
- [ ] `StartBuildingUpgrade` rejects invalid level jumps
- [ ] `CompleteUpgrade` rejects when timer hasn't elapsed
- [ ] All functions have `log.info()` at start and end

---

### TASK 3.2 — Economy Service
**File: `Assets/_Game/Scripts/Network/EconomyService.cs`**

```csharp
// SUMMARY: Wraps all PlayFab economy calls. Replaces LocalCityState's resource
// management. All currency changes go through here. Calls CloudScript for all
// building operations — never modifies economy state directly from Unity client.
```

Required interface:
```csharp
public async UniTask RefreshCurrenciesAsync();
public async UniTask<(bool success, long finishTimeUnix, string error)>
    StartBuildingUpgradeAsync(string buildingId, int slotIndex, int targetLevel);
public async UniTask<(bool success, string error)> CompleteUpgradeAsync(int slotIndex);
public async UniTask<Dictionary<int, BuildingInstance>> GetPlayerBuildingsAsync();
public int GetCurrency(string currencyCode);
public async UniTask<(bool success, List<string> items, string error)>
    OpenChestAsync(string chestItemId);
```

**Acceptance Criteria:**
- [ ] `RefreshCurrenciesAsync` called automatically on login
- [ ] `ResourceChangedEvent` fires after every currency refresh
- [ ] `StartBuildingUpgradeAsync` returns server's finishTime
- [ ] `CompleteUpgradeAsync` rejected if timer hasn't elapsed
- [ ] Registers with ServiceLocator on Awake

---

### TASK 3.3 — Connect City UI to Backend

Update `LocalCityState` to use `EconomyService` for all economy operations. Remove all PlayerPrefs resource storage.

Changes:
- City scene load → call `EconomyService.GetPlayerBuildingsAsync()` to populate slots
- Upgrade buttons call `EconomyService.StartBuildingUpgradeAsync()`
- Timer completion calls `EconomyService.CompleteUpgradeAsync()`
- Resource bar subscribes to `ResourceChangedEvent`
- Timer is now based on server's `finishTime` — not `UniTask.Delay`

**Acceptance Criteria:**
- [ ] Building state loaded from PlayFab on scene entry
- [ ] Upgrade calls visible in PlayFab Dashboard logs
- [ ] Timer survives app restart (based on server timestamp)
- [ ] Resource bar shows PlayFab currency values

---

### PHASE 3 COMPLETE CRITERIA
- [ ] All economy server-authoritative
- [ ] Client cannot skip timers or gain free resources
- [ ] PlayFab logs show CloudScript execution for every upgrade
- [ ] Git tag: `v0.3.0-backend-economy`

---
---

## PHASE 4 — WORLD MAP
### Goal: Players see the world map. Their city and others' cities are visible. Pan, zoom, tap.

> ⚠️ **Wall Warning — Firestore Map Performance:** When you add city markers, never open one Firestore listener per city. Use a bounds query and update it as the camera pans. See Wall 3 before writing any city-loading code.

> **Session setup:** Paste **Session Opener** + append: *"When querying Firestore for world map markers, use a single bounds query — never individual document listeners. Cancel and re-subscribe on camera pan > 10 tiles."*

---

### TASK 4.1 — Tilemap World Map

WorldMap scene hierarchy:
```
CinemachineCamera (orthographic, pan + zoom via Lean Touch)
Grid
├── Tilemap_Ground    (base terrain)
├── Tilemap_Overlay   (corruption — semi-transparent)
└── Tilemap_Objects   (resource nodes, monster spawns)
Layer_Cities          (city markers from Firestore)
Canvas
├── HUD_Top (resources)
├── HUD_Bottom (Button_MyCity, Button_Alliance)
└── Panel_TileInfo (tap tile info panel)
```

Camera controls:
```
Pinch:       Zoom (orthographic size 3–15)
Drag:        Pan (clamped to map bounds)
Tap tile:    Select → Panel_TileInfo
Double-tap:  Center on tile
```

**Acceptance Criteria:**
- [ ] 100×100 map renders at 60fps
- [ ] Pan and zoom work on touch
- [ ] Camera stays within bounds
- [ ] Tapping tile shows Panel_TileInfo

---

### TASK 4.2 — City Markers

> **Prompt:** Use **Session Opener** + Wall 3 Firestore bounds query prevention note.

Query strategy:
```csharp
// Single Firestore bounds query on worldX/worldY
// Update query when camera moves > 10 tiles from last position
// Requires Firestore composite index: worldX + worldY (create in Firebase Console)
// Never: one listener per city document
```

City marker tapped → Panel_TileInfo shows:
```
Player name, Faction, Power, Alliance name
Buttons: View Profile (placeholder), Scout (placeholder), Attack (placeholder)
```

**Acceptance Criteria:**
- [ ] Cities visible on map at correct coordinates
- [ ] Own city has "home" indicator
- [ ] Tapping shows info panel
- [ ] Markers update on camera pan (bounds query)
- [ ] Hidden at orthographic size > 10

---

### PHASE 4 COMPLETE CRITERIA
- [ ] World map renders and performs well on mobile
- [ ] Player and other cities visible
- [ ] Bounds-query Firestore loading
- [ ] Git tag: `v0.4.0-world-map`

---
---

## PHASE 5 — ALLIANCE & CULT SYSTEM
### Goal: Players can create, join, and communicate within their faction group.

> **Session setup:** Paste **Session Opener** + append: *"Alliance system uses Firestore for all data. Chat uses a real-time Firestore snapshot listener ordered by timestamp. All write operations except chat message creation go through Cloud Functions — not direct client Firestore writes."*

---

### TASK 5.1 — Alliance Service
**File: `Assets/_Game/Scripts/Network/AllianceService.cs`**

```csharp
// SUMMARY: Handles all Firestore operations for Alliance/Cult system.
// Chat uses real-time Firestore listener. Alliance writes validated by security rules.
// Cult-specific: patronId and ritualProgress fields present on Cult alliances only.
```

Required interface:
```csharp
public async UniTask<(bool success, string allianceId, string error)>
    CreateAllianceAsync(string name, string description, bool isOpen, int minPower);
public async UniTask<(bool success, string error)> JoinAllianceAsync(string allianceId);
public async UniTask<(bool success, string error)> LeaveAllianceAsync();
public async UniTask<List<AllianceDocument>> SearchAlliancesAsync(string nameQuery, FactionType faction);
public async UniTask<(bool success, string error)> SendChatMessageAsync(string message);
public void SubscribeToAllianceChat(Action<ChatMessage> onNewMessage);
public void UnsubscribeFromAllianceChat();
```

**Acceptance Criteria:**
- [ ] Create/join/leave lifecycle works in Firestore
- [ ] Chat real-time listener fires `onNewMessage` callback
- [ ] Alliance name uniqueness checked before creation
- [ ] Faction isolation: Order only searches Order alliances
- [ ] `AllianceJoinedEvent` and `AllianceLeftEvent` fire correctly

---

### TASK 5.2 — Alliance UI

> **Prompt:** Use the **New UI Screen** master prompt. Screen name: "Alliance Screen". List 4 tabs and their content.

```
Tab_Overview  — name, members, power, score, patron progress (Cult)
Tab_Members   — scrollable list, online status, leader controls
Tab_Chat      — real-time messages, input field, send button
Tab_Search    — only when not in alliance: search field, results, create button
```

**Acceptance Criteria:**
- [ ] All tabs work
- [ ] Chat updates in real-time without refresh
- [ ] Member online status (lastActive < 5 min = online)
- [ ] Cult tab: ritual progress bar visible

---

### PHASE 5 COMPLETE CRITERIA
- [ ] Full alliance lifecycle: create → join → chat → leave
- [ ] Real-time chat
- [ ] Faction isolation enforced
- [ ] Git tag: `v0.5.0-alliance`

---
---

## PHASE 6 — PvE MONSTER SYSTEM
### Goal: Monsters spawn on map. Players march to attack. Server resolves combat. Rewards granted.

> **Session setup:** Paste **Session Opener** + **CloudScript** master prompt for combat functions. Append: *"Monster spawning happens via Firebase Cloud Function on a schedule — never from Unity client."*

---

### TASK 6.1 — Monster Data ScriptableObjects
**File: `Assets/_Game/Scripts/Data/MonsterData.cs`**

Create 6 starter monster assets with levels, stats, rewards, and lore fragment IDs.
(Deep One Scout, Void Tendril, Corruption Wraith, Dreaming Shoggoth, Star-Spawn, Pale Herald)

**Acceptance Criteria:**
- [ ] `MonsterData.cs` compiles
- [ ] 6 monster assets in `Assets/_Game/Data/Monsters/`
- [ ] Each has 3+ levels with stats and rewards

---

### TASK 6.2 — Monster Spawn Cloud Function

Deploy to Firebase Cloud Functions. Runs every 30 minutes. Spawns monsters on unoccupied tiles respecting tile type constraints. Max 200 monsters per server.

**Acceptance Criteria:**
- [ ] Function deployed and visible in Firebase Console
- [ ] Monsters appear in Firestore worldTiles documents
- [ ] Tile type constraints respected
- [ ] Count cap enforced

---

### TASK 6.3 — March & Combat CloudScript

> **Prompt:** Use **CloudScript** master prompt twice — once for `StartMarch`, once for `ResolveMarchVsMonster`.

Functions: `StartMarch`, `ResolveMarchVsMonster`

Key rules:
- Max 5 active marches per player (validated server-side)
- Arrival time calculated server-side from distance
- Combat resolution on server — client cannot report "I won"
- Rewards granted via `AddUserVirtualCurrency` server-side

**Acceptance Criteria:**
- [ ] March starts, stores in PlayFab with server arrival time
- [ ] `ResolveMarchVsMonster` validates arrival time has elapsed
- [ ] Victory grants PG via PlayFab (visible in currency balance)
- [ ] Both functions have `log.info()` at start and end

---

### TASK 6.4 — Combat Encounter UI

> **Prompt:** Use **New UI Screen** prompt. Screen name: "Combat Encounter Popup". Include horror description reveal and DOTween animations.

Horror description: character-by-character reveal at 0.05s per character using DOTween.
Monster image: brief glitch effect on appear (DOTween material property animation).

**Acceptance Criteria:**
- [ ] Panel appears when `MarchCompletedEvent` fires
- [ ] Horror description reveals character-by-character
- [ ] Rewards match PlayFab currency grant
- [ ] Victory and defeat visually distinct

---

### PHASE 6 COMPLETE CRITERIA
- [ ] Monsters spawn automatically
- [ ] Full march → combat → reward loop works
- [ ] Server resolves all combat
- [ ] Git tag: `v0.6.0-pve`

---
---

## PHASE 7 — HORROR SYSTEMS
### Goal: Corruption spreads visually. Patron awakening glitches UI. Audio atmosphere active.

> **Session setup:** Paste **Session Opener** + **Horror Effect** master prompt at session start.

---

### TASK 7.1 — Corruption Visual System

> **Prompt:** Use **Horror Effect** master prompt. Effect name: "Corruption Spread Visual". Trigger: `CorruptionChangedEvent`. Effect: tile overlay darkens, post-processing Chromatic Aberration at > 0.5 corruption.

**File: `Assets/_Game/Scripts/Horror/CorruptionSystem.cs`**

Firebase Cloud Function `spreadCorruption` runs every 15 minutes and updates tile corruption levels. Unity listens via Firestore snapshot listener and calls `CorruptionSystem` to update visuals.

**Acceptance Criteria:**
- [ ] Tiles darken as corruption increases (DOTween alpha animation)
- [ ] Chromatic Aberration activates at 0.5 threshold
- [ ] Corruption updates from Firestore listener without restart
- [ ] No frame drops during visual update

---

### TASK 7.2 — Patron Awakening Horror Event

> **Prompt:** Use **Horror Effect** master prompt. Effect name: "Patron Awakening". Two trigger levels: 80% (warning) and 100% (full awakening). List exact visual sequence for each.

**File: `Assets/_Game/Scripts/Horror/AmbientHorrorManager.cs`**

At 80% patron progress:
- Distort 2–3 TMP labels (random characters, restore after 1.5s)
- Vignette intensity 0.3 → 0.8 → 0.3 over 3s (DOTween on URP Volume)
- Play horror audio clip
- Show cryptic overlay text for 2s then fade

At 100% (PatronAwakenedEvent):
- Full screen flash to black
- All UI text briefly → symbols for 3s
- "THE [PATRON NAME] STIRS." overlay
- Chromatic aberration max → fade over 5s

All effects: `PlayerPrefs.GetInt("HorrorEffectsEnabled", 1) == 0` disables visual effects.

**Acceptance Criteria:**
- [ ] 80% warning triggers correctly
- [ ] 100% awakening sequence plays
- [ ] Accessibility toggle fully disables visuals
- [ ] No frame drops below 30fps during effects

---

### TASK 7.3 — Horror Audio Manager

> **Prompt:** Use **Session Opener** + ask for this file alone. Specify Audio Mixer group names.

**File: `Assets/_Game/Scripts/Horror/HorrorAudioManager.cs`**

Audio Mixer groups: Master → Music, Ambient, SFX, Horror

Faction audio: Order gets orchestral + distant thunder. Cult gets droning atonal + reversed whispers.
Ritual Bell (Cult) / Beacon Pulse (Order): every 8 hours, checked on app open.

**Acceptance Criteria:**
- [ ] Faction-appropriate music in City and WorldMap
- [ ] Horror SFX trigger on `HorrorEventTriggeredEvent`
- [ ] 8-hour check-in plays on interval
- [ ] Audio Mixer exposed to settings (volume sliders)

---

### PHASE 7 COMPLETE CRITERIA
- [ ] Corruption spreads visually on map
- [ ] Patron awakening triggers UI distortion
- [ ] Audio atmosphere changes by faction and event
- [ ] Accessibility toggle respected
- [ ] Git tag: `v0.7.0-horror`

---
---

## PHASE 8 — MONETIZATION
### Goal: IAP works. Shop UI complete. Battle Pass implemented. Pity system tracking.

> **Session setup:** Paste **Session Opener** + append: *"All IAP receipt validation happens server-side via PlayFab CloudScript before any VC is credited. Never credit currency based on client-reported purchase success."*

---

### TASK 8.1 — IAP Setup

Install Unity IAP. Product IDs (create in App Store Connect and Google Play Console):
```
vc_small      $0.99   = 80 VC
vc_medium     $4.99   = 500 VC
vc_large      $9.99   = 1200 VC
vc_xlarge     $19.99  = 2800 VC
vc_huge       $49.99  = 8000 VC
battlepass    $4.99   = Season Battle Pass
starter_pack  $2.99   = First purchase only
```

**File: `Assets/_Game/Scripts/Network/IAPService.cs`**

Receipt → PlayFab CloudScript `ValidateIAPReceipt` → Apple/Google validation → credit VC.

**Acceptance Criteria:**
- [ ] Products load from store
- [ ] Sandbox purchase completes
- [ ] Receipt validated before VC credited
- [ ] Starter pack shown once only

---

### TASK 8.2 — Shop UI

> **Prompt:** Use **New UI Screen** master prompt. List 5 tabs and their content.

Pity counter: `PlayerPrefs.GetInt("pityCounter_standard", 0)` — shown as "Legendary in X opens". Updated in PlayFab per player, displayed from cached value.

**Acceptance Criteria:**
- [ ] All 5 tabs work
- [ ] VC purchase triggers IAP flow
- [ ] Chest opening goes through CloudScript
- [ ] Pity counter displayed accurately

---

### TASK 8.3 — Battle Pass

30-tier track. Stored in PlayFab Title Data. XP sources granted via CloudScript.
F2P players reach tier 15–18 in a normal play season.

**Acceptance Criteria:**
- [ ] All 30 tiers render
- [ ] XP updates after qualifying actions
- [ ] Free rewards claimable without purchase
- [ ] Paid rewards locked until purchase
- [ ] All grants through CloudScript

---

### PHASE 8 COMPLETE CRITERIA
- [ ] IAP works in sandbox
- [ ] Receipt validation server-side
- [ ] Shop UI functional
- [ ] Battle Pass progression works
- [ ] Git tag: `v0.8.0-monetization`

---
---

## PHASE 9 — PSYCHOLOGICAL ENGAGEMENT
### Goal: Daily login chain, push notifications, Zeigarnik exit screen, variable ratio chests active.

> **Session setup:** Paste **Session Opener** + append: *"Daily login uses server timestamp comparison — never device clock. Push notifications scheduled via Firebase Cloud Functions — never locally scheduled in Unity (users can manipulate device time)."*

---

### TASK 9.1 — Daily Login Chain

**File: `Assets/_Game/Scripts/Gameplay/DailyLoginSystem.cs`**

- Streak tracked in Firestore player document (`loginStreak`, `loginStreakUpdated`)
- Increment/reset logic uses server timestamp (Firestore `FieldValue.ServerTimestamp`)
- Mercy Token: PlayFab item `mercy_token`, auto-consumed on missed day (max 1/month, granted day 15)
- Reward schedule in PlayFab Title Data

**Acceptance Criteria:**
- [ ] Streak correct across real calendar days
- [ ] Mercy Token consumed on miss if available
- [ ] Rewards granted via CloudScript
- [ ] Popup shows correct day and streak progress

---

### TASK 9.2 — Push Notifications

Install Firebase Messaging SDK. FCM token stored in Firestore player document.

Triggers (all via Cloud Functions):
```
Building complete, March returned, 8-hour check-in,
Under attack, Patron threshold, KvK starting, Season ending
```

Permission requested on second app open, not first.

**Acceptance Criteria:**
- [ ] FCM token in player Firestore document
- [ ] Building complete notification fires at CloudScript finishTime
- [ ] 8-hour notification schedules correctly
- [ ] Permission on second open
- [ ] Tapping notification deep-links correctly

---

### TASK 9.3 — Zeigarnik Exit Screen

**File: `Assets/_Game/Scripts/UI/Popups/ExitSummaryPopup.cs`**

Shown for 2s on city exit if any active timers or pending resources. Auto-dismisses. Tap to dismiss early. No red text or alarming language.

```
"Before you go..."
[Building Name] completes in [time]
[X] PG waiting to collect
KvK begins in [time]
3 alliance members online
```

**Acceptance Criteria:**
- [ ] Appears only when relevant content exists
- [ ] Auto-dismisses after 2s
- [ ] No alarming colors or language

---

### PHASE 9 COMPLETE CRITERIA
- [ ] Daily login chain works across real calendar days
- [ ] Push notifications fire for all 7 trigger types
- [ ] Zeigarnik screen shows accurate data
- [ ] Git tag: `v0.9.0-engagement`

---
---

## PHASE 10 — SOFT LAUNCH PREP
### Goal: Tutorial, analytics, crash reporting, performance audit, app store submission ready.

---

### TASK 10.1 — Tutorial System (8 steps, resumable, state in PlayFab)
### TASK 10.2 — Analytics Events (Firebase Analytics, all events listed in Appendix F)
### TASK 10.3 — Crash Reporting (Firebase Crashlytics, no PII in reports)
### TASK 10.4 — Performance Audit (60fps on iPhone SE 2020, < 500MB memory)

Performance targets:
```
WorldMap (200 markers):     60fps sustained
City scene (full):          60fps sustained
Cold start to Login:        < 5 seconds
Memory (steady state):      < 500MB
Firestore listener update:  < 5ms
```

### PHASE 10 COMPLETE CRITERIA
- [ ] Tutorial complete and resumable
- [ ] Analytics verified in Firebase DebugView
- [ ] Crashlytics active, test crash visible in console
- [ ] Performance targets met on low-end device
- [ ] Privacy policy URL ready
- [ ] Git tag: `v1.0.0-rc1`

---
---

## APPENDIX A — COMPLETE FILE MANIFEST

```
Assets/_Game/Scripts/Core/
  GameManager.cs          ServiceLocator.cs      EventBus.cs
  GameEvents.cs           GameEnums.cs           GameConstants.cs

Assets/_Game/Scripts/Network/
  FirebaseService.cs      PlayFabService.cs      EconomyService.cs
  AllianceService.cs      IAPService.cs

Assets/_Game/Scripts/Player/
  LocalCityState.cs       PlayerProfile.cs

Assets/_Game/Scripts/Gameplay/Buildings/
  BuildingManager.cs      BuildingSlot.cs

Assets/_Game/Scripts/Gameplay/Resources/
  ResourceDisplay.cs

Assets/_Game/Scripts/Gameplay/Combat/
  MarchManager.cs         CombatResolver.cs

Assets/_Game/Scripts/Gameplay/
  DailyLoginSystem.cs     BattlePassManager.cs   TutorialManager.cs

Assets/_Game/Scripts/World/
  WorldMapController.cs   TileManager.cs         CorruptionSystem.cs
  CityMarker.cs

Assets/_Game/Scripts/UI/Screens/
  LoginScreen.cs          CityScreen.cs          WorldMapScreen.cs
  AllianceScreen.cs       ShopScreen.cs          BattlePassScreen.cs

Assets/_Game/Scripts/UI/Popups/
  FactionChoicePopup.cs   BuildingSlotPopup.cs   CombatEncounterPopup.cs
  LoginRewardPopup.cs     ExitSummaryPopup.cs    TileInfoPopup.cs

Assets/_Game/Scripts/UI/Components/
  ResourceBar.cs          TimerDisplay.cs        ProgressBar.cs
  BuildingCard.cs

Assets/_Game/Scripts/Horror/
  AmbientHorrorManager.cs HorrorAudioManager.cs

Assets/_Game/Scripts/Data/
  BuildingData.cs         MonsterData.cs         UnitData.cs
  PatronData.cs           ResearchNodeData.cs

Assets/_Game/Scripts/Editor/
  BuildAddressablesPreBuild.cs

Assets/link.xml
.cursorrules  (or .windsurfrules)
```

---

## APPENDIX B — CLOUDSCRIPT FUNCTION LIST

```
StartBuildingUpgrade        CompleteUpgrade           GetPlayerBuildings
StartMarch                  ResolveMarchVsMonster     ResolveMarchVsPlayer
OpenChest                   ClaimLoginReward          ClaimBattlePassReward
PurchaseWithVC              ValidateIAPReceipt        GetAllianceLeaderboard
GetPlayerLeaderboard        UpdatePlayerBattlePassXP
```

---

## APPENDIX C — FIREBASE CLOUD FUNCTIONS LIST

```
spawnMonsters()             — every 30 minutes
spreadCorruption()          — every 15 minutes
updateAlliancePower()       — every 1 hour
seasonTickServer()          — every 24 hours
sendBuildCompleteNotification(playerId, buildingName)
sendAttackNotification(targetPlayerId)
sendPatronThresholdNotification(serverId, patronId)
```

---

## APPENDIX D — REMOTE CONFIG DEFAULTS

```json
{
  "resource_gen_rate_multiplier": 1.0,
  "building_time_multiplier": 1.0,
  "pvp_shield_duration_hours": 8,
  "corruption_spread_rate": 0.05,
  "patron_awakening_threshold": 100000,
  "horror_event_frequency_hours": 24,
  "f2p_chest_drop_table_version": 1,
  "p2w_chest_drop_table_version": 1,
  "daily_login_bonus_multiplier": 1.0,
  "kvk_points_formula_version": 1,
  "tutorial_enabled": true,
  "battlepass_xp_multiplier": 1.0,
  "monster_spawn_rate_multiplier": 1.0,
  "max_monsters_per_server": 200,
  "march_speed_tiles_per_second": 0.1,
  "pity_counter_standard_chest": 60,
  "pity_counter_premium_chest": 30
}
```

---

## APPENDIX E — PLAYFAB CURRENCIES

```
PG  Pale Gold          Initial: 5000  Max: 999999999  (F2P soft currency)
VC  Void Crystals      Initial: 0     Max: 9999999   (premium currency)
EE  Eldritch Essence   Initial: 0     Max: 999999    (event currency, expires)
CP  Corruption Points  Initial: 0     Max: 9999999   (Cult only, not purchasable)
FT  Flame Tokens       Initial: 0     Max: 9999999   (Order only, not purchasable)
WM  Wanderer Marks     Initial: 0     Max: 9999999   (solo path, not purchasable)
```

---

## APPENDIX F — ANALYTICS EVENTS

```csharp
"tutorial_step_complete"    { step: int }
"faction_chosen"            { faction: string }
"building_upgraded"         { building_id: string, new_level: int }
"monster_killed"            { monster_id: string, level: int }
"alliance_joined"           { alliance_id: string }
"iap_initiated"             { product_id: string, price: float }
"iap_completed"             { product_id: string, price: float }
"chest_opened"              { chest_type: string, rarity: string }
"horror_event_seen"         { event_type: string }
"session_start"             { faction: string, city_power: long }
"d1_retained"               {}
"d7_retained"               {}
```
No PII in any event parameter. No player names, emails, or IDs.

---

## APPENDIX G — QUICK REFERENCE: WHEN TO USE WHICH PROMPT

| Situation | Use This Prompt |
|---|---|
| Starting any new session | Session Opener |
| Building a new UI screen or panel | New UI Screen |
| Writing a PlayFab CloudScript function | CloudScript Function |
| Building any horror/atmosphere effect | Horror Effect |
| Stuck on same issue 3+ times | Stuck (in fresh Claude.ai browser tab) |

---

## APPENDIX H — KNOWN WALLS QUICK REFERENCE

| Wall | Phase It Hits | Symptom | Solution Reference |
|---|---|---|---|
| Firebase/Android Gradle | Phase 1 | Duplicate class build error | Wall 1 in this document |
| Apple Sign-In | Phase 1 | Silent failure on iOS | Wall 2 — set aside a full day |
| Firestore map performance | Phase 4 | FPS drop / rate limit errors | Wall 3 — use bounds query |
| CloudScript invisible failures | Phase 3 | `{ success: false }` no reason | Wall 4 — enable logging first |
| Addressables not found | Phase 0 | `InvalidKeyException` on device | Wall 5 — run pre-build script |
| UniTask + Firebase conflict | Phase 1 | Async callbacks don't fire | Wall 6 — use `.AsUniTask()` |
| IL2CPP strips PlayFab | Phase 8 | Works in debug, fails in release | Wall 7 — create `link.xml` |

---

*End of VRD v1.1.0 — Eldritch Dominion — Vibe Coding Edition*

*Feed one phase at a time. Wait for the git tag before moving forward.*
*If the agent is stuck: open a fresh Claude.ai browser conversation and use the Stuck prompt.*
