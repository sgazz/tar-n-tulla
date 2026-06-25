using System;
using GameplayFramework.Tuning;
using UnityEngine;

namespace TarTulla.Game
{
    [CreateAssetMenu(fileName = "TarTullaGameplayProfile", menuName = "Tar&Tulla/Gameplay Profile")]
    public class TarTullaGameplayProfile : GameplayProfileBase
    {
        [Header("Character — skok i gravitacija")]
        public CharacterTuning Character = new();

        [Header("Rope — konopac")]
        public RopeTuning Rope = new();

        [Header("Tilt — vazdušna kontrola")]
        public TiltTuning Tilt = new();

        [Header("Camera — praćenje")]
        public CameraTuning Camera = new();

        [Header("Platforms — proceduralni nivo")]
        public PlatformTuning Platforms = new();

        [Header("Run Rules — pravila run-a")]
        public RunRulesTuning RunRules = new();

        public override bool ValidateProfile()
        {
            bool isValid = base.ValidateProfile();

            Character.jumpForce = Mathf.Max(0.1f, Character.jumpForce);
            Character.gravityScale = Mathf.Max(0.1f, Character.gravityScale);
            Character.maxFallSpeed = Mathf.Max(1f, Character.maxFallSpeed);
            Character.horizontalDamping = Mathf.Max(0f, Character.horizontalDamping);
            Character.landingVelocityThreshold = Mathf.Max(0f, Character.landingVelocityThreshold);
            Character.groundedGraceTime = Mathf.Max(0f, Character.groundedGraceTime);
            Character.jumpCooldown = Mathf.Max(0f, Character.jumpCooldown);

            if (Rope.restLength <= 0f)
            {
                Debug.LogWarning($"[{name}] Rope.restLength must be > 0.", this);
                isValid = false;
            }

            if (Rope.maxLength < Rope.restLength)
            {
                Debug.LogWarning($"[{name}] Rope.maxLength should be >= restLength.", this);
                Rope.maxLength = Rope.restLength;
            }

            Rope.springStrength = Mathf.Max(0.1f, Rope.springStrength);
            Rope.damping = Mathf.Max(0f, Rope.damping);
            Rope.pullAssistStrength = Mathf.Max(0f, Rope.pullAssistStrength);
            Rope.lineWidth = Mathf.Max(0.01f, Rope.lineWidth);
            Rope.overstretchColorThreshold = Mathf.Clamp(Rope.overstretchColorThreshold, 0.5f, 1f);

            Tilt.tiltSensitivity = Mathf.Max(0.1f, Tilt.tiltSensitivity);
            Tilt.maxHorizontalAirSpeed = Mathf.Max(0.1f, Tilt.maxHorizontalAirSpeed);
            Tilt.airAcceleration = Mathf.Max(0.1f, Tilt.airAcceleration);
            Tilt.groundedControlMultiplier = Mathf.Max(0f, Tilt.groundedControlMultiplier);
            Tilt.airborneControlMultiplier = Mathf.Max(0f, Tilt.airborneControlMultiplier);
            Tilt.inputDeadZone = Mathf.Clamp(Tilt.inputDeadZone, 0f, 0.95f);
            Tilt.smoothing = Mathf.Max(0.1f, Tilt.smoothing);

            Camera.smoothTime = Mathf.Max(0.01f, Camera.smoothTime);
            Camera.maxDownwardCorrection = Mathf.Max(0f, Camera.maxDownwardCorrection);

            Platforms.platformCount = Mathf.Max(1, Platforms.platformCount);
            Platforms.verticalSpacingMin = Mathf.Max(0.1f, Platforms.verticalSpacingMin);
            Platforms.verticalSpacingMax = Mathf.Max(Platforms.verticalSpacingMin, Platforms.verticalSpacingMax);
            Platforms.minVerticalGap = Mathf.Max(0.1f, Platforms.minVerticalGap);
            Platforms.maxVerticalGap = Mathf.Max(Platforms.minVerticalGap, Platforms.maxVerticalGap);
            Platforms.horizontalRange = Mathf.Max(0f, Platforms.horizontalRange);
            Platforms.maxHorizontalGap = Mathf.Max(0f, Platforms.maxHorizontalGap);
            Platforms.horizontalDirectionChangeChance = Mathf.Clamp01(Platforms.horizontalDirectionChangeChance);
            Platforms.platformWidth = Mathf.Max(0.5f, Platforms.platformWidth);
            Platforms.platformHeight = Mathf.Max(0.1f, Platforms.platformHeight);
            Platforms.platformWidthMin = Mathf.Max(0.5f, Platforms.platformWidthMin);
            Platforms.platformWidthMax = Mathf.Max(Platforms.platformWidthMin, Platforms.platformWidthMax);
            Platforms.narrowPlatformChance = Mathf.Clamp01(Platforms.narrowPlatformChance);
            Platforms.wideRecoveryPlatformEvery = Mathf.Max(0, Platforms.wideRecoveryPlatformEvery);
            Platforms.easyStartPlatformCount = Mathf.Max(0, Platforms.easyStartPlatformCount);
            Platforms.safeLandingWidthMultiplier = Mathf.Max(1f, Platforms.safeLandingWidthMultiplier);
            Platforms.oneWaySurfaceArc = Mathf.Clamp(Platforms.oneWaySurfaceArc, 90f, 180f);
            Platforms.recoveryPlatformEvery = Mathf.Max(0, Platforms.recoveryPlatformEvery);
            Platforms.recoveryPlatformWidthMultiplier = Mathf.Max(1f, Platforms.recoveryPlatformWidthMultiplier);
            Platforms.initialPlatformCount = Mathf.Max(1, Platforms.initialPlatformCount);
            Platforms.platformBufferAhead = Mathf.Max(0.1f, Platforms.platformBufferAhead);
            Platforms.cleanupDistanceBelowCamera = Mathf.Max(0.1f, Platforms.cleanupDistanceBelowCamera);
            Platforms.maxActivePlatforms = Mathf.Max(Platforms.initialPlatformCount, Platforms.maxActivePlatforms);
            Platforms.generationSegmentHeight = Mathf.Max(0.1f, Platforms.generationSegmentHeight);
            Platforms.difficultyRampStartHeight = Mathf.Max(0f, Platforms.difficultyRampStartHeight);
            Platforms.difficultyRampStrength = Mathf.Max(0f, Platforms.difficultyRampStrength);
            Platforms.minPlatformWidthAtHighDifficulty = Mathf.Max(0.5f, Platforms.minPlatformWidthAtHighDifficulty);
            Platforms.maxVerticalSpacingAtHighDifficulty = Mathf.Max(Platforms.verticalSpacingMin, Platforms.maxVerticalSpacingAtHighDifficulty);
            if (Platforms.maxActivePlatforms < Platforms.platformCount)
                Platforms.maxActivePlatforms = Platforms.platformCount;
            Platforms.screenHorizontalMargin = Mathf.Max(0f, Platforms.screenHorizontalMargin);
            Platforms.manualHalfWidthFallback = Mathf.Max(0.1f, Platforms.manualHalfWidthFallback);
            Character.jumperSoftBoundsForce = Mathf.Max(0f, Character.jumperSoftBoundsForce);

            RunRules.fallDistanceLimit = Mathf.Max(1f, RunRules.fallDistanceLimit);
            RunRules.resetDelay = Mathf.Max(0f, RunRules.resetDelay);

            return isValid;
        }

        [Serializable]
        public class CharacterTuning
        {
            [Tooltip("Snaga automatskog skoka nakon sletanja. Veća vrednost daje viši i energičniji skok.")]
            public float jumpForce = 11f;

            [Tooltip("Jačina gravitacije na jumperu. Veća vrednost = brži pad i niži skokovi.")]
            public float gravityScale = 3f;

            [Tooltip("Maksimalna brzina pada. Veća vrednost dozvoljava brži slobodan pad.")]
            public float maxFallSpeed = 25f;

            [Tooltip("Usporavanje horizontalnog kretanja. Veća vrednost smanjuje klizanje levo-desno.")]
            public float horizontalDamping = 2f;

            [Tooltip("Maksimalna vertikalna brzina pri kojoj se i dalje prihvata sletanje. Veća vrednost = tolerantnije sletanje.")]
            public float landingVelocityThreshold = 0.5f;

            [Tooltip("Kratko vreme nakon dodira platforme kada se jumper smatra na zemlji. Veća vrednost = manje propuštenih sletanja.")]
            public float groundedGraceTime = 0.05f;

            [Tooltip("Pauza između dva automatska skoka. Veća vrednost usporava ritam skakanja.")]
            public float jumpCooldown = 0.12f;

            [Header("Meki horizontalni limiti (rezerva)")]
            [Tooltip("Ako je uključeno, jumperi dobijaju blagu silu ka centru ekrana blizu ivica. Još nije implementirano u fizici.")]
            public bool useSoftHorizontalBoundsForJumpers;

            [Tooltip("Jačina meke sile ka centru kada su jumperi blizu ivica. Veća vrednost = jače vraćanje.")]
            public float jumperSoftBoundsForce;
        }

        [Serializable]
        public class RopeTuning
        {
            [Tooltip("Prirodna dužina konopca između Tar i Tulla. Manja vrednost daje zategnutiji osećaj.")]
            public float restLength = 3f;

            [Tooltip("Maksimalno rastezanje konopca pre jakog ograničenja. Manja vrednost = kraće maksimalno rastojanje.")]
            public float maxLength = 4.5f;

            [Tooltip("Snaga opruge konopca. Veća vrednost = izraženiji sling i brži povratak.")]
            public float springStrength = 50f;

            [Tooltip("Prigušenje oscilacija konopca. Veća vrednost = manje podbijanja, mirniji konopac.")]
            public float damping = 8f;

            [Tooltip("Pomoć pri vučenju partnera kada je jedan na platformi. Veća vrednost = jače pojačanje pri penjanju.")]
            public float pullAssistStrength = 25f;

            [Tooltip("Debljina linije konopca na ekranu. Veća vrednost = deblji vizuelni konopac.")]
            public float lineWidth = 0.08f;

            [Tooltip("Od koje rastegnutosti konopac menja boju. Niža vrednost = raniji vizuelni signal napetosti.")]
            public float overstretchColorThreshold = 0.85f;

            [Tooltip("Koliko snažno gornji jumper biva vučen nadole dok donji stoji na platformi. Niža vrednost = lakše penjanje.")]
            [Range(0f, 1f)]
            public float climbLeadPullFactor = 0.3f;

            [Tooltip("Dodatna sila nagore na donjem jumperu tokom penjanja. Veća vrednost = jače podizanje sa platforme.")]
            public float climbAnchorBoost = 2f;

            [Tooltip("Množilac auto-skoka kada je partner iznad na zategnutom konopcu. Veća vrednost = jači sling skok.")]
            public float climbSlingJumpMultiplier = 1.35f;
        }

        [Serializable]
        public class TiltTuning
        {
            [Tooltip("Osetljivost na naginjanje uređaja. Veća vrednost = jača reakcija na tilt.")]
            public float tiltSensitivity = 8f;

            [Tooltip("Maksimalna brzina kretanja levo-desno dok je jumper u vazduhu.")]
            public float maxHorizontalAirSpeed = 5f;

            [Tooltip("Ubrzanje od tilt ulaza u vazduhu. Veća vrednost = brže skretanje.")]
            public float airAcceleration = 20f;

            [Tooltip("Koliko tilt utiče dok je jumper na platformi. Niža vrednost = manje kontrole na zemlji.")]
            public float groundedControlMultiplier = 0.15f;

            [Tooltip("Koliko tilt utiče u vazduhu. Veća vrednost = više kontrole tokom leta.")]
            public float airborneControlMultiplier = 1f;

            [Tooltip("Mrtva zona oko neutralnog tilt-a. Veća vrednost ignoriše manja naginjanja.")]
            public float inputDeadZone = 0.08f;

            [Tooltip("Izgladivanje tilt ulaza. Veća vrednost = mekša, sporija promena smera.")]
            public float smoothing = 8f;
        }

        [Serializable]
        public class CameraTuning
        {
            [Tooltip("Koliko visoko kamera drži par iznad njihovog srednjeg tačka. Veća vrednost = više prostora iznad.")]
            public float verticalOffset = 1.5f;

            [Tooltip("Koliko glatko kamera prati kretanje. Veća vrednost = sporiji, mekši follow.")]
            public float smoothTime = 0.25f;

            [Tooltip("Da li kamera sme malo da se spusti ako se par kratko spusti, umesto strogo samo nagore.")]
            public bool allowSmallDownwardCorrection = true;

            [Tooltip("Maksimalno dopušteno spuštanje kamere ispod najviše tačke. Veća vrednost = fleksibilniji kadar.")]
            public float maxDownwardCorrection = 1.5f;

            [Tooltip("Drži kameru na X=0 i prati samo vertikalno kretanje. Isključi za horizontalni follow.")]
            public bool lockHorizontalPosition = true;
        }

        [Serializable]
        public class PlatformTuning
        {
            [Header("Opšte")]
            [Tooltip("Ukupan broj generisanih platformi. Više = duži vertikalni run.")]
            public int platformCount = 25;

            [Tooltip("Početna Y visina prve platforme u generisanom nivou.")]
            public float startY = -2f;

            [Tooltip("Seed za proceduralni raspored. Isti seed uvek daje isti raspored platformi.")]
            public int seed = 1337;

            [Header("Proceduralni stream")]
            [Tooltip("Uključuje beskonačni proceduralni tok platformi umesto fiksnog broja.")]
            public bool useProceduralGeneration = true;

            [Tooltip("Broj platformi generisanih na početku run-a u proceduralnom režimu.")]
            public int initialPlatformCount = 12;

            [Tooltip("Koliko visine iznad reference tačke generator unapred pravi platforme. Veća vrednost = više platformi ispred.")]
            public float platformBufferAhead = 18f;

            [Tooltip("Udaljenost ispod kamere nakon koje se stare platforme brišu. Veća vrednost = duže ostaju u memoriji.")]
            public float cleanupDistanceBelowCamera = 14f;

            [Tooltip("Maksimalan broj aktivnih platformi u sceni. Veća vrednost = više objekata, sporije čišćenje.")]
            public int maxActivePlatforms = 40;

            [Tooltip("Visina segmenta koji generator dodaje po pozivu. Veća vrednost = veći batch po frame-u.")]
            public float generationSegmentHeight = 10f;

            [Tooltip("Ako je uključeno, svaki run dobija novi seed umesto fiksnog iz profila.")]
            public bool randomizeSeedOnRun;

            [Header("Vertikalni razmak")]
            [Tooltip("Minimalni vertikalni razmak između platformi posle lakog starta. Manje = gušći climb.")]
            public float verticalSpacingMin = 1.8f;

            [Tooltip("Maksimalni vertikalni razmak između platformi posle lakog starta. Više = veći skokovi.")]
            public float verticalSpacingMax = 2.8f;

            [Tooltip("Apsolutni minimum vertikalnog razmaka koji generator sme da koristi.")]
            public float minVerticalGap = 1.5f;

            [Tooltip("Apsolutni maksimum vertikalnog razmaka koji generator sme da koristi.")]
            public float maxVerticalGap = 3f;

            [Header("Horizontalno postavljanje")]
            [Tooltip("Koliko levo-desno platforme mogu da se pomeraju od centra. Veća vrednost = širi zig-zag.")]
            public float horizontalRange = 2.8f;

            [Tooltip("Maksimalni horizontalni korak između dve platforme. Veća vrednost = širi lateralni skokovi.")]
            public float maxHorizontalGap = 2.2f;

            [Tooltip("Šansa da generator promeni horizontalni smer (0–1). Veća vrednost = češće menjanje smera.")]
            [Range(0f, 1f)]
            public float horizontalDirectionChangeChance = 0.55f;

            [Tooltip("Ako je uključeno, platforme naizmenično idu levo-desno umesto slučajnih promena smera.")]
            public bool forceAlternatingPattern;

            [Header("Veličina platforme")]
            [Tooltip("Osnovna širina platforme kada varijacija širine nije aktivna.")]
            public float platformWidth = 2.8f;

            [Tooltip("Debljina kolajdera platforme. Manja vrednost = manje zaglavljivanje sa strane i glave.")]
            public float platformHeight = 0.28f;

            [Tooltip("Uključuje nasumičnu širinu platformi između min i max vrednosti.")]
            public bool widthVariationEnabled;

            [Tooltip("Najuža dozvoljena širina platforme kada je varijacija uključena.")]
            public float platformWidthMin = 1.8f;

            [Tooltip("Najšira dozvoljena širina platforme kada je varijacija uključena.")]
            public float platformWidthMax = 3.4f;

            [Tooltip("Šansa za usku platformu (0–1). Veća vrednost = češće uske platforme.")]
            [Range(0f, 1f)]
            public float narrowPlatformChance = 0.15f;

            [Tooltip("Na svakoj N-toj platformi napravi širu recovery platformu. 0 = isključeno.")]
            public int wideRecoveryPlatformEvery = 6;

            [Header("Težina (ramp)")]
            [Tooltip("Postepeno pooštrava razmake i sužava platforme nakon određene visine.")]
            public bool difficultyRampEnabled = true;

            [Tooltip("Visina od startY posle koje počinje ramp težine.")]
            public float difficultyRampStartHeight = 25f;

            [Tooltip("Jačina pooštravanja (0+). Veća vrednost = brži prelaz ka težim vrednostima.")]
            public float difficultyRampStrength = 0.25f;

            [Tooltip("Najuža širina platforme na visokoj težini.")]
            public float minPlatformWidthAtHighDifficulty = 1.8f;

            [Tooltip("Maksimalni vertikalni razmak na visokoj težini.")]
            public float maxVerticalSpacingAtHighDifficulty = 3.2f;

            [Header("Recovery platforme")]
            [Tooltip("Na svakoj N-toj platformi u stream-u napravi širu recovery platformu. 0 = isključeno.")]
            public int recoveryPlatformEvery = 7;

            [Tooltip("Množilac širine recovery platforme. Veća vrednost = šira sigurna platforma.")]
            public float recoveryPlatformWidthMultiplier = 1.35f;

            [Header("Laki start")]
            [Tooltip("Broj prvih platformi sa lakšim razmakom i širim površinama za sigurniji početak.")]
            public int easyStartPlatformCount = 5;

            [Tooltip("Množilac širine za laki start i recovery platforme. Veća vrednost = šire i sigurnije sletanje.")]
            public float safeLandingWidthMultiplier = 1.25f;

            [Header("One-way ponašanje")]
            [Tooltip("Platforme prolaze kroz jumpere odozdo (Doodle Jump stil).")]
            public bool useOneWayPlatforms = true;

            [Tooltip("Luk površine one-way kolajdera (90–180). Veća vrednost = šira gornja površina za sletanje.")]
            [Range(90f, 180f)]
            public float oneWaySurfaceArc = 150f;

            [Header("Granice igrališta (portrait)")]
            [Tooltip("Ako je uključeno, generator računa dozvoljenu širinu igrališta na osnovu orthographic kamere.")]
            public bool useCameraBasedHorizontalBounds = true;

            [Tooltip("Sigurnosni razmak od ivice ekrana. Veća vrednost drži platforme dalje od ivica.")]
            public float screenHorizontalMargin = 0.35f;

            [Tooltip("Ako je uključeno, centar platforme se ograničava tako da cela platforma ostane vidljiva.")]
            public bool clampPlatformsToVisibleWidth = true;

            [Tooltip("Rezervna poluširina igrališta ako kamera nije dostupna.")]
            public float manualHalfWidthFallback = 4f;

            [Tooltip("Prikazuje granice vidljivog igrališta u Scene view-u radi lakšeg podešavanja.")]
            public bool drawPlayfieldBoundsGizmos = true;
        }

        [Serializable]
        public class RunRulesTuning
        {
            [Tooltip("Koliko daleko ispod najbolje dostignute visine oba jumpera smeju da padnu pre resetovanja run-a.")]
            public float fallDistanceLimit = 14f;

            [Tooltip("Pauza u sekundama pre automatskog reset-a nakon pada. Veća vrednost = sporiji restart.")]
            public float resetDelay = 0f;
        }
    }
}
