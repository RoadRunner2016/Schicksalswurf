using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    using Characters;

    /// <summary>
    /// NPC types and dialogue system for dungeon interactions.
    /// </summary>
    public enum NPCType
    {
        Merchant,
        QuestGiver,
        Healer,
        Sage
    }

    public class DialogueNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<DialogueOption> Options { get; set; } = new();
    }

    public class DialogueOption
    {
        public string Text { get; set; }
        public string NextNodeId { get; set; }
        public System.Action OnSelect { get; set; }
    }

    public class NPC
    {
        public string Name { get; set; }
        public NPCType Type { get; set; }
        public string Greeting { get; set; }
        public Dictionary<string, DialogueNode> Dialogue { get; set; } = new();
        public string StartNodeId { get; set; } = "start";

        // Merchant stock
        public bool IsMerchant => Type == NPCType.Merchant;
        public bool IsHealer => Type == NPCType.Healer;
        public bool IsQuestGiver => Type == NPCType.QuestGiver;

        public int HealCost { get; set; } = 10;
    }

    public static class NPCFactory
    {
        public static NPC CreateMerchant()
        {
            var npc = new NPC
            {
                Name = "Wanderhaendler",
                Type = NPCType.Merchant,
                Greeting = "Gruss, Reisender! Ich habe allerlei Waren im Angebot."
            };

            npc.Dialogue["start"] = new DialogueNode
            {
                Id = "start",
                Text = npc.Greeting,
                Options = new()
                {
                    new() { Text = "Zeig mir deine Waren.", NextNodeId = "shop" },
                    new() { Text = "Was machst du hier unten?", NextNodeId = "info" },
                    new() { Text = "Leb wohl.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["shop"] = new DialogueNode
            {
                Id = "shop",
                Text = "Hier sind meine Waren. Waehle weise.",
                Options = new()
                {
                    new() { Text = "Zurueck.", NextNodeId = "start" }
                }
            };
            npc.Dialogue["info"] = new DialogueNode
            {
                Id = "info",
                Text = "Ich handle mit Abenteurern wie euch. Die Dungeon-Tiefen bergen Gefahren, aber auch Reichtum.",
                Options = new()
                {
                    new() { Text = "Zurueck.", NextNodeId = "start" }
                }
            };
            npc.Dialogue["end"] = new DialogueNode
            {
                Id = "end",
                Text = "Moge dein Weg sicher sein.",
                Options = new()
            };

            return npc;
        }

        public static NPC CreateHealer()
        {
            var npc = new NPC
            {
                Name = "Einsiedler-Heiler",
                Type = NPCType.Healer,
                Greeting = "Friede sei mit dir. Ich kann deine Wunden heilen.",
                HealCost = 15
            };

            npc.Dialogue["start"] = new DialogueNode
            {
                Id = "start",
                Text = npc.Greeting + $" (Kosten: {npc.HealCost} Gold pro Held)",
                Options = new()
                {
                    new() { Text = "Heile die gesamte Gruppe.", NextNodeId = "heal" },
                    new() { Text = "Danke, nicht noetig.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["heal"] = new DialogueNode
            {
                Id = "heal",
                Text = "Die Heilung ist gewaehrt. Moget ihr stark bleiben.",
                Options = new()
                {
                    new() { Text = "Danke.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["end"] = new DialogueNode
            {
                Id = "end",
                Text = "Geh in Frieden.",
                Options = new()
            };

            return npc;
        }

        public static NPC CreateQuestGiver()
        {
            var npc = new NPC
            {
                Name = "Verwundeter Abenteurer",
                Type = NPCType.QuestGiver,
                Greeting = "Bitte, helft mir! Mein Gefaehrte wurde von Monstern ueberwältigt..."
            };

            npc.Dialogue["start"] = new DialogueNode
            {
                Id = "start",
                Text = npc.Greeting,
                Options = new()
                {
                    new() { Text = "Was ist passiert?", NextNodeId = "story" },
                    new() { Text = "Ich kann jetzt nicht helfen.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["story"] = new DialogueNode
            {
                Id = "story",
                Text = "Wir wurden von Goblins ueberfallen. Bitte, besiegt 5 Goblins fuer mich!",
                Options = new()
                {
                    new() { Text = "Ich werde helfen.", NextNodeId = "accept" },
                    new() { Text = "Vielleicht spaeter.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["accept"] = new DialogueNode
            {
                Id = "accept",
                Text = "Danke! Ich werde hier warten. Kehrt zurueck, wenn ihr erledigt habt, was ich bat.",
                Options = new()
                {
                    new() { Text = "Verstanden.", NextNodeId = "end" }
                }
            };
            npc.Dialogue["end"] = new DialogueNode
            {
                Id = "end",
                Text = "Geh mit Vorsicht.",
                Options = new()
            };

            return npc;
        }
    }
}
