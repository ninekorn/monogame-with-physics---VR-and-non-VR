#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Jitter.Dynamics.Joints;
using System.Reflection;
using Jitter.Forces;
using System.Diagnostics;

using SingleBodyConstraints = Jitter.Dynamics.Constraints.SingleBody;
using System.IO;
using Jitter.DataStructures;
#endregion

using OculusRiftSample;
using SCCoreSystems;

namespace JitterDemo
{
    public enum BodyTag { DrawMe, DontDrawMe,CompoundOBJ,InstancedCube }

    public class JitterDemo : Microsoft.Xna.Framework.Game
    {
        struct InstanceInfoPos
        {
            public Vector4 dirForward;
            public Vector4 dirRight;
            public Vector4 dirUp;
        };


        public struct InstanceInfo
        {
            public Matrix Position;

            //public Vector4 Position;
            //public Vector4 dirForward;
            //public Vector4 dirRight;
            //public Vector4 dirUp;
            public Vector2 AtlasCoordinate;
            /*public Vector4 Position0;
            public Vector4 Position1;
            public Vector4 Position2;
            public Vector4 Position3;*/
            //public Vector2 AtlasCoordinate;
            //public Vector4 instance0;
        }



        const int instx = 5;
        const int insty = 5;
        const int instz = 5;

        Int32 instanceCount = instx * insty* instz;

        VertexDeclaration instanceVertexDeclarationPos;
        VertexDeclaration instanceVertexDeclaration;

        VertexBuffer instanceBufferPos;
        VertexBuffer instanceBuffer;
        VertexBuffer geometryBuffer;
        IndexBuffer indexBuffer;
        VertexBufferBinding[] bindings;
        InstanceInfo[] instances;
        InstanceInfoPos[] instancesPos;
        

        Texture2D texture;
        Effect effect;
        int effectLoaded = 0;
        //private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public World World { private set; get; }
        //ManyCubes manyCubes;
        public static SharpDX.Matrix originRot = SharpDX.Matrix.Identity;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        OculusRift rift = new OculusRift();
        RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];

        const float PlayerSize = 1;
        Matrix playerMatrix = Matrix.CreateScale(PlayerSize);

        private enum Primitives { box, sphere, cylinder, cone, capsule, convexHull }

        private Primitives3D.GeometricPrimitive[] primitives = new Primitives3D.GeometricPrimitive[6];


        // ConvexHullObject
        List<ConvexHullObject> convexObj = new List<ConvexHullObject>();



        List<JVector> instancedVector = new List<JVector>();
        List<RigidBody> instancedRigidBodies = new List<RigidBody>();

        private Random random = new Random();

        private Color backgroundColor = new Color(63, 66, 73);
        private bool multithread = true;
        private int activeBodies = 0;

        private GamePadState padState;
        private KeyboardState keyState;
        private MouseState mouseState;

        public Camera Camera { private set; get; }
        public Display Display { private set; get; }
        public DebugDrawer DebugDrawer { private set; get; }
        public BasicEffect BasicEffect { private set; get; }
        public List<Scenes.Scene> PhysicScenes { private set; get; }
        private int currentScene = 0;

        RasterizerState wireframe, cullMode, normal;

        Color[] rndColors;


        float fovAngle;// = MathHelper.ToRadians(45);  // convert 45 degrees to radians
        float aspectRatio;// = graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight;
        float near;// = 0.01f; // the near clipping plane distance
        float far;// = 100f; // the far clipping plane distance





        public JitterDemo()
        {
            //_graphics = new GraphicsDeviceManager(this);
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            CollisionSystem collision = new CollisionSystemPersistentSAP();

            World = new World(collision);
            World.AllowDeactivation = true;
            World.SetIterations(3, 3);
            World.Gravity = new JVector(0, -9.81f, 0);
            World.ContactSettings.AllowedPenetration = 0.01f;

        }

        protected override void Initialize()
        {

            Camera = new Camera(this);
            Camera.Position = new Vector3(15, 15, 30);
            Camera.Target = Camera.Position + Vector3.Normalize(new Vector3(0, 0, 1));
            this.Components.Add(Camera);

            DebugDrawer = new DebugDrawer(this);
            this.Components.Add(DebugDrawer);

            Display = new Display(this);
            Display.DrawOrder = int.MaxValue;
            this.Components.Add(Display);

            primitives[(int)Primitives.box] = new Primitives3D.BoxPrimitive(GraphicsDevice);
            primitives[(int)Primitives.capsule] = new Primitives3D.CapsulePrimitive(GraphicsDevice);
            primitives[(int)Primitives.cone] = new Primitives3D.ConePrimitive(GraphicsDevice);
            primitives[(int)Primitives.cylinder] = new Primitives3D.CylinderPrimitive(GraphicsDevice);
            primitives[(int)Primitives.sphere] = new Primitives3D.SpherePrimitive(GraphicsDevice);
            primitives[(int)Primitives.convexHull] = new Primitives3D.SpherePrimitive(GraphicsDevice);



            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.EnableDefaultLighting();
            BasicEffect.PreferPerPixelLighting = true;

            this.PhysicScenes = new List<Scenes.Scene>();


            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == "JitterDemo.Scenes" && !type.IsAbstract && type.DeclaringType == null)
                {
                    if (type.Name == "SoftBodyJenga") currentScene = PhysicScenes.Count;
                    Scenes.Scene scene = (Scenes.Scene)Activator.CreateInstance(type, this);
                    this.PhysicScenes.Add(scene);
                }
            }

            if (PhysicScenes.Count > 0)
            {
                this.PhysicScenes[currentScene].Build();
            }


            // initialize the Rift
            int result = rift.Init(GraphicsDevice);

            if (result != 0)
            {
                throw new InvalidOperationException("rift.Init result: " + result);
            }

            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            float rad = sc_maths.DegreeToRadian(180);

            Matrix playerRot = Matrix.CreateRotationY(rad);


            playerMatrix *= playerRot;
            playerMatrix.M42 = 10;



            GenerateInstanceVertexDeclaration();
            GenerateGeometry(GraphicsDevice);
            GenerateInstanceInformation(GraphicsDevice, instx, insty, instz);

            bindings = new VertexBufferBinding[2];
            bindings[0] = new VertexBufferBinding(geometryBuffer);
            bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);
            //bindings[2] = new VertexBufferBinding(instanceBufferPos, 0, 1);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // create one rendertarget for each eye
            for (int eye = 0; eye < 2; eye++)
            {
                renderTargetEye[eye] = rift.CreateRenderTargetForEye(eye);
            }

            //manyCubes = new ManyCubes(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("Effect/shader");
            texture = Content.Load<Texture2D>("Texture/tex");

            if (effect != null)
            {
                //Console.WriteLine("test");
                effectLoaded = 1;
            }
        }

        private void GenerateInstanceVertexDeclaration()
        {
            /*VertexElement[] instanceStreamElements = new VertexElement[2];

            instanceStreamElements[0] =
                    new VertexElement(0, VertexElementFormat.Vector4,
                        VertexElementUsage.Position, 1);
            instanceStreamElements[1] =
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate, 1);*/



            /*instanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Normal, 1),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Normal, 2),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Normal, 3),
                new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.Normal, 4), //BlendWeight
                new VertexElement(80, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1) //+sizeof(float) * 4
            );*/

            /*instanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
            );*/


            /*instanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement((sizeof(float) * 16), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
            );*/


            instanceVertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
                new VertexElement((sizeof(float) * 16), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
            );


         


            /*instanceVertexDeclarationPos = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
                new VertexElement(((sizeof(float) * 4)), VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
                new VertexElement(((sizeof(float) * 8)), VertexElementFormat.Vector4, VertexElementUsage.Position, 4)
            );*/


            //instanceVertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }
        VertexPositionTexture[] vertices;


        //This creates a cube!
        public void GenerateGeometry(GraphicsDevice device)
        {
            vertices = new VertexPositionTexture[24];

            #region filling vertices
            vertices[0].Position = new Vector3(-1, 1, -1);
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].Position = new Vector3(1, 1, -1);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].Position = new Vector3(-1, 1, 1);
            vertices[2].TextureCoordinate = new Vector2(0, 1);
            vertices[3].Position = new Vector3(1, 1, 1);
            vertices[3].TextureCoordinate = new Vector2(1, 1);

            vertices[4].Position = new Vector3(-1, -1, 1);
            vertices[4].TextureCoordinate = new Vector2(0, 0);
            vertices[5].Position = new Vector3(1, -1, 1);
            vertices[5].TextureCoordinate = new Vector2(1, 0);
            vertices[6].Position = new Vector3(-1, -1, -1);
            vertices[6].TextureCoordinate = new Vector2(0, 1);
            vertices[7].Position = new Vector3(1, -1, -1);
            vertices[7].TextureCoordinate = new Vector2(1, 1);

            vertices[8].Position = new Vector3(-1, 1, -1);
            vertices[8].TextureCoordinate = new Vector2(0, 0);
            vertices[9].Position = new Vector3(-1, 1, 1);
            vertices[9].TextureCoordinate = new Vector2(1, 0);
            vertices[10].Position = new Vector3(-1, -1, -1);
            vertices[10].TextureCoordinate = new Vector2(0, 1);
            vertices[11].Position = new Vector3(-1, -1, 1);
            vertices[11].TextureCoordinate = new Vector2(1, 1);

            vertices[12].Position = new Vector3(-1, 1, 1);
            vertices[12].TextureCoordinate = new Vector2(0, 0);
            vertices[13].Position = new Vector3(1, 1, 1);
            vertices[13].TextureCoordinate = new Vector2(1, 0);
            vertices[14].Position = new Vector3(-1, -1, 1);
            vertices[14].TextureCoordinate = new Vector2(0, 1);
            vertices[15].Position = new Vector3(1, -1, 1);
            vertices[15].TextureCoordinate = new Vector2(1, 1);

            vertices[16].Position = new Vector3(1, 1, 1);
            vertices[16].TextureCoordinate = new Vector2(0, 0);
            vertices[17].Position = new Vector3(1, 1, -1);
            vertices[17].TextureCoordinate = new Vector2(1, 0);
            vertices[18].Position = new Vector3(1, -1, 1);
            vertices[18].TextureCoordinate = new Vector2(0, 1);
            vertices[19].Position = new Vector3(1, -1, -1);
            vertices[19].TextureCoordinate = new Vector2(1, 1);

            vertices[20].Position = new Vector3(1, 1, -1);
            vertices[20].TextureCoordinate = new Vector2(0, 0);
            vertices[21].Position = new Vector3(-1, 1, -1);
            vertices[21].TextureCoordinate = new Vector2(1, 0);
            vertices[22].Position = new Vector3(1, -1, -1);
            vertices[22].TextureCoordinate = new Vector2(0, 1);
            vertices[23].Position = new Vector3(-1, -1, -1);
            vertices[23].TextureCoordinate = new Vector2(1, 1);
            #endregion

            geometryBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
            geometryBuffer.SetData(vertices);

            #region filling indices

            int[] indices = new int[36];
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 1; indices[4] = 3; indices[5] = 2;

            indices[6] = 4; indices[7] = 5; indices[8] = 6;
            indices[9] = 5; indices[10] = 7; indices[11] = 6;

            indices[12] = 8; indices[13] = 9; indices[14] = 10;
            indices[15] = 9; indices[16] = 11; indices[17] = 10;

            indices[18] = 12; indices[19] = 13; indices[20] = 14;
            indices[21] = 13; indices[22] = 15; indices[23] = 14;

            indices[24] = 16; indices[25] = 17; indices[26] = 18;
            indices[27] = 17; indices[28] = 19; indices[29] = 18;

            indices[30] = 20; indices[31] = 21; indices[32] = 22;
            indices[33] = 21; indices[34] = 23; indices[35] = 22;

            #endregion

            indexBuffer = new IndexBuffer(device, typeof(int), 36, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        private void GenerateInstanceInformation(GraphicsDevice device,Int32 instx, Int32 insty,Int32 instz)
        {

            instancesPos = new InstanceInfoPos[instx * insty * instz];
            instances = new InstanceInfo[instx* insty* instz];
            Random rnd = new Random();

            for (int x = 0; x < instx; x++)
            {
                for (int y = 0; y < insty; y++)
                {
                    for (int z = 0; z < instz; z++)
                    {
                      
                        var index = x + instx * (y + insty * z);
                        var body = new RigidBody(new BoxShape(1, 1, 1));
                        body.Position = new JVector(x * 2, (y * 2) + 10, z * 2);
                        body.Orientation = JMatrix.Identity ;
                        //instances[index].Position = Vector4.Transform(new Vector3(x * 2, (y * 2) + 10, z * 2), WorldMatter);

                        //instances[index].Position = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);
                        // JVector.Transform(new JVector(x * 2, (y * 2) + 10, z * 2), JMatrix.Transpose(grabBody.Orientation));                                                                                             
                        //instances[index].Position0 = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);                                                                                              
                        //instances[index].Position1 = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);                                                                                           
                        //instances[index].Position2 = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);                                                                                  
                        //instances[index].Position3 = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);
                        //instances[index].instance0 = new Vector4(x * 2, (y * 2) + 10, z * 2, 1);
                        //instances[index].AtlasCoordinate = new Vector2(rnd.Next(0, 2), rnd.Next(0, 2));

                        JMatrix orientation = body.Orientation;
                        JQuaternion otherQuat = JQuaternion.CreateFromMatrix(orientation);


                        Quaternion quat = new Quaternion(otherQuat.X, otherQuat.Y, otherQuat.Z, otherQuat.W);

                        Vector4 direction_feet_forward;
                        Vector4 direction_feet_right;
                        Vector4 direction_feet_up;

                        direction_feet_forward = sc_maths._getDirectionXNA(Vector3.Forward, quat);
                        direction_feet_right = sc_maths._getDirectionXNA(Vector3.Right, quat);
                        direction_feet_up = sc_maths._getDirectionXNA(Vector3.Up, quat);


                        instances[index].Position = Matrix.Identity;
                        instances[index].Position.M11 = orientation.M11;
                        instances[index].Position.M12 = orientation.M12;
                        instances[index].Position.M13 = orientation.M13;

                        instances[index].Position.M21 = orientation.M21;
                        instances[index].Position.M22 = orientation.M22;
                        instances[index].Position.M23 = orientation.M23;

                        instances[index].Position.M31 = orientation.M31;
                        instances[index].Position.M32 = orientation.M32;
                        instances[index].Position.M33 = orientation.M33;

                        instances[index].Position *= Matrix.CreateScale(0.5f);

                        instances[index].Position.M41 = x * 2;
                        instances[index].Position.M42 = (y * 2) + 10;
                        instances[index].Position.M43 = z * 2;
                        instances[index].Position.M44 = 1.0f;

                        //instances[index].Position *= Matrix.CreateScale(1.0f);


                        //instances[index].dirForward = direction_feet_forward;
                        //instances[index].dirRight = direction_feet_right;
                        //instances[index].dirUp = direction_feet_up;

                        //instancesPos[index].dirForward = direction_feet_forward;
                        //instancesPos[index].dirRight = direction_feet_right;
                        //instancesPos[index].dirUp = direction_feet_up;



                        body.Tag = BodyTag.InstancedCube;

                        instancedVector.Add(body.Position);

                        instancedRigidBodies.Add(body);

                        World.AddBody(body);
                    }
                }
            }

           /*for (int i = 0; i < count; i++)
            {
                //random position example
                instances[i].World = new Vector4(-rnd.Next(100), rnd.Next(100), -rnd.Next(100), 1);

                instances[i].AtlasCoordinate = new Vector2(rnd.Next(0, 2), rnd.Next(0, 2));

                var body = new RigidBody(new BoxShape(1, 1, 1));
                body.Position = new JVector(instances[i].World.X, instances[i].World.Y, instances[i].World.Z);
                body.Tag = BodyTag.InstancedCube;

                instancedVector.Add(body.Position);

                instancedRigidBodies.Add(body);

                World.AddBody(body);
            }*/

            //Matrix mat = Matrix.Identity * Conversion.ToXNAMatrix(ori) * Matrix.CreateTranslation(Conversion.ToXNAVector(pos));

            instanceBuffer = new VertexBuffer(device, instanceVertexDeclaration, instx * insty * instz, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);

            //instanceBufferPos = new VertexBuffer(device, instanceVertexDeclarationPos, instx * insty * instz, BufferUsage.WriteOnly);
            //instanceBufferPos.SetData(instancesPos);

            

        }

        Matrix WorldMatter = Matrix.Identity;


        private Vector3 RayTo(int x, int y)
        {
            Vector3 nearSource = new Vector3(x, y, 0);
            Vector3 farSource = new Vector3(x, y, 1);

            Matrix world = Matrix.Identity;

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource, Camera.Projection, Camera.View, world);
            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(farSource, Camera.Projection, Camera.View, world);

            Vector3 direction = farPoint - nearPoint;
            return direction;
        }

        private void DestroyCurrentScene()
        {
            for (int i = this.Components.Count - 1; i >= 0; i--)
            {
                IGameComponent component = this.Components[i];

                if (component is Camera) continue;
                if (component is Display) continue;
                if (component is DebugDrawer) continue;

                this.Components.RemoveAt(i);
            }
            convexObj.Clear();

            World.Clear();
        }

        private bool PressedOnce(Keys key, Buttons button)
        {
            bool keyboard = keyState.IsKeyDown(key) && !keyboardPreviousState.IsKeyDown(key);

            if (key == Keys.Add) key = Keys.OemPlus;
            keyboard |= keyState.IsKeyDown(key) && !keyboardPreviousState.IsKeyDown(key);

            if (key == Keys.Subtract) key = Keys.OemMinus;
            keyboard |= keyState.IsKeyDown(key) && !keyboardPreviousState.IsKeyDown(key);

            bool gamePad = padState.IsButtonDown(button) && !gamePadPreviousState.IsButtonDown(button);

            return keyboard || gamePad;
        }


        #region update - global variables
        // Hold previous input states.
        KeyboardState keyboardPreviousState = new KeyboardState();
        GamePadState gamePadPreviousState = new GamePadState();
        MouseState mousePreviousState = new MouseState();

        // Store information for drag and drop
        JVector hitPoint, hitNormal;
        SingleBodyConstraints.PointOnPoint grabConstraint;
        RigidBody grabBody;
        float hitDistance = 0.0f;
        int scrollWheel = 0;
        #endregion

        protected override void Update(GameTime gameTime) //
        {
            padState = GamePad.GetState(PlayerIndex.One);
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            // let the user escape the demo
            if (PressedOnce(Keys.Escape, Buttons.Back)) this.Exit();

            // change threading mode
            if (PressedOnce(Keys.M, Buttons.A)) multithread = !multithread;

            if (PressedOnce(Keys.P, Buttons.A))
            {
                var e = World.RigidBodies.GetEnumerator();
                e.MoveNext(); e.MoveNext();
                World.RemoveBody(e.Current as RigidBody);
            }

            #region drag and drop physical objects with the mouse
            if (mouseState.LeftButton == ButtonState.Pressed &&
                mousePreviousState.LeftButton == ButtonState.Released ||
                padState.IsButtonDown(Buttons.RightThumbstickDown) &&
                gamePadPreviousState.IsButtonUp(Buttons.RightThumbstickUp))
            {
                JVector ray = Conversion.ToJitterVector(RayTo(mouseState.X, mouseState.Y));
                JVector camp = Conversion.ToJitterVector(Camera.Position);

                ray = JVector.Normalize(ray) * 100;

                float fraction;

                bool result = World.CollisionSystem.Raycast(camp, ray, RaycastCallback, out grabBody, out hitNormal, out fraction);

                if (result)
                {
                    hitPoint = camp + fraction * ray;

                    if (grabConstraint != null) World.RemoveConstraint(grabConstraint);

                    JVector lanchor = hitPoint - grabBody.Position;
                    lanchor = JVector.Transform(lanchor, JMatrix.Transpose(grabBody.Orientation));

                    grabConstraint = new SingleBodyConstraints.PointOnPoint(grabBody, lanchor);
                    grabConstraint.Softness = 0.01f;
                    grabConstraint.BiasFactor = 0.1f;

                    World.AddConstraint(grabConstraint);
                    hitDistance = (Conversion.ToXNAVector(hitPoint) - Camera.Position).Length();
                    scrollWheel = mouseState.ScrollWheelValue;
                    grabConstraint.Anchor = hitPoint;
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed || padState.IsButtonDown(Buttons.RightThumbstickDown))
            {
                hitDistance += (mouseState.ScrollWheelValue - scrollWheel) * 0.01f;
                scrollWheel = mouseState.ScrollWheelValue;

                if (grabBody != null)
                {
                    Vector3 ray = RayTo(mouseState.X, mouseState.Y); ray.Normalize();
                    grabConstraint.Anchor = Conversion.ToJitterVector(Camera.Position + ray * hitDistance);
                    grabBody.IsActive = true;
                    if (!grabBody.IsStatic)
                    {
                        grabBody.LinearVelocity *= 0.98f;
                        grabBody.AngularVelocity *= 0.98f;
                    }
                }
            }
            else
            {
                if (grabConstraint != null) World.RemoveConstraint(grabConstraint);
                grabBody = null;
                grabConstraint = null;
            }
            #endregion

            #region create random primitives

            if (PressedOnce(Keys.Space, Buttons.B))
            {
                SpawnRandomPrimitive(Conversion.ToJitterVector(Camera.Position), Conversion.ToJitterVector((Camera.Target - Camera.Position) * 75f));
            }
            #endregion

            #region switch through physic scenes
            if (PressedOnce(Keys.Add, Buttons.X))
            {
                DestroyCurrentScene();
                currentScene++;
                currentScene = currentScene % PhysicScenes.Count;
                PhysicScenes[currentScene].Build();
            }

            if (PressedOnce(Keys.Subtract, Buttons.Y))
            {
                DestroyCurrentScene();
                currentScene += PhysicScenes.Count - 1;
                currentScene = currentScene % PhysicScenes.Count;
                PhysicScenes[currentScene].Build();
            }
            #endregion

            UpdateDisplayText(gameTime);

            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (step > 1.0f / 100.0f)
            {
                step = 1.0f / 100.0f;
            }

            World.Step(step, multithread);



            /*JVector pos = ts.Position;
            JMatrix ori = ts.Orientation;

            JVector.Transform(ref pos, ref orientation, out pos);
            JVector.Add(ref pos, ref position, out pos);

            JMatrix.Multiply(ref ori, ref orientation, out ori);

            if (primitive != null)
            {
                primitive.AddWorldMatrix(scaleMatrix * Conversion.ToXNAMatrix(ori) * Matrix.CreateTranslation(Conversion.ToXNAVector(pos)));
            }*/




            for (int i = 0; i < instancedRigidBodies.Count; i++)
            {
                //JVector oriPos = instancedVector[i];
                JVector pos = instancedRigidBodies[i].Position;


                //instances[i].Position = new Vector4(pos.X, pos.Y, pos.Z, 1);


                instances[i].Position = Matrix.Identity;
                instances[i].Position.M11 = instancedRigidBodies[i].Orientation.M11;
                instances[i].Position.M12 = instancedRigidBodies[i].Orientation.M12;
                instances[i].Position.M13 = instancedRigidBodies[i].Orientation.M13;

                instances[i].Position.M21 = instancedRigidBodies[i].Orientation.M21;
                instances[i].Position.M22 = instancedRigidBodies[i].Orientation.M22;
                instances[i].Position.M23 = instancedRigidBodies[i].Orientation.M23;

                instances[i].Position.M31 = instancedRigidBodies[i].Orientation.M31;
                instances[i].Position.M32 = instancedRigidBodies[i].Orientation.M32;
                instances[i].Position.M33 = instancedRigidBodies[i].Orientation.M33;

                instances[i].Position *= Matrix.CreateScale(0.5f);



                instances[i].Position.M41 = pos.X;
                instances[i].Position.M42 = pos.Y;
                instances[i].Position.M43 = pos.Z;
                instances[i].Position.M44 = 1.0f;

                //instances[i].Position *= Matrix.CreateScale(1.0f);

                JMatrix orientation = instancedRigidBodies[i].Orientation;
                JQuaternion otherQuat = JQuaternion.CreateFromMatrix(orientation);


                Quaternion quat = new Quaternion(otherQuat.X, otherQuat.Y, otherQuat.Z, otherQuat.W);

                Vector4 direction_feet_forward;
                Vector4 direction_feet_right;
                Vector4 direction_feet_up;

                direction_feet_forward = sc_maths._getDirectionXNA(Vector3.Forward, quat);
                direction_feet_right = sc_maths._getDirectionXNA(Vector3.Right, quat);
                direction_feet_up = sc_maths._getDirectionXNA(Vector3.Up, quat);

                //instances[i].dirForward = direction_feet_forward;
                //instances[i].dirRight = direction_feet_right;
                //instances[i].dirUp = direction_feet_up;
 
                //instancesPos[i].dirForward = direction_feet_forward;
                //instancesPos[i].dirRight = direction_feet_right;
                //instancesPos[i].dirUp = direction_feet_up;



                //instances[i].Position = new Vector4(pos.X, pos.Y, pos.Z, 1);




                /*instances[i].Position0 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                instances[i].Position1 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                instances[i].Position2 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                instances[i].Position3 = new Vector4(pos.X, pos.Y, pos.Z, 1);*/

                // + new Vector4(oriPos.X, oriPos.Y, oriPos.Z,1)
                //instances[i].instance0 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                //instances[i].instance2 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                //instances[i].instance3 = new Vector4(pos.X, pos.Y, pos.Z, 1);
                //random position example
                //instances[i].World = new Vector4(-rnd.Next(100), rnd.Next(100), -rnd.Next(100), 1);
                //instances[i].AtlasCoordinate = new Vector2(rnd.Next(0, 2), rnd.Next(0, 2));

                //var body = new RigidBody(new BoxShape(1, 1, 1));
                //body.Position = new JVector(instances[i].World.X, instances[i].World.Y, instances[i].World.Z);
                //primitive.AddWorldMatrix(scaleMatrix * Conversion.ToXNAMatrix(ori) * Matrix.CreateTranslation(Conversion.ToXNAVector(pos)));
                /*if (i == 0)
                {
                    WorldMatter = (ScaleMatrix * Conversion.ToXNAMatrix(instancedRigidBodies[i].Orientation) * Matrix.CreateTranslation(Conversion.ToXNAVector(instancedRigidBodies[i].Position)));
                }*/
            }

            instanceBuffer = new VertexBuffer(GraphicsDevice, instanceVertexDeclaration, instanceCount, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);

            bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);

            //instanceBufferPos = new VertexBuffer(GraphicsDevice, instanceVertexDeclarationPos, instanceCount, BufferUsage.WriteOnly);
            //instanceBufferPos.SetData(instancesPos);
            //bindings[2] = new VertexBufferBinding(instanceBufferPos, 0, 1);




            /*for (int i = 0; i < instancedRigidBodies.Count; i++)
            {
                JVector pos = instancedRigidBodies[i].Position;
                instances[i].World = new Vector4(pos.X, pos.Y, pos.Z, 1);
                //random position example
                //instances[i].World = new Vector4(-rnd.Next(100), rnd.Next(100), -rnd.Next(100), 1);
                //instances[i].AtlasCoordinate = new Vector2(rnd.Next(0, 2), rnd.Next(0, 2));

                //var body = new RigidBody(new BoxShape(1, 1, 1));
                //body.Position = new JVector(instances[i].World.X, instances[i].World.Y, instances[i].World.Z);
                //primitive.AddWorldMatrix(scaleMatrix * Conversion.ToXNAMatrix(ori) * Matrix.CreateTranslation(Conversion.ToXNAVector(pos)));
            }

            instanceBuffer = new VertexBuffer(GraphicsDevice, instanceVertexDeclaration, instanceCount, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);*/





            gamePadPreviousState = padState;
            keyboardPreviousState = keyState;
            mousePreviousState = mouseState;

            base.Update(gameTime);

            rift.TrackHead();

            DoPlayerMotion();
        }
        Matrix ScaleMatrix = Matrix.Identity;


        private bool RaycastCallback(RigidBody body, JVector normal, float fraction)
        {
            if (body.IsStatic) return false;
            else return true;
        }

        RigidBody lastBody = null;

        #region Spawn Random Primitive
        private void SpawnRandomPrimitive(JVector position, JVector velocity)
        {
            RigidBody body = null;
            int rndn = rndn = random.Next(1); //7

            // less of the more advanced objects
            if (rndn == 5 || rndn == 6) rndn = random.Next(7);

            switch (rndn)
            {
                /*case 0:
                    body = new RigidBody(new ConeShape((float)random.Next(5, 50) / 20.0f, (float)random.Next(10, 20) / 20.0f));
                    break;
                case 1:
                    body = new RigidBody(new BoxShape((float)random.Next(10, 30) / 20.0f, (float)random.Next(10, 30) / 20.0f, (float)random.Next(10, 30) / 20.0f));
                    break;
                case 2:
                    body = new RigidBody(new SphereShape(0.4f));
                    break;
                case 3:
                    body = new RigidBody(new CylinderShape(1.0f, 0.5f));
                    break;
                case 4:
                    body = new RigidBody(new CapsuleShape(1.0f, 0.5f));
                    break;*/
                /*case 0:
                    Shape b1 = new BoxShape(new JVector(3, 1, 1));
                    Shape b2 = new BoxShape(new JVector(1, 1, 3));
                    Shape b3 = new CylinderShape(3.0f, 0.5f);

                    CompoundShape.TransformedShape t1 = new CompoundShape.TransformedShape(b1, JMatrix.Identity, JVector.Zero);
                    CompoundShape.TransformedShape t2 = new CompoundShape.TransformedShape(b2, JMatrix.Identity, JVector.Zero);
                    CompoundShape.TransformedShape t3 = new CompoundShape.TransformedShape(b3, JMatrix.Identity, new JVector(0, 0, 0));

                    CompoundShape ms = new CompoundShape(new CompoundShape.TransformedShape[3] { t1, t2, t3 });

                    body = new RigidBody(ms);
                    break;*/
                case 0:
                    ConvexHullObject obj2 = new ConvexHullObject(this);
                    Components.Add(obj2);
                    body = obj2.body;
                    body.Material.Restitution = 0.2f;
                    body.Material.StaticFriction = 0.8f;
                    convexObj.Add(obj2);
                    break;
            }

            World.AddBody(body);
            //body.IsParticle = true;
            // body.EnableSpeculativeContacts = true;
            body.Position = position;
            body.LinearVelocity = velocity;
            lastBody = body;
        }
        #endregion

        #region update the display text informations

        private float accUpdateTime = 0.0f;
        private void UpdateDisplayText(GameTime time) //,Matrix view, Matrix projection
        {
            accUpdateTime += (float)time.ElapsedGameTime.TotalSeconds;
            if (accUpdateTime < 0.1f) return;

            accUpdateTime = 0.0f;

            int contactCount = 0;
            foreach (Arbiter ar in World.ArbiterMap.Arbiters)
            {
                contactCount += ar.ContactList.Count;
            }

            Display.DisplayText[1] = World.CollisionSystem.ToString();

            Display.DisplayText[0] = "Current Scene: " + PhysicScenes[currentScene].ToString();
            //
            Display.DisplayText[2] = "Arbitercount: " + World.ArbiterMap.Arbiters.Count.ToString() + ";" + " Contactcount: " + contactCount.ToString();
            Display.DisplayText[3] = "Islandcount: " + World.Islands.Count.ToString();
            Display.DisplayText[4] = "Bodycount: " + World.RigidBodies.Count + " (" + activeBodies.ToString() + ")";
            Display.DisplayText[5] = (multithread) ? "Multithreaded" : "Single Threaded";


            int entries = (int)Jitter.World.DebugType.Num;
            double total = 0;

            for (int i = 0; i < entries; i++)
            {
                World.DebugType type = (World.DebugType)i;

                Display.DisplayText[8 + i] = type.ToString() + ": " +
                    ((double)World.DebugTimes[i]).ToString("0.00");

                total += World.DebugTimes[i];
            }

            Display.DisplayText[8 + entries] = "------------------------------";
            Display.DisplayText[9 + entries] = "Total Physics Time: " + total.ToString("0.00");

            //float tot = (float)(1000.0 / total);
            //string test = (int)tot + "";

            //Console.WriteLine(tot + "");

            Display.DisplayText[10 + entries] = "Physics Framerate: " + (int)(1000.0 / total) + " fps"; // + (1000.0 / total).ToString("0") 



            /*
#if(WINDOWS)
            Display.DisplayText[6] = "gen0: " + GC.CollectionCount(0).ToString() +
                "  gen1: " + GC.CollectionCount(1).ToString() +
                "  gen2: " + GC.CollectionCount(2).ToString();
#endif
            */


        }
        #endregion

        int startOVRDrawThread = 0;



        #region add draw matrices to the different primitives
        private void AddShapeToDrawList(Shape shape, JMatrix ori, JVector pos)
        {
            Primitives3D.GeometricPrimitive primitive = null;
            Matrix scaleMatrix = Matrix.Identity;

            if (shape is BoxShape)
            {
                primitive = primitives[(int)Primitives.box];
                scaleMatrix = Matrix.CreateScale(Conversion.ToXNAVector((shape as BoxShape).Size));
            }
            else if (shape is SphereShape)
            {
                primitive = primitives[(int)Primitives.sphere];
                scaleMatrix = Matrix.CreateScale((shape as SphereShape).Radius);
            }
            else if (shape is CylinderShape)
            {
                primitive = primitives[(int)Primitives.cylinder];
                CylinderShape cs = shape as CylinderShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
            }
            else if (shape is CapsuleShape)
            {
                primitive = primitives[(int)Primitives.capsule];
                CapsuleShape cs = shape as CapsuleShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius * 2, cs.Length, cs.Radius * 2);

            }
            else if (shape is ConeShape)
            {
                ConeShape cs = shape as ConeShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
                primitive = primitives[(int)Primitives.cone];
            }
            /*else if (shape is ConvexHullShape)
            {
                Console.WriteLine("convex");
                ConvexHullShape cs = shape as ConvexHullShape;
                //scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
                //scaleMatrix = Matrix.CreateScale(Conversion.ToXNAVector((shape as BoxShape).Size));
                primitive = primitives[(int)Primitives.convexHull];
 
            }*/

            if (primitive != null)
            {
                primitive.AddWorldMatrix(scaleMatrix * Conversion.ToXNAMatrix(ori) * Matrix.CreateTranslation(Conversion.ToXNAVector(pos)));
            }

        }

        private void AddBodyToDrawList(RigidBody rb)
        {
            if (rb.Tag is BodyTag && ((BodyTag)rb.Tag) == BodyTag.DontDrawMe) return; // || rb.Tag is BodyTag && ((BodyTag)rb.Tag) == BodyTag.InstancedCube

            bool isCompoundShape = (rb.Shape is CompoundShape);

            if (!isCompoundShape)
            {
                //GraphicsDevice.BlendState = BlendState.Opaque;
                //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                AddShapeToDrawList(rb.Shape, rb.Orientation, rb.Position);
            }
            else
            {
                //GraphicsDevice.BlendState = BlendState.Opaque;
                //GraphicsDevice.DepthStencilState = DepthStencilState.None;
                CompoundShape cShape = rb.Shape as CompoundShape;
                JMatrix orientation = rb.Orientation;
                JVector position = rb.Position;

                foreach (var ts in cShape.Shapes)
                {
                    JVector pos = ts.Position;
                    JMatrix ori = ts.Orientation;

                    JVector.Transform(ref pos, ref orientation, out pos);
                    JVector.Add(ref pos, ref position, out pos);

                    JMatrix.Multiply(ref ori, ref orientation, out ori);

                    AddShapeToDrawList(ts.Shape, ori, pos);
                }
            }
        }
        #endregion

        #region draw jitter debug data

        private void DrawJitterDebugInfo()
        {
            int cc = 0;

            foreach (Constraint constr in World.Constraints)
            {
                constr.DebugDraw(DebugDrawer);
            }

            foreach (RigidBody body in World.RigidBodies)
            {
                /*if (body.Shape is ConvexHullObject)
                {

                }
                else
                {
                    DebugDrawer.Color = rndColors[cc % rndColors.Length];
                    body.DebugDraw(DebugDrawer);
                }*/
                /*if (body.Tag is BodyTag && ((BodyTag)body.Tag) == BodyTag.CompoundOBJ)
                {
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.None;
                }
                else
                {
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }*/


                cc++;
            }
        }

        private void Walk(DynamicTree<SoftBody.Triangle> tree, int index)
        {
            DynamicTreeNode<SoftBody.Triangle> tn = tree.Nodes[index];
            if (tn.IsLeaf()) return;
            else
            {
                Walk(tree, tn.Child1);
                Walk(tree, tn.Child2);

                DebugDrawer.DrawAabb(tn.AABB.Min, tn.AABB.Max, Color.Red);
            }
        }

        private void DrawDynamicTree(SoftBody cloth)
        {
            Walk(cloth.DynamicTree, cloth.DynamicTree.Root);
        }

        private void DrawIslands()
        {
            JBBox box;

            foreach (CollisionIsland island in World.Islands)
            {
                box = JBBox.SmallBox;

                foreach (RigidBody body in island.Bodies)
                {
                    box = JBBox.CreateMerged(box, body.BoundingBox);
                }

                DebugDrawer.DrawAabb(box.Min, box.Max, island.IsActive() ? Color.Green : Color.Yellow);
            }
        }
        #endregion

        #region Draw Cloth

        private void DrawCloth()
        {
            foreach (SoftBody body in World.SoftBodies)
            {
                if (body.Tag is BodyTag && ((BodyTag)body.Tag) == BodyTag.DontDrawMe)
                {
                    return;
                }

                for (int i = 0; i < body.Triangles.Count; i++)
                {
                    //DebugDrawer.DrawTriangle(body.Triangles[i].VertexBody1.Position, body.Triangles[i].VertexBody2.Position, body.Triangles[i].VertexBody3.Position, new Color(0, 0.95f, 0, 0.5f));
                    DebugDrawer.DrawTriangle(body.Triangles[i].VertexBody3.Position, body.Triangles[i].VertexBody2.Position, body.Triangles[i].VertexBody1.Position, new Color(0, 0.95f, 0, 0.5f));
                }
                //DrawDynamicTree(body);
            }
        }
        #endregion



        Matrix WorldMatrix = Matrix.Identity;

        protected override void Draw(GameTime gameTime)
        {

            // draw scene for both eyes into respective rendertarget
            for (int eye = 0; eye < 2; eye++)
            {
                activeBodies = 0;

                GraphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                GraphicsDevice.Clear(new Color(130, 180, 255));


                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;


                Matrix view = rift.GetEyeViewMatrix(eye, playerMatrix);
                Matrix projection = rift.GetProjectionMatrix(eye);

                DrawCloth();
                //DrawIslands();

                //BasicEffect.View = view;// Camera.View;
                //BasicEffect.Projection = projection;// Camera.Projection;
                //BasicEffect.DiffuseColor = Color.LightGray.ToVector3();

                // Draw all shapes
                foreach (RigidBody body in World.RigidBodies)
                {
                   

                    if (body.Shape is ConvexHullShape)
                    {

                    }
                    else
                    {
                        if (body.IsActive)
                        {
                            activeBodies++;
                        }

                        if (body.Tag is int || body.IsParticle)
                        {
                            continue;
                        }
                        AddBodyToDrawList(body);
                    }
                }

                #region Debug Draw All Contacts
                //foreach (Arbiter a in World.ArbiterMap)
                //{
                //    foreach (Contact c in a.ContactList)
                //    {
                //        DebugDrawer.DrawLine(c.Position1 + 0.5f * JVector.Left, c.Position1 + 0.5f * JVector.Right, Color.Green);
                //        DebugDrawer.DrawLine(c.Position1 + 0.5f * JVector.Up, c.Position1 + 0.5f * JVector.Down, Color.Green);
                //        DebugDrawer.DrawLine(c.Position1 + 0.5f * JVector.Forward, c.Position1 + 0.5f * JVector.Backward, Color.Green);

                //        DebugDrawer.DrawLine(c.Position2 + 0.5f * JVector.Left, c.Position2 + 0.5f * JVector.Right, Color.Red);
                //        DebugDrawer.DrawLine(c.Position2 + 0.5f * JVector.Up, c.Position2 + 0.5f * JVector.Down, Color.Red);
                //        DebugDrawer.DrawLine(c.Position2 + 0.5f * JVector.Forward, c.Position2 + 0.5f * JVector.Backward, Color.Red);
                //    }
                //}

                #endregion


                if (primitives.Length > 0)
                {
                    foreach (Primitives3D.GeometricPrimitive prim in primitives)
                    {

                        prim.Draw(BasicEffect, view, projection);
                    }

                }




                if (convexObj.Count > 0)
                {
                    foreach (ConvexHullObject prim in convexObj)
                    {

                        prim.Draw(gameTime, view, projection);
                    }
                }

                //manyCubes.Draw(view, projection);

                //manyCubes.UnDraw(view, projection);

                DrawJitterDebugInfo();

                if (DebugDrawer != null)
                {
                    //Demo.GraphicsDevice.BlendState = BlendState.Opaque;
                    //Demo.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                    DebugDrawer.Draw(gameTime, view, projection);
                }

                if (effectLoaded == 1)
                {
                    //WorldMatrix.M41 = -5;
                    //WorldMatrix.M42 = 0;
                    //WorldMatrix.M43 = 0;


                    effect.CurrentTechnique = effect.Techniques["Instancing"];
                    //effect.Parameters["WVP"].SetValue(WorldMatter * view * projection);

                    effect.Parameters["World"].SetValue(WorldMatter);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    


                    effect.Parameters["cubeTexture"].SetValue(texture);

                    //effect.World = Matrix.Identity;
                    GraphicsDevice.Indices = indexBuffer;

                    effect.CurrentTechnique.Passes[0].Apply();

                    GraphicsDevice.SetVertexBuffers(bindings);
                    GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12, instanceCount);
                }

                PhysicScenes[currentScene].Draw(gameTime, view, projection, eye); //gameTime, view, projection, eye

            }

            // submit rendertargets to the Rift
            int result = rift.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }

        void DrawEyeViewIntoBackbuffer(int eye)
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var pp = GraphicsDevice.PresentationParameters;

            int height = pp.BackBufferHeight;
            int width = pp.BackBufferWidth;// Math.Min(pp.BackBufferWidth, (int)(height * rift.GetRenderTargetAspectRatio(eye)));
            int offset = (pp.BackBufferWidth - width) / 2;

            spriteBatch.Begin();
            spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), Color.White);
            spriteBatch.End();
        }
        void DoPlayerMotion()
        {
            //var mouse = Mouse.GetState();

            //var pp = GraphicsDevice.PresentationParameters;

            playerMatrix.Translation = new Vector3(
                                Camera.Position.X, // - pp.BackBufferWidth / 2
                            0,
                             Camera.Position.Z);

            /*playerMatrix.Translation = new Vector3(
                0.01f * (mouse.X), // - pp.BackBufferWidth / 2
                0.7f,
                0.01f * (mouse.Y )); //- pp.BackBufferHeight / 2*/
            playerMatrix.M42 = 20;
        }
    }
}
