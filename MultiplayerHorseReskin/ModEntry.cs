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
        internal static IMonitor SMonitor;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;

            // Events
            IModEvents events = helper.Events;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
            // helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            // SMAPI Commands
            helper.ConsoleCommands.Add("list_horses", "Lists the names of all horses on your farm.", Framework.CommandHandler.OnCommandReceived);
            // helper.ConsoleCommands.Add("list_farmers", "Lists the names and Multiplayer ID of all farmers", Framework.CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("reskin_horse", "Specify horse id and skin id you want to assign to it", Framework.CommandHandler.OnCommandReceived);

        }

        /*********
        ** Private methods
        *********/
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // TODO: LoadHorseSprites
            foreach(var d in GetHorsesDict())
            {
                LoadHorseSprites(d.Value);
            }

        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
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

        /// <summary>Raised after a mod message is received over the network.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // TODO: handle mod message received for horse reskin by farmhands
        }

        /*********
        ** Public methods
        *********/

        /// <summary>
        /// Gets all horses in game
        /// </summary>
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

        /// <summary>
        /// Gets Horse by id
        /// </summary>
        /// <param name="horseId">id of horse you wish to get</param>
        /// <returns>Horse object</returns>
        public static Horse GetHorseById(Guid horseId) { return GetHorsesDict()[horseId]; }


        /// <summary>
        /// Gets all stables that are fully constructed and contain a horse (i.e. not a tractor)
        /// </summary>
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

        /// <summary>
        /// Checks if given horse is not a tractor
        /// </summary>
        /// <param name="horse">Horse object</param>
        /// <returns>true if not a tractor</returns>
        public static bool IsNotATractor(Horse horse) { return !horse.Name.StartsWith("tractor/"); }

        public static void LoadHorseSprites(Horse horse)
        {
            // TODO: better handling of horse.Manners
            if (horse.Manners != 0)
                horse.Sprite = new AnimatedSprite($"Animals\\MultiplayerHorseReskin\\horse_{horse.Manners}", 7, 32, 32);
        }

        public static void SaveHorseReskin(Guid horseId, int skinId)
        {
            // TODO: only on main player => Context.IsMainPlayer
            // TODO: some validation?
            var horse = GetHorseById(horseId);
            if (horse != null)
                horse.Manners = skinId;
        }
    }
}
