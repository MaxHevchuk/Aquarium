using System;
using Aquarium.enums;

namespace Aquarium.Fish
{
    public class PredatorFish : FishAbstract
    {
        public PredatorFish(Cell cell)
        {
            _cell = cell;
            _cell.OccupiedBy(this);
            _energyCurrent = new Random().Next(50, 101);
        }

        public override void Eat<T>(T obj)
        {
            if (!typeof(T).IsSubclassOf(typeof(HerbivorousFish))
                && typeof(T) != typeof(HerbivorousFish)) return;

            if (!(obj is HerbivorousFish fish)) return;
            
            IncreaseEnergyByEating(fish.EnergyCurrent);
            fish.DeathType = DeathType.ByPredator;
        }
    }
}