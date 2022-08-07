namespace advisor.TurnProcessing;

using System.Collections.Generic;

public record FactionRecord(int? Number = null) {
    public string Name {
        get => GetStr("Name");
        set => SetStr("Name", value);
    }

    // ToDo: can be set multiple times
    public bool RewardTimes {
        get => HasFlag("RewardTimes");
        set => TogglFlag("RewardTimes", value);
    }

    public string Email {
        get => GetStr("Email");
        set => SetStr("Email", value);
    }

    public string Password {
        get {
            var value = GetStr("Password");
            return value == "none"
                ? null
                : value;
        }
        set => SetStr("Password", value ?? "none");
    }

    public string Battle {
        get => GetStr("Battle");
        set => SetStr("Battle", value);
    }

    public string Template {
        get => GetStr("Template");
        set => SetStr("Template", value);
    }

    // ToDo: can be set multiple times
    public int? Reward {
        get => GetInt("Reward");
        set => SetInt("Reward", value);
    }

    public bool SendTimes {
        get => HasFlag("SendTimes");
        set => TogglFlag("SendTimes", value);
    }

    public int? LastOrders {
        get => GetInt("LastOrders");
        set => SetInt("LastOrders", value);
    }

    public int? FirstTurn {
        get => GetInt("FirstTurn");
        set => SetInt("FirstTurn", value);
    }

    // ToDo: It is possible by GM to alter palyer factions with new units, skills and items.
    //       Each directive can appear multiple times in the players file.
    // public string Loc {
    //     get => GetStr("Loc");
    //     set => SetStr("Loc", value);
    // }

    // public string NewUnit {
    //     get => GetStr("NewUnit");
    //     set => SetStr("NewUnit", value);
    // }

    // public string Item {
    //     get => GetStr("Item");
    //     set => SetStr("Item", value);
    // }

    // Skill
    // Order


    public bool IsNew => Number == null;

    public Dictionary<string, string> Props { get; init; } = new ();

    private bool HasFlag(string name) => Props.ContainsKey(name);

    private void SetFlag(string name) {
        if (HasFlag(name)) {
            return;
        }

        Props.Add(name, null);
    }

    private void ClearFlag(string name) {
        if (!HasFlag(name)) {
            return;
        }

        Props.Remove(name);
    }

    private void TogglFlag(string name, bool set) {
        if (set) {
            SetFlag(name);
        }
        else {
            ClearFlag(name);
        }
    }

    private string GetStr(string name) => Props.Get(name);

    private void SetStr(string name, string value) {
        if (value == null && Props.ContainsKey(name)) {
            Props.Remove(name);
            return;
        }

        Props[name] = value;
    }

    private int? GetInt(string name) {
        var value = GetStr(name);
        return value == null
            ? null
            : int.Parse(value);
    }

    private void SetInt(string name, int? value) => SetStr(name, value?.ToString());
}
