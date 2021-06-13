using Aquarium.enums;
using Aquarium.Fish;

namespace Aquarium
{
    public class Herb : ICell
    {
        private readonly Cell _cell;

        private DeathType _deathType;


        public double GrowthCurrent { get; set; }

        public double GrowthIncreaseOnIteration { get; set; }

        public double GrowthMax { get; set; }

        public Herb(Cell cell)
        {
            _cell = cell;
            _cell.OccupiedBy(this);
        }

        public void Grow()
        {
            GrowthCurrent += GrowthIncreaseOnIteration;
            if (GrowthCurrent >= GrowthMax) GrowthCurrent = GrowthMax;
        }

        public void DecreaseByEating(HerbivorousFish fish)
        {
            var neededEnergy = fish.EnergyMax - fish.EnergyCurrent;
            if (neededEnergy >= GrowthCurrent)
                _deathType = DeathType.ByHerbivorous;
            else
                GrowthCurrent -= neededEnergy;
        }

        public Cell GetCell() => _cell;
        public DeathType GetDeathType() => _deathType;
    }
}