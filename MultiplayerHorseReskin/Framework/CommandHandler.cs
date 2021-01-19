using System;
using StardewModdingAPI;

namespace MultiplayerHorseReskin.Framework
{

    class HorseReskinMessage
    {
        public Guid horseId;
        public int skinId;
        public HorseReskinMessage(Guid horseId, int skinId)
        {
            this.horseId = horseId;
            this.skinId = skinId;
        }
    }
    class CommandHandler
    {
        /// <summary>
        /// Handles SMAPI commands
        /// </summary>
        /// <param name="command">The command entered in SMAPI console</param>
        /// <param name="args">The arguments entered with the command</param>
        internal static void OnCommandReceived(string command, string[] args)
        {
            if (!ModEntry.IsEnabled)
                return;

            // Ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
            {
                ModEntry.SMonitor.Log("Your farm has not loaded yet, please try command again once farm is loaded", LogLevel.Warn);
                return;
            }

            // Skin id (1-8)
            var skinId = "0";

            switch (command)
            {
                case "list_horses":
                    foreach(var d in ModEntry.GetHorsesDict())
                    {
                        ModEntry.SMonitor.Log($"{d.Key} - {d.Value.displayName}", LogLevel.Info);
                    }
                    return;
                case "reskin_horse":
                    if (args.Length < 2 || args.Length > 2)
                    {
                        ModEntry.SMonitor.Log($"reskin_horse requires 2 arguments, the name of the horse you wish to reskin and the id of the skin (1-8) you want for that horse", LogLevel.Error);
                        return;
                    }
                    var horseName = args[0];
                    skinId = args[1];
                    var horseIdFromName = ModEntry.GetHorseIdFromName(horseName);
                    if (horseIdFromName != null)
                    {
                        if (Context.IsMainPlayer)
                        {
                            ModEntry.SaveHorseReskin((Guid)horseIdFromName, int.Parse(skinId));
                        }
                        else
                        {
                            ModEntry.SHelper.Multiplayer.SendMessage(
                                message: new HorseReskinMessage((Guid)horseIdFromName, int.Parse(skinId)),
                                messageType: ModEntry.ReskinHorseMessageId,
                                modIDs: new[] { ModEntry.SModManifest.UniqueID }
                            );
                        }
                    }
                    return;
                case "reskin_horse_id":
                    if (args.Length < 2 || args.Length > 2)
                    {
                        ModEntry.SMonitor.Log($"reskin_horse requires 2 arguments, the id of the horse you wish to reskin and the id of the skin you want for that horse", LogLevel.Error);
                        return;
                    }
                    var horseId = args[0];
                    skinId = args[1];
                    
                    if (Context.IsMainPlayer)
                    {
                        ModEntry.SaveHorseReskin(Guid.Parse(horseId), int.Parse(skinId));
                    }
                    else
                    {
                        ModEntry.SHelper.Multiplayer.SendMessage(
                            message: new HorseReskinMessage(Guid.Parse(horseId), int.Parse(skinId)),
                            messageType: ModEntry.ReskinHorseMessageId,
                            modIDs: new[] { ModEntry.SModManifest.UniqueID }
                        );
                    }
                    return;
                default:
                    ModEntry.SMonitor.Log($"Unknown command '{command}'.", LogLevel.Error);
                    return;
            }
        }
    }
}
