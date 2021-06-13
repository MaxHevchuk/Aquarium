using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Aquarium.enums;
using Aquarium.Fish;

namespace Aquarium
{
    public partial class MainWindow
    {
        private static int _height;
        private static int _width;
        private static int _currentItr;
        private static DispatcherTimer _timer;

        private List<Image> _objImages;
        private static readonly Random Random = new Random();

        // <!-- INPUT NUMBERS OF -->
        private static int _numOfHerbFish;
        private static int _numOfPredFish;
        private static int _numOfHerbs;
        private static int _numOfStones;

        //<!-- INPUT PROPERTIES -->
        //<!-- age -->
        private static int _ageBeginMature;
        private static int _ageEndMature;
        private static int _ageMax;

        //<!-- energy -->
        private static double _energyMax;
        private static double _energyHungryLevel;
        private static double _energyDecreaseOnItr;
        private static int _pregnancyLength;

        //<!-- growth -->
        private static double _growthMax;
        private static double _growthIncreaseOnItr;

        public MainWindow()
        {
            InitializeComponent();
            OnGridUpdate(null, null);
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
                _timer.Tick += StartIteration;
            }

            _timer.Start();
        }

        private void StartIteration(object sender, EventArgs e)
        {
            HerbNextIteration();

            HerbivorousFishNextIteration();
            UpdateObjectsLife(AquariumContainer.GetInstance().HerbivorousFish, AquariumContainer.GetInstance().Herbs);

            PredatorFishNextIteration();
            UpdateObjectsLife(AquariumContainer.GetInstance().PredatorFish,
                AquariumContainer.GetInstance().HerbivorousFish);

            UpdateObjectsImages();
            UpdateObjectsPlaces();

            CurrentIterationTextBlock.Text = (++_currentItr).ToString();
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private static void HerbNextIteration()
        {
            AquariumContainer.GetInstance().Herbs.ForEach(fish => fish.Grow());
        }

        private static void HerbivorousFishNextIteration()
        {
            FishNextIteration(AquariumContainer.GetInstance().HerbivorousFish,
                AquariumContainer.GetInstance().Herbs);
        }

        private static void PredatorFishNextIteration()
        {
            FishNextIteration(AquariumContainer.GetInstance().PredatorFish,
                AquariumContainer.GetInstance().HerbivorousFish);
        }

        private static void FishNextIteration<T, TE>(List<T> fishes, List<TE> food)
            where T : FishAbstract, new()
            where TE : ICell
        {
            var aquarium = AquariumContainer.GetInstance();
            List<T> children = null;
            foreach (var fish in fishes)
            {
                fish.IncreaseAgeCurrent();
                fish.IncreasePregnancyState();
                fish.DecreaseEnergyOnIteration();

                if (fish.GetDeathType() != DeathType.Undefined) return;

                var nearFood = fish.FindTheNearestObj(food);
                var nearFoodCell = nearFood?.GetCell();

                var anotherFishes = new List<T>(fishes).Where(f => fish.IsMale != f.IsMale).ToList();
                anotherFishes.Remove(fish);
                var nearFish = fish.FindTheNearestObj(anotherFishes.Where(f => !f.IsPregnant).ToList());
                var nearFishCell = nearFish?.GetCell();

                if (nearFoodCell != null && fish.EnergyCurrent < fish.EnergyHungryLevel &&
                    fish.CountLengthTo(nearFoodCell, fish.GetCell()) > 1)
                {
                    fish.MoveTo(
                        fish.FindNearAvailableCellOnWayTo(nearFoodCell, aquarium.Cells));
                    // if fish is near to herb fish then this eat it
                }
                else if (nearFishCell != null && fish.EnergyCurrent >= fish.EnergyHungryLevel &
                    !fish.IsPregnant & fish.AgeBeginMature <= fish.AgeCurrent & fish.AgeCurrent < fish.AgeEndMature &
                    fish.CountLengthTo(nearFishCell, fish.GetCell()) > 1)
                {
                    fish.MoveTo(
                        fish.FindNearAvailableCellOnWayTo(nearFishCell, aquarium.Cells));
                    // if fishes are near one to another then reproduct
                }
                else
                    fish.MoveTo(
                        fish.FindNearAvailableCellOnWayTo(GetRandomCell(aquarium.Cells), aquarium.Cells));


                if (nearFood != null & nearFoodCell != null && fish.CountLengthTo(nearFoodCell, fish.GetCell()) == 1)
                    fish.Eat(nearFood);
                if (nearFish != null & nearFishCell != null && fish.PregnancyCurrent < 1 &&
                    nearFish.PregnancyCurrent < 1 && fish.CountLengthTo(nearFishCell, fish.GetCell()) == 1)
                    fish.Reproduction(nearFish);
                if (!fish.IsPregnant || fish.PregnancyCurrent < fish.PregnancyLength) continue;

                if (children == null) children = new List<T>();
                children.Add(CreateChild<T>());
            }

            children?.ForEach(AquariumContainer.GetInstance().Add);
        }

        private static T CreateChild<T>() where T : FishAbstract
        {
            Cell cell;
            do
            {
                cell = GetRandomCell(AquariumContainer.GetInstance().Cells);
            } while (!cell.IsAvailable());

            var child = (T) Activator.CreateInstance(typeof(T), cell);
            GenerateFish(child);

            return child;
        }

        private static void UpdateObjectsLife<T, TE>(List<T> listOfFish, List<TE> listOfFood)
            where T : FishAbstract
            where TE : ICell
        {
            var aquarium = AquariumContainer.GetInstance();
            foreach (var fish in listOfFish.Where(fish => fish.GetDeathType() != DeathType.Undefined).ToList())
            {
                fish.GetCell().Clear();
                aquarium.Remove(fish);
            }

            // listOfFish.Where(fish => fish.GetDeathType() != DeathType.Undefined).ToList().ForEach(fish =>
            // {
            //     fish.GetCell().Clear();
            //     aquarium.Remove(fish);
            // });

            foreach (var food in listOfFood.Where(food => food.GetDeathType() != DeathType.Undefined).ToList())
            {
                food.GetCell().Clear();
                aquarium.Remove(food);
            }

            // listOfFood.Where(food => food.GetDeathType() != DeathType.Undefined).ToList().ForEach(food =>
            // {
            //     food.GetCell().Clear();
            //     aquarium.Remove(food);
            // });
        }

        private void UpdateObjectsImages()
        {
            var aquarium = AquariumContainer.GetInstance();
            _objImages?.Clear();
            if (_objImages == null)
                _objImages = new List<Image>(_numOfHerbs + _numOfStones + _numOfHerbFish + _numOfPredFish);

            aquarium.Herbs.ForEach(herb => _objImages.Add(UpdateHerbImage(herb)));
            // foreach (var herb in aquarium.Herbs)
            // {
            //     _objImages.Add(UpdateHerbImage(herb));
            // }

            aquarium.HerbivorousFish.ForEach(fish => _objImages.Add(UpdateFishImage(fish, "herbivorous")));
            // foreach (var fish in aquarium.HerbivorousFish)
            // {
            //     _objImages.Add(UpdateFishImage(fish, "herbivorous"));
            // }

            aquarium.PredatorFish.ForEach(fish => _objImages.Add(UpdateFishImage(fish, "predator")));
            // foreach (var fish in aquarium.PredatorFish)
            // {
            //     _objImages.Add(UpdateFishImage(fish, "predator"));
            // }

            aquarium.Stones.ForEach(stone => _objImages.Add(UpdateStoneImage(stone)));
            // foreach (var stone in aquarium.Stones)
            // {
            //     _objImages.Add(UpdateStoneImage(stone));
            // }
        }

        private static Image UpdateStoneImage(Stone stone)
        {
            var image = new Image
                {Source = new BitmapImage(new Uri(@"../../src/res/icons/stone/stone.png", UriKind.Relative))};
            Grid.SetColumn(image, stone.Cell.X);
            Grid.SetRow(image, stone.Cell.Y);
            return image;
        }

        private static Image UpdateFishImage(FishAbstract fish, string fishName)
        {
            var uri = @"../../src/res/icons/fish/" + fishName;
            if (fish.AgeCurrent < fish.AgeBeginMature)
                uri += @"/young/";
            else if (fish.AgeCurrent >= fish.AgeBeginMature && fish.AgeCurrent < fish.AgeEndMature)
                uri += @"/adult/";
            else if (fish.AgeCurrent >= fish.AgeEndMature)
                uri += @"/old/";
            uri += fish.IsMale ? "boy.png" : fish.IsPregnant ? "girl_pregnant.png" : "girl.png";
            var image = new Image {Source = new BitmapImage(new Uri(uri, UriKind.Relative))};
            Grid.SetColumn(image, fish.GetCell().X);
            Grid.SetRow(image, fish.GetCell().Y);
            return image;
        }

        private static Image UpdateHerbImage(Herb herb)
        {
            var image = new Image
                {Source = new BitmapImage(new Uri(@"../../src/res/icons/herb/herb.png", UriKind.Relative))};
            Grid.SetColumn(image, herb.GetCell().X);
            Grid.SetRow(image, herb.GetCell().Y);
            return image;
        }

        private void UpdateObjectsPlaces()
        {
            AquariumGrid.Children.Clear();
            _objImages.ForEach(image => AquariumGrid.Children.Add(image));
            // foreach (var image in _objImages)
            // {
            //     AquariumGrid.Children.Add(image);
            // }
        }


        private void OnObjectsUpdate(object sender, RoutedEventArgs e)
        {
            _currentItr = 0;
            CurrentIterationTextBlock.Text = "0";
            OnGridUpdate(null, null);
            if (!ConvertToNumbers())
            {
                MessageBox.Show("Enter correct data!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_numOfHerbs + _numOfStones + _numOfHerbFish + _numOfPredFish
                >= _width * _height)
            {
                MessageBox.Show("You have specified more objects than aquarium can contain.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GenerateObjects();
            UpdateObjectsImages();
            UpdateObjectsPlaces();
        }

        private bool ConvertToNumbers()
        {
            return
                int.TryParse(NumOfHerbFish.Text, out _numOfHerbFish) &&
                int.TryParse(NumOfPredFish.Text, out _numOfPredFish) &&
                int.TryParse(NumOfHerbs.Text, out _numOfHerbs) &&
                int.TryParse(NumOfStones.Text, out _numOfStones)
                &&
                int.TryParse(AgeBeginMature.Text, out _ageBeginMature) &&
                int.TryParse(AgeEndMature.Text, out _ageEndMature) &&
                int.TryParse(AgeMax.Text, out _ageMax)
                &&
                double.TryParse(EnergyMax.Text, out _energyMax) &&
                double.TryParse(EnergyHundryLvl.Text, out _energyHungryLevel) &&
                double.TryParse(EnergyDecreaseOnItr.Text, out _energyDecreaseOnItr)
                &&
                int.TryParse(PregnancyLen.Text, out _pregnancyLength) &&
                double.TryParse(GrowthMax.Text, out _growthMax) &&
                double.TryParse(GrowthIncreaseOnItr.Text, out _growthIncreaseOnItr);
        }


        private void OnGridUpdate(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthTextBox.Text, out _width)
                || !int.TryParse(HeightTextBox.Text, out _height))
            {
                MessageBox.Show("Enter correct data!");
                return;
            }

            AquariumGrid.Children.Clear();
            AquariumGrid.RowDefinitions.Clear();
            AquariumGrid.ColumnDefinitions.Clear();

            GenerateNewGrid(AquariumGrid, _height, _width);
        }

        private static void GenerateObjects()
        {
            var aquarium = AquariumContainer.GetInstance(_height, _width);
            var cells = aquarium.Cells;
            aquarium.Clear();

            for (var i = 0; i < _numOfHerbFish; i++)
            {
                var fishH = new HerbivorousFish(GetRandomCell(cells));
                GenerateFish(fishH);
                aquarium.Add(fishH);
            }

            for (var i = 0; i < _numOfPredFish; i++)
            {
                var fishP = new PredatorFish(GetRandomCell(cells));
                GenerateFish(fishP);
                aquarium.Add(fishP);
            }

            for (var i = 0; i < _numOfHerbs; i++)
            {
                aquarium.Add(new Herb(GetRandomCell(cells))
                {
                    GrowthMax = _growthMax, GrowthIncreaseOnIteration = _growthIncreaseOnItr,
                    GrowthCurrent = new Random().Next((int) (_growthMax * 0.2), (int) (_growthMax * 0.5))
                });
            }

            for (var i = 0; i < _numOfStones; i++)
            {
                aquarium.Add(new Stone(GetRandomCell(cells)));
            }
        }

        private static Cell GetRandomCell(Cell[][] cells)
        {
            Cell cell;
            do
            {
                var i = Random.Next(cells.Length);
                var j = Random.Next(cells[i].Length);
                cell = cells[i][j];
            } while (!cell.IsAvailable());

            return cell;
        }

        private static void GenerateFish<T>(T fish) where T : FishAbstract
        {
            if (fish.GetCell() == null) fish.Cell = GetRandomCell(AquariumContainer.GetInstance().Cells);
            fish.IsMale = Random.Next(0, 2) == 1;
            fish.EnergyCurrent = new Random().Next((int) _energyHungryLevel, (int) _energyMax);
            fish.AgeBeginMature = _ageBeginMature;
            fish.AgeEndMature = _ageEndMature;
            fish.AgeMax = _ageMax;
            fish.EnergyMax = _energyMax;
            fish.EnergyHungryLevel = _energyHungryLevel;
            fish.EnergyDecreaseOnIteration = _energyDecreaseOnItr;
            fish.PregnancyLength = _pregnancyLength;
        }

        private static void GenerateNewGrid(Grid grid, int row, int column)
        {
            for (var i = 0; i < row; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < column; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }
    }
}