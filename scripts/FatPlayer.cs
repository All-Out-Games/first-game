using AO;
using TinyJson;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class FatPlayer : Player 
{
    private double _trophies;
    private double _coins;
    private double _amountOfFoodInStomach;
    private double _valueOfFoodInStomach;

    // these are levels, not the modified values, so we just use an int
    public int _stomachSizeLevel;
    public int _mouthSizeLevel;
    public int _clickPowerLevel;
    public int _rebirth;

    public int MaxEquippedPets
    {
        get
        {
            int result = 3;
            if (GamePasses.HasPetEquipCap1) { result += 1; }
            if (GamePasses.HasPetEquipCap2) { result += 3; }
            if (GamePasses.HasPetEquipCap3) { result += 6; }
            return result;
        }
    }

    public int MaxPetsInStorage
    {
        get
        {
            int result = 20;
            if (GamePasses.HasPetStorageCap1) { result += 10; }
            if (GamePasses.HasPetStorageCap2) { result += 25; }
            if (GamePasses.HasPetStorageCap3) { result += 50; }
            if (GamePasses.HasPetStorageCap3) { result += 100; }
            if (GamePasses.HasPetStorageCap4) { result += 250; }
            return result;
        }
    }

    public Food FoodBeingEaten;
    public float LastFoodClickTime;
    public float FoodProgressLerp;
    
    private PetManager _petManager;
    public PetManager PetManager 
    {
        get 
        {
            if (_petManager == null) {
                _petManager = Entity.AddComponent<PetManager>();
                _petManager.Player = this;
            }
            return _petManager;
        }
    }

    public Quest CurrentQuest;

    public Boss CurrentBoss;
    public bool DoingBossIntro;
    public double BossProgress;
    public double MyProgress;
    public double BossAccumulator;
    public double BossAutoclickerAccumulator;
    public double TimeLastTeleported;

    public int PlayerLevel => 1 + _stomachSizeLevel + _mouthSizeLevel + _clickPowerLevel;

    public List<string> UnlockedZones = new List<string>();

    public List<StatModifier> TemporaryBuffs = new();

    public PlayerGamePasses GamePasses;
    public HashSet<string>  GamePassesHashSet = new();

    public List<ResourceParticle> ActiveParticles = new();
    public float LastFoodParticleArriveTime;
    public float LastTrophyParticleArriveTime;
    public float LastCoinParticleArriveTime;

    public double _lerpedMoney;
    public double CoinsVisual => _lerpedMoney;

    public double _lerpedFoodInStomach;
    public double AmountOfFoodInStomachVisual => _lerpedFoodInStomach;

    public double _lerpedTrophies;
    public double TrophiesVisual => _lerpedTrophies;

    public bool IsBusy => CurrentBoss != null || FoodBeingEaten != null || EggHatchCoroutine.Alive();

    public double BossInteractCooldownTimer;

    public IEnumerator BossIntroCoroutine()
    {
        var ts = new UI.TextSettings()
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 86,
            color = Vector4.White,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            outline = true,
            outlineThickness = 2,
        };

        var textRect = UI.ScreenRect.CenterRect().Offset(0, 200);

        float timer = 0;
        while (Coroutine.Timer(ref timer, 1))
        {
            ts.color = Vector4.White * Ease.OutQuart(1.0f - timer);
            if (IsLocal) UI.Text(textRect, "Ready..", ts);
            yield return null;
        }

        timer = 0;
        while (Coroutine.Timer(ref timer, 1))
        {
            ts.color = Vector4.White * Ease.OutQuart(1.0f - timer);
            if (IsLocal) UI.Text(textRect, "Set..", ts);
            yield return null;
        }

        DoingBossIntro = false;
        CurrentBoss.SpineAnimator.SpineInstance.StateMachine.SetTrigger("start_eating");
        SpineAnimator.SpineInstance.StateMachine.SetTrigger("start_eating");
        var rng = new Random();
        timer = 0;
        while (Coroutine.Timer(ref timer, 2))
        {
            ts.color = Vector4.White;
            ts.size = 128;
            var offset = new Vector2((float)rng.NextDouble() * 2 - 1, (float)rng.NextDouble() * 2 - 1) * 8;
            var rect = textRect.Offset(offset.X, offset.Y);
            if (IsLocal) UI.Text(rect, "EAT!!!", ts);
            yield return null;
        }
    }

    public void GiveFood(double foodAmount, double valueAmount, bool particles = false, Vector2 fromPos = default, Entity toEntity = null)
    {
        IEnumerator Go(double foodAmount, double valueAmount)
        {
            yield return new WaitForSeconds(0.9f);
            AmountOfFoodInStomach += foodAmount;
            ValueOfFoodInStomach += valueAmount;
        }
        if (Network.IsServer) Coroutine.Start(Entity, Go(foodAmount, valueAmount));
        else if (particles) SpawnParticles(fromPos, foodAmount, ResourceParticleKind.Food, toEntity);
    }

    public void GiveCoins(double amount, bool particles = false, Vector2 fromPos = default, Entity toEntity = null)
    {
        IEnumerator Go(double amount)
        {
            yield return new WaitForSeconds(0.9f);
            Coins += amount;
        }
        if (Network.IsServer) Coroutine.Start(Entity, Go(amount));
        else if (particles) SpawnParticles(fromPos, amount, ResourceParticleKind.Coins, toEntity);
    }

    public void GiveTrophies(double amount, bool particles = false, Vector2 fromPos = default, Entity toEntity = null)
    {
        IEnumerator Go(double amount)
        {
            yield return new WaitForSeconds(0.9f);
            Trophies += amount;
        }
        if (Network.IsServer) Coroutine.Start(Entity, Go(amount));
        else if (particles) SpawnParticles(fromPos, amount, ResourceParticleKind.Trophy, toEntity);
    }

    public override void Start()
    {
    }

    [ClientRpc]
    public void DisableInSceneFoods(ulong[] foodNetworkIds)
    {
        if (!this.IsLocal) return;
        foreach (var foodNetworkId in foodNetworkIds)
        {
            var food = Entity.FindByNetworkId(foodNetworkId);
            if (food == null)
            {
                Log.Error($"Failed to find food with network id {foodNetworkId}");
                continue;
            }
            food.SetEnabled(false);
        }
    }

    public IEnumerator DoThumbsUp()
    {
        SpineAnimator.SpineInstance.StateMachine.SetTrigger("thumbs_up");
        AddFreezeReason("thumbs_up");
        yield return new WaitForSeconds(1.75f);
        RemoveFreezeReason("thumbs_up");
    }

    [ClientRpc]
    public void BossFightOver(bool won)
    {
        if (CurrentBoss == null)
        {
            Log.Info("Boss fight over but no boss?");
            return;
        }

        if (won) 
        {
            GiveTrophies(CurrentBoss.Reward, true, CurrentBoss.Entity.Position, Entity);
            if (Network.IsClient)
            {
                SpawnParticles(CurrentBoss.Entity.Position, CurrentBoss.Reward, ResourceParticleKind.Trophy, Entity);
            }

            if (GamePasses.Has2xTrophies)
            {
                Trophies *= 2;
            }

            if (IsLocal)
            {
                Notifications.Show("You won the boss fight!");
            }
            if (Network.IsServer)
            {
                if (CurrentQuest != null)
                {
                    CurrentQuest.OnBossBeatenServer(CurrentBoss);
                }
            }
            Coroutine.Start(Entity, DoThumbsUp());
            CurrentBoss.SpineAnimator.SpineInstance.StateMachine.SetTrigger("RESET");
        }
        else 
        {
            if (IsLocal)
            {
                Notifications.Show("You lost the boss fight!");
            }
            CurrentBoss.SpineAnimator.SpineInstance.StateMachine.SetTrigger("thumbs_up");
            SpineAnimator.SpineInstance.StateMachine.SetTrigger("RESET");
        }

        CurrentBoss.CurrentlyBattling = null;
        CurrentBoss = null;
        this.RemoveFreezeReason("BossFight");
    }

    [ServerRpc]
    public void RunCheatCommand(string command)
    {
        FatGameManager.Instance.RunChatCommand(this, command);
    }

    public void ResetProgressCheat()
    {
        Util.Assert(Network.IsServer);
        Trophies = 0;
        Coins = 0;
        AmountOfFoodInStomach = 0;
        ValueOfFoodInStomach = 0;
        StomachSizeLevel = 0;
        MouthSizeLevel = 0;
        ClickPowerLevel = 0;
        Rebirth = 0;
        CallClient_DeleteAllPets();
    }

    public void IncreaseMoneyCheat()
    {
        Util.Assert(Network.IsServer);
        Coins = (Coins + 5) * 2;
    }

    public void DecreaseMoneyCheat()
    {
        Util.Assert(Network.IsServer);
        Coins /= 2;
    }

    public void IncreaseTrophiesCheat()
    {
        Util.Assert(Network.IsServer);
        Trophies = (Trophies + 5) * 2;
    }

    public void DecreaseTrophiesCheat()
    {
        Util.Assert(Network.IsServer);
        Trophies /= 2;
    }

    public void ForceCompleteQuestCheat()
    {
        Util.Assert(Network.IsServer);
        if (CurrentQuest == null)
        {
            return;
        }
        CurrentQuest.ReportProgress(CurrentQuest.ProgressRequired);
    }

    public override Vector2 CalculatePlayerVelocity(Vector2 currentVelocity, Vector2 input, float deltaTime)
    {
        return DefaultPlayerVelocityCalculation(currentVelocity, input, deltaTime, (float)ModifiedMoveSpeed);
    }

    public Coroutine EggHatchCoroutine;

    public override void Update()
    {
        EggsToOpenHighWaterMark = Math.Max(EggsToOpenHighWaterMark, EggsToOpen.Count);
        if (EggsToOpen.Count == 0 && !EggHatchCoroutine.Alive())
        {
            EggsToOpenHighWaterMark = 0;
        }
        if (EggsToOpen.Count != 0 && !EggHatchCoroutine.Alive())
        {
            Log.Info("Starting egg anim");
            EggHatchCoroutine = Coroutine.Start(Entity, OpenEggAnimation());
        }

        if (AmountOfFoodInStomach != 0)
        {
            foreach (var sellArea in Scene.Components<SellArea>())
            {
                var distanceSqr = (sellArea.Entity.Position - Entity.Position).LengthSquared;
                if (distanceSqr <= SellArea.RadiusSquared)
                {
                    SpawnParticles(Entity.Position + new Vector2(0, 0.5f), AmountOfFoodInStomach, ResourceParticleKind.SellFood, sellArea.Entity);
                    GiveCoins(ValueOfFoodInStomach * ModifiedCashMultiplier, true, sellArea.Entity.Position, Entity);
                    ValueOfFoodInStomach = 0;
                    AmountOfFoodInStomach = 0;
                    if (CurrentQuest != null)
                    {
                        CurrentQuest.OnSoldItemsServer();
                    }
                    break;
                }
            }
        }

        // food
        var foodLerpTarget = AmountOfFoodInStomach;
        _lerpedFoodInStomach = _lerpedFoodInStomach + (foodLerpTarget - _lerpedFoodInStomach) * 0.15f;
        if (Math.Abs(_lerpedFoodInStomach - foodLerpTarget) <= 1) {
            _lerpedFoodInStomach = foodLerpTarget;
        }

        // trophies
        var trophyLerpTarget = Trophies;
        _lerpedTrophies = _lerpedTrophies + (trophyLerpTarget - _lerpedTrophies) * 0.15f;
        if (Math.Abs(_lerpedTrophies - trophyLerpTarget) <= 1) {
            _lerpedTrophies = trophyLerpTarget;
        }

        // money
        _lerpedMoney = _lerpedMoney + (Coins - _lerpedMoney) * 0.15f;

        if (ActiveParticles.Count > 0)
        {
            UI.PushContext(UI.Context.WORLD);
            using var _1 = AllOut.Defer(UI.PopContext);

            UI.PushLayerRelative(1);
            using var _2 = AllOut.Defer(UI.PopLayer);

            var quads = new List<IM.QuadData>(); // todo(josh): nonalloc
            for (int i = 0; i < ActiveParticles.Count; i++)
            {
                var particle = ActiveParticles[i];

                particle.Velocity *= 0.9f;
                particle.Lifetime += Time.DeltaTime;
                if (particle.Lifetime >= 0.75f)
                {
                    particle.Velocity = default;
                    var targetPosition = particle.Target.Position;
                    if (particle.Target == Entity)
                    {
                        targetPosition += new Vector2(0, 0.5f);
                    }
                    particle.MoveTowardSpeed += Time.DeltaTime * 4;
                    particle.Position = particle.Position.MoveTo(targetPosition, 20 * Time.DeltaTime * particle.MoveTowardSpeed, out var arrived);
                    if (arrived)
                    {
                        switch (particle.Kind)
                        {
                            case ResourceParticleKind.Food: {
                                LastFoodParticleArriveTime = Time.TimeSinceStartup;
                                break;
                            }
                            case ResourceParticleKind.Trophy: {
                                LastTrophyParticleArriveTime = Time.TimeSinceStartup;
                                break;
                            }
                            case ResourceParticleKind.SellFood: {
                                // nothing
                                break;
                            }
                            case ResourceParticleKind.Coins: {
                                LastCoinParticleArriveTime = Time.TimeSinceStartup;
                                break;
                            }
                            case ResourceParticleKind.BossFood: {
                                // nothing
                                break;
                            }
                            default: {
                                Log.Error("Unknown kind: " + particle.Kind);
                                return;
                            }
                        }

                        ActiveParticles[i] = ActiveParticles[ActiveParticles.Count-1];
                        ActiveParticles.RemoveAt(ActiveParticles.Count-1);
                        i -= 1;
                        continue;
                    }
                }

                particle.Position += particle.Velocity * Time.DeltaTime;
                ActiveParticles[i] = particle;
                var pos = particle.Position;
                var quad = new IM.QuadData();
                float hs = 0.2f;
                quad.p1 = pos - new Vector2(hs, hs);
                quad.p2 = pos + new Vector2(hs, hs);
                quad.color = Vector4.White;
                quad.texture = particle.Texture;
                quads.Add(quad);
            }
            if (quads.Count > 0)
            {
                IM.Quads(quads.ToArray()); // todo(josh): hnnnggggg
            }
        }

        if (Input.GetKeyDown(Input.Keycode.KEYCODE_P))
        {
            Log.Info($"HasVIP:                {GamePasses.HasVIP}");
            Log.Info($"Has2xTrophies:         {GamePasses.Has2xTrophies}");
            Log.Info($"Has2xMoney:            {GamePasses.Has2xMoney}");
            Log.Info($"HasTeleporter:         {GamePasses.HasTeleporter}");
            Log.Info($"HasBossAutoclicker:    {GamePasses.HasBossAutoclicker}");
            Log.Info($"HasPetEquipCap1:       {GamePasses.HasPetEquipCap1}");
            Log.Info($"HasPetEquipCap2:       {GamePasses.HasPetEquipCap2}");
            Log.Info($"HasPetEquipCap3:       {GamePasses.HasPetEquipCap3}");
            Log.Info($"HasPetStorageCap1:     {GamePasses.HasPetStorageCap1}");
            Log.Info($"HasPetStorageCap2:     {GamePasses.HasPetStorageCap2}");
            Log.Info($"HasPetStorageCap3:     {GamePasses.HasPetStorageCap3}");
        }

        if (Input.GetKeyHeld(Input.Keycode.KEYCODE_LEFT_CONTROL)) {
            if (Input.GetKeyHeld(Input.Keycode.KEYCODE_LEFT_SHIFT)) {
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_R)) {
                    CallServer_RunCheatCommand("reset_progress");
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_EQUAL)) {
                    CallServer_RunCheatCommand("+trophies");
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_MINUS)) {
                    CallServer_RunCheatCommand("-trophies");
                }
            }
            else {
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_EQUAL)) {
                    CallServer_RunCheatCommand("+money");
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_MINUS)) {
                    CallServer_RunCheatCommand("-money");
                }
            }

            if (Input.GetKeyDown(Input.Keycode.KEYCODE_Q)) {
                CallServer_RunCheatCommand("complete_quest");
            }
        }

        if (Network.IsServer)
        {
            if (CurrentQuest != null)
            {
                CurrentQuest.UpdateServer();
            }

            foreach (var buff in TemporaryBuffs)
            {
                buff.TimeLeft -= Time.DeltaTime;
                if (buff.TimeLeft <= 0)
                {
                    CallClient_RemoveTemporaryBuff(buff.Id);
                }
            }
        }

        foreach (var pet in Pet.AllPets)
        {
            if (pet.OwnerId != Entity.NetworkId)
            {
                continue;
            }
            pet.Arrived = false;
        }

        BossInteractCooldownTimer -= Time.DeltaTime;

        if (CurrentBoss != null)
        {
            if (!DoingBossIntro)
            {
                var progressPerClick = StomachSizeLevel;
                progressPerClick = Math.Min(progressPerClick, ClickPowerLevel);
                progressPerClick = Math.Min(progressPerClick, MouthSizeLevel);
                progressPerClick = Math.Max(progressPerClick, 1);

                if (GamePasses.HasBossAutoclicker)
                {
                    BossAutoclickerAccumulator += Time.DeltaTime;
                    while (Util.Timer(ref BossAutoclickerAccumulator, 1.0f / 15.0f)) // arjan says 15 clicks per second
                    {
                        MyProgress += progressPerClick;
                    }
                }

                if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT) || this.IsInputDown(Input.UnifiedInput.KEYCODE_E))
                {
                    MyProgress += progressPerClick;

                    if (Network.IsClient)
                    {
                        SpawnParticles(Entity.Position, 1, ResourceParticleKind.BossFood, Entity);
                    }
                }

                BossAccumulator += Time.DeltaTime;
                while (Util.Timer(ref BossAccumulator, CurrentBoss.Definition.TimeBetweenClicks))
                {
                    BossProgress += CurrentBoss.Definition.AmountPerClick;

                    if (Network.IsClient && this.IsLocal)
                    {
                        SpawnParticles(CurrentBoss.Entity.Position, 5, ResourceParticleKind.BossFood, CurrentBoss.Entity);
                    }
                }

                if (Network.IsServer)
                {
                    if (MyProgress >= CurrentBoss.AmountToWin || BossProgress >= CurrentBoss.AmountToWin)
                    {
                        CallClient_BossFightOver(MyProgress > BossProgress);
                        BossInteractCooldownTimer = 1;
                    }
                    else
                    {
                        if (IsInputDown(Input.UnifiedInput.MOUSE_RIGHT))
                        {
                            CallClient_BossFightOver(false);
                            BossInteractCooldownTimer = 1;
                        }
                    }
                }
            }
        }
        else
        {
            BossAutoclickerAccumulator = 0;
            DoingBossIntro = false;
        }

        if (FoodBeingEaten != null)
        {
            if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT) || this.IsInputDown(Input.UnifiedInput.KEYCODE_E))
            {
                FoodBeingEaten.OnClick(this);
            }

            if (Network.IsClient)
            {
                var foodScreenPos = Camera.WorldToScreen(FoodBeingEaten.EatUIPosition);
                var clickRect = new Rect(foodScreenPos);
                clickRect = clickRect.Grow(10, 50, 10, 50).Offset(0, 10);
                float scale01 = Ease.OutQuart(Ease.T(Time.TimeSinceStartup - LastFoodClickTime, 0.25f));
                float scale = AOMath.Lerp(1.5f, 1.0f, scale01);
                clickRect = clickRect.Scale(scale, scale);
                UI.Image(clickRect, References.Instance.BossBarBg, Vector4.White, References.Instance.BossBarSlice);

                float foodHealth01 = 1.0f - (float)Math.Clamp((float)FoodBeingEaten.CurrentHealth / (float)FoodBeingEaten.ClicksRequired, 0, 1);
                FoodProgressLerp = AOMath.Lerp(FoodProgressLerp, foodHealth01, 20 * Time.DeltaTime);
                float progress01 = AOMath.Lerp(0.2f, 1.0f, FoodProgressLerp);
                var chewProgressRect = clickRect.SubRect(0, 0, progress01, 1, 0, 0, 0, 0);
                UI.Image(chewProgressRect, References.Instance.BossBarBg, Vector4.HSVLerp(Vector4.Red, Vector4.Green, FoodProgressLerp), References.Instance.BossBarSlice);

                var pendingTextSettings = new UI.TextSettings()
                {
                    font = UI.TextSettings.Font.AlphaKind,
                    size = 32,
                    color = Vector4.White,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    wordWrap = false,
                    wordWrapOffset = 0,
                    outline = true,
                    outlineThickness = 2,
                };
                var colorEase = Ease.OutQuart(Ease.T(Time.TimeSinceStartup - LastFoodClickTime, 0.25f));
                pendingTextSettings.color.Y = colorEase;
                pendingTextSettings.color.Z = colorEase;
                pendingTextSettings.size = AOMath.Lerp(48, 32, colorEase);
                UI.Text(clickRect, $"{FoodBeingEaten.CurrentHealth}", pendingTextSettings);

                var instructionsTextSettings = new UI.TextSettings()
                {
                    font = UI.TextSettings.Font.AlphaKind,
                    size = 32,
                    color = Vector4.White,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    wordWrap = true,
                    outline = true,
                    outlineThickness = 2,
                };

                UI.Text(new Rect(foodScreenPos).Offset(0, -100).Grow(0, 1000, 0, 1000), "EAT: Left Click/E\nQUIT: Right Click", instructionsTextSettings);
            }
        }
        else
        {
            FoodProgressLerp = 0;
        }

        if (Network.IsServer)
        {
            if (FoodBeingEaten.Alive() && IsInputDown(Input.UnifiedInput.MOUSE_RIGHT))
            {
                FoodBeingEaten.CallClient_FinishEating(false);
            }

            if (CurrentBoss.Alive() && IsInputDown(Input.UnifiedInput.MOUSE_RIGHT))
            {
                CallClient_BossFightOver(false);
            }
        }
    }

    public void SpawnParticles(Vector2 position, double requestedAmount, ResourceParticleKind kind, Entity target)
    {
        requestedAmount = Math.Sqrt(requestedAmount);
        long amount = (long)Math.Min(30, requestedAmount);
        Texture tex = null;
        switch (kind)
        {
            case ResourceParticleKind.Food: {
                tex = References.Instance.FoodIcon;
                break;
            }
            case ResourceParticleKind.Trophy: {
                tex = References.Instance.TrophyIcon;
                break;
            }
            case ResourceParticleKind.SellFood: {
                tex = References.Instance.FoodIcon;
                break;
            }
            case ResourceParticleKind.Coins: {
                tex = References.Instance.CoinIcon;
                break;
            }
            case ResourceParticleKind.BossFood: {
                tex = References.Instance.FoodIcon;
                break;
            }
            default: {
                Log.Error("Unknown kind: " + kind);
                return;
            }
        }

        var rng = new Random();
        for (int i = 0; i < amount; i++)
        {
            ResourceParticle particle = new ResourceParticle();
            var radians = rng.NextDouble() * 2 * Math.PI;
            var dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            particle.Position = position;
            particle.Velocity = dir * 10 * rng.NextFloat();
            particle.Texture = tex;
            particle.Kind = kind;
            particle.Target = target;
            ActiveParticles.Add(particle);
        }
    }

    public static int LastBuffId;
    public void ServerGiveTemporaryBuff(StatModifierKind kind, float multiplyValue, float duration)
    {
        Util.Assert(Network.IsServer);
        LastBuffId += 1;
        CallClient_GiveTemporaryBuff(LastBuffId, (int)kind, multiplyValue, duration);
    }

    [ClientRpc]
    public void GiveTemporaryBuff(int id, int _kind, float multiplyValue, float duration)
    {
        var kind = (StatModifierKind)_kind;
        var mod = new StatModifier();
        mod.Kind          = kind;
        mod.MultiplyValue = multiplyValue;
        mod.Id            = id;
        mod.TimeLeft      = duration;
        TemporaryBuffs.Add(mod);
    }

    [ClientRpc]
    public void RemoveTemporaryBuff(int id)
    {
        for (int i = 0; i < TemporaryBuffs.Count; i++)
        {
            if (TemporaryBuffs[i].Id == id)
            {
                TemporaryBuffs.RemoveAt(i);
                return;
            }
        }
        Log.Error($"Failed to find buff with id: {id}");
    }

    public override void OnDestroy()
    {
        if (FoodBeingEaten != null && Network.IsServer)
        {
            FoodBeingEaten.CallClient_FinishEating(false);
        }

        if (CurrentBoss != null && Network.IsServer)
        {
            CallClient_BossFightOver(false);
        }

        Pet.AllPets.RemoveAll(p => p.OwnerId == this.Entity.NetworkId);
    }

    public void OnLoad()
    {
        if (Network.IsServer)
        {
            Trophies              = Save.GetDouble(this, "Trophies", 0);
            Coins                 = Save.GetDouble(this, "Coins", 0);
            AmountOfFoodInStomach = Save.GetDouble(this, "AmountOfFoodInStomach", 0);
            ValueOfFoodInStomach  = Save.GetDouble(this, "ValueOfFoodInStomach", 0);
            StomachSizeLevel      = Save.GetInt(this, "MaxFood", 0);
            MouthSizeLevel        = Save.GetInt(this, "MouthSize", 0);
            ClickPowerLevel       = Save.GetInt(this, "ChewSpeed", 0);
            Rebirth               = Save.GetInt(this, "Rebirth", 0);

            var gamePassesStr = Save.GetString(this, "GamePasses", "[]");
            CallClient_LoadGamePasses(gamePassesStr);

            var zones = Save.GetString(this, "UnlockedZones", "[]");
            CallClient_LoadZoneData(zones);

            var pets = Save.GetString(this, "AllPets", "[]");
            CallClient_LoadPetData(pets);
        }
    }

    [ClientRpc]
    public void LoadGamePasses(string gamePassesStr)
    {
        string[] gamePassesArray = gamePassesStr.FromJson<string[]>();
        if (gamePassesArray == null)
        {
            return;
        }
        GamePassesHashSet.Clear();
        GamePasses = new();
        foreach (var pass in gamePassesArray)
        {
            EnableGamePass(pass); // note(josh): intentionally non-rpc
        }
    }

    public void ServerSaveGamePasses()
    {
        Util.Assert(Network.IsServer);
        string[] gamePassesArray = GamePassesHashSet.ToArray();
        string saveValue = JSONWriter.ToJson(gamePassesArray);
        Log.Info($"Saving game passes: {saveValue}");
        Save.SetString(this, "GamePasses", saveValue);
    }

    public void ServerOnBuyGamePass(string pass)
    {
        Util.Assert(Network.IsServer);
        CallClient_EnableGamePass(pass);
        ServerSaveGamePasses();
    }

    [ClientRpc]
    public void EnableGamePass(string pass)
    {
        Log.Info($"Enabling game pass: {pass}");
        GamePassesHashSet.Add(pass);
        switch (pass)
        {
            case "vip":               { GamePasses.HasVIP             = true; break; }
            case "2x_trophies":       { GamePasses.Has2xTrophies      = true; break; }
            case "2x_money":          { GamePasses.Has2xMoney         = true; break; }
            case "teleporter":        { GamePasses.HasTeleporter      = true; break; }
            case "boss_autoclicker":  { GamePasses.HasBossAutoclicker = true; break; }
            case "pet_equip_cap_1":   { GamePasses.HasPetEquipCap1    = true; break; }
            case "pet_equip_cap_2":   { GamePasses.HasPetEquipCap2    = true; break; }
            case "pet_equip_cap_3":   { GamePasses.HasPetEquipCap3    = true; break; }
            case "pet_storage_cap_1": { GamePasses.HasPetStorageCap1  = true; break; }
            case "pet_storage_cap_2": { GamePasses.HasPetStorageCap2  = true; break; }
            case "pet_storage_cap_3": { GamePasses.HasPetStorageCap3  = true; break; }
            case "pet_storage_cap_4": { GamePasses.HasPetStorageCap4  = true; break; }
            case "pet_storage_cap_5": { GamePasses.HasPetStorageCap5  = true; break; }
            default: {
                Log.Error($"Unknown game pass {pass}");
                break;
            }
        }
    }

    [ClientRpc]
    public void SyncQuestTimeLeft(float timeLeft)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        CurrentQuest.TimeLeft = timeLeft;
    }

    [ClientRpc]
    public void OnQuestProgressUpdated(int progress)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        CurrentQuest.Progress = progress;
    }

    [ClientRpc]
    public void OnQuestFinished(bool success, string reasonIfFailed)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        if (IsLocal)
        {
            if (success)
            {
                Notifications.Show("Quest completed! :)");
            }
            else
            {
                Notifications.Show($"Quest failed: {reasonIfFailed}");
            }
        }

        CurrentQuest = null;
    }

    [ClientRpc]
    public void GiveUpEating()
    {
        if (FoodBeingEaten != null)
        {
            FoodBeingEaten.FinishEating(false);
        }
        FoodBeingEaten = null;
    }

    [ClientRpc]
    public void StartBossFight(ulong bossNetworkId, Vector2 position)
    {
        Log.Info("Really starting boss fight");
        var entity = Entity.FindByNetworkId(bossNetworkId);
        if (entity == null) return;

        Log.Info("Found boss entity and actually starting the fight this time for real");
        this.AddFreezeReason("BossFight");
        
        MyProgress = 0;
        BossProgress = 0;
        BossAccumulator = 0;
        BossAutoclickerAccumulator = 0;
        CurrentBoss = entity.GetComponent<Boss>();
        DoingBossIntro = true;
        Util.Assert(CurrentBoss != null);
        Coroutine.Start(Entity, BossIntroCoroutine());

        Entity.Position = position;
        CurrentBoss.CurrentlyBattling = this;
    }

    public double Trophies
    { 
        get => _trophies; 
        set 
        { 
            _trophies = value; 
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "Trophies", value);
                CallClient_NotifyTrophiesUpdate(value);
            }
        } 
    }

    public double Coins
    { 
        get => _coins; 
        set 
        { 
            _coins = value; 
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "Coins", value);
                CallClient_NotifyCoinsUpdate(value);
            }
        } 
    }

    public double AmountOfFoodInStomach
    { 
        get => _amountOfFoodInStomach;
        set 
        { 
            _amountOfFoodInStomach = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "AmountOfFoodInStomach", value);
                CallClient_NotifyAmountOfFoodInStomachUpdate(value);
            }
        } 
    }

    public double ValueOfFoodInStomach
    {
        get => _valueOfFoodInStomach;
        set
        {
            _valueOfFoodInStomach = value;
            if (Network.IsServer)
            {
                Save.SetDouble(this, "ValueOfFoodInStomach", value);
                CallClient_NotifyValueOfFoodInStomachUpdate(value);
            }
        }
    }

    public int StomachSizeLevel
    { 
        get => _stomachSizeLevel;
        set 
        { 
            _stomachSizeLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "MaxFood", value);
                CallClient_NotifyMaxFoodUpdate(value);
            }
        } 
    }

    public int MouthSizeLevel
    { 
        get => _mouthSizeLevel;
        set 
        { 
            _mouthSizeLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "MouthSize", value);
                CallClient_NotifyMouthSizeUpdate(value);
            }
        } 
    }

    public int ClickPowerLevel
    { 
        get => _clickPowerLevel;
        set 
        { 
            _clickPowerLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "ChewSpeed", value);
                CallClient_NotifyClickPowerUpdate(value);
            }
        } 
    }

    public int Rebirth
    {
        get => _rebirth;
        set
        {
            _rebirth = value;
            if (Network.IsServer)
            {
                Save.SetInt(this, "Rebirth", value);
                CallClient_NotifyRebirthUpdate(value);
            }
        }
    }

    public double BaseClickPowerValue     => ClickPowerByLevel [Math.Clamp(_clickPowerLevel,  0, ClickPowerByLevel.Length-1)].Value;
    public double BaseMouthSizeValue      => MouthSizeByLevel  [Math.Clamp(_mouthSizeLevel,   0, MouthSizeByLevel.Length-1)].Value;
    public double BaseStomachSizeValue    => StomachSizeByLevel[Math.Clamp(_stomachSizeLevel, 0, StomachSizeByLevel.Length-1)].Value;
    public double BaseCashMultiplierValue
    {
        get
        {
            double result = global::Rebirth.Instance.GetRebirthData(Rebirth).CashMultiplier;
            if (GamePasses.Has2xMoney)
            {
                result *= 2;
            }
            return result;
        }
    }

    public double ModifiedClickPower     => BaseClickPowerValue     * CalculateTotalMultiplierFromPets(StatModifierKind.ClickPower)     * CalculateTotalMultiplierFromBuffs(StatModifierKind.ClickPower);
    public double ModifiedMouthSize      => BaseMouthSizeValue      * CalculateTotalMultiplierFromPets(StatModifierKind.MouthSize)      * CalculateTotalMultiplierFromBuffs(StatModifierKind.MouthSize);
    public double ModifiedStomachSize    => BaseStomachSizeValue    * CalculateTotalMultiplierFromPets(StatModifierKind.StomachSize)    * CalculateTotalMultiplierFromBuffs(StatModifierKind.StomachSize);
    public double ModifiedCashMultiplier => BaseCashMultiplierValue * CalculateTotalMultiplierFromPets(StatModifierKind.Money)          * CalculateTotalMultiplierFromBuffs(StatModifierKind.Money);
    public double ModifiedMoveSpeed      =>                           CalculateTotalMultiplierFromPets(StatModifierKind.MoveSpeed)      * CalculateTotalMultiplierFromBuffs(StatModifierKind.MoveSpeed);

    public double CalculateTotalMultiplierFromPets(StatModifierKind kind)
    {
        double summedMultipliers = 0.0;
        foreach (var pet in PetManager.OwnedPets)
        {
            if (!pet.Equipped)
            {
                continue;
            }
            var defn = pet.GetDefinition();
            if (defn == null)
            {
                continue;
            }
            foreach (var modifier in defn.StatModifiers)
            {
                if (modifier.Kind == kind)
                {
                    summedMultipliers += modifier.MultiplyValue - 1.0;
                }
            }
        }
        return 1 + summedMultipliers;
    }

    public double CalculateTotalMultiplierFromBuffs(StatModifierKind kind)
    {
        double summedMultipliers = 0.0;
        foreach (var buff in TemporaryBuffs)
        {
            if (buff.Kind == kind)
            {
                summedMultipliers += buff.MultiplyValue - 1.0;
            }
        }
        return 1 + summedMultipliers;
    }

    [AOIgnore] public Dictionary<string, int> ZoneCosts = new Dictionary<string, int>() 
    {
        { "zone0", 0 },
        { "zone1", 10 },
        { "zone2", 20 },
        { "zone3", 30 },
        { "zone4", 40 },
        { "zone5", 50 },
        { "zone6", 60 },
        { "zone7", 70 },
        { "zone8", 80 },
        { "zone9", 90 },
    };

    public void PurchaseZone(string id)
    {
        if (!Network.IsServer) return;

        if (!ZoneCosts.TryGetValue(id, out var cost)) 
        {
            Log.Error($"Could not find cost for zone {id}");
            return;
        }

        if (Trophies < cost) 
        {
            Log.Error($"Not enough trophies to purchase zone {id}");
            return;
        }

        Trophies -= cost;
        CallClient_NotifyZoneUnlocked(id);
        Save.SetString(this, "UnlockedZones", JSONWriter.ToJson(UnlockedZones));
    }

    [ClientRpc]
    public void LoadZoneData(string data)
    {
        UnlockedZones = JSONParser.FromJson<List<string>>(data);
    }

    [ClientRpc]
    public void NotifyZoneUnlocked(string id)
    {
        UnlockedZones.Add(id);
    }

    [ClientRpc]
    public void LoadPetData(string data)
    {
        PetManager.LoadPetData(data);
    }

    [ClientRpc]
    public void AddPet(string petId, string petDefinitionId, string eggId)
    {
        PetManager.AddPet(petId, petDefinitionId);
    }

    [ClientRpc]
    public void EquipPet(string petId)
    {
        PetManager.EquipPet(petId);
    }

    [ClientRpc]
    public void UnequipPet(string id)
    {
        PetManager.UnequipPet(id);
    }

    [ClientRpc]
    public void DeletePet(string id)
    {
        PetManager.DeletePet(id);
    }

    [ClientRpc]
    public void DeleteAllPets()
    {
        PetManager.DeleteAllPets();
    }

    public struct EggToOpen
    {
        public PetData.PetDefinition Pet;
        public PetData.EggDefinition Egg;
        public EggToOpen(PetData.PetDefinition pet, PetData.EggDefinition egg)
        {
            Pet = pet;
            Egg = egg;
        }
    }

    public List<EggToOpen> EggsToOpen = new();
    public int EggsToOpenHighWaterMark;

    [ClientRpc]
    public void AddEggToOpen(string eggId, string petId)
    {
        if (!this.IsLocal) return;
        Log.Info($"Open egg: {eggId} {petId}");
        PetData.PetDefinition petDefinition = PetData.Pets[petId];
        PetData.EggDefinition eggDefinition = PetData.Eggs[eggId];
        Util.Assert(petDefinition != null);
        Util.Assert(eggDefinition != null);
        EggsToOpen.Add(new EggToOpen(petDefinition, eggDefinition));
    }

    [ClientRpc]
    public void AddEggsToOpen(string[] eggIds, string[] petIds)
    {
        if (!this.IsLocal) return;

        for (int i = 0; i < eggIds.Length; i++)
        {
            AddEggToOpen(eggIds[i], petIds[i]);
        }
    }

    [ServerRpc]
    public void IAmOpeningEggs()
    {
        CallClient_OpeningEggs();
    }

    [ServerRpc]
    public void IAmDoneOpeningEggs()
    {
        CallClient_DoneOpeningEggs();
    }

    [ClientRpc]
    public void OpeningEggs()
    {
        AddFreezeReason("EggHatchAnimation");
    }

    [ClientRpc]
    public void DoneOpeningEggs()
    {
        RemoveFreezeReason("EggHatchAnimation");
    }

    public void UpdateAndDrawEggSkeletonUI(float fadeT, SpineInstance eggSkeleton, float eggDropT, SpineInstance petSkeleton, float petAlpha, PetData.PetDefinition petDefinition, bool showEggCount)
    {
        var ts = new UI.TextSettings()
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 36,
            color = Vector4.White,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
            wordWrap = false,
            wordWrapOffset = 0,
            outline = true,
            outlineThickness = 2,
        };

        Rect eggRect = UI.ScreenRect.Slide(0, 1.0f - Ease.OutQuart(eggDropT)).Offset(0, -100).Scale(0.45f);
        if (eggSkeleton != null) eggSkeleton.Update(Time.DeltaTime);
        if (petSkeleton != null) petSkeleton.Update(Time.DeltaTime);
        UI.Image(UI.ScreenRect, null, new Vector4(0, 0, 0, 0.8f) * Ease.OutQuart(fadeT), new UI.NineSlice());
        if (petSkeleton != null)
        {
            Rect petRect = eggRect.Offset(0, petDefinition.EggOpenAnimYOffset);
            UI.DrawSkeleton(petRect.Scale(petAlpha), petSkeleton, Vector4.White * petAlpha, new Vector2(100, 100), (float)Math.Sin(2 * Math.PI * Time.TimeSinceStartup * 0.5) * 8);

            var petNameTs = ts;
            petNameTs.size = 100;
            petNameTs.color = GameUI.GetRarityColor(petDefinition.Rarity) * petAlpha;
            Rect nameTextRect = UI.Text(UI.ScreenRect.BottomCenterRect().Offset(0, 175), petDefinition.Name, petNameTs);

            var infoRect = UI.ScreenRect.SubRect(0.5f, 0.5f, 1, 0.5f, 0, 0, 0, 0);
            foreach(var modifier in petDefinition.StatModifiers)
            {
                var statModRect = infoRect.CutTop(50);
                var (text, col) = GameUI.BuildStatModifierText(modifier);

                UI.Text(statModRect, text, new UI.TextSettings() {
                    color = col * petAlpha,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                    size = 48
                });
            }
        }
        if (eggSkeleton != null) UI.DrawSkeleton(eggRect, eggSkeleton, Vector4.White, new Vector2(100, 100), 0);

        if (showEggCount)
        {
            UI.Text(UI.ScreenRect.BottomCenterRect().Offset(0, 25), $"{EggsToOpenHighWaterMark-EggsToOpen.Count}/{EggsToOpenHighWaterMark} eggs", ts);
        }
    }

    public IEnumerator OpenEggAnimation()
    {
        CallServer_IAmOpeningEggs();

        var ts = new UI.TextSettings()
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 48,
            color = Vector4.White,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
            wordWrap = false,
            wordWrapOffset = 0,
            outline = true,
            outlineThickness = 2,
        };

        Rect textRect = UI.ScreenRect.BottomCenterRect().Offset(0, 75);
        float timer0 = 0;
        while (Coroutine.Timer(ref timer0, 0.75f))
        {
            UI.PushLayer(GameUI.EggUILayer);
            UpdateAndDrawEggSkeletonUI(Ease.T(timer0, 0.75f), null, 0, null, 0, null, false);
            UI.PopLayer();
            yield return null;
            if (IsInputDown(Input.UnifiedInput.MOUSE_LEFT))
            {
                break;
            }
        }

        while (EggsToOpen.Count > 0)
        {
            var eggToOpen = EggsToOpen.Pop();
            var eggDefinition = eggToOpen.Egg;
            var petDefinition = eggToOpen.Pet;

            var eggSkeleton = SpineInstance.Make();
            eggSkeleton.SetSkeleton(References.Instance.EggOpenAnimSkeleton);
            eggSkeleton.SetSkin(eggDefinition.EggHatchAnimSkin);
            eggSkeleton.RefreshSkins();
            eggSkeleton.SetAnimation("idle", true);
            var petSkeleton = SpineInstance.Make();
            petSkeleton.SetSkeleton(petDefinition.Spine);
            petSkeleton.SetSkin(petDefinition.Skin);
            petSkeleton.RefreshSkins();
            petSkeleton.SetAnimation("idle", true);

            float startTime1 = Time.TimeSinceStartup;
            while (true)
            {
                UI.PushLayer(GameUI.EggUILayer);
                UpdateAndDrawEggSkeletonUI(1, eggSkeleton, Ease.T(Time.TimeSinceStartup - startTime1, 0.75f), null, 0, null, true);
                UI.Text(textRect, "Click to open!", ts);
                UI.PopLayer();
                yield return null;
                if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT))
                {
                    break;
                }
            }

            eggSkeleton.SetAnimation("hatch", false);
            float timer2 = 0;
            while (Coroutine.Timer(ref timer2, 3.5f))
            {
                UI.PushLayer(GameUI.EggUILayer);
                UpdateAndDrawEggSkeletonUI(1, eggSkeleton, 1, petSkeleton, Ease.Linearstep(1.7f, 1.95f, timer2), petDefinition, true);
                UI.PopLayer();
                yield return null;
                if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT))
                {
                    break;
                }
            }

            while (true)
            {
                UI.PushLayer(GameUI.EggUILayer);
                UpdateAndDrawEggSkeletonUI(1, null, 1, petSkeleton, 1, petDefinition, true);
                UI.Text(textRect, "Click to continue", ts);
                UI.PopLayer();
                yield return null;
                if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT))
                {
                    break;
                }
            }

            eggSkeleton.Destroy();
        }

        float timer3 = 0;
        while (Coroutine.Timer(ref timer3, 0.5f))
        {
            UI.PushLayer(GameUI.EggUILayer);
            UpdateAndDrawEggSkeletonUI(1.0f - Ease.T(timer3, 0.5f), null, 0, null, 0, null, false);
            UI.PopLayer();
            yield return null;
            if (this.IsInputDown(Input.UnifiedInput.MOUSE_LEFT))
            {
                break;
            }
        }

        CallServer_IAmDoneOpeningEggs();
    }

    [ServerRpc]
    public void RequestEquipPet(string id)
    {
        var equippedPetsCount = PetManager.OwnedPets.Count(p => p.Equipped);
        if (equippedPetsCount >= MaxEquippedPets) {
            return;
        }
        CallClient_EquipPet(id);
    }

    [ServerRpc]
    public void RequestUnequipPet(string id)
    {
        CallClient_UnequipPet(id);
    }

    [ServerRpc]
    public void RequestDeletePet(string id)
    {
        CallClient_DeletePet(id);
    }

    [ClientRpc]
    public void DoRebirth(int rebirth)
    {
        var rbd = global::Rebirth.Instance.GetRebirthData(rebirth);
        
        this.Rebirth = rebirth;
        if (IsLocal)
        {
            Notifications.Show($"You have been reborn! You are now a {rbd.RankName}!");
        }
        // TODO show an anim or something
    }

    [ServerRpc]
    public void RequestRebirth()
    {
        var rbd = global::Rebirth.Instance.GetRebirthData(this.Rebirth + 1);
        if (this.Trophies < rbd.TrophiesCost) {
            // TODO alert user
            return;
        }

        this.Trophies -= rbd.TrophiesCost;
        this.Rebirth += 1;
        this.Coins = 0;
        this.ClickPowerLevel = 0;
        this.MouthSizeLevel = 0;
        this.StomachSizeLevel = 0;
        
        CallClient_DoRebirth(this.Rebirth);
    }

    [ClientRpc] public void NotifyTrophiesUpdate(double val)                   { if (Network.IsClient) Trophies              = val; }
    [ClientRpc] public void NotifyCoinsUpdate(double val)                      { if (Network.IsClient) Coins                 = val; }
    [ClientRpc] public void NotifyAmountOfFoodInStomachUpdate(double val)      { if (Network.IsClient) AmountOfFoodInStomach = val; }
    [ClientRpc] public void NotifyValueOfFoodInStomachUpdate(double val)       { if (Network.IsClient) ValueOfFoodInStomach  = val; }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)                       { if (Network.IsClient) StomachSizeLevel      = val; }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val)                     { if (Network.IsClient) MouthSizeLevel        = val; }
    [ClientRpc] public void NotifyClickPowerUpdate(int val)                    { if (Network.IsClient) ClickPowerLevel       = val; }
    [ClientRpc] public void NotifyRebirthUpdate(int val)                       { if (Network.IsClient) Rebirth               = val; }

    [ServerRpc]
    public void RequestPurchaseStomachSize()
    {
        if (Network.IsClient) return;
        if (StomachSizeLevel >= StomachSizeByLevel.Length) return;
        double cost = StomachSizeByLevel[StomachSizeLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        StomachSizeLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseMouthSize()
    {
        if (Network.IsClient) return;
        if (MouthSizeLevel >= MouthSizeByLevel.Length) return;
        double cost = MouthSizeByLevel[MouthSizeLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        MouthSizeLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseClickPower()
    {
        if (Network.IsClient) return;
        if (ClickPowerLevel >= ClickPowerByLevel.Length) return;
        double cost = ClickPowerByLevel[ClickPowerLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        ClickPowerLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseItem(string id)
    {
        Shop.Instance.Purchase(this, id);
    }

    public struct UpgradeStatAndCost
    {
        public double Value;
        public double Cost;
    }

    public static UpgradeStatAndCost[] MouthSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,   Cost = 5 },
        new UpgradeStatAndCost(){ Value = 6,   Cost = 7 },
        new UpgradeStatAndCost(){ Value = 11,  Cost = 9 },
        new UpgradeStatAndCost(){ Value = 16,  Cost = 11 },
        new UpgradeStatAndCost(){ Value = 21,  Cost = 15 },
        new UpgradeStatAndCost(){ Value = 26,  Cost = 20 },
        new UpgradeStatAndCost(){ Value = 31,  Cost = 26 },
        new UpgradeStatAndCost(){ Value = 36,  Cost = 35 },
        new UpgradeStatAndCost(){ Value = 41,  Cost = 46 },
        new UpgradeStatAndCost(){ Value = 46,  Cost = 61 },
        new UpgradeStatAndCost(){ Value = 51,  Cost = 80 },
        new UpgradeStatAndCost(){ Value = 56,  Cost = 106 },
        new UpgradeStatAndCost(){ Value = 61,  Cost = 140 },
        new UpgradeStatAndCost(){ Value = 66,  Cost = 185 },
        new UpgradeStatAndCost(){ Value = 71,  Cost = 244 },
        new UpgradeStatAndCost(){ Value = 76,  Cost = 322 },
        new UpgradeStatAndCost(){ Value = 81,  Cost = 425 },
        new UpgradeStatAndCost(){ Value = 86,  Cost = 561 },
        new UpgradeStatAndCost(){ Value = 91,  Cost = 740 },
        new UpgradeStatAndCost(){ Value = 96,  Cost = 977 },
        new UpgradeStatAndCost(){ Value = 401, Cost = 1628 },
        new UpgradeStatAndCost(){ Value = 421, Cost = 2230 },
        new UpgradeStatAndCost(){ Value = 441, Cost = 3055 },
        new UpgradeStatAndCost(){ Value = 461, Cost = 4185 },
        new UpgradeStatAndCost(){ Value = 481, Cost = 5733 },
        new UpgradeStatAndCost(){ Value = 501, Cost = 7855 },
        new UpgradeStatAndCost(){ Value = 521, Cost = 10761 },
        new UpgradeStatAndCost(){ Value = 541, Cost = 14743 },
        new UpgradeStatAndCost(){ Value = 561, Cost = 20197 },
        new UpgradeStatAndCost(){ Value = 581, Cost = 27671 },
        new UpgradeStatAndCost(){ Value = 601, Cost = 37909 },
        new UpgradeStatAndCost(){ Value = 621, Cost = 51935 },
        new UpgradeStatAndCost(){ Value = 641, Cost = 71151 },
        new UpgradeStatAndCost(){ Value = 661, Cost = 97477 },
        new UpgradeStatAndCost(){ Value = 681, Cost = 133543 },
        new UpgradeStatAndCost(){ Value = 701, Cost = 182954 },
        new UpgradeStatAndCost(){ Value = 721, Cost = 250647 },
        new UpgradeStatAndCost(){ Value = 741, Cost = 343386 },
        new UpgradeStatAndCost(){ Value = 761, Cost = 470438 },
        new UpgradeStatAndCost(){ Value = 781, Cost = 644501 },
        new UpgradeStatAndCost(){ Value = 801, Cost = 882966 },
        new UpgradeStatAndCost(){ Value = 821, Cost = 1209663 },
        new UpgradeStatAndCost(){ Value = 841, Cost = 1657239 },
        new UpgradeStatAndCost(){ Value = 861, Cost = 2270417 },
        new UpgradeStatAndCost(){ Value = 881, Cost = 3110471 },
        new UpgradeStatAndCost(){ Value = 901, Cost = 4261346 },
        new UpgradeStatAndCost(){ Value = 921, Cost = 5838044 },
        new UpgradeStatAndCost(){ Value = 941, Cost = 7998120 },
        new UpgradeStatAndCost(){ Value = 961, Cost = 10957424 },
        new UpgradeStatAndCost(){ Value = 981, Cost = 15011671 },
    };

    public static UpgradeStatAndCost[] ClickPowerByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,    Cost = 5 },
        new UpgradeStatAndCost(){ Value = 1.1,  Cost = 7 },
        new UpgradeStatAndCost(){ Value = 1.2,  Cost = 8 },
        new UpgradeStatAndCost(){ Value = 1.3,  Cost = 11 },
        new UpgradeStatAndCost(){ Value = 1.4,  Cost = 14 },
        new UpgradeStatAndCost(){ Value = 1.5,  Cost = 19 },
        new UpgradeStatAndCost(){ Value = 1.6,  Cost = 24 },
        new UpgradeStatAndCost(){ Value = 1.7,  Cost = 31 },
        new UpgradeStatAndCost(){ Value = 1.8,  Cost = 41 },
        new UpgradeStatAndCost(){ Value = 1.9,  Cost = 53 },
        new UpgradeStatAndCost(){ Value = 2,    Cost = 69 },
        new UpgradeStatAndCost(){ Value = 2.1,  Cost = 90 },
        new UpgradeStatAndCost(){ Value = 2.2,  Cost = 116 },
        new UpgradeStatAndCost(){ Value = 2.3,  Cost = 151 },
        new UpgradeStatAndCost(){ Value = 2.4,  Cost = 197 },
        new UpgradeStatAndCost(){ Value = 2.5,  Cost = 256 },
        new UpgradeStatAndCost(){ Value = 2.6,  Cost = 333 },
        new UpgradeStatAndCost(){ Value = 2.7,  Cost = 433 },
        new UpgradeStatAndCost(){ Value = 2.8,  Cost = 562 },
        new UpgradeStatAndCost(){ Value = 2.9,  Cost = 731 },
        new UpgradeStatAndCost(){ Value = 11,   Cost = 1213 },
        new UpgradeStatAndCost(){ Value = 11.5, Cost = 1637 },
        new UpgradeStatAndCost(){ Value = 12,   Cost = 2210 },
        new UpgradeStatAndCost(){ Value = 12.5, Cost = 2984 },
        new UpgradeStatAndCost(){ Value = 13,   Cost = 4028 },
        new UpgradeStatAndCost(){ Value = 13.5, Cost = 5438 },
        new UpgradeStatAndCost(){ Value = 14,   Cost = 7342 },
        new UpgradeStatAndCost(){ Value = 14.5, Cost = 9911 },
        new UpgradeStatAndCost(){ Value = 15,   Cost = 13380 },
        new UpgradeStatAndCost(){ Value = 15.5, Cost = 18063 },
        new UpgradeStatAndCost(){ Value = 16,   Cost = 24386 },
        new UpgradeStatAndCost(){ Value = 16.5, Cost = 32921 },
        new UpgradeStatAndCost(){ Value = 17,   Cost = 44443 },
        new UpgradeStatAndCost(){ Value = 17.5, Cost = 59998 },
        new UpgradeStatAndCost(){ Value = 18,   Cost = 80997 },
        new UpgradeStatAndCost(){ Value = 18.5, Cost = 109346 },
        new UpgradeStatAndCost(){ Value = 19,   Cost = 147617 },
        new UpgradeStatAndCost(){ Value = 19.5, Cost = 199283 },
        new UpgradeStatAndCost(){ Value = 20,   Cost = 269032 },
        new UpgradeStatAndCost(){ Value = 20.5, Cost = 363194 },
        new UpgradeStatAndCost(){ Value = 21,   Cost = 490311 },
        new UpgradeStatAndCost(){ Value = 21.5, Cost = 661920 },
        new UpgradeStatAndCost(){ Value = 22,   Cost = 893593 },
        new UpgradeStatAndCost(){ Value = 22.5, Cost = 1206350 },
        new UpgradeStatAndCost(){ Value = 23,   Cost = 1628572 },
        new UpgradeStatAndCost(){ Value = 23.5, Cost = 2198573 },
        new UpgradeStatAndCost(){ Value = 24,   Cost = 2968073 },
        new UpgradeStatAndCost(){ Value = 24.5, Cost = 4006899 },
        new UpgradeStatAndCost(){ Value = 25,   Cost = 5409313 },
        new UpgradeStatAndCost(){ Value = 25.5, Cost = 7302573 },
    };

    public static UpgradeStatAndCost[] StomachSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 50,   Cost = 5 },
        new UpgradeStatAndCost(){ Value = 60,   Cost = 6 },
        new UpgradeStatAndCost(){ Value = 70,   Cost = 8 },
        new UpgradeStatAndCost(){ Value = 80,   Cost = 10 },
        new UpgradeStatAndCost(){ Value = 90,   Cost = 13 },
        new UpgradeStatAndCost(){ Value = 100,  Cost = 17 },
        new UpgradeStatAndCost(){ Value = 110,  Cost = 22 },
        new UpgradeStatAndCost(){ Value = 120,  Cost = 28 },
        new UpgradeStatAndCost(){ Value = 130,  Cost = 36 },
        new UpgradeStatAndCost(){ Value = 140,  Cost = 46 },
        new UpgradeStatAndCost(){ Value = 150,  Cost = 59 },
        new UpgradeStatAndCost(){ Value = 160,  Cost = 76 },
        new UpgradeStatAndCost(){ Value = 170,  Cost = 97 },
        new UpgradeStatAndCost(){ Value = 180,  Cost = 124 },
        new UpgradeStatAndCost(){ Value = 190,  Cost = 158 },
        new UpgradeStatAndCost(){ Value = 200,  Cost = 203 },
        new UpgradeStatAndCost(){ Value = 210,  Cost = 260 },
        new UpgradeStatAndCost(){ Value = 220,  Cost = 332 },
        new UpgradeStatAndCost(){ Value = 230,  Cost = 425 },
        new UpgradeStatAndCost(){ Value = 240,  Cost = 544 },
        new UpgradeStatAndCost(){ Value = 450,  Cost = 774 },
        new UpgradeStatAndCost(){ Value = 470,  Cost = 1021 },
        new UpgradeStatAndCost(){ Value = 490,  Cost = 1348 },
        new UpgradeStatAndCost(){ Value = 510,  Cost = 1780 },
        new UpgradeStatAndCost(){ Value = 530,  Cost = 2349 },
        new UpgradeStatAndCost(){ Value = 550,  Cost = 3101 },
        new UpgradeStatAndCost(){ Value = 570,  Cost = 4093 },
        new UpgradeStatAndCost(){ Value = 590,  Cost = 5403 },
        new UpgradeStatAndCost(){ Value = 610,  Cost = 7132 },
        new UpgradeStatAndCost(){ Value = 630,  Cost = 9414 },
        new UpgradeStatAndCost(){ Value = 650,  Cost = 12426 },
        new UpgradeStatAndCost(){ Value = 670,  Cost = 16403 },
        new UpgradeStatAndCost(){ Value = 690,  Cost = 21651 },
        new UpgradeStatAndCost(){ Value = 710,  Cost = 28580 },
        new UpgradeStatAndCost(){ Value = 730,  Cost = 37725 },
        new UpgradeStatAndCost(){ Value = 750,  Cost = 49798 },
        new UpgradeStatAndCost(){ Value = 770,  Cost = 65733 },
        new UpgradeStatAndCost(){ Value = 790,  Cost = 86767 },
        new UpgradeStatAndCost(){ Value = 810,  Cost = 114533 },
        new UpgradeStatAndCost(){ Value = 830,  Cost = 151184 },
        new UpgradeStatAndCost(){ Value = 850,  Cost = 199562 },
        new UpgradeStatAndCost(){ Value = 870,  Cost = 263422 },
        new UpgradeStatAndCost(){ Value = 890,  Cost = 347717 },
        new UpgradeStatAndCost(){ Value = 910,  Cost = 458987 },
        new UpgradeStatAndCost(){ Value = 930,  Cost = 605863 },
        new UpgradeStatAndCost(){ Value = 950,  Cost = 799739 },
        new UpgradeStatAndCost(){ Value = 970,  Cost = 1055655 },
        new UpgradeStatAndCost(){ Value = 990,  Cost = 1393465 },
        new UpgradeStatAndCost(){ Value = 1010, Cost = 1839374 },
        new UpgradeStatAndCost(){ Value = 1030, Cost = 2427973 },
    };
}

public struct PlayerGamePasses
{
    public bool HasVIP;
    public bool Has2xTrophies;
    public bool Has2xMoney;
    public bool HasTeleporter;
    public bool HasBossAutoclicker;
    public bool HasPetEquipCap1;
    public bool HasPetEquipCap2;
    public bool HasPetEquipCap3;
    public bool HasPetStorageCap1;
    public bool HasPetStorageCap2;
    public bool HasPetStorageCap3;
    public bool HasPetStorageCap4;
    public bool HasPetStorageCap5;
}

public enum ResourceParticleKind
{
    Food,
    Trophy,
    SellFood,
    Coins,
    BossFood,
}

public struct ResourceParticle
{
    public Vector2 Velocity;
    public Vector2 Position;
    public float Lifetime;
    public float MoveTowardSpeed;
    public Texture Texture;
    public Entity Target;
    public ResourceParticleKind Kind;
}
