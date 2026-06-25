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
            Platforms.verticalSpacingMin = Mathf.Max(0.5f, Platforms.verticalSpacingMin);
            Platforms.verticalSpacingMax = Mathf.Max(Platforms.verticalSpacingMin, Platforms.verticalSpacingMax);
            Platforms.horizontalRange = Mathf.Max(0f, Platforms.horizontalRange);
            Platforms.platformWidth = Mathf.Max(0.5f, Platforms.platformWidth);
            Platforms.platformHeight = Mathf.Max(0.2f, Platforms.platformHeight);

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
        }

        [Serializable]
        public class PlatformTuning
        {
            [Tooltip("Ukupan broj generisanih platformi. Više = duži vertikalni run.")]
            public int platformCount = 24;

            [Tooltip("Minimalni vertikalni razmak između platformi. Manje = gušći i lakši climb.")]
            public float verticalSpacingMin = 2.4f;

            [Tooltip("Maksimalni vertikalni razmak između platformi. Više = veći skokovi između platformi.")]
            public float verticalSpacingMax = 3.2f;

            [Tooltip("Koliko levo-desno platforme mogu da se pomeraju. Veća vrednost = širi zig-zag put.")]
            public float horizontalRange = 2f;

            [Tooltip("Širina platformi. Veća vrednost = lakše sletanje i više prostora.")]
            public float platformWidth = 3.2f;

            [Tooltip("Debljina kolajdera platforme. Manja vrednost = manje zaglavljivanje sa strane i glave.")]
            public float platformHeight = 0.3f;

            [Tooltip("Početna Y visina prve platforme u generisanom nivou.")]
            public float startY = -2f;

            [Tooltip("Seed za proceduralni raspored. Isti seed uvek daje isti raspored platformi.")]
            public int seed = 1337;
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
