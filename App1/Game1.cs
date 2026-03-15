using System;
using System.Collections.Generic;
using App1.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ClassLibrary;
using ClassLibrary.Graphic;
using ClassLibrary.Input;

namespace App1;

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

        // Start the game with the title scene.
        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        // Load the background theme music.
        _themeSong = Content.Load<Song>("audio/theme");
    }
}
