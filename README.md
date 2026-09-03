# Crown and Cipher 🕵️‍♂️👑

> **Infinite, personalized narrative generation in an open 3D world powered by WebMCP and Unity 6.**

* **Live Demo:** [http://webmcpgames.org/crown-and-cipher](http://webmcpgames.org/crown-and-cipher)
* **Video Walkthrough:** [YouTube Demo](https://youtu.be/buW_ZOx5Ps8)

![Screenshot](./poster.png)

---

## Overview

Traditional games rely on static branching dialogue trees. Generative AI offers infinite narratives, but historically introduces two massive bottlenecks:
1. **Server GPU Costs:** Hosting real-time LLM inference for every active player quickly becomes unsustainable for game developers.
2. **The Polling Trap:** Having an agent constantly inspect game state wastes tokens and hits rate limits.

**Crown and Cipher** solves both issues using **WebMCP**. By running Unity 6 WebGL directly in the browser and exposing structured tools to the client's model context, the agent acts as an autonomous Dungeon Master weaving mystery paths, roleplaying suspects, and validating win conditions at **zero server inference cost** to the developer.

---

## How It Works: Unity ↔ WebMCP Bridge

Unity’s C# runtime and the browser’s WebMCP JavaScript API communicate bi-directionally via native WebGL interop (`[DllImport("__Internal")]` and `window.unityInstance.SendMessage`):

* **Registration & Initialization:** At startup, `WebMCPManager.cs` calls `InitWebMCPTools()`. The `.jslib` bridge registers tools (`get_system_context`, `speak`, `wait`, `get_npc_location`, and `show_winning_panel`) directly with `navigator.modelContext` / `document.modelContext`.
* **Dynamic Murder Setup:** The agent invokes `get_system_context`, triggering Unity to dynamically pick a killer, assign 3 key leads, and build an indirect knowledge graph before handing the constraint prompt to the LLM.
* **NPC Dialogue:** When the agent speaks, it calls `speak({ npc_name, dialogue })`. The bridge uses `unityInstance.SendMessage` to dispatch the dialogue to Unity's `DialogueUI`.
* **Player Input:** When the player talks back, Unity forwards the text through `EnqueuePlayerMessage()` into the browser queue.

### Inspect the WebMCP Source
* **Unity Controller:** [`WebMCPManager.cs`](./Assets/Scripts/WebMCPManager.cs)
* **WebMCP Bridge:** [`WebMCPBridge.jslib`](./Assets/Plugins/WebMCPBridge.jslib)

---

## The `wait` Tool: Event-Driven Game Loop

The core breakthrough of this project is avoiding continuous polling loops. Instead of asking the game *"Has anything happened yet?"* every few seconds, the agent executes `wait` and enters a non-blocking standby state:

```javascript
// The wait tool
mcpContext.registerTool({
    name: "wait",
    description: "Waits on the event queue for in game interactions. Call this after every tool. If you get any failure to this tool call, call it again. This is the gameplay loop.",
    inputSchema: { "type": "object", properties: {}, required: [] },
    execute: function (args) {
        return new Promise((resolve) => {
            if (window.mcpMessageQueue.length > 0) {
                resolve(window.mcpMessageQueue.shift());
            } else {
                window.mcpPendingResolve = resolve;

                setTimeout(() => {
                    if (window.mcpPendingResolve === resolve) {
                        window.mcpPendingResolve = null;
                        resolve("call wait again.");
                    }
                }, 120000);
            }
        });
    }
});
```


1. After every action, the agent is instructed to call `wait`.
2. The tool leaves a JavaScript `Promise` open for up to 120 seconds without burning LLM context tokens.
3. The moment the player submits dialogue via the UI, Unity executes `EnqueuePlayerMessage()`.
4. If a promise is pending, it resolves immediately with `{ event: "player_spoke", npc_name, message }`. If the agent is busy, events queue safely in `mcpMessageQueue`.

---

## Registered WebMCP Tools

| Tool | Parameters | Description |
| :--- | :--- | :--- |
| `get_system_context` | *None* | Fetches the seed mystery, decides killer identity, creates the knowledge graph, and adds location context. Must be called first. |
| `wait` | *None* | Suspends the agent loop until a player interaction event fires (up to 120s timeout). |
| `speak` | `npc_name`, `dialogue` | Renders speech from an NPC's viewpoint directly into Unity's dialogue UI. |
| `get_npc_location` | `npc_name` | Queries Unity for the top 2 closest spatial landmarks near an NPC. |
| `show_winning_panel` | *None* | Triggers the victory UI and tears down player controls when the killer is caught. |

---

## Costs & The Future of Web-Based Games

* **Zero Marginal Cost per Player:** Running generative NPC narratives typically forces studios to maintain expensive LLM backend clusters. By leveraging client-side WebMCP agents, the user provides their own compute/agent while the developer simply hosts static WebGL assets.
* **True Procedural Replayability:** Unity's procedural knowledge-graph assignment ensures no two murder investigations share the same clues or killer path. A lot of games can be personalized to the player's liking, as their agent is already deeply personalized to them.
* **Low Latency, High Immersion:** By replacing token-heavy polling with suspended browser promises, games can orchestrate complex, real-time interactions.

---

## How to Play

> **Note on Assets:** Everything in this repo is MIT licensed but the 3D environment utilizes third-party Synty Studios assets bound by commercial licensing and is not distributed directly in this source repository. You can play the full, compiled production build instantly on the live site.

### 1. Enable WebMCP in Chrome
1. Make sure you are using a modern build of Google Chrome.
2. Navigate to `chrome://flags/#enable-webmcp-testing` in your address bar.
3. Set the flag to **Enabled**.

### 2. Launch the Game
1. Visit [webmcpgames.org/crown-and-cipher](http://webmcpgames.org/crown-and-cipher).
2. Connect your WebMCP-compatible browser agent (e.g. use an extension like [WebMCP Inspector](https://chromewebstore.google.com/detail/gbpdfapgefenggkahomfgkhfehlcenpd). The demo was recorded by using Gemini 3.6 Flash in this extension.)
3. The agent will initialize via `get_system_context`, assemble the procedural mystery, and begin guiding your investigation.

<br>

> **Note on ChatGPT Browser:** You can try it in ChatGPT's in-app browser but it doesn't support cursor lock as of now because of being an embedded browser and browser API issues. Controlling the first person character without cursor lock is frustrating. Let's hope that it gets fixed soon, as the possiblities of WebMCP games being generated and played in Codex are insane.

---

## LICENSE
Distributed under the [MIT License](./LICENSE)