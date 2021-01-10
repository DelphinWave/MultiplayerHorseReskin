using System;
using System.Collections.Generic;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace MultiplayerHorseReskin.Framework
{
    class CommandHandler
    {
        /// <summary>
        /// Handles SMAPI commands
        /// </summary>
        /// <param name="command">The command entered in SMAPI console</param>
        /// <param name="args">The arguments entered with the command</param>
        internal static void OnCommandReceived(string command, string[] args)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
            {
                ModEntry.SMonitor.Log("Your farm has not loaded yet, please try command again once farm is loaded", LogLevel.Warn);
                return;
            }

            if (!Context.IsMainPlayer)
            {
                ModEntry.SMonitor.Log("Only the host can write commands && commands are not currently supported during split-screen multiplayer", LogLevel.Warn);
                return;
            }


            switch (command)
            {
                case "list_horses":
                    foreach(var d in ModEntry.GetHorsesDict())
                    {
                        ModEntry.SMonitor.Log($"{d.Key} - {d.Value.displayName}", LogLevel.Info);
                    }
                    return;
                case "list_farmers":
                    foreach (Farmer farmer in Game1.getAllFarmers())
                        ModEntry.SMonitor.Log($"- {farmer.displayName}: {farmer.uniqueMultiplayerID}", LogLevel.Info);
                    return;
                case "reskin_horse":
                    if (args.Length < 2 || args.Length > 2)
                    {
                        ModEntry.SMonitor.Log($"reskin_horse requires 2 arguments, the id of the horse you wish to reskin and the id of the skin you want for that horse", LogLevel.Error);
                        return;
                    }
                    var horseId = args[0];
                    var skinId = args[1];
                    // TODO: actually send a multiplayer mod message
                    // TODO: better handling
                    ModEntry.SaveHorseReskin(Guid.Parse(horseId), Int32.Parse(skinId));
                    ModEntry.LoadHorseSprites(ModEntry.GetHorseById(Guid.Parse(horseId)));
                    return;
                default:
                    ModEntry.SMonitor.Log($"Unknown command '{command}'.", LogLevel.Error);
                    return;
            }
        }
    }
}
