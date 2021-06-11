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

        private List<Image> _images;


        // private Controller _aquarium;
        private static readonly Random Random = new Random();

        private readonly Grid _aquariumGrid;


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
            _aquariumGrid = GridAquarium;
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            // Application.Current.Dispatcher.Invoke((Action) delegate { _aquarium.Start(); });
            // StartIteration(null, null);
            // var timerCallback = new TimerCallback(StartIteration);
            // if (_timer == null) _timer = new Timer(timerCallback, new object(), 0, 5000);
            // else _timer.Change(0, 10000);

            if (_timer == null)
            {
                _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                _timer.Tick += StartIteration;
            }

            _timer.Start();
        }

        private void StartIteration(object sender, EventArgs e)
        {
            HerbNextIteration();

            HerbivorousNextIteration();
            UpdateObjectsLife(AquariumContainer.GetInstance().HerbivorousFish, AquariumContainer.GetInstance().Herbs);

            PredatorNextIteration();
            UpdateObjectsLife(AquariumContainer.GetInstance().PredatorFish,
                AquariumContainer.GetInstance().HerbivorousFish);

            UpdateObjectsImages();
            UpdateObjectsPlaces();
            
            CurrentIterationTextBlock.Text = (++_currentItr).ToString();
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            // _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Stop();
        }

        private static void HerbNextIteration()
        {
            foreach (var herb in AquariumContainer.GetInstance().Herbs)
            {
                herb.Grow();
            }
        }

        private static void HerbivorousNextIteration()
        {
            FishNextIteration(AquariumContainer.GetInstance().HerbivorousFish, AquariumContainer.GetInstance().Herbs);
        }

        private static void PredatorNextIteration()
        {
            FishNextIteration(AquariumContainer.GetInstance().PredatorFish,
                AquariumContainer.GetInstance().HerbivorousFish);
        }

        private static void FishNextIteration<T, TK>(List<T> fishes, List<TK> food)
            where T : FishAbstract
            where TK : ICell
        {
            foreach (var fish in fishes)
            {
                fish.IncreaseAgeCurrent();
                fish.IncreasePregnancyState();
                fish.DecreaseEnergyOnIteration();

                if (fish.GetDeathType() != DeathType.Undefined) return;

                var nearFood = fish.FindTheNearestObj(food);
                var nearFoodCell = nearFood.GetCell();

                var nearFish = fish.FindTheNearestObj(fishes);
                var nearFishCell = nearFish.GetCell();

                if (fish.CheckIfEnoughEnergy(nearFoodCell))
                {
                    fish.MoveTo(
                        fish.FindNearAvailableCellOnWayTo(nearFoodCell, AquariumContainer.GetInstance().Cells));
                    if (fish.CountLengthTo(nearFoodCell, fish.GetCell()) == 1)
                        fish.Eat(nearFood);
                    // if fish is near to herb fish then this eat it
                }
                else if (fish.CheckIfEnoughEnergy(nearFishCell))
                {
                    fish.MoveTo(
                        fish.FindNearAvailableCellOnWayTo(nearFishCell, AquariumContainer.GetInstance().Cells));
                    if (fish.CountLengthTo(nearFishCell, fish.GetCell()) == 1)
                        fish.Reproduction(nearFish);
                    // if fishes are near one to another then reproduct
                }
            }
        }

        private static void UpdateObjectsLife<T, TK>(List<T> listOfFish, List<TK> listOfFood)
            where T : FishAbstract
            where TK : ICell
        {
            if (listOfFish == null || listOfFood == null) return;

            foreach (var fish in listOfFish.Where(fish => fish.GetDeathType() != DeathType.Undefined).ToList())
            {
                fish.GetCell().Clear();
                AquariumContainer.GetInstance().Remove(fish);
            }

            foreach (var food in listOfFood.Where(food => food.GetDeathType() != DeathType.Undefined).ToList())
            {
                food.GetCell().Clear();
                AquariumContainer.GetInstance().Remove(food);
            }
        }

        private void UpdateObjectsImages()
        {
            var aquarium = AquariumContainer.GetInstance();
            _images?.Clear();
            if (_images == null)
                _images = new List<Image>(_numOfHerbs + _numOfStones + _numOfHerbFish + _numOfPredFish);

            foreach (var herb in aquarium.Herbs)
            {
                _images.Add(UpdateHerbImage(herb));
            }

            foreach (var fish in aquarium.HerbivorousFish)
            {
                _images.Add(UpdateFishImage(fish, "herbivorous"));
            }

            foreach (var fish in aquarium.PredatorFish)
            {
                _images.Add(UpdateFishImage(fish, "predator"));
            }

            foreach (var stone in aquarium.Stones)
            {
                _images.Add(UpdateStoneImage(stone));
            }
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
            uri += fish.IsMale ? "boy.png" : fish.IsPregnancy ? "girl_pregnant.png" : "girl.png";

            var image = new Image {Source = new BitmapImage(new Uri(uri, UriKind.Relative))};
            Grid.SetColumn(image, fish.GetCell().X);
            Grid.SetRow(image, fish.GetCell().Y);
            return image;
        }

        private static Image UpdateHerbImage(Herb herb)
        {
            var imageSource = new BitmapImage(new Uri(@"../../src/res/icons/herb/herb.png", UriKind.Relative));
            var image = new Image {Source = imageSource};
            Grid.SetColumn(image, herb.GetCell().X);
            Grid.SetRow(image, herb.GetCell().Y);
            return image;
        }

        private void UpdateObjectsPlaces()
        {
            _aquariumGrid.Children.Clear();
            foreach (var image in _images)
            {
                _aquariumGrid.Children.Add(image);
            }
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

            GridAquarium.Children.Clear();
            GridAquarium.RowDefinitions.Clear();
            GridAquarium.ColumnDefinitions.Clear();

            GenerateNewGrid(GridAquarium, _height, _width);
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
                    {GrowthMax = _growthMax, GrowthIncreaseOnIteration = _growthIncreaseOnItr});
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
            fish.IsMale = Random.Next(0, 2) == 1;
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