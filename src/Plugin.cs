﻿using BepInEx;
using MoreSlugcats;
using Noise;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using UnityEngine;
using Random = UnityEngine.Random;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ReviveOnKill;

[BepInPlugin("thescarydoor.reviveonkill", "ReviveOnKill", "1.0.0")]
sealed class Plugin : BaseUnityPlugin
{
    private const int TIMER_DISABLED = -1;
    private const int TIMER_ENABLED = 0;
    bool atLeastOneSlugcatIsArtificer = false;

    // Each player has their own death timer.
    sealed class PlayerData
    {
        public int timePassed = TIMER_DISABLED;
    }
    static readonly ConditionalWeakTable<Player, PlayerData> cwt = new();
    static PlayerData Data(Player p) => cwt.GetValue(p, _ => new());

    private bool IsArtificer(Player p)
    {
        return ModManager.MSC && p.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
    }

    private int MaxTime()
    {
        return Math.Max(1, (int)(Options.BleedoutTime.Value * 40)); // 40 ticks/second
    }

    public void OnEnable()
    {
        // Startup and Cleanup
        On.GameSession.ctor += GameSession_ctor;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorldGame.ShutDownProcess += RainWorld_ShutDownProcess;

        On.Creature.Die += Creature_Die;
        On.Creature.Violence += Creature_Violence;
        On.Player.ctor += Player_ctor;
        On.Player.SpearStick += Player_SpearStick;
        On.Player.Update += Player_Update;
        On.Scavenger.Violence += Scavenger_Violence;
        On.Spear.HitSomething += Spear_HitSomething;
    }

    private void Creature_Violence(
        On.Creature.orig_Violence orig, 
        Creature self, 
        BodyChunk source, 
        UnityEngine.Vector2? directionAndMomentum, 
        BodyChunk hitChunk, 
        PhysicalObject.Appendage.Pos hitAppendage, 
        Creature.DamageType type, 
        float damage, 
        float stunBonus)
    {
        // Remove spear stun.
        if (self is Player p
            && IsArtificer(p)
            && type == Creature.DamageType.Stab 
            && Options.ResistantToRegularSpears.Value)
        {
            stunBonus = 0;
        }
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    private void Creature_Die(On.Creature.orig_Die orig, Creature self)
    {
        if (self is Scavenger
            && self.killTag?.realizedCreature is Player p
            && IsArtificer(p)
            && Data(p).timePassed != TIMER_DISABLED)
        {
            Data(p).timePassed = TIMER_DISABLED;
            self.room.PlaySound(SoundID.Snail_Pop, self.killTag.realizedCreature.firstChunk.pos);
        }
        orig(self);
    }
    private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (IsArtificer(self))
        {
            Data(self).timePassed = TIMER_DISABLED;
            atLeastOneSlugcatIsArtificer = true;
        }
    }

    private bool Player_SpearStick(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
    {
        if (IsArtificer(self))
        {
            if (source is ExplosiveSpear && Options.ResistantToExplosiveSpears.Value)
            {
                return false;
            }
            if (source is ElectricSpear electricSpear && Options.ResistantToElectricSpears.Value)
            {
                electricSpear.Zap();
                return false;
            }
        }
        return orig(self, source, dmg, chunk, appPos, direction);
    }

    // Based on the effect when Artificer double jumps too many times.
    private void Spark(Player player)
    {
        // Have a slight rampup for generating sparks.
        float rampup = Custom.LerpMap(Data(player).timePassed, 0, MaxTime(), 0.8f, 1);
        if (Random.value > (0.75f / rampup))
        {
            player.room.AddObject(new Explosion.ExplosionSmoke(player.mainBodyChunk.pos, Custom.RNV() * 2f * Random.value, 1f));
        }
        if (Random.value > (0.5f / rampup))
        {
            player.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV(), Options.SparkColor.Value, null, 4, 8));
        }
    }

    // Based on https://github.com/Dual-Iron/hydrogen-rocks/blob/master/src/Plugin.cs
    private void Explode(Player player)
    {
        Vector2 pos = player.firstChunk.pos;

        // Explosion effect
        Color softRed = new(r: 1.0f, g: 0.4f, b: 0.3f);
        player.room.AddObject(new SootMark(player.room, pos, rad: 80f, bigSprite: true));
        player.room.AddObject(new Explosion.ExplosionLight(pos, rad: 150f, alpha: 1.0f, lifeTime: 7, lightColor: softRed));
        player.room.AddObject(new Explosion.ExplosionLight(pos, rad: 100f, alpha: 1.0f, lifeTime: 3, lightColor: Color.white));
        player.room.AddObject(new ExplosionSpikes(player.room, pos, _spikes: 14, innerRad: 30.0f, lifeTime: 9.0f, width: 7.0f, length: 170.0f, color: softRed));
        player.room.AddObject(new ShockWave(pos, size: 330.0f, intensity: 0.045f, lifeTime: 5));

        // Actual Explosion.
        player.room.AddObject(new Explosion(
            room: player.room,
            sourceObject: player,
            pos: pos,
            lifeTime: 7,
            rad: 100f,
            force: 5f,
            damage: 2f,
            stun: 50f,
            deafen: 0.25f,
            killTagHolder: player,
            killTagHolderDmgFactor: 0.7f,
            minStun: 50f,
            backgroundNoise: 1f
        ));
        player.room.ScreenMovement(pos, bump: Vector2.zero, shake: 1.3f);
        player.room.PlaySound(SoundID.Fire_Spear_Explode, pos);
        player.room.InGameNoise(new InGameNoise(pos, 8000f, player, 1f));
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (IsArtificer(self))
        {
            if (Data(self).timePassed != TIMER_DISABLED && Data(self).timePassed < MaxTime())
            {
                Data(self).timePassed++;
                Spark(self);
            }
            else if (Data(self).timePassed >= MaxTime())
            {
                Data(self).timePassed = TIMER_DISABLED;
                Explode(self);
                self.Die();
            }
        }
        orig(self, eu);
    }

    private void Scavenger_Violence(
        On.Scavenger.orig_Violence orig, 
        Scavenger self, 
        BodyChunk source, 
        Vector2? directionAndMomentum, 
        BodyChunk hitChunk, 
        PhysicalObject.Appendage.Pos hitAppendage, 
        Creature.DamageType type, 
        float damage, 
        float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        
        // Scavengers sometimes have a delayed death. Force them to always die instantly.
        if (atLeastOneSlugcatIsArtificer 
            && (self.State as HealthState).health < 0f
            && Options.ScavengersInstantlyDie.Value)
        {
            self.Die();
        }
    }

    private bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        float oldBonus = self.spearDamageBonus;
        if (result.obj is Player p && IsArtificer(p))
        {
            self.spearDamageBonus = 0f;
            if (Data(p).timePassed == TIMER_DISABLED)
            {
                Data(p).timePassed = TIMER_ENABLED;
            }
        }

        bool ret = orig(self, result, eu);
        self.spearDamageBonus = oldBonus;
        return ret;
    }

    private void GameSession_ctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }
    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        MachineConnector.SetRegisteredOI("thescarydoor.reviveonkill", new Options());
        Logger.LogDebug("ReviveOnKill Enabled");
    }

    private void RainWorld_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }

    private void ClearMemory()
    {
        atLeastOneSlugcatIsArtificer = false;
    }
}