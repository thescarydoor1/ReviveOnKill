using Menu.Remix.MixedUI;
using UnityEngine;

namespace ReviveOnKill
{
    sealed class Options : OptionInterface
    {
        public static Configurable<bool> ResistantToRegularSpears;
        public static Configurable<bool> ResistantToExplosiveSpears;
        public static Configurable<bool> ResistantToElectricSpears;
        public static Configurable<bool> ScavengersInstantlyDie;
        public static Configurable<float> BleedoutTime;
        public static Configurable<Color> SparkColor;
        
        public Options()
        {
            ResistantToRegularSpears = config.Bind("cfgResistantToRegularSpears", true);
            ResistantToExplosiveSpears = config.Bind("cfgResistantToExplosiveSpears", true);
            ResistantToElectricSpears = config.Bind("cfgResistantToElectricSpears", false);
            ScavengersInstantlyDie = config.Bind("cfgScavengersInstantlyDie", true);
            BleedoutTime = config.Bind("cfgBleedoutTime", 4f, new ConfigAcceptableRange<float>(0f, 60f));
            SparkColor = config.Bind("cfgSparkColor", Color.white);
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = new OpTab[] { new OpTab(this) };

            var titleLabel = new OpLabel(20, 600 - 30, "ReviveOnKill Settings", true);

            var label1 = new OpLabel(70, 600 - 60, "Regular spears don't stun");
            var checkbox1 = new OpCheckBox(ResistantToRegularSpears, 40, 600 - 63);

            var label2 = new OpLabel(70, 600 - 93, "Explosive spears don't instantly kill");
            var checkbox2 = new OpCheckBox(ResistantToExplosiveSpears, 40, 600 - 96);

            var label3 = new OpLabel(70, 600 - 123, "Electric spears don't stun");
            var checkbox3 = new OpCheckBox(ResistantToElectricSpears, 40, 600 - 126);

            var label4 = new OpLabel(70, 600 - 153, "Scavengers instantly die when health is depleted");
            var checkbox4 = new OpCheckBox(ScavengersInstantlyDie, 40, 600 - 156);

            var label5 = new OpLabel(40, 600 - 187, "Time until death after hit by a spear:");
            var entry5 = new OpUpdown(BleedoutTime, new Vector2(255, 600 - 191), 75);
            var units5 = new OpLabel(340, 600 - 187, "seconds");

            var label6 = new OpLabel(40, 600 - 217, "Spark color");
            var colorpicker6 = new OpColorPicker(SparkColor, new Vector2(40, 600 - 370));

            Tabs[0].AddItems(
                titleLabel,
                label1,
                checkbox1,
                label2,
                checkbox2,
                label3,
                checkbox3,
                label4,
                checkbox4,
                label5,
                entry5,
                units5,
                label6,
                colorpicker6);
        }
    }
}
