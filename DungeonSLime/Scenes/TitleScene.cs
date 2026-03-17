using System;
using DungeonSlime.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Graphic;
using System.Collections.Generic;
using MonoGameGum;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;

namespace DungeonSLime.Scenes;

public class TitleScene : Scene
{
    private const string DUNGEON_TEXT = "Dungeon";
    private const string SLIME_TEXT = "Slime";

    // The font to use to render normal text.
    private SpriteFont _font;

    // The font used to render the title text.
    private SpriteFont _font5x;

    // The position to draw the dungeon text at.
    private Vector2 _dungeonTextPos;

    // The origin to set for the dungeon text.
    private Vector2 _dungeonTextOrigin;

    // The position to draw the slime text at.
    private Vector2 _slimeTextPos;

    // The origin to set for the slime text.
    private Vector2 _slimeTextOrigin;
    
    // Title界面的slime列表
    private List<Slime> _titleSlimes;
    
    // 移动速度
    private const float MOVEMENT_SPEED = 3.0f;
    
    // 排行榜相关变量
    private List<ScoreEntry> _leaderboardEntries;
    private Vector2 _leaderboardTitlePos;
    private Vector2 _leaderboardTitleOrigin;
    private Vector2 _leaderboardEntriesPos;
    private Vector2 _leaderboardEntriesOrigin;
    
    // The texture used for the background pattern.背景图案纹理
    private Texture2D _backgroundPattern;

    // The destination rectangle for the background pattern to fill.用于填充背景图案的目标矩形区域
    private Rectangle _backgroundDestination;

    // The offset (偏移量) to apply when drawing the background pattern so it appears to
    // be scrolling.
    private Vector2 _backgroundOffset;

    // The speed that the background pattern scrolls.
    private float _scrollSpeed = 50.0f;
    
    
    private SoundEffect _uiSoundEffect;
    private Panel _titleScreenButtonsPanel;
    private Panel _optionsPanel;
    
    // The options button used to open the option's menu.
    private AnimatedButton _optionsButton;
    
    // The back button used to exit the options menu back to the title menu.
    private AnimatedButton _optionsBackButton;
    
    private OptionsSlider _musicSlider;
    private AnimatedButton _startButton;
    
    // Reference to the texture atlas that we can pass to UI elements when they
    // are created.
    private TextureAtlas _atlas;
    
    
    
    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

        // Load the font for the title text.
        _font5x = Content.Load<SpriteFont>("fonts/04B_30_5x");
        
        // Load the background pattern texture.
        _backgroundPattern = Content.Load<Texture2D>("images/background-pattern");
        
        // Load the sound effect to play when ui actions occur.
        _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
        
        // Load the texture atlas from the xml configuration file.
        _atlas = TextureAtlas.FromFile(Core.Content, "images/File.xml");
    }
    
    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // While on the title screen, we can enable exit on escape so the player
        // can close the game by pressing the escape key.
        Core.ExitOnEscape = true;

        // 初始化title界面的slime
        _titleSlimes = new List<Slime>();
        InitializeTitleSlimes();

        // 加载排行榜数据
        _leaderboardEntries = LeaderboardManager.GetTopEntries(3);

        // Set the position and origin for the Dungeon text.
        Vector2 size = _font5x.MeasureString(DUNGEON_TEXT);
        _dungeonTextPos = new Vector2(640, 100);
        _dungeonTextOrigin = size * 0.5f;

        // Set the position and origin for the Slime text.
        size = _font5x.MeasureString(SLIME_TEXT);
        _slimeTextPos = new Vector2(757, 207);
        _slimeTextOrigin = size * 0.5f;

        // Set the position and origin for the leaderboard title.
        string leaderboardTitle = "Top 3";
        size = _font.MeasureString(leaderboardTitle);
        _leaderboardTitlePos = new Vector2(640, 300); // 左侧20%位置
        _leaderboardTitleOrigin = size * 0.5f;

        // Set the position and origin for the leaderboard entries.
        string leaderboardEntries = GenerateLeaderboardText();
        size = _font.MeasureString(leaderboardEntries);
        _leaderboardEntriesPos = new Vector2(640, 400); // 左侧20%位置，下方
        _leaderboardEntriesOrigin = size * 0.5f;
        
        // Initialize the offset of the background pattern at zero.
        _backgroundOffset = Vector2.Zero;

        // Set the background pattern destination rectangle to fill the entire
        // screen background.
        _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        
        InitializeUI();
    }
    
    /// <summary>
    /// 初始化title界面的8只slime
    /// </summary>
    private void InitializeTitleSlimes()
    {
        // 创建纹理图集
        _atlas = TextureAtlas.FromFile(Content, "images/File.xml");

        for (int i = 0; i < 8; i++)
        {
            // 创建slime精灵
            AnimatedSprite slimeSprite = _atlas.CreateAnimatedSprite("slime-animation");
            slimeSprite.Scale = new Vector2(4.0f, 4.0f);
            
            // 设置随机颜色
            slimeSprite.Color = GetRandomColor();
            
            // 随机位置
            int totalColumns = Core.GraphicsDevice.PresentationParameters.BackBufferWidth / (int)slimeSprite.Width;
            int totalRows = Core.GraphicsDevice.PresentationParameters.BackBufferHeight / (int)slimeSprite.Height;
            
            int column = Random.Shared.Next(0, totalColumns);
            int row = Random.Shared.Next(0, totalRows);
            
            Vector2 position = new Vector2(column * slimeSprite.Width, row * slimeSprite.Height);
            
            // 随机左右移动方向
            float direction = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
            Vector2 velocity = new Vector2(direction * MOVEMENT_SPEED, 0);
            
            // 创建slime对象
            Slime slime = new Slime(E_SlimeType.Head, slimeSprite, position)
            {
                Velocity = velocity,
                LastDirection = velocity
            };
            
            _titleSlimes.Add(slime);
        }
    }
    
    /// <summary>
    /// 获取随机颜色
    /// </summary>
    private Color GetRandomColor()
    {
        Color[] colors = {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple,
            Color.Orange, Color.Pink, Color.Cyan, Color.Magenta, Color.Lime,
            Color.White, Color.Gray, Color.Brown, Color.Violet, Color.Turquoise
        };
        
        return colors[Random.Shared.Next(colors.Length)];
    }
    
    public override void Update(GameTime gameTime)
    {
        // 更新title界面的slime
        UpdateTitleSlimes(gameTime);
        
        // If the user presses enter, 等价于鼠标点击
        if (Core.InputMgr.Keyboard.WasKeyJustPressed(Keys.Enter) || Core.InputMgr.GamePads[0].WasButtonJustPressed(Buttons.A))
        {
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
            
            if (_optionsButton.IsFocused)
            {
                HandleOptionsClicked(this, EventArgs.Empty);
            }
            else if(_startButton.IsFocused)
            {
                HandleStartClicked(this, EventArgs.Empty);
            }
            
        }
        
        // 刷新排行榜数据（每次更新都重新加载，确保显示最新数据）
        _leaderboardEntries = LeaderboardManager.GetTopEntries(3);
        
        // Update the offsets for the background pattern wrapping so that it
        // scrolls down and to the right.
        float offset = _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _backgroundOffset.X -= offset;
        _backgroundOffset.Y -= offset;

        // Ensure that the offsets do not go beyond the texture bounds so it is
        // a seamless wrap (平铺).
        _backgroundOffset.X %= _backgroundPattern.Width;
        _backgroundOffset.Y %= _backgroundPattern.Height;
        
        GumService.Default.Update(gameTime);
    }
    
    /// <summary>
    /// 更新title界面的slime
    /// </summary>
    private void UpdateTitleSlimes(GameTime gameTime)
    {
        Rectangle screenBounds = new Rectangle(
            0, 0,
            Core.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Core.GraphicsDevice.PresentationParameters.BackBufferHeight
        );
        
        // 更新所有slime的位置和动画
        for (int i = _titleSlimes.Count - 1; i >= 0; i--)
        {
            Slime slime = _titleSlimes[i];
            
            // 更新位置
            slime.Position += slime.Velocity;
            
            // 更新动画
            slime.Update(gameTime);
            
            // 检查是否完全移出屏幕
            if (slime.IsOffScreen(screenBounds))
            {
                // 删除完全移出屏幕的slime
                _titleSlimes.RemoveAt(i);
                
                // 生成新的slime
                SpawnNewSlime();
            }
        }
    }
    
    /// <summary>
    /// 生成新的slime
    /// </summary>
    private void SpawnNewSlime()
    {
        // 创建纹理图集
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/File.xml");

        // 创建slime精灵
        AnimatedSprite slimeSprite = atlas.CreateAnimatedSprite("slime-animation");
        slimeSprite.Scale = new Vector2(4.0f, 4.0f);
        
        // 设置随机颜色
        slimeSprite.Color = GetRandomColor();
        
        // 随机位置
        int totalColumns = Core.GraphicsDevice.PresentationParameters.BackBufferWidth / (int)slimeSprite.Width;
        int totalRows = Core.GraphicsDevice.PresentationParameters.BackBufferHeight / (int)slimeSprite.Height;
        
        int column = Random.Shared.Next(0, totalColumns);
        int row = Random.Shared.Next(0, totalRows);
        
        Vector2 position = new Vector2(column * slimeSprite.Width, row * slimeSprite.Height);
        
        // 随机左右移动方向
        float direction = Random.Shared.Next(0, 2) == 0 ? -1 : 1;
        Vector2 velocity = new Vector2(direction * MOVEMENT_SPEED, 0);
        
        // 创建slime对象
        Slime slime = new Slime(E_SlimeType.Head, slimeSprite, position)
        {
            Velocity = velocity,
            LastDirection = velocity
        };
        
        _titleSlimes.Add(slime);
    }
    
    /// <summary>
    /// 生成排行榜文本
    /// </summary>
    /// <returns>格式化的排行榜文本</returns>
    private string GenerateLeaderboardText()
    {
        if (_leaderboardEntries == null || _leaderboardEntries.Count == 0)
        {
            return "No scores yet";
        }
        
        string text = "";
        for (int i = 0; i < _leaderboardEntries.Count; i++)
        {
            var entry = _leaderboardEntries[i];
            text += $"{i + 1}. {entry.Score} - {entry.FormatSurvivalTime()}\n\n";
        }
        
        // 移除最后一个换行符
        if (text.EndsWith("\n"))
        {
            text = text.Substring(0, text.Length - 1);
        }
        
        return text;
    }

    #region UI处理

        private void CreateTitlePanel()
        {
            // Create a container to hold all of our buttons
            _titleScreenButtonsPanel = new Panel();
            _titleScreenButtonsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _titleScreenButtonsPanel.AddToRoot();

            _startButton = new AnimatedButton(_atlas);
            _startButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
            _startButton.X = 50;
            _startButton.Y = -12;
            _startButton.Width = 30;
            _startButton.Text = "Start";
            _startButton.Click += HandleStartClicked;
            _titleScreenButtonsPanel.AddChild(_startButton);

            _optionsButton = new AnimatedButton(_atlas);
            _optionsButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsButton.X = -50;
            _optionsButton.Y = -12;
            _optionsButton.Width = 30;
            _optionsButton.Text = "Options";
            _optionsButton.Click += HandleOptionsClicked;
            _titleScreenButtonsPanel.AddChild(_optionsButton);

            _startButton.IsFocused = true;
        }
        
        private void HandleStartClicked(object sender, EventArgs e)
        {
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            // Change to the game scene to start the game.
            Core.ChangeScene(new GameScene());
        }
        
        private void HandleOptionsClicked(object sender, EventArgs e)
        {
            Console.WriteLine("HandleOptionsClicked");
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
        
            // Set the title panel to be invisible.
            _titleScreenButtonsPanel.IsVisible = false;

            // Set the options panel to be visible.
            _optionsPanel.IsVisible = true;

            // Give the back button on the options panel focus.
            _musicSlider.IsFocused = true;
        }
        
        private void CreateOptionsPanel() {
            _optionsPanel = new Panel();
            _optionsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _optionsPanel.IsVisible = false;
            _optionsPanel.AddToRoot();

            TextRuntime optionsText = new TextRuntime();
            optionsText.X = 10;
            optionsText.Y = 10;
            optionsText.Text = "OPTIONS";
            optionsText.UseCustomFont = true;
            optionsText.FontScale = 0.5f;
            optionsText.CustomFontFile = @"fonts/04b_30.fnt";
            _optionsPanel.AddChild(optionsText);

            _musicSlider = new OptionsSlider(_atlas);
            _musicSlider.Name = "MusicSlider";
            _musicSlider.Text = "MUSIC";
            _musicSlider.Anchor(Gum.Wireframe.Anchor.Top);
            _musicSlider.Y = 30f;
            _musicSlider.Minimum = 0;
            _musicSlider.Maximum = 1;
            _musicSlider.Value = Core.Audio.SongVolume;
            _musicSlider.SmallChange = .1;
            _musicSlider.LargeChange = .2;
            _musicSlider.ValueChanged += HandleMusicSliderValueChanged;
            _musicSlider.ValueChangeCompleted += HandleMusicSliderValueChangeCompleted;
            _optionsPanel.AddChild(_musicSlider);

            OptionsSlider sfxSlider = new OptionsSlider(_atlas);
            sfxSlider.Name = "SfxSlider";
            sfxSlider.Text = "SFX";
            sfxSlider.Anchor(Gum.Wireframe.Anchor.Top);
            sfxSlider.Y = 93;
            sfxSlider.Minimum = 0;
            sfxSlider.Maximum = 1;
            sfxSlider.Value = Core.Audio.SoundEffectVolume;
            sfxSlider.SmallChange = .1;
            sfxSlider.LargeChange = .2;
            sfxSlider.ValueChanged += HandleSfxSliderChanged;
            sfxSlider.ValueChangeCompleted += HandleSfxSliderChangeCompleted;
            _optionsPanel.AddChild(sfxSlider);

            _optionsBackButton = new AnimatedButton(_atlas);
            _optionsBackButton.Text = "BACK";
            _optionsBackButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsBackButton.X = -28f;
            _optionsBackButton.Y = -10f;
            _optionsBackButton.Click += HandleOptionsButtonBack;
            _optionsPanel.AddChild(_optionsBackButton);
        }
        
        /// <summary>
        /// 处理音效音量滑块值变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleSfxSliderChanged(object sender, EventArgs args)
        {
            // Intentionally not playing the UI sound effect here so that it is not
            // constantly triggered as the user adjusts the slider's thumb on the
            // track.

            // Get a reference to the sender as a Slider.
            var slider = (Slider)sender;

            // Set the global sound effect volume to the value of the slider.;
            Core.Audio.SoundEffectVolume = (float)slider.Value;
        }
        
        private void HandleSfxSliderChangeCompleted(object sender, EventArgs e)
        {
            // Play the UI Sound effect so the player can hear the difference in audio.
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }
        
        private void HandleMusicSliderValueChanged(object sender, EventArgs args)
        {
            // Intentionally not playing the UI sound effect here so that it is not
            // constantly triggered as the user adjusts the slider's thumb on the
            // track.

            // Get a reference to the sender as a Slider.
            var slider = (Slider)sender;

            // Set the global song volume to the value of the slider.
            Core.Audio.SongVolume = (float)slider.Value;
        }
        
        private void HandleMusicSliderValueChangeCompleted(object sender, EventArgs args)
        {
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(_uiSoundEffect);
        }

        private void HandleOptionsButtonBack(object sender, EventArgs e)
        {
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(_uiSoundEffect);

            // Set the title panel to be visible.
            _titleScreenButtonsPanel.IsVisible = true;

            // Set the options panel to be invisible.
            _optionsPanel.IsVisible = false;

            // Give the options button on the title panel focus since we are coming
            // back from the options screen.
            _optionsButton.IsFocused = true;
        }

        private void InitializeUI()
        {
            // Clear out any previous UI in case we came here from
            // a different screen:
            GumService.Default.Root.Children.Clear();

            CreateTitlePanel();
            CreateOptionsPanel();
        }

    
    #endregion







    
    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        // Draw the background pattern first using the PointWrap sampler state.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
        Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White * 0.5f);
        Core.SpriteBatch.End();

        if (_titleScreenButtonsPanel.IsVisible)
        {
            // Begin the sprite batch to prepare for rendering.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            // 绘制title界面的slime
            foreach (Slime slime in _titleSlimes)
            {
                slime.Draw(Core.SpriteBatch);
            }

            // Draw the leaderboard title.
            Core.SpriteBatch.DrawString(_font, "Top 3", _leaderboardTitlePos, Color.Yellow, 0.0f,
                _leaderboardTitleOrigin, 1.0f, SpriteEffects.None, 0.0f);

            // Draw the leaderboard entries.
            string leaderboardText = GenerateLeaderboardText();
            Core.SpriteBatch.DrawString(_font, leaderboardText, _leaderboardEntriesPos, Color.Yellow, 0.0f,
                _leaderboardEntriesOrigin, 0.7f, SpriteEffects.None, 0.0f);

            // Always end the sprite batch when finished.
            Core.SpriteBatch.End();
            
            
            
            // Begin the sprite batch to prepare for rendering.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // The color to use for the drop shadow text.
            Color dropShadowColor = Color.Black * 0.5f;

            // Draw the Dungeon text slightly offset from it is original position and
            // with a transparent color to give it a drop shadow
            Core.SpriteBatch.DrawString(_font5x, DUNGEON_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Dungeon text on top of that at its original position
            Core.SpriteBatch.DrawString(_font5x, DUNGEON_TEXT, _dungeonTextPos, Color.White, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Slime text slightly offset from it is original position and
            // with a transparent color to give it a drop shadow
            Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Slime text on top of that at its original position
            Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Always end the sprite batch when finished.
            Core.SpriteBatch.End();
            
        }

        GumService.Default.Draw();

    }




}