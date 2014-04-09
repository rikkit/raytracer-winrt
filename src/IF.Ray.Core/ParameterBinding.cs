using IF.Common.Metro.Progress;
using Windows.UI.Core;

namespace IF.Ray.Core
{
    /// <summary>
    /// Facade class for binding to render parameters
    /// </summary>
    public class ParameterBinding : PropertyChangingBase
    {
        private float _rotationX;
        private float _rotationY;
        private float _rotationZ;
        private float _zoom;

        public float RotationX
        {
            get { return _rotationX; }
            set
            {
                if (_rotationX.Equals(value))
                {
                    return;
                }

                _rotationX = value;
                RaisePropertyChanged();
            }
        }

        public float RotationY
        {
            get { return _rotationY; }
            set
            {
                if (_rotationY.Equals(value))
                {
                    return;
                }

                _rotationY = value;
                RaisePropertyChanged();
            }
        }

        public float RotationZ
        {
            get { return _rotationZ; }
            set
            {
                if (_rotationZ.Equals(value))
                {
                    return;
                }

                _rotationZ = value;
                RaisePropertyChanged();
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom.Equals(value))
                {
                    return;
                }

                _zoom = value;
                RaisePropertyChanged();
            }
        }
        
        public ParameterBinding(CoreDispatcher dispatcher) : base(dispatcher)
        {
            // set the defaults for these parameters
            _rotationX = 0;
            _rotationY = 0;
            _rotationZ = 0;
            _zoom = 1;
        }

        /// <summary>
        /// Linearly nterpolates between this binding and the provided one
        /// </summary>
        /// <param name="end">Binding to interpolate to</param>
        /// <param name="frame">current frame</param>
        /// <param name="totalFrames">final frame</param>
        /// <returns></returns>
        public ParameterBinding Interpolate(ParameterBinding end, int frame, int totalFrames)
        {
            float ratio = (float)frame/totalFrames;

            var interpolated = new ParameterBinding(end.Dispatcher)
            {
                RotationX = Lerp(RotationX, end.RotationX, ratio),
                RotationY = Lerp(RotationY, end.RotationY, ratio),
                RotationZ = Lerp(RotationZ, end.RotationZ, ratio),
                Zoom = Lerp(Zoom, end.Zoom, ratio)
            };

            return interpolated;
        }

        private static float Lerp(float first, float second, float ratio)
        {
            var diff = second - first;
            return first + diff*ratio;
        }
    }
}