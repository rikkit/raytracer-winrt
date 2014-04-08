using System.Threading.Tasks;

namespace IF.Ray.Core.Shapes
{
    public interface IShapeFactory
    {
        Task<IOccluder> GetShape<T>() where T : IOccluder, new();
    }
}