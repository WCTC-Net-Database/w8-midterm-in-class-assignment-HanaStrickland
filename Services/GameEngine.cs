using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;
    private ICharacter _vampire;
    private ICharacter _ghoul;

    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        // TODO Update this method to allow for attacking a selected monster in the room.
        // TODO e.g. "Which monster would you like to attack?"
        // TODO Right now it just attacks the first monster in the room.
        // TODO It is ok to leave this functionality if there is only one monster in the room.
        var CurrentRoomCharacters = _player.CurrentRoom.Characters;
        var target = CurrentRoomCharacters.FirstOrDefault(c => c != _player);

        if (CurrentRoomCharacters.Count > 0)
        {
            // 1 target
            if (CurrentRoomCharacters.Count == 1)
            {
                _player.Attack(target);
                CurrentRoomCharacters.Remove(target);
            }
            // >1 target
            else
            {
                int charListCount = 1;

                foreach (ICharacter character in CurrentRoomCharacters)
                {
                    _outputManager.WriteLine($"{charListCount}:\t{character.Name}");
                    charListCount +=1;
                }

                _outputManager.WriteLine("Which monster would you like to attack?");

                _outputManager.Display();

                int monsterIndex = Convert.ToInt16(Console.ReadLine()) - 1;

                target = CurrentRoomCharacters[monsterIndex];

                _outputManager.WriteLine($"You chose to attack {target.Name}");

                _player.Attack(target);
                CurrentRoomCharacters.Remove(target);
            }
        }
        else
        {
            _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _mapManager.DisplayMap();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Move North");
            _outputManager.WriteLine("2. Move South");
            _outputManager.WriteLine("3. Move East");
            _outputManager.WriteLine("4. Move West");

            // Check if there are characters in the current room to attack
            if (_player.CurrentRoom.Characters.Any(c => c != _player))
            {
                _outputManager.WriteLine("5. Attack");
            }

            _outputManager.WriteLine("6. Exit Game");

            _outputManager.Display();

            var input = Console.ReadLine();

            string? direction = null;
            switch (input)
            {
                case "n":
                    direction = "north";
                    break;
                case "s":
                    direction = "south";
                    break;
                case "e":
                    direction = "east";
                    break;
                case "w":
                    direction = "west";
                    break;
                case "5":
                    if (_player.CurrentRoom.Characters.Any(c => c != _player))
                    {
                        AttackCharacter();
                    }
                    else
                    {
                        _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                    }

                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                    break;
            }

            // Update map manager with the current room after movement
            if (!string.IsNullOrEmpty(direction))
            {
                _outputManager.Clear();
                _player.Move(direction);
                _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();

        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        // TODO Load your two new monsters here into the same room
        randomRoom = _rooms[random.Next(_rooms.Count)];
        _vampire = _context.Characters.OfType<Vampire>().FirstOrDefault();
        randomRoom.AddCharacter(_vampire);
        _ghoul = _context.Characters.OfType<Ghoul>().FirstOrDefault();
        randomRoom.AddCharacter(_ghoul);
    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        // TODO Update this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var parlor = _roomFactory.CreateRoom("parlor",_outputManager);
        var dining = _roomFactory.CreateRoom("dining", _outputManager);

        entrance.North = treasureRoom;
        entrance.West = library;
        entrance.East = garden;
        entrance.South = parlor;

        parlor.North = entrance;
        parlor.South = dining;
        dining.North = parlor;

        treasureRoom.South = entrance;
        treasureRoom.West = dungeonRoom;

        dungeonRoom.East = treasureRoom;

        library.East = entrance;
        library.South = armory;

        armory.North = library;

        garden.West = entrance;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden, parlor, dining};

        return entrance;
    }
}
