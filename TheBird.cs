using GTA;
using GTA.Native;
using System;
using System.Windows.Forms;

namespace GTA5M
{
    public class TheBird : Script
    {
        Keys activationKey;
        bool flippingOff = false;
        Ped fightingPed;
        Blip fightingPedBlip;
        Ped fleeingPed;

        public TheBird()
        {
            ScriptSettings localSettings = ScriptSettings.Load(".\\scripts\\TheBird.ini");

            Keys activationKeySetting;
            string activationKeyStr = localSettings.GetValue("Preferences", "ActivationKey", "J");
            if (Enum.TryParse(activationKeyStr, out activationKeySetting))
            {
                activationKey = activationKeySetting;
            }
            else
            {
                activationKey = Keys.J;
            }

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Tick += OnTick;
            Interval = 100;
        }

        void OnKeyDown(object o, KeyEventArgs e)
        {
            // User is pressing the activation key
            if (e.KeyCode == activationKey)
            {
                // If not currently fighting a ped and the player is alive and available
                if (fightingPed == null && !flippingOff && Game.Player.IsAlive && Game.Player.CanStartMission && Game.Player.Character.Weapons.Current.Hash == WeaponHash.Unarmed)
                {
                    flippingOff = true;

                    // Play middle finger animation
                    Function.Call(Hash._PLAY_AMBIENT_SPEECH1, Game.Player.Character, "GENERIC_FUCK_YOU", "SPEECH_PARAMS_FORCE", 0);
                    Game.Player.Character.Task.PlayAnimation("mp_player_int_upperfinger", "mp_player_int_finger_02", 2, 750, true, 1);
                    Wait(750);
                    Game.Player.Character.Task.PlayAnimation("mp_player_int_upperfinger", "mp_player_int_finger_02_exit", 2, 750, true, 1);

                    // Is player aiming at ped
                    Entity aimedEntity = Game.Player.GetTargetedEntity();
                    if (aimedEntity.IsPed())
                    {
                        // Give them some time to react...
                        Wait(250);

                        Ped aimedPed = (Ped)aimedEntity;

                        // If you're flipping off a cop...
                        if (aimedPed.Type() == PedType.Cop || aimedPed.Type() == PedType.SWAT || aimedPed.Type() == PedType.Army)
                        {
                            // Set wanted level to 1 star if you didn't have any already
                            if (Game.Player.WantedLevel == 0)
                            {
                                Game.Player.WantedLevel = 1;
                            }
                        }
                        else if (!aimedPed.IsFleeing)
                        {
                            // Prep the ped
                            bool pedWillFight = false;
                            aimedPed.Task.ClearAllImmediately();
                            aimedPed.AlwaysKeepTask = true;

                            // Use a random number generator to decide what to do
                            Random randomNumberGenerated = new Random();

                            // If you're flipping off a man...
                            if (aimedPed.Gender == Gender.Male)
                            {
                                int randomNumber = randomNumberGenerated.Next(4);
                                // Give him a golf club
                                if (randomNumber == 2)
                                {
                                    aimedPed.Weapons.Give(WeaponHash.GolfClub, 1, true, true);
                                }
                                // Give him a switchblade
                                else if (randomNumber == 1)
                                {
                                    aimedPed.Weapons.Give(WeaponHash.SwitchBlade, 30, true, true);
                                }
                                // Give him a bat
                                else if (randomNumber == 0)
                                {
                                    aimedPed.Weapons.Give(WeaponHash.Bat, 1, true, true);
                                }

                                pedWillFight = true;
                            }
                            // If you're flipping off a woman...
                            else
                            {
                                int randomNumber = randomNumberGenerated.Next(2);
                                // Make her fight
                                pedWillFight = (randomNumber == 2);
                            }

                            // Set the ped to fight you
                            if (pedWillFight)
                            {
                                fightingPed = aimedPed;
                                fightingPed.IsPersistent = true;
                                fightingPed.Task.FightAgainst(Game.Player.Character);
                                Function.Call(Hash._PLAY_AMBIENT_SPEECH1, fightingPed, "GENERIC_INSULT_MED", "SPEECH_PARAMS_FORCE", 0);

                                // Add a blip for the ped to the map
                                fightingPedBlip = fightingPed.AddBlip();
                            }
                            // Set the ped to flee from you
                            else
                            {
                                fleeingPed = aimedPed;
                                fleeingPed.IsPersistent = true;
                                fleeingPed.Task.FleeFrom(Game.Player.Character);
                                Function.Call(Hash._PLAY_AMBIENT_SPEECH1, aimedPed, "GENERIC_SHOCKED_HIGH", "SPEECH_PARAMS_FORCE", 0);
                            }
                        }
                    }
                }
            }
        }

        private void OnKeyUp(object o, KeyEventArgs e)
        {
            // User released the activation key
            if (e.KeyCode == activationKey)
            {
                flippingOff = false;
            }
        }

        private void OnTick(object o, EventArgs e)
        {
            // If a fighting ped is dead or you moved to far away from them
            if (fightingPed != null && (!Game.Player.IsAlive || !Game.Player.CanStartMission || !fightingPed.IsAlive || !fightingPed.IsNearEntity(Game.Player.Character, new GTA.Math.Vector3(100, 100, 100))))
            {
                // Destroy ped
                fightingPed.Destroy();
                fightingPed = null;

                // Destroy blip
                if (fightingPedBlip != null)
                {
                    fightingPedBlip.Remove();
                    fightingPedBlip = null;
                }
            }

            // If a fleeing ped is dead or you moved to far away from them
            if (fleeingPed != null && (!Game.Player.IsAlive || !Game.Player.CanStartMission || !fleeingPed.IsAlive || !fleeingPed.IsNearEntity(Game.Player.Character, new GTA.Math.Vector3(100, 100, 100)))) {
                // Destroy ped
                fleeingPed.Destroy();
                fleeingPed = null;
            }
        }
    }
}