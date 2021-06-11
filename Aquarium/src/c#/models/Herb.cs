using System;
using Aquarium.enums;
using Aquarium.Fish;

namespace Aquarium
{
    public class Herb : ICell
    {
        private readonly Cell _cell;

        private double _growthCurrent;
        private double _growthMax;
        private double _growthIncreaseOnIteration;
        private DeathType _deathType;


        public double GrowthCurrent => _growthCurrent;

        public double GrowthIncreaseOnIteration
        {
            set
            {
                if (value > 0) _growthIncreaseOnIteration = value;
            }
        }

        public double GrowthMax
        {
            set
            {
                if (value > 0)
                {
                    _growthMax = value;
                }
            }
        }

        public Herb(Cell cell)
        {
            _cell = cell;
            _cell.OccupiedBy(this);
            _growthCurrent = new Random().Next(50, 101);
        }

        public void Grow()
        {
            _growthCurrent += _growthIncreaseOnIteration;
            if (_growthCurrent >= _growthMax) _growthCurrent = _growthMax;
        }

        public void DecreaseByEating(FishAbstract fish)
        {
            var neededEnergy = fish.EnergyMax - fish.EnergyCurrent;
            if (neededEnergy >= _growthCurrent)
                _deathType = DeathType.ByHerbivorous;
            else
                _growthCurrent -= neededEnergy;
        }

        public Cell GetCell() => _cell;
        public DeathType GetDeathType() => _deathType;
    }
}