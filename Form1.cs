using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Drawing.Text;
using System.Diagnostics;

namespace SuperMarioBros
{
    public partial class Form1 : Form
    {
        //
        // DUE TO CSV, READING IS (y,x) when associating with location
        //

        // Enables Debug Mode
        bool DebugMode = false;

        // Disables UI Elements (Causes lag (Unknown reason))
        bool CinematicMode = false;

        PrivateFontCollection PFC;
        Font customFont;

        bool hintMoveBugShown = false;
        bool hintTimeBugShown = false;
        bool hintCoinBug1Shown = false;
        bool hintCoinBug2Shown = false;
        bool hintJumpBugShown = false;
        bool hintScoreBugShown = false;
        bool hintFireBugShown = false;
        bool hintLifeBugShown = false;
        bool hintCameraBugShown = false;
        bool hintStateBugShown = false;
        bool hintInfiniteBlockBugShown = false;
        bool hintDeathCeilingBugShown = false;
        bool hintCombatBugShown = false;
        bool hintGhostWallBugShown = false;
        bool hintPacifistFireballBugShown = false;
        bool hintFlagBugShown = false;
        bool hintsEnabled = false;

        int hintTimerTrigger = -1;
        string scheduledHintTitle = "";
        string scheduledHintText = "";

        // Block variables
        List<string[]> level;
        List<Block> levelBlocks = new List<Block>();
        List<Block> toRemoveBlock = new List<Block>();
        List<Block> collidedBlocks = new List<Block>();
        List<string[]> lookUp;
        float frontX;
        float backX;
        float offset = 0;
        bool FirstLoad;
        int scaleSize = 32;
        string globalLevelName;
        bool isUnderground;
        float offsetOut;
        Block spawnBlock;

        // Mario variables
        private Player Mario;
        bool movementRight = true;
        bool movementLeft = true;
        bool movementDown = true;
        bool movementUp = true;
        float intersectStrictness = 0.75f;
        bool movingRight;
        bool movingLeft;
        bool movingUp;
        bool movingDown;
        float previousOffset;
        double previousJumpHeight = 0;
        float jumpHeight = 9.81f;
        bool marioIntersect = false;
        ResourceManager resourceManager;
        bool setCrouch;
        int currentState = 0;
        int Lives = 3;
        float speed = 7.35f;
        bool livesLost = false;

        // Luigi Variables
        private Player Luigi;
        bool movementRight2 = true;
        bool movementLeft2 = true;
        bool movementDown2 = true;
        bool movementUp2 = true;
        bool movingRight2;
        bool movingLeft2;
        bool movingUp2;
        bool movingDown2;
        bool luigiIntersect = false;
        bool setCrouch2;
        int currentState2 = 0;
        double previousJumpHeight2 = 0;

        int playerCount = 1;

        // UI Variables
        int timeCounter = 400;
        int timeSubCounter = 0;
        int score = 0;
        int coinCounter;
        int timer = 0;
        int invTimer = -1;

        // Ending variables
        private Entity Flag;
        bool ending = false;
        int endTime = 0;
        int levelTimer = 0;
        int flagSpeed = 2;

        // Fire Variables
        bool throwFire = false;
        int fireBuffer = 0;
        bool throwFire2 = false;
        int fireBuffer2 = 0;

        // Pipe Variables
        bool canPipe = false;
        bool movingStage = false;
        string pipeDirection;
        Rectangle pipeRec;
        bool pipeCheck1 = false;
        bool pipeCheck2 = false;

        // Entity Variables
        List<Entity> entityList = new List<Entity>();
        List<Entity> toRemoveEntity = new List<Entity>();
        List<Entity> toAddEntity = new List<Entity>();
        List<string[]> entityLookUp;
        float entityOffset;

        // World Variables
        int worldNum = 1;
        int levelNum = 1;

        // Menu Variables
        bool Loading = false;
        int loadTimer = -1;
        bool menuSelect = false;

        // Fullscreen Variables
        bool fullscreen = false;
        int gameWidth = 800;
        int windowBorderWidth = 0;
        int windowBorderHeight = 0;
        Font hudFont;
        Control[] hudControls;
        Point[] hudBaseLocation;
        Size[] hudBaseSize;
        Label fullscreenHint;
        bool fullscreenKeyHeld = false;

        public Form1()
        {
            InitializeComponent();
            CreateFullscreenHint();
            LevelLoad("TitleScreen", 0);
            StartLevel();
            MenuConfig();
            ReadFile("TopScore.txt");
        }

        // Small on screen note about the fullscreen key
        private void CreateFullscreenHint()
        {
            fullscreenHint = new Label();
            fullscreenHint.AutoSize = true;
            fullscreenHint.BackColor = Color.Transparent;
            fullscreenHint.ForeColor = Color.White;
            fullscreenHint.Text = "F11 - полный экран";
            fullscreenHint.Location = new Point(300, 424);
            this.Controls.Add(fullscreenHint);
        }

        private void LevelLoad(string levelName, float startOffset)
        {
            if (levelBlocks.Count > 0)
            {
                levelBlocks.Clear();
            }
            if (entityList.Count > 0)
            {
                entityList.Clear();
            }
            levelBlocks = new List<Block>();
            globalLevelName = levelName;

            entityOffset = startOffset;

            // Culling setup
            frontX = 28 * scaleSize;
            backX = -3 * scaleSize;

            // Finds level file (Must be csv, gets name from input)
            level = FormatCSV(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), (@"Levels\" + levelName + ".csv")));

            // Loads table of IDs
            lookUp = FormatCSV(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), (@"LookUpSheet\IDList.csv")));

            // Loads table of entities
            entityLookUp = FormatCSV(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), (@"LookUpSheet\EntityList.csv")));

            // Ensures level starts at correct location
            offset = startOffset;
            previousOffset = offset;

            if (levelName == "TitleScreen")
            {
                Menu1.Visible = true;
                Menu2.Visible = true;
                MenuTop.Visible = true;
                MenuSelector.Visible = true;
                MenuTop.Text = "TOP - " + Convert.ToInt32(ReadFile("TopScore.txt")).ToString("00000");
            }
            else
            {
                Menu1.Visible = false;
                Menu2.Visible = false;
                MenuTop.Visible = false;
                MenuSelector.Visible = false;
            }
            if (levelName == "Load")
            {
                MarioLife.Visible = true;
                LivesCountText.Visible = true;
                LivesCountText.Text = "x " + Lives.ToString("00");
            }
            else
            {
                MarioLife.Visible = false;
                LivesCountText.Visible = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Ensures graphics aren't "jumpy"
            DoubleBuffered = true;

            // Prevents window rescaling
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // Starts event handlers
            // (KeyDown is already hooked up by the designer, hooking it again here would
            //  run every key press twice, which breaks switches like fullscreen)
            tmrGame.Tick += new EventHandler(onTick);
            this.KeyUp += new KeyEventHandler(IsKeyUp);
            this.Paint += new PaintEventHandler(onPaint);

            // Timer setup
            tmrGame.Interval = 20;
            tmrGame.Start();
            spriteClock.Start();

            // Mario movement speed
            speed = (speed * scaleSize) * (20f / 1000f);

            resourceManager = new ResourceManager("SuperMarioBros.Properties.Resources", Assembly.GetExecutingAssembly());

            // Displays debug aspects
            if (DebugMode)
            {
                posXDebug.Visible = true;
                posYDebug.Visible = true;
                offsetDebug.Visible = true;
                GoombaSpeed.Visible = true;
            }

            // Hides UI elements
            if (CinematicMode)
            {
                scoreDisplay.Visible = false;
                ScoreLabel.Visible = false;
                CoinCount.Visible = false;
                WorldTitle.Visible = false;
                LevelName.Visible = false;
                TimeCounter.Visible = false;
                TimeLabel.Visible = false;
            }
        }

        private void StartLevel()
        {
            endTime = -1;
            levelTimer = -1;
            ending = false;

            // Allows for inital paint
            FirstLoad = false;

            // Creates each block, with appropriate special features for specific IDs
            for (int i = 0; i < level.Count; i++)
            {
                for (int j = 0; j < level[i].Length; j++)
                {
                    if (Convert.ToInt32(level[i][j]) == 999 || Convert.ToInt32(level[i][j]) == 998 || Convert.ToInt32(level[i][j]) == 997)
                    {
                        Mario = new Player(j * scaleSize, i * scaleSize, 1 * scaleSize, 1 * scaleSize, false, Brushes.Red, scaleSize, true, currentState, DebugMode, 0);
                    }
                    if (Convert.ToInt32(level[i][j]) == 899 || Convert.ToInt32(level[i][j]) == 898 || Convert.ToInt32(level[i][j]) == 897)
                    {
                        if (playerCount >= 2)
                        {
                            Luigi = new Player(j * scaleSize, i * scaleSize, 1 * scaleSize, 1 * scaleSize, false, Brushes.Red, scaleSize, true, currentState2, DebugMode, 1);
                        }
                    }
                    else if (Convert.ToInt32(level[i][j]) == 29)
                    {
                        Flag = new Entity("Flag", (j * scaleSize) + (int)(0.5f * scaleSize), i * scaleSize, 1 * scaleSize, 1 * scaleSize, true, scaleSize, offset, entityLookUp, 1, 0, DebugMode);
                        entityList.Add(Flag);
                    }
                    else if (Convert.ToInt32(level[i][j]) == 49)
                    {
                        entityList.Add(new Entity("UnderCoin", (j * scaleSize), i * scaleSize + scaleSize, 1 * scaleSize, 1 * scaleSize, true, scaleSize, offset, entityLookUp, 1, 0, DebugMode));
                    }
                    else if (Convert.ToInt32(level[i][j]) == 53)
                    {
                        offsetOut = (j * scaleSize) - (3 * scaleSize);
                    }
                }
            }

            // Ensures the play field is a comfortable size, then fits the window to it
            // (the same field is used in windowed and fullscreen mode, so the game plays the same)
            gameWidth = Math.Min(800, level[0].Length * scaleSize);
            ApplyDisplayMode();

            movingRight = false;
            movingLeft = false;
            setCrouch = false;
            Mario.setRun(false);

            if (Luigi != null)
            {
                movingRight2 = false;
                movingLeft2 = false;
                setCrouch2 = false;
                Luigi.setRun(false);
            }
        }

        private void MenuConfig()
        {
            if (PFC == null)
            {
                PFC = new PrivateFontCollection();
                string fontPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Font\Super Mario Bros. NES.ttf");

                if (File.Exists(fontPath))
                {
                    PFC.AddFontFile(fontPath);
                    customFont = new Font(PFC.Families[0], 12, FontStyle.Regular);
                }
                else
                {
                    customFont = new Font("Arial", 12, FontStyle.Regular);
                }
            }

            ScoreLabel.Font = customFont;
            scoreDisplay.Font = customFont;
            TimeLabel.Font = customFont;
            TimeCounter.Font = customFont;
            WorldTitle.Font = customFont;
            LevelName.Font = customFont;
            CoinCount.Font = customFont;

            LivesCountText.Font = customFont;
            MarioLife.Font = customFont;

            Menu1.Font = customFont;
            Menu2.Font = customFont;
            MenuTop.Font = customFont;

            // Sets LevelName at the top of the screen based on the level read
            LevelName.Text = worldNum + "-" + levelNum;

            // The interface is laid out in play field coordinates, so the same layout is used
            // in windowed and fullscreen mode (ApplyHudScale does the actual scaling)
            if (hudControls == null)
            {
                CaptureHudLayout();
            }
            else
            {
                RestoreHudLayout();
            }

            SetHudPosition(CoinCount, new Point(gameWidth / 3, GetHudY(CoinCount)));
            SetHudPosition(WorldTitle, new Point(((gameWidth / 8) * 5), GetHudY(WorldTitle)));
            SetHudPosition(LevelName, new Point(((gameWidth / 8) * 5) + (WorldTitle.Width / 5), GetHudY(LevelName)));

            SetHudPosition(Menu1, new Point((gameWidth / 2) - (Menu1.Width / 2), GetHudY(Menu1)));
            SetHudPosition(Menu2, new Point((gameWidth / 2) - (Menu2.Width / 2), GetHudY(Menu2)));
            SetHudPosition(MenuTop, new Point((gameWidth / 2) - (MenuTop.Width / 2), GetHudY(MenuTop)));

            SetHudPosition(LivesCountText, new Point((gameWidth / 2) - (LivesCountText.Width / 2), GetHudY(LivesCountText)));

            // Reminds the player that fullscreen exists (only needed on the title screen)
            fullscreenHint.Visible = (globalLevelName == "TitleScreen");
            SetHudPosition(fullscreenHint, new Point(gameWidth - fullscreenHint.Width - 8, 52));

            ApplyHudScale();
        }

        // Fullscreen and interface scaling:

        // Switches between windowed and fullscreen (F11 / Alt+Enter, Escape leaves fullscreen)
        private void ToggleFullscreen()
        {
            // Keeps an open hint window, it is rebuilt straight away at the new size
            bool hintWasOpen = isHintActive;
            string openTitle = (hintTitle == null ? "" : hintTitle.Text);
            string openText = (hintText == null ? "" : hintText.Text);

            fullscreen = !fullscreen;
            DestroyHintPanel();
            ApplyDisplayMode();
            MenuConfig();

            if (hintWasOpen)
            {
                ShowHint(openTitle, openText);
            }
            Refresh();
        }

        // Fits the window to the play field, or to the whole screen in fullscreen
        private void ApplyDisplayMode()
        {
            if (fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                // Remembers the window frame so the play field keeps the same size when leaving fullscreen
                if (FormBorderStyle != FormBorderStyle.None)
                {
                    windowBorderWidth = Math.Max(0, Size.Width - ClientSize.Width);
                    windowBorderHeight = Math.Max(0, Size.Height - ClientSize.Height);
                }
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                Size = new Size(gameWidth + windowBorderWidth, ((level == null || level.Count == 0) ? ClientSize.Height : ((level.Count + 1) * scaleSize)));
            }
        }

        // How much the play field has to be scaled to fit the window
        private float GetRenderScale()
        {
            if (!fullscreen || level == null || level.Count == 0 || gameWidth < 1)
            {
                return 1f;
            }
            int fieldHeight = GetFieldHeight();
            if (fieldHeight < 1)
            {
                return 1f;
            }
            float scaleX = (float)ClientSize.Width / gameWidth;
            float scaleY = (float)ClientSize.Height / fieldHeight;
            return Math.Min(scaleX, scaleY);
        }

        // Height of the play field in play field pixels
        private int GetFieldHeight()
        {
            if (level == null || level.Count == 0)
            {
                return ClientSize.Height;
            }
            int height = ((level.Count + 1) * scaleSize) - windowBorderHeight;
            if (height < 1)
            {
                height = 1;
            }
            return height;
        }

        // Area of the window the play field is drawn into (the rest stays black)
        private Rectangle GetFieldViewport()
        {
            float scale = GetRenderScale();
            if (scale == 1f)
            {
                return ClientRectangle;
            }
            float width = gameWidth * scale;
            float height = GetFieldHeight() * scale;
            return new Rectangle((int)((ClientSize.Width - width) / 2f), (int)((ClientSize.Height - height) / 2f), (int)width, (int)height);
        }

        // Stores the current interface layout as the unscaled (play field) layout
        private void CaptureHudLayout()
        {
            hudControls = new Control[] { ScoreLabel, scoreDisplay, TimeLabel, TimeCounter, WorldTitle, LevelName, CoinCount, MarioLife, LivesCountText, Menu1, Menu2, MenuTop, MenuSelector, fullscreenHint, posXDebug, posYDebug, offsetDebug, GoombaSpeed };
            hudBaseLocation = new Point[hudControls.Length];
            hudBaseSize = new Size[hudControls.Length];
            for (int i = 0; i < hudControls.Length; i++)
            {
                hudBaseLocation[i] = hudControls[i].Location;
                hudBaseSize[i] = hudControls[i].Size;
            }
        }

        // Puts the interface back to the unscaled layout before it is positioned again
        private void RestoreHudLayout()
        {
            for (int i = 0; i < hudControls.Length; i++)
            {
                hudControls[i].Anchor = AnchorStyles.Top | AnchorStyles.Left;
                hudControls[i].Font = customFont;
                hudControls[i].Size = hudBaseSize[i];
            }
        }

        // Moves an interface element inside the play field
        private void SetHudPosition(Control target, Point newLocation)
        {
            int i = HudIndexOf(target);
            if (i < 0)
            {
                target.Location = newLocation;
                return;
            }
            hudBaseLocation[i] = newLocation;
        }

        private int GetHudY(Control target)
        {
            int i = HudIndexOf(target);
            if (i < 0)
            {
                return target.Location.Y;
            }
            return hudBaseLocation[i].Y;
        }

        // Centres an interface element in the play field
        private void CenterHud(Control target)
        {
            int i = HudIndexOf(target);
            if (i < 0)
            {
                target.Location = new Point((gameWidth / 2) - (target.Width / 2), target.Location.Y);
                return;
            }
            hudBaseLocation[i] = new Point((gameWidth / 2) - (hudBaseSize[i].Width / 2), hudBaseLocation[i].Y);
        }

        private int HudIndexOf(Control target)
        {
            if (hudControls == null)
            {
                return -1;
            }
            for (int i = 0; i < hudControls.Length; i++)
            {
                if (hudControls[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }

        // Applies the fullscreen scale to the interface (the labels are normal controls,
        // so they are not affected by the scaling done while painting)
        private void ApplyHudScale()
        {
            if (hudControls == null || customFont == null)
            {
                return;
            }
            float scale = GetRenderScale();
            Rectangle view = GetFieldViewport();

            Font scaledFont = new Font(customFont.FontFamily, customFont.Size * scale, customFont.Style);
            Font previousFont = hudFont;
            hudFont = scaledFont;

            for (int i = 0; i < hudControls.Length; i++)
            {
                hudControls[i].Font = scaledFont;
                hudControls[i].Size = new Size(Math.Max(1, (int)(hudBaseSize[i].Width * scale)), Math.Max(1, (int)(hudBaseSize[i].Height * scale)));
                hudControls[i].Location = new Point(view.X + (int)(hudBaseLocation[i].X * scale), view.Y + (int)(hudBaseLocation[i].Y * scale));
            }

            if (previousFont != null)
            {
                previousFont.Dispose();
            }
        }

        private void IsKeyDown(object sender, KeyEventArgs e)
        {
            // Fullscreen works everywhere, even while a hint window is open
            if (e.KeyCode == Keys.F11 || (e.Alt && e.KeyCode == Keys.Enter))
            {
                // Only switches once per press, holding the key repeats the message
                if (!fullscreenKeyHeld)
                {
                    fullscreenKeyHeld = true;
                    ToggleFullscreen();
                }
                return;
            }
            if (fullscreen && e.KeyCode == Keys.Escape)
            {
                if (!fullscreenKeyHeld)
                {
                    fullscreenKeyHeld = true;
                    ToggleFullscreen();
                }
                return;
            }

            if (isHintActive)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    hintPanel.Visible = false;
                    isHintActive = false;
                    tmrGame.Start();
                }
                return; 
            }

            if (globalLevelName == "TitleScreen")
            {
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    menuSelect = true;
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    menuSelect = false;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    if (menuSelect == false)
                    {
                        playerCount = 1;
                    }
                    else
                    {
                        playerCount = 2;
                    }
                    worldNum = 1;
                    levelNum = 1;
                    Lives = 3;
                    LevelLoad("Load", 0);
                    StartLevel();
                    MenuConfig();
                    timeCounter = 400;
                }
            }
            else if (globalLevelName != "Load")
            {
                if (playerCount < 2)
                {
                    // Inital key press
                    if ((e.KeyCode == Keys.Right || e.KeyCode == Keys.D) && Mario.GetControllable())
                    {
                        movingRight = true;

                        if (!hintMoveBugShown)
                        {
                            hintMoveBugShown = true;
                            ScheduleHint("ОШИБКА УПРАВЛЕНИЯ", "Марио идет не в ту сторону!\n\nФайл: Form1.cs\nМетод: IsKeyDown\n\nИнструкция: Найдите проверку Keys.Right и замените movingLeft на movingRight.");
                        }
                    }
                    if ((e.KeyCode == Keys.Left || e.KeyCode == Keys.A) && Mario.GetControllable())
                    {
                        movingLeft = true; 
                    }

                    if (e.KeyCode == Keys.Space && Mario.GetControllable() && Mario.GetGrounded() == true)
                    {
                        previousJumpHeight = jumpHeight;
                        Mario.SetGrounded(false);

                        if (!hintJumpBugShown)
                        {
                            hintJumpBugShown = true;
                            ScheduleHint("ФИЗИЧЕСКАЯ ОШИБКА", "Марио прыгает как на Луне!\n\nФайл: Form1.cs\nМетод: IsKeyDown\n\nИнструкция: Найдите расчет прыжка и уберите множитель * 3.0f.", 100);
                        }
                    }
                    if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                    {
                        setCrouch = true;
                        if (canPipe && pipeDirection == "Down")
                        {
                            movingStage = true;
                            canPipe = false;
                        }
                    }

                    if (e.KeyCode == Keys.E && Mario.GetControllable())
                    {
                        // If Mario is in the fire flower state then
                        if (Mario.GetState() == 2 && timer > fireBuffer && throwFire == false)
                        {
                            Mario.setSpecial(true);
                            fireBuffer = timer + 10;
                            throwFire = true;
                        }

                        Mario.setRun(true);
                    }
                }
                else
                {
                    // Inital Mario key press
                    if (e.KeyCode == Keys.D && Mario.GetControllable())
                    {
                        movingRight = true;
                    }
                    if ((e.KeyCode == Keys.A) && Mario.GetControllable())
                    {
                        movingLeft = true;
                    }
                    if (e.KeyCode == Keys.Z && Mario.GetControllable() && Mario.GetGrounded() == true)
                    {
                        previousJumpHeight = jumpHeight;
                        Mario.SetGrounded(false);
                    }
                    if (e.KeyCode == Keys.S)
                    {
                        setCrouch = true;
                        if (canPipe && pipeDirection == "Down")
                        {
                            movingStage = true;
                            canPipe = false;
                        }
                    }

                    if (e.KeyCode == Keys.LShiftKey && Mario.GetControllable())
                    {
                        // If Mario is in the fire flower state then
                        if (Mario.GetState() == 2 && timer > fireBuffer && throwFire == false)
                        {
                            Mario.setSpecial(true);
                            fireBuffer = timer + 10;
                            throwFire = true;
                        }

                        Mario.setRun(true);
                    }

                    // Inital Luigi key press
                    if (e.KeyCode == Keys.Right && Luigi.GetControllable())
                    {
                        movingRight2 = true;
                    }
                    if ((e.KeyCode == Keys.Left) && Luigi.GetControllable())
                    {
                        movingLeft2 = true;
                    }
                    if (e.KeyCode == Keys.Space && Luigi.GetControllable() && Luigi.GetGrounded() == true)
                    {
                        previousJumpHeight2 = jumpHeight;
                        Luigi.SetGrounded(false);
                    }
                    if (e.KeyCode == Keys.Down)
                    {
                        setCrouch2 = true;
                        if (canPipe && pipeDirection == "Down")
                        {
                            movingStage = true;
                            canPipe = false;
                        }
                    }

                    if (e.KeyCode == Keys.RShiftKey && Luigi.GetControllable())
                    {
                        // If Luigi is in the fire flower state then
                        if (Luigi.GetState() == 2 && timer > fireBuffer && throwFire == false)
                        {
                            Luigi.setSpecial(true);
                            fireBuffer2 = timer + 10;
                            throwFire2 = true;
                        }

                        Luigi.setRun(true);
                    }
                }
            }
        }

        private void IsKeyUp(object sender, KeyEventArgs e)
        {
            // Allows the fullscreen key to be pressed again
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.Escape)
            {
                fullscreenKeyHeld = false;
            }

            if (playerCount < 2)
            {
                // When key is released
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    movingRight = false;

                }
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    movingLeft = false;

                    if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                    {
                        setCrouch = false;
                    }
                    if (e.KeyCode == Keys.E)
                    {
                        Mario.setRun(false);
                    }
                }
                else
                {
                    // When Mario key is released
                    if (e.KeyCode == Keys.D)
                    {
                        movingRight = false;
                    }
                    if (e.KeyCode == Keys.A)
                    {
                        movingLeft = false;
                    }
                    if (e.KeyCode == Keys.S)
                    {
                        setCrouch = false;
                    }
                    if (e.KeyCode == Keys.LShiftKey)
                    {
                        Mario.setRun(false);
                    }

                    // When Mario key is released
                    if (e.KeyCode == Keys.Right)
                    {
                        movingRight2 = false;
                    }
                    if (e.KeyCode == Keys.Left)
                    {
                        movingLeft2 = false;
                    }
                    if (e.KeyCode == Keys.Down)
                    {
                        setCrouch2 = false;
                    }
                    if (e.KeyCode == Keys.RShiftKey)
                    {
                        Luigi.setRun(false);
                    }
                }
            }
        }

        private void Movement()
        {
            // This is what moves Mario
            if (Mario.GetControllable())
            {
                if (movingRight)
                {
                    moveRight(Mario);
                }
                if (movingLeft)
                {
                    Mario.SetDirection("Left");
                    if (movementLeft)
                    {
                        // Prevents player going too far left
                        if (Mario.GetRecPosition().X >= 5)
                        {
                            if (Mario.getRunning())
                            {
                                Mario.SetFltXVel(-(speed * 1.25f));
                            }
                            else
                            {
                                Mario.SetFltXVel(-(speed));
                            }
                        }
                    }
                    else
                    {
                        movementLeft = true;
                    }
                }
            }
            if (Luigi != null)
            {
                // This is what moves Mario
                if (Luigi.GetControllable())
                {
                    if (movingRight2)
                    {
                        moveRight(Luigi);
                    }
                    if (movingLeft2)
                    {
                        Luigi.SetDirection("Left");
                        if (movementLeft2)
                        {
                            // Prevents player going too far left
                            if (Luigi.GetRecPosition().X >= 5)
                            {
                                if (Luigi.getRunning())
                                {
                                    Luigi.SetFltXVel(-(speed * 1.25f));
                                }
                                else
                                {
                                    Luigi.SetFltXVel(-(speed));
                                }
                            }
                        }
                        else
                        {
                            movementLeft2 = true;
                        }
                    }
                }
                if (Mario.GetRecPosition().X < -scaleSize || Luigi.GetRecPosition().X < -scaleSize)
                {
                    Lives--;
                    livesLost = true;
                }
            }
        }

        private void onTick(object sender, EventArgs e)
        {
            if (globalLevelName == "Load" && Loading == false)
            {
                MarioLife.Text = "World " + worldNum + "-" + levelNum;
                CenterHud(MarioLife);
                ApplyHudScale();
                Loading = true;
                loadTimer = timer + 50;
            }

            if (Loading == true && loadTimer < timer)
            {
                LevelLoad(worldNum + "-" + levelNum, 0);
                Mario.SetControllable(true);
                StartLevel();
                MenuConfig();
                Loading = false;
            }

            if (menuSelect == false)
            {
                MenuSelector.Location = new Point(MenuSelector.Location.X, Menu1.Location.Y);
            }
            else
            {
                MenuSelector.Location = new Point(MenuSelector.Location.X, Menu2.Location.Y);
            }

            timer++;

            if (hintTimerTrigger != -1 && timer >= hintTimerTrigger)
            {
                ShowHint(scheduledHintTitle, scheduledHintText);
                hintTimerTrigger = -1; 
            }

            if (Mario.GetControllable() && globalLevelName != "Load" && globalLevelName != "TitleScreen")
            {
                // Every second...
                timeSubCounter++;

                if (timeSubCounter >= 40)
                {
                    timeCounter--;
                    timeSubCounter= 0;

                    if (!hintTimeBugShown && globalLevelName != "TitleScreen")
                    {
                        hintTimeBugShown = true;
                        ScheduleHint("АНОМАЛИЯ ВРЕМЕНИ", "Таймер уровня работает слишком быстро!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите строку timeCounter -= 3 и замените её на стандартное уменьшение: timeCounter--;");
                    }
                }
                // Reduces lives when the timer hits 0
                if (timeCounter == 0)
                {
                    Lives--;
                    livesLost = true;
                }
            }
            // Sets visual timer
            TimeCounter.Text = timeCounter.ToString("000");

            // Shows score
            if (scoreDisplay != null)
            {
                scoreDisplay.Text = score.ToString("000000");
            }

            // Allows Mario to crouch
            if (setCrouch && Mario.GetState() > 0)
            {
                Mario.setCrouch(true);
            }
            else
            {
                Mario.setCrouch(false);
            }
            if (Luigi != null)
            {
                if (setCrouch2 && Luigi.GetState() > 0)
                {
                    Luigi.setCrouch(true);
                }
                else
                {
                    Luigi.setCrouch(false);
                }
            }

            // Creates fire ball
            if (throwFire == true)
            {
                int directionSet = 0;
                if (Mario.GetDirection() == "Right")
                {
                    directionSet = 1;
                }
                else
                {
                    directionSet = 0;
                }
                entityList.Add(new Entity("Fire", (int)(Mario.GetRecPosition().X + offset + (Mario.GetSizeX() / 2)), (int)(Mario.GetRecPosition().Y + (Mario.GetSizeY() / 2)), scaleSize / 2, scaleSize / 2, false, scaleSize, offset, entityLookUp, directionSet, -1, DebugMode));

                if (!hintFireBugShown)
                {
                    hintFireBugShown = true;
                    ScheduleHint("БАГ ОРУЖИЯ", "Фаерболы летят в обратную сторону!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: В блоке throwFire исправьте логику directionSet (для Right должно быть 1).");
                }
            }
            throwFire = false;

            if (throwFire2 == true)
            {
                int directionSet = 0;
                if (Luigi.GetDirection() == "Right")
                {
                    directionSet = 1;
                }
                entityList.Add(new Entity("Fire", (int)(Luigi.GetRecPosition().X + offset + (Luigi.GetSizeX() / 2)), (int)(Luigi.GetRecPosition().Y + (Luigi.GetSizeY() / 2)), scaleSize / 2, scaleSize / 2, false, scaleSize, offset, entityLookUp, directionSet, -1, DebugMode));
            }
            throwFire2 = false;


            // Checks for intersect
            // Sets inital intersect to false
            marioIntersect = false;

            foreach (Block b in levelBlocks)
            {
                if (b.isSolid() && !b.getSemiSolid())
                {
                    if (b.getRec().IntersectsWith(Mario.GetFallDetectRec()))
                    {
                        collidedBlocks.Add(b);
                        marioIntersect = true;
                    }
                }
            }

            // If it is still false, then nothing is intersecting with the fallDetect Rectangle, so it needs to fall
            if (!marioIntersect)
            {
                Mario.SetGrounded(false);
            }

            if (Luigi != null)
            {
                // Checks for intersect
                // Sets inital intersect to false
                luigiIntersect = false;

                foreach (Block b in levelBlocks)
                {
                    if (b.isSolid() && !b.getSemiSolid())
                    {
                        if (b.getRec().IntersectsWith(Luigi.GetFallDetectRec()))
                        {
                            luigiIntersect = true;
                        }
                    }
                }

                // If it is still false, then nothing is intersecting with the fallDetect Rectangle, so it needs to fall
                if (!luigiIntersect)
                {
                    Luigi.SetGrounded(false);
                }
            }

            // Stores all of the blocks currently being collided with by Mario
            foreach (Block b in levelBlocks)
            {
                if (b.isSolid() && !b.getSemiSolid() && b.getRec().IntersectsWith(Mario.GetRecPosition()))
                {
                    collidedBlocks.Add(b);
                }
            }

            // Checks collided blocks for warp pipes, and if so what direction
            canPipe = false;
            if (canPipe == false)
            {
                foreach (Block b in collidedBlocks)
                {
                    if (b.ID == "51" || b.ID == "42")
                    {
                        if (b.ID == "51")
                        {
                            pipeDirection = "Down";
                        }
                        else if (b.ID == "42")
                        {
                            pipeDirection = "Right";
                        }
                        pipeCheck1 = true;
                        pipeRec = b.getRec();
                    }
                    else if (b.ID == "52" || b.ID == "43")
                    {
                        pipeCheck2 = true;
                        if (b.ID == "52")
                        {
                            pipeDirection = "Down";
                        }
                        else if (b.ID == "43")
                        {
                            pipeDirection = "Right";
                            pipeRec = b.getRec();
                        }
                    }
                }
                // Ensures the user can only use the pipe when in the middle
                if (pipeCheck1 && pipeCheck2 && pipeRec.X + (scaleSize / 1.25f) > Mario.GetRecPosition().X && pipeRec.X < Mario.GetRecPosition().X && pipeDirection == "Down")
                {
                    canPipe = true;
                }
                else if ((pipeCheck1 || pipeCheck2) && pipeRec.Y + scaleSize + 1 > Mario.GetRecPosition().Y && pipeDirection == "Right" && Mario.GetGrounded())
                {
                    movingStage = true;
                }
                pipeCheck1 = false;
                pipeCheck2 = false;
                collidedBlocks.Clear();
            }

            // Same concept, but for all other entities, if they are not static
            foreach (Entity entity in entityList)
            {
                if (!entity.getStatic() && !entity.checkJump())
                {
                    entity.SetIntersect(false);
                    foreach (Block b in levelBlocks)
                    {
                        if (b.isSolid() && b.getRec().IntersectsWith(entity.GetFallDetectRec()))
                        {
                            entity.SetIntersect(true);
                        }
                    }
                    if (!entity.GetIntersect())
                    {
                        entity.setGrounded(false);
                    }
                    else
                    {
                        entity.setGrounded(true);
                    }
                }

                if (!entity.isActive())
                {
                    toRemoveEntity.Add(entity);
                }
            }
            entityList.RemoveAll(toRemoveEntity.Contains);
            toRemoveEntity.Clear();

            // Handles fall speed of Mario
            if (!Mario.GetGrounded() && !Mario.GetPoleState())
            {
                if (previousJumpHeight > 0)
                {
                    // If jump height is positive then Mario is moving up
                    movingUp = true;
                    movingDown = false;
                    previousJumpHeight -= 9.81 * 0.02f * 2;
                }
                else
                {
                    // If jump height is negative then Mario is moving down
                    movingDown = true;
                    movingUp = false;
                    // Fall speed is double speed
                    previousJumpHeight -= 9.81 * 0.02f * 4;
                }
                Mario.SetFltYVel(previousJumpHeight);
            }
            else
            {
                Mario.SetFltYVel(0);
            }

            // If Mario has gotten a life then add to the life counter
            if (Mario.getLife())
            {
                Lives++;
                Mario.setLife(false);

                if (!hintLifeBugShown)
                {
                    hintLifeBugShown = true;
                    ScheduleHint("ЛОГИЧЕСКАЯ ОШИБКА", "Зеленый гриб убивает Марио!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите блок Mario.getLife() и исправьте Lives-- на Lives++;");
                }
            }

            if (Luigi != null)
            {
                // Handles fall speed of Luigi
                if (!Luigi.GetGrounded() && !Luigi.GetPoleState())
                {
                    if (previousJumpHeight2 > 0)
                    {
                        // If jump height is positive then Luigi is moving up
                        movingUp2 = true;
                        movingDown2 = false;
                        previousJumpHeight2 -= 9.81 * 0.02f * 2;
                    }
                    else
                    {
                        // If jump height is negativ then Luigi is moving down
                        movingDown2 = true;
                        movingUp2 = false;
                        // Fall speed is double speed
                        previousJumpHeight2 -= 9.81 * 0.02f * 4;
                    }
                    Luigi.SetFltYVel(previousJumpHeight2);
                }
                else
                {
                    Luigi.SetFltYVel(0);
                }

                // If Luigi has gotten a life then add to the life counter
                if (Luigi.getLife())
                {
                    Lives++;
                    Luigi.setLife(false);
                }
            }

            // Handles transition between stages through pipes
            if (movingStage)
            {
                previousJumpHeight = 0;
                Mario.SetControllable(false);

                if (pipeDirection == "Down")
                {
                    Mario.SetFltYVel(-3);
                    if (Mario.GetRecPosition().Y >= pipeRec.Y + scaleSize)
                    {
                        if (worldNum == 1 && levelNum == 2)
                        {
                            EndGameSuccess();
                        }
                        else
                        {
                            UndergroundLoad();
                            Mario.SetControllable(true);
                            movingStage = false;
                            pipeDirection = "";
                        }
                    }
                }
                else if (pipeDirection == "Right")
                {
                    Mario.SetFltXVel(3f);
                    Mario.SetGrounded(true);

                    if (Mario.GetRecPosition().X >= pipeRec.X + scaleSize)
                    {
                        if (worldNum == 1 && levelNum == 2)
                        {
                            EndGameSuccess();
                        }
                        else
                        {
                            UndergroundLoad();
                            Mario.SetControllable(true);
                            movingStage = false;
                            pipeDirection = "";
                        }
                    }
                }
            }

            // Handles X speed of Mario
            Movement();

            // Notes whether Mario is moving left or right
            if (movingLeft || movingRight)
            {
                Mario.setMoving(true);
            }
            else
            {
                Mario.setMoving(false);
            }

            if (Luigi != null)
            {
                // Notes whether Luigi is moving left or right
                if (movingLeft2 || movingRight2)
                {
                    Luigi.setMoving(true);
                }
                else
                {
                    Luigi.setMoving(false);
                }
            }

            if (Mario.GetPoleState())
            {
                Mario.SetFltYVel(0);
            }

            // Moves Mario
            Mario.ChangePosition((int)(Mario.GetRecPosition().X + Mario.GetFltXVel()), (int)(Mario.GetRecPosition().Y - Mario.GetFltYVel()));
            Mario.SetFltXVel(0);
            Mario.SetFltYVel(0);

            if (Luigi != null)
            {
                Luigi.ChangePosition((int)(Luigi.GetRecPosition().X + Luigi.GetFltXVel()), (int)(Luigi.GetRecPosition().Y - Luigi.GetFltYVel()));
                Luigi.SetFltXVel(0);
                Luigi.SetFltYVel(0);
            }

            // Death barrier underneath the map, so if Mario falls off the stage he dies
            if (Mario.GetRecPosition().Y > 2000)
            {
                Lives--;
                livesLost = true;

                if (!hintDeathCeilingBugShown)
                {
                    hintDeathCeilingBugShown = true;
                    ScheduleHint("ОШИБКА ГРАНИЦ", "Марио умирает от удара о небо!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите комментарий 'Death barrier underneath the map'. Неправильно заданы координаты ямы. Измените 'Y < 0' обратно на 'Y > 2000'.", 30);
                }
            }

            if (Luigi != null)
            {
                if (Luigi.GetRecPosition().Y > 2000) 
                {
                    Lives--;
                    livesLost = true;
                }
            }

            // Handles all of Entity movement and collision
            foreach (Entity entity in entityList)
            {
                if (entity.isActive() && entity.GetRecPosition().X > (backX - 20) - entityOffset && entity.GetRecPosition().X < (frontX + 20) - entityOffset)
                {

                    // Falling distance speed
                    if (!entity.getGrounded())
                    {
                        entity.setFallHeight(entity.getFallHeight() - (9.81f * 0.02f * 4));
                    }
                    else
                    {
                        entity.setFallHeight(0);
                        entity.SetFltYVel(0);
                    }
                    entity.SetFltYVel(entity.getFallHeight());
                    // Moves Entity
                    if (entity.GetFltXVel() < 0)
                    {
                        entity.ChangePosition((int)(entity.GetRecPosition().X + Math.Floor(entity.GetFltXVel())), (int)(entity.GetRecPosition().Y - entity.GetFltYVel()));
                    }
                    else
                    {
                        entity.ChangePosition((int)(entity.GetRecPosition().X + Math.Round(entity.GetFltXVel())), (int)(entity.GetRecPosition().Y - entity.GetFltYVel()));
                    }

                    if (entity.checkJump() && !entity.getStatic())
                    {
                        entity.SetIntersect(false);
                        foreach (Block b in levelBlocks)
                        {
                            if (b.isSolid())
                            {
                                if (b.getRec().IntersectsWith(entity.GetFallDetectRec()))
                                {
                                    entity.SetIntersect(true);
                                }
                            }
                        }
                        if (!entity.GetIntersect() || !entity.getCollide())
                        {
                            entity.setGrounded(false);
                        }
                        else
                        {
                            entity.setGrounded(true);
                        }
                    }

                    // Checks collision of Entity, if it is solid
                    foreach (Block b in levelBlocks)
                    {
                        if (b.isSolid() && entity.getCollide() && b.getRec().IntersectsWith(entity.GetRecPosition()))
                        {
                            if (!entity.getGrounded())
                            {
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionDown(entity, b);
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionRight(entity, b);
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionLeft(entity, b);
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionUp(entity, b);
                            }
                            if (entity.getGrounded() && entity.GetFltXVel() >= 0)
                            {
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionRight(entity, b);
                            }
                            if (entity.getGrounded() && entity.GetFltXVel() <= 0)
                            {
                                if (b.getRec().IntersectsWith(entity.GetRecPosition()))
                                    checkEntityCollisionLeft(entity, b);
                            }
                        }
                    }

                    // If the entity is destoryed it says it needs to add its score
                    if (entity.getAddScore() == true)
                    {
                        score += entity.getScore();
                        entity.setAddScore(false);

                        if (!hintScoreBugShown)
                        {
                            hintScoreBugShown = true;
                            ScheduleHint("БАГ СЧЕТЧИКА", "За убийство врагов у вас отнимают очки!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите строку score -= entity.getScore() и замените минус на плюс.", 50);
                        }

                        // If it is a coin it needs to add to the coin count as well
                        if (entity.getCoin())
                        {
                            coinCounter++;
                            entity.setCoin(false);

                            if (!hintCoinBug2Shown)
                            {
                                hintCoinBug2Shown = true;
                                ScheduleHint("ОШИБКА ЭКОНОМИКИ", "Выбивание монеты из блока дает 5 монет!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите переменную coinCounter и верните ей правильное прибавление (+1).");
                            }
                        }
                    }

                    // Lets the shell and fireball destory other entities
                    if (entity.ID == "11" || ((entity.ID == "18") && (entity.GetFltXVel() != 0)))
                    {
                        foreach (Entity en in entityList)
                        {
                            if (entity.GetRecPosition().IntersectsWith(en.GetRecPosition()) && entity != en && en.CheckEnemy())
                            {
                                en.setAddScore(true); 
                                en.Delete(); 
                                entity.Delete(); 

                                if (!hintPacifistFireballBugShown)
                                {
                                    hintPacifistFireballBugShown = true;
                                    ScheduleHint("ОГНЕННЫЕ ШАРЫ-ПАЦИФИСТЫ", "Огненный шар исчезает, но враг остается жив!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: Найдите место, где снаряд пересекается с врагом (en.CheckEnemy()). Раскомментируйте вызовы 'en.Delete();' и 'en.setAddScore(true);', чтобы враг уничтожался.", 10);
                                }
                            }
                        }
                    }
                    EntityCheck(entity, Mario);
                    if (Luigi != null)
                    {
                        EntityCheck(entity, Luigi);
                    }
                }
            }

            // Adds all entities to the entity list from toAddEntity, as entities can't be added when the list is being read
            foreach (Entity entity in toAddEntity)
            {
                entityList.Add(entity);
            }
            toAddEntity.Clear();

            // Resets the current level, or 
            if (livesLost == true)
            {
                if (Lives != 0 && timer > invTimer)
                {
                    LevelLoad("Load", 0);
                    StartLevel();
                    MenuConfig();
                    Luigi = null;
                    previousJumpHeight = 0;
                    FirstLoad = false;
                    timeCounter = 400;
                    Mario.SetState(0);
                }
                else if ((timer > invTimer))
                {
                    WriteFile("TopScore.txt", score.ToString());
                    score = 0;
                    LevelLoad("TitleScreen", 0);
                    StartLevel();
                    Luigi = null;
                }
            }
            livesLost = false;

            // Begins ending sequence
            if (timer == endTime)
            {
                ending = true;
            }

            // Progresses level to next level
            if (timer == levelTimer)
            {
                if (levelNum >= 4)
                {
                    levelNum = 1;
                    worldNum += 1;
                }
                else
                {
                    levelNum++;
                }
                currentState = Mario.GetState();
                if (Luigi != null)
                {
                    currentState2 = Luigi.GetState();
                }
                LevelLoad(worldNum + "-" + levelNum, 0);
                timeCounter = 400;
                StartLevel();
                MenuConfig();
                previousJumpHeight = 0;
                FirstLoad = false;
            }

            // After hitting the castle door
            if (ending)
            {
                Mario.SetPoleState(false);
                movementRight = true;
                foreach (Block b in levelBlocks)
                {
                    if (b.GetName() == "Castle5")
                    {
                        if (Mario.GetRecPosition().X < b.getRec().X)
                        {
                            movingRight = true;
                            moveRight(Mario);
                        }
                        else if (Mario.GetRecPosition().X >= b.getRec().X)
                        {
                            Mario.setRecBox(new Rectangle(Mario.GetRecPosition().X, Mario.GetRecPosition().Y, 0, 0));
                            if (levelTimer == -1)
                            {
                                levelTimer = timer + 50;
                            }
                        }
                    }
                }
            }

            foreach (Block b in levelBlocks)
            {
                // Begins the ending cutscene
                if (b.getRec().IntersectsWith(Mario.GetHitBox()) && b.checkIsGoal() && ending == false)
                {
                    if (Mario.GetState() > 2)
                    {
                        Mario.stopStar();
                    }
                    if (Flag.GetRecPosition().Y <= Mario.GetRecPosition().Y)
                    {
                        Flag.ChangePositionOverride((int)(Flag.GetRecPosition().X), (int)(Flag.GetRecPosition().Y + flagSpeed));
                    }
                    if (!Mario.GetPoleState())
                    {
                        Mario.SetPoleState(true);
                        movingUp = false;
                        movingDown = true;
                        Mario.ChangePosition((int)(Mario.GetRecPosition().X + (scaleSize / 2.5f)), (int)(Mario.GetRecPosition().Y));
                        Mario.SetControllable(false);
                    }
                    if (!Mario.GetGrounded())
                    {
                        Mario.ChangePosition((int)(Mario.GetRecPosition().X), (int)(Mario.GetRecPosition().Y + flagSpeed));
                    }
                    else
                    {
                        if (Flag.GetRecPosition().Y > Mario.GetRecPosition().Y && Mario.GetDirection() == "Right")
                        {
                            Mario.ChangePosition((int)(Mario.GetRecPosition().X + (scaleSize * (0.6f))));
                            Mario.SetDirection("Left");
                            
                            endTime = timer + 20; 

                            if (!hintFlagBugShown)
                            {
                                hintFlagBugShown = true;
                                ScheduleHint("БЕСКОНЕЧНЫЙ ФЛАГШТОК", "Уровень не заканчивается после спуска по флагу!\n\nФайл: Form1.cs\nМетод: onTick (обработка катсцены флага)\n\nИнструкция: Вы забыли запустить таймер окончания уровня. Найдите закомментированную строку и верните 'endTime = timer + 20;'.", 40);
                            }
                        }
                    }
                }

                // Handling hitting the bottom of the block
                if ((b.isSolid() || b.getSemiSolid()) && Mario.GetHitBox().Top < b.getRec().Bottom && Mario.GetHitBox().Top > b.getRec().Bottom - (b.getSize() * intersectStrictness) && previousJumpHeight > 0 && b.getRec().IntersectsWith(Mario.GetHitBox()))
                {
                    Mario.ChangePosition(Mario.GetRecPosition().X, Mario.GetRecPosition().Y - (Mario.GetRecPosition().Y - (b.getRec().Bottom)));
                    previousJumpHeight = 0;
                    if (b.getIsContainer())
                    {
                        if (!b.getHit())
                        {
                            b.setSemiSolid(false);
                            
                            b.Bounce();
                            // Spawns Entity specified
                            entityList.Add(b.spawnItem(b.getContents(), entityLookUp, offset, Mario.GetState()));
                            b.setHit(true);

                            if (!hintInfiniteBlockBugShown)
                            {
                                hintInfiniteBlockBugShown = true;
                                ScheduleHint("ОШИБКА СОСТОЯНИЯ", "Блок выдает бесконечные предметы!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: В блоке столкновения головой с блоком вы забыли изменить состояние блока после удара. Добавьте 'b.setHit(true);', чтобы блок опустел.", 40);
                            }
                        }
                    }
                    if (b.getBreakable())
                    {
                        if (Mario.GetState() > 0)
                        {
                            b.delete(entityLookUp, offset);
                            // Creates brick chunks
                            entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) - (scaleSize * 0.25f)), (int)((b.getY()) - (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 0, 0, DebugMode));
                            entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) + (scaleSize * 0.25f)), (int)((b.getY()) + (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 1, 1, DebugMode));
                            entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) - (scaleSize * 0.25f)), (int)((b.getY()) + (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 0, 1, DebugMode));
                            entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) + (scaleSize * 0.25f)), (int)((b.getY()) - (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 1, 0, DebugMode));
                            if (b.getScore())
                            {
                                score += 50;
                                b.setScore(false);
                            }
                        }
                        else
                        {
                            b.Bounce();
                        }
                    }
                }

                if (Luigi != null)
                {
                    // Handling hitting the bottom of the block
                    if ((b.isSolid() || b.getSemiSolid()) && Luigi.GetHitBox().Top < b.getRec().Bottom && Luigi.GetHitBox().Top > b.getRec().Bottom - (b.getSize() * intersectStrictness) && previousJumpHeight2 > 0 && b.getRec().IntersectsWith(Luigi.GetHitBox()))
                    {
                        Luigi.ChangePosition(Luigi.GetRecPosition().X, Luigi.GetRecPosition().Y - (Luigi.GetRecPosition().Y - (b.getRec().Bottom)));
                        previousJumpHeight2 = 0;
                        if (b.getIsContainer())
                        {
                            if (!b.getHit())
                            {
                                b.setSemiSolid(false);
                                
                                b.Bounce();
                                // Spawns Entity specified
                                entityList.Add(b.spawnItem(b.getContents(), entityLookUp, offset, Luigi.GetState()));
                                b.setHit(true);

                                if (!hintInfiniteBlockBugShown)
                                {
                                    hintInfiniteBlockBugShown = true;
                                    ScheduleHint("ОШИБКА СОСТОЯНИЯ", "Блок выдает бесконечные предметы!\n\nФайл: Form1.cs\nМетод: onTick\n\nИнструкция: В блоке столкновения головой с блоком вы забыли изменить состояние блока после удара. Добавьте 'b.setHit(true);', чтобы блок опустел.", 40);
                                }
                            }
                        }
                        if (b.getBreakable())
                        {
                            if (Luigi.GetState() > 0)
                            {
                                b.delete(entityLookUp, offset);
                                // Creates brick chunks
                                entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) - (scaleSize * 0.25f)), (int)((b.getY()) - (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 0, 0, DebugMode));
                                entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) + (scaleSize * 0.25f)), (int)((b.getY()) + (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 1, 1, DebugMode));
                                entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) - (scaleSize * 0.25f)), (int)((b.getY()) + (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 0, 1, DebugMode));
                                entityList.Add(new Entity(b.GetName() + "Break", (int)((b.getX()) + (scaleSize * 0.25f)), (int)((b.getY()) - (scaleSize * 0.25f)), scaleSize, scaleSize, false, scaleSize, offset, entityLookUp, 1, 0, DebugMode));
                                if (b.getScore())
                                {
                                    score += 50;
                                    b.setScore(false);
                                }
                            }
                            else
                            {
                                b.Bounce();
                            }
                        }
                    }
                }
            }

            // MAIN COLLISION HANDLING
            foreach (Block b in levelBlocks)
            {
                if (b.isSolid() && !b.getSemiSolid() && b.getRec().IntersectsWith(Mario.GetRecPosition()) && !movingStage)
                {
                    collidedBlocks.Add(b);
                    // Doesn't need to check these if Mario is on the floor
                    if (!Mario.GetGrounded())
                    {
                        // Order of collision handling
                        CheckCollisionsDown(b, Mario);
                        if (movingRight && b.getRec().IntersectsWith(Mario.GetRecPosition()))
                        {
                            CheckCollisionsRight(b, Mario);
                            movingRight = true;

                            if (!hintGhostWallBugShown)
                            {
                                hintGhostWallBugShown = true;
                                ScheduleHint("ПРИЗРАЧНЫЕ СТЕНЫ", "Марио проходит сквозь стены по горизонтали!\n\nФайл: Form1.cs\nМетод: onTick (раздел MAIN COLLISION HANDLING)\n\nИнструкция: Вы закомментировали вызовы CheckCollisionsRight и CheckCollisionsLeft. Раскомментируйте их, чтобы физика выталкивала Марио из стен.", 20);
                            }
                        }
                        if (movingLeft && b.getRec().IntersectsWith(Mario.GetRecPosition()))
                        {
                            CheckCollisionsLeft(b, Mario);
                            movingLeft = true;
                        }
                        if (b.getRec().IntersectsWith(Mario.GetRecPosition()))
                            CheckCollisionsDown(b, Mario);
                        if (b.getRec().IntersectsWith(Mario.GetRecPosition()))
                            CheckCollisionsUp(b, Mario);
                        if (movingLeft && b.getRec().IntersectsWith(Mario.GetRecPosition()))
                        {
                            CheckCollisionsLeft(b, Mario);
                            movingLeft = true;
                        }
                    }
                    // When Mario is on the floor then check...
                    if (movingRight && Mario.GetGrounded())
                    {
                        if (b.getRec().IntersectsWith(Mario.GetRecPosition()))
                        {
                            CheckCollisionsRight(b, Mario);
                            movingRight = true;

                            if (!hintGhostWallBugShown)
                            {
                                hintGhostWallBugShown = true;
                                ScheduleHint("ПРИЗРАЧНЫЕ СТЕНЫ", "Марио проходит сквозь стены по горизонтали!\n\nФайл: Form1.cs\nМетод: onTick (раздел MAIN COLLISION HANDLING)\n\nИнструкция: Вы закомментировали вызовы CheckCollisionsRight и CheckCollisionsLeft. Раскомментируйте их, чтобы физика выталкивала Марио из стен.", 20);
                            }
                        }
                    }
                    else if (movingLeft && Mario.GetGrounded())
                    {
                        if (b.getRec().IntersectsWith(Mario.GetRecPosition()))
                        {
                            CheckCollisionsLeft(b, Mario);
                            movingLeft = true;
                        }
                    }
                }
                if (Luigi != null)
                {
                    if (b.isSolid() && !b.getSemiSolid() && b.getRec().IntersectsWith(Luigi.GetRecPosition()) && !movingStage)
                    {
                        collidedBlocks.Add(b);
                        // Doesn't need to check these if Luigi is on the floor
                        if (!Luigi.GetGrounded())
                        {
                            // Order of collision handling
                            CheckCollisionsDown(b, Luigi);
                            if (movingRight2 && b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                            {
                                CheckCollisionsRight(b, Luigi);
                                movingRight2 = true;
                            }
                            if (movingLeft2 && b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                            {
                                CheckCollisionsLeft(b, Luigi);
                                movingLeft2 = true;
                            }
                            if (b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                                CheckCollisionsDown(b, Luigi);
                            if (b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                                CheckCollisionsUp(b, Luigi);
                            if (movingLeft2 && b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                            {
                                CheckCollisionsLeft(b, Luigi);
                                movingLeft2 = true;
                            }
                        }
                        // When Luigi is on the floor then check...
                        if (movingRight2 && Luigi.GetGrounded())
                        {
                            if (b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                            {
                                CheckCollisionsRight(b, Luigi);
                                movingRight2 = true;
                            }
                        }
                        else if (movingLeft2 && Luigi.GetGrounded())
                        {
                            if (b.getRec().IntersectsWith(Luigi.GetRecPosition()))
                            {
                                CheckCollisionsLeft(b, Luigi);
                                movingLeft2 = true;
                            }
                        }
                    }
                }
            }

            // When mario gets 100 coins give him a life
            if (coinCounter > 99)
            {
                coinCounter = coinCounter - 100;
                Lives++;
            }
            CoinCount.Text = "x" + coinCounter.ToString("00");

            // Handles animation looping
            foreach (Entity entity in entityList)
            {
                if (entity.getAnimated())
                {
                    if (entity.getTotalFrames() - 1 == entity.getCurrentFrame())
                    {
                        if (entity.isOneLoop())
                        {
                            entity.setCurrentFrame(-1);
                        }
                        else
                        {
                            entity.setCurrentFrame(0);
                        }
                    }
                    else
                    {
                        entity.setCurrentFrame(entity.getCurrentFrame() + 1);
                    }
                }
            }

            // Paints all updated blocks, entites, and Mario
            this.Refresh();

            // Updates debug text
            if (DebugMode)
            {
                posXDebug.Text = "PosX: " + (Mario.GetRecPosition().X).ToString();
                posYDebug.Text = "PosY: " + (Mario.GetRecPosition().Y).ToString();
                offsetDebug.Text = "Offset: " + offset.ToString();
            }
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            // In fullscreen the play field is scaled up and centred, the rest of the screen stays black
            float scale = GetRenderScale();
            if (scale != 1f)
            {
                Rectangle view = GetFieldViewport();
                e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);
                e.Graphics.SetClip(view);
                e.Graphics.TranslateTransform(view.X, view.Y);
                e.Graphics.ScaleTransform(scale, scale);
            }

            // If it is the first load then draw all the blocks (Boosts performance)
            if (!FirstLoad)
            {
                FirstLoad = true;
                for (int i = 0; i < level.Count; i++)
                {
                    for (int j = 0; j < level[i].Length; j++)
                    {
                        if (level[i][j] != "53")
                        {
                            levelBlocks.Add(new Block(level[i][j], lookUp, j, i, offset, scaleSize, e, resourceManager, DebugMode));
                        }
                        else
                        {
                            spawnBlock = new Block(level[i][j], lookUp, j, i, offset, scaleSize, e, resourceManager, DebugMode);
                            levelBlocks.Add(spawnBlock);
                        }
                    }
                }
            }

            // This is how I deal with the lack of layers, I pick and choose which order it is drawn in, so it moves from the back to the front.

            // Updates background blocks and invisible blocks
            foreach (Block b in levelBlocks)
            {
                if (!b.isSolid() || b.getSemiSolid())
                {
                    if (b.getX() < frontX && b.getX() > backX)
                    {
                        b.Update(e, offset);
                        if (b.isSpawner())
                        {
                            entityList.Add(b.spawnControl(entityLookUp));
                        }
                    }
                }
            }

            // Update entities
            foreach (Entity entity in entityList)
            {
                if (entity.isActive())
                {
                    entity.Update(e, offset);
                }
            }

            // Updates Mario
            Mario.Update(e);

            if (Luigi != null)
            {
                Luigi.Update(e);
            }

            // Updates foreground blocks
            foreach (Block b in levelBlocks)
            {
                if (b.isSolid() && !b.getSemiSolid())
                {
                    if (b.getX() < frontX && b.getX() > backX)
                    {
                        b.Update(e, offset);
                        if (b.isSpawner())
                        {
                            entityList.Add(b.spawnControl(entityLookUp));
                        }
                    }
                }
            }
        }

        private void EntityCheck(Entity entity, Player player)
        {
            // player interaction
            if (entity.GetRecPosition().IntersectsWith(player.GetRecPosition()))
            {
                // Only if it is a powerup
                if (entity.CheckPowerUp())
                {
                    entity.Delete();
                    score += entity.getScore();
                    if (entity.GetPowerUpState() > player.GetState())
                    {
                        if (player.GetState() == 0 && entity.GetPowerUpState() < 3)
                        {
                            player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - scaleSize);
                        }
                        if (entity.GetPowerUpState() == 3 && player.GetState() > 0)
                        {
                            player.SetState(4);
                        }
                        else
                        {
                            player.SetState(entity.GetPowerUpState());
                        }
                        foreach (Block b in levelBlocks)
                        {
                            if (b.isSolid())
                            {
                                if (b.getRec().IntersectsWith(player.GetRecPosition()) && player.GetGrounded())
                                {
                                    player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - (player.GetSizeY() / 2));
                                }
                            }
                        }
                    }
                    else if (player.GetState() == 3 || player.GetState() == 4)
                    {
                        if (player.GetState() == 4)
                        {
                            if (entity.GetPowerUpState() == 2)
                            {
                                player.setMemoryState(entity.GetPowerUpState());
                            }
                        }
                        if (player.GetState() == 3)
                        {
                            if (entity.GetPowerUpState() > 0 && entity.GetPowerUpState() < 3)
                            {
                                player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - scaleSize);
                                player.SetState(4);
                                player.setMemoryState(entity.GetPowerUpState());
                            }
                        }
                        foreach (Block b in levelBlocks)
                        {
                            if (b.isSolid())
                            {
                                if (b.getRec().IntersectsWith(player.GetRecPosition()) && player.GetGrounded())
                                {
                                    player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - (player.GetSizeY() / 2));
                                }
                            }
                        }
                    }
                }
                // Only if it is an enemy
                if (entity.CheckEnemy())
                {
                    bool tempMovingDown;
                    if (player == Mario)
                    {
                        tempMovingDown = movingDown;
                    }
                    else
                    {
                        tempMovingDown = movingDown2;
                    }
                    if (tempMovingDown) 
                    {
                        entity.Squish(ref toAddEntity, entityLookUp, offset, player, speed, ref invTimer, timer);
                        if (player == Mario)
                        {
                            previousJumpHeight = jumpHeight / 1.5f;
                        }
                        else
                        {
                            previousJumpHeight2 = jumpHeight / 1.5f;
                        }
                        player.SetGrounded(false);
                    }
                    else if (!(player.GetState() == 3 || player.GetState() == 4))
                    {
                        if (!(entity.ID == "18" && entity.GetFltXVel() == 0) && timer > invTimer)
                        {
                            if (tempMovingDown && !hintCombatBugShown)
                            {
                                hintCombatBugShown = true;
                                ScheduleHint("ОШИБКА БОЕВКИ", "Гумбы стали пуленепробиваемыми сверху!\n\nФайл: Form1.cs\nМетод: EntityCheck\n\nИнструкция: Найдите условие 'if (!tempMovingDown)'. Оператор '!' инвертировал логику: Марио убивает врагов только когда летит вверх. Уберите восклицательный знак.", 10);
                            }
                            else
                            {
                                if (player.GetState() == 0)
                                {
                                    Lives--;
                                    livesLost = true;
                                }
                                else
                                {
                                    player.SetState(player.GetState() - 1); 

                                    if (!hintStateBugShown)
                                    {
                                        hintStateBugShown = true;
                                        ScheduleHint("ОШИБКА СОСТОЯНИЯ", "Урон от врага делает Марио сильнее!\n\nФайл: Form1.cs\nМетод: EntityCheck\n\nИнструкция: В блоке получения урона найдите 'player.SetState(player.GetState() + 1)' и замените плюс на минус, чтобы Марио уменьшался при уроне.", 30);
                                    }
                                }
                            }
                        }
                        else if ((entity.ID == "18" && entity.GetFltXVel() == 0))
                        {
                            invTimer = timer + 5;
                            if (player.GetRecPosition().X + (scaleSize / 2) > entity.GetRecPosition().X + (scaleSize / 2))
                            {
                                entity.SetFltXVel(-(speed * 1.5f));
                            }
                            else
                            {
                                entity.SetFltXVel((speed * 1.5f));
                            }
                        }
                    }
                    else
                    {
                        entity.Delete();
                    }
                }
                // Only if it is a coin
                if (entity.CheckCoin())
                {
                    entity.setActive(false);
                    coinCounter++; 

                    if (!hintCoinBug1Shown)
                    {
                        hintCoinBug1Shown = true;
                        ScheduleHint("ОШИБКА ЭКОНОМИКИ", "За одну монету с пола дают сразу 10!\n\nФайл: Form1.cs\nМетод: EntityCheck\n\nИнструкция: Найдите блок CheckCoin() и исправьте прибавление на coinCounter++;");
                    }
                }
                // Can add other types of entity here vvvv
            }
        }

        private List<string[]> FormatCSV(string filepath)
        {
            // Converts CSV to List<string[]> for handling, as size of the stage is unknown
            List<string[]> data = new List<string[]>();
            StreamReader reader = new StreamReader(filepath);
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                data.Add(line);
            }
            return data;
        }

        // Checks collisions under Mario
        private void CheckCollisionsDown(Block b, Player player)
        {
            bool tempMovingDown;
            if (player == Mario)
            {
                tempMovingDown = movingDown;
            }
            else
            {
                tempMovingDown = movingDown2;
            }
            if (player.GetRecPosition().Bottom > b.getRec().Top && player.GetRecPosition().Bottom < b.getRec().Top + (b.getSize() * intersectStrictness) && tempMovingDown)
            {
                player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - ((player.GetRecPosition().Y + player.GetSizeY()) - b.getRec().Y));
                player.SetGrounded(true);
                if (player == Mario)
                {
                    movingDown = false;
                }
                else
                {
                    movingDown2 = false;
                }
                player.SetFltYVel(0);
            }
        }

        // Checks collisions above Mario
        private void CheckCollisionsUp(Block b, Player player)
        {
            bool tempMovingUp;
            if (player == Mario)
            {
                tempMovingUp = movingUp;
            }
            else
            {
                tempMovingUp = movingUp2;
            }
            if (player.GetRecPosition().Top < b.getRec().Bottom && player.GetRecPosition().Top > b.getRec().Bottom - (b.getSize() * intersectStrictness) && tempMovingUp)
            {
                player.ChangePosition(player.GetRecPosition().X, player.GetRecPosition().Y - (player.GetRecPosition().Y - (b.getRec().Bottom)));
                if (player == Mario)
                {
                    previousJumpHeight = 0;
                }
                else
                {
                    previousJumpHeight2 = 0;
                }
            }
        }

        // Checks collisions to the left of Mario
        private void CheckCollisionsLeft(Block b, Player player)
        {
            bool tempMovingLeft;
            if (player == Mario)
            {
                tempMovingLeft = movingLeft;
            }
            else
            {
                tempMovingLeft = movingLeft2;
            }
            if (player.GetRecPosition().Left < b.getRec().Right && player.GetRecPosition().Left > b.getRec().Right - (b.getSize() * intersectStrictness) && tempMovingLeft)
            {
                player.ChangePosition(player.GetRecPosition().X - (((player.GetRecPosition().X + player.GetSizeX()) - (b.getRec().X + b.getSize()))) + b.getSize());
                if (player == Mario)
                {
                    movingLeft = false;
                }
                else
                {
                    movingLeft2 = false;
                }
                player.SetFltXVel(0);
            }
        }

        // Checks collisions to the right of Mario
        private void CheckCollisionsRight(Block b, Player player)
        {
            bool tempMovingRight;
            if (player == Mario)
            {
                tempMovingRight = movingRight;
            }
            else
            {
                tempMovingRight = movingRight2;
            }
            if (player.GetRecPosition().Right > b.getRec().Left && player.GetRecPosition().Right < b.getRec().Left + (b.getSize() * intersectStrictness) && tempMovingRight)
            {
                player.ChangePosition(player.GetRecPosition().X - ((player.GetRecPosition().X + player.GetSizeX()) - b.getRec().X));
                if (player == Mario)
                {
                    movingRight = false;
                }
                else
                {
                    movingRight2 = false;
                }
                offset = previousOffset;
                player.SetFltXVel(0);
            }
        }

        private void spriteClock_Tick_1(object sender, EventArgs e)
        {
            // Animation frame handling
            foreach (Block b in levelBlocks)
            {
                if (b.getAnimated())
                {
                    if (b.getBounce())
                    {
                        if (b.getTotalFrames() == b.getCurrentFrame())
                        {
                            b.setFrameBool(true);
                        }
                        if (b.getCurrentFrame() == 0)
                        {
                            b.setFrameBool(false);
                        }
                        if (b.getFrameBool())
                        {
                            b.setCurrentFrame(b.getCurrentFrame() - 1);
                        }
                        else
                        {
                            b.setCurrentFrame(b.getCurrentFrame() + 1);
                        }
                    }
                    else
                    {
                        if (b.getTotalFrames() == b.getCurrentFrame())
                        {
                            b.setCurrentFrame(0);
                        }
                        else
                        {
                            b.setCurrentFrame(b.getCurrentFrame() + 1);
                        }
                    }
                }
            }
        }

        // Check collision of Entity underneath
        private void checkEntityCollisionDown(Entity e, Block b)
        {
            if (e.GetRecPosition().Bottom > b.getRec().Top && e.GetRecPosition().Bottom < b.getRec().Top + (b.getSize() * intersectStrictness) && !b.getSemiSolid())
            {
                e.ChangePosition(Convert.ToInt32(e.GetRecPosition().X), Convert.ToInt32(e.GetRecPosition().Y) - ((Convert.ToInt32(e.GetRecPosition().Y) + e.GetSizeY()) - b.getRec().Y));
                e.SetFltYVel(0);
            }
        }

        // Check collision of Entity upwards
        private void checkEntityCollisionUp(Entity e, Block b)
        {
            if (!b.getSemiSolid())
            {
                e.ChangePosition(e.GetRecPosition().X, e.GetRecPosition().Y - (e.GetRecPosition().Y - (b.getRec().Bottom)));
                e.setFallHeight(0);
            }
        }

        // Check collision of Entity to the right
        private void checkEntityCollisionRight(Entity e, Block b)
        {
            if (e.GetRecPosition().Right > b.getRec().Left && e.GetRecPosition().Right < b.getRec().Left + (b.getSize() * intersectStrictness) && !b.getSemiSolid())
            {
                e.ChangePosition(Convert.ToInt32(e.GetRecPosition().X) - ((Convert.ToInt32(e.GetRecPosition().X) + e.GetSizeX()) - b.getRec().X));
                //e.SetFltXVel(-e.GetFltXVel());
                e.flipDirection();
                if (e.getID() == 11)
                {
                    e.Delete();
                    toAddEntity.Add(new Entity("Explosion", (int)(e.GetRecPosition().X + offset), e.GetRecPosition().Y, scaleSize, scaleSize, true, scaleSize, offset, entityLookUp, 1, 0, DebugMode));
                }
            }
        }

        // Check collision of Entity to the left
        private void checkEntityCollisionLeft(Entity e, Block b)
        {
            if (e.GetRecPosition().Left < b.getRec().Right && e.GetRecPosition().Left > b.getRec().Right - (b.getSize() * intersectStrictness) && !b.getSemiSolid())
            {
                if (e.getID() == 11)
                {
                    e.Delete();
                    toAddEntity.Add(new Entity("Explosion", (int)(e.GetRecPosition().X + offset), e.GetRecPosition().Y, scaleSize, scaleSize, true, scaleSize, offset, entityLookUp, 1, 0, DebugMode));
                }
                e.ChangePosition(Convert.ToInt32(e.GetRecPosition().X) - (((Convert.ToInt32(e.GetRecPosition().X) + e.GetSizeX()) - (b.getRec().X + b.getSize()))) + b.getSize());
                //e.SetFltXVel(-e.GetFltXVel());
                e.flipDirection();
            }
        }

        private void moveRight(Player player)
        {
            player.SetDirection("Right");
            if (player.GetRecPosition().X + (player.GetSizeX() * 2) >= gameWidth / 2 && offset + gameWidth + scaleSize < (level[0].Length * scaleSize))
            {
                // If Player is just before half the screen, then move blocks left instead (Player is stationary)
                previousOffset = offset;
                if (player.getRunning() || player.GetState() == 3 || player.GetState() == 4)
                {
                    offset += speed * 1.25f; 
                    frontX += speed * 1.25f;
                    if (backX < player.GetRecPosition().X - (11 * scaleSize))
                        backX += speed * 1.25f;

                    if (!hintCameraBugShown)
                    {
                        hintCameraBugShown = true;
                        ScheduleHint("ОШИБКА КАМЕРЫ", "Камера уезжает влево, когда вы бежите вправо!\n\nФайл: Form1.cs\nМетод: moveRight\n\nИнструкция: Найдите строку 'offset -= speed * 1.25f;' и верните сложение (+=), чтобы камера двигалась за игроком.", 50);
                    }
                }
                else
                {
                    if (playerCount > 1)
                    {
                        if (player == Mario)
                        {
                            Luigi.ChangePosition((int)(Luigi.GetRecPosition().X - speed));
                        }
                        else
                        {
                            Mario.ChangePosition((int)(Mario.GetRecPosition().X - speed));
                        }
                    }
                    // The camera has to follow the player, so the view slides forward as well
                    offset += speed;
                    frontX += speed;
                    if (backX < player.GetRecPosition().X - (11 * scaleSize))
                        backX += speed;
                }
            }
            else if (player.GetRecPosition().X + player.GetSizeX() < gameWidth)
            {
                // Moves Player right
                if (player.getRunning() || player.GetState() == 3 || player.GetState() == 4)
                {
                    player.SetFltXVel(speed * 1.25f);
                }
                else
                {
                    player.SetFltXVel(speed);
                }
            }
        }

        // When Mario enters the underground area
        private void UndergroundLoad()
        {
            currentState = Mario.GetState();
            if (Luigi != null)
            {
                currentState2 = Luigi.GetState();
            }
            level.Clear();
            entityList.Clear();
            if (!isUnderground)
            {
                LevelLoad(globalLevelName + "u", 0);
            }
            else
            {
                LevelLoad(globalLevelName.Substring(0, globalLevelName.Length - 1), offsetOut);
                frontX = (28 * scaleSize) + offsetOut;
                backX = (-3 * scaleSize) + offsetOut;
            }
            isUnderground = !isUnderground;
            StartLevel();
            MenuConfig();
            previousJumpHeight = 0;
            FirstLoad = false;
            if (!isUnderground)
            {
                Mario.ChangePosition(Convert.ToInt32(spawnBlock.getRec().X - offsetOut + (scaleSize / 2)), spawnBlock.getRec().Y);
                if (Luigi != null)
                {
                    Luigi.ChangePosition(Convert.ToInt32(spawnBlock.getRec().X - offsetOut + (scaleSize / 2)), spawnBlock.getRec().Y);
                }
            }
        }

        private void MenuSelector_Click(object sender, EventArgs e)
        {

        }

        private string ReadFile(string filepath)
        {
            using (StreamReader file = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filepath), true))
            {
                string contents = file.ReadLine();
                file.Close();
                return contents;
            }
        }
        private void EndGameSuccess()
        {
            movingStage = false;
            pipeDirection = "";

            tmrGame.Stop();

            MessageBox.Show($"Поздравляем! Вы прошли игру!\nВаш итоговый счет: {score}", "Победа!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            WriteFile("TopScore.txt", score.ToString());

            score = 0;
            coinCounter = 0;
            Lives = 3;
            worldNum = 1;
            levelNum = 1;
            timeCounter = 400;

            LevelLoad("TitleScreen", 0);
            StartLevel();
            MenuConfig();
            Luigi = null;
            Mario.SetState(0);

            tmrGame.Start();
        }

        private void WriteFile(string filepath, string input)
        {
            string contents = ReadFile(filepath);
            using (StreamWriter file = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filepath), false))
            {
                if (Convert.ToInt32(contents) < Convert.ToInt32(input))
                {
                    file.WriteLine(input);
                    file.Close();
                }
                else
                {
                    if (Convert.ToInt32(contents) > 99999)
                    {
                        file.WriteLine(99999);
                    }
                    else
                    {
                        file.WriteLine(contents);
                    }
                    file.Close();
                }
            }
        }


        private void ScheduleHint(string title, string message, int delayFrames = 50)
        {
            // All bugs are fixed in this version, so no instructions are needed
            if (!hintsEnabled)
            {
                return;
            }
            scheduledHintTitle = title;
            scheduledHintText = message;
            hintTimerTrigger = timer + delayFrames;
        }

        Panel hintPanel;
        Label hintTitle;
        Label hintText;
        bool isHintActive = false;

        // Converts a play field size into window pixels
        private int Scaled(int value, float scale)
        {
            int result = (int)(value * scale);
            if (result < 1)
            {
                result = 1;
            }
            return result;
        }

        // Closes and forgets the hint window, it is rebuilt the next time a hint is shown
        private void DestroyHintPanel()
        {
            if (hintPanel != null)
            {
                this.Controls.Remove(hintPanel);
                hintPanel.Dispose();
            }
            hintPanel = null;
            hintTitle = null;
            hintText = null;
        }

        private void ShowHint(string title, string message)
        {
            // The hint window is a normal control as well, so it is built at the current scale
            float scale = GetRenderScale();
            if (scale < 1f)
            {
                scale = 1f;
            }

            if (hintPanel == null)
            {
                hintPanel = new Panel();
                hintPanel.Size = new Size(Scaled(500, scale), Scaled(300, scale));
                hintPanel.BackColor = Color.Black;
                hintPanel.BorderStyle = BorderStyle.Fixed3D;

                hintTitle = new Label();
                hintTitle.ForeColor = Color.Yellow;
                hintTitle.AutoSize = false;
                hintTitle.Size = new Size(Scaled(480, scale), Scaled(40, scale));
                hintTitle.Location = new Point(Scaled(10, scale), Scaled(10, scale));
                hintTitle.TextAlign = ContentAlignment.MiddleCenter;

                hintText = new Label();
                hintText.ForeColor = Color.White;
                hintText.AutoSize = false;
                hintText.Size = new Size(Scaled(480, scale), Scaled(200, scale));
                hintText.Location = new Point(Scaled(10, scale), Scaled(60, scale));
                hintText.TextAlign = ContentAlignment.TopLeft;

                Label hintClose = new Label();
                hintClose.ForeColor = Color.LightGreen;
                hintClose.Text = "Нажмите ENTER чтобы закрыть и продолжить...";
                hintClose.AutoSize = false;
                hintClose.Size = new Size(Scaled(480, scale), Scaled(30, scale));
                hintClose.Location = new Point(Scaled(10, scale), Scaled(260, scale));
                hintClose.TextAlign = ContentAlignment.MiddleCenter;

                hintPanel.Controls.Add(hintTitle);
                hintPanel.Controls.Add(hintText);
                hintPanel.Controls.Add(hintClose);
                this.Controls.Add(hintPanel);
            }

            if (customFont != null)
            {
                hintTitle.Font = new Font(customFont.FontFamily, 14 * scale, FontStyle.Regular);
                hintText.Font = new Font(customFont.FontFamily, 10 * scale, FontStyle.Regular);
                hintPanel.Controls[2].Font = new Font(customFont.FontFamily, 10 * scale, FontStyle.Regular);
            }
            else
            {
                hintTitle.Font = new Font("Arial", 16 * scale, FontStyle.Bold);
                hintText.Font = new Font("Arial", 12 * scale, FontStyle.Regular);
            }

            hintTitle.Text = title;
            hintText.Text = message;

            // Centred over the play field instead of the whole window
            Rectangle view = GetFieldViewport();
            hintPanel.Location = new Point(view.X + ((view.Width - hintPanel.Width) / 2), view.Y + ((view.Height - hintPanel.Height) / 2));

            hintPanel.BringToFront();
            hintPanel.Visible = true;
            isHintActive = true;

            tmrGame.Stop();

            movingRight = false;
            movingLeft = false;
            if (Luigi != null) { movingRight2 = false; movingLeft2 = false; }
        }
    }
}
