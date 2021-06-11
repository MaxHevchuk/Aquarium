using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Aquarium.enums;
using Aquarium.Fish;

namespace Aquarium
{
    public class Controller
    {
        private bool _isRun;
        private bool _isStopped;
        private readonly Grid _aquariumGrid;
        private readonly AquariumContainer _container;
        private List<Image> _images;

        public Controller(Grid grid)
        {
            _aquariumGrid = grid;
            _container = AquariumContainer.GetInstance();
        }


        public void Start()
        {
            _isRun = true;
            _isStopped = false;
            while (_isRun)
            {
                HerbNextIteration();

                HerbivorousNextIteration();
                UpdateObjectsLife(_container.HerbivorousFish, _container.Herbs);

                PredatorNextIteration();
                UpdateObjectsLife(_container.PredatorFish, _container.HerbivorousFish);

                _images.Clear();
                UpdateObjectsImages();
                UpdateObjectsPlaces();

                Thread.Sleep(2000);
            }

            _isStopped = true;
        }

        public bool Pause()
        {
            _isRun = false;
            Thread.Sleep(1000);
            return _isStopped;
        }

        private void HerbNextIteration()
        {
            var herbs = _container.Herbs;
            foreach (var herb in herbs)
            {
                herb.Grow();
            }
        }

        private void HerbivorousNextIteration()
        {
            var herbFish = _container.HerbivorousFish;
            var herbs = _container.Herbs;

            FishNextIteration(herbFish, herbs);
        }

        private void PredatorNextIteration()
        {
            var predatorFish = _container.PredatorFish;
            var herbFish = _container.HerbivorousFish;

            FishNextIteration(predatorFish, herbFish);
        }

        private void FishNextIteration<T, TK>(List<T> fishes, List<TK> food)
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
                    var nearCell = fish.FindNearAvailableCellOnWayTo(nearFoodCell, _container.Cells);
                    fish.MoveTo(nearCell);
                    if (fish.CountLengthTo(nearFoodCell, fish.GetCell()) == 1)
                        fish.Eat(nearFood);
                    // if fish is near to herb fish then this eat it
                }
                else if (fish.CheckIfEnoughEnergy(nearFishCell))
                {
                    var nearCell = fish.FindNearAvailableCellOnWayTo(nearFishCell, _container.Cells);
                    fish.MoveTo(nearCell);
                    if (fish.CountLengthTo(nearFishCell, fish.GetCell()) == 1)
                        fish.Reproduction(nearFish);
                    // if fishes are near one to another then reproduct
                }
            }
        }


        private void UpdateObjectsLife<T, TK>(List<T> listOfFish, List<TK> listOfFood)
            where T : FishAbstract
            where TK : ICell
        {
            if (listOfFish == null || listOfFood == null) return;

            foreach (var fish in listOfFish.Where(fish => fish.GetDeathType() != DeathType.Undefined).ToList())
            {
                fish.GetCell().Clear();
                _container.Remove(fish);
            }

            foreach (var food in listOfFood.Where(food => food.GetDeathType() != DeathType.Undefined).ToList())
            {
                food.GetCell().Clear();
                _container.Remove(food);
            }
        }

        public void UpdateObjectsImages()
        {
            var herbs = _container.Herbs;
            var herbivorous = _container.HerbivorousFish;
            var predator = _container.PredatorFish;
            var stones = _container.Stones;
            _images = new List<Image>(15);

            foreach (var herb in herbs)
            {
                _images.Add(UpdateHerbImage(herb));
            }

            foreach (var fish in herbivorous)
            {
                _images.Add(UpdateFishImage(fish, "herbivorous"));
            }

            foreach (var fish in predator)
            {
                _images.Add(UpdateFishImage(fish, "predator"));
            }

            foreach (var stone in stones)
            {
                _images.Add(UpdateStoneImage(stone));
            }
        }

        private static Image UpdateStoneImage(Stone stone)
        {
            var imageSource = new BitmapImage(new Uri(@"../../src/res/icons/stone/stone.png", UriKind.Relative));
            var image = new Image {Source = imageSource};
            Grid.SetColumn(image, stone.Cell.X);
            Grid.SetRow(image, stone.Cell.Y);
            return image;
        }

        private static Image UpdateFishImage(FishAbstract fish, string fishName)
        {
            var uri = @"../../src/res/icons/fish/" + fishName;
            if (fish.AgeCurrent < fish.AgeBeginMature)
                uri += @"/young/";
            else if (fish.AgeBeginMature >= fish.AgeCurrent && fish.AgeCurrent < fish.AgeEndMature)
                uri = @"/adult/";
            else if (fish.AgeCurrent >= fish.AgeEndMature)
                uri = @"/old/";
            uri += fish.IsMale ? "boy.png" : fish.IsNeedToGiveBirth ? "girl_pregnant" : "girl.png";

            var imageSource = new BitmapImage(new Uri(uri, UriKind.Relative));
            var image = new Image {Source = imageSource};
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

        public void UpdateObjectsPlaces()
        {
            _aquariumGrid.Children.Clear();

            foreach (var image in _images)
            {
                _aquariumGrid.Children.Add(image);
            }
        }
    }
}