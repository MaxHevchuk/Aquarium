namespace Aquarium
{
    public class Cell
    {
        private readonly int _x;
        private readonly int _y;
        private object _obj;

        public int X => _x;
        public int Y => _y;

        public Cell(int y, int x)
        {
            _x = x;
            _y = y;
        }

        public bool IsAvailable()
        {
            return _obj == null;
        }

        public void OccupiedBy<T>(T obj)
        {
            if (_obj == null)
                _obj = obj;
        }

        public void Clear()
        {
            _obj = null;
        }
    }
}