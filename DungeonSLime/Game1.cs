using DungeonSLime.Scenes;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameLibrary;
using MonoGameGum;



namespace DungeonSLime;

public class Game1 : Core
{
    // The background theme song.
    private Song _themeSong;

    public Game1() : base("贪吃蛇大作战", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        // Start playing the background music.
        Audio.PlaySong(_themeSong);
        
        // Initialize the Gum UI service
        InitializeGum();

        // Start the game with the title scene.
        ChangeScene(new TitleScene());
        
    }
    
    private void InitializeGum()
    {
        // Initialize the Gum service. The second parameter specifies
        // the version of the default visuals to use. V2 is the latest
        // version.
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3); //基础初始化，用我们的游戏实例搭建Gum系统

        // Tell the Gum service which content manager to use.  We will tell it to
        // use the global content manager from our Core.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content; //指出加载素材时使用的是ContentManager

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for Ui control.
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads); //显式注册键盘和手柄输入设备

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
            new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
            new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });  //自定义了导航按键

        // The assets created for the UI were done so at 1/4th the size to keep the size of the
        // texture atlas small.  So we will set the default canvas size to be 1/4th the size of
        // the game's resolution then tell gum to zoom in by a factor of 4.
        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth / 4.0f;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight / 4.0f;
        GumService.Default.Renderer.Camera.Zoom = 4.0f;  //这里让Gum以全分辨率渲染UI
    }


    protected override void LoadContent()
    {
        // Load the background theme music.
        _themeSong = Content.Load<Song>("audio/theme");
    }
}
