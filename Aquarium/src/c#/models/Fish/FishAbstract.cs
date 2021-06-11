using System;
using System.Collections.Generic;
using System.Linq;
using Aquarium.enums;

namespace Aquarium.Fish
{
    public abstract class FishAbstract : ICell
    {
        protected Cell _cell;

        private int _ageCurrent;
        private int _ageMax;
        private int _ageBeginMature;
        private int _ageEndMature;

        protected double _energyCurrent;
        private double _energyMax;
        private double _energyHungryLevel;
        private double _energyDecreaseOnIteration;

        private bool _isMale;

        private bool _isPregnancy;
        private bool _isNeedToGiveBirth;
        private int _pregnancyCurrent;
        private int _pregnancyLength;

        private DeathType _deathType;

        public int AgeCurrent => _ageCurrent;

        public int AgeMax
        {
            set => _ageMax = value;
        }

        public int AgeBeginMature
        {
            get => _ageBeginMature;
            set => _ageBeginMature = value;
        }

        public int AgeEndMature
        {
            get => _ageEndMature;
            set => _ageEndMature = value;
        }

        public double EnergyCurrent => _energyCurrent;

        public double EnergyMax
        {
            get => _energyMax;
            set => _energyMax = value;
        }

        public double EnergyHungryLevel
        {
            set => _energyHungryLevel = value;
        }

        public double EnergyDecreaseOnIteration
        {
            set => _energyDecreaseOnIteration = value;
        }

        public bool IsMale
        {
            get => _isMale;
            set => _isMale = value;
        }

        public bool IsPregnancy => _isPregnancy;
        public bool IsNeedToGiveBirth => _isNeedToGiveBirth;

        public int PregnancyLength
        {
            set => _pregnancyLength = value;
        }

        public DeathType DeathType
        {
            set => _deathType = value;
        }

        public abstract void Eat<T>(T obj);

        public void MoveTo(Cell cell)
        {
            if (cell == null) return;

            _cell.Clear();
            _cell = cell;
            _cell.OccupiedBy(this);
        }

        public int CountLengthTo(Cell to, Cell from)
        {
            var dTwo = (to.X - from.X) ^ 2 + (to.Y - from.Y) ^ 2;
            return (int) Math.Floor(Math.Sqrt(dTwo));
        }

        public bool CheckIfEnoughEnergy(Cell cell)
        {
            var energyNeeded = CountLengthTo(cell, _cell) * _energyDecreaseOnIteration;
            return _energyCurrent >= energyNeeded;
        }

        public Cell FindNearAvailableCellOnWayTo(Cell toCell, Cell[][] cells)
        {
            var x = _cell.X;
            var y = _cell.Y;
            var nearCells = new List<Cell>(8);

            var leftLimit = x == 0;
            var upLimit = y == 0;
            var rightLimit = x == cells[0].Length - 1;
            var downLimit = y == cells.Length - 1;

            if (leftLimit)
            {
                if (upLimit)

                    nearCells.AddRange(new[]
                    {
                        cells[y][x + 1],
                        cells[y + 1][x + 1],
                        cells[y + 1][x]
                    });
                else if (downLimit)
                    nearCells.AddRange(new[]
                    {
                        cells[y - 1][x],
                        cells[y - 1][x + 1],
                        cells[y][x + 1]
                    });
                else
                    nearCells.AddRange(new[]
                    {
                        cells[y - 1][x],
                        cells[y - 1][x + 1],
                        cells[y][x + 1],
                        cells[y + 1][x + 1],
                        cells[y + 1][x]
                    });
            }
            else if (rightLimit)
            {
                if (upLimit)
                    nearCells.AddRange(new[]
                    {
                        cells[y][x - 1],
                        cells[y + 1][x - 1],
                        cells[y + 1][x]
                    });
                else if (downLimit)
                    nearCells.AddRange(new[]
                    {
                        cells[y][x - 1],
                        cells[y - 1][x - 1],
                        cells[y - 1][x]
                    });
                else
                    nearCells.AddRange(new[]
                    {
                        cells[y - 1][x],
                        cells[y - 1][x - 1],
                        cells[y][x - 1],
                        cells[y + 1][x - 1],
                        cells[y + 1][x]
                    });
            }
            else if (upLimit)
            {
                nearCells.AddRange(new[]
                {
                    cells[y][x - 1],
                    cells[y + 1][x - 1],
                    cells[y + 1][x],
                    cells[y + 1][x + 1],
                    cells[y][x + 1]
                });
            }
            else if (downLimit)
            {
                nearCells.AddRange(new[]
                {
                    cells[y][x - 1],
                    cells[y - 1][x - 1],
                    cells[y - 1][x],
                    cells[y - 1][x + 1],
                    cells[y][x + 1]
                });
            }
            else
            {
                nearCells.AddRange(new[]
                {
                    cells[y][x - 1],
                    cells[y - 1][x - 1],
                    cells[y - 1][x],
                    cells[y - 1][x + 1],
                    cells[y][x + 1],
                    cells[y + 1][x + 1],
                    cells[y + 1][x],
                    cells[y + 1][x - 1]
                });
            }

            while (nearCells.Count != 0)
            {
                var nearCell = nearCells[0];
                var len = 0;
                var i = 0;
                foreach (var cell in nearCells.Where(cell => (i = CountLengthTo(toCell, cell)) < len))
                {
                    len = i;
                    nearCell = cell;
                }

                if (nearCell.IsAvailable()) return nearCell;
                nearCells.Remove(nearCell);
            }

            return null;
        }

        public T FindTheNearestObj<T>(List<T> objects) where T : ICell
        {
            T nearObj = default;

            var minimum = int.MaxValue;
            foreach (var obj in objects)
            {
                var cell = obj.GetCell();
                var d = CountLengthTo(cell, _cell);

                if (d >= minimum) continue;

                minimum = d;
                nearObj = obj;
            }

            return nearObj;
        }

        public void IncreaseAgeCurrent()
        {
            _ageCurrent++;
            if (_ageCurrent >= _ageMax) _deathType = DeathType.ByAge;
        }

        protected void IncreaseEnergyByEating(double energy)
        {
            _energyCurrent += energy;
            if (_energyCurrent >= _energyMax) _energyCurrent = _energyMax;
        }

        public void DecreaseEnergyOnIteration()
        {
            _energyCurrent -= _energyDecreaseOnIteration;
            if (_energyCurrent <= 0)
                _deathType = DeathType.ByHunger;
        }

        public void Reproduction<T>(T fish) where T : FishAbstract
        {
            if (_ageCurrent < _ageBeginMature || _ageCurrent >= _ageEndMature ||
                fish._ageCurrent < fish._ageBeginMature || fish._ageCurrent >= fish._ageEndMature)
                return;
            if (fish._isMale && !_isMale && !_isPregnancy)
                _isPregnancy = true;
        }

        public void IncreasePregnancyState()
        {
            if (!_isPregnancy) return;
            _pregnancyCurrent++;
            if (_pregnancyCurrent < _pregnancyLength) return;

            _isPregnancy = false;
            _isNeedToGiveBirth = true;
        }

        public Cell GetCell() => _cell;
        public DeathType GetDeathType() => _deathType;
    }
}