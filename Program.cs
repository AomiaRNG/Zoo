using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Foods = System.Collections.Generic.Dictionary<zoo.Food, uint>;

namespace zoo
{
    public enum Food
    {
        meat = 0,
        hay = 1,
        fruit = 2,
        grain = 3,
    }

    public struct Animal
    {
        public String type;
        public Food food;
        public uint foodAmount;
        public uint maxAge;
        public uint age;
    }

    public enum GameState
    {
        newgame,
        round,
        end,
    }

    class Program
    {
        static Animal[] animals = new Animal[7] {
            new Animal{ food = Food.meat, type = "Lion", foodAmount = 4, maxAge = 20 },
            new Animal{ food = Food.meat, type = "Tiger", foodAmount = 3, maxAge = 30},
            new Animal{ food = Food.hay, type = "Cow", foodAmount = 4, maxAge = 25 },
            new Animal{ food = Food.hay, type = "Sheep", foodAmount = 2, maxAge = 15 },
            new Animal{ food = Food.hay, type = "Elephant", foodAmount = 18, maxAge = 70 },
            new Animal{ food = Food.fruit, type = "Monkey", foodAmount = 3, maxAge = 50 },
            new Animal{ food = Food.grain, type = "Parrot", foodAmount = 1, maxAge = 5 },
        };

        public class Cage
        {
            public Animal animal;
            public uint animalQuantity = 0;
            public Foods foods = new Foods();
            public Cage(Animal animal, uint quantity)
            {
                this.animal = animal;
                this.animalQuantity = quantity;
            }
        }

        public struct AnimalChangeInfo
        {
            public string type;
            public uint count;
            public AnimalChangeInfo(string a, uint b)
            {
                type = a;
                count = b;
            }
        }

        public interface IGame
        {
            void StartGame(uint numberOfCages, uint b);

            void MoveFood(Food food, uint eatCount, uint cageCount);

            List<(AnimalChangeInfo, AnimalChangeInfo)> NextFood(uint eatCount, uint cageCount);

            List<Cage> GetCages();

            GameState GetGameState();

            void SetFoodToStorage(uint a);

            Foods GetFoodFromStorage();
        }        

        public class Game : IGame
        {
            private Foods foodOnStorage = new Foods();            

            private static readonly (int, int) randInterval = (1, 4);

            public List<Cage> cages = new List<Cage>();

            private GameState gameState = GameState.newgame;

            public void SetFoodToStorage(uint a)
            {
                for (int i = 0; i < Enum.GetNames(typeof(Food)).Length; i++)
                    foodOnStorage.Add((Food)i, a);
            }
            
            public Foods GetFoodFromStorage() => foodOnStorage;

            public void StartGame(uint numberOfCages, uint countOfFood)
            {
                SetFoodToStorage(countOfFood);//--!--
                Random r = new Random();
                for (int i = 0; i < numberOfCages; i++)
                {
                    var animal = animals[r.Next(0, animals.Length)];
                    uint quantity = (uint)r.Next(randInterval.Item1, randInterval.Item2);
                    cages.Add(new Cage(animal, quantity));
                }
                gameState = GameState.round;
            }

            public void MoveFood(Food food, uint eatCount, uint cageCount)
            {                
                var cage = cages[(int)cageCount - 1];
                cage.foods[food] = (cage.foods.ContainsKey(food) ? cage.foods[food] : 0) + eatCount;
                foodOnStorage[food] -= eatCount;
            }
            
            public List<(AnimalChangeInfo, AnimalChangeInfo)> NextFood(uint EatCount, uint CageCount)
            {
                //Storage storage = new Storage(50);
                List<(AnimalChangeInfo, AnimalChangeInfo)> newDieAnimal = new List<(AnimalChangeInfo, AnimalChangeInfo)>();
                for (int i = 1; i <= cages.Count; i++)
                {
                    uint localAnimalCount;
                    String animalType;
                    uint newAnimalsCount = 0;
                    var cage = cages[i - 1];
                    Food food = cage.animal.food;
                    uint foodCount = cage.foods.ContainsKey(food) ? cage.foods[food] : 0;

                    localAnimalCount = (uint)Math.Max(0, (int)(cage.animalQuantity - (foodCount / cage.animal.foodAmount)));
                    animalType = cage.animal.type;
                    
                    var AQ = cage.animalQuantity;                    
                    cage.animalQuantity = cage.animalQuantity - localAnimalCount;

                    foodCount = (uint)Math.Max(0, (int)(foodCount - AQ * cage.animal.foodAmount));
                                        
                    Random r = new Random();
                    var ProbablyNewAnimal = foodCount / cage.animal.foodAmount > cage.animalQuantity ? cage.animalQuantity : foodCount / cage.animal.foodAmount;
                    for (int j = 0; j < ProbablyNewAnimal; j++)
                        if (r.Next(1, 100) <= 30)
                            newAnimalsCount++;
                    cage.animalQuantity += newAnimalsCount;
                    cage.foods = new Foods();
                    AnimalChangeInfo a = new AnimalChangeInfo(animalType, newAnimalsCount);
                    AnimalChangeInfo b = new AnimalChangeInfo(animalType, localAnimalCount);
                    newDieAnimal.Add((a, b));
                }
                uint animalcount = 0;
                for (int i = 0; i < cages.Count; i++)
                    animalcount += cages[i].animalQuantity;
                if (animalcount == 0)
                    gameState = GameState.end;
                return newDieAnimal;
            }

            public List<Cage> GetCages() => cages;

            public GameState GetGameState() => gameState;
        }

        public class UserInOutput
        {
            IGame prog;
            public uint numOfCages = 0;
            public uint foodCount = 0;
            String input;
            string[] arguments;
            
            public UserInOutput(IGame a)
            {
                prog = a;
            }
            
            public void FoodCountOnStorage(uint a)
            {
                input = null;
                Console.WriteLine("Введите количество еды на складе каждого типа:");
                do
                {
                    if (input != null)
                        Console.WriteLine("Wrong number. Try again");

                    input = Console.ReadLine();
                } while (!uint.TryParse(input, out foodCount) || foodCount == 0);
                prog.StartGame(a, foodCount);
            }

            public void NumberOfCages()
            {
                Console.WriteLine("Введите количество клеток:");
                do
                {
                    if (input != null)
                        Console.WriteLine("Wrong number. Try again");

                    input = Console.ReadLine();
                } while (!uint.TryParse(input, out numOfCages) || numOfCages == 0);                
                FoodCountOnStorage(numOfCages);
            }

            public void StorageInformation(Foods a)
            {
                string b;
                b = string.Join("", a.Select(kv => kv.Value + " " + kv.Key + "\n"));
                Console.WriteLine("\nFood on storage: "+ '\n' + b);

            }

            public void Round()
            {
                printCages();
                StorageInformation(prog.GetFoodFromStorage());
                uint eatCount = 0;
                uint numCage = 0;
                Food food = 0;

                bool end = false;
                while (!end)
                {
                    input = Console.ReadLine();
                    arguments = input.Trim().Split(' ');

                    if (arguments.Length == 0)
                        Console.WriteLine("Invalid input. Try again");
                    else if (arguments[0] != "move" && arguments[0] != "next")
                        Console.WriteLine("Only next and move commands available. Try again");
                    else if (arguments[0] == "move")
                    {
                        if (arguments.Length > 5)
                            Console.WriteLine("Wrong data");
                        else if (arguments.Length < 2 || !uint.TryParse(arguments[1], out eatCount))
                            Console.WriteLine("Invalid count of food");
                        else if (arguments.Length < 3 || !Enum.TryParse(arguments[2], out food))
                            Console.WriteLine("Invalid food");
                        else if (arguments.Length < 4 || arguments[3] != "to")
                            Console.WriteLine("Only keyword to is avalable after food");
                        else if (arguments.Length < 5 || !uint.TryParse(arguments[4], out numCage))
                        {
                            if (numCage < 1 || uint.Parse(arguments[4]) > prog.GetCages().Count)
                                Console.WriteLine("Invalid enclosure number");
                        }
                        else
                        {
                            end = true;
                        }
                    }
                    else if (arguments[0] == "next")
                        end = true;
                }


                if (arguments.Length == 5 && arguments[0] == "move")
                {
                    if (numCage > 0 && numCage <= prog.GetCages().Count && prog.GetFoodFromStorage()[food] >= eatCount)
                        prog.MoveFood(food, eatCount, numCage);
                    else
                        Console.WriteLine("Invalid number of cage or count of food");
                }
                else if (arguments[0] == "next" && arguments.Length == 1)
                {
                    var param = prog.NextFood(eatCount, numCage);
                    for (int i = 0; i < param.Count; i++)
                    {
                        var newAnimal = param[i].Item1;
                        var dieAnimal = param[i].Item2;
                        if (newAnimal.count != 0)
                            Console.WriteLine($"{newAnimal.count} {newAnimal.type}{(newAnimal.count > 1 ? "s" : "")} spawned");
                        if (dieAnimal.count != 0)
                            Console.WriteLine($"{dieAnimal.count} {dieAnimal.type}{(dieAnimal.count > 1 ? "s" : "")} starved to death.");
                    }
                }
                if (prog.GetGameState() == GameState.end)
                    Console.WriteLine("Game over!");
            }

            public void printCages()
            {

                for (int i = 0; i < prog.GetCages().Count; i++)
                {
                    Cage cage = prog.GetCages()[i];

                    string a = string.Join(",", cage.foods.Select(kv => kv.Value + " " + kv.Key));
                    a = (a == "" ? "0" : a);
                    Console.WriteLine($"Enclosure {i + 1}: {cage.animal.type}.\tQuantity: {cage.animalQuantity}.\tEat: {cage.animal.food}.\tFood: {a}.");
                }
            }

            public GameState GetGameState() => prog.GetGameState();

        }

        static void Main(string[] args)
        {            
            UserInOutput user = new UserInOutput(new Game());
            user.NumberOfCages();
            while (user.GetGameState() != GameState.end)
            {
                user.Round();
            }
            Console.ReadLine();
        }
    }
}
