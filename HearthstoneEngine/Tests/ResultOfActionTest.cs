namespace Tests
{
    using System.Linq;
    using Domain;
    using Infrastructure;
    using NUnit.Framework;

    [TestFixture]
    public class ResultOfActionTest
    {
        private GameEngine _testee;
        private readonly Player _player = Player.Empty;

        [SetUp]
        public void RunBeforeEachTestMethod()
        {
            _testee = new GameEngine();
        }

        public bool IsSimilar(Minion onboardMinion, MinionCard card)
        {
            return onboardMinion.Attack == card.Attack &&
                   onboardMinion.Health == card.Health &&
                   onboardMinion.HasStatus(card.Status);
        }

        [Test]
        public void
            GivenBoardIsEmpty_WhenSummonMinionActionRequested_ThenCardRemovedFromHandAndManaDecresedAndMinionOboard()
        {
            var minionCard = new MinionCard(5, 1, 1);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, remainingMana: x => 7, hand: x => x.AddCard(minionCard)));

            var action = new SummonAction(_player, minionCard);

            var newGameState = _testee.ApplyAction(gameState, action);

            Assert.IsFalse(newGameState.Of(_player).Hand.Any());
            Assert.AreEqual(7 - 5, newGameState.Of(_player).RemainingMana);
            Assert.IsTrue(newGameState.Of(_player).Minions.Count() == 1);
        }

        [Test]
        public void GivenBothPlayersHaveMinionOnboard_WhenMinionAttacked_ThenHealthsAreUpdated()
        {
            const int minionAttack = 5;
            const int minionHealth = 10;

            var player1 = Player.Empty;
            var player2 = Player.Empty;

            var attacker = new Minion(minionAttack, minionHealth);
            var target = new Minion(minionAttack, minionHealth);

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(player1, minions: x => x.Add(attacker)),
                PlayerGameState.Empty.With(player2, minions: x => x.Add(target)));

            var action = new AttackAction(player1, attacker, target);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(player1).Minions.First().Health == minionHealth - minionAttack);
            Assert.IsTrue(newState.Of(player2).Minions.First().Health == minionHealth - minionAttack);
        }

        [Test]
        public void GivenBothPlayersHaveMinionOnboard_WhenMinionAttackKilledEachOther_ThenBothPlayersHaveZeroOnboard()
        {
            const int minionAttack = 5;
            const int minionHealth = 5;

            var player1 = Player.Empty;
            var player2 = Player.Empty;

            var attacker = new Minion(minionAttack, minionHealth);
            var target = new Minion(minionAttack, minionHealth);

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(player1, minions: x => x.Add(attacker)),
                PlayerGameState.Empty.With(player2, minions: x => x.Add(target)));

            var action = new AttackAction(player1, attacker, target);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(player1).Minions.Count == 0);
            Assert.IsTrue(newState.Of(player2).Minions.Count == 0);
        }

        [Test]
        public void GivenSenjinInHand_WhenSummonSenjin_ThenSenjinOnboard()
        {
            var senjin = Minions.SenjinShieldmasta;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(senjin), remainingMana: x => senjin.ManaCost));

            var action = new SummonAction(_player, senjin);

            var newState = _testee.ApplyAction(gameState, action);

            var onboardSenjin = newState.Of(_player).Minions.FirstOrDefault();

            Assert.IsNotNull(onboardSenjin);
            Assert.AreEqual(senjin.Attack, onboardSenjin.Attack);
            Assert.AreEqual(senjin.Health, onboardSenjin.Health);
        }

        [Test]
        public void GivenElvenArcherInHand_WhenSummonArcherTargetingOpponent_ThenOpponentHasMinusOneHealth()
        {
            var archer = Minions.ElvenArcher;
            var bFace = Face.Empty;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(archer), remainingMana: x => archer.ManaCost),
                PlayerGameState.Empty.With(face: x => bFace));

            var action = new SummonBattlecryTargetable(_player, archer, bFace);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Minions.Count == 1);
            Assert.IsTrue(newState.OfOpponent(_player).Face.Health == bFace.Health - 1);
        }

        [Test]
        public void GivenVoodooDoctorInHand_WhenSummonDoctorTargetingSelf_ThenSelfHasTwoHpHealed()
        {
            var doctor = Minions.VoodooDoctor;
            var face = Face.Empty.With(health: 15);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(doctor), remainingMana: x => doctor.ManaCost,
                    face: x => face));

            var action = new SummonBattlecryTargetable(_player, doctor, face);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Minions.Count == 1);
            Assert.IsTrue(newState.Of(_player).Face.Health == face.Health + 2);
        }

        [Test]
        public void GivenFrostElementalInHand_WhenSummonTargetingSelf_ThenSelfHasStatusFrozen()
        {
            var elemental = Minions.FrostElemental;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(elemental), remainingMana: x => elemental.ManaCost));

            var action = new SummonBattlecryTargetable(_player, elemental, gameState.Of(_player).Face);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Minions.Count == 1);
            Assert.IsTrue(newState.Of(_player).Face.HasStatus(Status.Frozen));
        }

        [Test]
        public void GivenDarkscaleHealerInHand_WhenSummoned_ThenAllFriendlyCharsHealedByTwo()
        {
            var healer = Minions.DarkscaleHealer;
            var face = Face.Empty.With(health: 15);
            var minion = Minion.Empty.With(health: 5, healthBuffCurrent: 3);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, x => face, x => x.Add(minion), hand: x => x.AddCard(healer), remainingMana: x => healer.ManaCost));

            var action = new SummonBattlecryTargetless(_player, healer, 1);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Face.Health == face.Health + 2);
            Assert.IsTrue(newState.Of(_player).Minions.First().Health == minion.Health + 2);
        }

        [Test]
        public void GivenDreadInfernalnHand_WhenSummoned_ThenAllCharactersInPlayRecieveOneDamage()
        {
            var infernal = Minions.DreadInfernal;
            var aFace = Face.Empty;
            var bFace = Face.Empty;
            var aMinion = Minion.Empty.With(health: 2, healthCurrent: 2);
            var bMinion = Minion.Empty.With(health: 3, healthCurrent: 3);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, x => aFace, x => x.Add(aMinion), hand: x => x.AddCard(infernal), remainingMana: x => infernal.ManaCost),
                PlayerGameState.Empty.With(Player.Empty, x => bFace, x => x.Add(bMinion)));

            var action = new SummonBattlecryTargetless(_player, infernal, desiredBoardPosition: 1);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Face.Health == aFace.Health - 1);
            Assert.IsTrue(newState.Of(_player).Minions.First().Health == aMinion.Health - 1);
            Assert.IsTrue(newState.OfOpponent(_player).Face.Health == bFace.Health - 1);
            Assert.IsTrue(newState.OfOpponent(_player).Minions.First().Health == bMinion.Health - 1);

            Assert.IsTrue(newState.Of(_player).Minions.Last().Health == infernal.Health);
        }

        [Test]
        public void GivenNightBladeInHand_WhenSummoned_ThenEnemyFaceRecievesThreeDamage()
        {
            var nightBlade = Minions.NightBlade;
            var bFace = Face.Empty;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(nightBlade), remainingMana: x => nightBlade.ManaCost),
                PlayerGameState.Empty.With(face: x => bFace));

            var action = new SummonBattlecryTargetless(_player, nightBlade);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.OfOpponent(_player).Face.Health == bFace.Health - 3);
        }

        [Test]
        public void GivenNoviceEngineerInHand_WhenSummoned_ThenPlayerDrawsOneCard()
        {
            var novice = Minions.NoviceEngineer;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(novice), remainingMana: x => novice.ManaCost,
                    deck: x => x.With(Minions.BoulderfistOgre.Yield())));

            var action = new SummonBattlecryTargetless(_player, novice);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Hand.Single() == Minions.BoulderfistOgre);
            Assert.IsTrue(newState.Of(_player).Deck.IsEmpty);
        }

        [Test]
        public void GivenSpellbreakerInHand_WhenSummonedTargetingMinion_ThenMinionGotEffectsDispelled()
        {
            var spellbreaker = Minions.Spellbreaker;
            const Status status = Status.Taunt | Status.Frozen;
            var enchantedMinion = new Minion(0, 5, status);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(spellbreaker), minions: x => x.Add(enchantedMinion), remainingMana: x => spellbreaker.ManaCost));

            var action = new SummonBattlecryTargetable(_player, spellbreaker, enchantedMinion, 1);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsFalse(newState.Of(_player).Minions.First().HasStatus(status));
        }

        [Test]
        public void GivenRaptorOnboard_WhenRaptorIsBuffedAndRecievesOneDamageAndGetsDispelled_ThenRaptorIsUndamaged()
        {
            var raptor = new Minion(Minions.BloodfenRaptor);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player,
                    hand: x => x.With(new[] { Minions.ShatteredSunCleric, Minions.ElvenArcher, Minions.Spellbreaker }),
                    minions: x => x.Add(raptor), remainingMana: x => 10));

            var buffAction = new SummonBattlecryTargetable(_player, Minions.ShatteredSunCleric, raptor);
            var newState = _testee.ApplyAction(gameState, buffAction);
            var buffedRaptor = newState.Of(_player).Minions.Last();

            var oneDamageAction = new SummonBattlecryTargetable(_player, Minions.ElvenArcher, buffedRaptor);
            newState = _testee.ApplyAction(newState, oneDamageAction);
            var damagedBuffedRaptor = newState.Of(_player).Minions.Last();

            var silenceAction = new SummonBattlecryTargetable(_player, Minions.Spellbreaker, damagedBuffedRaptor);
            newState = _testee.ApplyAction(newState, silenceAction);
            var silencedRaptor = newState.Of(_player).Minions.Last();

            Assert.IsFalse(silencedRaptor.IsDamaged);
        }

        [Test]
        public void GivenRaptorOnboard_WhenRaptorRecievesOneDamageAndGetsBuffedAndGetsDispelled_ThenRaptorIsDamaged()
        {
            var raptor = new Minion(Minions.BloodfenRaptor);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player,
                    hand: x => x.With(new[] { Minions.ShatteredSunCleric, Minions.ElvenArcher, Minions.Spellbreaker }),
                    minions: x => x.Add(raptor), remainingMana: x => 10));

            var oneDamageAction = new SummonBattlecryTargetable(_player, Minions.ElvenArcher, raptor);
            var newState = _testee.ApplyAction(gameState, oneDamageAction);
            var damagedRaptor = newState.Of(_player).Minions.Last();

            var buffAction = new SummonBattlecryTargetable(_player, Minions.ShatteredSunCleric, damagedRaptor);
            newState = _testee.ApplyAction(newState, buffAction);
            var damagedBuffedRaptor = newState.Of(_player).Minions.Last();

            var silenceAction = new SummonBattlecryTargetable(_player, Minions.Spellbreaker, damagedBuffedRaptor);
            newState = _testee.ApplyAction(newState, silenceAction);
            var silencedRaptor = newState.Of(_player).Minions.Last();

            Assert.IsTrue(silencedRaptor.IsDamaged);
            Assert.IsTrue(damagedBuffedRaptor.IsDamaged);
        }

        [Test]
        public void GivenMinionIsDamagedByOne_WhenHealedByFive_ThenOnlyOneHealthIsRestored()
        {
            var damagedMinion = new Minion(0, 5, healthCurrent: 4);
            var gameState = GameState.Empty.With(
               PlayerGameState.Empty.With(_player, hand: x => x.AddCard(Minions.VoodooDoctor), minions: x => x.Add(damagedMinion), remainingMana: x => 10));
            var healAction = new SummonBattlecryTargetable(_player, Minions.VoodooDoctor, damagedMinion);

            var newState = _testee.ApplyAction(gameState, healAction);

            Assert.IsTrue(newState.Of(_player).Minions.Last().Health == 5);
        }

        [Test]
        public void GivenEmptyBoard_WhenSummonsMurlocTidehunter_ThenPlayerWillHaveBothTidehunterAndScoutOnboard()
        {
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(Minions.MurlocTidehunter), remainingMana: x => 10));
            var summonAction = new SummonBattlecryTargetless(_player, Minions.MurlocTidehunter);

            var newState = _testee.ApplyAction(gameState, summonAction);

            var supposedTidehunter = newState.Of(_player).Minions.First();
            var supposedScout = newState.Of(_player).Minions.Last();
            Assert.IsTrue(supposedTidehunter.Attack == 2 && supposedTidehunter.Health == 1);
            Assert.IsTrue(supposedScout.Attack == 1 && supposedTidehunter.Health == 1);
        }

        [Test]
        public void GivenPlayerAndOpponenGotFifteenHealth_WhenPlayerCastsDrainLifeOnOpponentFace_ThenPlayerHasPlusTwoAndOpponentMinusTwoHealth()
        {
            var aFace = Face.Empty.With(health: 15);
            var bFace = Face.Empty.With(health: 15);

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, x => aFace, hand: x => x.AddCard(Spells.DrainLife), remainingMana: x => 10),
                PlayerGameState.Empty.With(face: x => bFace));

            var spellCastAction = new CastSpellTargetable(_player, Spells.DrainLife, bFace);

            var newState = _testee.ApplyAction(gameState, spellCastAction);

            var aHealth = newState.Of(_player).Face.Health;
            var bHealth = newState.OfOpponent(_player).Face.Health;

            Assert.IsTrue(aHealth == aFace.Health + 2);
            Assert.IsTrue(bHealth == bFace.Health - 2);
        }

        [Test]
        public void GivenOneMinionOnboard_WhenCastsMirrorImage_ThenImagesAreSummonedOnTheRightSideOfBoard()
        {
            var gameState = GameState.Empty.With(
                   PlayerGameState.Empty.With(_player, minions: x => x.Add(new Minion(Minions.BloodfenRaptor)), hand: x => x.AddCard(Spells.MirrorImage), remainingMana: x => 10));

            var spellCastAction = new CastSpellTargetless(_player, Spells.MirrorImage);

            var newState = _testee.ApplyAction(gameState, spellCastAction);

            var onBoardMinions = newState.Of(_player).Minions;
            Assert.IsTrue(onBoardMinions.Count == 3);
            Assert.IsTrue(IsSimilar(onBoardMinions[0], Minions.BloodfenRaptor));
            Assert.IsTrue(IsSimilar(onBoardMinions[1], Minions.Uncollectible.MirrorImage));
            Assert.IsTrue(IsSimilar(onBoardMinions[2], Minions.Uncollectible.MirrorImage));
        }

        [Test]
        public void GivenMinionOnboard_WhenPlayerCastsPowerWordShield_ThenMinionHasPlusTwoHealthAndPlayerDrawsCard()
        {
            var raptor = new Minion(Minions.BloodfenRaptor);
            var gameState = GameState.Empty.With(PlayerGameState.Empty.With(_player, minions: x => x.Add(raptor),
                deck: x => x.With(Minions.BoulderfistOgre.Yield()), hand: x => x.AddCard(Spells.PowerWordShield), remainingMana: x => 10));

            var spellCastAction = new CastSpellTargetable(_player, Spells.PowerWordShield, raptor);

            var newState = _testee.ApplyAction(gameState, spellCastAction);

            Assert.IsTrue(newState.Of(_player).Minions.First().Health == 4);
            Assert.IsTrue(newState.Of(_player).Hand.Single() == Minions.BoulderfistOgre);
        }

        [Test]
        public void GivenArgusAndTwoMinionsOnboard_WhenPlaysArgusIntoMiddle_ThenAdjacentMinionsGotOneOneBuffAndTaunt()
        {
            var yeti = new Minion(Minions.ChillwindYeti);
            var raptor = new Minion(Minions.BloodfenRaptor);
            var gameState = GameState.Empty.With(PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { yeti, raptor }),
                    hand: x => x.AddCard(Minions.DefenderOfArgus), remainingMana: x => 10));
            var summonAction = new SummonBattlecryTargetless(_player, Minions.DefenderOfArgus, 1);

            var newState = _testee.ApplyAction(gameState, summonAction);

            var minions = newState.Of(_player).Minions;
            var newYeti = minions[0];
            var supposedArgus = minions[1];
            var newRaptor = minions[2];

            Assert.IsTrue(minions.Count == 3);
            Assert.IsTrue(newYeti.HasStatus(Status.Taunt) && newYeti.Health == yeti.Health + 1 && newYeti.Attack == yeti.Attack + 1);
            Assert.IsTrue(IsSimilar(supposedArgus, Minions.DefenderOfArgus));
            Assert.IsTrue(newRaptor.HasStatus(Status.Taunt) && newRaptor.Health == raptor.Health + 1 && newRaptor.Attack == raptor.Attack + 1);
        }

        [Test]
        public void GivenInjuredBlademasterInHand_WhenSummoned_ThenBlademasterDealsFourDamageToHimself()
        {
            var gameState = GameState.Empty.With(PlayerGameState.Empty.With(_player,
                hand: x => x.AddCard(Minions.InjuredBlademaster), remainingMana: x => 10));
            var summonAction = new SummonBattlecryTargetless(_player, Minions.InjuredBlademaster);

            var newState = _testee.ApplyAction(gameState, summonAction);

            Assert.IsTrue(newState.Of(_player).Minions.Single().Health == Minions.InjuredBlademaster.Health - 4);
        }

        [Test]
        public void GivenAssassinate_WhenCastOnEnemyMinion_ThenEnemyMinionIsDestroyed()
        {
            var enemyMinion = new Minion(Minions.BloodfenRaptor);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(Spells.Assassinate), remainingMana: x => 10),
                PlayerGameState.Empty.With(Player.Empty, minions: x => x.Add(enemyMinion)));
            var spellCastAction = new CastSpellTargetable(_player, Spells.Assassinate, enemyMinion);

            var newState = _testee.ApplyAction(gameState, spellCastAction);

            Assert.IsTrue(newState.OfOpponent(_player).Minions.IsEmpty);
        }

        [Test]
        public void GivenPlayerCanPlayHex_WhenCastOnMinion_ThenMinionIsTransformedToFrog()
        {
            var hexTarget = new Minion(Minions.BoulderfistOgre);
            var enemyMinions = new[] { new Minion(Minions.BloodfenRaptor), hexTarget, new Minion(Minions.ChillwindYeti) };
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(Spells.ArcaneExplosion), remainingMana: x => 10),
                PlayerGameState.Empty.With(Player.Empty, minions: x => x.AddRange(enemyMinions)));
            var spellCastAction = new CastSpellTargetable(_player, Spells.Hex, hexTarget);

            var newState = _testee.ApplyAction(gameState, spellCastAction);

            var updatedEnemyMinions = newState.OfOpponent(_player).Minions;
            Assert.IsTrue(updatedEnemyMinions.Count == 3);
            Assert.IsTrue(IsSimilar(updatedEnemyMinions[1], Minions.Uncollectible.Frog));
        }

        [Test]
        public void GivenGurubashiOnboard_WhenGurubashiRecievesDamage_ThenHisAttackGetsBuffedByPlusThree()
        {
            var gurubashi = Minions.GurubashiBerserker.AsOnboard();
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.Add(gurubashi), hand: x => x.AddCard(Spells.Backstab)));
            var action = new CastSpellTargetable(_player, Spells.Backstab, gurubashi);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Minions.Single().Attack == 5);
        }

        [Test]
        public void GivenGurubashiAndPyromancerOnboard_WhenPlayerCastsBackstabOnGurubashi_ThenGurubashiWillHaveEightAttack()
        {
            var gurubashi = Minions.GurubashiBerserker.AsOnboard();
            var pyromancer = Minions.WildPyromancer.AsOnboard();
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { gurubashi, pyromancer }), hand: x => x.AddCard(Spells.Backstab)));
            var action = new CastSpellTargetable(_player, Spells.Backstab, gurubashi);

            var newState = _testee.ApplyAction(gameState, action);

            var newGuru = newState.Of(_player).Minions.First();
            var newPyro = newState.Of(_player).Minions.Last();
            Assert.IsTrue(newGuru.Attack == 8);
            Assert.IsTrue(newGuru.Health == 4);
            Assert.IsTrue(newPyro.Health == 1);
        }

        [Test]
        public void GivenAcolyteAndPyromancerOnboard_WhenPlayerCastsBackstabOnAcolyte_ThenPlayerDrawsTwoCards()
        {
            var acolyte = Minions.AcolyteOfPain.AsOnboard();
            var pyromancer = Minions.WildPyromancer.AsOnboard();
            var gameState = GameState.Empty.With(PlayerGameState.Empty.With(_player,
                minions: x => x.AddRange(new[] { acolyte, pyromancer }),
                hand: x => x.AddCard(Spells.Backstab),
                deck: x => x.With(new Card[] { Minions.BloodfenRaptor, Spells.Charge })));
            var action = new CastSpellTargetable(_player, Spells.Backstab, acolyte);

            var newState = _testee.ApplyAction(gameState, action);

            var newHand = newState.Of(_player).Hand;
            Assert.IsTrue(newHand.Count() == 2);
            Assert.IsTrue(newHand[0] == Minions.BloodfenRaptor);
            Assert.IsTrue(newHand[1] == Spells.Charge);
            Assert.IsTrue(newState.Of(_player).Minions.Single().Health == 1);
        }

        [Test]
        public void GivenPyromancerAndBluegillOnboard_WhenPlayerCastsBackstabOnPyro_ThenBluegillSurvives()
        {
            var pyromancer = Minions.WildPyromancer.AsOnboard();
            var bluegill = Minions.BluegillWarrior.AsOnboard();
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { pyromancer, bluegill }),
                    hand: x => x.AddCard(Spells.Backstab)));
            var action = new CastSpellTargetable(_player, Spells.Backstab, pyromancer);

            var newState = _testee.ApplyAction(gameState, action);

            var newBluegill = newState.Of(_player).Minions.Single();
            Assert.IsTrue(IsSimilar(newBluegill, Minions.BluegillWarrior));
        }

        [Test]
        public void GivenClericAndInjuredBlademasterOnboard_WhenBlademasterIsHealed_ThenClericDrawsOneCard()
        {
            var cleric = Minions.NorthshireCleric.AsOnboard();
            var blademaster = Minions.InjuredBlademaster.AsOnboard();
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { cleric, blademaster }),
                    hand: x => x.AddCard(Spells.HolyLight), deck: x => x.With(Minions.BloodfenRaptor.Yield())));
            var action = new CastSpellTargetable(_player, Spells.HolyLight, blademaster);

            var newState = _testee.ApplyAction(gameState, action);

            Assert.IsTrue(newState.Of(_player).Hand.Single() == Minions.BloodfenRaptor);
        }

        [Test]
        public void GivenClericAndPyromancerOnboard_WhenPlayerCastsCoinAndCircleOfHealing_ThenClericDrawsTwoCards()
        {
            var cleric = Minions.NorthshireCleric.AsOnboard();
            var pyromancer = Minions.WildPyromancer.AsOnboard();
            var gameState = GameState.Empty.With(PlayerGameState.Empty.With(_player,
                minions: x => x.AddRange(new[] {cleric, pyromancer}),
                hand: x => x.With(new[] {Spells.TheCoin, Spells.CircleOfHealing}),
                deck: x => x.With(new[] {Minions.BloodfenRaptor, Minions.BoulderfistOgre})));
            var coinAction = new CastSpellTargetless(_player, Spells.TheCoin);
            var circleAction = new CastSpellTargetless(_player, Spells.CircleOfHealing);

            var newState = _testee.ApplyAction(gameState, coinAction);
            newState = _testee.ApplyAction(newState, circleAction);

            var newHand = newState.Of(_player).Hand;
            var newBoard = newState.Of(_player).Minions;
            Assert.IsTrue(newHand.Count() == 2);
            Assert.IsTrue(newHand.First() == Minions.BloodfenRaptor);
            Assert.IsTrue(newHand.Last() == Minions.BoulderfistOgre);
            Assert.IsTrue(newBoard.Count == 2);
            Assert.IsTrue(newBoard.First().Health == Minions.NorthshireCleric.Health - 1);
            Assert.IsTrue(newBoard.Last().Health == Minions.WildPyromancer.Health - 1);
        }

        // frothing berserker
    }
}