using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

namespace MultiplayerHorseReskin
{
    public class ModEntry : Mod
    {
        // Static IMonitor
        internal static IMonitor SMonitor;

        // Static Helper
        internal static IModHelper SHelper;

        // Static ModManifest
        internal static IManifest SModManifest;

        // Whether the mod is enabled for the current farmhand.
        internal static bool IsEnabled = true;

        // The minimum version the host must have for the mod to be enabled on a farmhand.\
        private readonly string MinHostVersion = "1.0.0";

        // A request from a farmhand to reskin a horse
        internal static readonly string ReskinHorseMessageId = "HorseReskin";

        // Inform farmhands to update horse sprites
        internal static readonly string ReloadHorseSpritesMessageId = "HorseSpriteReload";


        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;
            SModManifest = ModManifest;

            // Events
            IModEvents events = helper.Events;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
            // helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            // SMAPI Commands
            SHelper.ConsoleCommands.Add("list_horses", "Lists the names of all horses on your farm.", Framework.CommandHandler.OnCommandReceived);
            SHelper.ConsoleCommands.Add("reskin_horse", "Specify [horse name] and the [skin id] (1-8) you want to assign to it. Try list_horses to see available horses.", Framework.CommandHandler.OnCommandReceived);
            SHelper.ConsoleCommands.Add("reskin_horse_id", "Specify [horse id] and the [skin id] (1-8) you want to assign to it. Try list_horses to see available horses.", Framework.CommandHandler.OnCommandReceived);

        }

        /*********
        ** Private methods
        *********/
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {

            // check if mod should be enabled for the current player
            IsEnabled = Context.IsMainPlayer;
            if (!IsEnabled)
            {
                ISemanticVersion hostVersion = SHelper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;
                if (hostVersion == null)
                {
                    IsEnabled = false;
                    SMonitor.Log("This mod is disabled because the host player doesn't have it installed.", LogLevel.Warn);
                }
                else if (hostVersion.IsOlderThan(this.MinHostVersion))
                {
                    IsEnabled = false;
                    SMonitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {this.MinHostVersion}.", LogLevel.Warn);
                }
                else
                    IsEnabled = true;
            }
            // TODO: LoadHorseSprites
            foreach (var d in GetHorsesDict())
            {
                LoadHorseSprites(d.Value);
            }

        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (!IsEnabled)
                return;

            bool IsPlayerInStable(Building stable)
            {
                var stableRect = new Rectangle(stable.tileX, stable.tileY, stable.tilesWide, stable.tilesHigh);
                if (stableRect.Contains(new Point(Game1.player.getTileX(), Game1.player.getTileY()))) // player standing in stable
                    return true;
                return false;
            }

            if(e.Button.IsActionButton())
            {

                foreach(Stable stable in GetHorseStables())
                {
                    if (IsPlayerInStable(stable))
                    {
                        // TODO: suppress action if mounting or dismounting horse
                        // TODO: present texture options for horses
                        SMonitor.Log("---------- Clicked -------------", LogLevel.Debug);
                        break;
                    }
                }
            }
        }

        /// <summary> Raised after a mod message is received over the network. </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            SMonitor.Log($"OnModMessageReceived", LogLevel.Debug);
            // TODO: handle mod message received for horse reskin by farmhands
            if (e.Type == ReskinHorseMessageId && Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                SMonitor.Log($"OnModMessageReceived - IsMainPlayer", LogLevel.Debug);
                Framework.HorseReskinMessage message = e.ReadAs<Framework.HorseReskinMessage>();
                SaveHorseReskin(message.horseId, message.skinId);
                return;
            }
            if(e.Type == ReloadHorseSpritesMessageId && !Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                SMonitor.Log($"OnModMessageReceived - !IsMainPlayer", LogLevel.Debug);
                Framework.HorseReskinMessage message = e.ReadAs<Framework.HorseReskinMessage>();
                LoadHorseSprites(GetHorseById(message.horseId));
            }
        }

        /*********
        ** Public methods
        *********/

        /// <summary> Gets all horses in game </summary>
        /// <returns>Dictionary of horses</returns>
        public static Dictionary<Guid, Horse> GetHorsesDict()
        {
            Dictionary<Guid, Horse> horses = new Dictionary<Guid, Horse>();
            foreach (NPC npc in Utility.getAllCharacters())
                if (npc is Horse && IsNotATractor(npc as Horse))
                {
                    Horse horse = npc as Horse;
                    horses.Add(horse.HorseId, npc as Horse);
                }
                    

            return horses;
        }

        /// <summary> Gets Horse by id </summary>
        /// <param name="horseId">id of horse you wish to get</param>
        /// <returns>Horse object</returns>
        public static Horse GetHorseById(Guid horseId) { return GetHorsesDict()[horseId]; }


        /// <summary> Gets all stables that are fully constructed and contain a horse (i.e. not a tractor) </summary>
        /// <returns>List of valid stables</returns>
        public static List<Stable> GetHorseStables()
        {
            List<Stable> stables = new List<Stable>();
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building is Stable)
                {
                    Stable stable = building as Stable;
                    Horse horse = Utility.findHorse(stable.HorseId);
                    if (horse != null && IsNotATractor(horse))
                        stables.Add(stable);
                }
            }
            return stables;
        }

        /// <summary> Checks if given horse is not a tractor </summary>
        /// <param name="horse">Horse object</param>
        /// <returns>true if not a tractor</returns>
        public static bool IsNotATractor(Horse horse) { return !horse.Name.StartsWith("tractor/"); }

        public static void LoadHorseSprites(Horse horse)
        {

            // TODO: better handling of horse.Manners
            if (horse.Manners > 0)
            {
                horse.Sprite = new AnimatedSprite($"Animals\\MultiplayerHorseReskin\\horse_{horse.Manners}", 7, 32, 32);
                SMonitor.Log($"Loaded skin {horse.Manners} for horse {horse.displayName}", LogLevel.Info);
            } else
            {
                SMonitor.Log($"No skin was set for {horse.displayName}", LogLevel.Info);
            }
        }

        public static void SaveHorseReskin(Guid horseId, int skinId)
        {
            if (!Context.IsMainPlayer)
                return;
            // TODO: some validation?
            var horse = GetHorseById(horseId);
            if (horse != null)
            {
                horse.Manners = skinId;
                SMonitor.Log($"Saving skin {skinId} to horse {horse.displayName}", LogLevel.Info);

                LoadHorseSprites(horse);

                // TODO: is there a way to wait for next save/tick?
                SHelper.Multiplayer.SendMessage(
                    message: new Framework.HorseReskinMessage(horseId, skinId), // TODO: new message class, redundant skinId
                    messageType: ReloadHorseSpritesMessageId,
                    modIDs: new[] { SModManifest.UniqueID }
                );
            }
        }

        public static Guid? GetHorseIdFromName(string horseName)
        {
            foreach (var d in GetHorsesDict())
                if (d.Value.displayName == horseName)
                    return d.Key;

            SMonitor.Log($"No horse named {horseName} was found", LogLevel.Error);
            return null;
        }
    }
}
