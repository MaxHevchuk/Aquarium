using Aquarium.enums;

namespace Aquarium.Fish
{
    public class PredatorFish : FishAbstract
    {
        public PredatorFish() : this(null)
        {
        }

        public PredatorFish(Cell cell)
        {
            Cell = cell;
            Cell.OccupiedBy(this);
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