# TrombSettings
Allows modders to show BepInEx config entries ingame    
<img src="https://imgur.com/9ZWj9Ez.jpg"/>

# For Modders
Example from the Highscore Accuracy mod    

```
        internal static ConfigEntry<AccType> accType;
        internal static ConfigEntry<bool> showLetterRank;
        internal static ConfigEntry<int> decimals;
        internal static ConfigEntry<bool> showAccIngame;
        internal static ConfigEntry<bool> showPBIngame;

        private void Awake()
        {
            Instance = this;

            // Start by binding to the BepInEx config.
            accType = Config.Bind("General", "Acc Type", AccType.BaseGame);
            showLetterRank = Config.Bind("General", "Show Letters", true);
            decimals = Config.Bind("General", "Decimal Places", 2);
            showAccIngame = Config.Bind("General", "Show acc in track", true);
            showPBIngame = Config.Bind("General", "Show PB in track", true);

            // Creates a new settings page
            TrombEntryList settings = TrombConfig.TrombSettings["Highscore Acc"];

            // Adds the config entries
            // Ordered from first to last added entry
            settings.Add(showLetterRank);
            settings.Add(accType);
            
            // For number values use the StepSliderConfig class
            //StepSliderConfig(min, max, increment, integerOnly, the numerical config entry)
            settings.Add(new StepSliderConfig(0, 4, 1, true, decimals));

            settings.Add(showAccIngame);
            settings.Add(showPBIngame);
        }
```
