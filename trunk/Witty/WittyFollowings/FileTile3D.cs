using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;

namespace WittyFollowings
{
    public class FlipTile3D : FrameworkElement
    {
        public FlipTile3D()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new ThreadStart(setup3D));
            this.Unloaded += new RoutedEventHandler(FlipTile3D_Unloaded);
        }

        private void FlipTile3D_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= tick;
        }

        #region render/layout overrides

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(this.RenderSize));
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_viewport != null)
            {
                _viewport.Measure(availableSize);
            }
            return base.MeasureOverride(availableSize);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_viewport != null)
            {
                _viewport.Arrange(new Rect(finalSize));
            }
            return base.ArrangeOverride(finalSize);
        }
        protected override Visual GetVisualChild(int index)
        {
            if (_viewport != null && index == 0)
            {
                return _viewport;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return (_viewport == null) ? 0 : 1;
            }
        }

        #endregion

        #region mouse overrides
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            _lastMouse = e.GetPosition(this);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _lastMouse = new Point(double.NaN, double.NaN);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _isFlipped = !_isFlipped;
            if (_isFlipped)
            {
                setClickedItem(e.GetPosition(this));
            }
        }


        #endregion

        #region private methods

        private void setClickedItem(Point point)
        {
            //move point to center
            point -= (new Vector(this.ActualWidth / 2, this.ActualHeight / 2));

            //flip it
            point.Y *= -1;

            //scale it to 1x1 dimentions
            double scale = Math.Min(this.ActualHeight, this.ActualWidth) / 2;
            point = (Point)(((Vector)point) / scale);

            //set it up so that bottomLeft = 0,0 & topRight = 1,1 (positive Y is up)
            point = (Point)(((Vector)point + new Vector(1, 1)) * .5);

            //scale up so that the point coordinates align w/ the x/y scale
            point.Y *= YCount;
            point.X *= XCount;

            //now we have the indicies of the x & y of the tile clicked
            int yIndex = (int)Math.Floor(point.Y);
            int xIndex = (int)Math.Floor(point.X);

            int tileIndex = yIndex * XCount + xIndex;
            Debug.WriteLine(tileIndex);

            _backMaterial.Brush = _tiles[tileIndex].DiffuseMaterial.Brush;
        }

        private void setup3D()
        {
            _viewport = new Viewport3D();

            //perf improvement. Clipping in 3D is expensive. Avoid if you can!
            _viewport.ClipToBounds = false;

            PerspectiveCamera camera = new PerspectiveCamera(
                new Point3D(0, 0, 3.73), //position
                new Vector3D(0, 0, -1), //lookDirection
                new Vector3D(0, 1, 0), //upDirection
                30 //FOV
                );

            _viewport.Camera = camera;

            Model3DGroup everything = new Model3DGroup();

            Model3DGroup lights = new Model3DGroup();
            DirectionalLight whiteLight = new DirectionalLight(Colors.White, new Vector3D(0, 0, -1));
            lights.Children.Add(whiteLight);

            everything.Children.Add(lights);

            ModelVisual3D model = new ModelVisual3D();

            double tileSizeX = 2.0 / XCount;
            double startX = -((double)XCount) / 2 * tileSizeX + tileSizeX / 2;
            double startY = -((double)YCount) / 2 * tileSizeX + tileSizeX / 2;

            int index;

            Size tileTextureSize = new Size(1.0 / XCount, 1.0 / YCount);

            //so, tiles are numbers, left-to-right (ascending x), bottom-to-top (ascending y)
            for (int y = 0; y < YCount; y++)
            {
                for (int x = 0; x < XCount; x++)
                {
                    index = y * XCount + x;

                    Rect backTextureCoordinates = new Rect(x * tileTextureSize.Width,
                        // this will give you a headache. Exists since we are going from bottom
                        //bottomLeft of 3D space (negative Y is down), but texture coor are negative Y is up
                        1 - y * tileTextureSize.Height - tileTextureSize.Height,
                        tileTextureSize.Width, tileTextureSize.Height);

                    _tiles[index] = new TileData();
                    _tiles[index].Setup3DItem(everything, getMaterial(index), new Size(tileSizeX, tileSizeX),
                        new Point(startX + x * tileSizeX, startY + y * tileSizeX), _backMaterial, backTextureCoordinates);
                }
            }

            model.Content = everything;

            _viewport.Children.Add(model);

            this.AddVisualChild(_viewport);
            this.InvalidateArrange();

            //start the per-frame tick for the physics
            CompositionTarget.Rendering += tick;
        }


        private void tick(object sender, EventArgs e)
        {
            Vector mouseFixed = fixMouse(_lastMouse, this.RenderSize);
            for (int i = 0; i < _tiles.Length; i++)
            {
                //TODO: unwire Render event if nothing happens
                _tiles[i].TickData(mouseFixed, _isFlipped);
            }
        }

        private static Vector fixMouse(Point mouse, Size size)
        {

            Debug.Assert(size.Width >= 0 && size.Height >= 0);
            double scale = Math.Max(size.Width, size.Height) / 2;

            //translate y going down to y going up stuff...
            mouse.Y = -mouse.Y + size.Height;

            mouse.Y -= size.Height / 2;
            mouse.X -= size.Width / 2;

            Vector v = new Vector(mouse.X, mouse.Y);

            v /= scale;

            return v;
        }


        #endregion

        #region private fields

        private DiffuseMaterial getMaterial(int index)
        {
            return _materials[index % _materials.Count];
        }
        private readonly IList<DiffuseMaterial> _materials = GetSamplePictures();

        private static IList<DiffuseMaterial> GetSamplePictures()
        {
            IList<DiffuseMaterial> materials;

            string[] files = Helpers.GetPicturePaths();
            if (files.Length > 0)
            {
                materials = new List<DiffuseMaterial>();

                foreach (string file in files)
                {
                    Uri uri = new Uri(file);

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = uri;
                    bitmapImage.DecodePixelWidth = 320;
                    bitmapImage.DecodePixelHeight = 240;
                    bitmapImage.EndInit();

                    //bitmapImage.Freeze();

                    ImageBrush imageBrush = new ImageBrush(bitmapImage);
                    imageBrush.Stretch = Stretch.UniformToFill;
                    imageBrush.ViewportUnits = BrushMappingMode.Absolute;
                    //imageBrush.Freeze();

                    DiffuseMaterial diffuseMaterial = new DiffuseMaterial(imageBrush);
                    materials.Add(diffuseMaterial);
                }
            }
            else
            {
                Brush[] brushes = new Brush[] { Brushes.LightBlue, Brushes.Pink, Brushes.LightGray, Brushes.Yellow, Brushes.Orange, Brushes.LightGreen };
                DiffuseMaterial[] materialsArray = new DiffuseMaterial[brushes.Length];
                for (int i = 0; i < materialsArray.Length; i++)
                {
                    materialsArray[i] = new DiffuseMaterial(brushes[i]);
                }
                materials = materialsArray;
            }

            return materials;
        }

        private DiffuseMaterial _backMaterial = new DiffuseMaterial();

        private Viewport3D _viewport = null;

        private readonly Pen _blackPen = new Pen(Brushes.Black, 2);

        private Point _lastMouse = new Point(double.NaN, double.NaN);

        private readonly TileData[] _tiles = new TileData[XCount * YCount];

        private bool _isFlipped = false;

        private const int XCount = 9, YCount = 9;

        #endregion
    }
}