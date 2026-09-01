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

        // Register the Speak Tool
        mcpContext.registerTool({
            name: "speak",
            description: "Sends text dialogue to the NPC with given id.",
            inputSchema: {
                type: "object",
                properties: {
                    npc_id: { type: "string", description: "The ID of the NPC" },
                    dialogue: { type: "string", description: "The text to say to the player." }
                },
                required: ["npc_id", "dialogue"]
            },
            execute: function (args) {
                var payload = JSON.stringify({ npc_id: args.npc_id, dialogue: args.dialogue });
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

        console.log("WebMCP Interrogation Tools Registered!");
        // window.unityInstance.SendMessage('WebMCPManager', 'ReceiveDialogue', 2, "HEYYY");
    },

    EnqueuePlayerMessage: function (npcIdPtr, messagePtr) {
        var npcId = UTF8ToString(npcIdPtr);
        var message = UTF8ToString(messagePtr);
        var payload = JSON.stringify({ event: "player_spoke", npcId: npcId, message: message });
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
        
        var payload = JSON.stringify({ event: message});

        if (window.mcpPendingResolve) {
            var resolve = window.mcpPendingResolve;
            window.mcpPendingResolve = null;
            resolve(payload);
        } else {
            window.mcpMessageQueue.push(payload);
        }
    }

});

