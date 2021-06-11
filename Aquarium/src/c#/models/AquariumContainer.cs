using System.Collections.Generic;
using Aquarium.Fish;

namespace Aquarium
{
    public class AquariumContainer
    {
        private static AquariumContainer _instance;

        private static int _height;
        private static int _width;

        private int _numberOfStones;
        private int _numberOfHerbs;
        private int _numberOfHerbivorousFish;
        private int _numberOfPredatorFish;

        private List<Stone> _stones;
        private List<Herb> _herbs;
        private List<HerbivorousFish> _herbivorousFish;
        private List<PredatorFish> _predatorFish;
        private readonly Cell[][] _cells;

        public List<Stone> Stones => _stones;
        public List<Herb> Herbs => _herbs;
        public List<HerbivorousFish> HerbivorousFish => _herbivorousFish;
        public List<PredatorFish> PredatorFish => _predatorFish;
        public Cell[][] Cells => _cells;

        private AquariumContainer(int height, int width)
        {
            _height = height;
            _width = width;
            _cells = InitializeCells(height, width);
        }

        public static AquariumContainer GetInstance()
        {
            return _instance;
        }

        public static AquariumContainer GetInstance(int height, int width)
        {
            if (_instance != null &&
                _height == height && _width == width) return _instance;
            _instance = height > 0 && width > 0
                ? new AquariumContainer(height, width)
                : new AquariumContainer(20, 20);
            return _instance;
        }

        private static Cell[][] InitializeCells(int height, int width)
        {
            var cells = new Cell[height][];
            for (var i = 0; i < height; i++)
            {
                cells[i] = new Cell[width];
                for (var j = 0; j < width; j++)
                {
                    cells[i][j] = new Cell(i, j);
                }
            }

            return cells;
        }

        public void Add<T>(T obj)
        {
            var objType = obj.GetType();

            if (objType == typeof(Herb))
            {
                if (_herbs == null)
                    _herbs = new List<Herb>(5);
                _herbs.Add(obj as Herb);
                _numberOfHerbs++;
            }
            else if (objType == typeof(HerbivorousFish))
            {
                if (_herbivorousFish == null)
                    _herbivorousFish = new List<HerbivorousFish>(5);
                _herbivorousFish.Add(obj as HerbivorousFish);
                _numberOfHerbivorousFish++;
            }
            else if (objType == typeof(PredatorFish))
            {
                if (_predatorFish == null)
                    _predatorFish = new List<PredatorFish>(5);
                _predatorFish.Add(obj as PredatorFish);
                _numberOfPredatorFish++;
            }
            else if (objType == typeof(Stone))
            {
                if (_stones == null)
                    _stones = new List<Stone>(5);

                _stones.Add(obj as Stone);
                _numberOfStones++;
            }
        }

        public bool Remove<T>(T obj)
        {
            var objType = obj.GetType();

            if (objType == typeof(Herb) && _herbs != null)
            {
                if (_herbs.Remove(obj as Herb))
                    _numberOfHerbs--;
            }
            else if (objType == typeof(HerbivorousFish) && _herbivorousFish != null)
            {
                if (_herbivorousFish.Remove(obj as HerbivorousFish))
                    _numberOfHerbivorousFish--;
            }
            else if (objType == typeof(PredatorFish) && _predatorFish != null)
            {
                if (_predatorFish.Remove(obj as PredatorFish))
                    _numberOfPredatorFish--;
            }
            else if (objType == typeof(Stone) && _stones != null)
            {
                if (_stones.Remove(obj as Stone))
                    _numberOfStones--;
            }
            else
                return false;

            return true;
        }

        public void Clear()
        {
            Herbs?.Clear();
            Stones?.Clear();
            HerbivorousFish?.Clear();
            PredatorFish?.Clear();
            foreach (var cell in _cells)
            {
                foreach (var c in cell)
                {
                    c.Clear();
                }
            }
        }
    }
}