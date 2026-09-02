mergeInto(LibraryManager.library, {
    InitWebMCPTools: function () {
        // Ensure the global WebMCP object exists
        const mcpContext = navigator.modelContext || document.modelContext;
        if (!mcpContext || typeof mcpContext.registerTool !== 'function') {
            console.error("WebMCP API not found. Ensure the Chrome flag is enabled.");
            return;
        }

        window.mcpMessageQueue = [];
        // For the wait tool.
        window.mcpPendingResolve = null;
        // For location, this will be resolved instantly
        window.mcpLocationResolve = null;
        // For the system context, this will be resolved instantly as well
        window.mcpPendingResolve = null;

        // NPC's speak Tool
        mcpContext.registerTool({
            name: "speak",
            description: "The NPC with the given name will speak the dialogue to the player.",
            inputSchema: {
                type: "object",
                properties: {
                    npc_name: { type: "string", description: "The Name of the NPC" },
                    dialogue: { type: "string", description: "The text to say to the player." }
                },
                required: ["npc_name", "dialogue"]
            },
            execute: function (args) {
                var payload = JSON.stringify({ npcName: args.npc_name, dialogue: args.dialogue });
                window.unityInstance.SendMessage('WebMCPManager', 'ReceiveDialogue', payload);
                return "Speaking...";
            },
        });

        // Wait tool
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

        // Tool to return NPC's 2 closest locations
        mcpContext.registerTool({
            name: "get_npc_location",
            description: "Returns the top 2 closest locations to the specified NPC.",
            inputSchema: {
                type: "object",
                properties: {
                    npc_name: { type: "string", description: "The Name of the NPC" }
                },
                required: ["npc_name"]
            },
            execute: function (args) {
                return new Promise((resolve) => {
                    window.mcpLocationResolve = resolve;
                    var payload = JSON.stringify({ npcName: args.npc_name });
                    // NPCManager will call the ReturnNPCLocation function
                    window.unityInstance.SendMessage("NPCManager", "GetNPCLocationContext", payload);
                });
            },
        });

        // Tool to setup the story
        mcpContext.registerTool({
            name: "get_system_context",
            description: "Fetches the context you'll need. This should be called before any other tool call, and only once.",
            inputSchema: {
                type: "object",
                properties: {},
                required: []
            },
            execute: function (args) {
                return new Promise((resolve) => {
                    window.mcpSystemContextResolve = resolve;
                    // WebMCPManager will call the ReturnSystemContext function
                    window.unityInstance.SendMessage("WebMCPManager", "GetSystemContext");
                });
            }
        });

        console.log("WebMCP Tools Registered!");
    },

    // When player sends a message to an NPC, WebMCPManager will call this function after receiving it from DialogueUI's delegate subscription
    EnqueuePlayerMessage: function (npcNamePtr, messagePtr) {
        var npcName = UTF8ToString(npcNamePtr);
        var message = UTF8ToString(messagePtr);
        var payload = JSON.stringify({ event: "player_spoke", npc_name: npcName, message: message });
        if (window.mcpPendingResolve) {
            var resolve = window.mcpPendingResolve;
            window.mcpPendingResolve = null;
            resolve(payload);
        } else {
            window.mcpMessageQueue.push(payload);
        }
    },

    // For interactions, this isn't being used as of now.
    EnqueueInteractionEvent: function (messagePtr) {
        var message = UTF8ToString(messagePtr);

        var payload = JSON.stringify({ event: message });

        if (window.mcpPendingResolve) {
            var resolve = window.mcpPendingResolve;
            window.mcpPendingResolve = null;
            resolve(payload);
        } else {
            window.mcpMessageQueue.push(payload);
        }
    },

    // 
    ReturnNPCLocation: function (resultPtr) {
        var result = UTF8ToString(resultPtr);
        if (window.mcpLocationResolve) {
            window.mcpLocationResolve(result);
            window.mcpLocationResolve = null;
        }
    },

    ReturnSystemContext: function (resultPtr) {
        var result = UTF8ToString(resultPtr);
        if (window.mcpSystemContextResolve) {
            window.mcpSystemContextResolve(result);
            window.mcpSystemContextResolve = null;
        }
    }

});

