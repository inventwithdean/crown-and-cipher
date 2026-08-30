mergeInto(LibraryManager.library, {
    InitWebMCPTools: function () {
        // Ensure the global WebMCP object exists
        const mcpContext = navigator.modelContext || document.modelContext;
        if (!mcpContext || typeof mcpContext.registerTool !== 'function') {
            console.error("WebMCP API not found. Ensure the Chrome flag is enabled.");
            return;
        }
        // Register the Speak Tool
        mcpContext.registerTool({
            name: "speak",
            description: "Sends text dialogue to the NPC with given id.",
            inputSchema: {
                type: "object",
                properties: {
                    id: { type: "integer", description: "The ID of the NPC" },
                    dialogue: { type: "string", description: "The text to say to the NPC" }
                },
                required: ["id", "dialogue"]
            },
            execute: function (args) {
                var payload = JSON.stringify({id: args.id, dialogue: args.dialogue});
                window.unityInstance.SendMessage('WebMCPManager', 'ReceiveDialogue', payload);
                return "Speaking...";
            },
        });
        
        console.log("WebMCP Interrogation Tools Registered!");
        // window.unityInstance.SendMessage('WebMCPManager', 'ReceiveDialogue', 2, "HEYYY");
    }
});

