using System;

namespace Aquarium.Fish
{
    public class HerbivorousFish : FishAbstract
    {
        public HerbivorousFish(Cell cell)
        {
            _cell = cell;
            _cell.OccupiedBy(this);
            _energyCurrent = new Random().Next(50, 101);
        }

        public override void Eat<T>(T obj)
        {
            if (!typeof(T).IsSubclassOf(typeof(Herb))
                && typeof(T) != typeof(Herb)) return;

            if (!(obj is Herb herb)) return;

            var energy = herb.GrowthCurrent;
            herb.DecreaseByEating(this);
            IncreaseEnergyByEating(energy);
        }
    }
}