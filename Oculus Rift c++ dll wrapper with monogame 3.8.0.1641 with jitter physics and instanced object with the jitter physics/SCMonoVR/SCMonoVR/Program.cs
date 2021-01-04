using System;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;

using System.Threading;
using OculusRiftSample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace JitterDemo
{
    static class Program
    {
        static JitterDemo game;
        static int startOVRDrawThread = 0;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread()]
        static void Main(string[] args)
        {

            


            using (game = new JitterDemo())
            {
                //Jitter.DynamicTree dt = new Jitter.DynamicTree();

                //JBBox jb;
                //jb.Min = JVector.Zero;
                //jb.Max = JVector.One;

                //JBBox jb2;
                //jb2.Min = JVector.Zero;
                //jb.Max = JVector.One * 2.0f;

                //dt.CreateProxy(ref jb, 1);
                //dt.CreateProxy(ref jb, 2);

                //JBBox testBox;
                //testBox.Min = JVector.Zero;
                //testBox.Max = JVector.One *20.0f;

                //dt.Query(bla, ref testBox);
                //dt.MoveProxy


                /*if (startOVRDrawThread == 0)
                {
                    Thread main_thread_update = new Thread(() =>
                    {
                        OculusRift rift = new OculusRift();
                        RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];

                    _thread_looper:

                        try
                        {
                            if (game != null)
                            {


                                if (game.hasInit == 1)
                                {
                                    //Console.WriteLine("test1");
                                    int result = rift.Init(game.GraphicsDevice);

                                    if (result != 0)
                                    {
                                        throw new InvalidOperationException("rift.Init result: " + result);
                                    }
                                    for (int eye = 0; eye < 2; eye++)
                                    {
                                        renderTargetEye[eye] = rift.CreateRenderTargetForEye(eye);
                                    }

                                    game.hasInit = 2;
                                }

                                if (game.hasInit == 2)
                                {
                                    ///Console.WriteLine("test2");
                                    /*for (int eye = 0; eye < 2; eye++)
                                    {

                                        game.GraphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                                        game.GraphicsDevice.Clear(Color.CornflowerBlue);

                                        game.GraphicsDevice.BlendState = BlendState.Opaque;
                                        game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                                    }

                                    var result = rift.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

                                    //game.GraphicsDevice.SetRenderTarget(null);
                                    //game.GraphicsDevice.Clear(Color.CornflowerBlue);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        Thread.Sleep(1);
                        goto _thread_looper;

                        //ShutDown();
                        //ShutDownGraphics();

                    }, 0);

                    main_thread_update.IsBackground = true;
                    main_thread_update.SetApartmentState(ApartmentState.STA);
                    main_thread_update.Start();

                    startOVRDrawThread = 1;
                }*/
                game.Run();               
            }
        }

        private static bool bla(int i)
        {

            return true;
        }
    }
}

