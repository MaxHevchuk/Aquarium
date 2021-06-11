namespace Aquarium
{
    public class Stone
    {
        private Cell _cell;

        public Cell Cell => _cell;

        public Stone(Cell cell)
        {
            _cell = cell;
            _cell.OccupiedBy(this);
        }
    }
}