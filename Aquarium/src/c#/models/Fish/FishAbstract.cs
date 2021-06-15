using System;
using System.Collections.Generic;
using System.Linq;
using Aquarium.enums;
using Aquarium.interfaces;

namespace Aquarium.Fish
{
    public abstract class FishAbstract : ICell
    {
        public int AgeCurrent { get; private set; }

        public int AgeMax { get; set; }

        public int AgeBeginMature { get; set; }

        public int AgeEndMature { get; set; }

        public double EnergyCurrent { get; set; }

        public double EnergyMax { get; set; }

        public double EnergyHungryLevel { set; get; }

        public double EnergyDecreaseOnIteration { get; set; }

        public bool IsMale { get; set; }

        public bool IsPregnant { get; private set; }

        public int PregnancyCurrent { get; private set; }

        public int PregnancyLength { set; get; }

        public DeathType DeathType { get; set; }

        public Cell Cell { get; set; }
        public Cell GetCell() => Cell;
        public DeathType GetDeathType() => DeathType;

        public abstract void Eat<T>(T obj);

        public void MoveTo(Cell cell)
        {
            if (cell == null) return;

            Cell.Clear();
            Cell = cell;
            Cell.OccupiedBy(this);
        }

        public int CountLengthTo(Cell to, Cell from)
        {
            if (to == null || from == null) return -1;
            var dTwo = Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2);
            return (int) Math.Sqrt(dTwo);
        }

        public Cell FindNearAvailableCellOnWayTo(Cell toCell, Cell[][] cells)
        {
            var x = Cell.X;
            var y = Cell.Y;
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
                var len = int.MaxValue;
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
                var d = CountLengthTo(cell, Cell);
                if (d >= minimum) continue;
                minimum = d;
                nearObj = obj;
            }

            return nearObj;
        }

        public void IncreaseAgeCurrent()
        {
            if (++AgeCurrent >= AgeMax) DeathType = DeathType.ByAge;
        }

        protected void IncreaseEnergyByEating(double energy)
        {
            if ((EnergyCurrent += energy) >= EnergyMax) EnergyCurrent = EnergyMax;
        }

        public void DecreaseEnergyOnIteration()
        {
            if ((EnergyCurrent -= EnergyDecreaseOnIteration) <= 0) DeathType = DeathType.ByHunger;
        }

        public void Reproduction<T>(T fish) where T : FishAbstract
        {
            if (AgeCurrent < AgeBeginMature || AgeCurrent >= AgeEndMature ||
                fish.AgeCurrent < fish.AgeBeginMature || fish.AgeCurrent >= fish.AgeEndMature)
                return;
            if (!IsPregnant && fish.IsMale && !IsMale)
                IsPregnant = true;
        }

        public void IncreasePregnancyState()
        {
            if (!IsPregnant) return;
            if (++PregnancyCurrent <= PregnancyLength) return;
            IsPregnant = false;
            PregnancyCurrent = 0;
        }
    }
}