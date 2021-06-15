using Aquarium.enums;
namespace Aquarium.interfaces
{
    public interface ICell
    {
        Cell GetCell();
        DeathType GetDeathType();
    }
}