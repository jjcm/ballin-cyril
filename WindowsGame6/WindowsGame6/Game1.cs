using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame6
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        Model myModel;

        float aspectRatio;
        SpriteFont lucidaConsole;

        GraphicsDeviceManager graphics;

        Vector2 fontPosition = new Vector2(0.0f, 0.0f);
        SpriteBatch spriteBatch;

        Vector3 modelPosition = new Vector3(0.0f, 0.0f, 0.0f);
        float modelRotation = 1.01f;

        Vector2 mouseDownInitialPosition = new Vector2(0.0f, 0.0f);
        Boolean mouseLeftCurrentlyDown = false;
        Boolean mouseRightCurrentlyDown = false;
        Boolean mouseMiddleCurrentlyDown = false;
        float previousDistance = 1000;
        float distance = 1000;

        float zWeight = 0;
        float yWeight = 10;
        float convergenceWeight = 0.0f;

        //ratio used for the C implementation
        float zRatio = 1;
        float yRatio = 1;

        Boolean targetHasChanged = false;

        String output = "";

        Boolean zSwitch = false;

        Vector3 cameraPosition = new Vector3(0.0f, 5000.0f, 10.0f);
        Vector3 previousCameraPosition = new Vector3(0.0f, 0.0f, 0.0f);

        //camera target variables
        Vector3 cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        float previousCameraTarget;

        Boolean touchToggle = false;
        MouseState m;

        Boolean toggleZoomBack = false;

        float zPlaneRadius = 0;
        float xPlaneRadius = 0;

        String zoomType = "A";
        float distanceToCameraTarget = 0.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.PreferredBackBufferWidth = 1000;
            this.graphics.PreferredBackBufferHeight = 1000;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            myModel = Content.Load<Model>("Models\\symbolic_Seattle");
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            lucidaConsole = Content.Load<SpriteFont>("Fonts/Lucida Console");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //modelRotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.01f);

            // TODO: Add your update logic here
            
            m = Mouse.GetState();

            KeyboardState k = Keyboard.GetState();

            if (m.MiddleButton == ButtonState.Pressed)
                toggleZoomBack = true;
            if (m.MiddleButton == ButtonState.Released)
                toggleZoomBack = false;
            
            /* pan */
            if (m.MiddleButton == ButtonState.Pressed)
            {
    
                //detect if this is the first instance of the mouse button being pressed
                if (!mouseMiddleCurrentlyDown)
                {
                    mouseDownInitialPosition.X = m.X;
                    mouseDownInitialPosition.Y = m.Y;
                    mouseMiddleCurrentlyDown = true;
                }
                modelPosition.X = modelPosition.X + (m.X - mouseDownInitialPosition.X) * 3;
                modelPosition.Z = modelPosition.Z + (m.Y - mouseDownInitialPosition.Y) * 3;
                mouseDownInitialPosition.X = m.X;
                mouseDownInitialPosition.Y = m.Y;

                /* pan 
   if (!mouseMiddleCurrentlyDown)
   {
       mouseDownInitialPosition.X = m.X;
       mouseDownInitialPosition.Y = m.Y;
       mouseMiddleCurrentlyDown = true;
       zPlaneRadius = (float)Math.Pow(Math.Pow(cameraPosition.X - cameraTarget.X, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2), 0.5);
   }


   System.Diagnostics.Debug.WriteLine(zPlaneRadius + "zplane");

   cameraPosition.X = ((float)Math.Cos((double)m.X / 100) * zPlaneRadius );
   cameraPosition.Z = ((float)Math.Sin((double)m.X / 100) * zPlaneRadius * -1) - (zPlaneRadius);
                  */

            }

            /* implementation #1 */
            if (m.LeftButton == ButtonState.Pressed)
            {
                //detect if this is the first instance of the mouse button being pressed
                if (!mouseLeftCurrentlyDown)
                {
                    mouseDownInitialPosition.X = m.X;
                    mouseDownInitialPosition.Y = m.Y;
                    previousCameraPosition = cameraPosition;
                    mouseLeftCurrentlyDown = true;
                }

                distance = previousDistance + m.Y - mouseDownInitialPosition.Y;
                output = "distance: " + distance
                    + "\n CamY: " + cameraPosition.Y
                    + "\n CamZ: " + cameraPosition.Z
                    + "\n TargetZ: " + cameraTarget.Z
                    + "\n yWeight: " + yWeight
                    + "\n zWeight: " + zWeight
                    + "\n distanceToTarget: " + distanceToCameraTarget
                    + "\n |-----------|"
                    + "\n |   reset   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |   angle   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      A     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      B     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      C     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |     pan   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |   orbit   |"
                    + "\n |-----------|"
                    ;
                if (mouseDownInitialPosition.X <= 100 && mouseDownInitialPosition.Y <= 520 && mouseDownInitialPosition.Y >= 140)
                {
                    if (mouseDownInitialPosition.Y <= 190)
                    {
                        System.Diagnostics.Debug.WriteLine("reset");
                        resetEnvironment();
                    }
                    else if (mouseDownInitialPosition.Y <= 245)
                    {
                        System.Diagnostics.Debug.WriteLine("angle");
                        zoomType = "angle";
                    }
                    else if (mouseDownInitialPosition.Y <= 300)
                    {
                        System.Diagnostics.Debug.WriteLine("A");
                        zoomType = "A";
                    }
                    else if (mouseDownInitialPosition.Y <= 355)
                    {
                        System.Diagnostics.Debug.WriteLine("B");
                        zoomType = "B";
                    }
                    else if (mouseDownInitialPosition.Y <= 410)
                    {
                        System.Diagnostics.Debug.WriteLine("C");
                        zoomType = "C";
                        distanceToCameraTarget = (cameraPosition.Y) / (cameraPosition.Z - cameraTarget.Z);
                        System.Diagnostics.Debug.WriteLine("zoom angle: " + distanceToCameraTarget);
                    }
                    else if (mouseDownInitialPosition.Y <= 465)
                    {
                        System.Diagnostics.Debug.WriteLine("pan");
                        zoomType = "pan";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("orbit");
                        zoomType = "orbit";
                    }

                }
                else
                {

                    if (zoomType == "A")
                    {

                        /*
                         * Z LOGIC
                         */
                        if (!targetHasChanged)
                        {
                            if (distance < 400)
                            {
                                zWeight = 1 - yWeight;
                                cameraPosition.Z = previousCameraPosition.Z + (distance - previousDistance) * zWeight;
                            }
                            if (distance < 700 && distance >= 400)
                            {
                                zWeight = 3 * distance / 700;
                                cameraPosition.Z = previousCameraPosition.Z - (distance - previousDistance) * zWeight;
                            }

                        }
                        if (distance >= 700)
                        {
                            if (cameraTarget.Z < cameraPosition.Z - 1)
                            {
                                convergenceWeight = (distance - 700) / 1000;
                                if (targetHasChanged) cameraTarget.Z = previousCameraTarget + (Math.Abs(distance - previousDistance) * convergenceWeight);
                                cameraPosition.Z = previousCameraPosition.Z - (Math.Abs(distance - previousDistance) * convergenceWeight);
                            }
                            if (cameraTarget.Z > cameraPosition.Z)
                            {
                                cameraPosition.Z = cameraTarget.Z + 1;

                            }
                            if (cameraPosition.Z - cameraTarget.Z <= 1)
                                targetHasChanged = false;
                            //else cameraTarget.Z = cameraPosition.Z - 1;

                            //cameraPosition.Z = 1;
                        }


                        /*
                         * Y LOGIC
                         */
                        if (distance < 1000)
                        {
                            yWeight = cameraPosition.Y / 500;
                            cameraPosition.Y = previousCameraPosition.Y + (distance - previousDistance) * yWeight;
                        }
                        else if (distance >= 1000)
                        {
                            yWeight = 10;
                            cameraPosition.Y = previousCameraPosition.Y + (distance - previousDistance) * yWeight;
                        }

                        //if we're sufficiently high enough, reset the distance in case it's drifted.
                        if (cameraPosition.Y > 5000)
                        {
                            distance = cameraPosition.Y / 5;
                            previousDistance = distance;
                        }


                        //cleanup
                        previousDistance = distance;
                        previousCameraPosition = cameraPosition;
                        mouseDownInitialPosition.Y = m.Y;
                        mouseDownInitialPosition.X = m.X;
                        previousCameraTarget = cameraTarget.Z;
                    }

                    if (zoomType == "B")
                    {
                        distanceToCameraTarget = (float)Math.Pow((Math.Pow(cameraPosition.Y, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2)), 0.5);
                        if (distanceToCameraTarget < 2000)
                        {
                            distance = previousDistance + m.Y - mouseDownInitialPosition.Y;
                            cameraPosition.Y = previousCameraPosition.Y + ((distance - previousDistance) * yRatio * yWeight);
                            //adjust the Z position to rotate along a circle
                            //cameraPosition.Z = previousCameraPosition.Z - (float)Math.Pow(100 - Math.Pow(cameraPosition.Y, 2), 0.5) * zRatio * yWeight;
                            cameraPosition.Z = previousCameraPosition.Z + (float)Math.Sin(distanceToCameraTarget / 640) * 300;
                            previousCameraPosition.Y = cameraPosition.Y;
                            mouseDownInitialPosition.Y = m.Y;
                            mouseDownInitialPosition.X = m.X;
                            previousDistance = distance;
                            yWeight = cameraPosition.Y / 500;
                        }
                        else
                        {
                            distance = previousDistance + m.Y - mouseDownInitialPosition.Y;
                            cameraPosition.Y = previousCameraPosition.Y + ((distance - previousDistance) * yRatio * yWeight);
                            cameraPosition.Z = previousCameraPosition.Z - ((distance - previousDistance) * zRatio * yWeight);
                            previousCameraPosition = cameraPosition;
                            mouseDownInitialPosition.Y = m.Y;
                            mouseDownInitialPosition.X = m.X;
                            previousDistance = distance;
                            yWeight = cameraPosition.Y / 500;
                        }
                    }

                    if (zoomType == "C")
                    {
                        distance = previousDistance + m.Y - mouseDownInitialPosition.Y;
                        cameraPosition.Y = previousCameraPosition.Y + ((distance - previousDistance) * yRatio * yWeight);
                        cameraPosition.Z = previousCameraPosition.Z - ((distance - previousDistance) * zRatio * yWeight);
                        previousCameraPosition = cameraPosition;
                        mouseDownInitialPosition.Y = m.Y;
                        mouseDownInitialPosition.X = m.X;
                        previousDistance = distance;
                        yWeight = cameraPosition.Y / 500;
                    }
                    if (zoomType == "angle")
                    {
                        targetHasChanged = true;
                        cameraTarget.Z = previousCameraTarget - (m.Y - mouseDownInitialPosition.Y);
                        if (distance > 0) distance = previousDistance - (m.Y - mouseDownInitialPosition.Y);
                        previousCameraTarget = cameraTarget.Z;
                        mouseDownInitialPosition.Y = m.Y;

                        distanceToCameraTarget = (float)Math.Pow((Math.Pow(cameraPosition.Y, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2)), 0.5);
                        yRatio = cameraPosition.Y / distanceToCameraTarget;
                        zRatio = (cameraTarget.Z - cameraPosition.Z) / distanceToCameraTarget;
                    }
                    if (zoomType == "pan")
                    {
                        modelPosition.X = modelPosition.X + (m.X - mouseDownInitialPosition.X) * 3;
                        modelPosition.Z = modelPosition.Z + (m.Y - mouseDownInitialPosition.Y) * 3;
                        mouseDownInitialPosition.X = m.X;
                        mouseDownInitialPosition.Y = m.Y;
                    }
                    if (zoomType == "orbit")
                    {


                        System.Diagnostics.Debug.WriteLine(zPlaneRadius + "zplane");

                        cameraPosition.X = ((float)Math.Cos((double)m.X / 100) * zPlaneRadius);
                        cameraPosition.Z = ((float)Math.Sin((double)m.X / 100) * zPlaneRadius * -1) - (zPlaneRadius);
                    }
                }

            }
    

            if (m.RightButton == ButtonState.Pressed)
            {
                //detect if this is the first instance of the mouse button being pressed
                if (!mouseRightCurrentlyDown)
                {
                    mouseDownInitialPosition.X = m.X;
                    mouseDownInitialPosition.Y = m.Y;
                    mouseRightCurrentlyDown = true;
                    targetHasChanged = true;
                }
                
                cameraTarget.Z = previousCameraTarget - (m.Y - mouseDownInitialPosition.Y);
                if (distance > 0) distance = previousDistance - (m.Y - mouseDownInitialPosition.Y);
                previousCameraTarget = cameraTarget.Z;
                mouseDownInitialPosition.Y = m.Y;

                distanceToCameraTarget = (float)Math.Pow((Math.Pow(cameraPosition.Y, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2)), 0.5);
                yRatio = cameraPosition.Y / distanceToCameraTarget;
                zRatio = (cameraTarget.Z - cameraPosition.Z) / distanceToCameraTarget;

                output = "distance: " + distance
                    + "\n CamY: " + cameraPosition.Y
                    + "\n CamZ: " + cameraPosition.Z
                    + "\n TargetZ: " + cameraTarget.Z
                    + "\n yWeight: " + yWeight
                    + "\n zWeight: " + zWeight
                    + "\n convergenceWeight: " + convergenceWeight
                    + "\n |-----------|"
                    + "\n |   reset   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |   angle   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      A     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      B     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |      C     |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |     pan   |"
                    + "\n |-----------|"
                    + "\n |-----------|"
                    + "\n |   orbit   |"
                    + "\n |-----------|"
                    ;
            }


            /*
             * Cleanup once the mouse buttons are released
             */
            if (m.RightButton == ButtonState.Released && mouseRightCurrentlyDown)
            {
                System.Diagnostics.Debug.WriteLine("Right Button Cleanup");
                mouseRightCurrentlyDown = false;
                previousCameraTarget = cameraTarget.Z;
                previousDistance = distance;
                System.Diagnostics.Debug.WriteLine(previousCameraTarget);
                zPlaneRadius = (float)Math.Pow(Math.Pow(cameraPosition.X - cameraTarget.X, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2), 0.5);
            }

            if (m.LeftButton == ButtonState.Released && mouseLeftCurrentlyDown)
            {
                System.Diagnostics.Debug.WriteLine("Left Button Cleanup");
                previousDistance = distance;
                mouseLeftCurrentlyDown = false;
                zPlaneRadius = (float)Math.Pow(Math.Pow(cameraPosition.X - cameraTarget.X, 2) + Math.Pow(cameraPosition.Z - cameraTarget.Z, 2), 0.5);
            }

            if (m.MiddleButton == ButtonState.Released && mouseMiddleCurrentlyDown)
            {
                mouseMiddleCurrentlyDown = false;
            }

            base.Update(gameTime);
        }

        private void resetEnvironment()
        {
            fontPosition = new Vector2(0.0f, 0.0f);

            modelPosition = new Vector3(0.0f, 0.0f, 0.0f);
            modelRotation = 1.01f;

            mouseDownInitialPosition = new Vector2(0.0f, 0.0f);
            mouseLeftCurrentlyDown = false;
            mouseRightCurrentlyDown = false;
            previousDistance = 1000;
            distance = 1000;

            zWeight = 0;
            yWeight = 10;
            convergenceWeight = 0.0f;

            targetHasChanged = false;

            zSwitch = false;

            cameraPosition = new Vector3(0.0f, 5000.0f, 10.0f);
            previousCameraPosition = new Vector3(0.0f, 0.0f, 0.0f);
            cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);

            touchToggle = false;

            toggleZoomBack = false;

        }

        private float computeDistanceOnCurveFromAngle()
        {
            return 0;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateRotationY(modelRotation)
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                }
                mesh.Draw();
            }
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            spriteBatch.DrawString(lucidaConsole, output, fontPosition, Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
