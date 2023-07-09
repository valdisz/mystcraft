namespace advisor;

public static class Errors {
    public static readonly Error E_UNKNOWN                 = Error.New(100_00, "Unknown error.");
    public static readonly Error E_OPERATION_NOT_SUPPORTED = Error.New(100_01, "Operation not supported.");


    public static readonly Error E_LIST_CANNOT_BE_EMPTY = Error.New(200_00, "List cannot be empty.");
    public static Error E_LIST_MUST_BE_WITHIN_LENGTH(int min, int max) => (min, max) switch {
        (0, int.MaxValue) => Error.New(200_10,  "List must be empty."),
        (0, _)            => Error.New(200_11, $"List must have at most {max} items."),
        (_, int.MaxValue) => Error.New(200_12, $"List must have at least {min} items."),
         _                => Error.New(200_13, $"List must be between {min} and {max} items.")
    };
    public static readonly Error E_LIST_MUST_HAVE_UNIQUE_ITEMS = Error.New(200_20, "List must have unique items.");


    public static readonly Error E_VALUE_CANNOT_BE_EMPTY = Error.New(200_30, "Value cannot be empty.");
    public static Error E_VALUE_MUST_BE_WITHIN_LENGTH(int min, int max) => (min, max) switch {
        (0, int.MaxValue) => Error.New(200_40,  "Value must be empty."),
        (0, _)            => Error.New(200_41, $"Value must have at most {max} characters long."),
        (_, int.MaxValue) => Error.New(200_42, $"Value must have at least {min} characters long."),
         _                => Error.New(200_43, $"Value must be between {min} and {max} characters long.")
    };


    public static readonly Error E_GAME_DOES_NOT_EXIST          = Error.New(300_00, "Game does not exist.");
    public static readonly Error E_PLAYER_DOES_NOT_EXIST        = Error.New(300_10, "Player does not exist.");
    public static readonly Error E_TURN_DOES_NOT_EXIST          = Error.New(300_20, "Turn does not exist.");
    public static readonly Error E_GAME_ENGINE_DOES_NOT_EXIST   = Error.New(300_30, "Game engine does not exist.");


    public static readonly Error E_GAME_MUST_BE_RUNNING  = Error.New(400_00, "Game must be running.");
    public static readonly Error E_GAME_ALREADY_RUNNING  = Error.New(400_01, "Game already running.");
    public static readonly Error E_GAME_PAUSED           = Error.New(400_10, "Game paused.");
    public static readonly Error E_GAME_ALREADY_PAUSED   = Error.New(400_11, "Game already paused.");
    public static readonly Error E_GAME_LOCKED           = Error.New(400_20, "Game locked.");
    public static readonly Error E_GAME_ALREADY_LOCKED   = Error.New(400_21, "Game already locked.");
    public static readonly Error E_GAME_STOPED           = Error.New(400_30, "Game stoped.");
    public static readonly Error E_GAME_ALREADY_STOPED   = Error.New(400_31, "Game already stoped.");
}
