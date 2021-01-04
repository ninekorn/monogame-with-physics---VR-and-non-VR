using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using OculusRiftSample;

namespace SCMonoVR
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        OculusRift rift = new OculusRift();
        RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];

        //ManyCubes manyCubes;
        //SpriteBatch spriteBatch;

        const float PlayerSize = 1;
        Matrix playerMatrix = Matrix.CreateScale(PlayerSize);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            gdm = new GraphicsDeviceManager(this);
            gdm.PreferredBackBufferWidth = 800;
            gdm.PreferredBackBufferHeight = 600;
            gdm.GraphicsProfile = GraphicsProfile.HiDef;
            gdm.SynchronizeWithVerticalRetrace = false;



            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // initialize the Rift
            int result = rift.Init(GraphicsDevice);

            if (result != 0)
            {
                throw new InvalidOperationException("rift.Init result: " + result);
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            for (int eye = 0; eye < 2; eye++)
                renderTargetEye[eye] = rift.CreateRenderTargetForEye(eye);

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
            rift.TrackHead();

            DoPlayerMotion();
        }


        protected override void Draw(GameTime gameTime)
        {

            // draw scene for both eyes into respective rendertarget
            for (int eye = 0; eye < 2; eye++)
            {
                GraphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                GraphicsDevice.Clear(new Color(130, 180, 255));

                Matrix view = rift.GetEyeViewMatrix(eye, playerMatrix);
                Matrix projection = rift.GetProjectionMatrix(eye);

                //manyCubes.Draw(view, projection);

                //base.Draw(gameTime);
            }

            // submit rendertargets to the Rift
            int result = rift.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }

        /*
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }*/

        void DrawEyeViewIntoBackbuffer(int eye)
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var pp = GraphicsDevice.PresentationParameters;

            int height = pp.BackBufferHeight;
            int width = Math.Min(pp.BackBufferWidth, (int)(height * rift.GetRenderTargetAspectRatio(eye)));
            int offset = (pp.BackBufferWidth - width) / 2;

            _spriteBatch.Begin();
            _spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), Color.White);
            _spriteBatch.End();
        }

        void DoPlayerMotion()
        {
            var mouse = Mouse.GetState();

            var pp = GraphicsDevice.PresentationParameters;

            playerMatrix.Translation = new Vector3(
                0.01f * (mouse.X - pp.BackBufferWidth / 2),
                0.7f,
                0.01f * (mouse.Y - pp.BackBufferHeight / 2));
        }
    }
}
