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
        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            // helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /*********
        ** Private methods
        *********/
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Set sprite for all horses that are not the player's horse
            foreach (Horse horse in GetHorses())
                if(horse.ownerId != Game1.player.uniqueMultiplayerID)
                    horse.Sprite = new AnimatedSprite("Animals\\MultiplayerHorseReskin\\horse", 7, 32, 32);

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
                        Monitor.Log("---------- Clicked -------------", LogLevel.Debug);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all horses in game
        /// </summary>
        /// <returns>List of horses</returns>
        public List<Horse> GetHorses()
        {
            List<Horse> horses = new List<Horse>();
            foreach (NPC npc in Utility.getAllCharacters())
                if (npc is Horse && IsNotATractor(npc as Horse))
                    horses.Add(npc as Horse);
            return horses;
        }

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
    }
}
