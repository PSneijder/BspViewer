using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using BspViewer.Extensions;
using System.IO;

namespace BspViewer
{
    sealed class Viewer
        : GameWindow
    {
        private Renderer _renderer;
        private Camera _camera;

        public Viewer()
            : base(640, 480, GraphicsMode.Default, AppDomain.CurrentDomain.FriendlyName)
        {
            VSync = VSyncMode.On;

            CursorVisible = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //var fileName = @"C:\Sierra\Half-Life\valve\maps\hldemo1.bsp";
            //var wadFileNames = new string[] { @"C:\Sierra\Half-Life\valve\halflife.wad" };

            var wadPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Assets\Wads\", "SampleWad.wad"));
            var fileName = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Assets\Maps\", "SampleMap.bsp"));
            var wadFileNames = new string[] { Path.Combine(wadPath) };

            var loader = new BspLoader(fileName);
            var map = loader.Load();

            var wadLoader = new WadLoader(map, fileName, wadFileNames);
            var textures = wadLoader.Load();
            
            _renderer = new Renderer(map, textures);
            _camera = new Camera(this);

            float[] origin = map.Entities["info_player_start"]["origin"].AsSingleArray();
            _camera.SetPosition(origin[0], origin[1], origin[2]);

            float[] angles = map.Entities["info_player_start"]["angles"].AsSingleArray();
            _camera.SetViewAngles(angles[0], angles[1]);

            InitGL();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)((Math.PI * 60.0f) / 180.0f), Width / Height, 8.0f, 4000.0f);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _camera.Update(e.Time);

            if (Keyboard[Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();

            _camera.Look();

            _renderer.Render(_camera.Position);

            SwapBuffers();
        }

        private void InitGL()
        {
            GL.ShadeModel(ShadingModel.Smooth);                // Enable Smooth Shading
            GL.ClearColor(Color4.CornflowerBlue);              // Blue Background
            GL.ClearDepth(1.0f);                               // Depth Buffer Setup
            GL.DepthFunc(DepthFunction.Lequal);                // The Type Of Depth Testing To Do

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Multisample);
        }
    }
}