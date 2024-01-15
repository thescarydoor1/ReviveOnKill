using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HUD;
using RWCustom;


namespace ReviveOnKill
{
    class ReviveCountHud : HudPart
    {
        private FLabel[] labels;
        private List<AbstractCreature> players;

        private float fade = 0f;
        private float lastFade = 0f;

        private bool isMultiplayer;

        public ReviveCountHud(HUD.HUD hud, List<AbstractCreature> players, bool isMultiplayer) : base(hud)
        {
            this.players = players ?? new List<AbstractCreature>();
            labels = new FLabel[this.players.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = new FLabel(Custom.GetDisplayFont(), "");
                hud.fContainers[1].AddChild(labels[i]);
            }

            this.isMultiplayer = isMultiplayer;
        }

        private bool ShouldDisplayLives(Player p)
        {
            return p != null && Plugin.IsArtificer(p) && Options.MaxRevives.Value > 0;
        }

        public override void Update()
        {
            if (isMultiplayer)
            {
                // Sad attempt to center the live counts.
                float labelStart = hud.rainWorld.screenSize.x / 2f
                                    - (labels.Length * 30f) / 2f + 20f;
                for (int i = 0; i < Math.Min(players.Count, labels.Length); i++)
                {
                    labels[i].x = labelStart + i * 30f;
                    labels[i].y = hud.rainWorld.screenSize.y - 30f;

                    PlayerState ps = players.ElementAt(i).state as PlayerState;
                    labels[i].color = PlayerGraphics.SlugcatColor(ps.slugcatCharacter);

                    Player p = players.ElementAt(i).realizedCreature as Player;
                    if (ShouldDisplayLives(p))
                    {
                        int lives = Options.MaxRevives.Value - Plugin.Data(p).numRevives;
                        labels[i].text = $"{lives}";
                    }
                }
            }
            else
            {
                if (hud.foodMeter != null)
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        float labelStart = hud.rainWorld.screenSize.x
                                            - hud.foodMeter.pos.x
                                            - 75f
                                            + (3 - labels.Length) * 30f;
                        labels[i].x = labelStart + i * 30f;
                        labels[i].y = hud.foodMeter.pos.y - 30;

                        PlayerState ps = players.ElementAt(i).state as PlayerState;
                        labels[i].color = PlayerGraphics.SlugcatColor(ps.slugcatCharacter);

                        Player p = players.ElementAt(i).realizedCreature as Player;
                        if (ShouldDisplayLives(p))
                        {
                            int lives = Options.MaxRevives.Value - Plugin.Data(p).numRevives;
                            labels[i].text = $"{lives}";
                        }

                    }
                    fade = hud.foodMeter.fade;
                }
                lastFade = fade;
            }
        }

        public override void Draw(float timeStacker)
        {
            if (!isMultiplayer)
            {
                foreach (var label in labels)
                {
                    label.alpha = Mathf.Lerp(lastFade, fade, timeStacker);
                }
            }
        }

        public override void ClearSprites()
        {
            foreach (FLabel label in labels)
            {
                label.RemoveFromContainer();
            }
        }
    }
}
