using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using GuTenTak.TwistedFate;
using SharpDX;
using EloBuddy.SDK.Constants;

namespace GuTenTak.TwistedFate
{
    internal class Program
    {
        public const string ChampionName = "TwistedFate";
        public static Menu Menu, ModesMenu1, ModesMenu2, ModesMenu3, DrawMenu;
        public static int SkinBase;
        public static Item Youmuu = new Item(ItemId.Youmuus_Ghostblade);
        public static Item Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
        public static Item Cutlass = new Item(ItemId.Bilgewater_Cutlass);
        public static Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static Item Simitar = new Item(ItemId.Mercurial_Scimitar);
        public static Item hextech = new Item(ItemId.Hextech_Gunblade, 700);


        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool AutoQ { get; protected set; }
        public static float Manaah { get; protected set; }
        public static object GameEvent { get; private set; }

        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active R;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnStart;
        }


        static void Game_OnStart(EventArgs args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnBuffGain += Common.OnBuffGain;
            Game.OnTick += OnTick;
            Orbwalker.OnPreAttack += Common.QAA;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            SkinBase = Player.Instance.SkinId;
            // Item
            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Q = new Spell.Skillshot(SpellSlot.Q, 1450, SkillShotType.Linear, 0, 1000, 40)
                {
                    AllowedCollisionCount = int.MaxValue
                };
                W = new Spell.Active(SpellSlot.W);
                R = new Spell.Active(SpellSlot.R, 5500);





                Bootstrap.Init(null);
                Chat.Print("GuTenTak Addon Loading Success", Color.Green);


                Menu = MainMenu.AddMenu("GuTenTak TwistedFate", "TwistedFate");
                Menu.AddSeparator();
                Menu.AddLabel("GuTenTak TwistedFate Addon");

                var Enemies = EntityManager.Heroes.Enemies.Where(a => !a.IsMe).OrderBy(a => a.BaseSkinName);
                ModesMenu1 = Menu.AddSubMenu("Menu", "Modes1TwistedFate");
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Combo Configs");
                ModesMenu1.Add("ComboQ", new CheckBox("Use Q on Combo", true));
                ModesMenu1.Add("ComboYellowCard", new CheckBox("Pick A Yello Card on Combo", true));
                ModesMenu1.Add("RYellow", new CheckBox("Use Auto Pick A Yellow Card on R", true));
                ModesMenu1.Add("ComboWRed", new KeyBind("Use Red Card", false, KeyBind.BindTypes.HoldActive, 'T'));
                ModesMenu1.Add("ComboWBlue", new KeyBind("Use Blue Card", false, KeyBind.BindTypes.HoldActive, 'E'));
                ModesMenu1.Add("ComboWYellow", new KeyBind("Use Yellow Card", false, KeyBind.BindTypes.HoldActive, 'S'));
                ModesMenu1.Add("WHumanizer", new CheckBox("Pick A Card Humanizer", true));
                ModesMenu1.Add("WHumanizerms", new Slider("Pick A Card Humanizer (ms)", 250, 0, 250));
                ModesMenu1.Add("WHumanizerrandom", new Slider("Pick A Card Humanizer (Random Min)", 0, 0, 125));
                ModesMenu1.Add("WHumanizerrandom2", new Slider("Pick A Card Humanizer (Random MAX)", 250, 0, 250));

                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Auto Harass Configs");
                ModesMenu1.Add("AutoHarass", new CheckBox("Auto Q immobile target", true));
                ModesMenu1.Add("ManaAuto", new Slider("Use Auto Harass Mana %", 40));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Harass Configs");
                ModesMenu1.Add("HarassQ", new CheckBox("Use Q on Harass", true));
                ModesMenu1.Add("ManaHQ", new Slider("Use Q Harass Mana %", 60));
                ModesMenu1.Add("HarassW", new CheckBox("Use W on Harass", true));
                ModesMenu1.Add("ManaHW", new Slider("Use W Harass Mana %", 60));
                ModesMenu1.Add("HarassPick", new ComboBox("Use Harass Pick A Card", 0, "Blue", "Red", "Yellow"));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Kill Steal Configs");
                ModesMenu1.Add("KS", new CheckBox("Use KillSteal", true));
                ModesMenu1.Add("KQ", new CheckBox("Use Q on KillSteal", true));

                ModesMenu2 = Menu.AddSubMenu("Farm", "Modes2TwistedFate");
                ModesMenu2.AddLabel("Last Hit Config");
                ModesMenu2.AddSeparator();
                ModesMenu2.Add("LastBlue", new CheckBox("Pick A Blue Card", true));
                ModesMenu2.Add("ManaLast", new Slider("Mana Under %", 40));
                ModesMenu2.AddSeparator();
                ModesMenu2.AddLabel("Lane Clear Config");
                ModesMenu2.AddSeparator();
                ModesMenu2.Add("FarmQ", new CheckBox("Use Q on LaneClear", true));
                ModesMenu2.Add("ManaLQ", new Slider("Use Q Mana %", 40));
                ModesMenu2.Add("MinionLC", new Slider("Use Q Min Minions on LaneClear", 3, 1, 5));
                ModesMenu2.Add("FarmW", new CheckBox("Use W on LaneClear", true));
                ModesMenu2.Add("ClearPick", new ComboBox("Use Clear Pick A Card", 1, "Red", "Blue"));
                ModesMenu2.Add("ManaLW", new Slider("Use W Mana %", 40));
                ModesMenu2.AddSeparator();
                ModesMenu2.AddLabel("Jungle Clear Config");
                ModesMenu2.AddSeparator();
                ModesMenu2.Add("JungleQ", new CheckBox("Use Q on JungleClear", true));
                ModesMenu2.Add("ManaJQ", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleW", new CheckBox("Use W on JungleClear", true));
                ModesMenu2.Add("JungleClearPick", new ComboBox("Use Jungle Pick A Card", 1, "Red", "Blue", "Yellow"));
                ModesMenu2.Add("ManaJW", new Slider("Mana %", 40));

                ModesMenu3 = Menu.AddSubMenu("Misc", "Modes3TwistedFate");
                //ModesMenu3.Add("AntiGap", new CheckBox("AntiGap - Pick Golden Card", true));

                ModesMenu3.AddLabel("Item Usage on Combo");
                ModesMenu3.Add("useYoumuu", new CheckBox("Use Youmuu", true));
                ModesMenu3.Add("usehextech", new CheckBox("Use Hextech", true));
                ModesMenu3.Add("useBotrk", new CheckBox("Use Botrk & Cutlass", true));
                ModesMenu3.Add("useQss", new CheckBox("Use QuickSilver", true));
                ModesMenu3.Add("minHPBotrk", new Slider("Min health to use Botrk %", 80));
                ModesMenu3.Add("enemyMinHPBotrk", new Slider("Min enemy health to use Botrk %", 80));

                ModesMenu3.AddLabel("QSS Configs");
                ModesMenu3.Add("Qssmode", new ComboBox(" ", 0, "Auto", "Combo"));
                ModesMenu3.Add("Stun", new CheckBox("Stun", true));
                ModesMenu3.Add("Blind", new CheckBox("Blind", true));
                ModesMenu3.Add("Charm", new CheckBox("Charm", true));
                ModesMenu3.Add("Suppression", new CheckBox("Suppression", true));
                ModesMenu3.Add("Polymorph", new CheckBox("Polymorph", true));
                ModesMenu3.Add("Fear", new CheckBox("Fear", true));
                ModesMenu3.Add("Taunt", new CheckBox("Taunt", true));
                ModesMenu3.Add("Silence", new CheckBox("Silence", false));
                ModesMenu3.Add("QssDelay", new Slider("Use QSS Delay(ms)", 250, 0, 1000));

                ModesMenu3.AddLabel("QSS Ult Configs");
                ModesMenu3.Add("ZedUlt", new CheckBox("Zed R", true));
                ModesMenu3.Add("VladUlt", new CheckBox("Vladimir R", true));
                ModesMenu3.Add("FizzUlt", new CheckBox("Fizz R", true));
                ModesMenu3.Add("MordUlt", new CheckBox("Mordekaiser R", true));
                ModesMenu3.Add("PoppyUlt", new CheckBox("Poppy R", true));
                ModesMenu3.Add("QssUltDelay", new Slider("Use QSS Delay(ms) for Ult", 250, 0, 1000));

                ModesMenu3.AddLabel("Skin Hack");
                ModesMenu3.Add("skinhack", new CheckBox("Activate Skin hack", false));
                ModesMenu3.Add("skinId", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9"));

                DrawMenu = Menu.AddSubMenu("Draws", "DrawTwistedFate");
                DrawMenu.Add("drawA", new CheckBox(" Draw Real AA", true));
                DrawMenu.Add("drawQ", new CheckBox(" Draw Q", true));
                DrawMenu.Add("drawR", new CheckBox(" Draw R", false));

            }

            catch (Exception e)
            {

            }

        }
        private static void Game_OnDraw(EventArgs args)
        {

            try
            {
                if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
                {
                    if (Q.IsReady() && Q.IsLearned)
                    {
                        Circle.Draw(Color.White, Q.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
                {
                    if (R.IsReady() && R.IsLearned)
                    {
                        Circle.Draw(Color.White, R.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawA"].Cast<CheckBox>().CurrentValue)
                {
                    Circle.Draw(Color.LightGreen, 590, Player.Instance.Position);
                }
            }
            catch (Exception e)
            {

            }
        }
        static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                //var AutoHarass = ModesMenu1["AutoHarass"].Cast<CheckBox>().CurrentValue;
                //var ManaAuto = ModesMenu1["ManaAuto"].Cast<Slider>().CurrentValue;
                var RedCard = ModesMenu1["ComboWRed"].Cast<KeyBind>().CurrentValue;
                var YellowCard = ModesMenu1["ComboWYellow"].Cast<KeyBind>().CurrentValue;
                var BlueCard = ModesMenu1["ComboWBlue"].Cast<KeyBind>().CurrentValue;

                if (YellowCard)
                {
                    Common.CardSelector.StartSelecting(Common.Cards.Yellow);
                }
                if (RedCard)
                {
                    Common.CardSelector.StartSelecting(Common.Cards.Red);
                }
                if (BlueCard)
                {
                    Common.CardSelector.StartSelecting(Common.Cards.Blue);
                }

                /*
                if (AutoHarass && ManaAuto <= _Player.ManaPercent)
                    {
                        Common.AutoQ();
                    }*/
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Common.Combo();
                    Common.ItemUsage();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Common.Harass();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {

                    Common.LaneClear();

                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {

                    Common.JungleClear();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    Common.LastHit();

                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    Common.Flee();

                }
            }
            catch (Exception e)
            {

            }
        }

        public static void OnTick(EventArgs args)
        {
            Common.KillSteal();
            Common.AutoQ();
            Common.Skinhack();
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name.ToLower() == "gate" && ModesMenu1["RYellow"].Cast<CheckBox>().CurrentValue)
            {
                Common.CardSelector.StartSelecting(Common.Cards.Yellow);
            }
        }

    }
}
