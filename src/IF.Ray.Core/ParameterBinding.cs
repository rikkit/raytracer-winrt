using Windows.UI.Core;
using IF.Common.Metro.Progress;

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
    }
}