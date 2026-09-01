mergeInto(LibraryManager.library, {
    InitWebMCPTools: function () {
        // Ensure the global WebMCP object exists
        const mcpContext = navigator.modelContext || document.modelContext;
        if (!mcpContext || typeof mcpContext.registerTool !== 'function') {
            console.error("WebMCP API not found. Ensure the Chrome flag is enabled.");
            return;
        }

        window.mcpMessageQueue = [];
        window.mcpPendingResolve = null;
        window.mcpLocationResolve = null;

        // Register the Speak Tool
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

        mcpContext.registerTool({
            name: "wait",
            description: "Waits on the event queue for player interaction. Call this to listen for replies.",
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
                                resolve("wait again");
                            }
                        }, 30000);
                    }
                });
            }
        });

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
                    window.unityInstance.SendMessage("NPCManager", "GetNPCLocationContext", payload);
                });
            },
        });



        console.log("WebMCP Interrogation Tools Registered!");
    },

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

    ReturnNPCLocation: function (resultPtr) {
        var result = UTF8ToString(resultPtr);
        if (window.mcpLocationResolve) {
            window.mcpLocationResolve(result);
            window.mcpLocationResolve = null;
        }
    }

});

