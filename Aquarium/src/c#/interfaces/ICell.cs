using Aquarium.enums;

namespace Aquarium
{
    public interface ICell
    {
        Cell GetCell();
        DeathType GetDeathType();
    }
}