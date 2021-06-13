namespace Aquarium
{
    public class Stone
    {
        public Cell Cell { get; }

        public Stone(Cell cell)
        {
            Cell = cell;
            Cell.OccupiedBy(this);
        }
    }
}