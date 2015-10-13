using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using color = System.Drawing;

namespace Kiwa_Akali
{
    class Program
    {
        public static Menu Menu, DrawM, Combom, Harassm;

        public static Spell.Targeted Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Targeted R;
        
 
        
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; } 

        } 

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Bootstrap.Init(null);
            Drawing.OnDraw += OnDraw;
            Game.OnTick += Game_OnTick;
            
            
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("Kiwa Akali v1.0");

            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Skillshot(SpellSlot.W,   700, SkillShotType.Circular);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Targeted(SpellSlot.R,  800);
            
          

            Menu = MainMenu.AddMenu("Kiwa Akali", "Kiwa.Akali");
            Menu.AddLabel("Kiwa Akali");
            Menu.AddSeparator();
            Menu.AddLabel("Version 1.0");

            Combom = Menu.AddSubMenu("Combo", "Comboakali");
            Combom.AddGroupLabel("Choose a spell to disable in combo :");
            Combom.AddSeparator();
            Combom.Add("UseQ", new CheckBox("Disable Q"));
            Combom.Add("UseE", new CheckBox("Disable E"));
            Combom.AddSeparator();  
            Combom.Add("UseW", new CheckBox("Disable W"));
            Combom.Add("UseR", new CheckBox("Disable R"));
            Combom.AddSeparator();
            Combom.Add("towerrange.r", new CheckBox("Dont Use R if target is under turret"));

            Harassm = Menu.AddSubMenu("Harass", "Harassakali");
            Harassm.AddGroupLabel("Choose a spell to disable in harass :");
            Harassm.AddSeparator();
            Harassm.Add("hrQ", new CheckBox("Disable Q"));          

            DrawM = Menu.AddSubMenu("Draws", "Drawakali");
            DrawM.AddLabel("Choose a spell to disable draw :");
            DrawM.AddSeparator();         
            DrawM.Add("drawdisableQ", new CheckBox("Disable Q", true));    
            DrawM.Add("drawdisableW", new CheckBox("Disable W", true));
            DrawM.AddSeparator();
            DrawM.Add("drawdisableR", new CheckBox("Disable R", true));


        }
               private static void Game_OnTick(EventArgs args)
               {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Combo();
                }
                 if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                 {
                    Harass();
                 }
            
        }



                 private static void Combo()
                 {
              
                  var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                  var useW = Combom["UseW"].Cast<CheckBox>().CurrentValue;
                  var useQ = Combom["UseQ"].Cast<CheckBox>().CurrentValue;
                  var useE = Combom["UseE"].Cast<CheckBox>().CurrentValue;
                  var useR = Combom["UseQ"].Cast<CheckBox>().CurrentValue;

                   if (useR && R.IsReady() && !target.IsDead && !target.IsZombie && target.IsValidTarget(R.Range))
                   {
                    if (UnderTheirTower(target))
                      if (Combom["towerrange.r"].Cast<CheckBox>().CurrentValue) return;
                          R.Cast(target);
                   }

                       if (useQ && Q.IsReady() && E.IsReady() && !target.IsDead && !target.IsZombie && target.IsValidTarget(Q.Range))
                       {
                          Q.Cast(target);
                       }

                        if (useW && W.IsReady() && !target.IsDead && !target.IsZombie && target.IsValidTarget(700))
                        {
                           W.Cast(target);
                        }

                        if (useE && E.IsReady() && !target.IsDead && !target.IsZombie && target.IsValidTarget(325))
                         {
                            E.Cast();
                         }

             }



                    private static void Harass()
                    {
                 
                     var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                     var useQ = Harassm["hrQ"].Cast<CheckBox>().CurrentValue;

                      if (useQ && Q.IsReady())
                      {
                         Q.Cast(target);
                      }
         
            }            
                  
     

                   private static void OnDraw(EventArgs args)
                   {
                    if (DrawM["drawdisableQ"].Cast<CheckBox>().CurrentValue)
                   {
                     new Circle() { Color = Color.DarkRed, BorderWidth = 1f, Radius = Q.Range }.Draw(_Player.Position);
                   }

                     if (DrawM["drawdisableW"].Cast<CheckBox>().CurrentValue)
                    {
                      new Circle() { Color = Color.DarkRed, BorderWidth = 1f, Radius = W.Range }.Draw(_Player.Position);
                    }

                      if (DrawM["drawdisableR"].Cast<CheckBox>().CurrentValue)
                      {
                       new Circle() { Color = Color.DarkRed, BorderWidth = 1f, Radius = R.Range }.Draw(_Player.Position);
                      }

            }



                   public static float GetDamage(SpellSlot spell, Obj_AI_Base target)
                   {
                    float ap = _Player.FlatMagicDamageMod + _Player.BaseAbilityDamage;
                    float ad = _Player.FlatMagicDamageMod + _Player.BaseAttackDamage;
                     if (spell == SpellSlot.Q)
                     {
                      if (!Q.IsReady())
                          return 0;
                           return _Player.CalculateDamageOnUnit(target, DamageType.Magical, 25f + 35f * (Q.Level - 1) + 100 / 100 * ad);
                     }
                            return 0;
            }


                                            
               private static bool UnderTheirTower(Obj_AI_Base target)
               {
                var tower =
                 ObjectManager
                  .Get<Obj_AI_Turret>()
                    .FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);

                      return tower != null;
            }
        }  
        }

    
  
 
                    
 
 


