namespace Aquarium
{
    public class Cell
    {
        private object _obj;
        public int X { get; }
        public int Y { get; }

        public Cell(int y, int x)
        {
            X = x;
            Y = y;
        }

        public bool IsAvailable() => _obj == null;

        public void OccupiedBy<T>(T obj)
        {
            if (_obj == null) _obj = obj;
        }

        public void Clear() => _obj = null;
    }
}