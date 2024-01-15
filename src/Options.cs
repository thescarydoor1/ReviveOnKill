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
        public static Configurable<int> MaxRevives;
        public static Configurable<float> SpearImmunityTime;
        public static Configurable<Color> SparkColor;

        public static Configurable<bool> EnableOnAllSlugcatsHack;

        public Options()
        {
            ResistantToRegularSpears = config.Bind("cfgResistantToRegularSpears", true);
            ResistantToExplosiveSpears = config.Bind("cfgResistantToExplosiveSpears", true);
            ResistantToElectricSpears = config.Bind("cfgResistantToElectricSpears", false);
            ScavengersInstantlyDie = config.Bind("cfgScavengersInstantlyDie", true);
            BleedoutTime = config.Bind("cfgBleedoutTime", 4f, new ConfigAcceptableRange<float>(0f, 60f));
            MaxRevives = config.Bind("cfgMaxRevives", 0, new ConfigAcceptableRange<int>(0, 99));
            SpearImmunityTime = config.Bind("cfgSpearImmunityTime", 0f, 
                new ConfigAcceptableRange<float>(0f, 10f));
            SparkColor = config.Bind("cfgSparkColor", Color.white);

            EnableOnAllSlugcatsHack = config.Bind("cfgEnableOnAllSlugcatsHack", false);
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
            var checkbox4 = new OpCheckBox(ScavengersInstantlyDie, 40, 600 - 157);

            var label5 = new OpLabel(40, 600 - 197, "Time until death after hit by a spear:");
            var entry5 = new OpUpdown(BleedoutTime, new Vector2(320, 600 - 201), 75);
            var units5 = new OpLabel(405, 600 - 197, "seconds");

            var label6 = new OpLabel(40, 600 - 232, "Max revives until perma death (0 for unlimited): ");
            var entry6 = new OpUpdown(MaxRevives, new Vector2(320, 600 - 237), 75);

            var label7 = new OpLabel(40, 600 - 267, "Post-revive spear immunity: ");
            var entry7 = new OpUpdown(SpearImmunityTime, new Vector2(320, 600 - 272), 75);
            var units7 = new OpLabel(405, 600 - 267, "seconds");

            var label8 = new OpLabel(40, 600 - 307, "Spark color");
            var colorpicker8 = new OpColorPicker(SparkColor, new Vector2(40, 600 - 460));

            var redColor = new Color(0.85f, 0.35f, 0.4f);
            var label9 = new OpLabel(70, 600 - 570, "Enable for all slugcats");
            label9.color = redColor;
            var checkbox9 = new OpCheckBox(EnableOnAllSlugcatsHack, 40, 600 - 573);
            checkbox9.colorEdge = redColor;
            checkbox9.description = "HACK: Other slugcats are not officially supported and are not guaranteed to work well";



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
                entry6,
                label7,
                entry7,
                units7,
                label8,
                colorpicker8,
                label9,
                checkbox9);
        }
    }
}
