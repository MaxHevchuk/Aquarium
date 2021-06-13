namespace Aquarium.Fish
{
    public class HerbivorousFish : FishAbstract
    {
        public HerbivorousFish() : this(null)
        {
        }

        public HerbivorousFish(Cell cell)
        {
            Cell = cell;
            Cell.OccupiedBy(this);
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