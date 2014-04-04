using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Progress;

namespace IF.Ray.Core
{
    public interface IAsyncRenderer
    {
        Task InitialiseSceneAsync();
        Task<WriteableBitmap> RenderAsync(int width, int height, ProgressToken token);
    }
}