namespace Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Domain;
    using Infrastructure;
    using NUnit.Framework;

    [TestFixture]
    public class AllPossibleActionsTest
    {
        private GameEngine _testee;
        private readonly Player _player = Player.Empty;

        [SetUp]
        public void RunBeforeEachTestMethod()
        {
            _testee = new GameEngine();
        }

        [Test]
        public void GivenMinionsOnBoard_WhenGetAttackActionsInvoked_ThenCorrectSetOfActionsReturned()
        {
            var aChar1 = new Minion(1, 1);
            var aChar2 = new Minion(1, 1);

            var bChar1 = new Minion(1, 1);
            var bChar2 = new Minion(1, 1);
            var bChar3 = new Minion(1, 1);
            var bFace = Face.Empty;

            var gameState = new GameState(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar1, aChar2)),
                PlayerGameState.Empty.With(face: x => bFace, minions: x => ImmutableList.Create(bChar1, bChar2, bChar3)));

            var expected = new List<AttackAction>
            {
                new AttackAction(_player, aChar1, bChar1),
                new AttackAction(_player, aChar1, bChar2),
                new AttackAction(_player, aChar1, bChar3),
                new AttackAction(_player, aChar2, bChar1),
                new AttackAction(_player, aChar2, bChar2),
                new AttackAction(_player, aChar2, bChar3),
                new AttackAction(_player, aChar1, bFace),
                new AttackAction(_player, aChar2, bFace)
            };

            CollectionAssert.AreEquivalent(expected, _testee.GetAllAttackActions(gameState, _player));
        }

        [Test]
        public void GivenPlayerHasZeroAttackMinion_WhenGetAllAttackActionsInvoked_ThenZeroAttackMinionCannotAttack()
        {
            var aChar = new Minion(1, 1);
            var aZeroAttackChar = new Minion(0, 1);
            var bFace = Face.Empty;
            var gameState = new GameState(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar, aZeroAttackChar)),
                PlayerGameState.Empty.With(face: x => bFace));
            var expectedAction = new AttackAction(_player, aChar, bFace);

            CollectionAssert.AreEquivalent(expectedAction.Yield(), _testee.GetAllAttackActions(gameState, _player));
        }

        [Test]
        public void GivenPlayerHasFrozenMinion_WhenGetAllAttackActionsInvoked_ThenFrozenMinionCannotAttack()
        {
            var player = new Player();
            var aChar = new Minion(1, 1);
            var aFrozenChar = new Minion(1, 1, Status.Frozen);
            var bFace = Face.Empty;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(player, minions: x => ImmutableList.Create(aChar, aFrozenChar)),
                PlayerGameState.Empty.With(face: x => bFace));
            var expectedAction = new AttackAction(player, aChar, bFace);

            CollectionAssert.AreEquivalent(expectedAction.Yield(), _testee.GetAllAttackActions(gameState, player));
        }

        [Test]
        public void GivenOpponentHasTauntMinion_WhenGetAllAttackActionsInvoked_ThenOnlyTauntMinionCanBeAttacked()
        {
            var aChar = new Minion(1, 1);
            var bChar = new Minion(1, 1);
            var bTauntChar = new Minion(1, 1, Status.Taunt);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar)),
                PlayerGameState.Empty.With(Player.Empty, minions: x => ImmutableList.Create(bTauntChar, bChar)));
            var expectedAction = new AttackAction(_player, aChar, bTauntChar);

            CollectionAssert.AreEquivalent(expectedAction.Yield(), _testee.GetAllAttackActions(gameState, _player));
        }

        [Test]
        public void GivenOpponentHasStealthMinion_WhenGetAllAttackActionsInvoked_ThenStealthMinionCannotBeAttacked()
        {
            var aChar = new Minion(1, 1);
            var bStealthChar = new Minion(1, 1, Status.Stealth);
            var bFace = Face.Empty;
            var gameState = new GameState(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar)),
                PlayerGameState.Empty.With(face: x => bFace, minions: x => ImmutableList.Create(bStealthChar)));
            var expectedAction = new AttackAction(_player, aChar, bFace);

            CollectionAssert.AreEquivalent(expectedAction.Yield(), _testee.GetAllAttackActions(gameState, _player));
        }

        [Test]
        public void GivenOpponentHasStealthTauntMinion_WhenGetAllAttackActionsInvoked_ThenStealthTauntMinionCannotBeAttacked()
        {
            var aChar = new Minion(1, 1);
            var bFace = Face.Empty;
            var bStealthTauntChar = new Minion(1, 1, Status.Stealth | Status.Taunt);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar)),
                PlayerGameState.Empty.With(face: x => bFace, minions: x => ImmutableList.Create(bStealthTauntChar)));
            var expectedAction = new AttackAction(_player, aChar, bFace);

            CollectionAssert.AreEquivalent(expectedAction.Yield(), _testee.GetAllAttackActions(gameState, _player));
        }

        [Test]
        public void GivenPlayerGotMinionCardsSomeManaAndSomeOnboardChars_WhenGetMinionSummonActionsInvoked_ThenCorrectSetOfActionsReturned()
        {
            var aChar1 = new Minion(1, 1);
            var aChar2 = new Minion(1, 1);

            var hand = new Hand(new[] { new MinionCard(1, 1, 1), new MinionCard(1, 1, 1), new MinionCard(4, 1, 1) });
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => ImmutableList.Create(aChar1, aChar2), remainingMana: x => 3, hand: x => hand));

            var expected = new[]
            {
                new SummonAction(_player, (MinionCard)hand[0]),
                new SummonAction(_player, (MinionCard)hand[0], 1),
                new SummonAction(_player, (MinionCard)hand[0], 2),
                new SummonAction(_player, (MinionCard)hand[1]),
                new SummonAction(_player, (MinionCard)hand[1], 1),
                new SummonAction(_player, (MinionCard)hand[1], 2)
            };

            var actual = _testee.GetAllSummonActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenPlayerHasMaxMinionsOnboard_WhenGetMinionsSummonActionsInvoked_ThenZeroSummonActionsReturned()
        {
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player,
                    minions: x => x.AddRange(new[]
                    {
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound),
                        new Minion(Minions.CoreHound)
                    }),
                    hand: x => x.AddCard(Minions.SenjinShieldmasta),
                    remainingMana: x => Minions.SenjinShieldmasta.ManaCost));

            var allSummonActions = _testee.GetAllSummonActions(gameState, _player);

            CollectionAssert.IsEmpty(allSummonActions);
        }

        [Test]
        public void GivenPlayerHasEmptyBoard_ThenWindspeakerCanStillBeSummonned()
        {
            var windspeaker = Minions.WindSpeaker;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, hand: x => x.AddCard(windspeaker), remainingMana: x => windspeaker.ManaCost));
            var expected = new SummonAction(_player, windspeaker).Yield();

            var actual = _testee.GetAllSummonActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenBothPlayersHaveMinionOnboard_ThenWindspeakerMustBattlecryAlliedMinionOnly()
        {
            var windspeaker = Minions.WindSpeaker;
            var windfuryTarget = new Minion(Minions.ChillwindYeti);
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.Add(windfuryTarget), hand: x => x.AddCard(windspeaker), remainingMana: x => windspeaker.ManaCost),
                PlayerGameState.Empty.With(minions: x => x.Add(new Minion(Minions.ChillwindYeti))));
            var expected = new[]
            {
                new SummonBattlecryTargetable(_player, windspeaker, windfuryTarget),
                new SummonBattlecryTargetable(_player, windspeaker, windfuryTarget, 1)
            };

            var actual = _testee.GetAllSummonActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenPlayerHasMoonfireInHand_ThenItMustBeAlwaysCastable()
        {
            var aFace = Face.Empty;
            var bFace = Face.Empty;
            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, x => aFace, hand: x => x.AddCard(Spells.Moonfire)),
                PlayerGameState.Empty.With(Player.Empty, x => bFace));

            var expected = new[]
            {
                new CastSpellTargetable(_player, Spells.Moonfire, aFace),
                new CastSpellTargetable(_player, Spells.Moonfire, bFace)
            };

            var actual = _testee.GetAllSpellCastActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenPlayerHasExecuteAndThereAreMinionsOnBoard_ThenItCanOnlyBeUsedOnTheEnemyDamagedMinion()
        {
            var aMinion = new Minion(Minions.BloodfenRaptor);
            var aDamagedMinion = new Minion(Minions.BoulderfistOgre).RecieveDamage(1);
            var bMinion = new Minion(Minions.SenjinShieldmasta);
            var bDamagedMinion = new Minion(Minions.ChillwindYeti).RecieveDamage(1);

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { aMinion, aDamagedMinion }), hand: x => x.AddCard(Spells.Execute), remainingMana: x => 10),
                PlayerGameState.Empty.With(Player.Empty, minions: x => x.AddRange(new[] { bMinion, bDamagedMinion })));

            var expected = new CastSpellTargetable(_player, Spells.Execute, bDamagedMinion).Yield();

            var actual = _testee.GetAllSpellCastActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenPlayerHasBackstabAndThereAreMinionsOnBoard_ThenItCanBeUsedOnAnyUndamagedMinion()
        {
            var aMinion = new Minion(Minions.BloodfenRaptor);
            var aDamagedMinion = new Minion(Minions.BoulderfistOgre).RecieveDamage(1);
            var bMinion = new Minion(Minions.SenjinShieldmasta);
            var bDamagedMinion = new Minion(Minions.ChillwindYeti).RecieveDamage(1);

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { aMinion, aDamagedMinion }), hand: x => x.AddCard(Spells.Backstab)),
                PlayerGameState.Empty.With(Player.Empty, minions: x => x.AddRange(new[] { bMinion, bDamagedMinion })));

            var expected = new[]
            {
                new CastSpellTargetable(_player, Spells.Backstab, aMinion),
                new CastSpellTargetable(_player, Spells.Backstab, bMinion)
            };

            var actual = _testee.GetAllSpellCastActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void GivenPlayerHasShadowWordPain_ThenItCanOnlyBeUsedOnMinionsWithAttackLessOrEqualToThree()
        {
            var bloodfen = Minions.BloodfenRaptor.AsOnboard();
            var boulderfist = Minions.BoulderfistOgre.AsOnboard();
            var senjin = Minions.SenjinShieldmasta.AsOnboard();
            var yeti = Minions.ChillwindYeti.AsOnboard();

            var gameState = GameState.Empty.With(
                PlayerGameState.Empty.With(_player, minions: x => x.AddRange(new[] { bloodfen, boulderfist }), hand: x => x.AddCard(Spells.PowerWordShield), remainingMana: x => 10),
                PlayerGameState.Empty.With(Player.Empty, minions: x => x.AddRange(new[] { senjin, yeti })));

            var expected = new[]
            {
                new CastSpellTargetable(_player, Spells.PowerWordShield, bloodfen),
                new CastSpellTargetable(_player, Spells.PowerWordShield, senjin)
            };

            var actual = _testee.GetAllSpellCastActions(gameState, _player);

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
