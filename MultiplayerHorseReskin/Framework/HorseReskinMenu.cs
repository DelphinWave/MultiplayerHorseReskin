using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerHorseReskin.Framework
{
    class HorseReskinMenu : IClickableMenu
    {
        public int currentSkinId = 1;
        public Dictionary<int, Texture2D> skinTextureMap;
        public Guid horseId;

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent okButton;

        public ClickableTextureComponent horsePreview;
        

        

        private static readonly int horseSpriteWidth, horseSpriteHeight = horseSpriteWidth = 32;
        private static readonly float horsePreviewScale = 4f;
        private static readonly int horseSpriteIndex = 8;
        private static readonly int menuPadding = 64;

        private static readonly int maxWidthOfMenu = (horseSpriteWidth * (int)horsePreviewScale) + menuPadding;
        private static readonly int maxHeightOfMenu = (horseSpriteHeight * (int)horsePreviewScale) + menuPadding;

        private readonly int backButtonId = 44;
        private readonly int forwardButtonId = 33;
        private readonly int okButtonId = 46;


        public HorseReskinMenu(Guid horseId, Dictionary<int, Texture2D> skinTextureMap)
        {
            this.horseId = horseId;
            this.skinTextureMap = new Dictionary<int, Texture2D>(skinTextureMap);
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
            resetBounds();
        }

        public Texture2D CurrentHorseTexture => this.skinTextureMap[this.currentSkinId];

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(forwardButtonId);
            this.snapCursorToCurrentSnappedComponent();
        }

        //public override void receiveKeyPress(Keys key)
        //{

        //}

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.LeftTrigger)
            {
                this.currentSkinId--;
                if (this.currentSkinId < 1)
                    this.currentSkinId = this.skinTextureMap.Count - 1;

                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                updateHorsePreview();
            }
            if (b == Buttons.RightTrigger)
            {
                this.currentSkinId++;
                if (this.currentSkinId > skinTextureMap.Count)
                    this.currentSkinId = 1;

                this.forwardButton.scale = this.forwardButton.baseScale;
                Game1.playSound("shwip");
                updateHorsePreview();
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (this.backButton.containsPoint(x, y))
            {
                this.currentSkinId--;
                if (this.currentSkinId < 1)
                    this.currentSkinId = this.skinTextureMap.Count - 1;

                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                updateHorsePreview();
            }
            if (this.forwardButton.containsPoint(x, y))
            {
                this.currentSkinId++;
                if (this.currentSkinId > skinTextureMap.Count)
                    this.currentSkinId = 1;

                this.forwardButton.scale = this.forwardButton.baseScale;
                Game1.playSound("shwip");
                updateHorsePreview();
            }
            if (this.okButton.containsPoint(x, y))
            {
                selectSkin();
                base.exitThisMenu();
                Game1.playSound("smallSelect");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.backButton.tryHover(x, y);
            this.forwardButton.tryHover(x, y);
            this.okButton.tryHover(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.resetBounds();
        }

        public override void draw(SpriteBatch b) {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            base.draw(b);
            IClickableMenu.drawTextureBox(b, base.xPositionOnScreen, base.yPositionOnScreen, maxWidthOfMenu + menuPadding, maxHeightOfMenu + menuPadding, Color.White);

            this.horsePreview.draw(b);
            this.horsePreview.draw(b);
            this.backButton.draw(b);
            this.forwardButton.draw(b);
            this.okButton.draw(b);
            drawMouse(b);
        }

        private void selectSkin()
        {
            if (Context.IsMainPlayer)
            {
                ModEntry.SaveHorseReskin(horseId, currentSkinId);
            }
            else
            {
                ModEntry.SHelper.Multiplayer.SendMessage(
                    message: new HorseReskinMessage(horseId, currentSkinId),
                    messageType: ModEntry.ReskinHorseMessageId,
                    modIDs: new[] { ModEntry.SModManifest.UniqueID }
                );
            }
        }

        private void updateHorsePreview()
        {
            this.horsePreview = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + menuPadding, base.yPositionOnScreen + menuPadding, horseSpriteWidth, horseSpriteHeight), CurrentHorseTexture, Game1.getSourceRectForStandardTileSheet(CurrentHorseTexture, horseSpriteIndex, horseSpriteWidth, horseSpriteHeight), horsePreviewScale);
        }

        private void resetBounds()
        {
            base.xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfMenu / 2 - IClickableMenu.spaceToClearSideBorder;
            base.yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfMenu / 2 - IClickableMenu.spaceToClearTopBorder;
            base.width = maxWidthOfMenu + IClickableMenu.spaceToClearSideBorder;
            base.height = maxHeightOfMenu + IClickableMenu.spaceToClearTopBorder;
            base.initialize(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, showUpperRightCloseButton: true);

            this.updateHorsePreview();

            this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + menuPadding, base.yPositionOnScreen + base.height - menuPadding, 48, 44), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, backButtonId), 1f)
            {
                myID = backButtonId,
                rightNeighborID = forwardButtonId
            };
            this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - menuPadding, base.yPositionOnScreen + base.height - menuPadding, 48, 44), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, forwardButtonId), 1f)
            {
                myID = forwardButtonId,
                leftNeighborID = backButtonId,
                rightNeighborID = okButtonId
            };
            this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - menuPadding, base.yPositionOnScreen + base.height, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, okButtonId), 1f)
            {
                myID = okButtonId,
                leftNeighborID = forwardButtonId,
                rightNeighborID = -99998
            };
        }
    
    }
}
